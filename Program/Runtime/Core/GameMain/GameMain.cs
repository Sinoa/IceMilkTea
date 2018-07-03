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

using UnityEngine;

namespace IceMilkTea.Core
{
    /// <summary>
    /// ゲームが終了要求に対する答えを表します
    /// </summary>
    public enum GameShutdownAnswer
    {
        /// <summary>
        /// ゲームが終了することを許可します
        /// </summary>
        Approve,

        /// <summary>
        /// ゲームが終了することを拒否します
        /// </summary>
        Reject,
    }



    /// <summary>
    /// ゲームメインクラスの実装をするための抽象クラスです。
    /// IceMilkTeaによるゲームのスタートアップからメインループを構築する場合は必ず継承し実装をして下さい。
    /// </summary>
    public abstract class GameMain : ScriptableObject
    {
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
            // ゲームメインをロードする
            var gameMain = LoadGameMain();
            CurrentContext = gameMain;


            // 永続ゲームオブジェクトを生成してMonoBehaviourのイベントブリッジコンポーネントをつける
            var persistentGameObject = CreatePersistentGameObject();
            MonoBehaviourEventBridge.Attach(persistentGameObject, gameMain.OnApplicationFocus, gameMain.OnApplicationPause);


            // アプリケーションのイベントハンドラを登録
            Application.wantsToQuit += gameMain.Internal_RequestShutdown;
            Application.quitting += gameMain.Internal_Shutdown;


            // GameMainを起動する
            gameMain.Startup();
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


            // リダイレクトするGameMainがあるか聞いて、存在するなら
            var redirectGameMain = gameMain.RedirectGameMain();
            if (redirectGameMain != null)
            {
                // ロードされたGameMainを解放して、リダイレクトされたGameMainを設定する
                Destroy(gameMain);
                gameMain = redirectGameMain;
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


        #region イベントハンドラ
        /// <summary>
        /// 起動するGameMainをリダイレクトします。
        /// IceMilkTeaによって起動されたGameMainから他のGameMainへリダイレクトする場合は、
        /// この関数をオーバーライドして起動するGameMainのインスタンスを返します。
        /// </summary>
        /// <returns>リダイレクトするGameMainがある場合はインスタンスを返しますが、ない場合はnullを返します</returns>
        protected virtual GameMain RedirectGameMain()
        {
            // リダイレクト先GameMainはなし
            return null;
        }


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
        /// このGameMainクラスのための RequestShutdown 関数です。
        /// </summary>>
        /// <returns>終了を許可する場合は true を、禁止する場合は false を返します</returns>
        private bool Internal_RequestShutdown()
        {
            // 終了処理の要求をしてApproveならtrueを、それ以外ならfalseを返す
            return RequestShutdown() == GameShutdownAnswer.Approve ? true : false;
        }


        /// <summary>
        /// ゲームの終了処理の要求を処理します。
        /// ゲームが終了してよいのかどうかを判断し終了のコントロールを行います。
        /// </summary>
        /// <returns>終了を許可する場合は GameShutdownAnswer.Approve を、禁止する場合は GameShutdownAnswer.Reject を返します</returns>
        protected virtual GameShutdownAnswer RequestShutdown()
        {
            // 通常は終了を許容する
            return GameShutdownAnswer.Approve;
        }


        /// <summary>
        /// このGameMainクラスのための Shutdown 関数です。
        /// </summary>
        private void Internal_Shutdown()
        {
            // 終了を叩く
            Shutdown();
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