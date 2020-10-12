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
    /// ローカルファイルコピー を用いたアセットバンドルインストーラクラスです
    /// </summary>
    public class ImtEditorAssetBundleInstaller : AssetBundleInstaller
    {
        // メンバ変数定義
        private DirectoryInfo baseDirectoryInfo;



        /// <summary>
        /// ImtEditorAssetBundleInstaller のインスタンスを初期化します
        /// </summary>
        /// <param name="baseDirectoryInfo">インストールする元になるベースディレクトリ情報</param>
        /// <exception cref="ArgumentNullException">baseDirectoryInfo が null です</exception>
        public ImtEditorAssetBundleInstaller(DirectoryInfo baseDirectoryInfo)
        {
            // nullが渡されたら
            if (baseDirectoryInfo == null)
            {
                // どこからダウンロードすればよいのか
                throw new ArgumentNullException(nameof(baseDirectoryInfo));
            }


            // そのまま受け取る
            this.baseDirectoryInfo = baseDirectoryInfo;
        }


        /// <summary>
        /// 指定されたアセットバンドル情報のアセットバンドルを非同期でインストールします
        /// </summary>
        /// <param name="info">インストールするアセットバンドル情報</param>
        /// <param name="installStream">インストールする先のストリーム</param>
        /// <param name="progress">インストール進捗通知を受ける IProgress 。不要の場合は null も指定可能です。</param>
        /// <returns>アセットバンドルの非同期インストールしているタスクを返します</returns>
        /// <exception cref="ArgumentNullException">installStream が null です</exception>
        public override Task InstallAsync(AssetBundleInfo info, Stream installStream, IProgress<double> progress)
        {
            // installStreamがnullなら
            if (installStream == null)
            {
                // どこにインストールすればいいんじゃ
                throw new ArgumentNullException(nameof(installStream));
            }



            // ファイルをコピーする元のファイル情報を用意
            var targetFileInfo = new FileInfo(Path.Combine(baseDirectoryInfo.FullName, info.RemotePath));


            // ファイルを読み取りストリームで開く
            using (var fileStream = targetFileInfo.OpenRead())
            {
                // バッファを確保して一気に書き込む
                var buffer = new byte[1 << 10];
                for (int readSize = 0; (readSize = fileStream.Read(buffer, 0, buffer.Length)) > 0;)
                {
                    // 読み取ったサイズ分書き込む
                    installStream.Write(buffer, 0, readSize);
                }
            }


            // 完了タスクを返す
            return Task.CompletedTask;
        }
    }
}