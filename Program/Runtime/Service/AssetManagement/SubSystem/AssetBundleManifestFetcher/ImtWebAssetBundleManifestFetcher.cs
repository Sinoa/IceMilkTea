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

namespace IceMilkTea.Service
{
    /// <summary>
    /// Webからマニフェストをフェッチする、単純なフェッチャークラスです
    /// </summary>
    public class ImtWebAssetBundleManifestFetcher : AssetBundleManifestFetcher
    {
        // 公開定数定義
        public const int DefaultTimeoutTime = 5000;
        public const int DefaultRetryCount = 2;

        // 非公開定数定義
        private const int ReceiveBufferSize = 1 << 10;
        private const int InitialRetryWaitTime = 500;

        // メンバ変数定義
        private Uri baseUrl;
        private int timeoutTime;
        private int retryCount;
        private byte[] receiveBuffer;



        /// <summary>
        /// ImtWebAssetBundleManifestFetcher のインスタンスを既定の値で初期化します
        /// </summary>
        /// <param name="baseUrl">フェッチするマニフェストが存在するWebサービスのベースURL</param>
        /// <exception cref="ArgumentNullException">baseUrl が null です</exception>
        /// <exception cref="ArgumentException">スキームがHTTPではありません、扱えるスキームは http または https です</exception>
        public ImtWebAssetBundleManifestFetcher(Uri baseUrl) : this(baseUrl, DefaultTimeoutTime, DefaultRetryCount)
        {
        }


        /// <summary>
        /// ImtWebAssetBundleManifestFetcher のインスタンスを既定の値で初期化します
        /// </summary>
        /// <param name="baseUrl">フェッチするマニフェストが存在するWebサービスのベースURL</param>
        /// <param name="timeoutTime">タイムアウトするまでの時間（ミリ秒）</param>
        /// <param name="retryCount">最大リトライ回数</param>
        /// <exception cref="ArgumentNullException">baseUrl が null です</exception>
        /// <exception cref="ArgumentException">スキームがHTTPではありません、扱えるスキームは http または https です</exception>
        /// <exception cref="ArgumentOutOfRangeException">timeoutTime は 0 以下の値を設定することが出来ません</exception>
        /// <exception cref="ArgumentOutOfRangeException">retryCount は 0 未満の値を設定することが出来ません</exception>
        public ImtWebAssetBundleManifestFetcher(Uri baseUrl, int timeoutTime, int retryCount)
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


            // タイムアウトの時間が0以下なら
            if (timeoutTime <= 0)
            {
                // 流石に 0 以下は無理
                throw new ArgumentOutOfRangeException(nameof(timeoutTime), $"{nameof(timeoutTime)} は 0 以下の値を設定することが出来ません");
            }


            // リトライカウントが0未満なら
            if (retryCount < 0)
            {
                // リトライしないならいけるが負の値は無理
                throw new ArgumentOutOfRangeException(nameof(retryCount), $"{nameof(retryCount)} は 0 未満の値を設定することが出来ません");
            }


            // 値を受け取って覚える
            this.baseUrl = baseUrl;
            this.timeoutTime = timeoutTime;
            this.retryCount = retryCount;


            // 受信バッファを作っておく
            receiveBuffer = new byte[ReceiveBufferSize];
        }


        public override Task<ImtAssetBundleManifest[]> FetchManifestAsync()
        {
            throw new System.NotImplementedException();
        }
    }
}