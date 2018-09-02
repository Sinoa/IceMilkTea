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
using System.Collections.Generic;
using System.IO;
using IceMilkTea.Core;
using IceMilkTea.Module;
using UnityEngine;

namespace IceMilkTea.Service
{
    // Unityのアセットの基底型になるObjectは、SystemのObjectとややこしくなるので
    // ここではUnityAssetと名付ける（WeakReference版も定義）
    using UnityAsset = UnityEngine.Object;
    using WeakUnityAsset = WeakReference<UnityEngine.Object>;



    /// <summary>
    /// アセットクリーンアップの度合いを表現します
    /// </summary>
    public enum AssetCleanupAggressiveLevel : int
    {
        /// <summary>
        /// キャッシュの消失チェックと、必要であればファイルクローズまでを行います。
        /// </summary>
        Low = 0,

        /// <summary>
        /// Unityに未参照アセットのアンロード要求を行ってから、Lowと同じ事をします。
        /// </summary>
        Normal = 1,

        /// <summary>
        /// GCを強制的に起動、Unityに未参照アセットのアンロード要求、キャッシュ消失チェックなど
        /// 最大限のアセットクリーンアップを行います。
        /// </summary>
        High = 2,
    }



    #region サービス本体
    /// <summary>
    /// Unityのゲームアセットを読み込む機能を提供するサービスクラスです
    /// </summary>
    public class AssetLoadService : GameService
    {
        // 定数定義
        public const string AssetScheme = "asset";

        // メンバ変数定義
        private AssetCacheStorage cacheStorage;
        private AssetLoaderProvider loaderProvider;
        private AssetCleaner assetCleaner;



        /// <summary>
        /// AssetLoadService のインスタンスを初期化します
        /// </summary>
        public AssetLoadService()
        {
            // 各種サブシステムの初期化
            cacheStorage = new AssetCacheStorage();
            loaderProvider = new AssetLoaderProvider();
            assetCleaner = new AssetCleaner(cacheStorage);
        }


        /// <summary>
        /// アセットローダリゾルバを登録します。
        /// 同じインスタンスのリゾルバは重複登録出来ません。
        /// </summary>
        /// <param name="resolver">登録するリゾルバ</param>
        public void RegisterResolver(AssetLoaderResolver resolver)
        {
            // ローダプロバイダにそのまま横流しする
            loaderProvider.AddResolver(resolver);
        }


        /// <summary>
        /// 指定されたアセットURLのアセットを非同期でロードします
        /// </summary>
        /// <typeparam name="TAssetType">ロードするアセットの型</typeparam>
        /// <param name="assetUrl">ロードするアセットのURL</param>
        /// <returns>アセットの非同期ロードを待機するインスタンスを返します</returns>
        /// <exception cref="ArgumentException">uriString（assetUrl） が null です</exception>
        /// <exception cref="UriFormatException">指定されたアセットURLのフォーマットが正しくありません</exception>
        /// <exception cref="InvalidOperationException">指定されたアセットURLからアセットをロードが出来ませんでした</exception>
        public IAwaitable<TAssetType> LoadAssetAsync<TAssetType>(string assetUrl) where TAssetType : UnityAsset
        {
            // 進捗通知を受けないものとしてLoadAssetAsyncを叩く
            return LoadAssetAsync<TAssetType>(assetUrl, null);
        }


        /// <summary>
        /// 指定されたアセットURLのアセットを非同期でロードします
        /// </summary>
        /// <typeparam name="TAssetType">ロードするアセットの型</typeparam>
        /// <param name="assetUrl">ロードするアセットのURL</param>
        /// <param name="progress">アセットのロード進捗通知を受ける IProgress</param>
        /// <returns>アセットの非同期ロードを待機するインスタンスを返します</returns>
        /// <exception cref="ArgumentException">assetUrl が null です</exception>
        /// <exception cref="ArgumentException">アセットロードサービスは asset スキーム以外のURLは処理を受け付けていません</exception>
        /// <exception cref="UriFormatException">指定されたアセットURLのフォーマットが正しくありません</exception>
        /// <exception cref="InvalidOperationException">指定されたアセットURLからアセットをロードが出来ませんでした</exception>
        public IAwaitable<TAssetType> LoadAssetAsync<TAssetType>(string assetUrl, IProgress<float> progress) where TAssetType : UnityAsset
        {
            // assetUrlがnullなら
            if (assetUrl == null)
            {
                // 何を読み込めばよいのですか
                throw new ArgumentNullException(nameof(assetUrl));
            }


            // URLからIDを作る
            var assetId = assetUrl.ToCrc64Code();


            // キャッシュストレージにキャッシュがあるか取得をして、取得出来たのなら
            var asset = cacheStorage.GetAssetCache<TAssetType>(assetId);
            if (asset != null)
            {
                // TODO : CompletedなAwaitableが欲しい
                // シグナル状態のManualReset用意してそのまま結果を突っ込んで返す
                var completedAwaitable = new ImtAwaitableManualReset<TAssetType>(true);
                completedAwaitable.PrepareResult(asset);
                return completedAwaitable;
            }


            // URIを生成する
            var url = new Uri(assetUrl);


            // もし asset スキームでは無いのなら
            if (url.Scheme != AssetScheme)
            {
                // assetスキーム以外は現在受け付けない
                throw new ArgumentException("アセットロードサービスは asset スキーム以外のURLは処理を受け付けていません");
            }


            // ローダプロバイダにローダを要求して誰一人と対応可能なローダがいなかったら
            var loader = loaderProvider.GetAssetLoader<TAssetType>(assetId, url);
            if (loader == null)
            {
                // ごめんなさい、ロード出来ません
                throw new InvalidOperationException("指定されたアセットURLからアセットをロードが出来ませんでした");
            }


            // ローダにロードを要求して、キャッシュ関数にも流す
            var loadAwaitable = loader.LoadAssetAsync<TAssetType>(assetId, url, progress);
            DoAssetCache(assetId, loader, loadAwaitable);


            // ローダが返した待機クラスのインスタンスを返す
            return loadAwaitable;
        }


