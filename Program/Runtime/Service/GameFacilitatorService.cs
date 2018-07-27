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
using IceMilkTea.Core;

namespace IceMilkTea.Service
{
    /// <summary>
    /// ゲームシーンを破棄する理由を表します
    /// </summary>
    public enum GameSceneDestroyReason
    {
    }



    /// <summary>
    /// ゲームのシーンとして制御する基本クラスです
    /// </summary>
    public abstract class GameScene
    {
        #region シーンイベントハンドラ
        /// <summary>
        /// シーンの初期化を行います
        /// </summary>
        protected internal virtual void Initialize()
        {
        }


        /// <summary>
        /// シーンの停止処理を行います
        /// </summary>
        protected internal virtual void Terminate()
        {
        }


        /// <summary>
        /// シーンの一時停止処理を行います
        /// </summary>
        protected internal virtual void Sleep()
        {
        }


        /// <summary>
        /// シーンの再開処理を行います
        /// </summary>
        protected internal virtual void Resume()
        {
        }


        /// <summary>
        /// シーンの更新処理を行います
        /// </summary>
        protected internal virtual void Update()
        {
        }
        #endregion
    }



    /// <summary>
    /// ゲーム進行を行うサービスクラスです
    /// </summary>
    public class GameFacilitatorService : GameService
    {
        #region シーン管理用データ型定義
        /// <summary>
        /// シーンの状態を表します
        /// </summary>
        private enum SceneState
        {
            /// <summary>
            /// シーンの開始準備状態です
            /// </summary>
            Ready,

            /// <summary>
            /// シーンが稼働中です
            /// </summary>
            Run,

            /// <summary>
            /// シーンは休止中です
            /// </summary>
            Sleep,

            /// <summary>
            /// シーンが停止します
            /// </summary>
            Shutdown,
        }



        /// <summary>
        /// シーンの管理コンテキストクラスです
        /// </summary>
        private class SceneManagementContext
        {
            /// <summary>
            /// シーン本体そのもの
            /// </summary>
            public GameScene Scene { get; set; }


            /// <summary>
            /// シーンの状態
            /// </summary>
            public SceneState State { get; set; }
        }
        #endregion



        // メンバ変数定義
        private List<SceneManagementContext> sceneManagementContextList;



        #region コンストラクタと起動停止処理部
        /// <summary>
        /// GameFacilitatorService のインスタンスを初期化します
        /// </summary>
        public GameFacilitatorService()
        {
            // シーン管理コンテキストリストインスタンスの生成
            sceneManagementContextList = new List<SceneManagementContext>();
        }


        /// <summary>
        /// サービスの起動処理を行います
        /// </summary>
        /// <param name="info">起動情報を設定します</param>
        protected internal override void Startup(out GameServiceStartupInfo info)
        {
            // 起動情報の設定をする
            info = new GameServiceStartupInfo()
            {
                // サービスの更新関数テーブルを構築する
                UpdateFunctionTable = new Dictionary<GameServiceUpdateTiming, Action>()
                {
                    { GameServiceUpdateTiming.MainLoopHead, UpdateService }
                }
            };
        }


        /// <summary>
        /// サービスの停止処理を行います
        /// </summary>
        protected internal override void Shutdown()
        {
        }
        #endregion


        #region サービスの更新
        /// <summary>
        /// サービスの更新を行います
        /// </summary>
        private void UpdateService()
        {
            // 実行状態のシーンだけ更新処理をする
            foreach (var sceneManagementContext in sceneManagementContextList)
            {
                // もしシーンが実行状態でないのなら
                if (sceneManagementContext.State != SceneState.Run)
                {
                    // 次のシーンへ
                    continue;
                }


                // シーンの更新処理を呼ぶ
                sceneManagementContext.Scene.Update();
            }
        }
        #endregion


        #region シーンリスト操作系
        public bool ClearSceneStack()
        {
            return false;
        }


        public void ChangeScene(GameScene newScene)
        {
        }


        public void PushScene(GameScene newScene, bool currentSceneIsSleep)
        {
        }


        public void PopScene()
        {
        }


        public GameScene FindTopMostScene<TScene>() where TScene : GameScene
        {
            return null;
        }


        public GameScene FindBottomMostScene<TScene>() where TScene : GameScene
        {
            return null;
        }


        public GameScene[] FindAllScene<TScene>() where TScene : GameScene
        {
            return null;
        }


        public int FindAllSceneNonAlloc<TScene>(GameScene[] result) where TScene : GameScene
        {
            return 0;
        }
        #endregion
    }
}