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
    /// Webからアセットバンドルマニフェストを取得するフェッチャークラスです
    /// </summary>
    public class ImtWebAssetBundleManifestFetcher : AssetBundleManifestFetcher
    {
        // メンバ変数定義
        private WebDownloader webDownloader;
        private Uri manifestUrl;



        /// <summary>
        /// ImtWebAssetBundleManifestFetcher のインスタンスを初期化します
        /// </summary>
        /// <param name="manifestUrl">取得元になるマニフェストURL</param>
        /// <exception cref="ArgumentNullException">manifestUrl が null です</exception>
        /// <exception cref="NotSupportedException">アセットバンドルマニフェストのフェッチURLにサポートしない '{manifestUrl.Scheme}' スキームが指定されました。サポートしているスキームは http および https です</exception>
        public ImtWebAssetBundleManifestFetcher(Uri manifestUrl)
        {
            // null を渡されたら
            if (manifestUrl == null)
            {
                // どこから取得すればよいのだ
                throw new ArgumentNullException(nameof(manifestUrl));
            }


            // スキームがHTTP系以外なら
            if (!manifestUrl.IsHttpScheme())
            {
                // HTTP系以外は受け付けない
                throw new NotSupportedException($"アセットバンドルマニフェストのフェッチURLにサポートしない '{manifestUrl.Scheme}' スキームが指定されました。サポートしているスキームは http および https です");
            }


            // URLを覚えてWebDownloaderを生成
            this.manifestUrl = manifestUrl;
            webDownloader = new WebDownloader();
        }


        /// <summary>
        /// マニフェストの取得を非同期で行います
        /// </summary>
        /// <returns>マニフェストの取得を非同期で行っているタスクを返します</returns>
        public override Task<ImtAssetBundleManifest> FetchAsync()
        {
            // 非同期でマニフェストデータをダウンロードしてデシリアライズするタスクを生成して返す
            return Task.Run(async () =>
            {
                // ダウンロードの受け皿としてメモリストリームを用意して非同期ダウンロードをする
                var saveStream = new MemoryStream();
                await webDownloader.DownloadAsync(manifestUrl, saveStream, null);


                // メモリストリームからストリームリーダーを生成する
                saveStream.Seek(0, SeekOrigin.Begin);
                using (var streamReader = new StreamReader(saveStream))
                {
                    // 全ての文字列を読み込んでJsonデシリアライズを行い返す
                    return JsonUtility.FromJson<ImtAssetBundleManifest>(streamReader.ReadToEnd());
                }
            });
        }
    }
}