        /// <summary>
        /// 不要になったアセットなどを意図的にクリーンアップを非同期的に実行します
        /// </summary>
        /// <param name="level">アセットクリーンアップの度合いを指定します。度合いが高いほどクリーンアップに時間がかかります。</param>
        /// <returns>クリーンアップを待機するインスタンスを返します</returns>
        public IAwaitable AssetCleanupAsync(AssetCleanupAggressiveLevel level)
        {
            // クリーナのクリーンアップ関数を叩く
            return assetCleaner.AssetCleanupAsync(level);
        }


        /// <summary>
        /// 指定されたアセットIDに対して、ロード待機オブジェクトからロード結果を取得して、キャッシュを行います
        /// </summary>
        /// <typeparam name="TAssetType">ロードしようとしているアセットの型</typeparam>
        /// <param name="assetId">キャッシュするアセットID</param>
        /// <param name="loader">該当アセットのロードを担当したアセットローダ</param>
        /// <param name="awaitable">ロード中の待機オブジェクト</param>
        private async void DoAssetCache<TAssetType>(ulong assetId, AssetLoader loader, IAwaitable<TAssetType> awaitable) where TAssetType : UnityAsset
        {
            // ロードの完了をまって、ロードに失敗しているようであれば
            var asset = await awaitable;
            if (asset == null)
            {
                // キャッシュはしないで終わる
                return;
            }


            // アセットのキャッシュをする
            cacheStorage.StoreAssetCache(assetId, asset, loader);
        }
    }
    #endregion



    #region キャッシュクラス
    /// <summary>
    /// 読み込まれたアセットのキャッシュを貯蔵するクラスです
    /// </summary>
    internal class AssetCacheStorage
    {
        /// <summary>
        /// アセットキャッシュの情報を保持した構造体です
        /// </summary>
        private struct AssetCacheInfo
        {
            /// <summary>
            /// キャッシュしているアセットID
            /// </summary>
            public ulong AssetId { get; private set; }


            /// <summary>
            /// キャッシュしているアセットへの弱参照プロパティ
            /// </summary>
            public WeakUnityAsset AssetReference { get; private set; }


            /// <summary>
            /// アセットのロードを担当したアセットローダ
            /// </summary>
            public AssetLoader AssetLoader { get; private set; }



            /// <summary>
            /// AssetCacheInfo のインスタンスを初期化します
            /// </summary>
            /// <param name="assetId">キャッシュするアセットID</param>
            /// <param name="asset">キャッシュするアセット</param>
            /// <param name="loader">キャッシュするアセットをロードしたローダ</param>
            public AssetCacheInfo(ulong assetId, UnityAsset asset, AssetLoader loader)
            {
                // 各フィールドを初期化する
                AssetId = assetId;
                AssetReference = new WeakUnityAsset(asset);
                AssetLoader = loader;
            }
        }



        // 定数定義
        private const int DefaultCacheEntryCapacity = 1 << 10;

        // メンバ変数定義
        private Dictionary<ulong, AssetCacheInfo> assetCacheTable;



        /// <summary>
        /// AssetCacheStorage のインスタンスを初期化します
        /// </summary>
        public AssetCacheStorage()
        {
            // キャッシュテーブルを生成する
            assetCacheTable = new Dictionary<ulong, AssetCacheInfo>(DefaultCacheEntryCapacity);
        }


        /// <summary>
        /// 指定されたアセットIDから、キャッシュ済みアセットの取得をします。
        /// </summary>
        /// <typeparam name="TAssetType">取得するアセットの型</typeparam>
        /// <param name="assetId">取得するキャッシュのアセットID</param>
        /// <returns>キャッシュ済みのアセットが存在する場合は、そのインスタンスを返しますが、キャッシュがない場合は null を返します</returns>
        public TAssetType GetAssetCache<TAssetType>(ulong assetId) where TAssetType : UnityAsset
        {
            // まずはアセットIDから参照を引っ張り出して存在しないなら
            AssetCacheInfo cacheInfo;
            if (!assetCacheTable.TryGetValue(assetId, out cacheInfo))
            {
                // まだキャッシュされていない
                return null;
            }


            // 参照から実体の取得が出来なかったら
            UnityAsset asset;
            if (!cacheInfo.AssetReference.TryGetTarget(out asset))
            {
                // ローダにアセットが消失したことを通知して
                // レコードを削除してキャッシュが取り出せなかったとして返す
                cacheInfo.AssetLoader.OnCacheLost(assetId);
                assetCacheTable.Remove(assetId);
                return null;
            }


            // 取り出した参照をキャストして返す
            return (TAssetType)asset;
        }


        /// <summary>
        /// 指定されたアセットIDとして、アセットを貯蔵します。
        /// もし既に貯蔵済みの場合は、以前のアセットの参照は破棄されます。
        /// </summary>
        /// <param name="assetId">貯蔵するアセットのアセットID</param>
        /// <param name="asset">貯蔵するアセット</param>
        /// <param name="loader">このアセットをロードしたローダ</param>
        /// <exception cref="ArgumentNullException">asset が null です</exception>
        public void StoreAssetCache(ulong assetId, UnityAsset asset, AssetLoader loader)
        {
            // もしassetがnullなら
            if (asset == null)
            {
                // 何を貯蔵するんですか
                throw new ArgumentNullException(nameof(asset));
            }


            // 一度テーブルから情報を引っ張り出せるか試みる
            AssetCacheInfo cacheInfo;
            if (assetCacheTable.TryGetValue(assetId, out cacheInfo))
            {
                // 取り出せたのなら参照を上書きして終了
                cacheInfo.AssetReference.SetTarget(asset);
                return;
            }


            // そもそもレコードすら無いのなら新規で追加する
            assetCacheTable[assetId] = new AssetCacheInfo(assetId, asset, loader);
        }


