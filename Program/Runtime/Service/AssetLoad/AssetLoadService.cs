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
using IceMilkTea.Core;
using UnityEngine;

namespace IceMilkTea.Service
{
    // Unityのアセットの基底型になるObjectは、SystemのObjectとややこしくなるので
    // ここではUnityAssetと名付ける（WeakReference版も定義）
    using UnityAsset = UnityEngine.Object;
    using WeakUnityAsset = WeakReference<UnityEngine.Object>;



    /// <summary>
    /// Unityのゲームアセットを読み込む機能を提供するサービスクラスです
    /// </summary>
    public class AssetLoadService : GameService
    {
        // 定数定義
        public const string AssetScheme = "asset";

        // メンバ変数定義
        private Crc64TextCoder textCoder;
        private AssetCacheStorage cacheStorage;
        private AssetLoaderProvider loaderProvider;



        /// <summary>
        /// AssetLoadService のインスタンスを初期化します
        /// </summary>
        public AssetLoadService()
        {
            // テキストコーダ、キャッシュストレージ、ローダプロバイダの初期化
            textCoder = new Crc64TextCoder();
            cacheStorage = new AssetCacheStorage();
            loaderProvider = new AssetLoaderProvider();
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
        /// <exception cref="ArgumentException">uriString（assetUrl） が null です</exception>
        /// <exception cref="ArgumentException">アセットロードサービスは asset スキーム以外のURLは処理を受け付けていません</exception>
        /// <exception cref="UriFormatException">指定されたアセットURLのフォーマットが正しくありません</exception>
        /// <exception cref="InvalidOperationException">指定されたアセットURLからアセットをロードが出来ませんでした</exception>
        public IAwaitable<TAssetType> LoadAssetAsync<TAssetType>(string assetUrl, IProgress<float> progress) where TAssetType : UnityAsset
        {
            // URIを生成する（assetUrl引数にまつわる例外はURIに委ねる）
            var url = new Uri(assetUrl);


            // もし asset スキームでは無いのなら
            if (url.Scheme != AssetScheme)
            {
                // assetスキーム以外は現在受け付けない
                throw new ArgumentException("アセットロードサービスは asset スキーム以外のURLは処理を受け付けていません");
            }


            // URLからIDを作る
            var assetId = textCoder.GetCode(assetUrl);


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


            // ローダプロバイダにローダを要求して誰一人と対応可能なローダがいなかったら
            var loader = loaderProvider.GetAssetLoader<TAssetType>(assetId, url);
            if (loader == null)
            {
                // ごめんなさい、ロード出来ません
                throw new InvalidOperationException("指定されたアセットURLからアセットをロードが出来ませんでした");
            }


            // ローダにロードを要求して、キャッシュ関数にも流す
            var loadAwaitable = loader.LoadAssetAsync<TAssetType>(assetId, url, progress);
            DoAssetCache(assetId, loadAwaitable);


            // ローダが返した待機クラスのインスタンスを返す
            return loadAwaitable;
        }


        /// <summary>
        /// 指定されたアセットIDに対して、ロード待機オブジェクトからロード結果を取得して、キャッシュを行います
        /// </summary>
        /// <typeparam name="TAssetType">ロードしようとしているアセットの型</typeparam>
        /// <param name="assetId">キャッシュするアセットID</param>
        /// <param name="awaitable">ロード中の待機オブジェクト</param>
        private async void DoAssetCache<TAssetType>(ulong assetId, IAwaitable<TAssetType> awaitable) where TAssetType : UnityAsset
        {
            // ロードの完了をまって、ロードに失敗しているようであれば
            var asset = await awaitable;
            if (asset == null)
            {
                // キャッシュはしないで終わる
                return;
            }


            // アセットのキャッシュをする
            cacheStorage.StoreAssetCache(assetId, asset);
        }
    }



    #region キャッシュクラス
    /// <summary>
    /// 読み込まれたアセットのキャッシュを貯蔵するクラスです
    /// </summary>
    internal class AssetCacheStorage
    {
        // 定数定義
        private const int DefaultCacheEntryCapacity = 1 << 10;

        // メンバ変数定義
        private Dictionary<ulong, WeakUnityAsset> assetCacheTable;



        /// <summary>
        /// AssetCacheStorage のインスタンスを初期化します
        /// </summary>
        public AssetCacheStorage()
        {
            // キャッシュテーブルを生成する
            assetCacheTable = new Dictionary<ulong, WeakUnityAsset>(DefaultCacheEntryCapacity);
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
            WeakUnityAsset assetReference;
            if (!assetCacheTable.TryGetValue(assetId, out assetReference))
            {
                // まだキャッシュされていない
                return null;
            }


            // 参照から実体の取得が出来なかったら
            UnityAsset asset;
            if (!assetReference.TryGetTarget(out asset))
            {
                // レコードを削除してキャッシュが取り出せなかったとして返す
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
        /// <exception cref="ArgumentNullException">asset が null です</exception>
        public void StoreAssetCache(ulong assetId, UnityAsset asset)
        {
            // もしassetがnullなら
            if (asset == null)
            {
                // 何を貯蔵するんですか
                throw new ArgumentNullException(nameof(asset));
            }


            // 一度テーブルから参照を引っ張り出せるか試みる
            WeakUnityAsset assetReference;
            if (assetCacheTable.TryGetValue(assetId, out assetReference))
            {
                // 取り出せたのなら参照を上書きして終了
                assetReference.SetTarget(asset);
                return;
            }


            // そもそもレコードすら無いのなら新規で追加する
            assetCacheTable[assetId] = new WeakUnityAsset(asset);
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
    }
    #endregion



    #region AssetLoaderResolverの実体
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
            // ホスト名部分がResourcesローダー系の物でないなら
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
    /// Unityのファイル状になっているアセットバンドルからアセットをロードするローダを解決するクラスです
    /// </summary>
    public class FileAssetBundleAssetLoaderResolver : AssetLoaderResolver
    {
        public override AssetLoader GetLoader<TAssetType>(ulong assetId, Uri assetUrl)
        {
            throw new NotImplementedException();
        }
    }



    /// <summary>
    /// IceMilkTeaArchiveからアセットをロードするローダを解決するクラスです
    /// </summary>
    public class ImtArchiveLoaderResolver : AssetLoaderResolver
    {
        public override AssetLoader GetLoader<TAssetType>(ulong assetId, Uri assetUrl)
        {
            throw new NotImplementedException();
        }
    }
    #endregion



    #region AssetLoaderの実体
    /// <summary>
    /// UnityのResourcesからアセットをロードするローダクラスです
    /// </summary>
    public class ResourcesAssetLoader : AssetLoader
    {
        /// <summary>
        /// 指定されたアセットIDと、アセットURLからアセットを非同期でロードします
        /// </summary>
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



    /// <summary>
    /// Unityのファイル状アセットバンドルからアセットをロードするローダクラスです
    /// </summary>
    public class FileAssetBundleAssetLoader : AssetLoader
    {
        public override IAwaitable<TAssetType> LoadAssetAsync<TAssetType>(ulong assetId, Uri assetUrl, IProgress<float> progress)
        {
            throw new NotImplementedException();
        }
    }



    /// <summary>
    /// IceMilkTeaArchiveからアセットをロードするローダクラスです
    /// </summary>
    public class ImtArchiveAssetLoader : AssetLoader
    {
        public override IAwaitable<TAssetType> LoadAssetAsync<TAssetType>(ulong assetId, Uri assetUrl, IProgress<float> progress)
        {
            throw new NotImplementedException();
        }
    }
    #endregion
}