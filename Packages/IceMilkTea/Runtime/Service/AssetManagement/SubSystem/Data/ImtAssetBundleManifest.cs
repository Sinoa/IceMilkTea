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
    /// <summary>
    /// 1つ以上のアセットバンドル情報を保持した構造体です。
    /// アセットバンドルの管理情報なども一部含みます。
    /// </summary>
    [Serializable]
    public struct ImtAssetBundleManifest
    {
        /// <summary>
        /// マニフェストを最後に更新したタイムスタンプ。
        /// 原則値の違いでのみ判断するので、正確な日時を記録する必要はありません。
        /// DateTimeOffset.ToUnixTimeSeconds() 関数を使った値を推奨します。
        /// </summary>
        public long LastUpdateTimeStamp;


        /// <summary>
        /// アセットバンドル情報をコンテンツ単位でグループ化した構造体の配列
        /// </summary>
        public AssetBundleContentGroup[] ContentGroups;



        /// <summary>
        /// マニフェストが保持している、アセットバンドル情報全てのサイズの合計値
        /// </summary>
        public long TotalAssetBundleSize
        {
            get
            {
                // トータルサイズを記憶する変数を宣言
                var totalSize = 0L;


                // 保持しているコンテンツグループ分回る
                for (int i = 0; i < ContentGroups.Length; ++i)
                {
                    // コンテンツグループが提供するアセットバンドル合計サイズを加算する
                    totalSize += ContentGroups[i].TotalAssetBundleSize;
                }


                // 結果を返す
                return totalSize;
            }
        }


        /// <summary>
        /// マニフェストが保持している、全アセットバンドルの件数を取得します
        /// </summary>
        public int TotalAssetBundleInfoCount
        {
            get
            {
                // トータルカウントを記憶する変数宣言
                var totalCount = 0;


                // 保持しているコンテンツグループ分回る
                for (int i = 0; i < ContentGroups.Length; ++i)
                {
                    // コンテンツグループが保持しているアセットバンドルの数を集計する
                    totalCount += ContentGroups[i].AssetBundleInfos.Length;
                }


                // 結果を返す
                return totalCount;
            }
        }



        /// <summary>
        /// 指定されたコンテンツグループ名のアセットバンドル合計サイズを取得します
        /// </summary>
        /// <param name="contentGroupName">サイズを取得したいコンテンツグループ名</param>
        /// <returns>指定されたコンテンツグループが保有するアセットバンドルの合計サイズを返します</returns>
        /// <exception cref="ArgumentException">指定された名前 '{contentGroupName}' のコンテンツグループは見つかりませんでした</exception>
        public long GetContentGroupAssetBundleTotalSize(string contentGroupName)
        {
            // 保持しているコンテンツグループ分回る
            for (int i = 0; i < ContentGroups.Length; ++i)
            {
                // もし指定されたコンテンツグループ名と一致したのなら
                if (ContentGroups[i].Name == contentGroupName)
                {
                    // このコンテンツグループのサイズを返す
                    return ContentGroups[i].TotalAssetBundleSize;
                }
            }


            // ループから抜けたということは見つからなかったということ
            throw new ArgumentException($"指定された名前 '{contentGroupName}' のコンテンツグループは見つかりませんでした", nameof(contentGroupName));
        }


        /// <summary>
        /// このマニフェストが保持している、すべてのコンテンツグループのすべてのアセットバンドル情報を
        /// 指定されたグループ名をキーとした、アセットバンドル情報の配列にすべてコピーします。
        /// </summary>
        /// <remarks>
        /// もし、コピー先の配列がコピーする情報を受けられる十分な長さがない場合は、コピー先の配列が埋まるまでの数をコピーします。
        /// また、コピー元の長さより長い配列の場合は、残りの要素は特に触れることはありません。
        /// </remarks>
        /// <param name="destination">コピー先の配列</param>
        /// <exception cref="ArgumentNullException">destination が null です</exception>
        public void AllAssetBundleInfoCopyTo(KeyValuePair<string, AssetBundleInfo>[] destination)
        {
            // nullを渡されたら
            if (destination == null)
            {
                // どこにコピーすれば良いのか
                throw new ArgumentNullException(nameof(destination));
            }


            // 長さが 0 の配列を渡されたのなら
            if (destination.Length == 0)
            {
                // 何もせず終了
                return;
            }


            // 保持しているコンテンツグループ分回る
            int totalCopyCount = 0;
            for (int groupIndex = 0; groupIndex < ContentGroups.Length; ++groupIndex)
            {
                // コンテンツグループが所持しているアセットバンドル情報分回る
                var assetBundleInfos = ContentGroups[groupIndex].AssetBundleInfos;
                for (int infoIndex = 0; infoIndex < assetBundleInfos.Length; ++infoIndex)
                {
                    // 渡された配列にコピーする
                    destination[totalCopyCount++] = new KeyValuePair<string, AssetBundleInfo>(ContentGroups[groupIndex].Name, assetBundleInfos[infoIndex]);


                    // もしコピーした件数と渡された配列の長さと一致したのなら
                    if (totalCopyCount == destination.Length)
                    {
                        // もうコピーは出来ないので終了
                        return;
                    }
                }
            }
        }
    }
}