        /// <summary>
        /// 保持しているキャッシュエントリ情報をすべて確認し、未参照アセットが在った場合レコードから削除します。
        /// また、削除されるれコードがある場合アセットローダにキャッシュが消失した通知も行います。
        /// </summary>
        internal void DoCleanupUnreferencedAssets()
        {
            // キャッシュテーブルの値の数分ループして削除されるべきIDを列挙する
            var removeAssetIdList = new List<ulong>(assetCacheTable.Count);
            foreach (var cacheEntry in assetCacheTable.Values)
            {
                // アセットの参照が取り出せなかったら
                UnityAsset asset;
                if (!cacheEntry.AssetReference.TryGetTarget(out asset))
                {
                    // 削除されるべき対象としてIDを覚える
                    removeAssetIdList.Add(cacheEntry.AssetId);
                }
            }


            // キャッシュエントリ削除の対象分回る
            foreach (var removeAssetId in removeAssetIdList)
            {
                // 該当のレコードを取り出してキャッシュ消失通知を行い削除する
                var cacheEntry = assetCacheTable[removeAssetId];
                cacheEntry.AssetLoader.OnCacheLost(removeAssetId);
                assetCacheTable.Remove(removeAssetId);
            }
        }
    }
    #endregion



    #region アセットローダプロバイダ
    /// <summary>
    /// アセットローダーを提供するクラスです
    /// </summary>
    internal class AssetLoaderProvider
    {
        // メンバ変数定義
        private List<AssetLoaderResolver> resolverList;



        /// <summary>
        /// AssetLoaderProvider のインスタンスを初期化します
        /// </summary>
        public AssetLoaderProvider()
        {
            // リゾルバリストを生成する
            resolverList = new List<AssetLoaderResolver>();
        }


        /// <summary>
        /// 指定された、アセットローダリゾルバを追加します。
        /// ただし、既に追加済みの場合は何もしません。
        /// </summary>
        /// <param name="resolver">追加するリゾルバ</param>
        /// <exception cref="ArgumentNullException">resolver が null です</exception>
        public void AddResolver(AssetLoaderResolver resolver)
        {
            // nullが渡されたら
            if (resolver == null)
            {
                // 何を追加すれば良いんですか
                throw new ArgumentNullException(nameof(resolver));
            }


            // 既に指定されたリゾルバが存在するなら
            if (resolverList.Contains(resolver))
            {
                // 何もせず終了
                return;
            }


            // リゾルバの追加
            resolverList.Add(resolver);
        }


        /// <summary>
        /// 指定されたアセットIDとアセットURLから適切なアセットローダを取得します
        /// </summary>
        /// <typeparam name="TAssetType">取得するアセットの型</typeparam>
        /// <param name="assetId">これからロードする予定のアセットID</param>
        /// <param name="assetUrl">これからロードする予定のアセットURL</param>
        /// <returns>対応可能なアセットローダが存在した場合は、そのローダのインスタンスを返しますが、存在しない場合は null を返します</returns>
        public AssetLoader GetAssetLoader<TAssetType>(ulong assetId, Uri assetUrl) where TAssetType : UnityAsset
        {
            // 登録されているリゾルバ分回る
            foreach (var resolver in resolverList)
            {
                // アセットIDとURLを渡してローダを取得出来たのなら
                var loader = resolver.GetLoader<TAssetType>(assetId, assetUrl);
                if (loader != null)
                {
                    // このローダを返す
                    return loader;
                }
            }


            // ループから抜けてきたという事は誰も担当出来るローダがいなかったとして null を返す
            return null;
        }
    }
    #endregion



    #region アセットローダリゾルバとローダの抽象クラス
    /// <summary>
    /// アセットパスから適切なローダーを解決するリゾルバ抽象クラスです
    /// </summary>
    public abstract class AssetLoaderResolver
    {
        /// <summary>
        /// 指定されたアセットIDとアセットURLから、最適なアセットローダを取得します。
        /// </summary>
        /// <typeparam name="TAssetType">取得するアセットの型</typeparam>
        /// <param name="assetId">ロード要求のあるアセットID</param>
        /// <param name="assetUrl">ロード要求のあるアセットURL</param>
        /// <returns>最適なアセットローダがある場合は、そのローダのインスタンスを返しますが、存在しない場合は null を返します</returns>
        public abstract AssetLoader GetLoader<TAssetType>(ulong assetId, Uri assetUrl) where TAssetType : UnityAsset;
    }



    /// <summary>
    /// アセットのロードを実際に行うローダ抽象クラスです
    /// </summary>
    public abstract class AssetLoader
    {
        /// <summary>
        /// 指定されたアセットID、アセットURLからアセットを非同期に読み込みます。
        /// </summary>
        /// <typeparam name="TAssetType">取得するアセットの型</typeparam>
        /// <param name="assetId">読み込むアセットID</param>
        /// <param name="assetUrl">読み込むアセットURL</param>
        /// <param name="progress">読み込み状況の進捗通知を受ける IProgress</param>
        /// <returns>アセットの非同期ロードを待機する待機可能クラスのインスタンスを返します</returns>
        public abstract IAwaitable<TAssetType> LoadAssetAsync<TAssetType>(ulong assetId, Uri assetUrl, IProgress<float> progress) where TAssetType : UnityAsset;


