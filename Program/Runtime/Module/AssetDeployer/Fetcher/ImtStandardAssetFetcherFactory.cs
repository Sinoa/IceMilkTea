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
using System.IO;

namespace IceMilkTea.Module
{
    /// <summary>
    /// IceMilkTea 標準アセットフェッチャファクトリクラスです
    /// </summary>
    public class ImtStandardAssetFetcherFactory : IAssetFetcherFactory
    {
        /// <summary>
        /// フェッチするアセットURIからアセットフェッチャのインスタンスを生成します
        /// </summary>
        /// <param name="assetUri">フェッチする元になるアセットURI</param>
        /// <returns>生成されたアセットフェッチャのインスタンスを返します</returns>
        /// <exception cref="NotSupportedException">指定されたアセットURIのスキーム '{scheme}' はサポートしていません。</exception>
        public IAssetFetcher CreateFetcher(Uri assetUri)
        {
            // HTTP、HTTPSスキームの場合
            var scheme = assetUri.Scheme;
            if (scheme == Uri.UriSchemeHttp || scheme == Uri.UriSchemeHttps)
            {
                // HTTP向けアセットフェッチャを生成して返す
                return new HttpAssetFetcher(assetUri);
            }
            else if (scheme == Uri.UriSchemeFile)
            {
                // FILEスキームの場合ならファイルアセットフェッチャを生成して返す
                return new FileAssetFetcher(new FileInfo(assetUri.LocalPath));
            }


            // 他のスキームを指定された場合は非サポート例外を吐く
            throw new NotSupportedException($"指定されたアセットURIのスキーム '{scheme}' はサポートしていません。");
        }
    }
}