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
    /// <summary>
    /// オープン済みアセットバンドルの参照をキャッシュするクラスです
    /// </summary>
    internal class AssetBundleCache
    {
        // 定数定義
        private const int DefaultCapacity = 1 << 10;

        // メンバ変数定義
        private Dictionary<ulong, AssetBundle> assetBundleCacheTable;



        /// <summary>
        /// AssetBundleCache のインスタンスを初期化します
        /// </summary>
        public AssetBundleCache()
        {
            // キャッシュテーブルの生成
            assetBundleCacheTable = new Dictionary<ulong, AssetBundle>(DefaultCapacity);
        }


        /// <summary>
        /// アセットバンドル名からアセットバンドルIDを取得します
        /// </summary>
        /// <param name="assetBundleName">アセットバンドルIDを取得したいアセットバンドル名</param>
        /// <returns>取得されたアセットバンドルIDを返します</returns>
        /// <exception cref="ArgumentNullException">assetBundleName が null です</exception>
        public ulong GetAssetBundleId(string assetBundleName)
        {
            // nullを渡されたら
            if (assetBundleName == null)
            {
                // どんなIDを取得すればよいのか
                throw new ArgumentNullException(nameof(assetBundleName));
            }


            // CRC64計算結果を返す
            return assetBundleName.ToCrc64Code();
        }


        /// <summary>
        /// 指定されたIDでアセットバンドルをキャッシュします。
        /// また、既にキャッシュ済みのアセットバンドルが存在する場合、キャッシュ済みアセットバンドルはアンロードされます。
        /// </summary>
        /// <param name="assetBundleId">キャッシュするアセットバンドルのID</param>
        /// <param name="assetBundle">キャッシュするアセットバンドル</param>
        /// <exception cref="ArgumentException">assetBundle が null です</exception>
        public void CacheAssetBundle(ulong assetBundleId, AssetBundle assetBundle)
        {
            // もしnullを渡されたら
            if (assetBundle == null)
            {
                // 何をキャッシュするのか
                throw new ArgumentNullException(nameof(assetBundle));
            }


            // キャッシュ済みのアセットバンドルが存在する場合は
            AssetBundle cachedAssetBundle;
            if (assetBundleCacheTable.TryGetValue(assetBundleId, out cachedAssetBundle))
            {
                // アンロードしておく
                cachedAssetBundle.Unload(false);
            }


            // IDをキーにアセットバンドルの参照を覚えておく
            assetBundleCacheTable[assetBundleId] = assetBundle;
        }


        /// <summary>
        /// 指定されたアセットバンドルIDからアセットバンドルの取得を試みます。
        /// </summary>
        /// <param name="assetBundleId">取得するアセットバンドルID</param>
        /// <param name="assetBundle">取得したアセットバンドルを格納します</param>
        /// <returns>アセットバンドルの取得に成功したときは true を、取得ができなかった場合は false を返します</returns>
        public bool TryGetAssetBundle(ulong assetBundleId, out AssetBundle assetBundle)
        {
            // DictionaryのTryGetをそのまま呼ぶ
            return assetBundleCacheTable.TryGetValue(assetBundleId, out assetBundle);
        }
    }
}