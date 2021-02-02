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
using System.Threading.Tasks;
using IceMilkTea.Core;
using IceMilkTea.SubSystem;
using UnityEngine;

namespace IceMilkTea.Service
{
    /// <summary>
    /// AssetBundleStorageController を使って高レベル層の管理を行うマネージャクラスです。
    /// 主に、アセットバンドルキャッシュと、アセットバンドルの依存関係を解決を行います。
    /// </summary>
    internal class AssetBundleStorageManager
    {
        private const int DefaultAssetBundleTableCapacity = 2 << 10;

        private readonly AssetBundleManifestManager manifestManager;
        private readonly AssetBundleStorageController storageController;
        private readonly Dictionary<string, Task<AssetBundleManagementContext>> assetBundleTable;
        private readonly IAssetStorage storage;



        /// <summary>
        /// ロード済みアセットバンドルの数
        /// </summary>
        public int LoadedAssetBundleCount => GetLoadedAssetBundleCount();



        /// <summary>
        /// AssetBundleStorageManager のインスタンスを初期化します
        /// </summary>
        /// <param name="manifestManager">パス解決などに利用するマニフェストマネージャ</param>
        /// <param name="storageController">アセットバンドルの入出力を実際に行うコントローラ</param>
        /// <exception cref="ArgumentNullException">manifestManager または storageController または storage が null です</exception>
        public AssetBundleStorageManager(AssetBundleManifestManager manifestManager, AssetBundleStorageController storageController, IAssetStorage storage)
        {
            this.manifestManager = manifestManager ?? throw new ArgumentNullException(nameof(manifestManager));
            this.storageController = storageController ?? throw new ArgumentNullException(nameof(storageController));
            this.storage = storage ?? throw new ArgumentNullException(nameof(storage));
            assetBundleTable = new Dictionary<string, Task<AssetBundleManagementContext>>(DefaultAssetBundleTableCapacity);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="catalog"></param>
        /// <param name="item"></param>
        /// <param name="assetBundleOpenStack"></param>
        /// <exception cref="InvalidAssetBundleException">アセットバンドル '{item.Name}' が依存する、アセットバンドル '{dependenceAssetBundleName}' の情報が見つかりませんでした。</exception>
        /// <exception cref="InvalidAssetBundleException">アセットバンドル '{item.Name}' が '{dependenceAssetBundleName}' に依存していますが、循環参照を起こしています。</exception>
        /// <exception cref="AssetBundleNotFoundException">アセットバンドル '{item.Name}' がストレージにインストールされていないため開くことが出来ません</exception>
        /// <returns></returns>
        public async Task<AssetBundle> OpenAsync(ImtCatalog catalog, ImtCatalogItem item, Stack<string> assetBundleOpenStack)
        {
            if (assetBundleOpenStack == null)
            {
                throw new ArgumentNullException(nameof(assetBundleOpenStack));
            }


            assetBundleOpenStack.Push(item.Name);
            foreach (var dependenceAssetBundleName in item.DependentItemNames)
            {
                var dependentItem = catalog.GetItem(dependenceAssetBundleName);
                if (dependentItem == null)
                {
                    throw new InvalidAssetBundleException($"アセットバンドル '{item.Name}' が依存する、アセットバンドル '{dependenceAssetBundleName}' の情報が見つかりませんでした。", item.Name, null);
                }


                if (assetBundleOpenStack.Contains(dependenceAssetBundleName))
                {
                    throw new InvalidAssetBundleException($"アセットバンドル '{item.Name}' が '{dependenceAssetBundleName}' に依存していますが、循環参照を起こしています。", item.Name, assetBundleOpenStack);
                }


                await OpenAsync(catalog, dependentItem, assetBundleOpenStack);
            }

            // アセットバンドル情報から名前を取得して既にコンテキストが存在するなら
            if (assetBundleTable.TryGetValue(item.Name, out var context))
            {
                // コンテキストからアセットバンドルを取得して返す
                assetBundleOpenStack.Pop();
                return (await context).GetAssetBundle();
            }


            // 開こうとしているアセットバンドルがまだストレージに存在しないなら
            if (!storage.ExistsAsset(item.LocalUri))
            {
                // 未インストールアセットバンドルが存在することを例外で吐く
                throw new AssetBundleNotFoundException($"アセットバンドル '{item.Name}' がストレージにインストールされていないため開くことが出来ません", item.LocalUri.ToString());
            }

            // 新しくコントローラからアセットバンドルを開くことを要求する
            var createContextTask = CreateAssetBundleManagementContextAsync(item);
            assetBundleTable[item.Name] = createContextTask;
            assetBundleOpenStack.Pop();
            return (await createContextTask).GetAssetBundle();
        }


        private async Task<AssetBundleManagementContext> CreateAssetBundleManagementContextAsync(ImtCatalogItem item)
        {
            //var stream = storage.OpenAsset(item.LocalUri, AssetStorageAccess.Read);
            // 指定されたパスのアセットバンドルを非同期で開くが開けなかったら
            var assetBundle = await AssetBundle.LoadFromFileAsync(storage.ToAssetFilePath(item.LocalUri));
            if (assetBundle == null)
            {
                // アセットバンドルが開けなかったことを例外で吐く
                throw new InvalidAssetBundleException($"アセットバンドル '{item.Name}' を開くことが出来ませんでした", item.Name, null);
            }

            // 開いたアセットバンドルを返す
            return new AssetBundleManagementContext(assetBundle);
        }


        /// <summary>
        /// 指定されたアセットバンドルの利用を破棄します。
        /// また、アセットバンドルの依存解決も同時に行い、依存するアセットバンドルも破棄します。
        /// </summary>
        /// <param name="assetBundle">利用を破棄するアセットバンドル</param>
        public void Release(AssetBundle assetBundle)
        {
            // アセットバンドル管理テーブルを回る
            var assetBundleInfo = default(AssetBundleInfo);
            foreach (var managementRecord in assetBundleTable)
            {
                // そもそもLoadが終わってない場合や参照が違うアセットバンドルなら
                var task = managementRecord.Value;
                if (!task.IsCompleted || !task.Result.EqualAssetBundleReference(assetBundle))
                {
                    // 次のレコードへ
                    continue;
                }


                // 管理名からアセットバンドル情報を取得してループから抜ける
                manifestManager.GetAssetBundleInfo(managementRecord.Key, out assetBundleInfo);
                break;
            }


            // この時点でアセットバンドル情報が存在していないなら
            if (string.IsNullOrWhiteSpace(assetBundleInfo.Name))
            {
                // そもそも解放すべきアセットバンドルが存在しないか解放済みのため終了
                return;
            }


            // アセットバンドル情報から依存するアセットバンドル分回る
            foreach (var dependenceAssetBundleName in assetBundleInfo.DependenceAssetBundleNames)
            {
                // 依存先アセットバンドル名からアセットバンドル管理コンテキストを取得するが、管理テーブルに無いなら
                if (!assetBundleTable.TryGetValue(dependenceAssetBundleName, out var dependenceContextTask))
                {
                    // 次の依存アセットバンドルへ
                    continue;
                }

                //まだLoad中のAssetBundle
                if (!dependenceContextTask.IsCompleted)
                {
                    //memo:<e.kudaka>正直スキップしていいのかはわからない
                    //Release候補においてもいいかもしれない
                    continue;
                }

                // 依存先アセットバンドルの参照を取り出す
                var dependenceAssetBundle = dependenceContextTask.Result.GetAssetBundle();
                dependenceContextTask.Result.ReleaseAssetBundle(dependenceAssetBundle);


                // 再帰的に解放を行う
                Release(dependenceAssetBundle);
            }


            // 自身のアセットバンドル管理コンテキストを取得して存在しないなら
            if (!assetBundleTable.TryGetValue(assetBundleInfo.Name, out var contextTask))
            {
                // 何もせず終了
                return;
            }

            if (!contextTask.IsCompleted)
                return;

            // コンテキストから解放を行い参照が無くなった場合は
            contextTask.Result.ReleaseAssetBundle(assetBundle);
            if (contextTask.Result.Unreferenced)
            {
                // アセットストレージコントローラに閉じるように要求して、管理テーブルからレコードを削除する
                storageController.Close(assetBundle);
                assetBundleTable.Remove(assetBundleInfo.Name);
            }
        }


        private int GetLoadedAssetBundleCount()
        {
            var count = 0;
            foreach (var task in assetBundleTable.Values)
            {
                if (task.IsCompleted)
                {
                    count++;
                }
            }


            return count;
        }


        /// <summary>
        /// ロード済みアセットバンドルの情報を渡します
        /// </summary>
        /// <param name="result">ロード済みアセットバンドルを受け取る配列への参照。配列の長さが不十分の場合にすべての参照を渡せない可能性があります。</param>
        /// <returns>情報を渡した数を返します</returns>
        public int GetLoadedAssetBundles(AssetBundle[] result)
        {
            if ((result ?? throw new ArgumentException(nameof(result))).Length == 0)
            {
                return 0;
            }


            var targetIndex = 0;
            foreach (var task in assetBundleTable.Values)
            {
                if (task.IsCompleted)
                {
                    result[targetIndex++] = task.Result.GetAssetBundle();
                    if (targetIndex == result.Length)
                    {
                        return targetIndex;
                    }
                }
            }


            return targetIndex;
        }



        #region アセットバンドル管理コンテキストクラス定義
        /// <summary>
        /// アセットバンドルの管理状況を保持したコンテキストクラスです
        /// </summary>
        private class AssetBundleManagementContext
        {
            private readonly AssetBundle assetBundle;



            /// <summary>
            /// アセットバンドルの参照カウント値を取得します
            /// </summary>
            public int ReferenceCount { get; private set; }


            /// <summary>
            /// アセットバンドルへの参照がどこからも無くなっているかどうか
            /// </summary>
            public bool Unreferenced => ReferenceCount == 0;



            /// <summary>
            /// AssetBundleManagementContext のインスタンスを初期化します
            /// </summary>
            /// <param name="targetAssetBundle">管理対象となるアセットバンドル</param>
            /// <exception cref="ArgumentNullException">targetAssetBundle が null です</exception>
            public AssetBundleManagementContext(AssetBundle targetAssetBundle)
            {
                assetBundle = targetAssetBundle ?? throw new ArgumentNullException(nameof(targetAssetBundle));
                ReferenceCount = 1;
            }


            /// <summary>
            /// 管理しているアセットバンドルを取得します
            /// </summary>
            /// <returns>管理しているアセットバンドルを返します</returns>
            public AssetBundle GetAssetBundle()
            {
                // 参照カウントをインクリメントして参照を返す
                ++ReferenceCount;
                return assetBundle;
            }


            /// <summary>
            /// 対象のアセットバンドルの参照を破棄します
            /// </summary>
            /// <param name="assetBundle">破棄するアセットバンドルの参照</param>
            /// <exception cref="ArgumentException">解放しようとしたアセットバンドルの参照が異なっています。Managed={this.assetBundle.name} Request={assetBundle.name}</exception>
            /// <exception cref="InvalidOperationException">既に解放済みのアセットバンドルを更に解放しようとしました</exception>
            public void ReleaseAssetBundle(AssetBundle assetBundle)
            {
                // もし渡された参照と管理対象のアセットバンドルの参照が異なるなら
                if (!EqualAssetBundleReference(assetBundle))
                {
                    // これは間違った解放をしようとしている
                    throw new ArgumentException($"解放しようとしたアセットバンドルの参照が異なっています。Managed={this.assetBundle.name} Request={assetBundle.name}", nameof(assetBundle));
                }


                // 参照カウントが既に0なら
                if (ReferenceCount == 0)
                {
                    // 必要以上なReleaseが呼び出された可能性がある
                    throw new InvalidOperationException("既に解放済みのアセットバンドルを更に解放しようとしました");
                }


                // 問題なければ参照カウントをデクリメントする
                --ReferenceCount;
            }


            /// <summary>
            /// 指定されたアセットバンドルの参照と一致するかどうか確認をします
            /// </summary>
            /// <param name="assetBundle">参照の確認を行うアセットバンドル</param>
            /// <returns>参照が一致していれば true を、一致していなければ false を返します</returns>
            public bool EqualAssetBundleReference(AssetBundle assetBundle)
            {
                // 素直に参照の比較結果を返す
                return this.assetBundle == assetBundle;
            }
        }
        #endregion
    }
}