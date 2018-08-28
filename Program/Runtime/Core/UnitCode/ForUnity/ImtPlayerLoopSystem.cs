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
using UnityEngine;
using UnityEngine.Experimental.LowLevel;

namespace IceMilkTea.Core
{
    /// <summary>
    /// ループシステムの挿入をする時、対象の型に対して挿入するタイミングを指示します
    /// </summary>
    public enum InsertTiming
    {
        /// <summary>
        /// 対象の前に挿入を指示します
        /// </summary>
        BeforeInsert,

        /// <summary>
        /// 対象の後に挿入を指示します
        /// </summary>
        AfterInsert,
    }



    /// <summary>
    /// PlayerLoopSystem構造体の内容をクラスとして表現され、更に調整するための機構を保持したクラスです
    /// </summary>
    public class ImtPlayerLoopSystem
    {
        /// <summary>
        /// ループシステムの検索で、対象のループシステムを見つけられなかったときに返す値です
        /// </summary>
        public const int LoopSystemNotFoundValue = -1;



        // クラス変数宣言
        private static ImtPlayerLoopSystem lastBuildLoopSystem;

        // メンバ変数定義
        private Type type;
        private List<ImtPlayerLoopSystem> subLoopSystemList;
        private PlayerLoopSystem.UpdateFunction updateDelegate;
        private IntPtr updateFunction;
        private IntPtr loopConditionFunction;



        #region コンストラクタ
        /// <summary>
        /// クラスの初期化を行います
        /// </summary>
        static ImtPlayerLoopSystem()
        {
            // アプリケーション終了イベントを登録する
            Application.quitting += OnApplicationQuit;
        }


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


        #region Unityイベントハンドラ
        /// <summary>
        /// Unityがアプリケーションの終了をする時に呼び出されます
        /// </summary>
        private static void OnApplicationQuit()
        {
            //イベントの登録を解除する
            Application.quitting -= OnApplicationQuit;


            // Unityの弄り倒したループ構成をもとに戻してあげる
            PlayerLoop.SetPlayerLoop(PlayerLoop.GetDefaultPlayerLoop());
        }
        #endregion


        #region Unity変換関数群
        /// <summary>
        /// Unityの標準プレイヤーループを ImtPlayerLoopSystem として取得します
        /// </summary>
        /// <returns>Unityの標準プレイヤーループをImtPlayerLoopSystemにキャストされた結果を返します</returns>
        public static ImtPlayerLoopSystem GetUnityDefaultPlayerLoop()
        {
            // キャストして返すだけ
            return (ImtPlayerLoopSystem)PlayerLoop.GetDefaultPlayerLoop();
        }


        /// <summary>
        /// このインスタンスを本来の構造へ構築し、Unityのプレイヤーループへ設定します
        /// </summary>
        public void BuildAndSetUnityPlayerLoop()
        {
            // 最後に構築した経験のあるループシステムとして覚えて、自身をキャストして設定するだけ
            lastBuildLoopSystem = this;
            PlayerLoop.SetPlayerLoop((PlayerLoopSystem)this);
        }
        #endregion


        #region コントロール関数群
        /// <summary>
        /// BuildAndSetUnityDefaultPlayerLoop関数によって最後に構築されたループシステムを取得します。
        /// まだ一度も構築した経験がない場合は、GetUnityDefaultPlayerLoop関数の値を採用します。
        /// </summary>
        /// <returns>最後に構築されたループシステムを返します</returns>
        public static ImtPlayerLoopSystem GetLastBuildLoopSystem()
        {
            // 過去に構築経験があれば返して、まだなければGetUnityDefaultPlayerLoopの結果を返す
            return lastBuildLoopSystem ?? GetUnityDefaultPlayerLoop();
        }