        /// <summary>
        /// 該当アセットIDのキャッシュが消失した時のハンドリングを行います
        /// </summary>
        /// <param name="assetId">キャッシュが消失したアセットID</param>
        public virtual void OnCacheLost(ulong assetId)
        {
        }
    }
    #endregion



    #region Resolver&Loader Resources
    /// <summary>
    /// UnityのResourcesからアセットをロードするローダを解決するクラスです
    /// </summary>
    public class ResourcesAssetLoaderResolver : AssetLoaderResolver
    {
        // 定数定義
        private const string ResourcesHostName = "resources";

        // メンバ変数定義
        private ResourcesAssetLoader loader;



        /// <summary>
        /// ResourcesAssetLoaderResolver のインスタンスを初期化します
        /// </summary>
        public ResourcesAssetLoaderResolver()
        {
            // リクエストごとに異なるResourcesロードは無いのでこのタイミングでインスタンスを作っておく
            loader = new ResourcesAssetLoader();
        }


        /// <summary>
        /// 指定されたアセットIDとURLから必要なローダを取得します
        /// </summary>
        /// <typeparam name="TAssetType">取得するアセットの型</typeparam>
        /// <param name="assetId">ロード要求されているアセットID</param>
        /// <param name="assetUrl">ロード要求されているアセットURL</param>
        /// <returns>対応可能なアセットローダが存在した場合は、インスタンスを返しますが、見つからない場合は null を返します</returns>
        public override AssetLoader GetLoader<TAssetType>(ulong assetId, Uri assetUrl)
        {
            // ホスト名部分がResourcesローダ系の物でないなら
            if (assetUrl.Host != ResourcesHostName)
            {
                // 残念ながら対応出来ない
                return null;
            }


            // Resources系なら対応出来るのでローダを返す
            return loader;
        }
    }



    /// <summary>
    /// UnityのResourcesからアセットをロードするローダクラスです
    /// </summary>
    public class ResourcesAssetLoader : AssetLoader
    {
        /// <summary>
        /// 指定されたアセットIDと、アセットURLからアセットを非同期でロードします
        /// </summary>
        /// <typeparam name="TAssetType">読み込むアセットの型</typeparam>
        /// <param name="assetId">読み込むアセットID</param>
        /// <param name="assetUrl">読み込むアセットURL</param>
        /// <param name="progress">ロード進捗通知を受ける IProgress</param>
        /// <returns>待機可能なロードクラスのインスタンスを返します</returns>
        public override IAwaitable<TAssetType> LoadAssetAsync<TAssetType>(ulong assetId, Uri assetUrl, IProgress<float> progress)
        {
            // Resourcesから非同期でロードする待機可能クラスのインスタンスを返す（LocalPathの先頭はスラッシュが入っているので除去）
            return Resources.LoadAsync<TAssetType>(assetUrl.LocalPath.TrimStart('/')).ToAwaitable<TAssetType>(progress);
        }
    }
    #endregion



    #region Resolver&Loader AssetBundle
    /// <summary>
    /// Unityのファイル状になっているアセットバンドルからアセットをロードするローダを解決するクラスです
    /// </summary>
    public class FileAssetBundleAssetLoaderResolver : AssetLoaderResolver
    {
        // 定数定義
        private const string AssetBundleHostName = "assetbundle";

        // メンバ変数定義
        private Dictionary<ulong, AssetLoader> loaderTable;
        private string baseDirectoryPath;



        /// <summary>
        /// FileAssetBundleAssetLoaderResolver のインスタンスを初期化します
        /// </summary>
        /// <param name="assetBundleDirectoryBasePath">アセットバンドルが格納されているルートとなるディレクトリパス</param>
        public FileAssetBundleAssetLoaderResolver(string assetBundleDirectoryBasePath)
        {
            // 初期化する
            loaderTable = new Dictionary<ulong, AssetLoader>();
            baseDirectoryPath = assetBundleDirectoryBasePath;
        }


        /// <summary>
        /// 指定されたアセットIDとURLから必要なローダを取得します
        /// </summary>
        /// <typeparam name="TAssetType">取得するアセットの型</typeparam>
        /// <param name="assetId">ロード要求されているアセットID</param>
        /// <param name="assetUrl">ロード要求されているアセットURL</param>
        /// <returns>対応可能なアセットローダが存在した場合は、インスタンスを返しますが、見つからない場合は null を返します</returns>
        public override AssetLoader GetLoader<TAssetType>(ulong assetId, Uri assetUrl)
        {
            // ホスト名部分がAssetBundleローダ系の物でないなら
            if (assetUrl.Host != AssetBundleHostName)
            {
                // 残念ながら対応できない
                return null;
            }


            // 開くべきアセットバンドルのファイルパスを取得してアセットバンドルIDも作る
            var assetBundleFilePath = GetAssetBundleFilePath(assetUrl);
            var assetBundleId = assetBundleFilePath.ToCrc64Code();


            // もし、アセットバンドルファイルパスが空文字列として来たのなら
            if (string.IsNullOrWhiteSpace(assetBundleFilePath))
            {
                // 自分の担当かと思ったけど、そうではなかったようだ
                return null;
            }


            // 既に開いた経験のあるアセットバンドルなら
            AssetLoader loader;
            if (loaderTable.TryGetValue(assetBundleId, out loader))
            {
                // 開いたことのあるローダで返す
                return loader;
            }


            // 開いたことが無いなら新しくローダを生成して覚える
            loader = new FileAssetBundleAssetLoader(assetBundleFilePath);
            loaderTable[assetBundleId] = loader;


            // ローダを返す
            return loader;
        }


