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
using System.Linq;
using IceMilkTea.Core;
using UnityEditor;
using UnityEngine;

namespace IceMilkTeaEditor.Utility
{
    /// <summary>
    /// GameMainアセットを操作するクラスです
    /// </summary>
    public static class GameMainAssetUtility
    {
        /// <summary>
        /// アセットとして生成可能な GameMain の型を取得します
        /// </summary>
        /// <remarks>
        /// HideCreateGameMainAssetMenuAttribute 属性クラスがマークされた GameMain は取得されないことに注意して下さい
        /// </remarks>
        /// <returns>アセットとして生成可能な GameMain の型の配列を返します</returns>
        public static Type[] GetCreatableGameMainTypes()
        {
            // 利用するクラスの型を取り出す
            var gameMainType = typeof(GameMain);
            var hideAttributeType = typeof(HideCreateGameMainAssetMenuAttribute);


            // HideCreateGameMainAssetMenuAttribute属性が付いていない、GameMain継承クラスを返す
            return MonoImporter.GetAllRuntimeMonoScripts()
                .Select(x => x.GetClass())
                .Where(x => x != null && x.IsSubclassOf(gameMainType) && Attribute.GetCustomAttribute(x, hideAttributeType) == null)
                .ToArray();
        }


        /// <summary>
        /// 指定された GameMain の型のアセットを生成します
        /// </summary>
        /// <param name="gameMainType">生成するアセットの GameMain 型</param>
        /// <param name="savePath">生成するアセットのパス</param>
        public static void CreateGameMainAsset(Type gameMainType, string savePath)
        {
            // 指定された型のScriptableObjectインスタンスを生成する
            var instance = ScriptableObject.CreateInstance(gameMainType);


            // 指定されたパスにアセットを生成する
            AssetDatabase.CreateAsset(instance, savePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}