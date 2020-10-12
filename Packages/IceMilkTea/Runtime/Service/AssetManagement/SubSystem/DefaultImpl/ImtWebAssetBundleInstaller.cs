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
using System.Threading.Tasks;
using IceMilkTea.Core;

namespace IceMilkTea.Service
{
    /// <summary>
    /// Webからアセットバンドルをインストールする、単純なインストーラクラスです
    /// </summary>
    public class ImtWebAssetBundleInstaller : AssetBundleInstaller
    {
        // 公開定数定義
        public const int DefaultTimeoutTime = 5000;
        public const int DefaultRetryCount = 2;

        // 非公開定数定義
        private const int ReceiveBufferSize = 128 << 10;
        private const int InitialRetryWaitTime = 500;
        private const int NotifyIntervalTime = 500;

        // 読み取り専用クラス変数宣言
        private static readonly Progress<double> EmptyProgress = new Progress<double>(_ => { });

        // メンバ変数定義
        private Uri baseUrl;
        private int timeoutTime;
        private int retryCount;
        private byte[] receiveBuffer;



        /// <summary>
        /// ImtWebAssetBundleInstaller のインスタンスを既定の値で初期化します
        /// </summary>
        /// <param name="baseUrl">インストールするアセットバンドルが存在するWebサービスのベースURL</param>
        /// <exception cref="ArgumentNullException">baseUrl が null です</exception>
        /// <exception cref="ArgumentException">スキームがHTTPではありません、扱えるスキームは http または https です</exception>
        public ImtWebAssetBundleInstaller(Uri baseUrl) : this(baseUrl, DefaultTimeoutTime, DefaultRetryCount)
        {
        }


        /// <summary>
        /// ImtWebAssetBundleInstaller のインスタンスを既定の値で初期化します
        /// </summary>
        /// <param name="baseUrl">インストールするアセットバンドルが存在するWebサービスのベースURL</param>
        /// <param name="timeoutTime">タイムアウトするまでの時間（ミリ秒）</param>
        /// <param name="retryCount">最大リトライ回数</param>
        /// <exception cref="ArgumentNullException">baseUrl が null です</exception>
        /// <exception cref="ArgumentException">スキームがHTTPではありません、扱えるスキームは http または https です</exception>
        /// <exception cref="ArgumentOutOfRangeException">timeoutTime は 0 以下の値を設定することが出来ません</exception>
        /// <exception cref="ArgumentOutOfRangeException">retryCount は 0 未満の値を設定することが出来ません</exception>
        public ImtWebAssetBundleInstaller(Uri baseUrl, int timeoutTime, int retryCount)
        {
            // nullが渡されたら
            if (baseUrl == null)
            {
                // どこからダウンロードすればよいのか
                throw new ArgumentNullException(nameof(baseUrl));
            }


            // スキームがHTTP系じゃないなら
            if (!baseUrl.IsHttpScheme())
            {
                // http系のスキームを要求する例外を吐く
                throw new ArgumentException("スキームがHTTPではありません、扱えるスキームは http または https です", nameof(baseUrl));
            }


            // タイムアウトの時間が0以下なら
            if (timeoutTime <= 0)
            {
                // 流石に 0 以下は無理
                throw new ArgumentOutOfRangeException(nameof(timeoutTime), $"{nameof(timeoutTime)} は 0 以下の値を設定することが出来ません");
            }


            // リトライカウントが0未満なら
            if (retryCount < 0)
            {
                // リトライしないならいけるが負の値は無理
                throw new ArgumentOutOfRangeException(nameof(retryCount), $"{nameof(retryCount)} は 0 未満の値を設定することが出来ません");
            }


            // 値を受け取って覚える
            this.baseUrl = baseUrl;
            this.timeoutTime = timeoutTime;
            this.retryCount = retryCount;


            // 受信バッファを作っておく
            receiveBuffer = new byte[ReceiveBufferSize];
        }


