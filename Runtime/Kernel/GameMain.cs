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
using UnityEngine.PlayerLoop;

namespace IceMilkTea.Core
{
    /// <summary>
    /// ゲームメインクラスの実装をするための抽象クラスです。
    /// IceMilkTeaによるゲームのスタートアップからメインループを構築する場合は必ず継承し実装をして下さい。
    /// </summary>
    [HideCreateGameMainAssetMenu]
    public abstract class GameMain : ScriptableObject
    {
        private Action messagePumpHandler;



        #region プロパティ
        /// <summary>
        /// 現在のゲームメインコンテキストを取得します
        /// </summary>
        public static GameMain Current { get; private set; }


        /// <summary>
        /// 現在のゲームメインが保持しているサービスマネージャを取得します
        /// </summary>
        public GameServiceManager ServiceManager { get; private set; }
        #endregion



        #region エントリポイントとロジック関数
        /// <summary>
        /// Unity起動時に実行されるゲームのエントリポイントです
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Main()
        {
            // ゲームメインをロードする
            Current = LoadGameMain();


            // IceMilkTeaはこのまま起動を継続してはいけないのなら
            if (!Current.Continue())
            {
                // ロードしたばかりのGameMainを解放して起動を中止する
                Current = null;
                Resources.UnloadUnusedAssets();
                return;
            }


            // サービスマネージャのインスタンスを生成するが、nullが返却されるようなことがあれば
            Current.ServiceManager = new GameServiceManager();
            InstallSynchronizationContext();
            RegisterHandler();
            Current.ServiceManager.Startup();
            Current.Startup();
        }


        /// <summary>
        /// 指定されたゲームメインによって動作を上書きします。
        /// この関数は、テストの為に用意された関数であり、通常のゲームロジック上で使用される想定はありません。
        /// 動作の切り替えや、想定のゲームメイン動作を設定する場合は GameMain.RedirectGameMain 関数をオーバーライドして下さい。
        /// </summary>
        /// <param name="gameMain">上書きするゲームメインの参照</param>
        internal static void OverrideGameMain(GameMain gameMain)
        {
            // 起動中のゲームメインがあるのなら
            if (Current != null)
            {
                // 何があろうとシャットダウンして、ImtPlayerLoopSystemから既定Unityループシステムを読み込んで上書きすることですべての登録を破壊できる
                InternalShutdown();
                ImtPlayerLoopSystem.GetDefaultPlayerLoop().BuildAndSetUnityPlayerLoop();
            }


            // 渡されたゲームメインを設定して初期化を実行する
            Current = gameMain;
            Current.ServiceManager = new GameServiceManager();
            RegisterHandler();
            Current.ServiceManager.Startup();
            Current.Startup();
        }


        /// <summary>
        /// Unityのアプリケーション終了時に処理するべき後処理を行います
        /// </summary>
        private static void InternalShutdown()
        {
            UnregisterHandler();
            UninstallSynchronizationContext();
            Current.ServiceManager.Shutdown();
            Current.Shutdown();
        }


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
                // リダイレクトされたGameMainを設定して、ロードされたGameMainを解放
                gameMain = redirectGameMain;
                Resources.UnloadUnusedAssets();
            }


            // ロードしたゲームメインを返す
            return gameMain;
        }


        private static void InstallSynchronizationContext()
        {
            ImtSynchronizationContext.Install(out Current.messagePumpHandler);
        }


        private static void UninstallSynchronizationContext()
        {
            ImtSynchronizationContext.Uninstall();
        }


        /// <summary>
        /// GameMainの動作に必要なハンドラの登録処理を行います
        /// </summary>
        private static void RegisterHandler()
        {
            // アプリケーションの終了イベントを引っ掛けておく
            Application.quitting += InternalShutdown;


            var mainUpdate = new ImtPlayerLoopSystem(typeof(GameMain), Current.UpdateCore);
            var loopSystem = ImtPlayerLoopSystem.GetCurrentPlayerLoop();
            loopSystem.Insert<TimeUpdate.WaitForLastPresentationAndUpdateTime>(InsertTiming.AfterInsert, mainUpdate);
            loopSystem.BuildAndSetUnityPlayerLoop();
        }


        /// <summary>
        /// GameMainの動作に必要なハンドラの解除処理を行います
        /// </summary>
        private static void UnregisterHandler()
        {
            // アプリケーション終了イベントを外す
            // （PlayerLoopSystemはPlayerLoopSystem自身が登録解除まで担保してくれているのでそのまま）
            Application.quitting -= InternalShutdown;
        }



        private void UpdateCore()
        {
            messagePumpHandler();
            Update();
        }
        #endregion


        #region オーバーライド可能なGameMainのハンドラ関数
        /// <summary>
        /// IceMilkTeaのシステムがこのまま継続して起動するかどうかを判断します
        /// </summary>
        /// <returns>起動を継続する場合は true を、継続しない場合は false を返します</returns>
        protected virtual bool Continue()
        {
            // 通常は起動を継続する
            return true;
        }


        /// <summary>
        /// ゲームの起動処理を行います。
        /// 主に、ゲームサービスの初期登録や必要な追加モジュールの初期化などを行います。
        /// </summary>
        protected virtual void Startup()
        {
        }


        /// <summary>
        /// ゲームの終了処理を行います。
        /// ゲームサービスそのものの終了処理は、サービス側で処理されるべきで、
        /// この関数では主に、追加モジュールなどの解放やサービス管轄外の解放などを行うべきです。
        /// </summary>
        protected virtual void Shutdown()
        {
        }


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
        /// ゲームのメインループ処理を行います。
        /// </summary>
        protected virtual void Update()
        {
        }
        #endregion



        #region SafeGameMain実装
        /// <summary>
        /// 起動するべきGameMainが見つからなかった場合や、起動できない場合において
        /// 代わりに起動するための GameMain クラスです。
        /// </summary>
        [HideCreateGameMainAssetMenu]
        private class SafeGameMain : GameMain
        {
            /// <summary>
            /// セーフ起動時のIceMilkTeaは、起動を継続しないようにします。
            /// </summary>
            /// <returns>この関数は常にfalseを返します</returns>
            protected override bool Continue()
            {
                // 起動を止めるようにする
                return false;
            }
        }
        #endregion
    }
}