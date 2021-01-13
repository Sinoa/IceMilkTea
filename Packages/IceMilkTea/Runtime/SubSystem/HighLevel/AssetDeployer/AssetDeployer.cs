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
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using IceMilkTea.Core;

namespace IceMilkTea.SubSystem
{
    /// <summary>
    /// ゲームアセットのデプロイを制御するクラスです
    /// </summary>
    /// <typeparam name="TStorage">この制御クラスが使用するストレージクラスの型</typeparam>
    public class AssetDeployer<TStorage> where TStorage : class, IAssetStorage
    {
        // メンバ変数定義
        private Dictionary<string, CatalogInfo> catalogInfoTable;



        /// <summary>
        /// この制御クラスが使用しているストレージ
        /// </summary>
        public TStorage AssetStorage { get; private set; }


        /// <summary>
        /// この制御クラスが使用するアセットデータベース
        /// </summary>
        public ImtAssetDatabase AssetDatabase { get; private set; }



        /// <summary>
        /// AssetDeployer クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="assetStorage">アセットを貯蔵するストレージ</param>
        /// <exception cref="ArgumentNullException">assetStorage が null です</exception>
        public AssetDeployer(TStorage assetStorage)
        {
            // 初期化をする
            AssetStorage = assetStorage ?? throw new ArgumentNullException(nameof(assetStorage));
            catalogInfoTable = new Dictionary<string, CatalogInfo>();
            AssetDatabase = new ImtAssetDatabase(assetStorage);
        }


        #region カタログ制御関数
        /// <summary>
        /// カタログを取り扱う際のカタログ情報を設定します
        /// </summary>
        /// <param name="info">設定するカタログ情報</param>
        /// <exception cref="ArgumentException">無効なカタログ名です</exception>
        /// <exception cref="ArgumentException">カタログ情報に取り扱えない情報が設定されています。</exception>
        public void SetCatalogInfo(CatalogInfo info)
        {
            // 例外判定を入れる
            ThrowExceptionIfInvalidCatalogName(info.Name);


            // カタログが取り扱えないフィールドを持ってしまっている場合は
            if (info.Reader == null || info.RemoteUri == null)
            {
                // 設定は出来ない例外を吐く
                throw new ArgumentException("カタログ情報に取り扱えない情報が設定されています。", nameof(info));
            }


            // カタログ情報をそのまま受け取る
            catalogInfoTable[info.Name] = info;
        }


        /// <summary>
        /// 指定された名前のカタログ情報の取得に試みます
        /// </summary>
        /// <param name="name">取得するカタログ情報の名前</param>
        /// <param name="info">取得されたカタログ情報を受取る参照</param>
        /// <returns>取得に成功した場合は true を、失敗した場合は false を返します</returns>
        public bool TryGetCatalogInfo(string name, out CatalogInfo info)
        {
            // テーブルのTryGet関数をそのまま使う
            return catalogInfoTable.TryGetValue(name, out info);
        }


        /// <summary>
        /// 指定されたカタログ名が含まれているかどうか確認をします
        /// </summary>
        /// <param name="name">確認するカタログ名</param>
        /// <returns>含まれている場合は true を、含まれていない場合は false を返します</returns>
        public bool ContainCatalog(string name)
        {
            // テーブルの存在確認関数の結果を返す
            return catalogInfoTable.ContainsKey(name);
        }


