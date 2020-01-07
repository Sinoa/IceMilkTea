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

namespace IceMilkTea.Core
{
    #region 進捗通知データ
    /// <summary>
    /// フェッチャの進捗通知レポートを持つ構造体です
    /// </summary>
    public readonly struct FetcherReport
    {
        /// <summary>
        /// フェッチするコンテンツの長さ
        /// </summary>
        public readonly long ContentLength;


        /// <summary>
        /// フェッチした長さ
        /// </summary>
        public readonly long FetchedLength;



        /// <summary>
        /// FetcherReport 構造体のインスタンスを初期化します
        /// </summary>
        /// <param name="contentLength">フェッチするコンテンツの長さ</param>
        /// <param name="fetchedLength">フェッチ済みの長さ</param>
        public FetcherReport(long contentLength, long fetchedLength)
        {
            // メンバの初期化
            ContentLength = contentLength;
            FetchedLength = fetchedLength;
        }
    }
    #endregion



    #region インターフェイス
    /// <summary>
    /// データのフェッチを行うインターフェイスです
    /// </summary>
    public interface IDataFetcher
    {
        /// <summary>
        /// フェッチするコンテンツの長さ
        /// </summary>
        long ContentLength { get; }


        /// <summary>
        /// フェッチした長さ
        /// </summary>
        long FetchedLength { get; }



        /// <summary>
        /// フェッチを非同期で行い対象のストリームに出力します
        /// </summary>
        /// <param name="outStream">出力先のストリーム</param>
        /// <param name="progress">フェッチャの進捗通知を受ける進捗オブジェクト</param>
        /// <param name="cancellationToken">キャンセル要求を監視するためのトークン</param>
        /// <returns>フェッチ処理を実行しているタスクを返します</returns>
        Task FetchAsync(Stream outStream, IProgress<FetcherReport> progress, CancellationToken cancellationToken);
    }
    #endregion



    #region HTTPフェッチャ
    /// <summary>
    /// HTTPを用いたフェッチャクラスです
    /// </summary>
    public class HttpDataFetcher : IDataFetcher
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
        /// HttpDataFetcher クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="remoteUri">ダウンロードするリモートURI</param>
        /// <exception cref="ArgumentNullException">remoteUri が null です</exception>
        public HttpDataFetcher(Uri remoteUri) : this(remoteUri, DefaultBufferSize, DefaultTimeOutInterval)
        {
        }


        /// <summary>
        /// HttpDataFetcher クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="remoteUri">ダウンロードするリモートURI</param>
        /// <param name="bufferSize">ダウンロードバッファサイズ。既定は DefaultBufferSize です。</param>
        /// <exception cref="ArgumentNullException">remoteUri が null です</exception>
        public HttpDataFetcher(Uri remoteUri, int bufferSize) : this(remoteUri, bufferSize, DefaultTimeOutInterval)
        {
        }


