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
using UnityEngine.LowLevel;
using UnityObject = UnityEngine.Object;

namespace IceMilkTea.Core
{
    #region GameMain
    /// <summary>
    /// GameMain クラスのアセット生成ツールメニューの非表示を示す属性クラスです
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class HideCreateGameMainAssetMenuAttribute : Attribute
    {
    }
    #endregion



    #region PlayerLoop
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
    /// ループシステムに挿入するユーザーカスタムの更新抽象クラスです
    /// </summary>
    public abstract class PlayerLoopUpdater
    {
        /// <summary>
        /// ループシステムによって実行される更新関数です
        /// </summary>
        protected internal abstract void Update();
    }



    /// <summary>
    /// PlayerLoopSystem構造体の内容をクラスとして表現されPlayerLoopSystemの順序を操作するための機能を提供しています
    /// </summary>
    public class ImtPlayerLoopSystem
    {
        /// <summary>
        /// ループシステムの検索で、対象のループシステムを見つけられなかったときに返す値です
        /// </summary>
        public const int LoopSystemNotFoundValue = -1;

        // メンバ変数定義
        private Type type;
        private List<ImtPlayerLoopSystem> subLoopSystemList;
        private PlayerLoopSystem.UpdateFunction updateDelegate;
        private IntPtr updateFunction;
        private IntPtr loopConditionFunction;



        #region 初期化＆終了実装
        /// <summary>
        /// クラスの初期化を行います
        /// </summary>
        static ImtPlayerLoopSystem()
        {
            // アプリケーション終了イベントを登録する
            Application.quitting += OnApplicationQuit;
        }


        /// <summary>
        /// Unityがアプリケーションの終了をする時に呼び出されます
        /// </summary>
        private static void OnApplicationQuit()
        {
            //イベントの登録を解除してUnityの既定ループ機構に戻す
            Application.quitting -= OnApplicationQuit;
            PlayerLoop.SetPlayerLoop(PlayerLoop.GetDefaultPlayerLoop());
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
            subLoopSystemList = new List<ImtPlayerLoopSystem>();


            // もしサブシステムが有効な数で存在するなら
            if (originalPlayerLoopSystem.subSystemList != null && originalPlayerLoopSystem.subSystemList.Length > 0)
            {
                // 再帰的にコピーを生成する
                subLoopSystemList.AddRange(originalPlayerLoopSystem.subSystemList.Select(original => new ImtPlayerLoopSystem(ref original)));
            }
        }


        /// <summary>
        /// 指定された型と更新関数でインスタンスの初期化を行います
        /// </summary>
        /// <param name="type">生成するPlayerLoopSystemの型</param>
        /// <param name="updateDelegate">生成するPlayerLoopSystemの更新関数。更新関数が不要な場合はnullの指定が可能です</param>
        /// <exception cref="ArgumentNullException">type が null です</exception>
        /// <exception cref="ArgumentNullException">updateDelegate が null です</exception>
        public ImtPlayerLoopSystem(Type type, PlayerLoopSystem.UpdateFunction updateDelegate)
        {
            // シンプルに初期化をする
            this.type = type ?? throw new ArgumentNullException(nameof(type));
            this.updateDelegate = updateDelegate ?? throw new ArgumentNullException(nameof(updateDelegate));
            subLoopSystemList = new List<ImtPlayerLoopSystem>();
        }


        /// <summary>
        /// 指定されたアップデータを動かすPlayerLoopSystemのインスタンスを初期化します
        /// </summary>
        /// <param name="updater">PlayerLoopSystemによって動作させるアップデータ</param>
        /// <exception cref="ArgumentNullException">updater が null です</exception>
        public ImtPlayerLoopSystem(PlayerLoopUpdater updater)
        {
            // シンプルに初期化をする
            type = (updater ?? throw new ArgumentNullException(nameof(updater))).GetType();
            updateDelegate = updater.Update;
            subLoopSystemList = new List<ImtPlayerLoopSystem>();
        }
        #endregion


        #region Unity変換関数群
        /// <summary>
        /// Unityの標準プレイヤーループを ImtPlayerLoopSystem として取得します
        /// </summary>
        /// <returns>Unityの標準プレイヤーループをImtPlayerLoopSystemにキャストされた結果を返します</returns>
        public static ImtPlayerLoopSystem GetDefaultPlayerLoop()
        {
            // キャストして返すだけ
            return (ImtPlayerLoopSystem)PlayerLoop.GetDefaultPlayerLoop();
        }


        /// <summary>
        /// Unityの現在設定されているプレイヤーループを ImtPlayerLoopSystem として取得します
        /// </summary>
        /// <returns></returns>
        public static ImtPlayerLoopSystem GetCurrentPlayerLoop()
        {
            // キャストして返すだけ
            return (ImtPlayerLoopSystem)PlayerLoop.GetCurrentPlayerLoop();
        }


        /// <summary>
        /// このインスタンスを本来の構造へ構築し、Unityのプレイヤーループへ設定します
        /// </summary>
        public void BuildAndSetUnityPlayerLoop()
        {
            // 自身をキャストして設定するだけ
            PlayerLoop.SetPlayerLoop((PlayerLoopSystem)this);
        }
        #endregion


