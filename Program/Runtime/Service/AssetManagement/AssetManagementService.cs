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

using IceMilkTea.Core;

namespace IceMilkTea.Service
{
    /// <summary>
    /// ゲームアセットの読み込み、取得、管理を総合的に管理をするサービスクラスです
    /// </summary>
    public class AssetManagementService : GameService
    {
        // メンバ変数定義
        private UrlCache urlCache;
        private UnityAssetCache assetCache;
        private AssetBundleCache assetBundleCache;



        /// <summary>
        /// AssetManagementService のインスタンスを初期化します
        /// </summary>
        public AssetManagementService()
        {
            // サブシステムなどの初期化をする
            urlCache = new UrlCache();
            assetCache = new UnityAssetCache();
            assetBundleCache = new AssetBundleCache();
        }


        // コンパイルエラーが出ないようにするための雑対応
        public System.Threading.Tasks.Task<T> LoadAssetAsync<T>(string name) where T : UnityEngine.Object
        {
            throw new System.NotImplementedException();
        }
    }
}