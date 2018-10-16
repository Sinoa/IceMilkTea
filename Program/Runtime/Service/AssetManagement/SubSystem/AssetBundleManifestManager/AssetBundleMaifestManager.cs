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
using System.Threading.Tasks;
using IceMilkTea.Core;

namespace IceMilkTea.Service
{
    /// <summary>
    /// アセットバンドルマニフェストを制御、管理を行うマネージャ抽象クラスです
    /// </summary>
    public abstract class AssetBundleManifestManager
    {
        // 限定公開メンバ変数定義
        protected ImtAssetBundleManifest currentAssetBundleManifest;



        /// <summary>
        /// マネージャの初期化を非同期で行います
        /// </summary>
        /// <returns>初期化操作を行っているタスクを返します</returns>
        public abstract Task InitializeAsync();


        /// <summary>
        /// マニフェストのフェッチを非同期で行います。
        /// フェッチされたマニフェストは、内部データに反映はされません。
        /// データとして更新が必要かどうかについては GetUpdatableAssetBundlesAsync() を用いてください。
        /// </summary>
        /// <param name="progress">フェッチダウンロードの進捗通知を受ける Progress</param>
        /// <returns>マニフェストフェッチの非同期操作をしているタスクを返します</returns>
        public abstract Task<ImtAssetBundleManifest> FetchManifestAsync(IProgress<WebDownloadProgress> progress);


        /// <summary>
        /// 指定された新しいマニフェストを元に、更新の必要のあるアセットバンドル情報の取得を非同期で行います。
        /// </summary>
        /// <param name="newerManifest">新しいとされるマニフェスト</param>
        /// <param name="progress">チェック進捗通知を受ける Progress</param>
        /// <returns>現在管理しているマニフェスト情報から、新しいマニフェスト情報で更新の必要なるアセットバンドル情報の配列を、操作しているタスクを返します</returns>
        public abstract Task<UpdatableAssetBundleInfo[]> GetUpdatableAssetBundlesAsync(ImtAssetBundleManifest newerManifest, IProgress<UpdatableAssetBundleProgress> progress);


        /// <summary>
        /// 指定された新しいマニフェストで、現在管理しているマニフェストに非同期で更新します。
        /// </summary>
        /// <param name="newerManifest">新しいとされるマニフェスト</param>
        /// <returns>マニフェストの更新を行っているタスクを返します</returns>
        public abstract Task UpdateManifestAsync(ImtAssetBundleManifest newerManifest);


        /// <summary>
        /// 指定された名前のアセットバンドル情報を取得します
        /// </summary>
        /// <param name="assetBundleName">アセットバンドル情報を取得する、アセットバンドル名</param>
        /// <param name="assetBundleInfo">取得されたアセットバンドルの情報を格納する参照</param>
        /// <exception cref="ArgumentNullException">assetBundleName が null です</exception>
        /// <exception cref="ArgumentException">アセットバンドル名 '{assetBundleName}' のアセットバンドル情報が見つかりませんでした</exception>
        public void GetAssetBundleInfo(string assetBundleName, out AssetBundleInfo assetBundleInfo)
        {
            // 名前に null が渡されたら
            if (assetBundleName == null)
            {
                // どうしろってんだい
                throw new ArgumentNullException(nameof(assetBundleName));
            }


            // 現在のマニフェストに含まれるコンテンツグループ分回る
            var contentGrops = currentAssetBundleManifest.ContentGroups;
            for (int i = 0; i < contentGrops.Length; ++i)
            {
                // コンテンツグループ内にあるアセットバンドル情報の数分回る
                var assetBundleInfos = contentGrops[i].AssetBundleInfos;
                for (int j = 0; j < assetBundleInfos.Length; ++j)
                {
                    // アセットバンドル名が一致したのなら
                    if (assetBundleInfos[j].Name == assetBundleName)
                    {
                        // この情報を渡して終了
                        assetBundleInfo = assetBundleInfos[j];
                        return;
                    }
                }
            }


            // ループから抜けてきたということは見つからなかったということ
            throw new ArgumentException($"アセットバンドル名 '{assetBundleName}' のアセットバンドル情報が見つかりませんでした", nameof(assetBundleName));
        }


        public abstract string[] GetContentGroupNames();


        public abstract void GetContentGroupInfo(string contentGroupName, out AssetBundleContentGroup contentGroup);
    }
}