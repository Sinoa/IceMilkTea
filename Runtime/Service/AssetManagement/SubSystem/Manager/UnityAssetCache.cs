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

namespace IceMilkTea.Service
{
    // Unityのアセットの基底型になるObjectは、System.Objectとややこしくなるので
    // ここではUnityAssetと名付ける（WeakReference版も定義）
    using UnityAsset = UnityEngine.Object;
    using WeakUnityAsset = WeakReference<UnityEngine.Object>;



    /// <summary>
    /// Unityアセットの参照を保持しておきキャッシュするクラスです。
    /// </summary>
    internal class UnityAssetCache
    {
        // 定数定義
        private const int DefaultCapacity = 2 << 10;

        // メンバ変数定義
        private Dictionary<UriInfo, WeakUnityAsset> assetCacheTable;



        /// <summary>
        /// UnityAssetCache のインスタンスを初期化します
        /// </summary>
        public UnityAssetCache()
        {
            // アセットキャッシュテーブルを生成する
            assetCacheTable = new Dictionary<UriInfo, WeakUnityAsset>(DefaultCapacity);
        }


        /// <summary>
        /// 指定されたアセットUrlとアセットでキャッシュします
        /// </summary>
        /// <param name="assetUrl">キャッシュするアセットのUrl</param>
        /// <param name="asset">キャッシュするアセット</param>
        /// <exception cref="ArgumentNullException">assetUrl が null です</exception>
        /// <exception cref="ArgumentNullException">asset が null です</exception>
        public void CacheAsset(UriInfo assetUrl, UnityAsset asset)
        {
            // もしnullを渡されていたら
            if (assetUrl == null)
            {
                // キャッシュするキーがわからない
                throw new ArgumentNullException(nameof(assetUrl));
            }


            // もしnullを渡されていたら
            if (asset == null)
            {
                // なにをキャッシュするのか
                throw new ArgumentNullException(nameof(asset));
            }


            // まずはキャッシュテーブルから参照が取得できるのなら
            WeakUnityAsset weakUnityAsset;
            if (assetCacheTable.TryGetValue(assetUrl, out weakUnityAsset))
            {
                // 参照の更新だけして終了
                weakUnityAsset.SetTarget(asset);
                return;
            }


            // キャッシュテーブルにもキャッシュすらない場合は新しい参照を追加する
            assetCacheTable[assetUrl] = new WeakUnityAsset(asset);
        }


        /// <summary>
        /// 指定されたアセットIDからアセットの取得を行います。
        /// 過去にキャッシュされたアセットでも参照が途切れている場合はキャッシュが取得出来ない場合があります。
        /// </summary>
        /// <param name="assetUrl">取得したいアセットのUrl</param>
        /// <param name="asset">取得されたアセットの参照を設定します</param>
        /// <returns>指定されたアセットIDからアセットキャッシュの取得ができた場合は true を、出来なかった場合は false を返します</returns>
        public bool TryGetAsset(UriInfo assetUrl, out UnityAsset asset)
        {
            // キャッシュテーブルから参照の取得ができなかった場合は
            if (!assetCacheTable.TryGetValue(assetUrl, out var weakUnityAsset))
            {
                // 参照を初期化して取得に失敗を返す
                asset = null;
                return false;
            }


            // 参照変数からキャッシュへの参照を取得を試み結果を返す（参照が途切れてレコードの削除は別関数で行う）
            if (!weakUnityAsset.TryGetTarget(out var assetTemp))
            {
                asset = null;
                return false;
            }

            //WeakRefが指す対象がC#的なnullではないが、Unityのアセットとして不正な場合
            //assetCacheから削除する
            if (assetTemp == null)
            {
                asset = null;
                assetCacheTable.Remove(assetUrl);
                return false;
            }

            asset = assetTemp;
            return true;
        }


        /// <summary>
        /// 内部のキャッシュテーブルに登録されたキャッシュアセットの、
        /// 参照が消失しているレコードをクリーンアップします
        /// </summary>
        public void CleanupUnreferencedCache()
        {
            // 削除するべきアセットIDリストをリストアップするためにテーブル全体を舐める
            var removeTargetList = new List<UriInfo>(assetCacheTable.Count);
            foreach (var kvp in assetCacheTable)
            {
                // 参照からキャッシュの参照が取得でき無いのなら
                if (!kvp.Value.TryGetTarget(out var _))
                {
                    // 削除する対象として記録する
                    removeTargetList.Add(kvp.Key);
                }
            }


            // 削除する対象として記録されたアセットID分回る
            foreach (var assetId in removeTargetList)
            {
                // キャッシュテーブルから削除する
                assetCacheTable.Remove(assetId);
            }
        }


        /// <summary>
        /// 内部のキャッシュテーブルに登録されたキャッシュ情報を、全て無条件でクリーンアップします
        /// </summary>
        public void CleanupAllCache()
        {
            // キャッシュテーブルをクリアする
            assetCacheTable.Clear();
        }
    }
}