        #region コントロール関数群
        /// <summary>
        /// 指定された型の更新ループに対して、ループシステムをタイミングの位置に挿入します
        /// </summary>
        /// <typeparam name="T">これから挿入するループシステムの挿入起点となる更新型</typeparam>
        /// <param name="timing">T で指定された更新ループを起点にどのタイミングで挿入するか</param>
        /// <param name="loopSystem">挿入するループシステム</param>
        /// <exception cref="ArgumentNullException">loopSystemがnullです</exception>
        /// <returns>対象のループシステムが挿入された場合はtrueを、挿入されなかった場合はfalseを返します</returns>
        public bool Insert<T>(InsertTiming timing, ImtPlayerLoopSystem loopSystem)
        {
            // 再帰検索を有効にして本来の挿入関数を叩く
            return Insert<T>(timing, loopSystem, true);
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
        public bool Insert<T>(InsertTiming timing, ImtPlayerLoopSystem loopSystem, bool recursiveSearch)
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
                        if (subLoopSystem.Insert<T>(timing, loopSystem, recursiveSearch))
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
        public bool Remove<T>(bool recursiveSearch)
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
                        if (subLoopSystem.Remove<T>(recursiveSearch))
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
        /// 指定された更新型でループシステムを探し出します。
        /// </summary>
        /// <typeparam name="T">検索するループシステムの型</typeparam>
        /// <param name="recursiveSearch">対象の型の検索を再帰的に行うかどうか</param>
        /// <returns>最初に見つけたループシステムを返しますが、見つけられなかった場合はnullを返します</returns>
        public ImtPlayerLoopSystem Find<T>(bool recursiveSearch)
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
            return subLoopSystemList.Find(loopSystem => loopSystem.Find<T>(recursiveSearch) != null);
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
        /// クラス化されているPlayerLoopSystemを構造体のPlayerLoopSystemへ変換します。
        /// また、サブループシステムを保持している場合はサブループシステムも構造体のインスタンスが新たに生成され、初期化されます。
        /// </summary>
        /// <returns>内部コンテキストのコピーを行ったPlayerLoopSystemを返します</returns>
        private PlayerLoopSystem ToPlayerLoopSystem()
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


        /// <summary>
        /// ImpPlayerLoopSystem内のLoopSystem階層表示を文字列へ変換します
        /// </summary>
        /// <returns>このインスタンスのLoopSystem階層状況を文字列化したものを返します</returns>
        public override string ToString()
        {
            // バッファを用意してループシステムツリーの内容をダンプして結果を返す
            var buffer = new StringBuilder();
            DumpLoopSystemTree(buffer, string.Empty);
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
    #endregion



    #region Utility
    /// <summary>
    /// Unity関連実装でユーティリティな関数として使えるような、関数が実装されているクラスです
    /// </summary>
    public static class ImtUnityUtility
    {
        /// <summary>
        /// 永続的に存在し続けるゲームオブジェクトを生成します。
        /// この関数で生成されるゲームオブジェクトはヒエラルキに表示されません。
        /// また、名前はNewGameObjectとして作られます。
        /// </summary>
        /// <returns>生成された永続ゲームオブジェクトを返します</returns>
        public static GameObject CreatePersistentGameObject()
        {
            // "NewGameObject" な見えないゲームオブジェクトを生成して返す
            return CreatePersistentGameObject("NewGameObject", HideFlags.HideInHierarchy);
        }


        /// <summary>
        /// 永続的に存在し続けるゲームオブジェクトを生成します。
        /// この関数で生成されるゲームオブジェクトはヒエラルキに表示されません。
        /// </summary>
        /// <param name="name">生成する永続ゲームオブジェクトの名前</param>
        /// <returns>生成された永続ゲームオブジェクトを返します</returns>
        public static GameObject CreatePersistentGameObject(string name)
        {
            // 見えないゲームオブジェクトを生成して返す
            return CreatePersistentGameObject(name, HideFlags.HideInHierarchy);
        }


        /// <summary>
        /// 永続的に存在し続けるゲームオブジェクトを生成します。
        /// </summary>
        /// <param name="name">生成する永続ゲームオブジェクトの名前</param>
        /// <param name="hideFlags">生成する永続ゲームオブジェクトの隠しフラグ</param>
        /// <returns>生成された永続ゲームオブジェクトを返します</returns>
        public static GameObject CreatePersistentGameObject(string name, HideFlags hideFlags)
        {
            // ゲームオブジェクトを生成する
            var gameObject = new GameObject(name);


            // ヒエラルキから姿を消して永続化
            gameObject.hideFlags = hideFlags;
            UnityObject.DontDestroyOnLoad(gameObject);


            // トランスフォームを取得して念の為初期値を入れる
            var transform = gameObject.GetComponent<Transform>();
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;


            // 作ったゲームオブジェクトを返す
            return gameObject;
        }
    }
    #endregion
}