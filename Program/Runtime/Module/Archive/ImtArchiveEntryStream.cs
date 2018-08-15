// zlib/libpng License
//
// Copyright (c) 2018 Sinoa
//
// This software is provided 'as-is', without any express or implied warranty.
// In no event will the authors be held liable for any damages arising from the use of this software.
// Permission is granted to anyone to use this software for any purpose,
// including commercial applications, and to alter it and redistribute it freely,
// subject to the following restrictions:
//
// 1. The origin of this software must not be misrepresented; you must not claim that you wrote the original software.
//    If you use this software in a product, an acknowledgment in the product documentation would be appreciated but is not required.
// 2. Altered source versions must be plainly marked as such, and must not be misrepresented as being the original software.
// 3. This notice may not be removed or altered from any source distribution.

using System;
using System.IO;

namespace IceMilkTea.Module
{
    /// <summary>
    /// アーカイブ内に含まれるエントリの実体を読み込む為のストリームクラスです
    /// </summary>
    /// <remarks>
    /// このストリームは、読み取り専用で書き込むことは出来ません、さらに長さの変更などが出来ないことにも注意してください。
    /// 書き込みや長さの変更を行おうとすると NotSupportedException がスローされます。
    /// </remarks>
    public class ImtArchiveEntryStream : Stream
    {
        // 以下メンバ変数定義
        private ImtArchiveEntryInfo entryInfo;
        private Stream originalStream;
        private long virtualPosition;



        /// <summary>
        /// ストリームの読み込みが可能かどうか
        /// </summary>
        public override bool CanRead => originalStream.CanRead;


        /// <summary>
        /// ストリームのシークが可能かどうか
        /// </summary>
        public override bool CanSeek => originalStream.CanSeek;


        /// <summary>
        /// このストリームは書き込みが出来ません。
        /// 常に false を返します。
        /// </summary>
        public override bool CanWrite => false;


        /// <summary>
        /// ストリームの長さを取得します
        /// </summary>
        public override long Length => entryInfo.Size;


        /// <summary>
        /// ストリームの操作位置を設定取得をします
        /// </summary>
        public override long Position
        {
            get
            {
                // 仮想の位置を返す
                return virtualPosition;
            }
            set
            {
                // 負の値が来たら
                if (value < 0)
                {
                    // ストリームの負の位置ってどこですか
                    throw new ArgumentOutOfRangeException(nameof(Position));
                }


                // ストリーム本来の長さ以上に設定されようとしたら
                if (value >= entryInfo.Size)
                {
                    // ストリームの末尾を超えた設定は許されない
                    throw new EndOfStreamException();
                }


                // 位置は仮想の位置に設定する
                virtualPosition = value;
            }
        }



        /// <summary>
        /// ImtArchiveReadStream のインスタンスを初期化します
        /// </summary>
        /// <remarks>
        /// このインスタンスに渡すストリームは、最低でも CanRead CanSeek をサポートするストリームでなければいけません。
        /// </remarks>
        /// <param name="info">アーカイブに含まれるエントリの情報</param>
        /// <param name="stream">アーカイブからデータを読み取るためのストリーム</param>
        /// <exception cref="ArgumentNullException">stream が null です</exception>
        /// <exception cref="ArgumentException">渡されたエントリ情報に問題が発生しました。詳細は例外の内容を確認してください。</exception>
        /// <exception cref="NotSupportedException">ストリームは最低でも CanRead および CanSeek をサポートしなければいけません</exception>
        /// <exception cref="ArgumentException">エントリの実体が存在しないエントリの情報が渡されました</exception>
        public ImtArchiveEntryStream(ImtArchiveEntryInfo info, Stream stream)
        {
            // ストリームがnullなら
            if (stream == null)
            {
                // どうやって読み込めばよいのか
                throw new ArgumentNullException(nameof(stream));
            }


            // ストリームが、読み取りまたはシークが出来ないものなら
            if (!(stream.CanRead && stream.CanSeek))
            {
                // ストリームは読み取りとシークをサポートしなければならない
                throw new NotSupportedException("ストリームは最低でも CanRead および CanSeek をサポートしなければいけません");
            }


            // エントリの情報の検証に問題があるなら
            var entryValidateResult = ImtArchiveEntryInfo.Validate(ref info);
            if (entryValidateResult != ImtArchiveEntryInfoValidateResult.NoProblem)
            {
                // ストリームの読み込みは出来ない
                throw new ArgumentException($"エントリ情報の問題'{entryValidateResult.ToString()}'が発生しました", nameof(info));
            }


            // エントリの実体が無いのなら
            if (!info.IsContainEntryData)
            {
                // そもそも読み込めないですね
                throw new ArgumentException("エントリの実体が存在しないエントリの情報が渡されました", nameof(info));
            }


            // 初期化をして終了
            entryInfo = info;
            originalStream = stream;
            virtualPosition = 0;
        }