        /// <summary>
        /// 指定されたカタログ名のカタログをリモートから非同期でフェッチします。
        /// </summary>
        /// <param name="name">フェッチするカタログ名</param>
        /// <param name="progress">カタログのフェッチ状態の進捗通知を受け取る進捗オブジェクト</param>
        /// <param name="cancellationToken">キャンセル要求を監視するためのトークン</param>
        /// <returns>フェッチを正しく完了した場合は true を、フェッチに失敗した場合は false を返すタスクを返します</returns>
        /// <exception cref="ArgumentNullException">progress が null です</exception>
        /// <exception cref="OperationCanceledException">非同期操作がキャンセルされました</exception>
        public async Task<bool> FetchCatalogAsync(string name, IProgress<FetchReport> progress, CancellationToken cancellationToken)
        {
            // キャンセル判定と例外判定を入れる
            cancellationToken.ThrowIfCancellationRequested();
            ThrowExceptionIfProgressIsNull(progress);


            // 指定されたカタログ情報が存在しないなら
            if (!ContainCatalog(name))
            {
                // フェッチ自体出来ないことを返す
                return false;
            }


            // カタログ情報を取得してフェッチャと書き込みストリームを用意
            TryGetCatalogInfo(name, out var catalogInfo);
            var fetcher = CreateFetcher(catalogInfo.RemoteUri);

            // Catalogのディスク書き込みは、Catalog内Itemの更新完了後とする
            var writeStream = AssetStorage.OpenCatalog(name, AssetStorageAccess.Write);


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
                var report = new FetchReport();
                var fetchProgress = new ThrottleableProgress<FetcherReport>(x =>
                {
                    // 転送レートを含む監視情報を更新する
                    report.Update(name, null, x.ContentLength, x.FetchedLength, outStream.WriteBitRate, 0, 1);
                    progress.Report(report);
                });


                try
                {
                    // フェッチを非同期で実行する
                    await fetcher.FetchAsync(outStream, fetchProgress, cancellationToken);
                }
                catch (Exception error)
                {
                    throw new Exception($"カタログ '{catalogInfo.RemoteUri}' のフェッチに失敗しました", error);
                }
            }

            // 成功を返す
            return true;
        }


        /// <summary>
        /// すべてのカタログをリモートから非同期でフェッチします
        /// </summary>
        /// <param name="progress">カタログのフェッチ状態の進捗通知を受け取る進捗オブジェクト</param>
        /// <param name="cancellationToken">キャンセル要求を監視するためのトークン</param>
        /// <returns>フェッチを正しく完了した場合は true を、フェッチに失敗した場合は false を返すタスクを返します</returns>
        /// <exception cref="ArgumentNullException">progress が null です</exception>
        /// <exception cref="OperationCanceledException">非同期操作がキャンセルされました</exception>
        public async Task<bool> FetchCatalogAsync(IProgress<FetchReport> progress, CancellationToken cancellationToken)
        {
            // カタログ情報テーブルのレコード数分回る
            foreach (var name in catalogInfoTable.Keys)
            {
                //Cancelリクエストされてるか見る
                if (cancellationToken.IsCancellationRequested)
                {
                    return false;
                }

                // 単体のフェッチ関数を叩いてもし失敗を返されたら
                var result = await FetchCatalogAsync(name, progress, cancellationToken);
                if (result == false)
                {
                    // この時点で失敗を返す
                    return false;
                }
            }


            // 成功を返す
            return true;
        }
        #endregion


        #region アセット操作関数
        /// <summary>
        /// すべての更新可能なアセットを非同期で確認します
        /// </summary>
        /// <param name="progress">差分チェック中の進捗通知オブジェクト</param>
        /// <param name="differenceList">チェックした結果を受け取るアセット差分リスト</param>
        /// <param name="cancellationToken">キャンセル要求を監視するためのトークン</param>
        /// <returns>確認中のタスクを返します</returns>
        /// <exception cref="ArgumentNullException">progress が null です</exception>
        /// <exception cref="ArgumentNullException">differenceList が null です</exception>
        /// <exception cref="OperationCanceledException">非同期操作がキャンセルされました</exception>
        /// <exception cref="TaskCanceledException">非同期操作がキャンセルされました</exception>
        public async Task CheckUpdatableAssetAsync(IProgress<string> progress, IList<AssetDifference> differenceList, CancellationToken cancellationToken)
        {
            // 通知オブジェクトがnullなら
            if (progress == null)
            {
                // どうやって通知すればよいのだろうか
                throw new ArgumentNullException(nameof(progress));
            }


            // カタログ情報のカタログ分回る
            foreach (var catalogName in catalogInfoTable.Keys)
            {
                // 名前入り差分チェック関数を叩く
                progress.Report(catalogName);
                await CheckUpdatableAssetAsync(catalogName, differenceList, cancellationToken);
            }
        }


