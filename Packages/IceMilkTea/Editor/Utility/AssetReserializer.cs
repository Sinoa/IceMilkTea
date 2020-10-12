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
using System.Linq;
using IceMilkTeaEditor.Common;
using UnityEditor;

namespace IceMilkTeaEditor.Utility
{
    public static class AssetReserializer
    {
        /// <summary>
        /// 指定されたアセットオブジェクトの再シリアライズをします
        /// </summary>
        /// <param name="assetObject">再シリアライズするアセットオブジェクト。ディレクトリアセットの場合はディレクトリ配下の全てが再シリアライズ対象になります。</param>
        public static void Reserialize(UnityEngine.Object assetObject)
        {
            // 指示されたオブジェクトがシーンヒエラルキ等の場合は
            if (!AssetDatabase.Contains(assetObject ?? throw new ArgumentNullException(nameof(assetObject))))
            {
                // ヒエラルキ上のオブジェクトは再シリアライズが出来ない例外を吐く
                throw new ArgumentException("ヒエラルキ上のオブジェクトに対して再シリアライズは出来ません", nameof(assetObject));
            }


            // アセットからパスを取得してディレクトリでは無いのなら
            var path = AssetDatabase.GetAssetPath(assetObject);
            if (!Directory.Exists(path))
            {
                // このパスのオブジェクトだけ再シリアライズして終了
                AssetDatabase.ForceReserializeAssets(new string[] { path });
                return;
            }


            // ディレクトリ配下すべてを取得してすべてプログレスダイアログに表示されるようにする
            var reserializeTargetPaths = AssetDatabase.FindAssets(string.Empty, new string[] { path })
                .Select(x => AssetDatabase.GUIDToAssetPath(x))
                .ToArray()
                .DisplayProgress(x => { x.Title = $"再シリアライズ中... [{x.Count}/{x.Max}]"; x.Text = x.Item; });


            // 列挙できるすべてのパスを再シリアライズする
            AssetDatabase.ForceReserializeAssets(reserializeTargetPaths);
        }


        /// <summary>
        /// Unityプロジェクト全体の再シリアライズをします
        /// </summary>
        public static void ReserializeAll()
        {
            // プロジェクト全体の再シリアライズをしてもらうようにする
            AssetDatabase.ForceReserializeAssets();
        }
    }
}