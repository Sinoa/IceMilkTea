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

namespace IceMilkTea.Service
{
    /// <summary>
    /// AssetManagementService にてサービスの操作に関するイベントを聞き取るインターフェイスです
    /// </summary>
    public interface IAssetManagementEventListener
    {
        /// <summary>
        /// アセットが新たにキャッシュされた時に呼び出されます
        /// </summary>
        /// <param name="assetUri">キャッシュされたアセットのURI</param>
        void OnNewAssetCached(Uri assetUri);

        /// <summary>
        /// アセットがキャッシュテーブルからパージされた時に呼び出されます
        /// </summary>
        /// <param name="assetUri">パージされたアセットのURI</param>
        void OnPurgeAssetCache(Uri assetUri);
    }
}