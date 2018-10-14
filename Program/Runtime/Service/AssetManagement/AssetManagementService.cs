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
using UnityEngine;

namespace IceMilkTea.Service
{
    /// <summary>
    /// ゲームアセットの読み込み、取得、管理を総合的に管理をするサービスクラスです
    /// </summary>
    public class AssetManagementService : GameService
    {
        // 定数定義
        private const string AssetScheme = "asset";
        private const string ResourcesHostName = "resources";
        private const string AssetBundleHostName = "assetbundle";
        private const string AssetNameQueryName = "name";

        // 読み取り専用クラス変数宣言
        private static readonly IProgress<float> EmptyProgress = new Progress<float>(_ => { });

        // メンバ変数定義
        private UriInfoCache uriCache;
        private UnityAssetCache assetCache;
        private AssetBundleStorage storage;
        private AssetBundleInstaller installer;
        private AssetBundleManifestFetcher manifestFetcher;
        private AssetBundleManifestStorage manifestStorage;



        /// <summary>
        /// AssetManagementService のインスタンスを初期化します。
        /// </summary>
        /// <param name="storage">アセットバンドルを貯蔵するストレージ</param>
        /// <param name="installer">アセットバンドルをストレージにインストールするインストーラ</param>
        /// <param name="manifestStorage">マニフェストを貯蔵するストレージ</param>
        /// <param name="manifestFetcher">マニフェストをフェッチするフェッチャー</param>
        public AssetManagementService(AssetBundleStorage storage, AssetBundleInstaller installer, AssetBundleManifestStorage manifestStorage, AssetBundleManifestFetcher manifestFetcher)
        {
            // サブシステムなどの初期化をする
            uriCache = new UriInfoCache();
            assetCache = new UnityAssetCache();
            this.storage = storage;
            this.installer = installer;
            this.manifestStorage = manifestStorage;
            this.manifestFetcher = manifestFetcher;
        }


        #region LoadAsync
        /// <summary>
        /// 指定されたアセットURLのアセットを非同期でロードします
        /// </summary>
        /// <typeparam name="T">ロードするアセットの型</typeparam>
        /// <param name="assetUrl">ロードするアセットのURL</param>
        /// <returns>指定されたアセットの非同期ロードを操作しているタスクを返します</returns>
        /// <exception cref="ArgumentNullException">assetUrl が null です</exception>
        /// <exception cref="InvalidOperationException">指定されたアセットのロードに失敗しました Url={assetUrl}</exception>
        public Task<T> LoadAssetAsync<T>(string assetUrl) where T : UnityEngine.Object
        {
            // 進捗通知を受けずに非同期ロードを行う
            return LoadAssetAsync<T>(assetUrl, null);
        }


        /// <summary>
        /// 指定されたアセットURLのアセットを非同期でロードします
        /// </summary>
        /// <typeparam name="T">ロードするアセットの型</typeparam>
        /// <param name="assetUrl">ロードするアセットのURL</param>
        /// <param name="progress">アセットロードの進捗通知を受ける IProgress</param>
        /// <returns>指定されたアセットの非同期ロードを操作しているタスクを返します</returns>
        /// <exception cref="ArgumentNullException">assetUrl が null です</exception>
        /// <exception cref="InvalidOperationException">指定されたアセットのロードに失敗しました Url={assetUrl}</exception>
        /// <exception cref="ArgumentException">不明なスキーム '{uriInfo.Uri.Scheme}' が指定されました。AssetManagementServiceは 'asset' スキームのみサポートしています。</exception>
        /// <exception cref="ArgumentException">不明なストレージホスト '{storageName}' が指定されたました。 'resources' または 'assetbundle' を指定してください。</exception>
        public async Task<T> LoadAssetAsync<T>(string assetUrl, IProgress<float> progress) where T : UnityEngine.Object
        {
            // もしURLがnullなら
            if (assetUrl == null)
            {
                // 何をロードするのか不明
                throw new ArgumentNullException(nameof(assetUrl));
            }


            // UriキャッシュからUri情報を取得する
            var uriInfo = uriCache.GetOrCreateUri(assetUrl);


            // もしアセットキャッシュからアセットを取り出せるのなら
            UnityEngine.Object asset;
            if (assetCache.TryGetAsset(uriInfo, out asset))
            {
                // このアセットを返す
                return (T)asset;
            }


            // スキームがassetでなければ
            if (uriInfo.Uri.Scheme != AssetScheme)
            {
                // assetスキーム以外は断るようにする
                throw new ArgumentException($"不明なスキーム '{uriInfo.Uri.Scheme}' が指定されました。AssetManagementServiceは 'asset' スキームのみサポートしています。");
            }


            // プログレスが null なら空のプログレスを設定する
            progress = progress ?? EmptyProgress;


            // ホスト名（ストレージ名）を取得してもし Resources なら.
            var storageName = uriInfo.Uri.Host;
            if (storageName == ResourcesHostName)
            {
                // Resoucesからアセットをロードする
                asset = await LoadResourcesAssetAsync<T>(uriInfo, progress);
            }
            else if (storageName == AssetBundleHostName)
            {
                // Resourcesでないならアセットバンドル側からロードする
                asset = await LoadAssetBundleAssetAsync<T>(storageName, uriInfo, progress);
            }
            else
            {
                // どれも違うのなら何でロードすればよいのかわからない例外を吐く
                throw new ArgumentException($"不明なストレージホスト '{storageName}' が指定されたました。 '{ResourcesHostName}' または '{AssetBundleHostName}' を指定してください。");
            }


            // もしアセットのロードに失敗していたら
            if (asset == null)
            {
                // アセットのロードに失敗したことを通知する
                throw new InvalidOperationException($"指定されたアセットのロードに失敗しました Url={assetUrl}");
            }


            // 読み込まれたアセットをキャッシュに追加して返す
            assetCache.CacheAsset(uriInfo, asset);
            return (T)asset;
        }
        #endregion


