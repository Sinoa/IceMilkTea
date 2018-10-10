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

namespace IceMilkTea.Service
{
    // Unityのアセットの基底型になるObjectは、System.Objectとややこしくなるので、ここではUnityAssetと名付ける
    using UnityAsset = UnityEngine.Object;



    /// <summary>
    /// ゲームアセットの貯蔵状態の管理やアセットの読み込みなどを提供する抽象クラスです
    /// </summary>
    public abstract class AssetStorageManager
    {
        /// <summary>
        /// 指定されたアセットURLのアセットがロードできる位置に存在するかどうかを確認します
        /// </summary>
        /// <param name="assetUrl">存在の確認を行うアセットURL</param>
        /// <returns>対象のアセットがロード可能な位置に存在する場合は true を、存在しない場合は false を返します</returns>
        public abstract bool AssetExists(Uri assetUrl);


        /// <summary>
        /// 指定したアセットURLに対してインストールするためのストリームを非同期で取得します
        /// </summary>
        /// <param name="assetUrl">アセット書き込む先のアセットURL</param>
        /// <param name="progress">ストリームの準備進捗通知を受ける IProgress</param>
        /// <returns>指定したアセットURLのインストールストリームを非同期で取得するタスクを返します</returns>
        public abstract Task<Stream> GetInstallStreamAsync(Uri assetUrl, IProgress<double> progress);


        /// <summary>
        /// 指定されたアセットURLのアセットを非同期で削除します
        /// </summary>
        /// <param name="assetUrl">削除するアセットURL</param>
        /// <param name="progress">削除の進捗通知を受ける IProgress</param>
        /// <returns>指定したアセットURLのアセットを非同期で削除するタスクを返します</returns>
        public abstract Task RemoveAssetAsync(Uri assetUrl, IProgress<double> progress);


        /// <summary>
        /// このストレージが管理しているすべてのアセットを非同期で削除します
        /// </summary>
        /// <param name="progress">削除の進捗通知を受ける IProgress</param>
        /// <returns>すべてのアセットを非同期で削除するタスクを返します</returns>
        public abstract Task RemoveAllAssetAsync(IProgress<double> progress);


        /// <summary>
        /// 指定されたアセットの非同期ロードを行います
        /// </summary>
        /// <param name="assetUrl">ロードするアセットURL</param>
        /// <param name="progress">ロードの進捗通知を受ける IProgress</param>
        /// <returns>アセットのロードを非同期で行うタスクを返します</returns>
        public abstract Task<UnityAsset> LoadAssetAsync(Uri assetUrl, IProgress<double> progress);
    }
}