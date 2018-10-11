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
using System.Threading.Tasks;

namespace IceMilkTea.Service
{
    /// <summary>
    /// ImtAssetBundleManifest を貯蔵するストレージ抽象クラスです
    /// </summary>
    public abstract class AssetBundleManifestStorage
    {
        /// <summary>
        /// ストレージにマニフェストを設定します。
        /// 同名のマニフェストが存在する場合は、上書きされることに気をつけてください。
        /// </summary>
        /// <param name="manifest">設定するマニフェストの参照</param>
        public abstract void SetManifest(ref ImtAssetBundleManifest manifest);


        /// <summary>
        /// 指定された名前のマニフェストを取得を試みます。
        /// </summary>
        /// <param name="name">取得を試みるマニフェスト名</param>
        /// <param name="manifest">取得されたマニフェスト</param>
        /// <returns>取得に成功した場合は true を、失敗した場合は false を返します</returns>
        public abstract bool TryGetManifest(string name, out ImtAssetBundleManifest manifest);


        /// <summary>
        /// 管理している全てのマニフェストの名前を取得します
        /// </summary>
        /// <returns>取得した全てのマニフェストの名前の配列を返します。１つもない場合は長さ０の配列として返します</returns>
        public abstract string[] GetAllManifestName();


        /// <summary>
        /// 管理しているマニフェストを非同期で永続化します
        /// </summary>
        /// <param name="progress">永続化の進捗通知を受ける IProgress 。不要の場合は null を指定することが出来ます。</param>
        /// <returns>永続化の非同期操作をしているタスクを返します</returns>
        public abstract Task SaveAsync(IProgress<float> progress);


        /// <summary>
        /// 永続化したマニフェストを非同期で読み込みます
        /// </summary>
        /// <param name="progress">読み込みの進捗通知を受ける IProgress 。不要の場合は null を指定することが出来ます。</param>
        /// <returns>読み込みの非同期操作をしているタスクを返します</returns>
        public abstract Task LoadAsync(IProgress<float> progress);
    }
}