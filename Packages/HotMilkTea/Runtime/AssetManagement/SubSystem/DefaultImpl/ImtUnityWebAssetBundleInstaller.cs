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
using UnityEngine.Networking;

namespace IceMilkTea.Service
{
    /// <summary>
    /// UnityWebRequest を用いたアセットバンドルインストーラクラスです
    /// </summary>
    public class ImtUnityWebAssetBundleInstaller : AssetBundleInstaller
    {
        // 定数定義
        private const int ReceiveBufferSize = 32 << 10;

        // メンバ変数定義
        private byte[] receiveBuffer;
        private Uri baseUrl;



        /// <summary>
        /// ImtUnityWebAssetBundleInstaller のインスタンスを既定の値で初期化します
        /// </summary>
        /// <param name="baseUrl">インストールするアセットバンドルが存在するWebサービスのベースURL</param>
        /// <exception cref="ArgumentNullException">baseUrl が null です</exception>
        /// <exception cref="ArgumentException">スキームがHTTPではありません、扱えるスキームは http または https です</exception>
        public ImtUnityWebAssetBundleInstaller(Uri baseUrl)
        {
            // nullが渡されたら
            if (baseUrl == null)
            {
                // どこからダウンロードすればよいのか
                throw new ArgumentNullException(nameof(baseUrl));
            }


            // スキームがHTTP系じゃないなら
            if (!baseUrl.IsHttpScheme())
            {
                // http系のスキームを要求する例外を吐く
                throw new ArgumentException("スキームがHTTPではありません、扱えるスキームは http または https です", nameof(baseUrl));
            }


            // 値を受け取って受信バッファを生成する
            this.baseUrl = baseUrl;
            receiveBuffer = new byte[ReceiveBufferSize];
        }


        /// <summary>
        /// 指定されたアセットバンドル情報のアセットバンドルを非同期でインストールします
        /// </summary>
        /// <param name="info">インストールするアセットバンドル情報</param>
        /// <param name="installStream">インストールする先のストリーム</param>
        /// <param name="progress">インストール進捗通知を受ける IProgress 。不要の場合は null も指定可能です。</param>
        /// <returns>アセットバンドルの非同期インストールしているタスクを返します</returns>
        /// <exception cref="ArgumentNullException">installStream が null です</exception>
        public override async Task InstallAsync(AssetBundleInfo info, Stream installStream, IProgress<double> progress)
        {
            // ダウンロードする元のURLをつくってHttpWebRequestを生成する（念の為タイムスタンプクエリ的なものでキャッシュを防ぐ）
            var targetUrl = new Uri(baseUrl, info.RemotePath + $"?timestamp={DateTimeOffset.Now.ToUnixTimeMilliseconds()}");


            // UnityWebRequestを生成してダウンロードハンドラを設定する
            var webRequest = UnityWebRequest.Get(targetUrl.ToString());
            webRequest.downloadHandler = new DownloadHandlerStream(installStream, receiveBuffer);


            // ダウンロードを行う
            await webRequest.SendWebRequest().ToAwaitable(new Progress<float>(x => progress?.Report(x)));
        }
    }
}