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
    /// <summary>
    /// ゲームアセットの読み込み、取得、管理を総合的に管理をするサービスクラスです
    /// </summary>
    public class AssetManagementService : GameService
    {
        // メンバ変数定義
        private UriInfoCache urlCache;
        private UnityAssetCache assetCache;
        private AssetBundleCache assetBundleCache;
        private List<AssetBundleManifestFetcher> manifestFetcherList;
        private List<AssetBundleStorage> storageList;
        private List<AssetBundleInstaller> installerList;



        /// <summary>
        /// AssetManagementService のインスタンスを初期化します
        /// </summary>
        public AssetManagementService()
        {
            // サブシステムなどの初期化をする
            urlCache = new UriInfoCache();
            assetCache = new UnityAssetCache();
            assetBundleCache = new AssetBundleCache();
            manifestFetcherList = new List<AssetBundleManifestFetcher>();
            storageList = new List<AssetBundleStorage>();
            installerList = new List<AssetBundleInstaller>();
        }


        /// <summary>
        /// マニフェストフェッチャーの追加を行います。
        /// </summary>
        /// <param name="fetcher">追加するフェッチャー</param>
        /// <exception cref="ArgumentNullException">fetcher が null です</exception>
        /// <exception cref="ArgumentException">既に追加済みの fetcher です</exception>
        public void AddManifestFetcher(AssetBundleManifestFetcher fetcher)
        {
            // null を渡されたら
            if (fetcher == null)
            {
                // nullは許されない
                throw new ArgumentNullException(nameof(fetcher));
            }


            // 既に追加済みのフェッチャーだったら
            if (manifestFetcherList.Contains(fetcher))
            {
                // 多重追加は許されない
                throw new ArgumentException($"既に追加済みの {nameof(fetcher)} です");
            }


            // 追加する
            manifestFetcherList.Add(fetcher);
        }


        /// <summary>
        /// ストレージの追加を行います。
        /// </summary>
        /// <param name="storage">追加するストレージ</param>
        /// <exception cref="ArgumentNullException">storage が null です</exception>
        /// <exception cref="ArgumentException">既に追加済みの storage です</exception>
        public void AddStorage(AssetBundleStorage storage)
        {
            // null を渡されたら
            if (storage == null)
            {
                // null は許されない
                throw new ArgumentNullException(nameof(storage));
            }


            // 既に追加済みのストレージだったら
            if (storageList.Contains(storage))
            {
                // 多重追加は許されない
                throw new ArgumentException($"既に追加済みの {nameof(storage)} です");
            }


            // 追加する
            storageList.Add(storage);
        }


        /// <summary>
        /// インストーラの追加を行います。
        /// </summary>
        /// <param name="installer"></param>
        public void AddInstaller(AssetBundleInstaller installer)
        {
            // null を渡されたら
            if (installer == null)
            {
                // null は許されない
                throw new ArgumentNullException(nameof(installer));
            }


            // 既に追加済みのインストーラだったら
            if (installerList.Contains(installer))
            {
                // 多重追加は許されない
                throw new ArgumentException($"既に追加済みの {nameof(installer)} です");
            }


            // 追加する
            installerList.Add(installer);
        }


        // コンパイルエラーが出ないようにするための雑対応
        public System.Threading.Tasks.Task<T> LoadAssetAsync<T>(string name) where T : UnityEngine.Object
        {
            throw new System.NotImplementedException();
        }
    }
}