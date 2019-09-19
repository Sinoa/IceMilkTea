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

namespace IceMilkTea.SubSystem
{
    /// <summary>
    /// アーカイブのローレベルなライタークラスです。
    /// </summary>
    /// <remarks>
    /// このクラスは、ヘッダやエントリ情報などのデータを素直に書き込むだけの実装になっており、アーカイブの整合性などを保証をしていません。
    /// ストリームの位置などを意識した制御を行うようにしてください。
    /// </remarks>
    public class ImtArchiveWriter
    {
        // メンバ変数定義
        private byte[] internalBuffer;



        /// <summary>
        /// このクラスがコンストラクタで受け取ったストリーム
        /// </summary>
        public Stream BaseStream { get; private set; }



        /// <summary>
        /// ImtArchiveWriter のインスタンスを初期化します
        /// </summary>
        /// <remarks>
        /// このクラスは、渡されたストリームを閉じることは無いので、インスタンスはそのまま参照を捨てても問題はありません
        /// </remarks>
        /// <param name="stream">アーカイブデータを書き込むためのストリーム</param>
        /// <exception cref="ArgumentNullException">stream が null です</exception>
        /// <exception cref="NotSupportedException">stream が CanWrite をサポートしていません</exception>
        public ImtArchiveWriter(Stream stream)
        {
            // ストリームが渡されていないのなら
            if (stream == null)
            {
                // 何も出来ない
                throw new ArgumentNullException(nameof(stream));
            }


            // ストリームの書き込みが出来ないのなら
            if (!stream.CanWrite)
            {
                // 書き込みが出来ないとダメです
                throw new NotSupportedException($"{nameof(stream)} が CanWrite をサポートしていません");
            }


            // ストリームを覚えて、内部バッファを今のうちに用意しておく
            BaseStream = stream;
            internalBuffer = new byte[ImtArchiveHeader.HeaderSize > ImtArchiveEntryInfo.InfoSize ? ImtArchiveHeader.HeaderSize : ImtArchiveEntryInfo.InfoSize];
        }


