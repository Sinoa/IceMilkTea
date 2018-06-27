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
using UnityEngine;

namespace IceMilkTea.Core
{
    /// <summary>
    /// ゲームメインクラスの実装をするための抽象クラスです。
    /// IceMilkTeaによるゲームのスタートアップからメインループを構築する場合は必ず継承し実装をして下さい。
    /// </summary>
    public abstract class GameMain : ScriptableObject
    {
        // 以下クラス変数宣言
        private static GameObject persistentGameObject;



        /// <summary>
        /// 現在のゲームメインコンテキストを取得します
        /// </summary>
        public static GameMain CurrentContext { get; private set; }



        /// <summary>
        /// Unity起動時に実行されるゲームのエントリポイントです
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Main()
        {
            // 永続ゲームオブジェクトを生成してMonoBehaviourのイベントブリッジコンポーネントをつける
            persistentGameObject = CreatePersistentGameObject();
            MonoBehaviourEventBridge.Attach(persistentGameObject, Internal_OnApplicationFocus, Internal_OnApplicationPause);


            // アプリケーションのイベントハンドラを登録
            Application.wantsToQuit += Internal_RequestShutdown;
            Application.quitting += Internal_Shutdown;


            // ゲームメインをロードする
            CurrentContext = LoadGameMain();


            // GameMainを起動する
            Internal_Startup();
        }


        #region GameMain用ロジック群
        /// <summary>
        /// ゲームメインをロードします
        /// </summary>
        /// <returns>ロードされたゲームメインを返します</returns>
        private static GameMain LoadGameMain()
        {
            // 内部で保存されたGameMainのGameMainをロードする
            var gameMain = Resources.Load<GameMain>("GameMain");


            // ロードが出来なかったのなら
            if (gameMain == null)
            {
                // セーフ起動用のゲームメインで立ち上げる
                return CreateInstance<SafeGameMain>();
            }


            // ロードしたゲームメインを返す
            return gameMain;
        }


        /// <summary>
        /// ゲームが起動してから消えるまで永続的に存在し続けるゲームオブジェクトを生成します。
        /// ここで生成されるゲームオブジェクトはヒエラルキに表示されません
        /// </summary>
        /// <returns>生成された永続ゲームオブジェクトを返します</returns>
        private static GameObject CreatePersistentGameObject()
        {
            // ゲームオブジェクトを生成する
            var gameObject = new GameObject("__IceMilkTea_Persistent_GameObject__");


            // ヒエラルキから姿を消して永続化
            gameObject.hideFlags = HideFlags.HideInHierarchy;
            DontDestroyOnLoad(gameObject);


            // トランスフォームを取得して念の為初期値を入れる
            var transform = gameObject.GetComponent<Transform>();
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;


            // 作ったゲームオブジェクトを返す
            return gameObject;
        }
        #endregion


        #region 永続ゲームオブジェクト用ロジック群
        /// <summary>
        /// GameMainが保有している永続ゲームオブジェクトに対象のコンポーネントをアタッチします
        /// </summary>
        /// <typeparam name="T">アタッチするコンポーネントの型</typeparam>
        /// <returns>アタッチされたコンポーネントのインスタンスを返します</returns>
        public T AddComponent<T>() where T : Component
        {
            // コンポーネントをアタッチして返す
            return persistentGameObject.AddComponent<T>();
        }


        /// <summary>
        /// GameMainが保有している永続ゲームオブジェクトに対象のゲームオブジェクトを子供として配置します。
        /// また、配置する際に追加するゲームオブジェクトのワールド姿勢は維持されます。
        /// </summary>
        /// <param name="child">永続ゲームオブジェクトの子にするゲームオブジェクト</param>
        public void AddChildGameObject(GameObject child)
        {
            // ワールド姿勢を維持したまま子に追加
            AddChildGameObject(child, true);
        }


