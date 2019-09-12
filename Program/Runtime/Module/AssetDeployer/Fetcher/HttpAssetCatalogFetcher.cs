// zlib/libpng License
//
// Copyright (c) 2019 Sinoa
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
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace IceMilkTea.Module
{
    /// <summary>
    /// HTTPを用いたアセットカタログフェッチャクラスです
    /// </summary>
    public class HttpAssetCatalogFetcher : IAssetCatalogFetcher
    {
        // 定数定義
        public const int DefaultBufferSize = 1 << 20;
        public const int DefaultTimeOutInterval = 10 * 1000;

        // メンバ変数定義
        private Uri assetUri;
        private int bufferSize;
        private int timeoutInterval;



        /// <summary>
        /// フェッチの進捗を割合で取得します
        /// </summary>
        public double Progress { get; private set; }



        /// <summary>
        /// HttpAssetCatalogFetcher クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="assetUri">ダウンロードするアセットのURI</param>
        /// <exception cref="ArgumentNullException">assetUri が null です</exception>
        public HttpAssetCatalogFetcher(Uri assetUri) : this(assetUri, DefaultBufferSize, DefaultTimeOutInterval)
        {
        }


        /// <summary>
        /// HttpAssetCatalogFetcher クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="assetUri">ダウンロードするアセットのURI</param>
        /// <param name="bufferSize">ダウンロードバッファサイズ。既定は DefaultBufferSize です。</param>
        /// <exception cref="ArgumentNullException">assetUri が null です</exception>
        public HttpAssetCatalogFetcher(Uri assetUri, int bufferSize) : this(assetUri, bufferSize, DefaultTimeOutInterval)
        {
        }


        /// <summary>
        /// HttpAssetCatalogFetcher クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="assetUri">ダウンロードするアセットのURI</param>
        /// <param name="bufferSize">ダウンロードバッファサイズ。既定は DefaultBufferSize です。</param>
        /// <param name="timeoutInterval">レスポンスを受け取るまでのタイムアウト時間をミリ秒で指定します。無限に待ち続ける場合は -1 を指定します。既定は DefaultTimeOutInterval です</param>
        /// <exception cref="ArgumentNullException">assetUri が null です</exception>
        public HttpAssetCatalogFetcher(Uri assetUri, int bufferSize, int timeoutInterval)
        {
            // 初期化する
            this.assetUri = assetUri ?? throw new ArgumentNullException(nameof(assetUri));
            this.bufferSize = bufferSize;
            this.timeoutInterval = timeoutInterval;
        }


        /// <summary>
        /// アセットカタログのフェッチを非同期で行い対象のストリームに出力します
        /// </summary>
        /// <param name="outStream">出力先のストリーム</param>
        /// <returns>フェッチ処理を実行しているタスクを返します</returns>
        /// <exception cref="OperationCanceledException">非同期の操作がキャンセルされました</exception>
        /// <exception cref="TaskCanceledException">非同期の操作がキャンセルされました</exception>
        /// <exception cref="TimeoutException">HTTPの応答より先にタイムアウトしました</exception>
        /// <exception cref="WebException">HTTPの要求処理中にエラーが発生しました</exception>
        public Task FetchAsync(Stream outStream)
        {
            // キャンセルはしない同じ関数を叩く
            return FetchAsync(outStream, CancellationToken.None);
        }


        /// <summary>
        /// アセットカタログのフェッチを非同期で行い対象のストリームに出力します
        /// </summary>
        /// <param name="outStream">出力先のストリーム</param>
        /// <param name="cancellationToken">キャンセル要求を監視するためのトークン。既定は None です。</param>
        /// <returns>フェッチ処理を実行しているタスクを返します</returns>
        /// <exception cref="OperationCanceledException">非同期の操作がキャンセルされました</exception>
        /// <exception cref="TaskCanceledException">非同期の操作がキャンセルされました</exception>
        /// <exception cref="TimeoutException">HTTPの応答より先にタイムアウトしました</exception>
        /// <exception cref="WebException">HTTPの要求処理中にエラーが発生しました</exception>
        /// <exception cref="ArgumentNullException">outStream が null です</exception>
        public async Task FetchAsync(Stream outStream, CancellationToken cancellationToken)
        {
            // 進捗率をリセット
            Progress = 0.0;


            // この時点でのキャンセルリクエストを判定してさらに出力先ストリームが無いなら
            cancellationToken.ThrowIfCancellationRequested();
            if (outStream == null)
            {
                // 出力先ストリームが無いとどうすればよいのか
                throw new ArgumentNullException(nameof(outStream));
            }


            // WebRequestのインスタンスを生成してからレスポンスタスクとタイムアウトタスクを生成して、先にタイムアウトタスクが完了してしまったのなら
            var request = WebRequest.CreateHttp(assetUri);
            var responseTask = request.GetResponseAsync();
            var timeoutTask = Task.Delay(timeoutInterval < 0 ? -1 : timeoutInterval, cancellationToken);
            var finishTask = await Task.WhenAny(responseTask, timeoutTask);
            if (finishTask == timeoutTask)
            {
                // 要求の中断を行いタイムアウト例外を投げる
                request.Abort();
                throw new TimeoutException("HTTPの応答より先にタイムアウトしました");
            }


            // レスポンスとストリームを受け取る
            using (var response = (HttpWebResponse)responseTask.Result)
            using (var stream = response.GetResponseStream())
            {
                // 受信バッファを生成
                var contentLength = response.ContentLength;
                var totalReadSize = 0L;
                var buffer = new byte[bufferSize];


                // 全てのストリームを読みきるまでループ
                int readSize = 0;
                while ((readSize = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                {
                    // 読み取ったデータを出力先ストリームに書き込んで合計読み込みサイズと進捗を求める
                    await outStream.WriteAsync(buffer, 0, readSize, cancellationToken);
                    totalReadSize += readSize;
                    Progress = contentLength == -1 ? 0.0 : totalReadSize / (double)contentLength;
                }


                // 最後は無条件に進捗を1.0にする
                Progress = 1.0;
            }
        }
    }
}