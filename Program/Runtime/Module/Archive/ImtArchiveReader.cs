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
    /// アーカイブのローレベルなリーダークラスです。
    /// </summary>
    /// <remarks>
    /// このクラスは、ヘッダやエントリ情報などのデータを素直に読み込むだけの実装になっており、読み込まれたデータの整合性などを保証をしていません。
    /// ストリームの位置などを意識した制御を行うようにしてください。
    /// </remarks>
    public class ImtArchiveReader
    {
        // メンバ変数定義
        private byte[] internalBuffer;



        /// <summary>
        /// このクラスがコンストラクタで受け取ったストリーム
        /// </summary>
        public Stream BaseStream { get; private set; }



        /// <summary>
        /// ImtArchiveReader のインスタンスを初期化します
        /// </summary>
        /// <remarks>
        /// このクラスは、渡されたストリームを閉じることは無いので、インスタンスはそのまま参照を捨てても問題はありません
        /// </remarks>
        /// <param name="stream">アーカイブデータを読み込むためのストリーム</param>
        /// <exception cref="ArgumentNullException">stream が null です</exception>
        /// <exception cref="ArgumentException">stream が CanRead をサポートしていません</exception>
        public ImtArchiveReader(Stream stream)
        {
            // ストリームが渡されていないなら
            if (stream == null)
            {
                // そもそも動作を続行出来ない
                throw new ArgumentNullException(nameof(stream));
            }


            // ストリームがCanReadをサポートしていないなら
            if (!stream.CanRead)
            {
                // 読み込めないストリームはだめ
                throw new ArgumentException($"{nameof(stream)} が CanRead をサポートしていません", nameof(stream));
            }


            // ストリームを覚える
            BaseStream = stream;


            // 内部バッファを今のうちに用意しておく
            internalBuffer = new byte[ImtArchiveHeader.HeaderSize > ImtArchiveEntryInfo.InfoSize ? ImtArchiveHeader.HeaderSize : ImtArchiveEntryInfo.InfoSize];
        }


        /// <summary>
        /// ストリームから ImtArchiveHeader を読み込んで header に設定します
        /// </summary>
        /// <param name="header">読み込んだ ImtArchiveHeader を受け取る参照</param>
        /// <exception cref="InvalidOperationException">ストリームから必要なデータを読み込めませんでした</exception>
        public void Read(out ImtArchiveHeader header)
        {
            // ストリームからヘッダサイズ分読み込むが、読みきれなかった場合は
            if (!FillRead(internalBuffer, 0, ImtArchiveHeader.HeaderSize))
            {
                // データ不十分として例外を吐く
                throw new InvalidOperationException("ストリームから必要なデータを読み込めませんでした");
            }


            // マジックナンバーの配列を作ってデータを設定（ただのバイト配列なのでエンディアン影響はなし）
            // データ読み取りインデックスも初期化する
            header.MagicNumber = new byte[4];
            Array.Copy(internalBuffer, header.MagicNumber, sizeof(byte) * 4);
            var index = sizeof(byte) * 4;


            // もしリトルエンディアンなCPUなら
            if (BitConverter.IsLittleEndian)
            {
                // リトルエンディアン向けなフィールド初期化をする
                // uint初期化
                header.ArchiveInfo = (
                    ((uint)internalBuffer[index + 0] << 0) |
                    ((uint)internalBuffer[index + 1] << 8) |
                    ((uint)internalBuffer[index + 2] << 16) |
                    ((uint)internalBuffer[index + 3] << 24));
                index += sizeof(uint);


                // long初期化
                header.EntryInfoListOffset = (long)(
                    ((ulong)internalBuffer[index + 0] << 0) |
                    ((ulong)internalBuffer[index + 1] << 8) |
                    ((ulong)internalBuffer[index + 2] << 16) |
                    ((ulong)internalBuffer[index + 3] << 24) |
                    ((ulong)internalBuffer[index + 4] << 32) |
                    ((ulong)internalBuffer[index + 5] << 40) |
                    ((ulong)internalBuffer[index + 6] << 48) |
                    ((ulong)internalBuffer[index + 7] << 56));
                index += sizeof(long);


                // int初期化
                header.EntryInfoCount = (int)(
                    ((uint)internalBuffer[index + 0] << 0) |
                    ((uint)internalBuffer[index + 1] << 8) |
                    ((uint)internalBuffer[index + 2] << 16) |
                    ((uint)internalBuffer[index + 3] << 24));
                index += sizeof(int);


                // uint初期化
                header.Reserved = (
                    ((uint)internalBuffer[index + 0] << 0) |
                    ((uint)internalBuffer[index + 1] << 8) |
                    ((uint)internalBuffer[index + 2] << 16) |
                    ((uint)internalBuffer[index + 3] << 24));
            }
            else
            {
                // ビッグエンディアン向けなフィールド初期化をする
                // uint初期化
                header.ArchiveInfo = (
                    ((uint)internalBuffer[index + 0] << 24) |
                    ((uint)internalBuffer[index + 1] << 16) |
                    ((uint)internalBuffer[index + 2] << 8) |
                    ((uint)internalBuffer[index + 3] << 0));
                index += sizeof(uint);


                // long初期化
                header.EntryInfoListOffset = (long)(
                    ((ulong)internalBuffer[index + 0] << 56) |
                    ((ulong)internalBuffer[index + 1] << 48) |
                    ((ulong)internalBuffer[index + 2] << 40) |
                    ((ulong)internalBuffer[index + 3] << 32) |
                    ((ulong)internalBuffer[index + 4] << 24) |
                    ((ulong)internalBuffer[index + 5] << 16) |
                    ((ulong)internalBuffer[index + 6] << 8) |
                    ((ulong)internalBuffer[index + 7] << 0));
                index += sizeof(long);


                // int初期化
                header.EntryInfoCount = (int)(
                    ((uint)internalBuffer[index + 0] << 24) |
                    ((uint)internalBuffer[index + 1] << 16) |
                    ((uint)internalBuffer[index + 2] << 8) |
                    ((uint)internalBuffer[index + 3] << 0));
                index += sizeof(int);


                // uint初期化
                header.Reserved = (
                    ((uint)internalBuffer[index + 0] << 24) |
                    ((uint)internalBuffer[index + 1] << 16) |
                    ((uint)internalBuffer[index + 2] << 8) |
                    ((uint)internalBuffer[index + 3] << 0));
            }
        }


        /// <summary>
        /// ストリームから ImtArchiveEntryInfo を読み込んで info に設定します
        /// </summary>
        /// <param name="header">読み込んだ ImtArchiveEntryInfo を受け取る参照</param>
        /// <exception cref="InvalidOperationException">ストリームから必要なデータを読み込めませんでした</exception>
        public void Read(out ImtArchiveEntryInfo info)
        {
            // ストリームからエントリサイズ分読み込むが、読みきれなかった場合は
            if (!FillRead(internalBuffer, 0, ImtArchiveEntryInfo.InfoSize))
            {
                // データ不十分として例外を吐く
                throw new InvalidOperationException("ストリームから必要なデータを読み込めませんでした");
            }


            // 読み取りインデックスを初期化
            var index = 0;


            // もしリトルエンディアンなCPUなら
            if (BitConverter.IsLittleEndian)
            {
                // リトルエンディアン向けなフィールド初期化をする
                // ulong初期化
                info.Id = (
                    ((ulong)internalBuffer[index + 0] << 0) |
                    ((ulong)internalBuffer[index + 1] << 8) |
                    ((ulong)internalBuffer[index + 2] << 16) |
                    ((ulong)internalBuffer[index + 3] << 24) |
                    ((ulong)internalBuffer[index + 4] << 32) |
                    ((ulong)internalBuffer[index + 5] << 40) |
                    ((ulong)internalBuffer[index + 6] << 48) |
                    ((ulong)internalBuffer[index + 7] << 56));
                index += sizeof(ulong);


                // long初期化
                info.Offset = (long)(
                    ((ulong)internalBuffer[index + 0] << 0) |
                    ((ulong)internalBuffer[index + 1] << 8) |
                    ((ulong)internalBuffer[index + 2] << 16) |
                    ((ulong)internalBuffer[index + 3] << 24) |
                    ((ulong)internalBuffer[index + 4] << 32) |
                    ((ulong)internalBuffer[index + 5] << 40) |
                    ((ulong)internalBuffer[index + 6] << 48) |
                    ((ulong)internalBuffer[index + 7] << 56));
                index += sizeof(long);


                // long初期化
                info.Size = (long)(
                    ((ulong)internalBuffer[index + 0] << 0) |
                    ((ulong)internalBuffer[index + 1] << 8) |
                    ((ulong)internalBuffer[index + 2] << 16) |
                    ((ulong)internalBuffer[index + 3] << 24) |
                    ((ulong)internalBuffer[index + 4] << 32) |
                    ((ulong)internalBuffer[index + 5] << 40) |
                    ((ulong)internalBuffer[index + 6] << 48) |
                    ((ulong)internalBuffer[index + 7] << 56));
                index += sizeof(long);


                // ulong初期化
                info.Reserved = (
                    ((ulong)internalBuffer[index + 0] << 0) |
                    ((ulong)internalBuffer[index + 1] << 8) |
                    ((ulong)internalBuffer[index + 2] << 16) |
                    ((ulong)internalBuffer[index + 3] << 24) |
                    ((ulong)internalBuffer[index + 4] << 32) |
                    ((ulong)internalBuffer[index + 5] << 40) |
                    ((ulong)internalBuffer[index + 6] << 48) |
                    ((ulong)internalBuffer[index + 7] << 56));
            }
            else
            {
                // ビッグエンディアン向けなフィールド初期化をする
                // ulong初期化
                info.Id = (
                    ((ulong)internalBuffer[index + 0] << 56) |
                    ((ulong)internalBuffer[index + 1] << 48) |
                    ((ulong)internalBuffer[index + 2] << 40) |
                    ((ulong)internalBuffer[index + 3] << 32) |
                    ((ulong)internalBuffer[index + 4] << 24) |
                    ((ulong)internalBuffer[index + 5] << 16) |
                    ((ulong)internalBuffer[index + 6] << 8) |
                    ((ulong)internalBuffer[index + 7] << 0));
                index += sizeof(ulong);


                // long初期化
                info.Offset = (long)(
                    ((ulong)internalBuffer[index + 0] << 56) |
                    ((ulong)internalBuffer[index + 1] << 48) |
                    ((ulong)internalBuffer[index + 2] << 40) |
                    ((ulong)internalBuffer[index + 3] << 32) |
                    ((ulong)internalBuffer[index + 4] << 24) |
                    ((ulong)internalBuffer[index + 5] << 16) |
                    ((ulong)internalBuffer[index + 6] << 8) |
                    ((ulong)internalBuffer[index + 7] << 0));
                index += sizeof(long);


                // long初期化
                info.Size = (long)(
                    ((ulong)internalBuffer[index + 0] << 56) |
                    ((ulong)internalBuffer[index + 1] << 48) |
                    ((ulong)internalBuffer[index + 2] << 40) |
                    ((ulong)internalBuffer[index + 3] << 32) |
                    ((ulong)internalBuffer[index + 4] << 24) |
                    ((ulong)internalBuffer[index + 5] << 16) |
                    ((ulong)internalBuffer[index + 6] << 8) |
                    ((ulong)internalBuffer[index + 7] << 0));
                index += sizeof(long);


                // ulong初期化
                info.Reserved = (
                    ((ulong)internalBuffer[index + 0] << 56) |
                    ((ulong)internalBuffer[index + 1] << 48) |
                    ((ulong)internalBuffer[index + 2] << 40) |
                    ((ulong)internalBuffer[index + 3] << 32) |
                    ((ulong)internalBuffer[index + 4] << 24) |
                    ((ulong)internalBuffer[index + 5] << 16) |
                    ((ulong)internalBuffer[index + 6] << 8) |
                    ((ulong)internalBuffer[index + 7] << 0));
            }
        }


        /// <summary>
        /// 指定されたバッファに、読み込むべきサイズが満たされるまで、ストリームからデータを読み取ります。
        /// </summary>
        /// <remarks>
        /// この関数は、ストリームから指定バイト数を読み切るまで、処理をブロックします。
        /// </remarks>
        /// <param name="buffer">読み込んだデータを書き込むバッファ</param>
        /// <param name="index">バッファに書き込む、開始インデックス</param>
        /// <param name="count">読み取るデータのサイズ</param>
        /// <returns>ストリームから読み出したサイズが count 値まで読み込めた場合は true を、ストリームから読み出せなくなった場合は false を返します</returns>
        private bool FillRead(byte[] buffer, int index, int count)
        {
            // ストリームから要求されたサイズ分読み込むまでループ
            for (int readTotalSize = 0; readTotalSize < count;)
            {
                // データを読み込む
                var readSize = BaseStream.Read(buffer, index + readTotalSize, count - readTotalSize);
                readTotalSize += readSize;


                // 読み取りサイズが0バイトなら
                if (readSize == 0)
                {
                    // 読み取り中なのに最後まで読み込めなかったことを返す
                    return false;
                }
            }


            // ループから抜けてきたのなら、希望通りのサイズまで読み込めたということを返す
            return true;
        }
    }
}