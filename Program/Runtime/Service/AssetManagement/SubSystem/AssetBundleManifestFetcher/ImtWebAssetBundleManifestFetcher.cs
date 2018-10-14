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
using System.Net;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using IceMilkTea.Core;
using UnityEngine;

namespace IceMilkTea.Service
{
    /// <summary>
    /// Webからマニフェストをフェッチする、単純なフェッチャークラスです
    /// </summary>
    public class ImtWebAssetBundleManifestFetcher : AssetBundleManifestFetcher
    {
        // 公開定数定義
        public const int DefaultTimeoutTime = 5000;
        public const int DefaultRetryCount = 2;

        // 非公開定数定義
        private const int ReceiveBufferSize = 1 << 10;
        private const int InitialRetryWaitTime = 500;

        // メンバ変数定義
        private Uri manifestUrl;
        private int timeoutTime;
        private int retryCount;
        private byte[] receiveBuffer;



        /// <summary>
        /// ImtWebAssetBundleManifestFetcher のインスタンスを既定の値で初期化します
        /// </summary>
        /// <param name="manifestUrl">フェッチするマニフェストが存在するWebサービスのベースURL</param>
        /// <exception cref="ArgumentNullException">manifestUrl が null です</exception>
        /// <exception cref="ArgumentException">スキームがHTTPではありません、扱えるスキームは http または https です</exception>
        public ImtWebAssetBundleManifestFetcher(Uri manifestUrl) : this(manifestUrl, DefaultTimeoutTime, DefaultRetryCount)
        {
        }


        /// <summary>
        /// ImtWebAssetBundleManifestFetcher のインスタンスを既定の値で初期化します
        /// </summary>
        /// <param name="manifestUrl">フェッチするマニフェストが存在するWebサービスのベースURL</param>
        /// <param name="timeoutTime">タイムアウトするまでの時間（ミリ秒）</param>
        /// <param name="retryCount">最大リトライ回数</param>
        /// <exception cref="ArgumentNullException">manifestUrl が null です</exception>
        /// <exception cref="ArgumentException">スキームがHTTPではありません、扱えるスキームは http または https です</exception>
        /// <exception cref="ArgumentOutOfRangeException">timeoutTime は 0 以下の値を設定することが出来ません</exception>
        /// <exception cref="ArgumentOutOfRangeException">retryCount は 0 未満の値を設定することが出来ません</exception>
        public ImtWebAssetBundleManifestFetcher(Uri manifestUrl, int timeoutTime, int retryCount)
        {
            // nullが渡されたら
            if (manifestUrl == null)
            {
                // どこからダウンロードすればよいのか
                throw new ArgumentNullException(nameof(manifestUrl));
            }


            // スキームがHTTP系じゃないなら
            if (!manifestUrl.IsHttpScheme())
            {
                // http系のスキームを要求する例外を吐く
                throw new ArgumentException("スキームがHTTPではありません、扱えるスキームは http または https です", nameof(manifestUrl));
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
            this.manifestUrl = manifestUrl;
            this.timeoutTime = timeoutTime;
            this.retryCount = retryCount;


            // 受信バッファを作っておく
            receiveBuffer = new byte[ReceiveBufferSize];
        }


        /// <summary>
        /// マニフェストを非同期で取得します
        /// </summary>
        /// <returns>マニフェストの取得を非同期で行っているタスクを返します</returns>
        public override async Task<ImtAssetBundleManifest[]> FetchManifestAsync()
        {
            // 最後に発生した例外を保持する変数を宣言
            var exception = default(Exception);


            // リトライ回数以下の回数分回る
            for (int i = 0; i <= retryCount; ++i)
            {
                try
                {
                    // 実際のフェッチを行い関数から戻ってきたらそのまま結果を返す
                    return await DoFetchManifestAsync();
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
                        return Array.Empty<ImtAssetBundleManifest>();
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


            // ココまで来たのなら原則エラーなので長さ0の結果を返す
            return Array.Empty<ImtAssetBundleManifest>();
        }


        /// <summary>
        /// 実際のマニフェストを非同期で取得します
        /// </summary>
        /// <returns>マニフェストの取得を非同期で行っているタスクを返します</returns>
        private async Task<ImtAssetBundleManifest[]> DoFetchManifestAsync()
        {
            // ダウンロードする元のURLをつくってHttpWebRequestを生成する（念の為タイムスタンプクエリ的なものでキャッシュを防ぐ）
            var targetUrl = new Uri(manifestUrl.ToString() + $"?timestamp={DateTimeOffset.Now.ToUnixTimeMilliseconds()}");
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


            // ダウンロードストリームからストリームリーダのインスタンスを開く
            var httpResponse = (HttpWebResponse)responseTask.Result;
            using (var downloadStream = httpResponse.GetResponseStream())
            using (var streamReader = new StreamReader(downloadStream))
            {
                // jsonデータを非同期で読み込んで結果を返す（配列を要求されるので長さ１の配列として返す）
                var manifest = JsonUtility.FromJson<ImtAssetBundleManifest>(await streamReader.ReadToEndAsync());
                return new ImtAssetBundleManifest[] { manifest };
            }
        }
    }
}