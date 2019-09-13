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
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using IceMilkTea.Core;

namespace IceMilkTea.SubSystem
{
    /// <summary>
    /// ゲームアセットのデプロイを制御する抽象クラスです
    /// </summary>
    public class AssetDeployer
    {
        // メンバ変数定義
        private Dictionary<string, CatalogInfo> catalogInfoTable;
        private IAssetStorage assetStorage;



        /// <summary>
        /// AssetDeployer クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="assetStorage">アセットを貯蔵するストレージ</param>
        /// <exception cref="ArgumentNullException">assetStorage が null です</exception>
        public AssetDeployer(IAssetStorage assetStorage)
        {
            // 初期化をする
            this.assetStorage = assetStorage ?? throw new ArgumentNullException(nameof(assetStorage));
        }


        #region カタログ制御関数
        /// <summary>
        /// アセットデプロイを参照するカタログを設定します
        /// </summary>
        /// <param name="name">設定するカタログの名前</param>
        /// <param name="reader">カタログリーダー</param>
        /// <param name="remoteUri">外部からカタログをフェッチするときに参照するリモートURI</param>
        /// <exception cref="ArgumentException">無効なカタログ名です</exception>
        /// <exception cref="ArgumentNullException">reader が null です</exception>
        /// <exception cref="ArgumentNullException">remoteUri が null です</exception>
        public void SetCatalog(string name, ICatalogReader reader, Uri remoteUri)
        {
            // 例外判定を入れる
            ThrowExceptionIfInvalidCatalogName(name);


            // カタログ情報テーブルにそのまま設定する
            reader = reader ?? throw new ArgumentNullException(nameof(reader));
            remoteUri = remoteUri ?? throw new ArgumentNullException(nameof(remoteUri));
            catalogInfoTable[name] = new CatalogInfo(name, reader, remoteUri);
        }


        /// <summary>
        /// 指定されたカタログ名が含まれているかどうか確認をします
        /// </summary>
        /// <param name="name">確認するカタログ名</param>
        /// <returns>含まれている場合は true を、含まれていない場合は false を返します</returns>
        /// <exception cref="ArgumentException">無効なカタログ名です</exception>
        public bool ContainCatalog(string name)
        {
            // 例外判定を入れてからテーブルの存在確認関数の結果を返す
            ThrowExceptionIfInvalidCatalogName(name);
            return catalogInfoTable.ContainsKey(name);
        }


        /// <summary>
        /// 指定されたカタログ名のカタログを一時カタログとしてリモートから非同期でフェッチします。
        /// </summary>
        /// <param name="name">フェッチするカタログ名</param>
        /// <param name="progress">カタログのフェッチ状態の進捗通知を受け取る進捗オブジェクト</param>
        /// <param name="cancellationToken">キャンセル要求を監視するためのトークン</param>
        /// <returns>フェッチを正しく完了した場合は true を、フェッチに失敗した場合は false を返すタスクを返します</returns>
        /// <exception cref="ArgumentException">無効なカタログ名です</exception>
        /// <exception cref="OperationCanceledException">非同期操作がキャンセルされました</exception>
        public async Task<bool> FetchTempCatalogAsync(string name, IProgress<FetchCatalogReport> progress, CancellationToken cancellationToken)
        {
            // 例外判定を入れて monitor が null ならインスタンスを作っておく
            cancellationToken.ThrowIfCancellationRequested();
            ThrowExceptionIfInvalidCatalogName(name);
            progress = progress ?? NullProgress<FetchCatalogReport>.Null;


            // 指定されたカタログが存在しないなら
            if (!ContainCatalog(name))
            {
                // フェッチ自体出来ないことを返す
                return false;
            }


            // カタログ情報を取得してフェッチャと書き込みストリームを用意
            var catalogInfo = catalogInfoTable[name];
            var fetcher = CreateFetcher(catalogInfo.RemoteUri);
            var writeStream = assetStorage.OpenTempCatalogWrite(name);


            // フェッチャまたは書き込みストリームの準備に失敗していたら
            if (fetcher == null || writeStream == null)
            {
                // 失敗を返す
                return false;
            }


            // 書き込みストリームを監視可能ストリームに包む
            using (var outStream = new MonitorableStream(writeStream))
            {
                // フェッチ用進捗通知オブジェクトとレポートオブジェクトを生成
                var report = new FetchCatalogReport();
                var fetchProgress = new ThrottleableProgress<FetcherReport>(x =>
                {
                    // 転送レートを含む監視情報を更新する
                    report.Update(name, x.ContentLength, x.FetchedLength, outStream.WriteBitRate);
                    progress.Report(report);
                });


                // フェッチを非同期で実行する
                await fetcher.FetchAsync(outStream, fetchProgress, cancellationToken);
            }


            // 成功を返す
            return true;
        }
        #endregion