        /// <summary>
        /// 指定されたインデックスの位置に更新関数を挿入します。
        /// また、nullの更新関数を指定すると何もしないループシステムが生成されます。
        /// </summary>
        /// <typeparam name="T">更新関数を表す型</typeparam>
        /// <param name="index">挿入するインデックスの位置</param>
        /// <param name="function">挿入する更新関数</param>
        public void InsertLoopSystem<T>(int index, PlayerLoopSystem.UpdateFunction function)
        {
            // 新しいループシステムを作って本来の挿入関数を叩く
            var loopSystem = new ImtPlayerLoopSystem(typeof(T), function);
            InsertLoopSystem(index, loopSystem);
        }


        /// <summary>
        /// 指定されたインデックスの位置にループシステムを挿入します
        /// </summary>
        /// <param name="index">挿入するインデックスの位置</param>
        /// <param name="loopSystem">挿入するループシステム</param>
        /// <exception cref="ArgumentNullException">loopSystemがnullです</exception>
        public void InsertLoopSystem(int index, ImtPlayerLoopSystem loopSystem)
        {
            // ループシステムがnullなら（境界チェックはあえてここでやらず、List<T>コンテナに任せる）
            if (loopSystem == null)
            {
                // nullの挿入は許されない！
                throw new ArgumentNullException(nameof(loopSystem));
            }


            // 指定されたインデックスにループシステムを挿入する
            subLoopSystemList.Insert(index, loopSystem);
        }


        /// <summary>
        /// 指定された型の更新ループに対して、ループシステムをタイミングの位置に挿入します
        /// </summary>
        /// <typeparam name="T">これから挿入するループシステムの挿入起点となる更新型</typeparam>
        /// <typeparam name="U">挿入する予定の更新関数を表す型</typeparam>
        /// <param name="timing">T で指定された更新ループを起点にどのタイミングで挿入するか</param>
        /// <param name="function">挿入する更新関数</param>
        /// <returns>対象のループシステムが挿入された場合はtrueを、挿入されなかった場合はfalseを返します</returns>
        public bool InsertLoopSystem<T, U>(InsertTiming timing, PlayerLoopSystem.UpdateFunction function)
        {
            // 再帰検索を有効にして挿入関数を叩く
            return InsertLoopSystem<T, U>(timing, function, true);
        }


        /// <summary>
        /// 指定された型の更新ループに対して、ループシステムをタイミングの位置に挿入します
        /// </summary>
        /// <typeparam name="T">これから挿入するループシステムの挿入起点となる更新型</typeparam>
        /// <typeparam name="U">挿入する予定の更新関数を表す型</typeparam>
        /// <param name="timing">T で指定された更新ループを起点にどのタイミングで挿入するか</param>
        /// <param name="function">挿入する更新関数</param>
        /// <param name="recursiveSearch">対象の型の検索を再帰的に行うかどうか</param>
        /// <returns>対象のループシステムが挿入された場合はtrueを、挿入されなかった場合はfalseを返します</returns>
        public bool InsertLoopSystem<T, U>(InsertTiming timing, PlayerLoopSystem.UpdateFunction function, bool recursiveSearch)
        {
            // 新しいループシステムを作って本来の挿入関数を叩く
            var loopSystem = new ImtPlayerLoopSystem(typeof(U), function);
            return InsertLoopSystem<T>(timing, loopSystem, recursiveSearch);
        }


        /// <summary>
        /// 指定された型の更新ループに対して、ループシステムをタイミングの位置に挿入します
        /// </summary>
        /// <typeparam name="T">これから挿入するループシステムの挿入起点となる更新型</typeparam>
        /// <param name="timing">T で指定された更新ループを起点にどのタイミングで挿入するか</param>
        /// <param name="loopSystem">挿入するループシステム</param>
        /// <exception cref="ArgumentNullException">loopSystemがnullです</exception>
        /// <returns>対象のループシステムが挿入された場合はtrueを、挿入されなかった場合はfalseを返します</returns>
        public bool InsertLoopSystem<T>(InsertTiming timing, ImtPlayerLoopSystem loopSystem)
        {
            // 再帰検索を有効にして本来の挿入関数を叩く
            return InsertLoopSystem<T>(timing, loopSystem, true);
        }


