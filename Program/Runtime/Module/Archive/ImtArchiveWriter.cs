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
    /// アーカイブのローレベルなライタークラスです。
    /// </summary>
    /// <remarks>
    /// このクラスは、ヘッダやエントリ情報などのデータを素直に書き込むだけの実装になっており、アーカイブの整合性などを保証をしていません。
    /// ストリームの位置などを意識した制御を行うようにしてください。
    /// </remarks>
    public class ImtArchiveWriter
    {
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
        /// <exception cref="ArgumentException">stream が CanWrite をサポートしていません</exception>
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
                throw new ArgumentException($"{nameof(stream)} が CanWrite をサポートしていません");
            }


            // ストリームを覚える
            BaseStream = stream;
        }


        /// <summary>
        /// ストリームに ImtArchiveHeader を書き込みます
        /// </summary>
        /// <param name="header">書き込む ImtArchiveHeader 構造体の参照</param>
        public void Write(ref ImtArchiveHeader header)
        {
            // マジックナンバーを書き込む
            BaseStream.Write(header.MagicNumber, 0, sizeof(byte) * 4);


            // 他のフィールドは一度バッファ化
            // TODO : もしかしたら、ビット演算で格納していったほうが早いかもしれない
            var archiveInfoBuffer = BitConverter.GetBytes(header.ArchiveInfo);
            var entryInfoListOffsetBuffer = BitConverter.GetBytes(header.EntryInfoListOffset);
            var entryInfoCountBuffer = BitConverter.GetBytes(header.EntryInfoCount);
            var reservedBuffer = BitConverter.GetBytes(header.Reserved);


            // もしビッグエンディアンなCPUなら
            // TODO : 事前にビット演算済み制御だったらこんなフローも不要な可能性
            if (!BitConverter.IsLittleEndian)
            {
                // 全バッファを反転する
                Array.Reverse(archiveInfoBuffer);
                Array.Reverse(entryInfoListOffsetBuffer);
                Array.Reverse(entryInfoCountBuffer);
                Array.Reverse(reservedBuffer);
            }


            // 全バッファ化したデータを書き込んでいく
            BaseStream.Write(archiveInfoBuffer, 0, archiveInfoBuffer.Length);
            BaseStream.Write(entryInfoListOffsetBuffer, 0, entryInfoListOffsetBuffer.Length);
            BaseStream.Write(entryInfoCountBuffer, 0, entryInfoCountBuffer.Length);
            BaseStream.Write(reservedBuffer, 0, reservedBuffer.Length);
        }


        /// <summary>
        /// ストリームに ImtArchiveEntryInfo を書き込みます
        /// </summary>
        /// <param name="info">書き込む ImtArchiveEntryInfo 構造体の参照</param>
        public void Write(ref ImtArchiveEntryInfo info)
        {
            // 全フィールドを一度バッファ化
            // TODO : もしかしたら、ビット演算で格納していったほうが早いかもしれない
            var idBuffer = BitConverter.GetBytes(info.Id);
            var offsetBuffer = BitConverter.GetBytes(info.Offset);
            var sizeBuffer = BitConverter.GetBytes(info.Size);
            var reservedBuffer = BitConverter.GetBytes(info.Reserved);


            // もしビッグエンディアンなCPUなら
            // TODO : 事前にビット演算済み制御だったらこんなフローも不要な可能性
            if (!BitConverter.IsLittleEndian)
            {
                // 全バッファを反転する
                Array.Reverse(idBuffer);
                Array.Reverse(offsetBuffer);
                Array.Reverse(sizeBuffer);
                Array.Reverse(reservedBuffer);
            }


            // 全バッファ化したデータを書き込んでいく
            BaseStream.Write(idBuffer, 0, idBuffer.Length);
            BaseStream.Write(offsetBuffer, 0, offsetBuffer.Length);
            BaseStream.Write(sizeBuffer, 0, sizeBuffer.Length);
            BaseStream.Write(reservedBuffer, 0, reservedBuffer.Length);
        }


        /// <summary>
        /// 指定されたバッファ全体を書き込みます
        /// </summary>
        /// <param name="buffer">書き込むバッファ</param>
        public void Write(byte[] buffer)
        {
            // インデックスの位置と長さを指定して書き込む
            Write(buffer, 0, buffer.Length);
        }


        /// <summary>
        /// 指定されたバッファの範囲を書き込みます
        /// </summary>
        /// <param name="buffer">書き込むバッファ</param>
        /// <param name="index">バッファの読み取る開始インデックス</param>
        /// <param name="count">バッファから読み取る量</param>
        public void Write(byte[] buffer, int index, int count)
        {
            // 素直に書き込む
            BaseStream.Write(buffer, index, count);
        }
    }
}