        /// <summary>
        /// アセットURLからアセットバンドルのファイルパスを取得します
        /// </summary>
        /// <param name="assetUrl">アセットバンドルのファイルパスを取り出すための、アセットURL</param>
        /// <returns>取得されたアセットバンドルファイルパスを返しますが、URLが正しくない場合は空文字列を返すことがあります</returns>
        private string GetAssetBundleFilePath(Uri assetUrl)
        {
            // セグメントの数が2以下なら
            if (assetUrl.Segments.Length <= 2)
            {
                // アセットバンドルのファイル名が指定されていない可能性があるのでから文字列を返して諦める
                return string.Empty;
            }


            // ローカルパスに含まれる最後のセグメントだけアセット名になるのでそれを削除して
            // 最後の前後に残るスラッシュを消してURLからパスを摘出後ベースパスとくっつけて返す
            var assetName = assetUrl.Segments[assetUrl.Segments.Length - 1];
            var cutPath = assetUrl.LocalPath.Remove(assetUrl.LocalPath.Length - assetName.Length, assetName.Length).Trim('/');
            return Path.Combine(baseDirectoryPath, cutPath).Replace('\\', '/');
        }
    }



    /// <summary>
    /// Unityのファイル状アセットバンドルからアセットをロードするローダクラスです
    /// </summary>
    public class FileAssetBundleAssetLoader : AssetLoader
    {
        // 定数定義
        private const string QueryBaseKeyName = "base";

        // メンバ変数定義
        private string assetBundlePath;
        private AssetBundle assetBundle;
        private Dictionary<string, string> uriQueryBufferTable;
        private List<ulong> loadedAssetIdList;



        /// <summary>
        /// FileAssetBundleAssetLoader のインスタンスを初期化します
        /// </summary>
        /// <param name="assetBundlePath">このローダが担当するアセットバンドルのファイルパス</param>
        public FileAssetBundleAssetLoader(string assetBundlePath)
        {
            // 初期化をする
            this.assetBundlePath = assetBundlePath;
            uriQueryBufferTable = new Dictionary<string, string>();
            loadedAssetIdList = new List<ulong>();
        }


        /// <summary>
        /// 指定されたアセットIDと、アセットURLからアセットを非同期でロードします
        /// </summary>
        /// <typeparam name="TAssetType">読み込むアセットの型</typeparam>
        /// <param name="assetId">読み込むアセットID</param>
        /// <param name="assetUrl">読み込むアセットURL</param>
        /// <param name="progress">ロード進捗通知を受ける IProgress</param>
        /// <returns>待機可能なロードクラスのインスタンスを返します</returns>
        public override IAwaitable<TAssetType> LoadAssetAsync<TAssetType>(ulong assetId, Uri assetUrl, IProgress<float> progress)
        {
            // ロード完了待機ハンドルを生成して実際のロードを非同期で行い、ひとまずハンドルをすぐに返す
            var waitHandle = new ImtAwaitableManualReset<TAssetType>(false);
            DoAssetLoadAsync(assetId, assetUrl, progress, waitHandle);
            return waitHandle;
        }


        /// <summary>
        /// 実際の非同期ロードを行います
        /// </summary>
        /// <typeparam name="TAssetType">読み込むアセットの型</typeparam>
        /// <param name="assetId">読み込むアセットID</param>
        /// <param name="assetUrl">読み込むアセットURL</param>
        /// <param name="progress">ロード進捗通知を受ける IProgress</param>
        private async void DoAssetLoadAsync<TAssetType>(ulong assetId, Uri assetUrl, IProgress<float> progress, ImtAwaitableManualReset<TAssetType> waitHandle) where TAssetType : UnityAsset
        {
            // もしまだアセットバンドルが開かれていなかったら
            if (assetBundle == null)
            {
                // アセットバンドルを非同期で開く
                assetBundle = await AssetBundle.LoadFromFileAsync(assetBundlePath).ToAwaitable(progress);
            }


            // アセットバンドルから該当のアセットを非同期にロードするがロード出来なかったら
            var asset = await assetBundle.LoadAssetAsync<TAssetType>(GetAssetName(assetUrl)).ToAwaitable<TAssetType>(progress);
            if (asset == null)
            {
                // 読み込めなかったということでnullで待機ハンドルにシグナルを送る
                waitHandle.Set(null);
                return;
            }


            // 読み込めたのならアセットIDを覚えて、待機ハンドルに結果付きでシグナルを送る
            loadedAssetIdList.Add(assetId);
            waitHandle.Set(asset);
        }


        /// <summary>
        /// アセットURLからアセット名を取得します
        /// </summary>
        /// <param name="assetUrl">アセット名を取り出すための、アセットURL</param>
        /// <returns>取得されたアセット名を返します</returns>
        private string GetAssetName(Uri assetUrl)
        {
            // URLの最後のセグメントがアセット名の一部になるので取得する
            var assetName = assetUrl.Segments[assetUrl.Segments.Length - 1];


            // まずはURLからクエリテーブルを取り出す
            uriQueryBufferTable.Clear();
            assetUrl.ToQueryDictionary(uriQueryBufferTable);


            // もし base キーのクエリがある場合は
            if (uriQueryBufferTable.ContainsKey(QueryBaseKeyName))
            {
                // 結合する
                assetName = Path.Combine(uriQueryBufferTable[QueryBaseKeyName], assetName).Replace('\\', '/');
            }


            // 最終的なアセット名を返す
            return assetName;
        }


