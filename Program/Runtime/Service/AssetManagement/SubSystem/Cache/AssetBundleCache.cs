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
using UnityEngine;

namespace IceMilkTea.Service
{
    /// <summary>
    /// オープン済みアセットバンドルの参照をキャッシュするクラスです
    /// </summary>
    internal class AssetBundleCache
    {
        // 定数定義
        private const int DefaultCapacity = 2 << 10;

        // メンバ変数定義
        private Dictionary<string, AssetBundle> assetBundleCacheTable;



        /// <summary>
        /// AssetBundleCache のインスタンスを初期化します
        /// </summary>
        public AssetBundleCache()
        {
            // キャッシュテーブルの生成
            assetBundleCacheTable = new Dictionary<string, AssetBundle>(DefaultCapacity);
        }


        /// <summary>
        /// 指定されたIDでアセットバンドルをキャッシュします。
        /// また、既にキャッシュ済みのアセットバンドルが存在する場合、キャッシュ済みアセットバンドルはアンロードされます。
        /// </summary>
        /// <param name="assetBundlePath">キャッシュするアセットバンドルのパス</param>
        /// <param name="assetBundle">キャッシュするアセットバンドル</param>
        /// <exception cref="ArgumentNullException">assetBundlePath が null です</exception>
        /// <exception cref="ArgumentNullException">assetBundle が null です</exception>
        public void CacheAssetBundle(string assetBundlePath, AssetBundle assetBundle)
        {
            // もしnullを渡されていたら
            if (assetBundlePath == null)
            {
                // キャッシュするアセットバンドル名がわからない
                throw new ArgumentNullException(nameof(assetBundlePath));
            }


            // もしnullを渡されていたら
            if (assetBundle == null)
            {
                // 何をキャッシュするのか
                throw new ArgumentNullException(nameof(assetBundle));
            }


            // キャッシュ済みのアセットバンドルが存在する場合は
            AssetBundle cachedAssetBundle;
            if (assetBundleCacheTable.TryGetValue(assetBundlePath, out cachedAssetBundle))
            {
                // アンロードしておく
                cachedAssetBundle.Unload(false);
            }


            // IDをキーにアセットバンドルの参照を覚えておく
            assetBundleCacheTable[assetBundlePath] = assetBundle;
        }


        /// <summary>
        /// 指定されたアセットバンドルIDからアセットバンドルの取得を試みます。
        /// </summary>
        /// <param name="assetBundlePath">キャッシュするアセットバンドルの名前</param>
        /// <param name="assetBundle">取得したアセットバンドルを格納します</param>
        /// <returns>アセットバンドルの取得に成功したときは true を、取得ができなかった場合は false を返します</returns>
        /// <exception cref="ArgumentNullException">assetBundlePath が null です</exception>
        public bool TryGetAssetBundle(string assetBundlePath, out AssetBundle assetBundle)
        {
            // DictionaryのTryGetをそのまま呼ぶ
            return assetBundleCacheTable.TryGetValue(assetBundlePath, out assetBundle);
        }
    }
}