        #region Resources Load
        /// <summary>
        /// Resourcesから非同期にアセットのロードを行います
        /// </summary>
        /// <typeparam name="T">ロードするアセットの型</typeparam>
        /// <param name="assetUrl">ロードするアセットURL</param>
        /// <param name="progress">ロードの進捗通知を受ける　IProgress</param>
        /// <returns>ロードに成功した場合は有効なアセットの参照をかえします。ロードに失敗した場合は null を返します。</returns>
        private async Task<T> LoadResourcesAssetAsync<T>(UriInfo assetUrl, IProgress<float> progress) where T : UnityEngine.Object
        {
            // 結果を納める変数宣言
            T result = default(T);


            // Resourcesホストの場合はローカルパスがロードするパスになる
            var assetPath = assetUrl.Uri.LocalPath.TrimStart('/');


            // もしマルチスプライト型のロード要求なら
            if (typeof(T) == typeof(MultiSprite))
            {
                // Resourcesには、残念ながらAll系の非同期ロード関数がないのでここで同期読み込みをするが、ロードに失敗したら
                var sprites = Resources.LoadAll<Sprite>(assetPath);
                if (sprites == null)
                {
                    // ロードが出来なかったということでnullを返す
                    return null;
                }


                // マルチスプライトアセットとしてインスタンスを生成して結果に納める
                result = (T)(UnityEngine.Object)new MultiSprite(sprites);
            }
            else
            {
                // 特定型ロードでなければ通常の非同期ロードを行う
                result = await Resources.LoadAsync<T>(assetPath).ToAwaitable<T>(progress);
            }


            // 結果を返す
            return result;
        }
        #endregion


        #region AssetBundle Load
        /// <summary>
        /// AssetBundleから非同期にアセットのロードを行います
        /// </summary>
        /// <typeparam name="T">ロードするアセットの型</typeparam>
        /// <param name="storageName">ロードするアセットを含むアセットバンドルを開くストレージ</param>
        /// <param name="assetUrl">ロードするアセットURL</param>
        /// <param name="progress">ロードの進捗通知を受ける IProgress</param>
        /// <returns>ロードに成功した場合は有効なアセットの参照をかえします。ロードに失敗した場合は null を返します。</returns>
        /// <exception cref="InvalidOperationException">アセットバンドルからロードするべきアセット名を取得出来ませんでした。クエリに 'name' パラメータがあることを確認してください。</exception>
        private async Task<T> LoadAssetBundleAssetAsync<T>(string storageName, UriInfo assetUrl, IProgress<float> progress) where T : UnityEngine.Object
        {
            // ロードするアセット名を取得するが見つけられなかったら
            var assetPath = default(string);
            if (!assetUrl.QueryTable.TryGetValue(AssetNameQueryName, out assetPath))
            {
                // ロードするアセット名が不明である例外を吐く
                throw new InvalidOperationException($"アセットバンドルからロードするべきアセット名を取得出来ませんでした。クエリに '{AssetNameQueryName}' パラメータがあることを確認してください。");
            }


            // ローカルパスを取得してアセットバンドルを開く
            var localPath = assetUrl.Uri.LocalPath.TrimStart('/');
            var assetBundle = await storage.OpenAsync(localPath);


            // 結果を納める変数宣言
            T result = default(T);


            // もしマルチスプライト型のロード要求なら
            if (typeof(T) == typeof(MultiSprite))
            {
                // サブアセット系非同期ロードを行い待機する
                var requestTask = assetBundle.LoadAssetWithSubAssetsAsync<Sprite>(assetPath);
                await requestTask;


                // もし読み込みが出来なかったのなら
                if (requestTask.allAssets == null)
                {
                    // 読み込めなかったことを結果に入れる
                    result = null;
                }
                else
                {
                    // 読み込み結果を格納する
                    var spriteArray = Array.ConvertAll(requestTask.allAssets, x => (Sprite)x);
                    result = (T)(UnityEngine.Object)new MultiSprite(spriteArray);
                }
            }
            else
            {
                // 特定型ロードでなければ通常の非同期ロードを行う
                result = await assetBundle.LoadAssetAsync<T>(assetPath).ToAwaitable<T>(progress);
            }


            // 結果を返す
            return result;
        }
        #endregion


        #region Manifest
        /// <summary>
        /// マニフェストの更新を非同期で行います。
        /// このサービスを利用する前に最初に呼び出すことを推奨します
        /// </summary>
        /// <returns>アセットの更新を非同期で行っているタスクを返します</returns>
        public async Task UpdateManifestAsync()
        {
            await manifestStorage.LoadAsync(null);


            var manifests = await manifestFetcher.FetchManifestAsync();
            for (int i = 0; i < manifests.Length; ++i)
            {
                manifestStorage.SetManifest(ref manifests[i]);
            }


            await manifestStorage.SaveAsync(null);
        }


        // TODO : 今はフルダウンロードというひどい実装
        public async Task InstallAssetBundleAsync()
        {
            var manifestNames = manifestStorage.GetAllManifestName();
            foreach (var manifestName in manifestNames)
            {
                ImtAssetBundleManifest manifest;
                if (!manifestStorage.TryGetManifest(manifestName, out manifest))
                {
                    continue;
                }


                AssetBundleInfo[] infos = manifest.AssetBundleInfos;
                for (int i = 0; i < infos.Length; ++i)
                {
                    using (var installStream = await storage.GetInstallStreamAsync(infos[i]))
                    {
                        await installer.InstallAsync(infos[i], installStream, null);
                    }
                }
            }
        }
        #endregion
    }
}