        /// <summary>
        /// GameMainが保有している永続ゲームオブジェクトに対象のゲームオブジェクトを子供として配置します
        /// </summary>
        /// <param name="child">永続ゲームオブジェクトの子にするゲームオブジェクト</param>
        /// <param name="worldPositionStays">子になるゲームオブジェクトのワールド姿勢を維持するか否か</param>
        /// <exception cref="ArgumentNullException">childがnullです</exception>
        public void AddChildGameObject(GameObject child, bool worldPositionStays)
        {
            // 引数チェック
            if (child == null)
            {
                // nullは受け付けられない
                throw new ArgumentNullException(nameof(child));
            }


            // 対象のゲームオブジェクトの親は永続ゲームオブジェクト
            child.transform.SetParent(persistentGameObject.transform, worldPositionStays);
        }


        /// <summary>
        /// GameMainが保有している永続ゲームオブジェクトに新しくゲームオブジェクトを生成します
        /// </summary>
        /// <param name="name">新しく生成するゲームオブジェクト名。nullまたは空白文字列の場合は"NewGameObject"の名前が採用されます</param>
        /// <returns>生成したゲームオブジェクトを返します</returns>
        public GameObject CreateChildGameObject(string name)
        {
            // ゲームオブジェクトを生成して子供の追加をする
            var gameObject = new GameObject(string.IsNullOrWhiteSpace(name) ? "NewGameObject" : name);
            AddChildGameObject(gameObject);


            // 生成したゲームオブジェクトを返す
            return gameObject;
        }
        #endregion


        #region 内部イベントハンドラ
        /// <summary>
        /// このGameMainクラスのための Startup 関数です。
        /// </summary>
        private static void Internal_Startup()
        {
            // 起動を叩く
            CurrentContext.Startup();
        }


        /// <summary>
        /// このGameMainクラスのための OnApplicationFocus 関数です。
        /// </summary>
        /// <param name="focus">フォーカスを得られたときはtrueを、失ったときはfalse</param>
        private static void Internal_OnApplicationFocus(bool focus)
        {
            // フォーカス変化イベントを叩く
            CurrentContext.OnApplicationFocus(focus);
        }


        /// <summary>
        /// このGameMainクラスのための OnApplicationPause 関数です。
        /// </summary>
        /// <param name="pause">一時停止になったときはtrueを、再生状態になったときはfalse</param>
        private static void Internal_OnApplicationPause(bool pause)
        {
            // ポーズ変化イベントを叩く
            CurrentContext.OnApplicationPause(pause);
        }


        /// <summary>
        /// このGameMainクラスのための RequestShutdown 関数です。
        /// </summary>>
        /// <returns>修了を許可する場合はtrueを、禁止する場合はfalseを返します</returns>
        private static bool Internal_RequestShutdown()
        {
            // 終了処理の要求をして結果をそのまま返す
            return CurrentContext.RequestShutdown();
        }


        /// <summary>
        /// このGameMainクラスのための Shutdown 関数です。
        /// </summary>
        private static void Internal_Shutdown()
        {
            // 終了を叩く
            CurrentContext.Shutdown();
        }
        #endregion


        #region 外部イベントハンドラ
        /// <summary>
        /// ゲームの起動処理を行います。
        /// サービスの初期化や他のサブシステムの初期化などを主に行います。
        /// </summary>
        protected virtual void Startup()
        {
        }


        /// <summary>
        /// ゲームアプリケーションがウィンドウやプレイヤーなどのフォーカスの状態が変化したときの処理を行います
        /// </summary>
        /// <param name="focus">フォーカスを得られたときはtrueを、失ったときはfalse</param>
        protected virtual void OnApplicationFocus(bool focus)
        {
        }


        /// <summary>
        /// ゲームアプリケーションの再生状態が変化したときの処理を行います
        /// </summary>
        /// <param name="pause">一時停止になったときはtrueを、再生状態になったときはfalse</param>
        protected virtual void OnApplicationPause(bool pause)
        {
        }


        /// <summary>
        /// ゲームの終了処理の要求を処理します。
        /// ゲームが終了してよいのかどうかを判断し修了のコントロールを行います。
        /// </summary>
        /// <returns>修了を許可する場合はtrueを、禁止する場合はfalseを返します</returns>
        protected virtual bool RequestShutdown()
        {
            // 通常は終了を許容する
            return true;
        }


        /// <summary>
        /// ゲームの終了処理を行います。
        /// サービスの終了処理や他のサブシステムの終了処理などを主に行います。
        /// </summary>
        protected virtual void Shutdown()
        {
        }
        #endregion
    }
}