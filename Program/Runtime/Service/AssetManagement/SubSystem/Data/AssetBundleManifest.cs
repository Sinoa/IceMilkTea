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

namespace IceMilkTea.Service
{
    /// <summary>
    /// 1つ以上のアセットバンドル情報を保持した構造体です。
    /// アセットバンドルの管理情報なども一部含みます。
    /// </summary>
    [System.Serializable]
    public struct ImtAssetBundleManifest
    {
        /// <summary>
        /// マニフェスト名
        /// </summary>
        public string Name;


        /// <summary>
        /// アセットバンドルをフェッチする元になるURL
        /// </summary>
        public string FetchBaseUrl;


        /// <summary>
        /// マニフェストが保持しているアセットバンドル情報の配列
        /// </summary>
        public AssetBundleInfo[] AssetBundleInfos;



        /// <summary>
        /// マニフェストが保持している、アセットバンドル情報全てのサイズの合計値
        /// </summary>
        public long TotalAssetBundleSize
        {
            get
            {
                // 保持している情報の数分ループ
                var totalSize = 0L;
                for (int i = 0; i < AssetBundleInfos.Length; ++i)
                {
                    // サイズを加算
                    totalSize += AssetBundleInfos[i].Size;
                }


                // 結果を返す
                return totalSize;
            }
        }
    }
}