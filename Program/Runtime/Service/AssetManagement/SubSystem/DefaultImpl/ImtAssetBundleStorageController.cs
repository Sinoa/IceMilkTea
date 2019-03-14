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
using IceMilkTea.Core;
using UnityEngine;

namespace IceMilkTea.Service
{
    /// <summary>
    /// IceMilkTeaが提供する単純なアセットバンドルストレージクラスです
    /// </summary>
    public class ImtAssetBundleStorageController : AssetBundleStorageController
    {
        // メンバ変数定義
        private DirectoryInfo baseDirectoryInfo;



        /// <summary>
        /// ImtDefaultAssetBundleStorage のインスタンスを初期化します
        /// </summary>
        /// <param name="baseDirectoryPath">アセットバンドルを格納するベースディレクトリパス</param>
        /// <exception cref="ArgumentNullException">baseDirectoryPath が null です</exception>
        /// <exception cref="ArgumentException">アセットバンドルストレージディレクトリパスに利用できない文字が含まれています</exception>
        public ImtAssetBundleStorageController(string baseDirectoryPath)
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
        }



        /// <summary>
        /// 指定されたアセットバンドル情報のアセットバンドルが存在するかを確認します
        /// </summary>
        /// <param name="info">確認するアセットバンドル情報</param>
        /// <returns>存在する場合は true を、存在しない場合は false を返します</returns>
        public override bool Exists(AssetBundleInfo info)
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


            // アセットバンドルの最終的なパスを生成してファイル情報インスタンスを生成して、ディレクトリが存在しないなら
            var assetBundleFileInfo = new FileInfo(Path.Combine(baseDirectoryInfo.FullName, info.LocalPath));
            if (!assetBundleFileInfo.Directory.Exists)
            {
                // サブディレクトリの生成もやる
                assetBundleFileInfo.Directory.Create();
            }


            // ベースディエクトリパスからアセットバンドルへのパスを作ってファイルストリームを返す
            return Task.FromResult((Stream)new FileStream(assetBundleFileInfo.FullName, FileMode.Create, FileAccess.ReadWrite));
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
            if (!Exists(info))
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
        /// <param name="info">開くアセットバンドル情報</param>
        /// <returns>アセットバンドルを非同期で開くタスクを返します</returns>
        /// <exception cref="ArgumentNullException">localPath が null です</exception>
        /// <exception cref="FileNotFoundException">指定されたパスにアセットバンドルが存在しません</exception>
        /// <exception cref="InvalidOperationException">アセットバンドル '{assetBundlePath}' を開くことが出来ませんでした</exception>
        public override async Task<AssetBundle> OpenAsync(AssetBundleInfo info)
        {
            // パスを作る
            var assetBundlePath = Path.Combine(baseDirectoryInfo.FullName, info.LocalPath);


            // ディレクトリ情報を更新する
            baseDirectoryInfo.Refresh();


            // そもそもベースディレクトリが存在しない または ファイルが存在していないなら
            if (!baseDirectoryInfo.Exists || !File.Exists(assetBundlePath))
            {
                // アセットバンドルが存在しない例外を吐く
                throw new FileNotFoundException("指定されたパスにアセットバンドルが存在しません", assetBundlePath);
            }


            // 指定されたパスのアセットバンドルを非同期で開くが開けなかったら
            var assetBundle = await AssetBundle.LoadFromFileAsync(assetBundlePath);
            if (assetBundle == null)
            {
                // アセットバンドルが開けなかったことを例外で吐く
                throw new InvalidOperationException($"アセットバンドル '{assetBundlePath}' を開くことが出来ませんでした");
            }


            // 開いたアセットバンドルを返す
            return assetBundle;
        }


        /// <summary>
        /// 指定されたローカルパスのアセットバンドルを閉じます
        /// </summary>
        /// <param name="assetBundle">閉じるアセットバンドル</param>
        /// <exception cref="ArgumentNullException">localPath が null です</exception>
        public override void Close(AssetBundle assetBundle)
        {
            // 素直に綴る
            assetBundle.Unload(false);
        }


        /// <summary>
        /// 指定されたアセットバンドルのベリファイを行います
        /// </summary>
        /// <param name="info">ベリファイを行うアセットバンドル情報</param>
        /// <param name="progress">ベリファイの進捗通知を受ける Progress</param>
        /// <returns>ベリファイをパスした場合は true を、パスしなかった場合は false を返すタスクを返します</returns>
        public override Task<bool> VerifyAsync(AssetBundleInfo info, IProgress<double> progress)
        {
            // パスを作る
            var assetBundlePath = Path.Combine(baseDirectoryInfo.FullName, info.LocalPath);


            // ディレクトリ情報を更新する
            baseDirectoryInfo.Refresh();


            // そもそもベースディレクトリが存在しない または ファイルが存在していないなら
            if (!baseDirectoryInfo.Exists || !File.Exists(assetBundlePath))
            {
                // ここはパスしなかったことを返す
                return Task.FromResult(false);
            }


            // ハッシュを非同期で計算するタスクを生成して返す
            return Task.Run(() =>
            {
                // ここでハッシュを生成する（Initializeも考えたけど、よくよく考えたらメモリに残しっぱはまずいかな）
                var hash = new SHA1CryptoServiceProvider();


                // ハッシュ計算対象のファイルを開く
                using (var fileStream = new FileStream(assetBundlePath, FileMode.Open, FileAccess.Read))
                {
                    // バッファを用意して最初に読み込む
                    // TODO : 将来的にはスタック上で処理したい
                    var buffer = new byte[512];
                    var readSize = fileStream.Read(buffer, 0, buffer.Length);


                    // ファイルポインタ位置が末尾まで来ていない間ループ
                    while (fileStream.Position < fileStream.Length)
                    {
                        // ハッシュの途中計算をしてから続きの読み込みを行う
                        hash.TransformBlock(buffer, 0, readSize, buffer, 0);
                        readSize = fileStream.Read(buffer, 0, buffer.Length);


                        // 進捗通知をする
                        progress?.Report((double)fileStream.Position / fileStream.Length);
                    }


                    // バッファの残りを最終ハッシュ計算をする
                    hash.TransformFinalBlock(buffer, 0, readSize);
                    progress?.Report((double)fileStream.Position / fileStream.Length);
                }


                // ハッシュ結果を受け取って、もしアセットバンドル情報のハッシュ長が異なるなら
                var hashResult = hash.Hash;
                if (hashResult.Length != info.Hash.Length)
                {
                    // ハッシュが一致しないことを返す
                    return false;
                }


                // ハッシュの長さ分ループ
                for (int i = 0; i < hashResult.Length; ++i)
                {
                    // もし値が異なるなら
                    if (hashResult[i] != info.Hash[i])
                    {
                        // ハッシュが一致しないことを返す
                        return false;
                    }
                }


                // ここまで到達したということはハッシュはパスしたということになる
                return true;
            });
        }
    }
}