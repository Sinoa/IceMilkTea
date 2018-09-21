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

using UnityEngine;

namespace IceMilkTea.Core
{
    /// <summary>
    /// System.Object(object) クラスの拡張関数実装用クラスです
    /// </summary>
    public static class ObjectExtensions
    {
        /// <summary>
        /// 対象オブジェクトの ToString() を実行し、その結果をコンソールに出力します
        /// </summary>
        /// <typeparam name="T">ToString() を実行する型</typeparam>
        /// <param name="obj">ToString() を実行するオブジェクト</param>
        /// <returns>直前のオブジェクトをそのまま返します</returns>
        public static T Dump<T>(this T obj)
        {
            Debug.Log(obj.ToString());
            return obj;
        }
    }
}