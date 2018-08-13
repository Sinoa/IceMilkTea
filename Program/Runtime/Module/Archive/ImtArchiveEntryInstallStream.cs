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
    /// アーカイブのエントリインストール結果を表現します
    /// </summary>
    public enum ImtArchiveEntryInstallResult
    {
        /// <summary>
        /// インストールは成功しました
        /// </summary>
        Success,

        /// <summary>
        /// インストールは失敗しました
        /// </summary>
        Failed,
    }



    /// <summary>
    /// アーカイブのエントリインストールが完了したことのハンドリングを行うデリゲートです
    /// </summary>
    /// <param name="installer">実際にインストールを行ったインストーラ</param>
    /// <param name="result">インストール結果</param>
    public delegate void ImtArchiveEntryInstallFinishHandler(ImtArchiveEntryInstaller installer, ImtArchiveEntryInstallResult result);



    /// <summary>
    /// ImtArchiveEntryInstaller によってアーカイブにエントリをインストールする為の、ストリームクラスです。
    /// </summary>
    /// <remarks>
    /// このクラスを経由してアーカイブにエントリの実体をインストールします。
    /// インストールは通常の書き込み関数 Write() を用いて行って下さい。
    /// インストールが完了した場合は必ず FinishInstall() 関数を呼び出して下さい。
    /// </remarks>
    public class ImtArchiveEntryInstallStream : Stream
    {
        // 以下メンバ変数定義
        private ImtArchiveEntryInstallFinishHandler installFinishHandler;
        private ImtArchiveEntryInstaller installer;
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
        /// ストリームの書き込みが可能かどうか
        /// </summary>
        public override bool CanWrite => originalStream.CanWrite;


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
        /// ImtArchiveEntryInstallStream のインスタンスを初期化します
        /// </summary>
        /// <remarks>
        /// このインスタンスに渡すストリームは、最低でも CanWrite CanSeek をサポートするストリームでなければいけません。
        /// また、インストーラクラスは、エントリの実体の書き込みが終わったら FinishInstall() 関数を呼び出さなければなりません。
        /// </remarks>
        /// <param name="info">アーカイブに含まれるエントリの情報</param>
        /// <param name="stream">アーカイブへ実際に書き込むためのストリーム</param>
        /// <param name="installer">このインストールストリームを制御するインストーラ</param>
        /// <param name="installFinishHandler">インストール完了ハンドリングを行う関数への参照</param>
        /// <exception cref="ArgumentNullException">stream または installFinishHandler または installer が null です</exception>
        /// <exception cref="NotSupportedException">ストリームは最低でも CanWrite および CanSeek をサポートしなければいけません</exception>
        /// <see cref="FinishInstall"/>
        public ImtArchiveEntryInstallStream(ImtArchiveEntryInfo info, Stream stream, ImtArchiveEntryInstaller installer, ImtArchiveEntryInstallFinishHandler installFinishHandler)
        {
            // ストリームまたは、インストーラ、インストール完了ハンドラがnullなら
            if (stream == null || installer == null || installFinishHandler == null)
            {
                // どうやって書き込んで、そして完了をすればよいのか
                throw new ArgumentNullException($"{nameof(stream)} or {nameof(installer)} or {nameof(installFinishHandler)}");
            }


            // ストリームが、書き込みまたはシークが出来ないものなら
            if (!(stream.CanRead && stream.CanSeek))
            {
                // ストリームは書き込みとシークをサポートしなければならない
                throw new NotSupportedException("ストリームは最低でも CanWrite および CanSeek をサポートしなければいけません");
            }


            // 初期化をして終了
            entryInfo = info;
            originalStream = stream;
            virtualPosition = 0;
            this.installer = installer;
            this.installFinishHandler = installFinishHandler;
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
        /// 指定されたバッファの範囲を、ストリームに書き込みます。
        /// また、この関数はストリームの長さを超えた、書き込みを行う事は出来ません。
        /// 必要以上に書き込もうとしても書き込まれない可能性があります。
        /// </summary>
        /// <param name="buffer">ストリームに書き込むための、読み取り用バッファ</param>
        /// <param name="offset">バッファから読み取る開始インデックス</param>
        /// <param name="count">バッファから読み取るサイズ</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            // 仮想位置がサイズと同値なら
            if (virtualPosition == entryInfo.Size)
            {
                // もう末尾まで到達しているので直ちに終了する
                return;
            }


            // 残りの書き込める範囲より要求サイズが大きいなら調整する
            var availableSize = entryInfo.Size - virtualPosition;
            count = availableSize < count ? (int)availableSize : count;


            // ストリームをロック
            lock (originalStream)
            {
                // 望まれる位置にシークしてからストリームからデータを書き込む（例外はオリジナルのストリームに委ねる）
                // TODO : Seekは可能であれば、本当に必要とする時（他がSeekもしていない時）だけSeekするようにしたい
                originalStream.Seek(entryInfo.Offset + virtualPosition, SeekOrigin.Begin);
                originalStream.Write(buffer, offset, count);
            }


            // 書き込んだサイズ分仮想位置を進める
            virtualPosition += count;
        }


        /// <summary>
        /// インストールを完了します
        /// </summary>
        /// <param name="result">インストール結果</param>
        public void FinishInstall(ImtArchiveEntryInstallResult result)
        {
            // インストール完了呼び出し関数が存在しないなら
            if (installFinishHandler == null)
            {
                // 終了
                return;
            }


            // インストール完了呼び出しをして参照を解除する
            installFinishHandler(installer, result);
            installFinishHandler = null;
        }
    }
}