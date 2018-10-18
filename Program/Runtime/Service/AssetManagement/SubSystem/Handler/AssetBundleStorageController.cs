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
using UnityEngine;

namespace IceMilkTea.Service
{
    /// <summary>
    /// ストレージに貯蔵されているアセットバンドルの入出力などを行う抽象クラスです
    /// </summary>
    public abstract class AssetBundleStorageController
    {
        /// <summary>
        /// 指定されたアセットバンドル情報から、アセットバンドルが存在するかを確認します
        /// </summary>
        /// <param name="info">確認するアセットバンドル情報</param>
        /// <returns>存在することが確認できた場合は true を、存在を確認出来なかった場合は false を返します</returns>
        public abstract bool Exists(AssetBundleInfo info);


        /// <summary>
        /// 指定されたアセットバンドル情報のアセットバンドルに、インストールするためのストリームを非同期で取得します
        /// </summary>
        /// <param name="info">インストールするアセットバンドルの情報</param>
        /// <returns>指定したアセットバンドルに書き込むためのストリームを取得するタスクを返します</returns>
        public abstract Task<Stream> GetInstallStreamAsync(AssetBundleInfo info);


        /// <summary>
        /// 指定されたアセットバンドル情報のアセットバンドルを、非同期に削除します
        /// </summary>
        /// <param name="info">削除するアセットバンドルの情報</param>
        /// <returns>アセットバンドルの削除を非同期に操作しているタスクを返します</returns>
        public abstract Task RemoveAsync(AssetBundleInfo info);


        /// <summary>
        /// AssetBundleStorageController が管理しているアセットバンドル全てを非同期で削除します
        /// </summary>
        /// <param name="progress">削除の進捗通知を受ける Progress。もし通知を受けない場合は null の指定が可能です</param>
        /// <returns>アセットバンドルの削除を非同期で行っているタスクを返します</returns>
        public abstract Task RemoveAllAsync(IProgress<double> progress);


        /// <summary>
        /// 指定されたアセットバンドル情報のアセットバンドルを非同期で開きます
        /// </summary>
        /// <param name="info">アセットバンドルとして開くアセットバンドル情報</param>
        /// <returns>アセットバンドルを非同期で開くタスクを返します</returns>
        public abstract Task<AssetBundle> OpenAsync(AssetBundleInfo info);


        /// <summary>
        /// 指定されたアセットバンドルを閉じます
        /// </summary>
        /// <param name="assetBundle">閉じるアセットバンドル</param>
        public abstract void Close(AssetBundle assetBundle);
    }
}