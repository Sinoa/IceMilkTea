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
    /// ImtArchiveEntryInstallStream に対してストリームのIOをモニタリングすることが出来るストリームクラスです
    /// </summary>
    public class ImtArchiveEntryMonitorableInstallStream : ImtArchiveEntryInstallStream
    {
        // メンバ変数定義
        private ImtArchiveIoMonitor ioMonitor;
        private ImtArchiveEntryInfo entryInfo;



        /// <summary>
        /// ストリーム位置の取得と設定を監視します
        /// </summary>
        public override long Position
        {
            get
            {
                // 監視オブジェクトに位置取得通知をしてベースクラスに仕事をなげる
                ioMonitor.OnEntryPositionGet(ref entryInfo);
                return base.Position;
            }
            set
            {
                // 監視オブジェクトに位置設定通知をしてベースクラスに仕事を任せる
                ioMonitor.OnEntryPositionSet(ref entryInfo, value);
                base.Position = value;
            }
        }



        /// <summary>
        /// ImtArchiveEntryMonitorableInstallStream のインスタンスを初期化します
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
        /// <exception cref="ArgumentNullException">monitor が null です</exception>
        /// <see cref="FinishInstall"/>
        public ImtArchiveEntryMonitorableInstallStream(ImtArchiveEntryInfo info, Stream stream, ImtArchiveEntryInstaller installer, ImtArchiveEntryInstallFinishHandler installFinishHandler, ImtArchiveIoMonitor monitor) : base(info, stream, installer, installFinishHandler)
        {
            // モニターがnullなら
            if (monitor == null)
            {
                // 私は誰に通知をすればよいのだ
                throw new ArgumentNullException(nameof(monitor));
            }


            // 情報を受取る
            ioMonitor = monitor;
            entryInfo = info;


            // モニタにストリームオープンされたことを通知する
            ioMonitor.OnEntryOpen(ref info);
        }


        /// <summary>
        /// 通常の読み込みを行ってから、エントリの読み取りを監視オブジェクトに通知します。
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
            // ベースクラスに仕事を投げて、実際のサイズなどを監視オブジェクトに通知する
            var result = base.Read(buffer, offset, count);
            ioMonitor.OnEntryRead(ref entryInfo, buffer.Length, offset, count, result);
            return result;
        }


        /// <summary>
        /// ストリームの位置を変更する内容を、監視オブジェクトに通知して、通常のストリームの位置設定を行います。
        /// </summary>
        /// <param name="offset">設定する位置</param>
        /// <param name="origin">指定された offset がどの位置からを示すかを表します</param>
        /// <returns>設定された新しいストリームの位置を返します</returns>
        /// <exception cref="ArgumentOutOfRangeException">指定されたオフセットがストリームの範囲を超えています</exception>
        public override long Seek(long offset, SeekOrigin origin)
        {
            // 監視オブジェクトにシーク通知をしてベースクラスに仕事を投げる
            ioMonitor.OnEntrySeek(ref entryInfo, offset, origin);
            return base.Seek(offset, origin);
        }


        /// <summary>
        /// 指定されたバッファの範囲を、ストリームに書き込みます。
        /// また、この関数はストリームの長さを超えた、書き込みを行う事は出来ません。
        /// 必要以上に書き込もうとしても書き込まれない可能性があります。
        /// その後、エントリの書き込みを監視オブジェクトに通知します。
        /// </summary>
        /// <param name="buffer">ストリームに書き込むための、読み取り用バッファ</param>
        /// <param name="offset">バッファから読み取る開始インデックス</param>
        /// <param name="count">バッファから読み取るサイズ</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            // 仮想位置を覚えて、ベースクラスに書き込みをしてもらって、新しいポジションから引くと書き込んだサイズが導き出せる
            var currentVirtualPosition = base.Position;
            base.Write(buffer, offset, count);
            ioMonitor.OnEntryWrite(ref entryInfo, buffer.Length, offset, count, (int)(base.Position - currentVirtualPosition));
        }
    }
}