        /// <summary>
        /// 指定されたカタログの更新可能なアセットを確認します
        /// </summary>
        /// <param name="catalogName">確認するカタログ名</param>
        /// <param name="differenceList">チェックした結果を受け取るアセット差分リスト</param>
        /// <param name="cancellationToken">キャンセル要求を監視するためのトークン</param>
        /// <returns>確認中のタスクを返します</returns>
        /// <exception cref="ArgumentException">無効なカタログ名です</exception>
        /// <exception cref="ArgumentNullException">differenceList が null です</exception>
        /// <exception cref="OperationCanceledException">非同期操作がキャンセルされました</exception>
        /// <exception cref="TaskCanceledException">非同期操作がキャンセルされました</exception>
        public async Task<ICatalog> CheckUpdatableAssetAsync(string catalogName, IList<AssetDifference> differenceList, CancellationToken cancellationToken)
        {
            // 例外判定を入れる
            cancellationToken.ThrowIfCancellationRequested();
            ThrowExceptionIfInvalidCatalogName(catalogName);
            ThrowExceptionIfDifferenceListIsNull(differenceList);

            // ストレージに指定されたカタログ名のデータが無いのなら
            if (!AssetStorage.ExistsCatalog(catalogName))
            {
                // 何もせず終了
                return null;
            }


            // カタログ情報がないなら
            if (!catalogInfoTable.ContainsKey(catalogName))
            {
                // お取り扱いが出来ない
                throw new ArgumentException(catalogName);
            }

            // カタログを読み込む
            var catalogInfo = catalogInfoTable[catalogName];
            var catalog = default(ICatalog);
            using (var stream = AssetStorage.OpenCatalog(catalogName, AssetStorageAccess.Read))
            {
                catalog = await catalogInfo.Reader.ReadCatalogAsync(stream);
            }

            // 差分判定を非同期で実行する
            await Task.Run(() => AssetDatabase.GetCatalogDifference(catalogName, catalog, differenceList), cancellationToken);
            return catalog;
        }


        /// <summary>
        /// すべてのカタログのアセット更新を非同期で実行します
        /// </summary>
        /// <param name="progress">アセットの更新進捗通知を受ける進捗オブジェクト</param>
        /// <param name="cancellationToken">キャンセル要求を監視するためのトークン</param>
        /// <returns>アセットの更新を実行しているタスクを返します</returns>
        /// <exception cref="ArgumentNullException">progress が null です</exception>
        /// <exception cref="OperationCanceledException">非同期操作がキャンセルされました</exception>
        /// <exception cref="TaskCanceledException">非同期操作がキャンセルされました</exception>
        public async Task UpdateAssetAsync(IProgress<FetchReport> progress, CancellationToken cancellationToken)
        {
            // 通知オブジェクトがnullなら
            if (progress == null)
            {
                // どうやって通知すればよいのだろうか
                throw new ArgumentNullException(nameof(progress));
            }


            // カタログ情報のカタログ分回る
            foreach (var catalogName in catalogInfoTable.Keys)
            {
                // 名前入りアセット更新を叩く
                await UpdateAssetAsync(catalogName, progress, cancellationToken);
            }

            await Task.Run(() => this.AssetDatabase.Save());
        }


