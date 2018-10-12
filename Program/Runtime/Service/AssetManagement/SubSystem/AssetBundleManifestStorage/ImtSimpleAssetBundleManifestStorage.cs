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
using System.Threading.Tasks;

namespace IceMilkTea.Service
{
    /// <summary>
    /// IceMilkTeaが提供する単純なアセットバンドルマニフェストストレージクラスです
    /// </summary>
    public class ImtSimpleAssetBundleManifestStorage : AssetBundleManifestStorage
    {
        // メンバ変数定義
        private Dictionary<string, ImtAssetBundleManifest> manifestTable;



        /// <summary>
        /// ImtSimpleAssetBundleManifestStorage のインスタンスを初期化します
        /// </summary>
        public ImtSimpleAssetBundleManifestStorage()
        {
            // マニフェストテーブルのインスタンスを生成する
            manifestTable = new Dictionary<string, ImtAssetBundleManifest>();
        }


        /// <summary>
        /// 管理している全てのマニフェストの名前を取得します
        /// </summary>
        /// <returns>管理しているマニフェストの名前の配列を返します。1件もない場合でも長さ0の配列として返します</returns>
        public override string[] GetAllManifestName()
        {
            // マニフェストテーブルの管理件数が0件の場合は
            if (manifestTable.Count == 0)
            {
                // 長さ0の配列を返す
                Array.Empty<string>();
            }


            throw new NotImplementedException();
        }


        /// <summary>
        /// 永続化されたマニフェストを非同期でロードします。
        /// 必ずストレージとして利用する場合は先にロードを行うようにしてください。
        /// </summary>
        /// <param name="progress">ロードの進捗通知を受ける IProgress</param>
        /// <returns>マニフェストを非同期でロードしているタスクを返します</returns>
        public override Task LoadAsync(IProgress<float> progress)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// 管理しているマニフェストを非同期で永続化します。
        /// </summary>
        /// <param name="progress">永続化の進捗通知を受ける IProgress</param>
        /// <returns>マニフェストを非同期で永続化しているタスクを返します</returns>
        public override Task SaveAsync(IProgress<float> progress)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// マニフェストストレージに指定されたマニフェストを管理対象にします
        /// </summary>
        /// <param name="manifest">管理対象となるマニフェストの参照</param>
        public override void SetManifest(ref ImtAssetBundleManifest manifest)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// マニフェストストレージに管理さている、指定されたマニフェスト名のマニフェストを取得に試みます
        /// </summary>
        /// <param name="name">取得したいマニフェストの名前</param>
        /// <param name="manifest">取得したマニフェストを受け取る変数</param>
        /// <returns>マニフェストの取得に成功した場合は true を、失敗した場合は false を返します</returns>
        public override bool TryGetManifest(string name, out ImtAssetBundleManifest manifest)
        {
            throw new NotImplementedException();
        }
    }
}