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
using System.Threading.Tasks;

namespace IceMilkTea.SubSystem
{
    /// <summary>
    /// ゲームアセットのデプロイを制御する抽象クラスです
    /// </summary>
    public abstract class AssetDeployer
    {
        /// <summary>
        /// 指定された名前のアセットが存在するかどうかを確認します
        /// </summary>
        /// <param name="name">確認したいアセット名</param>
        /// <returns>存在する場合は true を、存在しない場合は false を返します</returns>
        public abstract bool AssetExists(string name);


        /// <summary>
        /// 現在のアセットカタログの情報を元に、すべてのアセットの更新を非同期で行います。
        /// </summary>
        /// <returns>アセットの更新をしているタスクを返します</returns>
        public abstract Task UpdateAssetAllAsync();


        /// <summary>
        /// 指定されたアセット名の更新を非同期で行います
        /// </summary>
        /// <param name="name">更新するアセットの名前</param>
        /// <returns>アセットの更新をしているタスクを返します</returns>
        public abstract Task UpdateAssetAsync(string name);


        /// <summary>
        /// 指定されたアセット名の削除をします
        /// </summary>
        /// <param name="name">削除するアセット名</param>
        public abstract void DeleteAsset(string name);


        /// <summary>
        /// カタログを含むすべてのアセットを削除します
        /// </summary>
        public abstract void DeleteAll();


        #region 生成関数
        /// <summary>
        /// リモートURIからデータをフェッチするフェッチャのインスタンスを生成します
        /// </summary>
        /// <param name="remoteUri">フェッチする元になるリモートURI</param>
        /// <returns>生成されたフェッチャのインスタンスを返しますが、生成出来なかった場合は null を返します。</returns>
        protected virtual IFetcher CreateFetcher(Uri remoteUri)
        {
            // HTTP、HTTPSスキームの場合
            var scheme = remoteUri.Scheme;
            if (scheme == Uri.UriSchemeHttp || scheme == Uri.UriSchemeHttps)
            {
                // HTTP向けフェッチャを生成して返す
                return new HttpFetcher(remoteUri);
            }
            else if (scheme == Uri.UriSchemeFile)
            {
                // FILEスキームの場合ならファイルフェッチャを生成して返す
                return new FileFetcher(new FileInfo(remoteUri.LocalPath));
            }


            // 非サポートのスキームならnullを返す
            return null;
        }
        #endregion
    }
}