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
using IceMilkTea.Core;
using UnityEngine;

namespace IceMilkTea.Service
{
    /// <summary>
    /// アセットバンドルのロードモードを表現する列挙型です
    /// </summary>
    public enum AssetBundleLoadMode
    {
        /// <summary>
        /// 通常通りのAssetBundleStorageからのロードを行います
        /// </summary>
        Native,

        /// <summary>
        /// ロードパスがそのままUnityのプロジェクトからロードを行います
        /// </summary>
        Simulate,

        /// <summary>
        /// エディタ環境でビルドしたアセットバンドルからロードを行います
        /// </summary>
        LocalBuild,
    }



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
        private AssetBundleManifestManager manifestManager;
        private AssetBundleStorageManager storageManager;
        private AssetBundleLoadMode loadMode;



        /// <summary>
        /// AssetManagementService のインスタンスを初期化します。
        /// </summary>
        /// <param name="storageController">アセットバンドルの貯蔵物を制御するストレージコントローラ</param>
        /// <param name="installer">アセットバンドルをストレージにインストールするインストーラ</param>
        /// <param name="manifestFetcher">マニフェストをフェッチするフェッチャー</param>
        /// <param name="storageDirectoryInfo">アセットマネージャが利用するディレクトリ情報</param>
        /// <param name="isSimulate">アセットバンドルロード時にシミュレートするか否か</param>
        /// <exception cref="ArgumentNullException">storageController が null です</exception>
        /// <exception cref="ArgumentNullException">installer が null です</exception>
        /// <exception cref="ArgumentNullException">manifestFetcher が null です</exception>
        /// <exception cref="ArgumentNullException">storageDirectoryInfo が null です</exception>
        public AssetManagementService(AssetBundleStorageController storageController, AssetBundleInstaller installer, AssetBundleManifestFetcher manifestFetcher, DirectoryInfo storageDirectoryInfo, AssetBundleLoadMode loadMode)
        {
            // storageがnullなら
            if (storageController == null)
            {
                // どこに貯蔵すれば良いのだ
                throw new ArgumentNullException(nameof(storageController));
            }


            // installerがnullなら
            if (installer == null)
            {
                // 何でインストールすればよいのだ
                throw new ArgumentNullException(nameof(installer));
            }


            // manifestFetcherがnullなら
            if (manifestFetcher == null)
            {
                // どうやって取得すればよいのだ
                throw new ArgumentNullException(nameof(manifestFetcher));
            }


            // storageDirectoryInfoがnullなら
            if (storageDirectoryInfo == null)
            {
                // どこで管理をすればよいのだ
                throw new ArgumentNullException(nameof(storageDirectoryInfo));
            }


            // ロードモードがローカルビルドロードなら
            if (loadMode == AssetBundleLoadMode.LocalBuild)
            {
                // システムによってロードするパスを設定してストレージマネージャとフェッチャを騙す
                if (SystemInfo.operatingSystemFamily == OperatingSystemFamily.Windows)
                {
                    // Windowsビルド時のローカルビルドパスを教える
                    storageDirectoryInfo = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, "AssetBundles/StandaloneWindows").Replace("\\", "/"));
                    manifestFetcher = new ImtEditorAssetBundleManifestFetcher(Path.Combine(storageDirectoryInfo.FullName, "StandaloneWindows"));
                }
                else if (SystemInfo.operatingSystemFamily == OperatingSystemFamily.MacOSX)
                {
                    // Macビルド時のローカルビルドパスを教える
                    storageDirectoryInfo = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, "AssetBundles/StandaloneOSX").Replace("\\", "/"));
                    manifestFetcher = new ImtEditorAssetBundleManifestFetcher(Path.Combine(storageDirectoryInfo.FullName, "StandaloneOSX"));
                }
            }


            // サブシステムなどの初期化をする
            uriCache = new UriInfoCache();
            assetCache = new UnityAssetCache();
            manifestManager = new AssetBundleManifestManager(manifestFetcher, storageDirectoryInfo);
            storageManager = new AssetBundleStorageManager(manifestManager, storageController, installer);
            this.loadMode = loadMode;
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
            else if ((loadMode == AssetBundleLoadMode.Native || loadMode == AssetBundleLoadMode.LocalBuild) && storageName == AssetBundleHostName)
            {
                // Resourcesでないならアセットバンドル側からロードする
                asset = await LoadAssetBundleAssetAsync<T>(storageName, uriInfo, progress);
            }
            else if (loadMode == AssetBundleLoadMode.Simulate && storageName == AssetBundleHostName)
            {
                // Unityプロジェクトからロードする
                asset = await LoadProjectAssetAsync<T>(uriInfo, progress);
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
            else if (typeof(T) == typeof(SceneAsset))
            {
                // シーンアセット型のロード要求なら、素直にLoadSceneしても良いとしてnull結果を返す
                result = null;
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
            // ロードするアセット名を取得するが見つけられなかったら（ただし、シーン型である場合は例外として未定義を許可する）
            var assetPath = default(string);
            if (!assetUrl.QueryTable.TryGetValue(AssetNameQueryName, out assetPath) && (typeof(T) != typeof(SceneAsset)))
            {
                // ロードするアセット名が不明である例外を吐く
                throw new InvalidOperationException($"アセットバンドルからロードするべきアセット名を取得出来ませんでした。クエリに '{AssetNameQueryName}' パラメータがあることを確認してください。");
            }


            // ローカルパスを取得してアセットバンドルを開く
            var localPath = assetUrl.Uri.LocalPath.TrimStart('/');
            AssetBundleInfo assetBundleInfo;
            manifestManager.GetAssetBundleInfo(localPath, out assetBundleInfo);
            var assetBundle = await storageManager.OpenAsync(assetBundleInfo);


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
            else if (typeof(T) == typeof(SceneAsset))
            {
                // もしシーンアセット型のロード要求ならシーンアセット型のインスタンスを生成する
                result = (T)(UnityEngine.Object)ScriptableObject.CreateInstance<SceneAsset>();
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


        #region Simulate Load
        /// <summary>
        /// Unityプロジェクトから非同期にアセットのロードを行います。
        /// また、この関数はエディタ環境以外では動作しないことに注意して下さい。
        /// エディタ以外で利用しようとすると例外をスローします。
        /// </summary>
        /// <typeparam name="T">ロードするアセットの型</typeparam>
        /// <param name="assetUrl">ロードするアセットURL</param>
        /// <param name="progress">ロードの進捗通知を受ける IProgress</param>
        /// <returns>ロードに成功した場合は有効なアセットの参照をかえします。ロードに失敗した場合は null を返します。</returns>
        /// <exception cref="InvalidOperationException">ロードするべきアセット名を取得出来ませんでした。クエリに 'name' パラメータがあることを確認してください。</exception>
        /// <exception cref="InvalidOperationException">このロード関数はエディタ以外では動作しません</exception>
        private Task<T> LoadProjectAssetAsync<T>(UriInfo assetUrl, IProgress<float> progress) where T : UnityEngine.Object
        {
#if UNITY_EDITOR
            // ロードするアセット名を取得するが見つけられなかったら（ただし、シーン型である場合は例外として未定義を許可する）
            var assetPath = default(string);
            if (!assetUrl.QueryTable.TryGetValue(AssetNameQueryName, out assetPath) && (typeof(T) != typeof(SceneAsset)))
            {
                // ロードするアセット名が不明である例外を吐く
                throw new InvalidOperationException($"アセットバンドルからロードするべきアセット名を取得出来ませんでした。クエリに '{AssetNameQueryName}' パラメータがあることを確認してください。");
            }


            // シーン型の読み込み以外 かつ "assets"から始まらないアセットパスなら
            if (typeof(T) != typeof(SceneAsset) && !assetPath.ToLower().StartsWith("assets"))
            {
                // アセットバンドルの依存アセットを取得する
                var dependenceAssetPaths = UnityEditor.AssetDatabase.GetAssetBundleDependencies(Path.GetFileName(assetUrl.Uri.LocalPath), true);
                foreach (var dependenceAssetPath in dependenceAssetPaths)
                {
                    // ファイル名が一致するなら
                    if (Path.GetFileName(dependenceAssetPath) == assetPath)
                    {
                        // このパスを採用してループ終了
                        assetPath = dependenceAssetPath;
                    }
                }
            }


            // 結果を納める変数宣言
            T result = default(T);


            // もしマルチスプライト型のロード要求なら
            if (typeof(T) == typeof(MultiSprite))
            {
                // 残念ながら非同期ロード関数がないのでここで同期読み込みをするが、ロードに失敗したら
                var sprites = Array.ConvertAll(UnityEditor.AssetDatabase.LoadAllAssetsAtPath(assetPath), x => (Sprite)x);
                if (sprites == null)
                {
                    // ロードが出来なかったということでnullを返す
                    return Task.FromResult(default(T));
                }


                // マルチスプライトアセットとしてインスタンスを生成して結果に納める
                result = (T)(UnityEngine.Object)new MultiSprite(sprites);
            }
            else if (typeof(T) == typeof(SceneAsset))
            {
                // もしシーンアセット型のロード要求ならシーンアセット型のインスタンスを生成する
                result = (T)(UnityEngine.Object)ScriptableObject.CreateInstance<SceneAsset>();
            }
            else
            {
                // 特定型ロードでなければ通常のロードを行う（もちろん非同期ロードはない）
                result = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(assetPath);
            }


            // 結果を返す
            return Task.FromResult(result);
#else
            throw new InvalidOperationException("このロード関数はエディタ以外では動作しません");
#endif
        }
        #endregion


        #region Manifest
        // TODO : 今はフル更新というひどい実装
        public async Task UpdateManifestAsync(IProgress<AssetBundleCheckProgress> progress)
        {
            // ネイティブロードモード以外は何もせず終了
            if (loadMode == AssetBundleLoadMode.Simulate) return;


            // マニフェストの更新を行う
            await manifestManager.LoadManifestAsync();
            var manifest = await manifestManager.FetchManifestAsync();
            var updatableList = await manifestManager.GetUpdatableAssetBundlesAsync(manifest, progress);
            if (updatableList.Length > 0)
            {
                await manifestManager.UpdateManifestAsync(manifest);
            }
        }


        // TODO : 今はフルダウンロードというひどい実装
        public async Task InstallAssetBundleAsync(IProgress<AssetBundleInstallProgress> progress)
        {
            // ネイティブロードモード以外は何もせず終了
            if (loadMode != AssetBundleLoadMode.Native) return;
            await storageManager.InstallAllAsync(progress);
        }
        #endregion
    }
}