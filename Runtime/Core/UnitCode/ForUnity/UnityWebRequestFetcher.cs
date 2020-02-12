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
using UnityEngine.Networking;

namespace IceMilkTea.Core
{
    #region UnityWebRequestフェッチャ
    /// <summary>
    /// UnityWebRequestを用いたフェッチャクラスです
    /// </summary>
    public class UnityWebRequestFetcher : IDataFetcher
    {
        // 定数定義
        public const int DefaultBufferSize = 1 << 20;
        public const int DefaultTimeOutInterval = 10 * 1000;

        // メンバ変数定義
        private Uri remoteUri;
        private int bufferSize;
        private int timeoutInterval;



        /// <summary>
        /// フェッチするコンテンツの長さ。ただし長さが不明の場合は -1 になる場合があります。
        /// </summary>
        public long ContentLength { get; private set; }


        /// <summary>
        /// フェッチした長さ
        /// </summary>
        public long FetchedLength { get; private set; }



        /// <summary>
        /// UnityWebRequest クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="remoteUri">ダウンロードするリモートURI</param>
        /// <exception cref="ArgumentNullException">remoteUri が null です</exception>
        public UnityWebRequestFetcher(Uri remoteUri) : this(remoteUri, DefaultBufferSize, DefaultTimeOutInterval)
        {
        }


        /// <summary>
        /// UnityWebRequest クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="remoteUri">ダウンロードするリモートURI</param>
        /// <param name="bufferSize">ダウンロードバッファサイズ。既定は DefaultBufferSize です。</param>
        /// <exception cref="ArgumentNullException">remoteUri が null です</exception>
        public UnityWebRequestFetcher(Uri remoteUri, int bufferSize) : this(remoteUri, bufferSize, DefaultTimeOutInterval)
        {
        }


        /// <summary>
        /// UnityWebRequest クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="remoteUri">ダウンロードするリモートURI</param>
        /// <param name="bufferSize">ダウンロードバッファサイズ。既定は DefaultBufferSize です。</param>
        /// <param name="timeoutInterval">レスポンスを受け取るまでのタイムアウト時間をミリ秒で指定します。無限に待ち続ける場合は -1 を指定します。既定は DefaultTimeOutInterval です</param>
        /// <exception cref="ArgumentNullException">remoteUri が null です</exception>
        public UnityWebRequestFetcher(Uri remoteUri, int bufferSize, int timeoutInterval)
        {
            // 初期化する
            this.remoteUri = remoteUri ?? throw new ArgumentNullException(nameof(remoteUri));
            this.bufferSize = bufferSize;
            this.timeoutInterval = timeoutInterval;
        }


        /// <summary>
        /// フェッチを非同期で行い対象のストリームに出力します
        /// </summary>
        /// <param name="outStream">出力先のストリーム</param>
        /// <returns>フェッチ処理を実行しているタスクを返します</returns>
        /// <exception cref="OperationCanceledException">非同期の操作がキャンセルされました</exception>
        /// <exception cref="TaskCanceledException">非同期の操作がキャンセルされました</exception>
        /// <exception cref="TimeoutException">HTTPの応答より先にタイムアウトしました</exception>
        /// <exception cref="WebException">HTTPの要求処理中にエラーが発生しました</exception>
        public Task FetchAsync(Stream outStream)
        {
            // 進捗通知も受け取らずキャンセルしない
            return FetchAsync(outStream, new Progress<FetcherReport>(), CancellationToken.None);
        }


        /// <summary>
        /// フェッチを非同期で行い対象のストリームに出力します
        /// </summary>
        /// <param name="outStream">出力先のストリーム</param>
        /// <param name="progress">フェッチャの進捗通知を受ける進捗オブジェクト。既定は Progress です。</param>
        /// <returns>フェッチ処理を実行しているタスクを返します</returns>
        /// <exception cref="OperationCanceledException">非同期の操作がキャンセルされました</exception>
        /// <exception cref="ArgumentNullException">progress が null です</exception>
        /// <exception cref="TaskCanceledException">非同期の操作がキャンセルされました</exception>
        /// <exception cref="TimeoutException">HTTPの応答より先にタイムアウトしました</exception>
        /// <exception cref="WebException">HTTPの要求処理中にエラーが発生しました</exception>
        /// <exception cref="ArgumentNullException">outStream が null です</exception>
        public Task FetchAsync(Stream outStream, IProgress<FetcherReport> progress)
        {
            // キャンセルはしない
            return FetchAsync(outStream, progress, CancellationToken.None);
        }


        /// <summary>
        /// フェッチを非同期で行い対象のストリームに出力します
        /// </summary>
        /// <param name="outStream">出力先のストリーム</param>
        /// <param name="progress">フェッチャの進捗通知を受ける進捗オブジェクト。既定は Progress です。</param>
        /// <param name="cancellationToken">キャンセル要求を監視するためのトークン。既定は None です。</param>
        /// <returns>フェッチ処理を実行しているタスクを返します</returns>
        /// <exception cref="OperationCanceledException">非同期の操作がキャンセルされました</exception>
        /// <exception cref="ArgumentNullException">progress が null です</exception>
        /// <exception cref="TaskCanceledException">非同期の操作がキャンセルされました</exception>
        /// <exception cref="TimeoutException">HTTPの応答より先にタイムアウトしました</exception>
        /// <exception cref="WebException">HTTPの要求処理中にエラーが発生しました</exception>
        /// <exception cref="ArgumentNullException">outStream が null です</exception>
        public async Task FetchAsync(Stream outStream, IProgress<FetcherReport> progress, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (outStream == null)
            {
                // 出力先ストリームが無いとどうすればよいのか
                throw new ArgumentNullException(nameof(outStream));
            }

            // progress が null なら
            if (progress == null)
            {
                // どうやって進捗通知をすればよいだろうか
                throw new ArgumentNullException(nameof(progress));
            }

			var webRequest = UnityWebRequest.Get(remoteUri);
			webRequest.downloadHandler = new DownloadHandlerStream(outStream, new byte[bufferSize]);
			var ao = webRequest.SendWebRequest();

			// ContentLength取得ヘルパ
			long GetContentLength()
			{
				var header = webRequest.GetResponseHeader("Content-Length");
				if( header == null ) { return 0; }

				var contentLength = ulong.Parse(header);
				return (long)contentLength;
			};

			var timeoutTask = Task.Delay(timeoutInterval < 0 ? -1 : timeoutInterval, cancellationToken);

			// Content-Lengthを取るまで
			while( !ao.isDone )
			{
				ContentLength = GetContentLength();
				if( ContentLength != 0 ) { break; }

				if( timeoutTask.IsCompleted ) {
                    // 要求の中断を行いタイムアウト例外を投げる
					webRequest.Abort();
                    throw new TimeoutException("HTTPの応答より先にタイムアウトしました");				
				}
				await Task.Yield();
			}

			// Content-Length取った後のデータ取得完了まで
			await ao.ToAwaitable(
				new Progress<float>(x =>  
					{
						FetchedLength = (long)webRequest.downloadedBytes;
						progress.Report(new FetcherReport(ContentLength, FetchedLength));
					}
				)
			);

			//すぐに終わるとProgressがキックされないため確実にここでキックする
			ContentLength = GetContentLength();
			FetchedLength = (long)webRequest.downloadedBytes;
			progress.Report(new FetcherReport(ContentLength, FetchedLength));
        }
    }
    #endregion
}