        /// <summary>
        /// 指定されたカタログのアセット更新を非同期で実行します
        /// </summary>
        /// <param name="catalogName">更新を実行するカタログ名</param>
        /// <param name="progress">アセットの更新進捗通知を受ける進捗オブジェクト</param>
        /// <param name="cancellationToken">キャンセル要求を監視するためのトークン</param>
        /// <exception cref="OperationCanceledException">非同期操作がキャンセルされました</exception>
        /// <exception cref="TaskCanceledException">非同期操作がキャンセルされました</exception>
        /// <returns>アセットの更新を実行しているタスクを返します</returns>
        public async Task UpdateAssetAsync(string catalogName, IProgress<FetchReport> progress, CancellationToken cancellationToken)
        {
            // 例外判定を入れる
            cancellationToken.ThrowIfCancellationRequested();
            ThrowExceptionIfInvalidCatalogName(catalogName);
            ThrowExceptionIfProgressIsNull(progress);

            var differences = new List<AssetDifference>();
            var newCatalog = await CheckUpdatableAssetAsync(catalogName, differences, cancellationToken);

            //基本的には起きないはずなんだが…
            if (newCatalog is null)
                return;

            //まずは更新が必要な全数を出す
            var totalCount = differences.Count(x => x.Status == AssetDifferenceStatus.Update || x.Status == AssetDifferenceStatus.New);

            int downloadedCount = 0;
            foreach (var diff in differences)
            {
                if (diff.Status == AssetDifferenceStatus.Delete)
                {
                    this.AssetStorage.DeleteAsset(diff.Asset.LocalUri);
                }
                else if (diff.Status == AssetDifferenceStatus.Update || diff.Status == AssetDifferenceStatus.New)
                {
                    using (var stream = this.AssetStorage.OpenAsset(diff.Asset.LocalUri, AssetStorageAccess.Write))
                    using (var outStream = new MonitorableStream(stream))
                    {
                        // フェッチ用進捗通知オブジェクトとレポートオブジェクトを生成
                        var report = new FetchReport();
                        var fetchProgress = new ThrottleableProgress<FetcherReport>(x =>
                        {
                            // 転送レートを含む監視情報を更新する
                            report.Update(null, diff.Asset.Name, x.ContentLength, x.FetchedLength, outStream.WriteBitRate, downloadedCount, totalCount);
                            progress.Report(report);
                        });


                        try
                        {
                            var fetcher = CreateFetcher(diff.Asset.RemoteUri);
                            await fetcher.FetchAsync(stream, fetchProgress, cancellationToken);
                        }
                        catch (Exception error)
                        {
                            throw new Exception($"アセット '{diff.Asset.RemoteUri}' のフェッチに失敗しました。", error);
                        }
                    }
                    downloadedCount++;
                }
            }

            this.AssetDatabase.UpdateCatalog(catalogName, newCatalog);
        }
        #endregion


        #region 生成関数
        /// <summary>
        /// リモートURIからデータをフェッチするフェッチャのインスタンスを生成します
        /// </summary>
        /// <param name="remoteUri">フェッチする元になるリモートURI</param>
        /// <returns>生成されたフェッチャのインスタンスを返しますが、生成出来なかった場合は null を返します。</returns>
        protected virtual IDataFetcher CreateFetcher(Uri remoteUri)
        {
            // HTTP、HTTPSスキームの場合
            var scheme = remoteUri.Scheme;
            if (scheme == Uri.UriSchemeHttp || scheme == Uri.UriSchemeHttps)
            {
#if UNITY_5_3_OR_NEWER
                // UnityWebRequestフェッチャを生成して返す
                return new UnityWebRequestFetcher(remoteUri);
#else 
                // HTTP向けフェッチャを生成して返す
                return new HttpDataFetcher(remoteUri);
#endif
            }
            else if (scheme == Uri.UriSchemeFile)
            {
                // FILEスキームの場合ならファイルフェッチャを生成して返す
                return new FileDataFetcher(new FileInfo(remoteUri.LocalPath));
            }


            // 非サポートのスキームならnullを返す
            return null;
        }


        /// <summary>
        /// 指定された名前のハッシュアルゴリズムを生成します。
        /// 通常は HashAlgorithm.Create(string) を使用します。
        /// </summary>
        /// <param name="hashName">生成するハッシュアルゴリズムの名前</param>
        /// <returns>ハッシュアルゴリズムが生成された場合はインスタンスを、生成出来なかった場合は null を返します</returns>
        protected virtual HashAlgorithm CreateHash(string hashName)
        {
            // 素直に標準的な関数を用いる
            return HashAlgorithm.Create(hashName);
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


        /// <summary>
        /// アセット差分リストの参照が null の場合に例外を送出します
        /// </summary>
        /// <param name="differenceList">確認する参照</param>
        /// <exception cref="ArgumentNullException">differenceList が null です</exception>
        private void ThrowExceptionIfDifferenceListIsNull(IList<AssetDifference> differenceList)
        {
            // もし null なら
            if (differenceList == null)
            {
                // 例外を吐く
                throw new ArgumentNullException(nameof(differenceList));
            }
        }


        /// <summary>
        /// プログレスがnullの場合に例外を送出します
        /// </summary>
        /// <typeparam name="T">Progress が進捗通知をするオブジェクトの型</typeparam>
        /// <param name="progress">確認する Progress</param>
        /// <exception cref="ArgumentNullException">progress が null です</exception>
        private void ThrowExceptionIfProgressIsNull<T>(IProgress<T> progress)
        {
            // もし null を渡されたのなら
            if (progress == null)
            {
                // 例外を吐く
                throw new ArgumentNullException(nameof(progress));
            }
        }
        #endregion


        #region AssetDatabase

        public Task LoadAssetDatabase()
        {
            return Task.Run(() => this.AssetDatabase.Load());
        }
        #endregion
    }