        /// <summary>
        /// 指定された型の更新ループに対して、ループシステムをタイミングの位置に挿入します
        /// </summary>
        /// <typeparam name="T">これから挿入するループシステムの挿入起点となる更新型</typeparam>
        /// <param name="timing">T で指定された更新ループを起点にどのタイミングで挿入するか</param>
        /// <param name="loopSystem">挿入するループシステム</param>
        /// <param name="recursiveSearch">対象の型の検索を再帰的に行うかどうか</param>
        /// <exception cref="ArgumentNullException">loopSystemがnullです</exception>
        /// <returns>対象のループシステムが挿入された場合はtrueを、挿入されなかった場合はfalseを返します</returns>
        public bool InsertLoopSystem<T>(InsertTiming timing, ImtPlayerLoopSystem loopSystem, bool recursiveSearch)
        {
            // ループシステムがnullなら
            if (loopSystem == null)
            {
                // nullの挿入は許されない！
                throw new ArgumentNullException(nameof(loopSystem));
            }


            // 挿入するインデックス値を探すが見つけられなかったら
            var insertIndex = IndexOf<T>();
            if (insertIndex == LoopSystemNotFoundValue)
            {
                // もし再帰的に調べるのなら
                if (recursiveSearch)
                {
                    // 自身のサブループシステム分回る
                    foreach (var subLoopSystem in subLoopSystemList)
                    {
                        // サブループシステムに対して挿入を依頼して成功したのなら
                        if (subLoopSystem.InsertLoopSystem<T>(timing, loopSystem, recursiveSearch))
                        {
                            // 成功を返す
                            return true;
                        }
                    }
                }


                // やっぱり駄目だったよ
                return false;
            }


            // 検索結果を見つけたのなら、挿入タイミングによってインデックス値を調整して挿入後、成功を返す
            insertIndex = timing == InsertTiming.BeforeInsert ? insertIndex : insertIndex + 1;
            subLoopSystemList.Insert(insertIndex, loopSystem);
            return true;
        }


        /// <summary>
        /// 指定された型の更新ループをサブループシステムから削除します
        /// </summary>
        /// <typeparam name="T">削除する更新ループの型</typeparam>
        /// <param name="recursiveSearch">対象の型を再帰的に検索し削除するかどうか</param>
        /// <returns>対象のループシステムが削除された場合はtrueを、削除されなかった場合はfalseを返します</returns>
        public bool RemoveLoopSystem<T>(bool recursiveSearch)
        {
            // 削除するインデックス値を探すが見つけられなかったら
            var removeIndex = IndexOf<T>();
            if (removeIndex == LoopSystemNotFoundValue)
            {
                // もし再帰的に調べるのなら
                if (recursiveSearch)
                {
                    // 自身のサブループシステム分回る
                    foreach (var subLoopSystem in subLoopSystemList)
                    {
                        // サブループシステムに対して削除依頼して成功したのなら
                        if (subLoopSystem.RemoveLoopSystem<T>(recursiveSearch))
                        {
                            // 成功を返す
                            return true;
                        }
                    }
                }


                // だめでした
                return false;
            }


            // 対象インデックスの要素を殺す
            subLoopSystemList.RemoveAt(removeIndex);
            return true;
        }


