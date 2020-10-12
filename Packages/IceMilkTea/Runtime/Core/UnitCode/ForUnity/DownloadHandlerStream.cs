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
using UnityEngine.Networking;

namespace IceMilkTea.Core
{
    /// <summary>
    /// UnityWebRequest で用いる DownloadHandler の実装クラスです。
    /// この DownloadHandler は、渡されたストリームに対して直接出力を行います。
    /// </summary>
    public class DownloadHandlerStream : DownloadHandlerScript
    {
        // 定数定義
        private const int DefaultReceiveBufferSize = 16 << 10;

        // メンバ変数定義
        private Stream outputStream;
        private int contentLength;
        private int receivedSize;



        /// <summary>
        /// DownloadHandlerStream のインスタンスを既定受信バッファサイズで初期化をします
        /// </summary>
        /// <param name="outputStream">ダウンロードしたデータを出力する先のストリーム</param>
        /// <exception cref="ArgumentNullException">outputStream が null です</exception>
        /// <exception cref="ArgumentException">書き込みをサポートしていない出力ストリームです</exception>
        public DownloadHandlerStream(Stream outputStream) : this(outputStream, new byte[DefaultReceiveBufferSize])
        {
        }


        /// <summary>
        /// DownloadHandlerStream のインスタンスを指定された受信バッファサイズで初期化をします
        /// </summary>
        /// <param name="outputStream">ダウンロードしたデータを出力する先のストリーム</param>
        /// <param name="receiveBufferSize"></param>
        /// <exception cref="ArgumentNullException">outputStream が null です</exception>
        /// <exception cref="ArgumentException">書き込みをサポートしていない出力ストリームです</exception>
        public DownloadHandlerStream(Stream outputStream, int receiveBufferSize) : this(outputStream, new byte[receiveBufferSize])
        {
        }


        /// <summary>
        /// DownloadHandlerStream のインスタンス
        /// </summary>
        /// <param name="outputStream">ダウンロードしたデータを出力する先のストリーム</param>
        /// <param name="outsideReceiveBuffer"></param>
        /// <exception cref="ArgumentNullException">outputStream が null です</exception>
        /// <exception cref="ArgumentException">書き込みをサポートしていない出力ストリームです</exception>
        public DownloadHandlerStream(Stream outputStream, byte[] outsideReceiveBuffer) : base(outsideReceiveBuffer)
        {
            // nullを渡されたら
            if (outputStream == null)
            {
                // 出力先がnullなのはだめ
                throw new ArgumentNullException(nameof(outputStream));
            }


            // 出力先ストリームが書き込みをサポートしていないなら
            if (!outputStream.CanWrite)
            {
                // 書き込みが出来ないのにどうやって出力しろと
                throw new ArgumentException("書き込みをサポートしていない出力ストリームです", nameof(outputStream));
            }


            // ストリームを受ける
            this.outputStream = outputStream;
        }


        /// <summary>
        /// コンテンツの長さを受信した時の処理を行います
        /// </summary>
        /// <param name="contentLength">受信したコンテンツの長さ</param>
        [Obsolete]
        protected override void ReceiveContentLength(int contentLength)
        {
            // コンテンツの長さを覚える
            this.contentLength = contentLength;
        }


        /// <summary>
        /// データを受信した時の処理を行います
        /// </summary>
        /// <param name="data">受信したデータを持っているバッファ</param>
        /// <param name="dataLength">受信した実際の長さ</param>
        /// <returns>ダウンロードを継続する場合は true を、中断する場合は false を返します</returns>
        protected override bool ReceiveData(byte[] data, int dataLength)
        {
            // 受信データをストリームに書き込む
            outputStream.Write(data, 0, dataLength);


            // 受信した合計サイズに加算する
            receivedSize += dataLength;
            return true;
        }


        /// <summary>
        /// ダウンロードの進捗を取得します
        /// </summary>
        /// <returns>ダウンロードの進捗率を返します</returns>
        protected override float GetProgress()
        {
            // もし ContentLength ヘッダが渡されていなくてコンテンツの長さが不明な時は常に0.0を返し続ける
            return contentLength == 0 ? 0.0f : (float)receivedSize / contentLength;
        }


        /// <summary>
        /// 常に長さ0の配列を取得します
        /// </summary>
        /// <returns>常に長さ0の配列を返します</returns>
        protected override byte[] GetData()
        {
            // データはストリームへ書き込まれているため、データを拾うことは出来ないので、長さ0の配列を返す
            return Array.Empty<byte>();
        }


        /// <summary>
        /// 常に空文字列を取得します
        /// </summary>
        /// <returns>常に空文字列を返します</returns>
        protected override string GetText()
        {
            // 理由はGetData関数と同様
            return string.Empty;
        }
    }
}