        /// <summary>
        /// HttpDataFetcher クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="remoteUri">ダウンロードするリモートURI</param>
        /// <param name="bufferSize">ダウンロードバッファサイズ。既定は DefaultBufferSize です。</param>
        /// <param name="timeoutInterval">レスポンスを受け取るまでのタイムアウト時間をミリ秒で指定します。無限に待ち続ける場合は -1 を指定します。既定は DefaultTimeOutInterval です</param>
        /// <exception cref="ArgumentNullException">remoteUri が null です</exception>
        public HttpDataFetcher(Uri remoteUri, int bufferSize, int timeoutInterval)
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
        public Task FetchAsync(Stream outStream, IProgress<FetcherReport> progress, CancellationToken cancellationToken)
        {
            return Task.Run(async () =>
            {
                // この時点でのキャンセルリクエストを判定してさらに出力先ストリームが無いなら
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


                // WebRequestのインスタンスを生成してからレスポンスタスクとタイムアウトタスクを生成して、先にタイムアウトタスクが完了してしまったのなら
                var request = WebRequest.CreateHttp(remoteUri);
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
                    // 状態初期化と受信バッファを生成
                    ContentLength = response.ContentLength;
                    FetchedLength = 0;
                    var buffer = new byte[bufferSize];


                    // 全てのストリームを読みきるまでループ
                    int readSize = 0;
                    while ((readSize = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false)) > 0)
                    {
                        // 読み取ったデータを出力先ストリームに書き込んで合計読み込みサイズと進捗を求めて進捗通知をする
                        await outStream.WriteAsync(buffer, 0, readSize, cancellationToken).ConfigureAwait(false);
                        FetchedLength += readSize;
                        progress.Report(new FetcherReport(ContentLength, FetchedLength));
                    }
                }
            });
        }
    }
    #endregion



    #region Fileフェッチャ
    /// <summary>
    /// ファイルシステムを用いたフェッチャクラスです
    /// </summary>
    public class FileDataFetcher : IDataFetcher
    {
        // メンバ変数定義
        private FileInfo assetFileInfo;



        /// <summary>
        /// フェッチするコンテンツの長さ
        /// </summary>
        public long ContentLength { get; private set; }


        /// <summary>
        /// フェッチした長さ
        /// </summary>
        public long FetchedLength { get; private set; }



        /// <summary>
        /// FileDataFetcher クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="assetFileInfo">コピー元となるファイル情報</param>
        /// <exception cref="ArgumentNullException">assetFileInfo が null です</exception>
        public FileDataFetcher(FileInfo assetFileInfo)
        {
            // ファイル情報を受け取る
            this.assetFileInfo = assetFileInfo ?? throw new ArgumentNullException(nameof(assetFileInfo));
        }


        /// <summary>
        /// フェッチを非同期で行い対象のストリームに出力します
        /// </summary>
        /// <param name="outStream">出力先のストリーム</param>
        /// <returns>フェッチ処理を実行しているタスクを返します</returns>
        /// <exception cref="OperationCanceledException">非同期の操作がキャンセルされました</exception>
        /// <exception cref="ArgumentNullException">outStream が null です</exception>
        public Task FetchAsync(Stream outStream)
        {
            // 通知も受け取らないしキャンセルもしない
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
        /// <exception cref="ArgumentNullException">outStream が null です</exception>
        /// <exception cref="FileNotFoundException">コピー元となるファイル '{assetFilePath}' が見つかりません</exception>
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
        /// <exception cref="ArgumentNullException">outStream が null です</exception>
        /// <exception cref="FileNotFoundException">コピー元となるファイル '{assetFilePath}' が見つかりません</exception>
        public async Task FetchAsync(Stream outStream, IProgress<FetcherReport> progress, CancellationToken cancellationToken)
        {
            // この時点でのキャンセルリクエストを判定してさらに出力先ストリームが無いなら
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


            // コピー元のファイルのフルパスを取得する
            var assetFilePath = assetFileInfo.FullName;


            // コピー元となるファイルが存在しないなら
            assetFileInfo.Refresh();
            if (!assetFileInfo.Exists)
            {
                // 例外を吐く
                throw new FileNotFoundException($"コピー元となるファイル '{assetFilePath}' が見つかりません", assetFilePath);
            }


            // ファイルを開く(キャッシュサイズが16KBなのはiOSに合わせているだけです)
            using (var fileStream = new FileStream(assetFilePath, FileMode.Open, FileAccess.Read, FileShare.None, 16 << 10, true))
            {
                // 必要な情報を用意
                ContentLength = fileStream.Length;
                FetchedLength = 0;
                int readSize = 0;
                var buffer = new byte[1 << 20];


                // 読み切るまでループ
                while ((readSize = await fileStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                {
                    // 出力先ストリームに書き込んで合計読み込みサイズに加算して進捗通知
                    await outStream.WriteAsync(buffer, 0, readSize);
                    FetchedLength += readSize;
                    progress.Report(new FetcherReport(ContentLength, FetchedLength));
                }
            }
        }
    }
    #endregion
}