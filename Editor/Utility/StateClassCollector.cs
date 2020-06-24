﻿// zlib/libpng License
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
    /// ImtStateMachine クラスが持つ State クラスを継承しているクラスを収集するクラスです
    /// </summary>
    public class StateClassCollector
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
                .Where(x => x != null && x.BaseType != null && x.BaseType.DeclaringType != null && x.BaseType.DeclaringType.IsGenericType)
                .Select(x => (ParentType: x.BaseType.DeclaringType.GetGenericTypeDefinition(), StateType: x))
                .Where(x => x.ParentType.IsAssignableFrom(typeof(ImtStateMachine<,>)))
                .Select(x => x.StateType);
        }
    }
}