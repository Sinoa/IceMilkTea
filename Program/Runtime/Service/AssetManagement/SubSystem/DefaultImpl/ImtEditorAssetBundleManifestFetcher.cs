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
using System.Security.Cryptography;
using System.Threading.Tasks;
using UnityEngine;

namespace IceMilkTea.Service
{
    /// <summary>
    /// ローカルからアセットバンドルマニフェストを取得するフェッチャークラスです
    /// </summary>
    public class ImtEditorAssetBundleManifestFetcher : AssetBundleManifestFetcher
    {
        // メンバ変数定義
        private AssetBundleManifest unityAssetBundleManifest;
        private DirectoryInfo baseDirectoryInfo;



        /// <summary>
        /// ImtWebAssetBundleManifestFetcher のインスタンスを初期化します
        /// </summary>
        /// <param name="manifestUrl">取得元になるマニフェストURL</param>
        /// <exception cref="ArgumentNullException">manifestUrl が null です</exception>
        /// <exception cref="NotSupportedException">アセットバンドルマニフェストのフェッチURLにサポートしない '{manifestUrl.Scheme}' スキームが指定されました。サポートしているスキームは http および https です</exception>
        public ImtEditorAssetBundleManifestFetcher(string rootAssetBundlePath)
        {
            // ルートアセットバンドルを読み込んでアセットバンドルマニフェストを読み込む
            var rootAssetBundle = AssetBundle.LoadFromFile(rootAssetBundlePath);
            unityAssetBundleManifest = rootAssetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");


            // ルートパスのディレクトリをベースディレクトリ情報
            baseDirectoryInfo = new DirectoryInfo(Path.GetDirectoryName(rootAssetBundlePath));


            // ルートアセットバンドルを閉じる
            rootAssetBundle.Unload(false);
        }


        /// <summary>
        /// マニフェストの取得を非同期で行います
        /// </summary>
        /// <returns>マニフェストの取得を非同期で行っているタスクを返します</returns>
        public override Task<ImtAssetBundleManifest> FetchAsync()
        {
            // 結果として返すアセットバンドルマニフェストを生成してタイムスタンプとコンテンツグループを初期化
            var manifest = new ImtAssetBundleManifest();
            manifest.LastUpdateTimeStamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            manifest.ContentGroups = new AssetBundleContentGroup[1];


            // 全てのアセットバンドル名を取得してコンテンツグループを初期化
            var allAssetBundleNames = unityAssetBundleManifest.GetAllAssetBundles();
            manifest.ContentGroups[0].Name = "AssetBundle";
            manifest.ContentGroups[0].AssetBundleInfos = new AssetBundleInfo[allAssetBundleNames.Length];


            // 全てのアセットバンドル名分回る
            for (int i = 0; i < allAssetBundleNames.Length; ++i)
            {
                // アセットバンドル名を取り出して依存リストも取得
                var assetBundleName = allAssetBundleNames[i];
                var dependencies = unityAssetBundleManifest.GetDirectDependencies(assetBundleName);


                // アセットバンドルのファイル情報を生成して、ハッシュも用意する
                var assetBundleFilePath = Path.Combine(baseDirectoryInfo.FullName, assetBundleName).Replace("\\", "/");
                var assetBundleFileInfo = new FileInfo(assetBundleFilePath);
                var assetBundleHash = SHA1.Create().ComputeHash(File.ReadAllBytes(assetBundleFilePath));


                // アセットバンドル情報を初期化する
                manifest.ContentGroups[0].AssetBundleInfos[i] = new AssetBundleInfo()
                {
                    // 諸々情報を詰める
                    DependenceAssetBundleNames = dependencies,
                    Name = assetBundleName,
                    LocalPath = assetBundleName,
                    RemotePath = assetBundleName,
                    Size = assetBundleFileInfo.Length,
                    Hash = assetBundleHash,
                    UserData = 0,
                };
            }


            // 用意したマニフェストを返す
            return Task.FromResult(manifest);
        }
    }
}