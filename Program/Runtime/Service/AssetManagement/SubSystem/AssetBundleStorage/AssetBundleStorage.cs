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
    /// アセットバンドルを貯蔵するストレージ抽象クラスです
    /// </summary>
    public abstract class AssetBundleStorage
    {
        /// <summary>
        /// ストレージ名を取得します
        /// </summary>
        public abstract string StorageName { get; }



        /// <summary>
        /// 指定されたアセットバンドル情報のアセットバンドルが存在するか確認をします
        /// </summary>
        /// <param name="info">存在の確認を行うアセットバンドル情報</param>
        /// <returns>対象のアセットがロード可能な位置に存在する場合は true を、存在しない場合は false を返します</returns>
        public abstract bool Exists(ref AssetBundleInfo info);


        /// <summary>
        /// 指定されたアセットバンドル情報のアセットバンドルに、インストールするためのストリームを非同期で取得します。
        /// </summary>
        /// <param name="info">インストールするアセットバンドル情報</param>
        /// <returns>指定したアセットバンドルを書き込むためのストリームを取得する非同期タスクを返します</returns>
        public abstract Task<Stream> GetInstallStreamAsync(AssetBundleInfo info);


        /// <summary>
        /// 指定されたアセットバンドルを非同期で削除します
        /// </summary>
        /// <param name="info">削除するアセットバンドルのアセットバンドル情報</param>
        /// <returns>指定したアセットバンドルを非同期で削除するタスクを返します</returns>
        public abstract Task RemoveAsync(AssetBundleInfo info);


        /// <summary>
        /// このストレージが管理しているすべてのアセットバンドルを非同期で削除します
        /// </summary>
        /// <param name="progress">削除の進捗通知を受ける IProgress</param>
        /// <returns>すべてのアセットバンドルを非同期で削除するタスクを返します</returns>
        public abstract Task RemoveAllAsync(IProgress<double> progress);


        /// <summary>
        /// 指定されたローカルパスからアセットバンドルを非同期で開きます
        /// </summary>
        /// <param name="assetUrl">開くアセットバンドルを決定するためのローカルパス</param>
        /// <returns>アセットバンドルを非同期で開くタスクを返します</returns>
        public abstract Task<AssetBundle> OpenAsync(string localPath);


        /// <summary>
        /// 指定されたアセットバンドルを閉じます
        /// </summary>
        /// <param name="localPath">閉じるアセットバンドルのローカルパス</param>
        public abstract void Close(string localPath);
    }
}