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

namespace IceMilkTea.SubSystem
{
    /// <summary>
    /// ImtStorage によるURIクラスの拡張関数実装用クラスです
    /// </summary>
    public static class ImtStorageUriExtensions
    {
        // 定数定義
        public const string SchemeAssetBundle = "assetbundle";
        public const string SchemeAssetCatalog = "assetcatalog";
        public const string SchemeAssetDatabase = "assetdatabase";
        public const string SchemeStreammingAssets = "streammingassets";
        public const string SchemeResources = "resources";
        public const string SchemeTemporary = "temporary";



        /// <summary>
        /// URIが AssetBundle スキームかどうか
        /// </summary>
        /// <param name="uri">確認するURI</param>
        /// <returns>assetbundle スキームの場合は true を、違う場合は false を返します</returns>
        public static bool IsAssetBundle(this Uri uri)
        {
            // URIが絶対パスのときのみAssetBundleスキームかどうかを返す
            return uri.IsAbsoluteUri ? uri.Scheme == SchemeAssetBundle : false;
        }


        /// <summary>
        /// URIが AssetCatalog スキームかどうか
        /// </summary>
        /// <param name="uri">確認するURI</param>
        /// <returns>assetcatalog スキームの場合は true を、違う場合は false を返します</returns>
        public static bool IsAssetCatalog(this Uri uri)
        {
            // URIが絶対パスのときのみAssetCatalogスキームかどうかを返す
            return uri.IsAbsoluteUri ? uri.Scheme == SchemeAssetCatalog : false;
        }


        /// <summary>
        /// URIが AssetDatabase スキームかどうか
        /// </summary>
        /// <param name="uri">確認するURI</param>
        /// <returns>assetdatabase スキームの場合は true を、違う場合は false を返します</returns>
        public static bool IsAssetDatabase(this Uri uri)
        {
            // URIが絶対パスのときのみAssetDatabaseスキームかどうかを返す
            return uri.IsAbsoluteUri ? uri.Scheme == SchemeAssetDatabase : false;
        }


        /// <summary>
        /// URIが StreammingAssets スキームかどうか
        /// </summary>
        /// <param name="uri">確認するURI</param>
        /// <returns>streammingassets スキームの場合は true を、違う場合は false を返します</returns>
        public static bool IsStreammingAssets(this Uri uri)
        {
            // URIが絶対パスのときのみStreammingAssetsスキームかどうかを返す
            return uri.IsAbsoluteUri ? uri.Scheme == SchemeStreammingAssets : false;
        }


        /// <summary>
        /// URIが Resources スキームかどうか
        /// </summary>
        /// <param name="uri">確認するURI</param>
        /// <returns>resources スキームの場合は true を、違う場合は false を返します</returns>
        public static bool IsResources(this Uri uri)
        {
            // URIが絶対パスのときのみResourcesスキームかどうかを返す
            return uri.IsAbsoluteUri ? uri.Scheme == SchemeResources : false;
        }


        /// <summary>
        /// URIが Temporary スキームかどうか
        /// </summary>
        /// <param name="uri">確認するURI</param>
        /// <returns>temporary スキームの場合は true を、違う場合は false を返します</returns>
        public static bool IsTemporary(this Uri uri)
        {
            // URIが絶対パスのときのみTemporaryスキームかどうかを返す
            return uri.IsAbsoluteUri ? uri.Scheme == SchemeTemporary : false;
        }
    }
}