        #region アセット操作関数
        #endregion


        #region 生成関数
        /// <summary>
        /// リモートURIからデータをフェッチするフェッチャのインスタンスを生成します
        /// </summary>
        /// <param name="remoteUri">フェッチする元になるリモートURI</param>
        /// <returns>生成されたフェッチャのインスタンスを返しますが、生成出来なかった場合は null を返します。</returns>
        protected virtual IFetcher CreateFetcher(Uri remoteUri)
        {
            // HTTP、HTTPSスキームの場合
            var scheme = remoteUri.Scheme;
            if (scheme == Uri.UriSchemeHttp || scheme == Uri.UriSchemeHttps)
            {
                // HTTP向けフェッチャを生成して返す
                return new HttpFetcher(remoteUri);
            }
            else if (scheme == Uri.UriSchemeFile)
            {
                // FILEスキームの場合ならファイルフェッチャを生成して返す
                return new FileFetcher(new FileInfo(remoteUri.LocalPath));
            }


            // 非サポートのスキームならnullを返す
            return null;
        }
        #endregion


        #region 例外関数
        /// <summary>
        /// カタログ名が無効な名前だった場合に例外を送出します
        /// </summary>
        /// <param name="name">確認するカタログ名</param>
        /// <exception cref="ArgumentException">無効なカタログ名です</exception>
        private void ThrowExceptionIfInvalidCatalogName(string name)
        {
            // 無効な名前を渡されたら
            if (string.IsNullOrWhiteSpace(name))
            {
                // 無効な引数である例外を吐く
                throw new ArgumentException("無効なカタログ名です", nameof(name));
            }
        }
        #endregion



        #region 内部型定義
        /// <summary>
        /// カタログを扱う情報を保持した構造体です
        /// </summary>
        private struct CatalogInfo
        {
            /// <summary>
            /// カタログ名
            /// </summary>
            public string Name { get; private set; }


            /// <summary>
            /// カタログをフェッチする際に参照するURI
            /// </summary>
            public Uri RemoteUri { get; private set; }


            /// <summary>
            /// カタログを読み込むリーダー
            /// </summary>
            public ICatalogReader Reader { get; private set; }



            /// <summary>
            /// CatalogInfo 構造体のインスタンスを初期化します
            /// </summary>
            /// <param name="name">カタログ名</param>
            /// <param name="reader">カタログリーダー</param>
            /// <param name="remoteUri"></param>
            public CatalogInfo(string name, ICatalogReader reader, Uri remoteUri)
            {
                // 初期化をする
                Name = name;
                Reader = reader;
                RemoteUri = remoteUri;
            }
        }
        #endregion
    }



    /// <summary>
    /// カタログのフェッチ状態のレポートクラスです
    /// </summary>
    public class FetchCatalogReport
    {
        /// <summary>
        /// フェッチしているカタログ名
        /// </summary>
        public string CatalogName { get; private set; }


        /// <summary>
        /// フェッチするカタログの全体の長さ
        /// </summary>
        public long ContentLength { get; private set; }


        /// <summary>
        /// フェッチした長さ
        /// </summary>
        public long FetchedLength { get; private set; }


        /// <summary>
        /// フェッチの転送ビットレート（bps）
        /// </summary>
        public long BitRate { get; private set; }


        /// <summary>
        /// 現在の進捗割合
        /// </summary>
        public double Progress => FetchedLength == ContentLength ? 0.0 : FetchedLength / (double)ContentLength;



        /// <summary>
        /// FetchCatalogMonitor クラスのインスタンスを初期化します
        /// </summary>
        public FetchCatalogReport()
        {
            // 更新処理を呼んでおく
            Update(null, 0, 0, 0);
        }


        /// <summary>
        /// 受け取ったパラメータを有効な範囲になるように更新します
        /// </summary>
        /// <param name="catalogName">フェッチ中のカタログ名 null の場合は、空文字列になります。</param>
        /// <param name="contentLength">フェッチされるコンテンツの長さ、もし fetchedLength より小さい場合は fetchedLength と同値になります。</param>
        /// <param name="fetchedLength">フェッチされた長さ、負の値になった場合は 0 になります</param>
        /// <param name="bitRate">フェッチの転送ビットレート、負の値になった場合は 0 になります</param>
        public void Update(string catalogName, long contentLength, long fetchedLength, long bitRate)
        {
            // 全パラメータを有効な状態で設定する
            CatalogName = catalogName ?? string.Empty;
            FetchedLength = Math.Max(fetchedLength, 0);
            ContentLength = Math.Max(contentLength, FetchedLength);
            BitRate = Math.Max(bitRate, 0);
        }
    }
}