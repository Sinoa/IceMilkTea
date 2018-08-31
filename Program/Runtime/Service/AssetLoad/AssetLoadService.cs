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
    }



    #region キャッシュクラス
    /// <summary>
    /// 読み込まれたアセットのキャッシュを貯蔵するクラスです
    /// </summary>
    internal class AssetCacheStorage
    {
        // メンバ変数定義
        private Dictionary<ulong, WeakUnityAsset> assetCacheTable;



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
        /// <param name="assetId">これからロードする予定のアセットID</param>
        /// <param name="assetUrl">これからロードする予定のアセットURL</param>
        /// <returns>対応可能なアセットローダが存在した場合は、そのローダのインスタンスを返しますが、存在しない場合は null を返します</returns>
        public AssetLoader GetAssetLoader(ulong assetId, Uri assetUrl)
        {
            // 登録されているリゾルバ分回る
            foreach (var resolver in resolverList)
            {
                // アセットIDとURLを渡してローダを取得出来たのなら
                var loader = resolver.GetLoader(assetId, assetUrl);
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
        /// <param name="assetId">ロード要求のあるアセットID</param>
        /// <param name="assetUrl">ロード要求のあるアセットURL</param>
        /// <returns>最適なアセットローダがある場合は、そのローダのインスタンスを返しますが、存在しない場合は null を返します</returns>
        public abstract AssetLoader GetLoader(ulong assetId, Uri assetUrl);
    }



    /// <summary>
    /// アセットのロードを実際に行うローダ抽象クラスです
    /// </summary>
    public abstract class AssetLoader
    {
        /// <summary>
        /// 指定されたアセットID、アセットURLからアセットを非同期に読み込みます。
        /// </summary>
        /// <param name="assetId">読み込むアセットID</param>
        /// <param name="assetUrl">読み込むアセットURL</param>
        /// <param name="progress">読み込み状況の進捗通知を受ける IProgress</param>
        /// <returns>アセットの非同期ロードを待機する待機可能クラスのインスタンスを返します</returns>
        public abstract IAwaitable<UnityAsset> LoadAssetAsync(ulong assetId, Uri assetUrl, IProgress<float> progress);
    }
    #endregion



    #region AssetLoaderResolverの実体
    /// <summary>
    /// UnityのResourcesからアセットをロードするローダを解決するクラスです
    /// </summary>
    public class ResourcesAssetLoaderResolver : AssetLoaderResolver
    {
        public override AssetLoader GetLoader(ulong assetId, Uri assetUrl)
        {
            throw new NotImplementedException();
        }
    }



    /// <summary>
    /// Unityのファイル状になっているアセットバンドルからアセットをロードするローダを解決するクラスです
    /// </summary>
    public class FileAssetBundleAssetLoaderResolver : AssetLoaderResolver
    {
        public override AssetLoader GetLoader(ulong assetId, Uri assetUrl)
        {
            throw new NotImplementedException();
        }
    }



    /// <summary>
    /// IceMilkTeaArchiveからアセットをロードするローダを解決するクラスです
    /// </summary>
    public class ImtArchiveLoaderResolver : AssetLoaderResolver
    {
        public override AssetLoader GetLoader(ulong assetId, Uri assetUrl)
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
        public override IAwaitable<UnityAsset> LoadAssetAsync(ulong assetId, Uri assetUrl, IProgress<float> progress)
        {
            throw new NotImplementedException();
        }
    }



    /// <summary>
    /// Unityのファイル状アセットバンドルからアセットをロードするローダクラスです
    /// </summary>
    public class FileAssetBundleAssetLoader : AssetLoader
    {
        public override IAwaitable<UnityAsset> LoadAssetAsync(ulong assetId, Uri assetUrl, IProgress<float> progress)
        {
            throw new NotImplementedException();
        }
    }



    /// <summary>
    /// IceMilkTeaArchiveからアセットをロードするローダクラスです
    /// </summary>
    public class ImtArchiveAssetLoader : AssetLoader
    {
        public override IAwaitable<UnityAsset> LoadAssetAsync(ulong assetId, Uri assetUrl, IProgress<float> progress)
        {
            throw new NotImplementedException();
        }
    }
    #endregion
}