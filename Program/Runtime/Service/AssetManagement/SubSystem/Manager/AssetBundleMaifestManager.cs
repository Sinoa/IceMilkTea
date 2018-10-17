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
using System.IO;
using System.Threading.Tasks;

namespace IceMilkTea.Service
{
    /// <summary>
    /// アセットバンドルマニフェストを制御、管理を行うマネージャクラスです
    /// </summary>
    internal class AssetBundleManifestManager
    {
        // メンバ変数定義
        private AssetBundleManifestFetcher fetcher;
        private ImtAssetBundleManifest manifest;
        private DirectoryInfo saveDirectoryInfo;
        private Uri manifestUrl;



        /// <summary>
        /// AssetBundleManifestManager のインスタンスを初期化します
        /// </summary>
        /// <param name="fetcher">マニフェストの取り込みを行うフェッチャー</param>
        /// <param name="saveDirectoryInfo">マニフェストを保存するディレクトリ情報</param>
        /// <param name="manifestUrl">管理を行うマニフェストが存在するURL</param>
        /// <exception cref="ArgumentNullException">fetcher が null です</exception>
        /// <exception cref="ArgumentNullException">saveDirectoryInfo が null です</exception>
        /// <exception cref="ArgumentNullException">manifestUrl が null です</exception>
        public AssetBundleManifestManager(AssetBundleManifestFetcher fetcher, DirectoryInfo saveDirectoryInfo, Uri manifestUrl)
        {
            // もし null を渡された場合は
            if (fetcher == null)
            {
                // どうやってマニフェストを取り出そうか
                throw new ArgumentNullException(nameof(fetcher));
            }


            // 保存先ディレクトリ情報がnullなら
            if (saveDirectoryInfo == null)
            {
                // どこに保存すればいいんじゃ
                throw new ArgumentNullException(nameof(saveDirectoryInfo));
            }


            // マニフェストURLがnullなら
            if (manifestUrl == null)
            {
                // 何を元に管理をすればよいのだ
                throw new ArgumentNullException(nameof(manifestUrl));
            }


            // 受け取る
            this.fetcher = fetcher;
            this.saveDirectoryInfo = saveDirectoryInfo;
            this.manifestUrl = manifestUrl;
        }


        /// <summary>
        /// マニフェストの取り込みを非同期で行います。取り込んだマニフェストは、内部データに反映はされません。
        /// データとして更新が必要かどうかについては GetUpdatableAssetBundlesAsync() を用いてください。
        /// </summary>
        /// <returns>取り込みに成功した場合は、有効な参照の ImtAssetBundleManifest のインスタンスを返しますが、失敗した場合は null を返すタスクを返します</returns>
        public async Task<ImtAssetBundleManifest?> FetchManifestAsync()
        {
            // フェッチャーのフェッチをそのまま呼ぶ
            return await fetcher.FetchAsync(manifestUrl);
        }


        /// <summary>
        /// 指定された新しいマニフェストを元に、更新の必要のあるアセットバンドル情報の取得を非同期で行います。
        /// </summary>
        /// <param name="newerManifest">新しいとされるマニフェスト</param>
        /// <param name="progress">チェック進捗通知を受ける Progress</param>
        /// <returns>現在管理しているマニフェスト情報から、新しいマニフェスト情報で更新の必要なるアセットバンドル情報の配列を、操作しているタスクを返します</returns>
        public Task<UpdatableAssetBundleInfo[]> GetUpdatableAssetBundlesAsync(ImtAssetBundleManifest newerManifest, IProgress<UpdatableAssetBundleProgress> progress)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// 指定された新しいマニフェストで、現在管理しているマニフェストに非同期で更新します。
        /// </summary>
        /// <param name="newerManifest">新しいとされるマニフェスト</param>
        /// <returns>マニフェストの更新を行っているタスクを返します</returns>
        public Task UpdateManifestAsync(ImtAssetBundleManifest newerManifest)
        {
            throw new NotImplementedException();
        }


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
            var contentGrops = manifest.ContentGroups;
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


        public string[] GetContentGroupNames()
        {
            throw new NotImplementedException();
        }


        public void GetContentGroupInfo(string contentGroupName, out AssetBundleContentGroup contentGroup)
        {
            throw new NotImplementedException();
        }
    }
}