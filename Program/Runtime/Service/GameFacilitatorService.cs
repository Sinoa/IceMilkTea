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
        /// シーンの更新処理を行います
        /// </summary>
        protected internal virtual void Update()
        {
        }


        /// <summary>
        /// アプリケーションの要因でシーンの一時停止処理を行います
        /// </summary>
        protected internal virtual void OnApplicationSleep()
        {
        }


        /// <summary>
        /// アプリケーションの要因でシーンの再開処理を行います
        /// </summary>
        protected internal virtual void OnApplicationResume()
        {
        }


        /// <summary>
        /// アプリケーションの要因でフォーカスを失った処理を行います
        /// </summary>
        protected internal virtual void OnApplicationFocusOut()
        {
        }


        /// <summary>
        /// アプリケーションの要因でフォーカスを取得した処理を行います
        /// </summary>
        protected internal virtual void OnApplicationFocusIn()
        {
        }
        #endregion
    }



    /// <summary>
    /// ゲーム進行を行うサービスクラスです。
    /// ゲーム進行サービスは、シーンという単位でゲーム進行を管理し、シーンはまるでスタックのように管理します。
    /// </summary>
    public class GameFacilitatorService : GameService
    {
        #region シーン管理情報の型定義
        /// <summary>
        /// リストに存在するシーンのステータスを表します
        /// </summary>
        private enum SceneState
        {
            /// <summary>
            /// シーンの開始準備が完了しました
            /// </summary>
            Ready,

            /// <summary>
            /// シーンは稼働中です。
            /// ただし Update が呼び出されるかどうかについては、保証していません。
            /// </summary>
            Running,

            /// <summary>
            /// シーンは解放される事のマークをされています
            /// </summary>
            Destroy,

            /// <summary>
            /// シーンは動作開始準備完了状態だったが、解放される対象としてマークされました
            /// </summary>
            ReadyedButDestroy,
        }



        /// <summary>
        /// SceneState 列挙型の内容で、破棄対象かどうかを判定する関数です
        /// </summary>
        /// <param name="state">判定する SceneState の値</param>
        /// <returns>渡された SceneState の値が Destroy系（破棄対象マーク値）なら true を、異なる場合は false を返します</returns>
        private bool IsDestroy(SceneState state)
        {
            // Destroy系ステータスの時は true を返す
            return state == SceneState.Destroy || state == SceneState.ReadyedButDestroy;
        }


        /// <summary>
        /// SceneState 列挙型の内容で、準備完了かどうかを判定する関数です
        /// </summary>
        /// <param name="state">判定する SceneState の値</param>
        /// <returns>渡された SceneState の値が Ready系（動作開始準備完了状態値）なら true を、異なる場合は false を返します</returns>
        private bool IsReady(SceneState state)
        {
            // Ready系ステータスの時は true を返す
            return state == SceneState.Ready;
        }


        /// <summary>
        /// SceneState 列挙型の内容で、動作中かどうかを判定する関数です
        /// </summary>
        /// <param name="state">判定する SceneState の値</param>
        /// <returns>渡された SceneState の値が Running系（動作状態値）なら true を、異なる場合は false を返します</returns>
        private bool IsRunning(SceneState state)
        {
            // Running系ステータスの時は true を返す
            return state == SceneState.Running;
        }



        /// <summary>
        /// シーンの管理状態を保持するコンテキストクラスです
        /// </summary>
        private class SceneManagementContext
        {
            /// <summary>
            /// 管理対象になっているシーン本体
            /// </summary>
            public GameScene Scene { get; set; }


            /// <summary>
            /// シーンの管理状態
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
            // シーン管理リストの初期化
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
                    { GameServiceUpdateTiming.MainLoopHead, UpdateService },
                    { GameServiceUpdateTiming.MainLoopTail, FinalFrameUpdate },
                    { GameServiceUpdateTiming.OnApplicationSuspend, OnApplicationSuspend },
                    { GameServiceUpdateTiming.OnApplicationResume, OnApplicationResume },
                    { GameServiceUpdateTiming.OnApplicationFocusOut, OnApplicationFocusOut },
                    { GameServiceUpdateTiming.OnApplicationFocusIn, OnApplicationFocusIn },
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
        }


        /// <summary>
        /// フレーム最後のサービス更新を行います
        /// </summary>
        private void FinalFrameUpdate()
        {
        }


        /// <summary>
        /// アプリケーションが一時停止した時の処理を行います
        /// </summary>
        private void OnApplicationSuspend()
        {
        }


        /// <summary>
        /// アプリケーションが再開した時の処理を行います
        /// </summary>
        private void OnApplicationResume()
        {
        }


        /// <summary>
        /// アプリケーションのフォーカスを失った時の処理を行います
        /// </summary>
        private void OnApplicationFocusOut()
        {
        }


        /// <summary>
        /// アプリケーションのフォーカスを得られた時の処理を行います
        /// </summary>
        private void OnApplicationFocusIn()
        {
        }
        #endregion


        #region シーンリスト操作系
        /// <summary>
        /// 指定されたシーンを次に実行するシーンとしてリクエストします。
        /// </summary>
        /// <remarks>
        /// 同一フレーム内で複数のリクエストをすることは可能ですが、
        /// 初期化処理は要求した順に行われ Update が呼び出されるのは、最後に要求したシーンとなります。
        /// また、初期化処理から実際に更新処理が実行されるタイミングは、次のフレームの開始タイミングとなります。
        /// </remarks>
        /// <param name="scene">次に実行するシーン</param>
        /// <exception cref="ArgumentNullException">scene が null です</exception>
        public void RequestNextScene(GameScene scene)
        {
            // scene が null なら
            if (scene == null)
            {
                // そんな追加は許されない
                throw new ArgumentNullException(nameof(scene));
            }


            // 初期化準備完了ステータスとしてシーンを管理リストに追加する
            sceneManagementContextList.Add(new SceneManagementContext()
            {
                // 管理情報の設定
                Scene = scene,
                State = SceneState.Ready,
            });
        }


        /// <summary>
        /// 最後に NextScene 要求のあったシーンまたは、次に破棄可能なシーンの破棄要求をします。
        /// </summary>
        /// <remarks>
        /// 破棄要求のあったシーンが、実際に破棄されるタイミングはフレームの最後のタイミングとなります
        /// </remarks>
        public void RequestDropScene()
        {
            // 管理リストの末尾からループする
            for (int i = sceneManagementContextList.Count - 1; i >= 0; --i)
            {
                // ステータスが Destroy系 なら
                var status = sceneManagementContextList[i].State;
                if (IsDestroy(status))
                {
                    // 次のシーン管理情報へ
                    continue;
                }


                // もし動作中状態なら
                if (IsRunning(status))
                {
                    // 通常の破棄ステータスを設定して終了
                    sceneManagementContextList[i].State = SceneState.Destroy;
                    return;
                }


                // もし準備完了状態なら
                if (IsReady(status))
                {
                    // 準備完了だが破棄されるというステータスを設定して終了
                    sceneManagementContextList[i].State = SceneState.ReadyedButDestroy;
                    return;
                }
            }
        }


        /// <summary>
        /// RequestDropScene() 関数の呼び出し後 RequestNextScene() 関数を呼び出します
        /// </summary>
        /// <param name="scene">新しく切り替えるシーン</param>
        /// <exception cref="ArgumentNullException">scene が null です</exception>
        public void ChangeScene(GameScene scene)
        {
            // 破棄要求関数呼び出し後、シーン実行要求関数を呼ぶだけ
            RequestDropScene();
            RequestNextScene(scene);
        }


        /// <summary>
        /// 全てのシーンを破棄するように要求します
        /// </summary>
        public void RequestDropAllScene()
        {
            // 管理情報の数分回る
            foreach (var sceneManagementContext in sceneManagementContextList)
            {
                // もし動作中状態なら
                if (IsRunning(sceneManagementContext.State))
                {
                    // 通常の破棄ステータスを設定して次へ
                    sceneManagementContext.State = SceneState.Destroy;
                    continue;
                }


                // もし準備完了状態なら
                if (IsReady(sceneManagementContext.State))
                {
                    // 準備完了だが破棄されるというステータスを設定して次へ
                    sceneManagementContext.State = SceneState.ReadyedButDestroy;
                    continue;
                }
            }
        }


        /// <summary>
        /// 最も新しく実行要求のあった、指定のシーン型のインスタンスを検索します
        /// </summary>
        /// <typeparam name="TScene">検索するシーン型</typeparam>
        /// <returns>シーンスタックの最上位に最も近い、シーンインスタンスが見つかった場合は、そのインスタンスを返します。しかし、見つけられなかった場合は null を返します。</returns>
        public GameScene FindNewestScene<TScene>() where TScene : GameScene
        {
            // 未実装
            throw new NotImplementedException();
        }


        /// <summary>
        /// シーンスタックの最下位に最も近い、指定されたシーン型のインスタンスを検索します
        /// </summary>
        /// <typeparam name="TScene">検索するシーン型</typeparam>
        /// <returns>シーンスタックの最下位に最も近い、シーンインスタンスが見つかった場合は、そのインスタンスを返します。しかし、見つけられなかった場合は null を返します。</returns>
        public GameScene FindOldestScene<TScene>() where TScene : GameScene
        {
            // 未実装
            throw new NotImplementedException();
        }


        /// <summary>
        /// シーンスタックに積まれた内の、指定されたシーン型のインスタンスをすべて検索します
        /// </summary>
        /// <typeparam name="TScene">検索するシーン型</typeparam>
        /// <returns>シーンインスタンスを見つけた場合は、見つけたインスタンスの配列を返します。しかし、見つけられなかった場合は、長さ 0 の配列を返します。</returns>
        public GameScene[] FindAllScene<TScene>() where TScene : GameScene
        {
            // 未実装
            throw new NotImplementedException();
        }


        /// <summary>
        /// シーンスタックに積まれた内の、指定されたシーン型のインスタンスをすべて検索します
        /// </summary>
        /// <remarks>
        /// FindAllScene() と この関数の違いは、検索結果を格納するバッファを呼び出し元から受け取る点にあります。
        /// 非常に、高い頻度でシーン全体検索を行う場合において FindAllScene() 関数は内部で結果バッファを生成することにより
        /// ヒープを沢山消費してしまいパフォーマンスに悪影響を与えてしまうため、事前にバッファを用意することで、この問題を回避します。
        /// また、バッファサイズが実際に検索結果の数を下回ったとしても、バッファを超える検索は行わずバッファが満たされた状態で関数は終了します。
        /// </remarks>
        /// <typeparam name="TScene">検索するシーン型</typeparam>
        /// <param name="result">検索結果を格納する GameScene バッファ</param>
        /// <returns>result 引数に与えられたバッファに格納した総数を返します。</returns>
        /// <exception cref="ArgumentNullException">result が null です</exception>
        public int FindAllSceneNonAlloc<TScene>(GameScene[] result) where TScene : GameScene
        {
            // 未実装
            throw new NotImplementedException();
        }


        /// <summary>
        /// 指定されたシーンがシーンスタックに含まれているか確認をします
        /// </summary>
        /// <param name="scene">確認をするシーン</param>
        /// <returns>該当のシーンが、シーンスタックに含まれている場合は true を、含まれていない場合は false を返します</returns>
        public bool ContainsScene(GameScene scene)
        {
            // 未実装
            throw new NotImplementedException();
        }


        /// <summary>
        /// 指定されたシーンのひとつ下に存在するシーンを取得します
        /// </summary>
        /// <param name="scene">取り出すシーンの一つ上に存在するシーン</param>
        /// <returns>指定されたシーンのひとつ下のシーンを返します</returns>
        /// <exception cref="ArgumentNullException">scene が null です</exception>
        /// <exception cref="InvalidOperationException">指定された scene は、シーンスタックに含まれていません</exception>
        /// <exception cref="InvalidOperationException">指定された scene は、シーンスタックの最下位に存在しています</exception>
        public GameScene GetPreviousScene(GameScene scene)
        {
            // 未実装
            throw new NotImplementedException();
        }
        #endregion
    }
}