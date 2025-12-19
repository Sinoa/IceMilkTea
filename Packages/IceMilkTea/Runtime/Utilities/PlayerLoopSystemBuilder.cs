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
    /// PlayerLoopSystem の構造を構築する機能を提供します。
    /// Unity の PlayerLoop に対して更新関数の注入や構造の可視化を行います。
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
        /// 現在アクティブな PlayerLoopSystem を使用して新しいビルダーを生成します
        /// </summary>
        /// <returns>現在の PlayerLoop を基にした PlayerLoopSystemBuilder のインスタンス</returns>
        public static PlayerLoopSystemBuilder CreateFromCurrentPlayerLoop()
        {
            return new PlayerLoopSystemBuilder(PlayerLoop.GetCurrentPlayerLoop());
        }

        /// <summary>
        /// Unity のデフォルト PlayerLoopSystem を使用して新しいビルダーを生成します
        /// </summary>
        /// <returns>デフォルトの PlayerLoop を基にした PlayerLoopSystemBuilder のインスタンス</returns>
        public static PlayerLoopSystemBuilder CreateFromDefaultPlayerLoop()
        {
            return new PlayerLoopSystemBuilder(PlayerLoop.GetDefaultPlayerLoop());
        }

        /// <summary>
        /// 空の PlayerLoopSystem を使用して新しいビルダーを生成します
        /// </summary>
        /// <returns>空の PlayerLoop を基にした PlayerLoopSystemBuilder のインスタンス</returns>
        public static PlayerLoopSystemBuilder CreateEmpty()
        {
            return new PlayerLoopSystemBuilder(new PlayerLoopSystem());
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
        /// <exception cref="ArgumentNullException">function または pivotType が null の場合にスローされます</exception>
        public bool InjectUpdateFunction(PlayerLoopSystem.UpdateFunction function, Type pivotType, bool before)
        {
            if (function == null)
            {
                throw new ArgumentNullException(nameof(function));
            }

            if (pivotType == null)
            {
                throw new ArgumentNullException(nameof(pivotType));
            }

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

        /// <summary>
        /// 指定された基準PlayerLoopSystem型の前か後ろに、更新関数を注入します。
        /// 既に同じ宣言型が登録されている場合は注入を行いません。
        /// </summary>
        /// <param name="function">注入する更新関数。登録される型名は更新関数を定義している型の名前になります。</param>
        /// <param name="pivotType">更新関数を注入する位置の基準となるPlayerLoopSystem型または、注入した更新関数の定義型</param>
        /// <param name="before">pivotTypeの前に注入する場合は true を、後ろに注入する場合は false を指定</param>
        /// <returns>注入に成功した場合は true を、重複または基準型が見つからない場合は false を返します</returns>
        /// <exception cref="ArgumentNullException">function または pivotType が null の場合にスローされます</exception>
        public bool TryInjectUpdateFunction(PlayerLoopSystem.UpdateFunction function, Type pivotType, bool before)
        {
            if (function == null)
            {
                throw new ArgumentNullException(nameof(function));
            }

            if (ContainsUpdateFunction(function))
            {
                return false;
            }

            return InjectUpdateFunction(function, pivotType, before);
        }

        /// <summary>
        /// 指定された型がPlayerLoopSystem内に既に登録されているかを検証します
        /// </summary>
        /// <param name="type">検索する型</param>
        /// <returns>指定された型が既に存在する場合は true を、存在しない場合は false を返します</returns>
        /// <exception cref="ArgumentNullException">type が null の場合にスローされます</exception>
        public bool ContainsType(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return ContainsTypeRecursive(_rootPlayerLoopSystem, type);
        }

        /// <summary>
        /// 指定された更新関数の宣言型がPlayerLoopSystem内に既に登録されているかを検証します
        /// </summary>
        /// <param name="function">検索する更新関数</param>
        /// <returns>更新関数の宣言型が既に存在する場合は true を、存在しない場合は false を返します</returns>
        /// <exception cref="ArgumentNullException">function が null の場合にスローされます</exception>
        public bool ContainsUpdateFunction(PlayerLoopSystem.UpdateFunction function)
        {
            if (function == null)
            {
                throw new ArgumentNullException(nameof(function));
            }

            var declaringType = function.Method.DeclaringType;
            if (declaringType == null)
            {
                return false;
            }

            return ContainsTypeRecursive(_rootPlayerLoopSystem, declaringType);
        }

        /// <summary>
        /// PlayerLoopSystem ツリーを再帰的に探索し、指定された型が存在するかを確認します。
        /// 各ノードの type フィールドと subSystemList を順次確認します。
        /// </summary>
        /// <param name="playerLoopSystem">探索対象の PlayerLoopSystem</param>
        /// <param name="targetType">検索する型</param>
        /// <returns>指定された型が見つかった場合は true を返します</returns>
        private static bool ContainsTypeRecursive(PlayerLoopSystem playerLoopSystem, Type targetType)
        {
            if (playerLoopSystem.type != null && playerLoopSystem.type == targetType)
            {
                return true;
            }

            var subSystemList = playerLoopSystem.subSystemList;
            if (subSystemList == null || subSystemList.Length == 0)
            {
                return false;
            }

            foreach (var subSystem in subSystemList)
            {
                if (ContainsTypeRecursive(subSystem, targetType))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 新しい PlayerLoopSystem を注入した配列を生成します。
        /// 指定されたインデックスに更新関数を挿入し、既存の要素を前後に配置します。
        /// </summary>
        /// <param name="oldArray">元の PlayerLoopSystem 配列</param>
        /// <param name="injectIndex">新しい PlayerLoopSystem を挿入するインデックス</param>
        /// <param name="function">注入する更新関数</param>
        /// <returns>新しい PlayerLoopSystem が挿入された配列</returns>
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

        /// <summary>
        /// 指定された型を持つ PlayerLoopSystem の位置を再帰的に探索し、スタックに経路を記録します。
        /// 発見時のスタックは、ルートから対象ノードまでのパスを保持します。
        /// </summary>
        /// <param name="currentPlayerLoopSystem">現在探索中の PlayerLoopSystem</param>
        /// <param name="targetType">探索対象の型</param>
        /// <param name="stackTrace">探索経路を記録するスタック</param>
        /// <returns>対象の型が見つかった場合は true を返します</returns>
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

        /// <summary>
        /// PlayerLoopSystem ツリーを再帰的に探索し、階層構造を文字列として構築します。
        /// 深さに応じたインデントを付与して木構造を表現します。
        /// </summary>
        /// <param name="playerLoopSystem">現在処理中の PlayerLoopSystem</param>
        /// <param name="builder">文字列を構築する StringBuilder</param>
        /// <param name="depth">現在の階層の深さ</param>
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