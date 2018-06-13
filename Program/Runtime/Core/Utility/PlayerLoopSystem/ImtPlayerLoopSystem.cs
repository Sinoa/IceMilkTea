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
using System.Text;
using UnityEngine.Experimental.LowLevel;

namespace IceMilkTea.Core
{
    /// <summary>
    /// PlayerLoopSystem構造体の内容をクラスとして表現したクラスです
    /// </summary>
    internal class ImtPlayerLoopSystem
    {
        // メンバ変数定義
        private Type type;
        private List<ImtPlayerLoopSystem> subLoopSystemList;
        private PlayerLoopSystem.UpdateFunction updateDelegate;
        private IntPtr updateFunction;
        private IntPtr loopConditionFunction;



        #region コンストラクタ
        /// <summary>
        /// 指定されたPlayerLoopSystem構造体オブジェクトから値をコピーしてインスタンスの初期化を行います。
        /// また、指定されたPlayerLoopSystem構造体オブジェクトにサブループシステムが存在する場合は再帰的にインスタンスの初期化が行われます。
        /// </summary>
        /// <param name="originalPlayerLoopSystem">コピー元になるPlayerLoopSystem構造体オブジェクトへの参照</param>
        public ImtPlayerLoopSystem(ref PlayerLoopSystem originalPlayerLoopSystem)
        {
            // 参照元から値を引っ張って初期化する
            type = originalPlayerLoopSystem.type;
            updateDelegate = originalPlayerLoopSystem.updateDelegate;
            updateFunction = originalPlayerLoopSystem.updateFunction;
            loopConditionFunction = originalPlayerLoopSystem.loopConditionFunction;


            // もしサブシステムが有効な数で存在するなら
            if (originalPlayerLoopSystem.subSystemList != null && originalPlayerLoopSystem.subSystemList.Length > 0)
            {
                // 再帰的にコピーを生成する
                var enumerable = originalPlayerLoopSystem.subSystemList.Select(original => new ImtPlayerLoopSystem(ref original));
                subLoopSystemList = new List<ImtPlayerLoopSystem>(enumerable);
            }
            else
            {
                // 存在しないならインスタンスの生成だけする
                subLoopSystemList = new List<ImtPlayerLoopSystem>();
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
            this.type = type;
            this.updateDelegate = updateDelegate;
            subLoopSystemList = new List<ImtPlayerLoopSystem>();
        }
        #endregion


        #region コントロール関数群
        /// <summary>
        /// 指定されたインデックスの位置にループシステムを挿入します
        /// </summary>
        /// <param name="index">挿入するインデックスの位置</param>
        /// <param name="loopSystem">挿入するループシステム</param>
        public void InsertLoopSystem(int index, ImtPlayerLoopSystem loopSystem)
        {
        }


        /// <summary>
        /// 指定のジェネリック型の更新ループの前に、指定されたループシステムを挿入します
        /// </summary>
        /// <typeparam name="T">これから挿入するループシステムの挿入起点となる更新型</typeparam>
        /// <param name="loopSystem">挿入するループシステム</param>
        public void InsertBeforeLoopSystem<T>(ImtPlayerLoopSystem loopSystem)
        {
        }


        /// <summary>
        /// 指定のジェネリック型の更新ループの後に、指定されたループシステムを挿入します
        /// </summary>
        /// <typeparam name="T">これから挿入するループシステムの挿入起点となる更新型</typeparam>
        /// <param name="loopSystem">挿入するループシステム</param>
        public void InsertAfterLoopSystem<T>(ImtPlayerLoopSystem loopSystem)
        {
        }


        /// <summary>
        /// 指定された更新型でループシステムを探し出します。
        /// </summary>
        /// <typeparam name="T">検索するループシステムの型</typeparam>
        /// <returns>最初に見つけたループシステムを返しますが、見つけられなかった場合はnullを返します</returns>
        public ImtPlayerLoopSystem FindLoopSystem<T>()
        {
        }


        /// <summary>
        /// 指定された更新型で存在インデックス位置を取得します
        /// </summary>
        /// <typeparam name="T">検索するループシステムの型</typeparam>
        /// <returns>最初に見つけたループシステムのインデックスを返しますが、見つけられなかった場合は -1 をかえします</returns>
        public int IndexOf<T>()
        {
        }


        /// <summary>
        /// 内部で保持しているUnityネイティブ関数の参照をリセットします
        /// </summary>
        public void ResetUnityNativeFunctions()
        {
            // Unityのネイティブ関数系全てリセットする
            updateFunction = default(IntPtr);
            loopConditionFunction = default(IntPtr);
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
            type = newType;
        }


        /// <summary>
        /// クラス化されているPlayerLoopSystemを構造体のPlayerLoopSystemへ変換します。
        /// また、サブループシステムを保持している場合はサブループシステムも構造体のインスタンスが新たに生成され、初期化されます。
        /// </summary>
        /// <returns>内部コンテキストのコピーを行ったPlayerLoopSystemを返します</returns>
        public PlayerLoopSystem ToPlayerLoopSystem()
        {
            // 新しいPlayerLoopSystem構造体のインスタンスを生成して初期化を行った後返す
            return new PlayerLoopSystem()
            {
                // 各パラメータのコピー（サブループシステムも再帰的に構造体へインスタンス化）
                type = type,
                updateDelegate = updateDelegate,
                updateFunction = updateFunction,
                loopConditionFunction = loopConditionFunction,
                subSystemList = subLoopSystemList.Select(source => source.ToPlayerLoopSystem()).ToArray(),
            };
        }
        #endregion


        #region オペレータ＆ToStringオーバーライド
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


        /// <summary>
        /// ImpPlayerLoopSystem内のLoopSystem階層表示を文字列へ変換します
        /// </summary>
        /// <returns>このインスタンスのLoopSystem階層状況を文字列化したものを返します</returns>
        public override string ToString()
        {
            // バッファ用意
            var buffer = new StringBuilder();


            // バッファにループシステムツリーの内容をダンプする
            DumpLoopSystemTree(buffer, string.Empty);


            // バッファの内容を返す
            return buffer.ToString();
        }


        /// <summary>
        /// ImpPlayerLoopSystem内のLoopSystem階層を再帰的にバッファへ文字列を追記します
        /// </summary>
        /// <param name="buffer">追記対象のバッファ</param>
        /// <param name="indentSpace">現在のインデントスペース</param>
        private void DumpLoopSystemTree(StringBuilder buffer, string indentSpace)
        {
            // 自分の名前からぶら下げツリー表記
            buffer.Append($"{indentSpace}[{(type == null ? "NULL" : type.Name)}]\n");
            foreach (var subSystem in subLoopSystemList)
            {
                // 新しいインデントスペース文字列を用意して自分の子にダンプさせる
                subSystem.DumpLoopSystemTree(buffer, indentSpace + "  ");
            }
        }
        #endregion
    }
}