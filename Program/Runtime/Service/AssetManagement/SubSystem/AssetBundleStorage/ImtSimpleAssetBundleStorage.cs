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
using System.IO;
using System.Threading.Tasks;
using IceMilkTea.Core;
using UnityEngine;

namespace IceMilkTea.Service
{
    /// <summary>
    /// IceMilkTeaが提供する単純なアセットバンドルストレージクラスです
    /// </summary>
    public class ImtSimpleAssetBundleStorage : AssetBundleStorage
    {
        // メンバ変数定義
        private DirectoryInfo baseDirectoryInfo;
        private Dictionary<string, AssetBundle> assetBundleTable;



        /// <summary>
        /// ImtDefaultAssetBundleStorage のインスタンスを初期化します
        /// </summary>
        /// <param name="baseDirectoryPath">アセットバンドルを格納するベースディレクトリパス</param>
        /// <exception cref="ArgumentNullException">baseDirectoryPath が null です</exception>
        /// <exception cref="ArgumentException">アセットバンドルストレージディレクトリパスに利用できない文字が含まれています</exception>
        public ImtSimpleAssetBundleStorage(string baseDirectoryPath)
        {
            // もしnullを渡されたら
            if (baseDirectoryPath == null)
            {
                // nullは流石に受け入れられない
                throw new ArgumentNullException(nameof(baseDirectoryPath));
            }


            // パスに使えない文字が含まれていたら
            if (baseDirectoryPath.ContainInvalidPathChars())
            {
                // パスとして使えない文字が含まれていることを例外として吐く
                throw new ArgumentException($"アセットバンドルストレージディレクトリパスに利用できない文字が含まれています");
            }


            // ディレクトリ情報のインスタンスを生成する
            baseDirectoryInfo = new DirectoryInfo(baseDirectoryPath);
#if UNITY_IOS
            // iOSでのみ対象ディレクトリパス配下がバックアップされないように指示をする
            UnityEngine.iOS.Device.SetNoBackupFlag(baseDirectoryPath);
#endif


            // アセットバンドルテーブルを生成する
            assetBundleTable = new Dictionary<string, AssetBundle>();
        }



        /// <summary>
        /// 指定されたアセットバンドル情報のアセットバンドルが存在するかを確認します
        /// </summary>
        /// <param name="info">確認するアセットバンドル情報</param>
        /// <returns>存在する場合は true を、存在しない場合は false を返します</returns>
        public override bool Exists(ref AssetBundleInfo info)
        {
            // ディレクトリ情報を更新する
            baseDirectoryInfo.Refresh();


            // そもそもベースディレクトリパスが存在していないなら
            if (!baseDirectoryInfo.Exists)
            {
                // もちろんアセットバンドル自体もあるわけもなく
                return false;
            }


            // ベースディレクトリパスからアセットバンドルへのパスを作ってファイルが存在するかどうかの結果を返す
            var assetBundlePath = Path.Combine(baseDirectoryInfo.FullName, info.LocalPath);
            return File.Exists(assetBundlePath);
        }


        /// <summary>
        /// 指定されたアセットバンドル情報のアセットバンドルのインストールストリームを非同期で取得します
        /// </summary>
        /// <param name="info">インストールストリームとして取得するアセットバンドル情報</param>
        /// <returns>インストールストリームを非同期で取得するタスクを返します</returns>
        public override Task<Stream> GetInstallStreamAsync(AssetBundleInfo info)
        {
            // ディレクトリ情報を更新する
            baseDirectoryInfo.Refresh();


            // そもそもベースディレクトリが存在しないなら
            if (!baseDirectoryInfo.Exists)
            {
                // 作っておく
                baseDirectoryInfo.Create();
            }


            // ベースディエクトリパスからアセットバンドルへのパスを作ってファイルストリームを返す
            var assetBundlePath = Path.Combine(baseDirectoryInfo.FullName, info.LocalPath);
            return Task.FromResult((Stream)File.OpenWrite(assetBundlePath));
        }


