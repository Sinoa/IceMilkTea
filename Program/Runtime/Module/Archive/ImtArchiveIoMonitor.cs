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

using System.IO;

namespace IceMilkTea.Module
{
    /// <summary>
    /// アーカイブのIOを細かく監視することが出来る抽象クラスです。
    /// また、モニタリングクラスのすべてのメンバはスレッドセーフに動作しなければなりません。
    /// </summary>
    public abstract class ImtArchiveIoMonitor
    {
        /// <summary>
        /// アーカイブからエントリのオープンが行われた事を知ります
        /// </summary>
        /// <param name="entryInfo">オープンされたエントリ情報</param>
        protected internal abstract void OnEntryOpen(ref ImtArchiveEntryInfo entryInfo);


        /// <summary>
        /// エントリがシークしたことを知ります
        /// </summary>
        /// <param name="entryInfo">シークしたエントリ情報</param>
        /// <param name="offset">シークしたオフセット</param>
        /// <param name="origin">指定された、シークする原点</param>
        protected internal abstract void OnEntrySeek(ref ImtArchiveEntryInfo entryInfo, long offset, SeekOrigin origin);


        /// <summary>
        /// エントリがデータを読み込んだ事を知ります
        /// </summary>
        /// <param name="entryInfo">データの読み込みをしたエントリ情報</param>
        /// <param name="bufferLength">読み取ったデータを書き込むバッファの長さ</param>
        /// <param name="offset">指定されたバッファの書き込み位置</param>
        /// <param name="requestSize">要求された読み込みサイズ</param>
        /// <param name="readSize">読み込まれた実際のサイズ</param>
        protected internal abstract void OnEntryRead(ref ImtArchiveEntryInfo entryInfo, int bufferLength, long offset, int requestSize, int readSize);


        /// <summary>
        /// エントリがデータを書き込んだ事を知ります
        /// </summary>
        /// <param name="entryInfo">データの書き込みをしたエントリ情報</param>
        /// <param name="bufferLength">書き込みを行うバッファの長さ</param>
        /// <param name="offset">指定されたバッファの読み込み位置</param>
        /// <param name="writeSize">指定されたバッファの読み取りサイズ</param>
        protected internal abstract void OnEntryWrite(ref ImtArchiveEntryInfo entryInfo, int bufferLength, long offset, int writeSize);


        /// <summary>
        /// エントリがストリームの位置を取得したことを知ります
        /// </summary>
        /// <param name="entryInfo">ストリームの位置を取得しようとしたエントリ</param>
        protected internal abstract void OnEntryPositionGet(ref ImtArchiveEntryInfo entryInfo);


        /// <summary>
        /// エントリがストリームの位置を設定したことを知ります
        /// </summary>
        /// <param name="entryInfo">ストリームの位置を設定しようとしたエントリ</param>
        /// <param name="value">設定したストリームの位置</param>
        protected internal abstract void OnEntryPositionSet(ref ImtArchiveEntryInfo entryInfo, long value);
    }
}