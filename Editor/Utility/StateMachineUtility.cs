// zlib/libpng License
//
// Copyright (c) 2020 Sinoa
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
using System.Linq;
using IceMilkTea.Core;

namespace IceMilkTeaEditor.Utility
{
    /// <summary>
    /// ImtStateMachine クラスにまつわるユーティリティ機能を提供するクラスです
    /// </summary>
    public static class StateMachineUtility
    {
        /// <summary>
        /// State クラスを継承するクラスを収集します
        /// </summary>
        /// <returns>収集された型の反復可能オブジェクトを返します</returns>
        public static IEnumerable<Type> CollectStateTypes()
        {
            // ステートマシンのステートを継承しているすべての型を拾い上げる列挙オブジェクトを返す
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x => IsGenericDeclaringType(x))
                .Select(x => (ParentType: x.BaseType.DeclaringType.GetGenericTypeDefinition(), StateType: x))
                .Where(x => x.ParentType.IsAssignableFrom(typeof(ImtStateMachine<,>)))
                .Select(x => x.StateType);
        }


        /// <summary>
        /// 対象の型の基本クラスがジェネリック型かどうかを判断します
        /// </summary>
        /// <param name="type">判断する元になる型</param>
        /// <returns>type の基本クラスを持つ親クラスがジェネリック型の場合は true を、異なる場合は false を返します</returns>
        private static bool IsGenericDeclaringType(Type type)
        {
            // 安全に基本クラスの親がジェネリック型かどうかを返す
            return
                type != null &&
                type.BaseType != null &&
                type.BaseType.DeclaringType != null &&
                type.BaseType.DeclaringType.IsGenericType;
        }


        /// <summary>
        /// 指定されたアセンブリ名から型をフィルタリングします
        /// </summary>
        /// <param name="types">フィルタする対象となる型の反復可能オブジェクト</param>
        /// <param name="assemblyNames">フィルタするアセンブリ名の反復可能オブジェクト</param>
        /// <returns>フィルタリングされた結果の型の反復可能オブジェクト</returns>
        public static IEnumerable<Type> FilterAssembly(this IEnumerable<Type> types, IEnumerable<string> assemblyNames)
        {
            // アセンブリ名に組まれるかどうかのフィルタをする
            return types.Where(x => assemblyNames.Contains(x.Assembly.GetName().Name));
        }


        /// <summary>
        /// 指定された型の反復オブジェクトから、型のフルパス名を射影します
        /// </summary>
        /// <param name="types">型のフルパス名を射影する型の反復可能オブジェクト</param>
        /// <returns>指定された型のフルパス名の反復可能オブジェクトを返します</returns>
        public static IEnumerable<string> SelectTypeFullPathName(this IEnumerable<Type> types)
        {
            // 型からネームスペース入りフルパス名を射影する
            return types.Select(x => x.FullName.Replace("+", "."));
        }
    }
}