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
using UnityEngine.Experimental.LowLevel;

namespace IceMilkTea.Profiler
{
    /// <summary>
    /// プロファイラモジュール内で利用されるユーティリティクラスです
    /// </summary>
    internal static class ProfilerUtility
    {
        /// <summary>
        /// 挿入する際に対象の前か後ろかを指定します
        /// </summary>
        public enum InsertPoint
        {
            /// <summary>
            /// 対象の前に挿入を実行します
            /// </summary>
            BeforeInsert,

            /// <summary>
            /// 対象の後ろに挿入を実行します
            /// </summary>
            AfterInsert,
        }


        /// <summary>
        /// 対象のアップデート型の前後どちらかに、指定された更新関数を挿入します。
        /// この関数は、単一の更新関数を追加する場合に特化しているので、複数の登録（数十など）が必要な場合は別途実装を検討をしてください。
        /// </summary>
        /// <param name="insertPivot">挿入する起点となるアップデートの型</param>
        /// <param name="insertPoint">指定された起点の前か後ろか</param>
        /// <param name="function">挿入する更新関数</param>
        /// <exception cref="NullReferenceException">insertPivotまたはfunctionがnullです</exception>
        public static void InsertUpdateFunction(Type insertPivot, InsertPoint insertPoint, PlayerLoopSystem.UpdateFunction function)
        {
            // 無効なパラメータをわされていないか
            if (insertPivot == null || function == null)
            {
                // nullを渡すのは許されない
                throw new NullReferenceException("nullです");
            }


            // Unityの更新ループへ指定された更新関数を挿入した新しいループシステムを生成して設定する
            var newPlayerLoopSystem = PlayerLoop.GetDefaultPlayerLoop();
            CreatePlayerLoopSystem(insertPivot, insertPoint, function, ref newPlayerLoopSystem);
            PlayerLoop.SetPlayerLoop(newPlayerLoopSystem);
        }


        /// <summary>
        /// 対象のアップデート型の前後どちらかに、指定されたループシステム内の挿入位置を見つけ挿入します。
        /// この関数は、単一の更新関数を追加する場合に特化しているので、複数の登録（数十など）が必要な場合は別途実装を検討をしてください。
        /// </summary>
        /// <param name="insertPivot">挿入する起点となるアップデートの型</param>
        /// <param name="insertPoint">指定された起点の前か後ろか</param>
        /// <param name="function">挿入する更新関数</param>
        /// <param name="loopSystem">挿入操作が行われる対象のPlayerLoopSystemの参照</param>
        private static void CreatePlayerLoopSystem(Type insertPivot, InsertPoint insertPoint, PlayerLoopSystem.UpdateFunction function, ref PlayerLoopSystem loopSystem)
        {
            // サブループシステムの取得ができないなら
            if (loopSystem.subSystemList == null || loopSystem.subSystemList.Length == 0)
            {
                // これ以上の詮索は無用
                return;
            }


            // サブループシステムの数分回る
            var subLoopSystems = loopSystem.subSystemList;
            for (int i = 0; i < subLoopSystems.Length; ++i)
            {
                // 潜る
                CreatePlayerLoopSystem(insertPivot, insertPoint, function, ref subLoopSystems[i]);


                // この時点で挿入する軸の型が見つかったのなら
                if (subLoopSystems[i].type == insertPivot)
                {
                    // 挿入タイミングと新しいバッファの生成をする
                    var newSubLoopsystemLength = subLoopSystems.Length + 1;
                    var newSubLoopSystems = new PlayerLoopSystem[newSubLoopsystemLength];
                    var insertIndex = insertPoint == InsertPoint.BeforeInsert ? i : i + 1;
                    var indexGap = 0;


                    // 新しいインデックスの長さ分回る
                    for (int newIndex = 0; newIndex < newSubLoopsystemLength; ++newIndex)
                    {
                        // 挿入位置に到達したのなら
                        if (newIndex == insertIndex)
                        {
                            // 新しい更新システムを登録する
                            newSubLoopSystems[newIndex] = new PlayerLoopSystem()
                            {
                                type = function.Method.DeclaringType,
                                updateDelegate = function,
                            };


                            // インデックスギャップを加算して次へ
                            ++indexGap;
                            continue;
                        }


                        // コピーをする
                        newSubLoopSystems[newIndex] = subLoopSystems[newIndex - indexGap];
                    }


                    // 作られた新しい更新ループシステムを設定して抜ける
                    loopSystem.subSystemList = newSubLoopSystems;
                    return;
                }
            }
        }
    }
}