        /// <summary>
        /// ストリームに ImtArchiveHeader を書き込みます
        /// </summary>
        /// <param name="header">書き込む ImtArchiveHeader 構造体の参照</param>
        /// <exception cref="ArgumentException">ヘッダ内の MagicNumber が null または 4バイト 未満です</exception>
        public void Write(ref ImtArchiveHeader header)
        {
            // マジックナンバーが希望な状態でないなら
            if (header.MagicNumber == null || header.MagicNumber.Length < 4)
            {
                // 書き込めない
                throw new ArgumentException($"ヘッダ内の {nameof(header.MagicNumber)} が null です", nameof(header.MagicNumber));
            }


            // マジックナンバーをバッファに詰めて書き込みインデックスを用意
            Array.Copy(header.MagicNumber, internalBuffer, sizeof(byte) * 4);
            var index = sizeof(byte) * 4;


            // リトルエンディアンなCPUなら
            if (BitConverter.IsLittleEndian)
            {
                // リトルエンディアン向けのバッファ書き込みをしていく
                // uint書き込み
                internalBuffer[index++] = (byte)((header.ArchiveInfo >> 0) & 0xFF);
                internalBuffer[index++] = (byte)((header.ArchiveInfo >> 8) & 0xFF);
                internalBuffer[index++] = (byte)((header.ArchiveInfo >> 16) & 0xFF);
                internalBuffer[index++] = (byte)((header.ArchiveInfo >> 24) & 0xFF);


                // long書き込み
                internalBuffer[index++] = (byte)((header.EntryInfoListOffset >> 0) & 0xFF);
                internalBuffer[index++] = (byte)((header.EntryInfoListOffset >> 8) & 0xFF);
                internalBuffer[index++] = (byte)((header.EntryInfoListOffset >> 16) & 0xFF);
                internalBuffer[index++] = (byte)((header.EntryInfoListOffset >> 24) & 0xFF);
                internalBuffer[index++] = (byte)((header.EntryInfoListOffset >> 32) & 0xFF);
                internalBuffer[index++] = (byte)((header.EntryInfoListOffset >> 40) & 0xFF);
                internalBuffer[index++] = (byte)((header.EntryInfoListOffset >> 48) & 0xFF);
                internalBuffer[index++] = (byte)((header.EntryInfoListOffset >> 56) & 0xFF);


                // int書き込み
                internalBuffer[index++] = (byte)((header.EntryInfoCount >> 0) & 0xFF);
                internalBuffer[index++] = (byte)((header.EntryInfoCount >> 8) & 0xFF);
                internalBuffer[index++] = (byte)((header.EntryInfoCount >> 16) & 0xFF);
                internalBuffer[index++] = (byte)((header.EntryInfoCount >> 24) & 0xFF);


                // uint書き込み
                internalBuffer[index++] = (byte)((header.Reserved >> 0) & 0xFF);
                internalBuffer[index++] = (byte)((header.Reserved >> 8) & 0xFF);
                internalBuffer[index++] = (byte)((header.Reserved >> 16) & 0xFF);
                internalBuffer[index++] = (byte)((header.Reserved >> 24) & 0xFF);
            }
            else
            {
                // ビッグエンディアン向けのバッファ書き込みをしていく（実際はバッファにはリトルエンディアン状態になる）
                // uint書き込み
                internalBuffer[index++] = (byte)((header.ArchiveInfo >> 24) & 0xFF);
                internalBuffer[index++] = (byte)((header.ArchiveInfo >> 16) & 0xFF);
                internalBuffer[index++] = (byte)((header.ArchiveInfo >> 8) & 0xFF);
                internalBuffer[index++] = (byte)((header.ArchiveInfo >> 0) & 0xFF);


                // long書き込み
                internalBuffer[index++] = (byte)((header.EntryInfoListOffset >> 56) & 0xFF);
                internalBuffer[index++] = (byte)((header.EntryInfoListOffset >> 48) & 0xFF);
                internalBuffer[index++] = (byte)((header.EntryInfoListOffset >> 40) & 0xFF);
                internalBuffer[index++] = (byte)((header.EntryInfoListOffset >> 32) & 0xFF);
                internalBuffer[index++] = (byte)((header.EntryInfoListOffset >> 24) & 0xFF);
                internalBuffer[index++] = (byte)((header.EntryInfoListOffset >> 16) & 0xFF);
                internalBuffer[index++] = (byte)((header.EntryInfoListOffset >> 8) & 0xFF);
                internalBuffer[index++] = (byte)((header.EntryInfoListOffset >> 0) & 0xFF);


                // int書き込み
                internalBuffer[index++] = (byte)((header.EntryInfoCount >> 24) & 0xFF);
                internalBuffer[index++] = (byte)((header.EntryInfoCount >> 16) & 0xFF);
                internalBuffer[index++] = (byte)((header.EntryInfoCount >> 8) & 0xFF);
                internalBuffer[index++] = (byte)((header.EntryInfoCount >> 0) & 0xFF);


                // uint書き込み
                internalBuffer[index++] = (byte)((header.Reserved >> 24) & 0xFF);
                internalBuffer[index++] = (byte)((header.Reserved >> 16) & 0xFF);
                internalBuffer[index++] = (byte)((header.Reserved >> 8) & 0xFF);
                internalBuffer[index++] = (byte)((header.Reserved >> 0) & 0xFF);
            }


            // バッファ内容をストリームに書き込む
            BaseStream.Write(internalBuffer, 0, ImtArchiveHeader.HeaderSize);
        }