        /// <summary>
        /// 管理下のアセットバンドル全てを非同期で削除します
        /// </summary>
        /// <param name="progress">削除の進捗通知を受ける IProgress</param>
        /// <returns>削除の非同期操作タスクを返します</returns>
        public override Task RemoveAllAsync(IProgress<double> progress)
        {
            // ディレクトリ情報を更新する
            baseDirectoryInfo.Refresh();


            // そもそも削除するベースディレクトリが存在しないなら
            if (!baseDirectoryInfo.Exists)
            {
                // 直ちに完了とする
                return Task.CompletedTask;
            }


            // ディレクトリまるごと削除する（もしディレクトリまるごと削除に時間がかかるならループ削除）
            baseDirectoryInfo.Delete(true);
            return Task.CompletedTask;
        }


        /// <summary>
        /// 指定されたアセットバンドル情報のアセットバンドルを削除する
        /// </summary>
        /// <param name="info">削除するアセットバンドル情報</param>
        /// <returns>削除の非同期操作タスクを返します</returns>
        public override Task RemoveAsync(AssetBundleInfo info)
        {
            // そもそもアセットバンドルが存在しないなら
            if (!Exists(ref info))
            {
                // 直ちに終了する
                return Task.CompletedTask;
            }


            // ベースディレクトリパスからアセットバンドルへのパスを作ってファイルの削除を行う
            var assetBundlePath = Path.Combine(baseDirectoryInfo.FullName, info.LocalPath);
            File.Delete(assetBundlePath);
            return Task.CompletedTask;
        }


        /// <summary>
        /// 指定されたローカルパスのアセットバンドルを非同期で開きます
        /// </summary>
        /// <param name="localPath">開くアセットバンドルパス</param>
        /// <returns>アセットバンドルを非同期で開くタスクを返します</returns>
        /// <exception cref="ArgumentNullException">localPath が null です</exception>
        /// <exception cref="FileNotFoundException">指定されたパスにアセットバンドルが存在しません</exception>
        /// <exception cref="InvalidOperationException">アセットバンドル '{assetBundlePath}' を開くことが出来ませんでした</exception>
        public override async Task<AssetBundle> OpenAsync(string localPath)
        {
            // null を渡されたら
            if (localPath == null)
            {
                // 何を開けばよいのか
                throw new ArgumentNullException(nameof(localPath));
            }


            // 既に開いた経験のあるアセットバンドルなら
            var assetBundle = default(AssetBundle);
            if (assetBundleTable.TryGetValue(localPath, out assetBundle))
            {
                // 直ちに返す
                return assetBundle;
            }


            // パスを作る
            var assetBundlePath = Path.Combine(baseDirectoryInfo.FullName, localPath);


            // ディレクトリ情報を更新する
            baseDirectoryInfo.Refresh();


            // そもそもベースディレクトリが存在しない または ファイルが存在していないなら
            if (!baseDirectoryInfo.Exists || !File.Exists(assetBundlePath))
            {
                // アセットバンドルが存在しない例外を吐く
                throw new FileNotFoundException("指定されたパスにアセットバンドルが存在しません", assetBundlePath);
            }


            // 指定されたパスのアセットバンドルを非同期で開くが開けなかったら
            assetBundle = await AssetBundle.LoadFromFileAsync(assetBundlePath);
            if (assetBundle == null)
            {
                // アセットバンドルが開けなかったことを例外で吐く
                throw new InvalidOperationException($"アセットバンドル '{assetBundlePath}' を開くことが出来ませんでした");
            }


            // アセットバンドルテーブルにキャッシュして返す
            return assetBundleTable[localPath] = assetBundle;
        }


        /// <summary>
        /// 指定されたローカルパスのアセットバンドルを閉じます
        /// </summary>
        /// <param name="localPath">閉じるアセットバンドルのパス</param>
        /// <exception cref="ArgumentNullException">localPath が null です</exception>
        public override void Close(string localPath)
        {
            // null を渡されたら
            if (localPath == null)
            {
                // 何を閉じればよいのか
                throw new ArgumentNullException(nameof(localPath));
            }


            // キャッシュされたアセットバンドルを取得を試みるが、取得出来なかったら
            var assetBundle = default(AssetBundle);
            if (!assetBundleTable.TryGetValue(localPath, out assetBundle))
            {
                // 何もせず終了
                return;
            }


            // アセットバンドルを閉じて該当のキーを削除
            assetBundle.Unload(false);
            assetBundleTable.Remove(localPath);
        }
    }
}