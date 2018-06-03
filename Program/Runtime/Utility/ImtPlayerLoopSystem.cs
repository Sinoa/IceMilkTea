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
using System.Linq;
using UnityEngine.Experimental.LowLevel;

namespace IceMilkTea.Utility
{
    /// <summary>
    /// PlayerLoopSystem構造体の内容をクラスとして表現したクラスです
    /// </summary>
    public class ImtPlayerLoopSystem
    {
        /// <summary>
        /// ループシステムを表現する型
        /// </summary>
        public Type Type { get; private set; }

        /// <summary>
        /// このループシステムが保持するサブループシステムのリスト
        /// </summary>
        public List<ImtPlayerLoopSystem> SubSystemList { get; private set; }

        /// <summary>
        /// このループシステムが実行するべき更新関数。
        /// 更新する事がなければnullが設定出来ます
        /// </summary>
        public PlayerLoopSystem.UpdateFunction UpdateDelegate { get; set; }

        /// <summary>
        /// Unityのネイティブ更新関数への参照
        /// </summary>
        public IntPtr UpdateFunction { get; private set; }

        /// <summary>
        /// Unityのネイティブループ条件関数への参照
        /// </summary>
        public IntPtr LoopConditionFunction { get; private set; }



        /// <summary>
        /// 指定されたオリジナルのPlayerLoopSystemから値をコピーしてインスタンスの初期化を行います
        /// </summary>
        /// <param name="originalPlayerLoopSystem">コピー元になるPlayerLoopSystemへの参照</param>
        public ImtPlayerLoopSystem(ref PlayerLoopSystem originalPlayerLoopSystem)
        {
            // 参照元から値を引っ張って初期化する
            Type = originalPlayerLoopSystem.type;
            UpdateDelegate = originalPlayerLoopSystem.updateDelegate;
            UpdateFunction = originalPlayerLoopSystem.updateFunction;
            LoopConditionFunction = originalPlayerLoopSystem.loopConditionFunction;


            // もしサブシステムが有効な数で存在するなら
            if (originalPlayerLoopSystem.subSystemList != null && originalPlayerLoopSystem.subSystemList.Length > 0)
            {
                // 再帰的にコピーを生成する
                var enumerable = originalPlayerLoopSystem.subSystemList.Select(original => new ImtPlayerLoopSystem(ref original));
                SubSystemList = new List<ImtPlayerLoopSystem>(enumerable);
            }
            else
            {
                // 存在しないならインスタンスの生成だけする
                SubSystemList = new List<ImtPlayerLoopSystem>();
            }
        }


        /// <summary>
        /// 指定された型でインスタンスの初期化を行います
        /// </summary>
        /// <param name="type">生成するPlayerLoopSystemの型</param>
        public ImtPlayerLoopSystem(Type type) : this(type, null)
        {
        }


        /// <summary>
        /// 指定された型と更新関数でインスタンスの初期化を行います
        /// </summary>
        /// <param name="type">生成するPlayerLoopSystemの型</param>
        /// <param name="updateDelegate">生成するPlayerLoopSystemの更新関数。更新関数が不要な場合はnullの指定が可能です</param>
        /// <exception cref="ArgumentNullException">typeがnullです</exception>
        public ImtPlayerLoopSystem(Type type, PlayerLoopSystem.UpdateFunction updateDelegate)
        {
            // 更新の型がnullなら
            if (type == null)
            {
                // 関数は死ぬ
                throw new ArgumentNullException(nameof(type));
            }


            // シンプルに初期化をする
            Type = type;
            UpdateDelegate = updateDelegate;
            SubSystemList = new List<ImtPlayerLoopSystem>();
        }


        /// <summary>
        /// 内部で保持しているUnityネイティブ関数の参照をリセットします
        /// </summary>
        public void ResetUnityNativeFunctions()
        {
            // Unityのネイティブ関数系全てリセットする
            UpdateFunction = default(IntPtr);
            LoopConditionFunction = default(IntPtr);
        }


        /// <summary>
        /// 指定された型に変更します
        /// </summary>
        /// <param name="newType">変更する新しい型</param>
        /// <exception cref="ArgumentNullException">newTypeがnullです</exception>
        public void ChangeType(Type newType)
        {
            // もしnullが渡されていたら
            if (newType == null)
            {
                // 関数は死ぬ
                throw new ArgumentNullException(nameof(newType));
            }


            // 指示された型を設定する
            Type = newType;
        }


        /// <summary>
        /// クラス化されているPlayerLoopSystemを構造体のPlayerLoopSystemへ変換します
        /// </summary>
        /// <returns>内部のコンテキストのコピーを行ったPlayerLoopSystemを返します</returns>
        public PlayerLoopSystem ToPlayerLoopSystem()
        {
            // 新しいPlayerLoopSystem構造体のインスタンスを生成して初期化を行った後返す
            return new PlayerLoopSystem()
            {
                // 各パラメータのコピー
                type = Type,
                updateDelegate = UpdateDelegate,
                updateFunction = UpdateFunction,
                loopConditionFunction = LoopConditionFunction,
                subSystemList = SubSystemList.Select(source => source.ToPlayerLoopSystem()).ToArray(),
            };
        }


        /// <summary>
        /// PlayerLoopSystemからImtPlayerLoopSystemへキャストします
        /// </summary>
        /// <param name="original">キャストする元になるPlayerLoopSystem</param>
        public static explicit operator ImtPlayerLoopSystem(PlayerLoopSystem original)
        {
            // 渡されたPlayerLoopSystemからImtPlayerLoopSystemのインスタンスを生成して返す
            return new ImtPlayerLoopSystem(ref original);
        }


        /// <summary>
        /// ImtPlayerLoopSystemからPlayerLoopSystemへキャストします
        /// </summary>
        /// <param name="klass">キャストする元になるImtPlayerLoopSystem</param>
        public static explicit operator PlayerLoopSystem(ImtPlayerLoopSystem klass)
        {
            // 渡されたImtPlayerLoopSystemからPlayerLoopSystemへ変換する関数を叩いて返す
            return klass.ToPlayerLoopSystem();
        }
    }
}