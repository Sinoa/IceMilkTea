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
    /// ImtArchiveEntryStream に対してストリームのIOをモニタリングすることが出来るストリームクラスです
    /// </summary>
    public class ImtArchiveEntryMonitorableStream : ImtArchiveEntryStream
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
        /// ImtArchiveEntryMonitorableStream のインスタンスを初期化します
        /// </summary>
        /// <param name="info">監視するエントリ情報</param>
        /// <param name="stream">ベースクラスに渡すストリーム</param>
        /// <param name="monitor">実際の監視を行う監視オブジェクト</param>
        /// <exception cref="ArgumentNullException">stream が null です</exception>
        /// <exception cref="ArgumentException">渡されたエントリ情報に問題が発生しました。詳細は例外の内容を確認してください。</exception>
        /// <exception cref="NotSupportedException">ストリームは最低でも CanRead および CanSeek をサポートしなければいけません</exception>
        /// <exception cref="ArgumentException">エントリの実体が存在しないエントリの情報が渡されました</exception>
        /// <exception cref="ArgumentNullException">monitor が null です</exception>
        public ImtArchiveEntryMonitorableStream(ImtArchiveEntryInfo info, Stream stream, ImtArchiveIoMonitor monitor) : base(info, stream)
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
    }
}