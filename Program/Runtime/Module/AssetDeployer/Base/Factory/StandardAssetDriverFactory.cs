// zlib/libpng License
//
// Copyright (c) 2019 Sinoa
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

namespace IceMilkTea.Module
{
    /// <summary>
    /// 標準的なアセットドライバのファクトリクラスです
    /// </summary>
    public class StandardAssetDriverFactory : IAssetDriverFactory
    {
        // クラス変数宣言
        private static readonly Dictionary<string, Func<Uri, IAssetFetchDriver>> FetchDriverFactoryTable;
        private static readonly Dictionary<string, Func<Uri, IAssetDeployDriver>> DeployDriverFactoryTable;



        /// <summary>
        /// StandardAssetDriverFactory クラスの初期化をします
        /// </summary>
        static StandardAssetDriverFactory()
        {
            // フェッチドライバファクトリテーブルの初期化
            FetchDriverFactoryTable = new Dictionary<string, Func<Uri, IAssetFetchDriver>>()
            {
                { Uri.UriSchemeHttp, uri => new HttpAssetFetchDriver(uri) },
                { Uri.UriSchemeHttps, uri => new HttpAssetFetchDriver(uri) },
            };


            // デプロイドライバファクトリテーブルの初期化
            DeployDriverFactoryTable = new Dictionary<string, Func<Uri, IAssetDeployDriver>>()
            {
                { Uri.UriSchemeFile, uri => new FileAssetDeployDriver(uri.LocalPath) }
            };
        }


        /// <summary>
        /// フェッチするアセットの情報からフェッチドライバを生成します
        /// </summary>
        /// <param name="info">フェッチドライバを生成するためのフェッチ情報</param>
        /// <returns>生成されたフェッチドライバを返します</returns>
        /// <exception cref="ArgumentNullException">info が null です</exception>
        public IAssetFetchDriver CreateFetchDriver(Uri assetUri)
        {
            // スキームを取得して対応するフェッチドライバを生成して返す
            if (FetchDriverFactoryTable.TryGetValue(assetUri.Scheme, out var create))
            {
                // ドライバを生成して返す
                return create(assetUri);
            }


            // 対応するスキームの生成関数が無いなら例外を吐く
            throw new NotSupportedException($"スキーム '{assetUri.Scheme}' は対応したドライバを生成出来ません");
        }


        /// <summary>
        /// デプロイするアセットの情報からデプロイドライバを生成します
        /// </summary>
        /// <param name="info">デプロイドライバを生成するためのデプロイ情報</param>
        /// <returns>生成されたデプロイドライバを返します</returns>
        /// <exception cref="ArgumentNullException">info が null です</exception>
        public IAssetDeployDriver CreateDeployDriver(Uri assetUri)
        {
            // スキームを取得して対応するデプロイドライバを生成して返す
            if (DeployDriverFactoryTable.TryGetValue(assetUri.Scheme, out var create))
            {
                // ドライバを生成して返す
                return create(assetUri);
            }


            // 対応するスキームの生成関数が無いなら例外を吐く
            throw new NotSupportedException($"スキーム '{assetUri.Scheme}' は対応したドライバを生成出来ません");
        }
    }
}