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
using System.Text;
using IceMilkTea.Core;
using UnityEngine;

namespace IceMilkTea.Service
{
    // Unityのアセットの基底型になるObjectは、SystemのObjectとややこしくなるのでここではUnityAssetと名付ける（WeakReference版も定義）
    using UnityAsset = UnityEngine.Object;
    using WeakUnityAsset = WeakReference<UnityEngine.Object>;


    /// <summary>
    /// Unityのゲームアセットを読み込む機能を提供するサービスクラスです
    /// </summary>
    public class AssetLoadService : GameService
    {
        private Crc64Base crc;
        private Encoding utf8;
        private Dictionary<ulong, AssetBundle> assetBundleCache;
        private Dictionary<ulong, WeakUnityAsset> assetCacheTable;



        public IAwaitable<UnityAsset> LoadAsync()
        {
            return new ImtTask<UnityAsset>(_ => null);
        }
    }



    internal class AssetBundleContainer
    {
    }



    internal class AssetCacheController
    {
    }
}