        /// <summary>
        /// ストリームのバッファをクリアします
        /// </summary>
        public override void Flush()
        {
            // ストリームをロック
            lock (originalStream)
            {
                // オリジナルのストリームをフラッシュする
                originalStream.Flush();
            }
        }


        /// <summary>
        /// ストリームから、指定バイト分読み込み、指定されたバッファにデータを書き込みます。
        /// </summary>
        /// <param name="buffer">読み取られたデータを書き込むバッファ</param>
        /// <param name="offset">バッファの書き込む位置のオフセット</param>
        /// <param name="count">バッファに書き込むバイト数</param>
        /// <returns>
        /// バッファに書き込んだバイト数を返しますが、指定されたバイト数未満を返すことがあります。
        /// また、ストリームの末尾に到達している場合は 0 を返すことがあります。
        /// </returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            // 仮想位置がサイズと同値なら
            if (virtualPosition == entryInfo.Size)
            {
                // もう末尾まで到達しているので直ちに0を返す
                return 0;
            }


            // 残りの読み取れる範囲より要求サイズが大きいなら調整する
            var availableSize = entryInfo.Size - virtualPosition;
            count = availableSize < count ? (int)availableSize : count;


            // ストリームをロック
            int readSize;
            lock (originalStream)
            {
                // 望まれる位置にシークしてからストリームからデータを読み込む（Readの例外はStream側に任せる）
                // TODO : Seekは可能であれば、本当に必要とする時（他がReadもSeekもしていない時）だけSeekするようにしたい
                originalStream.Seek(entryInfo.Offset + virtualPosition, SeekOrigin.Begin);
                readSize = originalStream.Read(buffer, offset, count);
            }


            // 読み込んだサイズ分仮想位置を進めて返す
            virtualPosition += readSize;
            return readSize;
        }


        /// <summary>
        /// ストリームの位置を、指定された位置に設定します。
        /// </summary>
        /// <param name="offset">設定する位置</param>
        /// <param name="origin">指定された offset がどの位置からを示すかを表します</param>
        /// <returns>設定された新しいストリームの位置を返します</returns>
        /// <exception cref="ArgumentOutOfRangeException">指定されたオフセットがストリームの範囲を超えています</exception>
        public override long Seek(long offset, SeekOrigin origin)
        {
            // 最終的に設定する位置を、原点の指定方法に基づいて求める
            long finalOffset =
                origin == SeekOrigin.Begin ? offset :
                origin == SeekOrigin.Current ? virtualPosition + offset :
                origin == SeekOrigin.End ? entryInfo.Size + offset : 0L;


            // 求められたオフセットが、負の値またはストリームの長さ以上なら
            if (finalOffset < 0 || finalOffset >= entryInfo.Size)
            {
                // 指定されたオフセットはストリームが扱える範囲を超えている
                throw new ArgumentOutOfRangeException(nameof(offset), "指定されたオフセットがストリームの範囲を超えています");
            }


            // 仮想位置に求められた位置を設定して返す
            return virtualPosition = finalOffset;
        }


        /// <summary>
        /// このストリームでは、長さの設定はサポートをしていません。
        /// 常に NotSupportedException をスローします。
        /// </summary>
        /// <param name="value">長さの設定はサポートしていません</param>
        /// <exception cref="NotSupportedException">このストリームでは、長さの設定はサポートをしていません。</exception>
        public override void SetLength(long value)
        {
            // サポートはしていない
            throw new NotSupportedException("このストリームでは、長さの設定はサポートをしていません。");
        }


        /// <summary>
        /// このストリームでは、書き込みのサポートをしていません。
        /// 常に NotSupportedException をスローします。
        /// </summary>
        /// <param name="buffer">書き込みをサポートしていません</param>
        /// <param name="offset">書き込みをサポートしていません</param>
        /// <param name="count">書き込みをサポートしていません</param>
        /// <exception cref="NotSupportedException">このストリームでは、書き込みのサポートをしていません。</exception>
        public override void Write(byte[] buffer, int offset, int count)
        {
            // サポートはしていない
            throw new NotSupportedException("このストリームでは、書き込みのサポートをしていません。");
        }


        /// <summary>
        /// このインスタンスが保持しているエントリ情報のコピーを取得します
        /// </summary>
        /// <param name="result">エントリの情報を受け取りたい構造体への参照</param>
        public void GetEntryInfo(out ImtArchiveEntryInfo result)
        {
            // 全てコピー（そのまま代入でも良いけど）
            result.Id = entryInfo.Id;
            result.Offset = entryInfo.Offset;
            result.Size = entryInfo.Size;
            result.Reserved = entryInfo.Reserved;
        }
    }
}