    #region 情報
    /// <summary>
    /// カタログを扱う情報を保持したクラスです
    /// </summary>
    public readonly struct CatalogInfo
    {
        /// <summary>
        /// カタログ名
        /// </summary>
        public string Name { get; }


        /// <summary>
        /// カタログをフェッチする際に参照するURI
        /// </summary>
        public Uri RemoteUri { get; }


        /// <summary>
        /// カタログを読み込むリーダー
        /// </summary>
        public ICatalogReader Reader { get; }

        /// <summary>
        /// CatalogInfo 構造体のインスタンスを初期化します
        /// </summary>
        /// <param name="name">カタログ名</param>
        /// <param name="reader">カタログリーダー</param>
        /// <param name="remoteUri">カタログをフェッチする参照リモートURI</param>
        public CatalogInfo(string name, ICatalogReader reader, Uri remoteUri)
        {
            // 初期化をする
            Name = name;
            Reader = reader;
            RemoteUri = remoteUri;
        }
    }
    #endregion



    #region レポート関連型
    /// <summary>
    /// フェッチ状態のレポートクラスです
    /// </summary>
    public class FetchReport
    {
        /// <summary>
        /// フェッチしているカタログ名
        /// </summary>
        public string CatalogName { get; private set; }


        /// <summary>
        /// フェッチしているアセット名
        /// </summary>
        public string AssetName { get; private set; }


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
        public double Progress => FetchedLength == ContentLength ? 1.0 : FetchedLength / (double)ContentLength;


        /// <summary>
        /// ダウンロードしたファイル数
        /// </summary>
        public int DownloadedCount { get; private set; }


        /// <summary>
        /// ダウンロードする全ファイル数
        /// </summary>
        public int TotalCount { get; private set; }

        /// <summary>
        /// FetchCatalogMonitor クラスのインスタンスを初期化します
        /// </summary>
        public FetchReport()
        {
            // 更新処理を呼んでおく
            Update(null, null, 0, 0, 0, 0, 0);
        }


        /// <summary>
        /// 受け取ったパラメータを有効な範囲になるように更新します
        /// </summary>
        /// <param name="catalogName">フェッチ中のカタログ名 null の場合は、空文字列になります。</param>
        /// <param name="assetName">フェッチ中のアセット名 null の場合は、空文字列になります。</param>
        /// <param name="contentLength">フェッチされるコンテンツの長さ、もし fetchedLength より小さい場合は fetchedLength と同値になります。</param>
        /// <param name="fetchedLength">フェッチされた長さ、負の値になった場合は 0 になります</param>
        /// <param name="bitRate">フェッチの転送ビットレート、負の値になった場合は 0 になります</param>
        public void Update(string catalogName, string assetName, long contentLength, long fetchedLength, long bitRate, int downloadedCount, int totalCount)
        {
            // 全パラメータを有効な状態で設定する
            CatalogName = catalogName ?? string.Empty;
            AssetName = assetName;
            FetchedLength = Math.Max(fetchedLength, 0);
            ContentLength = Math.Max(contentLength, FetchedLength);
            BitRate = Math.Max(bitRate, 0);
            DownloadedCount = downloadedCount;
            TotalCount = totalCount;
        }
    }
    #endregion
}