        /// <summary>
        /// 指定された型の更新ループを指定された数だけ移動します。
        /// また、移動量が境界を超えないように内部で調整されます。
        /// </summary>
        /// <typeparam name="T">移動する更新ループの型</typeparam>
        /// <param name="count">移動する量、負の値なら前方へ、正の値なら後方へ移動します</param>
        /// <param name="recursiveSearch">移動する型が見つからない場合、再帰的に検索をするかどうか</param>
        /// <returns>移動に成功した場合はtrueを、移動に失敗した場合はfalseを返します</returns>
        public bool MoveLoopSystem<T>(int count, bool recursiveSearch)
        {
            // 移動する更新ループの位置を特定するが、見つけられなかったら
            var currentIndex = IndexOf<T>();
            if (currentIndex == LoopSystemNotFoundValue)
            {
                // もし再帰的に調べるのなら
                if (recursiveSearch)
                {
                    // 自身のサブループシステム分回る
                    foreach (var childLoopSystem in subLoopSystemList)
                    {
                        // サブループシステムに対して削除を依頼して成功したのなら
                        if (childLoopSystem.MoveLoopSystem<T>(count, recursiveSearch))
                        {
                            // 成功を返す
                            return true;
                        }
                    }
                }


                // だめだったらだめ
                return false;
            }


            // 新しいインデックス値を求める
            // 更にインデックス値が後方へ移動する場合は削除分ズレるので-1する
            var newIndex = currentIndex + count + (count > 0 ? -1 : 0);
            if (newIndex < 0) newIndex = 0;
            if (newIndex > subLoopSystemList.Count) newIndex = subLoopSystemList.Count;


            // 古いインデックスから値を取り出して削除した後新しいインデックスに挿入
            var subLoopSystem = subLoopSystemList[currentIndex];
            subLoopSystemList.RemoveAt(currentIndex);
            subLoopSystemList.Insert(newIndex, subLoopSystem);
            return true;
        }


        /// <summary>
        /// 指定された更新型でループシステムを探し出します。
        /// </summary>
        /// <typeparam name="T">検索するループシステムの型</typeparam>
        /// <param name="recursiveSearch">対象の型の検索を再帰的に行うかどうか</param>
        /// <returns>最初に見つけたループシステムを返しますが、見つけられなかった場合はnullを返します</returns>
        public ImtPlayerLoopSystem FindLoopSystem<T>(bool recursiveSearch)
        {
            // 自身のサブループシステムに該当の型があるか調べて、見つけたら
            var result = subLoopSystemList.Find(loopSystem => loopSystem.type == typeof(T));
            if (result != null)
            {
                // 結果を返す
                return result;
            }


            // 見つけられなく、かつ再帰検索でないのなら
            if (result == null && !recursiveSearch)
            {
                // 諦めてnullを返す
                return null;
            }


            // 自分のサブループシステムにも検索を問いかける
            return subLoopSystemList.Find(loopSystem => loopSystem.FindLoopSystem<T>(recursiveSearch) != null);
        }


        /// <summary>
        /// 指定された更新型で存在インデックス位置を取得します
        /// </summary>
        /// <typeparam name="T">検索するループシステムの型</typeparam>
        /// <returns>最初に見つけたループシステムのインデックスを返しますが、見つけられなかった場合は ImtPlayerLoopSystem.LoopSystemNotFoundValue をかえします</returns>
        public int IndexOf<T>()
        {
            // 自身のサブループシステムに該当の型があるか調べるが、見つけられなかったら
            var result = subLoopSystemList.FindIndex(loopSystem => loopSystem.type == typeof(T));
            if (result == -1)
            {
                // 見つけられなかったことを返す
                return LoopSystemNotFoundValue;
            }


            // 見つけた位置を返す
            return result;
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
        /// 指定された型を設定します
        /// </summary>
        /// <param name="type">変更する新しい型</param>
        /// <exception cref="ArgumentNullException">typeがnullです</exception>
        public void SetType(Type type)
        {
            // もしnullが渡されていたら
            if (type == null)
            {
                // 関数は死ぬ
                throw new ArgumentNullException(nameof(type));
            }


            // 指示された型を設定する
            this.type = type;
        }


        /// <summary>
        /// 指定された更新関数を設定します
        /// </summary>
        /// <param name="updateFunction">設定する新しい更新関数。nullを設定することができます</param>
        public void SetUpdateFunction(PlayerLoopSystem.UpdateFunction updateFunction)
        {
            // 更新関数を素直に設定する
            updateDelegate = updateFunction;
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