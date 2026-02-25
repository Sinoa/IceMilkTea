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
    public abstract class GameMain
    {
        #region プロパティ
        /// <summary>
        /// 現在のゲームメインコンテキストを取得します
        /// </summary>
        public static GameMain Current { get; private set; }


        /// <summary>
        /// 現在のゲームメインが保持しているサービスマネージャを取得します
        /// </summary>
        public GameServiceManager ServiceManager { get; private set; }


        /// <summary>
        /// 現在のゲームメインが保持しているゲームコンフィグを取得します
        /// </summary>
        public IGameConfig Config { get; private set; }
        #endregion



        #region エントリポイントとロジック関数
        /// <summary>
        /// 指定されたゲームメインでフレームワークを起動します。
        /// 利用者は <see cref="GameMainAttribute"/> を付与した静的メソッドからこのメソッドを呼び出して下さい。
        /// </summary>
        /// <param name="gameMain">起動するゲームメインのインスタンス</param>
        /// <exception cref="ArgumentNullException"><paramref name="gameMain"/> が null です</exception>
        /// <exception cref="InvalidOperationException">既にゲームメインが起動しています</exception>
        public static void Run(GameMain gameMain)
        {
            if (gameMain == null)
            {
                throw new ArgumentNullException(nameof(gameMain));
            }

            if (Current != null)
            {
                throw new InvalidOperationException("既にゲームメインが起動しています。二重起動はできません。");
            }

            InitializeAndStart(gameMain);
        }


        /// <summary>
        /// ゲームメインの初期化と起動を行います
        /// </summary>
        /// <param name="gameMain">起動するゲームメインのインスタンス</param>
        private static void InitializeAndStart(GameMain gameMain)
        {
            Current = gameMain;
            Current.Config = Current.CreateConfig();
            Current.ServiceManager = new GameServiceManager();
            RegisterHandler();
            Current.Startup();
            Current.ServiceManager.Startup();
        }


        /// <summary>
        /// Unityのアプリケーション終了時に処理するべき後処理を行います
        /// </summary>
        private static void InternalShutdown()
        {
            UnregisterHandler();
            Current.ServiceManager.Shutdown();
            Current.Shutdown();
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
            Update();
        }
        #endregion


        #region オーバーライド可能なGameMainのハンドラ関数
        /// <summary>
        /// ゲームコンフィグを生成します。
        /// アプリケーション固有のコンフィグを使用する場合は、この関数をオーバーライドして <see cref="IGameConfig"/> の実装を返して下さい。
        /// </summary>
        /// <returns>ゲームコンフィグのインスタンスを返します</returns>
        protected virtual IGameConfig CreateConfig()
        {
            return new NullGameConfig();
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
        /// ゲームのメインループ処理を行います。
        /// </summary>
        protected virtual void Update()
        {
        }
        #endregion
    }
}