        /// <summary>
        /// キャッシュストレージから、このローダが読み込んだアセットのキャッシュが消失した通知のハンドリングを行います
        /// </summary>
        /// <param name="assetId">消失したアセットのID</param>
        public override void OnCacheLost(ulong assetId)
        {
            // ロードしたアセットリストから該当のIDを削除
            loadedAssetIdList.Remove(assetId);


            // もし読み込んだアセットのリストがまだ空では無いなら
            if (loadedAssetIdList.Count > 0)
            {
                // まだ何もしない
                return;
            }


            // リストが空になったのなら、一度アセットバンドルは閉じる
            assetBundle.Unload(false);
            assetBundle = null;
        }
    }
    #endregion



    #region Resolver&Loader ImtArchive
    /// <summary>
    /// IceMilkTeaArchiveからアセットをロードするローダを解決するクラスです
    /// </summary>
    public class ImtArchiveLoaderResolver : AssetLoaderResolver
    {
        // 定数定義
        private const string ImtArchiveHostName = "imtarchive";

        // メンバ変数定義
        private Dictionary<ulong, AssetLoader> loaderTable;
        private string baseDirectoryPath;



        /// <summary>
        /// ImtArchiveLoaderResolver のインスタンスを初期化します
        /// </summary>
        /// <param name="archiveDirectoryPath">アーカイブが保存されるディレクトリパス</param>
        public ImtArchiveLoaderResolver(string archiveDirectoryPath)
        {
            // 初期化する
            loaderTable = new Dictionary<ulong, AssetLoader>();
            baseDirectoryPath = archiveDirectoryPath;
        }


        /// <summary>
        /// 指定されたアセットIDとURLから必要なローダを取得します
        /// </summary>
        /// <typeparam name="TAssetType">取得するアセットの型</typeparam>
        /// <param name="assetId">ロード要求されているアセットID</param>
        /// <param name="assetUrl">ロード要求されているアセットURL</param>
        /// <returns>対応可能なアセットローダが存在した場合は、インスタンスを返しますが、見つからない場合は null を返します</returns>
        public override AssetLoader GetLoader<TAssetType>(ulong assetId, Uri assetUrl)
        {
            // ホスト名部分がImtArchiveローダ系の物でないなら
            if (assetUrl.Host != ImtArchiveHostName)
            {
                // 残念ながら対応出来ない
                return null;
            }


            // 開くべきアーカイブファイルパスを取得してアーカイブIDも作る
            var archivePath = GetArchiveFilePath(assetUrl);
            var archiveId = archivePath.ToCrc64Code();


            // もし、アーカイブパスが空文字列として来たのなら
            if (string.IsNullOrWhiteSpace(archivePath))
            {
                // 自分の担当かと思ったけど、そうではなかったようだ
                return null;
            }


            // 既に開いた経験のあるローダなら
            AssetLoader loader;
            if (loaderTable.TryGetValue(archiveId, out loader))
            {
                // 開いたことのあるローダで返す
                return loader;
            }


            // 開いたことが無いなら新しくローダを生成して覚える
            loader = new ImtArchiveAssetLoader(archivePath);
            loaderTable[archiveId] = loader;


            // ローダを返す
            return loader;
        }


        /// <summary>
        /// アセットURLからアーカイブのファイルパスを取得します
        /// </summary>
        /// <param name="assetUrl">アーカイブのファイルパスを取り出すための、アセットURL</param>
        /// <returns>取得されたアーカイブパスを返しますが、URLが正しくない場合は空文字列を返すことがあります</returns>
        private string GetArchiveFilePath(Uri assetUrl)
        {
            // セグメントの数が2以下なら
            if (assetUrl.Segments.Length <= 2)
            {
                // ファイル名が指定されていない可能性があるのでから文字列を返して諦める
                return string.Empty;
            }


            // ローカルパスに含まれる最後のセグメントだけアセット名になるのでそれを削除して
            // 最後の前後に残るスラッシュを消してURLからパスを摘出後ベースパスとくっつけて返す
            var assetName = assetUrl.Segments[assetUrl.Segments.Length - 1];
            var cutPath = assetUrl.LocalPath.Remove(assetUrl.LocalPath.Length - assetName.Length, assetName.Length).Trim('/');
            return Path.Combine(baseDirectoryPath, cutPath).Replace('\\', '/');
        }
    }



    /// <summary>
    /// IceMilkTeaArchiveからアセットをロードするローダクラスです
    /// </summary>
    public class ImtArchiveAssetLoader : AssetLoader
    {
        /// <summary>
        /// アーカイブ内に含まれたアセットバンドル情報を収める構造体です
        /// </summary>
        private struct EntryAssetBundleInfo
        {
            /// <summary>
            /// 開いているアセットバンドル
            /// </summary>
            public AssetBundle AssetBundle { get; private set; }


            /// <summary>
            /// 該当アセットバンドルからロードしたアセットIDリスト
            /// </summary>
            public List<ulong> LoadedAssetIdList { get; private set; }



            /// <summary>
            /// EntryAssetBundleInfo のインスタンスを初期化します
            /// </summary>
            /// <param name="assetBundle">開かれたアセットバンドル</param>
            public EntryAssetBundleInfo(AssetBundle assetBundle)
            {
                // そのまま受け取る
                AssetBundle = assetBundle;
                LoadedAssetIdList = new List<ulong>();
            }
        }



        // 定数定義
        private const string QueryNameKeyName = "name";

        // メンバ変数定義
        private ImtArchive archive;
        private Dictionary<ulong, EntryAssetBundleInfo> assetBundleTable;
        private Dictionary<string, string> uriQueryBufferTable;



