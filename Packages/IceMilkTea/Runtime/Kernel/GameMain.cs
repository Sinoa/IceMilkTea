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
    /// <remarks>
    /// このクラスは Domain Reload を無効化した状態でも安全に複数回起動できるよう、 Play Mode 開始時に静的状態をリセットします。
    /// 派生クラスで独自の静的フィールドを保持する場合は、 Domain Reload 無効時にそれらが Play Mode をまたいで残存することに注意し、
    /// 派生クラス側で <see cref="UnityEngine.RuntimeInitializeOnLoadMethodAttribute"/> などを用いて初期化して下さい。
    /// </remarks>
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
        /// このゲームメインでフレームワークを起動します。
        /// 利用者は <see cref="GameMainAttribute"/> を付与した静的メソッドからこのメソッドを呼び出して下さい。
        /// </summary>
        /// <exception cref="InvalidOperationException">既にゲームメインが起動しています</exception>
        public void Run()
        {
            if (Current != null)
            {
                throw new InvalidOperationException("既にゲームメインが起動しています。二重起動はできません。");
            }

            InitializeAndStart();
        }


        /// <summary>
        /// 起動済みの <see cref="GameServiceManager"/> を停止してから、 <see cref="OnRestart"/> によるサービス再登録を行い、
        /// 再び <see cref="GameServiceManager"/> を起動します。
        /// <see cref="Run"/> のように <see cref="GameMain"/> 自体を作り直さずにゲームを再起動したい場合に使用します。
        /// 既定の <see cref="OnRestart"/> は <see cref="Startup"/> を呼び出すため、 オーバーライドしていない場合は初回起動と同じ登録処理が再実行されます。
        /// </summary>
        /// <exception cref="InvalidOperationException">本インスタンスが現在の <see cref="Current"/> ではない場合</exception>
        public void Restart()
        {
            if (Current != this)
            {
                throw new InvalidOperationException("本インスタンスは現在起動中の GameMain ではありません。Run() で起動した GameMain のみ Restart() を呼べます。");
            }

            ServiceManager.Shutdown();
            OnRestart();
            ServiceManager.Startup();
        }


        /// <summary>
        /// ゲームメインの初期化と起動を行います
        /// </summary>
        private void InitializeAndStart()
        {
            Current = this;
            Config = CreateConfig();
            ServiceManager = new GameServiceManager();
            RegisterHandler();
            Startup();
            ServiceManager.Startup();
        }


        /// <summary>
        /// Unityのアプリケーション終了時に処理するべき後処理を行います
        /// </summary>
        private static void InternalShutdown()
        {
            // ハンドラ解除・サービス停止・ゲーム終了処理を行い、最後に必ず Current をクリアする
            // （Domain Reload が無効な状態でも Current が次の Play Mode へ残存しないようにするため）
            try
            {
                UnregisterHandler();
                Current.ServiceManager.Shutdown();
                Current.Shutdown();
            }
            finally
            {
                Current = null;
            }
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


        #if UNITY_EDITOR
        /// <summary>
        /// Domain Reload が無効な状態でも Play Mode 開始時に <see cref="Current"/> を確実に初期化するためのリセット処理です。
        /// このメソッドは Editor 専用で、 Play Mode に入るたびに <see cref="Run"/> より前に呼び出されます。
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticStateOnEnterPlayMode()
        {
            // 前回の Play Mode で InternalShutdown が異常終了し Current が残存した場合に備え、保険でクリアする
            Current = null;
        }
        #endif
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
        /// <see cref="Restart"/> によるゲーム再起動時に呼び出されるサービス再登録用のフックです。
        /// 既定の実装では <see cref="Startup"/> を呼び出し、 初回起動と同じ登録処理を再実行します。
        /// 再起動時に登録するサービスを最小限に絞りたい場合などにオーバーライドして下さい。
        /// </summary>
        protected virtual void OnRestart()
        {
            Startup();
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