        /// <summary>
        /// ストリームに ImtArchiveEntryInfo を書き込みます
        /// </summary>
        /// <param name="info">書き込む ImtArchiveEntryInfo 構造体の参照</param>
        public void Write(ref ImtArchiveEntryInfo info)
        {
            // 書き込みインデックスを用意
            var index = 0;


            // もしリトルエンディアンなCPUなら
            if (BitConverter.IsLittleEndian)
            {
                // リトルエンディアン向けのバッファ書き込みをしていく
                // ulong書き込み
                internalBuffer[index++] = (byte)((info.Id >> 0) & 0xFF);
                internalBuffer[index++] = (byte)((info.Id >> 8) & 0xFF);
                internalBuffer[index++] = (byte)((info.Id >> 16) & 0xFF);
                internalBuffer[index++] = (byte)((info.Id >> 24) & 0xFF);
                internalBuffer[index++] = (byte)((info.Id >> 32) & 0xFF);
                internalBuffer[index++] = (byte)((info.Id >> 40) & 0xFF);
                internalBuffer[index++] = (byte)((info.Id >> 48) & 0xFF);
                internalBuffer[index++] = (byte)((info.Id >> 56) & 0xFF);


                // long書き込み
                internalBuffer[index++] = (byte)((info.Offset >> 0) & 0xFF);
                internalBuffer[index++] = (byte)((info.Offset >> 8) & 0xFF);
                internalBuffer[index++] = (byte)((info.Offset >> 16) & 0xFF);
                internalBuffer[index++] = (byte)((info.Offset >> 24) & 0xFF);
                internalBuffer[index++] = (byte)((info.Offset >> 32) & 0xFF);
                internalBuffer[index++] = (byte)((info.Offset >> 40) & 0xFF);
                internalBuffer[index++] = (byte)((info.Offset >> 48) & 0xFF);
                internalBuffer[index++] = (byte)((info.Offset >> 56) & 0xFF);


                // long書き込み
                internalBuffer[index++] = (byte)((info.Size >> 0) & 0xFF);
                internalBuffer[index++] = (byte)((info.Size >> 8) & 0xFF);
                internalBuffer[index++] = (byte)((info.Size >> 16) & 0xFF);
                internalBuffer[index++] = (byte)((info.Size >> 24) & 0xFF);
                internalBuffer[index++] = (byte)((info.Size >> 32) & 0xFF);
                internalBuffer[index++] = (byte)((info.Size >> 40) & 0xFF);
                internalBuffer[index++] = (byte)((info.Size >> 48) & 0xFF);
                internalBuffer[index++] = (byte)((info.Size >> 56) & 0xFF);


                // ulong書き込み
                internalBuffer[index++] = (byte)((info.Reserved >> 0) & 0xFF);
                internalBuffer[index++] = (byte)((info.Reserved >> 8) & 0xFF);
                internalBuffer[index++] = (byte)((info.Reserved >> 16) & 0xFF);
                internalBuffer[index++] = (byte)((info.Reserved >> 24) & 0xFF);
                internalBuffer[index++] = (byte)((info.Reserved >> 32) & 0xFF);
                internalBuffer[index++] = (byte)((info.Reserved >> 40) & 0xFF);
                internalBuffer[index++] = (byte)((info.Reserved >> 48) & 0xFF);
                internalBuffer[index++] = (byte)((info.Reserved >> 56) & 0xFF);
            }
            else
            {
                // ビッグエンディアン向けのバッファ書き込みをしていく（実際はバッファにはリトルエンディアン状態になる）
                // ulong書き込み
                internalBuffer[index++] = (byte)((info.Id >> 56) & 0xFF);
                internalBuffer[index++] = (byte)((info.Id >> 48) & 0xFF);
                internalBuffer[index++] = (byte)((info.Id >> 40) & 0xFF);
                internalBuffer[index++] = (byte)((info.Id >> 32) & 0xFF);
                internalBuffer[index++] = (byte)((info.Id >> 24) & 0xFF);
                internalBuffer[index++] = (byte)((info.Id >> 16) & 0xFF);
                internalBuffer[index++] = (byte)((info.Id >> 8) & 0xFF);
                internalBuffer[index++] = (byte)((info.Id >> 0) & 0xFF);


                // long書き込み
                internalBuffer[index++] = (byte)((info.Offset >> 56) & 0xFF);
                internalBuffer[index++] = (byte)((info.Offset >> 48) & 0xFF);
                internalBuffer[index++] = (byte)((info.Offset >> 40) & 0xFF);
                internalBuffer[index++] = (byte)((info.Offset >> 32) & 0xFF);
                internalBuffer[index++] = (byte)((info.Offset >> 24) & 0xFF);
                internalBuffer[index++] = (byte)((info.Offset >> 16) & 0xFF);
                internalBuffer[index++] = (byte)((info.Offset >> 8) & 0xFF);
                internalBuffer[index++] = (byte)((info.Offset >> 0) & 0xFF);


                // long書き込み
                internalBuffer[index++] = (byte)((info.Size >> 56) & 0xFF);
                internalBuffer[index++] = (byte)((info.Size >> 48) & 0xFF);
                internalBuffer[index++] = (byte)((info.Size >> 40) & 0xFF);
                internalBuffer[index++] = (byte)((info.Size >> 32) & 0xFF);
                internalBuffer[index++] = (byte)((info.Size >> 24) & 0xFF);
                internalBuffer[index++] = (byte)((info.Size >> 16) & 0xFF);
                internalBuffer[index++] = (byte)((info.Size >> 8) & 0xFF);
                internalBuffer[index++] = (byte)((info.Size >> 0) & 0xFF);


                // ulong書き込み
                internalBuffer[index++] = (byte)((info.Reserved >> 56) & 0xFF);
                internalBuffer[index++] = (byte)((info.Reserved >> 48) & 0xFF);
                internalBuffer[index++] = (byte)((info.Reserved >> 40) & 0xFF);
                internalBuffer[index++] = (byte)((info.Reserved >> 32) & 0xFF);
                internalBuffer[index++] = (byte)((info.Reserved >> 24) & 0xFF);
                internalBuffer[index++] = (byte)((info.Reserved >> 16) & 0xFF);
                internalBuffer[index++] = (byte)((info.Reserved >> 8) & 0xFF);
                internalBuffer[index++] = (byte)((info.Reserved >> 0) & 0xFF);
            }


            // バッファ内容をストリームに書き込む
            BaseStream.Write(internalBuffer, 0, ImtArchiveEntryInfo.InfoSize);
        }
    }
}