        /// <summary>
        /// ImtArchiveAssetLoader のインスタンス
        /// </summary>
        /// <param name="archivePath">このローダが担当するアーカイブファイルへのパス</param>
        public ImtArchiveAssetLoader(string archivePath)
        {
            // この時点でアーカイブを開いておく
            archive = new ImtArchive(archivePath);
            if (archive.CanFetchManageData())
            {
                // マネージデータが読み込めるなら読み込む
                archive.FetchManageData();
            }


            // アセットバンドルテーブルも初期化
            assetBundleTable = new Dictionary<ulong, EntryAssetBundleInfo>();
            uriQueryBufferTable = new Dictionary<string, string>();
        }


        /// <summary>
        /// 指定されたアセットIDと、アセットURLからアセットを非同期でロードします
        /// </summary>
        /// <typeparam name="TAssetType">読み込むアセットの型</typeparam>
        /// <param name="assetId">読み込むアセットID</param>
        /// <param name="assetUrl">読み込むアセットURL</param>
        /// <param name="progress">ロード進捗通知を受ける IProgress</param>
        /// <returns>待機可能なロードクラスのインスタンスを返します</returns>
        public override IAwaitable<TAssetType> LoadAssetAsync<TAssetType>(ulong assetId, Uri assetUrl, IProgress<float> progress)
        {
            // ロード完了待機ハンドルを生成して実際のロードを非同期で行い、ひとまずハンドルをすぐに返す
            var waitHandle = new ImtAwaitableManualReset<TAssetType>(false);
            DoAssetLoadAsync(assetId, assetUrl, progress, waitHandle);
            return waitHandle;
        }


        /// <summary>
        /// 実際の非同期ロードを行います
        /// </summary>
        /// <typeparam name="TAssetType">読み込むアセットの型</typeparam>
        /// <param name="assetId">読み込むアセットID</param>
        /// <param name="assetUrl">読み込むアセットURL</param>
        /// <param name="progress">ロード進捗通知を受ける IProgress</param>
        private async void DoAssetLoadAsync<TAssetType>(ulong assetId, Uri assetUrl, IProgress<float> progress, ImtAwaitableManualReset<TAssetType> waitHandle) where TAssetType : UnityAsset
        {
            // アセットバンドル名とアセット名の取得してアセットバンドルIDも用意
            var assetBundleName = GetAssetBundleName(assetUrl);
            var assetName = GetAssetName(assetUrl);
            var assetBundleId = assetBundleName.ToCrc64Code();


            // もしまだアセットバンドルが開かれていなかったら
            EntryAssetBundleInfo info;
            if (!assetBundleTable.TryGetValue(assetBundleId, out info))
            {
                // アセットバンドルのエントリIDを計算してアセットバンドルを非同期で開く
                var entryId = archive.CalculateEntryId(assetBundleName);
                var assetBundle = await AssetBundle.LoadFromStreamAsync(archive.GetEntryStream(entryId)).ToAwaitable(progress);


                // 開いたアセットバンドルを覚える
                info = new EntryAssetBundleInfo(assetBundle);
                assetBundleTable[assetBundleId] = info;
            }


            // アセットバンドルから該当のアセットを非同期にロードするがロード出来なかったら
            var asset = await info.AssetBundle.LoadAssetAsync<TAssetType>(assetName).ToAwaitable<TAssetType>(progress);
            if (asset == null)
            {
                // 読み込めなかったということでnullで待機ハンドルにシグナルを送る
                waitHandle.Set(null);
                return;
            }


            // 読み込めたのならアセットIDを覚えて、待機ハンドルに結果付きでシグナルを送る
            // TODO : アーカイブ内アセットバンドルについては、開きっぱにするかどうかで検討中
            //info.LoadedAssetIdList.Add(assetId);
            waitHandle.Set(asset);
        }


        /// <summary>
        /// アセットURLから読み込むべきアセットバンドル名を取得します
        /// </summary>
        /// <param name="assetUrl">アセットバンドル名が含まれているアセットURL</param>
        /// <returns>アセットURLから取得されたアセットバンドル名を返します</returns>
        private string GetAssetBundleName(Uri assetUrl)
        {
            // URLの最後のセグメントがアセットバンドル名の一部になるので取得してそのまま返す
            return assetUrl.Segments[assetUrl.Segments.Length - 1];
        }


        /// <summary>
        /// アセットURLから読み込むべきアセット名を取得します
        /// </summary>
        /// <param name="assetUrl">アセット名が含まれるアセットURL</param>
        /// <returns>アセットURLから取得されたアセット名を返しますが、取得出来なかった場合は null を返します</returns>
        private string GetAssetName(Uri assetUrl)
        {
            // URLからクエリを取り出す
            uriQueryBufferTable.Clear();
            assetUrl.ToQueryDictionary(uriQueryBufferTable);


            // nameのキーの取得結果をそのまま伝える
            string assetName;
            uriQueryBufferTable.TryGetValue(QueryNameKeyName, out assetName);
            return assetName;
        }
    }
    #endregion



    #region アセットクリーナ
    /// <summary>
    /// アセットのクリーンアップを行うクラスです
    /// </summary>
    internal class AssetCleaner
    {
        // メンバ変数定義
        private AssetCleanupAwaitable[] cleanupAwaitables;



        /// <summary>
        /// AssetCleaner のインスタンスを初期化します
        /// </summary>
        /// <param name="cacheStorate">クリーンアップ対象になるキャッシュストレージ</param>
        public AssetCleaner(AssetCacheStorage cacheStorate)
        {
            // 各レベル毎のクリーンアップ待機可能クラスのインスタンスを生成
            cleanupAwaitables = new AssetCleanupAwaitable[3];
            cleanupAwaitables[(int)AssetCleanupAggressiveLevel.Low] = new AssetCleanupLowLevelAwaitable(cacheStorate);
            cleanupAwaitables[(int)AssetCleanupAggressiveLevel.Normal] = new AssetCleanupNormalLevelAwaitable(cacheStorate);
            cleanupAwaitables[(int)AssetCleanupAggressiveLevel.High] = new AssetCleanupHighLevelAwaitable(cacheStorate);
        }


