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
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace IceMilkTea.Module
{
    /// <summary>
    /// HTTPを用いたアセットフェッチャクラスです
    /// </summary>
    public class HttpAssetFetcher : IAssetFetcher
    {
        // 定数定義
        public const int DefaultBufferSize = 1 << 20;
        public const int DefaultTimeOutInterval = 10 * 1000;
        public const int NotifyInterval = 100;

        // クラス変数宣言
        private static readonly Progress<double> emptyProgress;

        // メンバ変数定義
        private Uri assetUri;
        private int bufferSize;
        private int timeoutInterval;
        private Stopwatch notifyIntervalStopwatch;



        /// <summary>
        /// HttpAssetFetcher クラスの初期化をします
        /// </summary>
        static HttpAssetFetcher()
        {
            // クラス変数の初期化
            emptyProgress = new Progress<double>();
        }


        /// <summary>
        /// HttpAssetFetcher クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="assetUri">ダウンロードするアセットのURI</param>
        /// <param name="bufferSize">ダウンロードバッファサイズ。既定は DefaultBufferSize です。</param>
        /// <exception cref="ArgumentNullException">assetUri が null です</exception>
        public HttpAssetFetcher(Uri assetUri) : this(assetUri, DefaultBufferSize, DefaultTimeOutInterval)
        {
        }


        /// <summary>
        /// HttpAssetFetcher クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="assetUri">ダウンロードするアセットのURI</param>
        /// <param name="bufferSize">ダウンロードバッファサイズ。既定は DefaultBufferSize です。</param>
        /// <exception cref="ArgumentNullException">assetUri が null です</exception>
        public HttpAssetFetcher(Uri assetUri, int bufferSize) : this(assetUri, bufferSize, DefaultTimeOutInterval)
        {
        }


        /// <summary>
        /// HttpAssetFetcher クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="assetUri">ダウンロードするアセットのURI</param>
        /// <param name="bufferSize">ダウンロードバッファサイズ。既定は DefaultBufferSize です。</param>
        /// <param name="timeoutInterval">レスポンスを受け取るまでのタイムアウト時間をミリ秒で指定します。無限に待ち続ける場合は -1 を指定します。既定は DefaultTimeOutInterval です</param>
        /// <exception cref="ArgumentNullException">assetUri が null です</exception>
        public HttpAssetFetcher(Uri assetUri, int bufferSize, int timeoutInterval)
        {
            // 初期化する
            this.assetUri = assetUri ?? throw new ArgumentNullException(nameof(assetUri));
            this.bufferSize = bufferSize;
            this.timeoutInterval = timeoutInterval;
            notifyIntervalStopwatch = Stopwatch.StartNew();
        }


        /// <summary>
        /// アセットのフェッチを非同期で行い対象のストリームに出力します
        /// </summary>
        /// <param name="outStream">出力先のストリーム</param>
        /// <param name="progress">アセットのフェッチ進捗の通知をするプログレス。既定は null です。</param>
        /// <param name="cancellationToken">キャンセル要求を監視するためのトークン。既定は None です。</param>
        /// <returns>フェッチ処理を実行しているタスクを返します</returns>
        /// <exception cref="OperationCanceledException">非同期の操作がキャンセルされました</exception>
        /// <exception cref="TaskCanceledException">非同期の操作がキャンセルされました</exception>
        /// <exception cref="TimeoutException">HTTPの応答より先にタイムアウトしました</exception>
        /// <exception cref="WebException">HTTPの要求処理中にエラーが発生しました</exception>
        public async Task FetchAsync(Stream outStream, IProgress<double> progress, CancellationToken cancellationToken)
        {
            // この時点でのキャンセルリクエストを判定する
            cancellationToken.ThrowIfCancellationRequested();


            // プログレスのインスタンス保証をする
            progress = progress ?? emptyProgress;


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
                    // 読み取ったデータを出力先ストリームに書き込んで合計読み込みサイズを求める
                    await outStream.WriteAsync(buffer, 0, readSize, cancellationToken);
                    totalReadSize += readSize;


                    // 通知インターバル時間を超えている場合は
                    if (notifyIntervalStopwatch.ElapsedMilliseconds >= NotifyInterval)
                    {
                        // 現在の進捗を通知してストップウォットのリセットをする
                        progress.Report(contentLength == -1 ? 0.0 : totalReadSize / (double)contentLength);
                        notifyIntervalStopwatch.Restart();
                    }
                }


                // 最後に通知をする
                progress.Report(1.0);
            }
        }
    }
}