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
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace IceMilkTea.Core
{
    /// <summary>
    /// HTTPアクセスを用いたWebからデータをダウンロードをするダウンローダクラスです
    /// </summary>
    public class WebDownloader
    {
        // 公開定数定義
        public const int DefaultReceiveBufferSize = 1 << 10;
        public const int DefaultTimeoutTime = 5000;
        public const int DefaultRetryCount = 2;
        public const int DefaultRetryWaitTime = 500;

        // 定数定義
        private const int DownloadProgressNotifyInterval = 100;

        // メンバ変数定義
        private CancellationTokenSource cancellationTokenSource;
        private byte[] receiveBuffer;
        private bool downloading;



        /// <summary>
        /// ダウンロードが開始される際に呼び出されます
        /// </summary>
        public event Action OnDownloadStart;

        /// <summary>
        /// ダウンロード中にリトライできないエラーか、リトライ回数を超えてエラーが発生した際に呼び出されます
        /// </summary>
        public event Action<Exception> OnDownloadError;

        /// <summary>
        /// ダウンロードが中断された際に呼び出されます
        /// </summary>
        public event Action OnDownloadCancel;

        /// <summary>
        /// ダウンロードに問題が発生しリトライ待機をした際に呼び出されます
        /// </summary>
        public event Action OnDownloadRetryWait;

        /// <summary>
        /// ダウンロードがリトライした際に呼び出されます
        /// </summary>
        public event Action OnDownloadRetry;

        /// <summary>
        /// ダウンロードが終了した際に呼び出されます
        /// </summary>
        public event Action OnDownloadFinish;



        /// <summary>
        /// Webアクセス時にアクセスタイムアウトになるまでの時間（ミリ秒）を取得します
        /// </summary>
        public int TimeoutTime { get; private set; }


        /// <summary>
        /// ダウンロード失敗時にリトライする最大回数を取得します
        /// </summary>
        public int RetryCount { get; private set; }


        /// <summary>
        /// リトライする際にリトライするまでの待機時間（ミリ秒）を取得します。
        /// またリトライする毎に、指定された時間の倍数の待機を行います。
        /// </summary>
        public int RetryWaitTime { get; private set; }



        #region 初期化コード
        /// <summary>
        /// WebDownloader のインスタンスを既定の値で初期化します
        /// </summary>
        public WebDownloader()
        {
            // 内部バッファを生成して初期化をする
            InitializeInstance(new byte[DefaultReceiveBufferSize]);
        }


        /// <summary>
        /// WebDownloader のインスタンスを内部バッファを生成する初期化をします
        /// </summary>
        /// <param name="receiveBufferSize">WebDownloader が内部で使用する受信バッファサイズ</param>
        /// <exception cref="ArgumentOutOfRangeException">WebDownloader の受信バッファサイズに 0 以下のサイズは指定出来ません</exception>
        public WebDownloader(int receiveBufferSize)
        {
            // もし0以下の値が渡されたら
            if (receiveBufferSize <= 0)
            {
                // 流石にバッファ無しはだめ
                throw new ArgumentOutOfRangeException(nameof(receiveBufferSize), "WebDownloader の受信バッファサイズに 0 以下のサイズは指定出来ません");
            }


            // 内部バッファを生成して初期化をする
            InitializeInstance(new byte[receiveBufferSize]);
        }


        /// <summary>
        /// WebDownloader のインスタンスを指定された外部バッファを用いて初期化をします
        /// </summary>
        /// <param name="outsideReceiveBuffer">WebDownloader に割り当てられた外部受信バッファ</param>
        /// <exception cref="ArgumentOutOfRangeException">WebDownloader の受信バッファサイズに 0 以下のサイズは指定出来ません</exception>
        public WebDownloader(byte[] outsideReceiveBuffer)
        {
            // もし null を渡されたら
            if (outsideReceiveBuffer == null)
            {
                // 流石にnullは受け付けられない
                throw new ArgumentNullException(nameof(outsideReceiveBuffer));
            }


            // もしバッファの長さが0なら
            if (outsideReceiveBuffer.Length == 0)
            {
                // 参照を受け取っても結果的にはバッファ無しはだめ
                throw new ArgumentOutOfRangeException(nameof(outsideReceiveBuffer), "WebDownloader の受信バッファサイズに 0 以下のサイズは指定出来ません");
            }


            // 外部バッファを使った初期化をする
            InitializeInstance(outsideReceiveBuffer);
        }


        /// <summary>
        /// 全コンストラクタから呼び出される共通初期化関数です
        /// </summary>
        /// <param name="receiveBuffer">受信バッファ</param>
        private void InitializeInstance(byte[] receiveBuffer)
        {
            // 初期化をする
            this.receiveBuffer = receiveBuffer;
            TimeoutTime = DefaultTimeoutTime;
            RetryCount = DefaultRetryCount;
            RetryWaitTime = DefaultRetryWaitTime;
            downloading = false;
        }
        #endregion


        #region パラメータ設定関数
        /// <summary>
        /// アクセスタイムアウトになるまでの時間（ミリ秒）を設定します
        /// </summary>
        /// <param name="timeoutTime">アクセスタイムアウトになるまでの時間（ミリ秒）</param>
        /// <exception cref="InvalidOperationException">ダウンロード操作中のため 'SetTimeoutTime' の操作は出来ません</exception>
        /// <exception cref="ArgumentOutOfRangeException">タイムアウトの時間に 0 以下の値が指定されました</exception>
        public void SetTimeoutTime(int timeoutTime)
        {
            // ダウンロード中なら例外を吐く
            ThrowIfDownloading("SetTimeoutTime");


            // もし 0 以下の値が指定されたら
            if (timeoutTime <= 0)
            {
                // 流石に猶予無しは無理
                throw new ArgumentOutOfRangeException(nameof(timeoutTime), "タイムアウトの時間に 0 以下の値が指定されました");
            }


            // 値を受け取る
            TimeoutTime = timeoutTime;
        }


        /// <summary>
        /// ダウンロード失敗時のリトライする最大回数を設定します
        /// </summary>
        /// <param name="retryCount">リトライする最大回数</param>
        /// <exception cref="InvalidOperationException">ダウンロード操作中のため 'SetRetryCount' の操作は出来ません</exception>
        /// <exception cref="ArgumentOutOfRangeException">リトライ回数に 0 未満の値が指定されました</exception>
        public void SetRetryCount(int retryCount)
        {
            // ダウンロード中なら例外を吐く
            ThrowIfDownloading("SetRetryCount");


            // もし 0 未満の値が指定されたら
            if (retryCount < 0)
            {
                // 0未満回数のリトライはどういうこっちゃ
                throw new ArgumentOutOfRangeException(nameof(retryCount), "リトライ回数に 0 未満の値が指定されました");
            }


            // 値を受け取る
            RetryCount = retryCount;
        }


        /// <summary>
        /// リトライする際に、次にリトライするまでの待機時間（ミリ秒）を設定します
        /// </summary>
        /// <param name="retryWaitTime">リトライするまでの待機時間（ミリ秒）</param>
        /// <exception cref="InvalidOperationException">ダウンロード操作中のため 'SetRetryWaitTime' の操作は出来ません</exception>
        /// <exception cref="ArgumentOutOfRangeException">リトライ待機時間に 0 以下の値が指定されました</exception>
        public void SetRetryWaitTime(int retryWaitTime)
        {
            // ダウンロード中なら例外を吐く
            ThrowIfDownloading("SetRetryWaitTime");


            // もし 0 以下の値が指定された場合は
            if (retryWaitTime <= 0)
            {
                // リトライするのに直ちにリトライするのはどうかと思う
                throw new ArgumentOutOfRangeException(nameof(retryWaitTime), "リトライ待機時間に 0 以下の値が指定されました");
            }


            // 値を受け取る
            RetryWaitTime = retryWaitTime;
        }
        #endregion


        #region ダウンロード制御関数
        /// <summary>
        /// ダウンロード操作を中止します
        /// </summary>
        public void Abort()
        {
            // キャンセルソーストークンがないなら
            if (cancellationTokenSource == null)
            {
                // 何もせず終了
                return;
            }


            // キャンセルを通知する
            cancellationTokenSource.Cancel();
        }


        /// <summary>
        /// 指定されたURLからデータを非同期でダウンロードします
        /// </summary>
        /// <param name="url">ダウンロードするリソースが存在するURL</param>
        /// <param name="outputStream">ダウンロードしたデータを出力する先のストリーム</param>
        /// <param name="progress">ダウンロード進捗通知を受けるプログレス。もし、進捗通知を受けない場合は null の指定が可能です。</param>
        /// <returns>ダウンロード操作中のタスクを返します</returns>
        /// <exception cref="ArgumentNullException">url が null です</exception>
        /// <exception cref="ArgumentNullException">outputStream が null です</exception>
        /// <exception cref="ArgumentException">ダウンロードデータの出力先ストリームが、書き込みをサポートしていません</exception>
        public async Task DownloadAsync(Uri url, Stream outputStream, IProgress<double> progress)
        {
            // URLにnullを渡されたら
            if (url == null)
            {
                // 何を落とせばよいというのだ
                throw new ArgumentNullException(nameof(url));
            }


            // 出力ストリームにnullを渡されたら
            if (outputStream == null)
            {
                // どこに出力すればよいというのだ
                throw new ArgumentNullException(nameof(outputStream));
            }


            // 出力ストリームが書き込みをサポートしていないなら
            if (!outputStream.CanWrite)
            {
                // 書き込みをサポートしていないストリームは受け付けない
                throw new ArgumentException("ダウンロードデータの出力先ストリームが、書き込みをサポートしていません", nameof(outputStream));
            }


            // ダウンロード開始状態にする
            SetDownloadFlagAndRaiseOnDownloadStart();


            // 例外発生時に最後の例外を覚える変数を宣言して、キャンセルソースとトークンを生成する
            var lastError = default(Exception);
            var cancellationToken = (cancellationTokenSource = new CancellationTokenSource()).Token;


            // リトライ回数分を含むダウンロードを行う数分ループ
            for (int i = 0; i <= RetryCount; ++i)
            {
                // 何かしらエラーを覚えているのなら
                if (lastError != null)
                {
                    // リトライしたということなので、イベントを呼び出しつつ、リトライする前に少し待機する
                    RaiseOnDownloadRetryWait();
                    await Task.Delay(RetryWaitTime);
                    RaiseOnDownloadRetry();
                }


                try
                {
                    // 実際のダウンロードを呼ぶ
                    await DownloadAsync(url, outputStream, progress, cancellationToken);
                }
                catch (OperationCanceledException canceledException)
                {
                    // 操作キャンセル例外ならば例外情報だけ覚えて、そのままループを抜ける
                    lastError = canceledException;
                    break;
                }
                catch (TimeoutException timeoutException)
                {
                    // 最後に発生した例外として覚える
                    lastError = timeoutException;
                }
                catch (WebException webException)
                {
                    // エラー原因を収集する
                    var httpResponse = (HttpWebResponse)webException.Response;
                    var statusCode = (int)httpResponse.StatusCode;
                    var isServerError = statusCode >= 500 && statusCode <= 599;


                    // エラーが発生した原因のステータスコードがサーバー側エラーなら
                    if (isServerError)
                    {
                        // 最後に発生した例外として覚える
                        lastError = webException;
                    }
                    else
                    {
                        // サーバー側原因でないのなら例外を覚えて直ちにループを抜ける
                        lastError = webException;
                        break;
                    }
                }
                catch (Exception exception)
                {
                    // 原因不明なら最後に発生した例外として覚えて、無理にリトライしないようにループから抜ける
                    lastError = exception;
                    break;
                }
            }


            // キャンセルソースを殺す
            cancellationTokenSource.Dispose();
            cancellationTokenSource = null;


            // もし最後に発生した例外が覚えられているのなら
            if (lastError != null)
            {
                // もしエラー内容がキャンセル例外なら
                if (lastError is OperationCanceledException)
                {
                    // ダウンロードの中断を通知して終了
                    ResetDownloadFlagAndRaiseOnDownloadCancel();
                    return;
                }


                // ダウンロードのエラーイベントを通知してキャプチャして投げる
                ResetDownloadFlagAndRaiseOnDownloadError(lastError);
                ExceptionDispatchInfo.Capture(lastError).Throw();
                return;
            }


            // ダウンロード終了状態にする
            ResetDownloadFlagAndRaiseOnDownloadFinish();
        }


        /// <summary>
        /// 実際のダウンロードを行います
        /// </summary>
        /// <param name="url">ダウンロードするリソースが存在するURL</param>
        /// <param name="outputStream">ダウンロードしたデータを出力する先のストリーム</param>
        /// <param name="progress">ダウンロード進捗通知を受けるプログレス。もし、進捗通知を受けない場合は null の指定が可能です。</param>
        /// <param name="cancellationToken">ダウンロード操作のキャンセルを確認するトークン</param>
        /// <returns>ダウンロード操作中のタスクを返します</returns>
        /// <exception cref="OperationCanceledException">操作がキャンセルされました</exception>
        /// <exception cref="TimeoutException">HTTP要求より先にタイムアウトしました</exception>
        private async Task DownloadAsync(Uri url, Stream outputStream, IProgress<double> progress, CancellationToken cancellationToken)
        {
            // この時点でキャンセルされているかどうかを見る
            cancellationToken.ThrowIfCancellationRequested();


            // ダウンロードを行うHttpWebRequestを生成する
            var httpRequest = WebRequest.CreateHttp(url);
            var responseTask = default(Task<WebResponse>);


            try
            {
                // タイムアウト用タスクを用意してWebレスポンスとどっちが先に終わるか待機して、もしタイムアウトが先に終了したら
                responseTask = httpRequest.GetResponseAsync();
                var timeoutTask = Task.Delay(TimeoutTime, cancellationToken);
                var firstFinishTask = await Task.WhenAny(responseTask, timeoutTask);
                if (firstFinishTask == timeoutTask)
                {
                    // リクエストを中断してタイムアウト例外を吐く
                    httpRequest.Abort();
                    throw new TimeoutException("HTTP要求より先にタイムアウトしました");
                }
            }
            catch (TaskCanceledException)
            {
                // キャンセルトークンによって待機タスクがキャンセルされたのならキャンセル例外をココで投げる
                cancellationToken.ThrowIfCancellationRequested();
            }


            // レスポンスを受け取って全体の長さを取得（長さの取得が出来なかったら 1 を設定して計算時に飽和させる）
            var httpResponse = (HttpWebResponse)responseTask.Result;
            var contentLength = httpResponse.ContentLength == -1 ? 1 : httpResponse.ContentLength;


            // ダウンロードストリームを取得
            using (var downloadStream = httpResponse.GetResponseStream())
            {
                // トータルの読み込み計上と読み込みサイズを覚える変数を宣言
                var totalReadSize = 0;
                var readSize = 0;


                // ダウンロード通知間隔確認用ストップウォッチを起動
                var notifyStopwatch = Stopwatch.StartNew();


                // 読み切るまでひたすらループする、また読み取りは非同期で読み取る
                while ((readSize = await downloadStream.ReadAsync(receiveBuffer, 0, receiveBuffer.Length)) > 0)
                {
                    // このタイミングでキャンセル通知を確認する
                    cancellationToken.ThrowIfCancellationRequested();


                    // 読み取ったサイズ分書き込んでトータル読み込みに加算（書き込みも非同期）
                    await outputStream.WriteAsync(receiveBuffer, 0, readSize);
                    totalReadSize += readSize;


                    // もしダウンロード通知経過時間が既定間隔を超えていたら
                    if (notifyStopwatch.ElapsedMilliseconds > DownloadProgressNotifyInterval)
                    {
                        // 進捗を求めて通知する
                        var progressSize = (double)totalReadSize / contentLength;
                        progress?.Report(progressSize);


                        // ストップウォッチを再起動
                        notifyStopwatch.Restart();
                    }
                }
            }
        }
        #endregion


        #region 共通ロジック関数
        /// <summary>
        /// ダウンロード中の場合、ダウンロード中である例外を送出します
        /// </summary>
        /// <param name="controlText">例外を送出する際に、付随する操作しようとした内容を出力する文字列</param>
        private void ThrowIfDownloading(string controlText)
        {
            // ダウンロード中なら
            if (downloading)
            {
                // ダウンロード中である例外を吐く
                throw new InvalidOperationException($"ダウンロード操作中のため '{(controlText == null ? "NULL" : controlText)}' の操作は出来ません");
            }
        }


        /// <summary>
        /// ダウンロード開始状態フラグをセットし、ダウンロードが開始されたイベントを起こします
        /// </summary>
        private void SetDownloadFlagAndRaiseOnDownloadStart()
        {
            // ダウンロード中であることを示して、イベントを起こす
            downloading = true;
            OnDownloadStart?.Invoke();
        }


        /// <summary>
        /// ダウンロード開始状態フラグをリセットし、ダウンロードが終了されたイベントを起こします
        /// </summary>
        private void ResetDownloadFlagAndRaiseOnDownloadFinish()
        {
            // ダウンロードは終わったことを示して、イベントを起こす
            downloading = false;
            OnDownloadFinish?.Invoke();
        }


        /// <summary>
        /// ダウンロード開始状態フラグをリセットし、ダウンロードが中断されたイベントを起こします
        /// </summary>
        private void ResetDownloadFlagAndRaiseOnDownloadCancel()
        {
            // ダウンロードは終わったことを示して、イベントを起こす
            downloading = false;
            OnDownloadCancel?.Invoke();
        }


        /// <summary>
        /// ダウンロード開始状態フラグをリセットし、ダウンロードにエラーが発生したイベントを起こします
        /// </summary>
        /// <param name="exception">エラーが発生した例外</param>
        private void ResetDownloadFlagAndRaiseOnDownloadError(Exception exception)
        {
            // ダウンロードは終わったことを示して、イベントを起こす
            downloading = false;
            OnDownloadError?.Invoke(exception);
        }


        /// <summary>
        /// リトライ前の待機イベントを起こします
        /// </summary>
        private void RaiseOnDownloadRetryWait()
        {
            // リトライ前イベントを起こす
            OnDownloadRetryWait?.Invoke();
        }


        /// <summary>
        /// リトライのイベントを起こします
        /// </summary>
        private void RaiseOnDownloadRetry()
        {
            // リトライイベントを起こす
            OnDownloadRetry?.Invoke();
        }
        #endregion
    }
}