        /// <summary>
        /// 不要になったアセットなどを意図的にクリーンアップを非同期的に実行します。
        /// また、非同期動作中にこの関数を操作しても、動作中のクリーンアップが終わるまでは
        /// 指定された、度合いのクリーンアップは行われません。
        /// </summary>
        /// <param name="level">アセットクリーンアップの度合いを指定します。度合いが高いほどクリーンアップに時間がかかります。</param>
        /// <returns>クリーンアップを待機するインスタンスを返しますが、既に非同期操作中のクリーンがある場合は、その待機クラスのインスタンスを返します。</returns>
        public IAwaitable AssetCleanupAsync(AssetCleanupAggressiveLevel level)
        {
            // クリーンアップレベル分回る
            foreach (var cleanupAwaitable in cleanupAwaitables)
            {
                // 既に動作中なら
                if (cleanupAwaitable.IsRunning)
                {
                    // この動作中の待機クラスを返す
                    return cleanupAwaitable;
                }
            }


            // 指定されたレベルのクリーンアップを開始して返す
            return cleanupAwaitables[(int)level].Run();
        }
    }
    #endregion



    #region AssetClearWorker
    /// <summary>
    /// 待機可能な、アセットクリーンアップ抽象クラスです
    /// </summary>
    internal abstract class AssetCleanupAwaitable : ImtAwaitableUpdateBehaviour
    {
        // メンバ変数定義
        protected AssetCacheStorage assetCacheStorage;



        /// <summary>
        /// AssetCleanupLowLevelAwaitable のインスタンスを初期化します
        /// </summary>
        /// <param name="storage">クリーンアップ時にキャッシュクリーンアップを対応するストレージ</param>
        public AssetCleanupAwaitable(AssetCacheStorage storage)
        {
            // 受け取る
            assetCacheStorage = storage;
        }
    }



    /// <summary>
    /// 待機可能な、度合いが最も低いレベルアセットクリーンアップクラスです
    /// </summary>
    internal class AssetCleanupLowLevelAwaitable : AssetCleanupAwaitable
    {
        /// <summary>
        /// AssetCleanupLowLevelAwaitable のインスタンスを初期化します
        /// </summary>
        /// <param name="storage">クリーンアップ時にキャッシュクリーンアップを対応するストレージ</param>
        public AssetCleanupLowLevelAwaitable(AssetCacheStorage storage) : base(storage)
        {
        }


        /// <summary>
        /// 待機可能クラスの更新を開始します
        /// </summary>
        protected internal override void Start()
        {
            // キャッシュストレージに未参照の解放をお願いして作業は完了
            assetCacheStorage.DoCleanupUnreferencedAssets();
            SetSignalWithCompleted();
        }
    }



    /// <summary>
    /// 待機可能な、度合いが通常のレベルアセットクリーンアップクラスです
    /// </summary>
    internal class AssetCleanupNormalLevelAwaitable : AssetCleanupAwaitable
    {
        /// <summary>
        /// AssetCleanupNormalLevelAwaitable のインスタンスを初期化します
        /// </summary>
        /// <param name="storage">クリーンアップ時にキャッシュクリーンアップを対応するストレージ</param>
        public AssetCleanupNormalLevelAwaitable(AssetCacheStorage storage) : base(storage)
        {
        }


        /// <summary>
        /// 待機可能クラスの更新を開始します
        /// </summary>
        protected internal override async void Start()
        {
            // まずはUnityのリソース解放関数を叩いて待機
            await Resources.UnloadUnusedAssets();


            // キャッシュストレージに未参照の解放をお願いして作業は完了
            assetCacheStorage.DoCleanupUnreferencedAssets();
            SetSignalWithCompleted();
        }
    }



    /// <summary>
    /// 待機可能な、度合いが最も高いレベルアセットクリーンアップクラスです
    /// </summary>
    internal class AssetCleanupHighLevelAwaitable : AssetCleanupAwaitable
    {
        // メンバ変数定義
        private ImtTask gcTask;



        /// <summary>
        /// AssetCleanupHighLevelAwaitable のインスタンスを初期化します
        /// </summary>
        /// <param name="storage">クリーンアップ時にキャッシュクリーンアップを対応するストレージ</param>
        public AssetCleanupHighLevelAwaitable(AssetCacheStorage storage) : base(storage)
        {
            // GCを起動するためのタスクを生成する
            gcTask = new ImtTask(_ => GC.Collect());
        }


        /// <summary>
        /// 待機可能クラスの更新を開始します
        /// </summary>
        protected internal override async void Start()
        {
            // 本来はこの関数は多重呼び出しされないはずだが、念の為GCタスクが動作していないことを確認
            if (!gcTask.IsRunning)
            {
                // GCタスクを後ろで実行して待機する
                // MEMO : 実際は全世界が止まるので極端に良くなるわけではない
                await gcTask.Run(ImtAwaitableUpdateBehaviourScheduler.GetThreadPoolScheduler());
            }


            // まずはUnityのリソース解放関数を叩いて待機
            await Resources.UnloadUnusedAssets();


            // キャッシュストレージに未参照の解放をお願いして作業は完了
            assetCacheStorage.DoCleanupUnreferencedAssets();
            SetSignalWithCompleted();
        }
    }
    #endregion
}