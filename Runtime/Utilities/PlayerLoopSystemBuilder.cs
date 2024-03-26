// Zlib License
//
// Copyright (c) 2024 Sinoa
//
// This software is provided ‘as-is’, without any express or implied
// warranty. In no event will the authors be held liable for any damages
// arising from the use of this software.
//
// Permission is granted to anyone to use this software for any purpose,
// including commercial applications, and to alter it and redistribute it
// freely, subject to the following restrictions:
//
// 1. The origin of this software must not be misrepresented; you must not
// claim that you wrote the original software. If you use this software
// in a product, an acknowledgment in the product documentation would be
// appreciated but is not required.
//
// 2. Altered source versions must be plainly marked as such, and must not be
// misrepresented as being the original software.
//
// 3. This notice may not be removed or altered from any source
// distribution.

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.LowLevel;

namespace Foxtamp.IceMilkTea.Utilities
{
    /// <summary>
    /// PlayerLoopSystem の構造を構築する機能を提供します
    /// </summary>
    public class PlayerLoopSystemBuilder
    {
        private PlayerLoopSystem _rootPlayerLoopSystem;

        /// <summary>
        /// 指定された PlayerLoopSystem をルートとしてインスタンスの初期化を行います
        /// </summary>
        /// <param name="rootPlayerLoopSystem">ルートとなる PlayerLoopSystem のインスタンス</param>
        public PlayerLoopSystemBuilder(PlayerLoopSystem rootPlayerLoopSystem)
        {
            _rootPlayerLoopSystem = rootPlayerLoopSystem;
        }

        /// <summary>
        /// 操作した PlayerLoopSystem の結果を構築します
        /// </summary>
        /// <returns>設定するための構築済み PlayerLoopSystem を返します</returns>
        public PlayerLoopSystem Build()
        {
            return _rootPlayerLoopSystem;
        }

        /// <summary>
        /// 指定された基準PlayerLoopSystem型の前か後ろに、更新関数を注入します
        /// </summary>
        /// <param name="function">注入する更新関数。登録される型名は更新関数を定義している型の名前になります。</param>
        /// <param name="pivotType">更新関数を注入する位置の基準となるPlayerLoopSystem型または、注入した更新関数の定義型</param>
        /// <param name="before">pivotTypeの前に注入する場合は true を、後ろに注入する場合は false を指定</param>
        /// <returns>注入に成功した場合は true を、失敗した場合は false を返します</returns>
        public bool InjectUpdateFunction(PlayerLoopSystem.UpdateFunction function, Type pivotType, bool before)
        {
            var pivotTraceStack = new Stack<(int index, PlayerLoopSystem element)>();
            pivotTraceStack.Push((0, _rootPlayerLoopSystem));

            var found = TraceInjectionPoint(_rootPlayerLoopSystem, pivotType, pivotTraceStack);
            if (!found)
            {
                return false;
            }

            var injectIndex = pivotTraceStack.Pop().index + (before ? 0 : 1);
            var (updateIndex, updateTargetPlayerLoopSystem) = pivotTraceStack.Pop();
            updateTargetPlayerLoopSystem.subSystemList = CreateInjectedPlayerLoopSystemArray(updateTargetPlayerLoopSystem.subSystemList, injectIndex, function);

            if (_rootPlayerLoopSystem.type == updateTargetPlayerLoopSystem.type)
            {
                _rootPlayerLoopSystem = updateTargetPlayerLoopSystem;
                return true;
            }

            while (pivotTraceStack.Count > 0)
            {
                var (parentIndex, parentPlayerLoopSystem) = pivotTraceStack.Pop();
                parentPlayerLoopSystem.subSystemList[updateIndex] = updateTargetPlayerLoopSystem;
                updateIndex = parentIndex;
                updateTargetPlayerLoopSystem = parentPlayerLoopSystem;
            }

            return true;
        }

        private static PlayerLoopSystem[] CreateInjectedPlayerLoopSystemArray(PlayerLoopSystem[] oldArray, int injectIndex, PlayerLoopSystem.UpdateFunction function)
        {
            var newPlayerLoopSystemArray = new PlayerLoopSystem[oldArray.Length + 1];
            newPlayerLoopSystemArray[injectIndex] = new PlayerLoopSystem()
            {
                type = function.Method.DeclaringType,
                updateDelegate = function,
            };

            var backwardElementCount = oldArray.Length - injectIndex;
            var destinationIndex = injectIndex + 1;
            Array.Copy(oldArray, newPlayerLoopSystemArray, injectIndex);
            Array.Copy(oldArray, injectIndex, newPlayerLoopSystemArray, destinationIndex, backwardElementCount);

            return newPlayerLoopSystemArray;
        }

        private static bool TraceInjectionPoint(PlayerLoopSystem currentPlayerLoopSystem, Type targetType, Stack<(int index, PlayerLoopSystem element)> stackTrace)
        {
            if (currentPlayerLoopSystem.type != null && currentPlayerLoopSystem.type == targetType)
            {
                return true;
            }

            var subSystemList = currentPlayerLoopSystem.subSystemList;
            if (subSystemList == null || subSystemList.Length == 0)
            {
                return false;
            }


            for (int i = 0; i < subSystemList.Length; ++i)
            {
                var subSystem = subSystemList[i];
                stackTrace.Push((i, subSystem));

                var found = TraceInjectionPoint(subSystem, targetType, stackTrace);
                if (found)
                {
                    return true;
                }

                stackTrace.Pop();
            }

            return false;
        }

        /// <summary>
        /// 保持している PlayerLoopSystem の木構造を文字列として生成します
        /// </summary>
        /// <returns>木構造として表現された文字列を返します</returns>
        public string CreateTreeText()
        {
            var builder = new StringBuilder();
            CreateTreeText(_rootPlayerLoopSystem, builder, 0);
            return builder.ToString();
        }

        private static void CreateTreeText(PlayerLoopSystem playerLoopSystem, StringBuilder builder, int depth)
        {
            for (var i = 0; i < depth; ++i)
            {
                builder.Append("  ");
            }

            var type = playerLoopSystem.type;
            var typeName = type != null ? type.Name : "NULL";
            builder.AppendLine($"[{typeName}]");

            var subSystemList = playerLoopSystem.subSystemList;
            if (subSystemList != null)
            {
                foreach (var subSystem in subSystemList)
                {
                    CreateTreeText(subSystem, builder, depth + 1);
                }
            }
        }
    }
}