        /// <summary>
        /// 指定されたアセットバンドル情報のアセットバンドルを非同期でインストールします
        /// </summary>
        /// <param name="info">インストールするアセットバンドル情報</param>
        /// <param name="installStream">インストールする先のストリーム</param>
        /// <param name="progress">インストール進捗通知を受ける IProgress 。不要の場合は null も指定可能です。</param>
        /// <returns>アセットバンドルの非同期インストールしているタスクを返します</returns>
        /// <exception cref="ArgumentNullException">installStream が null です</exception>
        public override async Task InstallAsync(AssetBundleInfo info, Stream installStream, IProgress<double> progress)
        {
            // nullを渡されたら
            if (installStream == null)
            {
                // 流石にインストール先がnullなのは無理
                throw new ArgumentNullException(nameof(installStream));
            }


            // 最後に発生した例外を保持する変数を宣言
            var exception = default(Exception);


            // リトライ回数以下の回数分回る
            for (int i = 0; i <= retryCount; ++i)
            {
                try
                {
                    // 実際のダウンロードを行い関数から戻ってきたら、直ちにループから抜ける
                    await DoInstallAsync(info, installStream, progress ?? EmptyProgress);
                    exception = null;
                    break;
                }
                catch (TimeoutException timeoutException)
                {
                    // 最後に発生した例外として覚える
                    exception = timeoutException;
                }
                catch (WebException webException)
                {
                    // 発生した原因のステータスコードがサーバー側エラーなら
                    var httpResponse = (HttpWebResponse)webException.Response;
                    var statusCode = (int)httpResponse.StatusCode;
                    var isServerError = statusCode >= 500 && statusCode <= 599;
                    if (isServerError)
                    {
                        // 最後に発生した例外として覚える
                        exception = webException;
                    }
                    else
                    {
                        // サーバー側原因で無いのなら例外をキャプチャして再投げ
                        ExceptionDispatchInfo.Capture(webException).Throw();
                        return;
                    }
                }


                // リトライする前に少し待機する
                await Task.Delay(InitialRetryWaitTime * (i + 1));
                continue;
            }


            // 例外が残っていたら
            if (exception != null)
            {
                // キャプチャして例外を投げる
                ExceptionDispatchInfo.Capture(exception).Throw();
            }
        }


        /// <summary>
        /// 実際のダウンロードを行います
        /// </summary>
        /// <param name="info">インストールするアセットバンドル情報</param>
        /// <param name="installStream">インストールする先のストリーム</param>
        /// <param name="progress">インストール進捗通知を受ける IProgress</param>
        /// <returns>アセットバンドルの非同期インストールしているタスクを返します</returns>
        private async Task DoInstallAsync(AssetBundleInfo info, Stream installStream, IProgress<double> progress)
        {
            // ダウンロードする元のURLをつくってHttpWebRequestを生成する（念の為タイムスタンプクエリ的なものでキャッシュを防ぐ）
            var targetUrl = new Uri(baseUrl, info.RemotePath + $"?timestamp={DateTimeOffset.Now.ToUnixTimeMilliseconds()}");
            var httpRequest = WebRequest.CreateHttp(targetUrl);


            // タイムアウト用タスクを用意してWebレスポンスとどっちが先に終わるか待機して、もしタイムアウトが先に終了したら
            var responseTask = httpRequest.GetResponseAsync();
            var timeoutTask = Task.Delay(timeoutTime);
            var firstFinishTask = await Task.WhenAny(responseTask, timeoutTask);
            if (firstFinishTask == timeoutTask)
            {
                // リクエストを中断してタイムアウト例外を吐く
                httpRequest.Abort();
                throw new TimeoutException("HTTP要求より先にタイムアウトしました");
            }


            // レスポンスを受け取って全体の長さを取得（長さの取得が出来なかったら 1 を設定して計算時に飽和させる）
            var httpResponse = (HttpWebResponse)responseTask.Result;
            var contentLength = httpResponse.ContentLength == -1 ? 1 : httpResponse.ContentLength;
            using (var downloadStream = httpResponse.GetResponseStream())
            {
                // 通知判定経過時間計測用ストップウォッチを生成
                var notifyStopwatch = new Stopwatch();


                // 実際の読み書きをタスク化して終了するまで待つ
                await Task.Run(() =>
                {
                    // すべてを読み切るまでひたすらループする
                    var totalProcessSize = 0L;
                    for (int readSize = 0; (readSize = downloadStream.Read(receiveBuffer, 0, receiveBuffer.Length)) > 0;)
                    {
                        // 読み取ったサイズ書き込む
                        installStream.Write(receiveBuffer, 0, readSize);


                        // 処理したサイズに加算して全体の進捗を求める
                        totalProcessSize += readSize;
                        var progressSize = (double)totalProcessSize / contentLength;


                        // もし ストップウォッチが未起動 または 最後に通知した最後の経過時間が通知時間間隔を超えていたら
                        if (notifyStopwatch.IsRunning == false || notifyStopwatch.ElapsedMilliseconds > NotifyIntervalTime)
                        {
                            // 進捗率が1.0超過しないように飽和処理してから進捗を通知する
                            progressSize = Math.Min(progressSize, 1.0);
                            progress.Report(progressSize);


                            // ストップウォッチを再起動する
                            notifyStopwatch.Restart();
                        }
                    }
                });
            }
        }
    }
}