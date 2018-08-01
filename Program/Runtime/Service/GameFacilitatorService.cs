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
    public class GameFacilitatorService<TSceneBase> : GameService where TSceneBase : GameScene
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
            public TSceneBase Scene { get; set; }


            /// <summary>
            /// シーンの管理状態
            /// </summary>
            public SceneState State { get; set; }
        }
        #endregion



        // メンバ変数定義
        private List<SceneManagementContext> sceneManagementContextList;



        /// <summary>
        /// 現在実行中のシーンを取得します
        /// </summary>
        public TSceneBase CurrentScene { get; private set; }



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
            // 管理情報の数分末尾から回る
            for (int i = sceneManagementContextList.Count - 1; i >= 0; --i)
            {
                // もし準備完了または準備完了破棄対象のステータスなら
                if (IsReady(sceneManagementContextList[i].State) || sceneManagementContextList[i].State == SceneState.ReadyedButDestroy)
                {
                    // 解放処理は実行しないで次へ
                    continue;
                }


                // 準備完了以外なら無条件で解放処理を呼ぶ
                sceneManagementContextList[i].Scene.Terminate();
            }


            // 管理リストを空にする
            sceneManagementContextList.Clear();
        }
        #endregion


        #region サービスの更新
        /// <summary>
        /// サービスの更新を行います
        /// </summary>
        private void UpdateService()
        {
            // Updateを呼ぶべきシーンを初期化する
            var needUpdateScene = default(TSceneBase);


            // 管理情報の数分回る（全体を巡回しながら初期化を呼ぶべき子も一緒に探す）
            foreach (var sceneManagementContext in sceneManagementContextList)
            {
                // もし動作開始準備なシーンなら
                if (IsReady(sceneManagementContext.State))
                {
                    // 初期化処理を呼び出して実行状態にする
                    sceneManagementContext.Scene.Initialize();
                    sceneManagementContext.State = SceneState.Running;


                    // そしてUpdateを呼ぶべきシーンとして覚えて次へ
                    needUpdateScene = sceneManagementContext.Scene;
                    continue;
                }


                // もし実行状態なシーンなら
                if (IsRunning(sceneManagementContext.State))
                {
                    // Updateを呼ぶべきシーンとして覚えて次へ
                    needUpdateScene = sceneManagementContext.Scene;
                    continue;
                }
            }


            // 巡回更新が終わったら現在処理するべきシーンの更新をする
            CurrentScene = needUpdateScene;


            // Updateを呼ぶべきシーンが存在するなら
            if (CurrentScene != null)
            {
                // Updateを呼ぶ
                CurrentScene.Update();
            }
        }


        /// <summary>
        /// フレーム最後のサービス更新を行います
        /// </summary>
        private void FinalFrameUpdate()
        {
            // 管理情報の数分末尾から回る
            for (int i = sceneManagementContextList.Count - 1; i >= 0; --i)
            {
                // 破棄対象なら
                if (IsDestroy(sceneManagementContextList[i].State))
                {
                    // 本当の破棄処理なら
                    if (sceneManagementContextList[i].State == SceneState.Destroy)
                    {
                        // 破棄処理を呼ぶ
                        sceneManagementContextList[i].Scene.Terminate();
                    }


                    // 要素を削除する
                    sceneManagementContextList.RemoveAt(i);
                }
            }
        }


        /// <summary>
        /// アプリケーションが一時停止した時の処理を行います
        /// </summary>
        private void OnApplicationSuspend()
        {
            // 現在のシーンが存在するなら
            if (CurrentScene != null)
            {
                // アプリケーションが一時停止したイベントを呼ぶ
                CurrentScene.OnApplicationSleep();
            }
        }


        /// <summary>
        /// アプリケーションが再開した時の処理を行います
        /// </summary>
        private void OnApplicationResume()
        {
            // 現在のシーンが存在するなら
            if (CurrentScene != null)
            {
                // アプリケーションが再開したイベントを呼ぶ
                CurrentScene.OnApplicationResume();
            }
        }


        /// <summary>
        /// アプリケーションのフォーカスを失った時の処理を行います
        /// </summary>
        private void OnApplicationFocusOut()
        {
            // 現在のシーンが存在するなら
            if (CurrentScene != null)
            {
                // アプリケーションがフォーカスを失ったイベントを呼ぶ
                CurrentScene.OnApplicationFocusOut();
            }
        }


        /// <summary>
        /// アプリケーションのフォーカスを得られた時の処理を行います
        /// </summary>
        private void OnApplicationFocusIn()
        {
            // 現在のシーンが存在するなら
            if (CurrentScene != null)
            {
                // アプリケーションがフォーカスを得られたイベントを呼ぶ
                CurrentScene.OnApplicationFocusIn();
            }
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
        public void RequestNextScene(TSceneBase scene)
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
        public void ChangeScene(TSceneBase scene)
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
        /// 最も新しい実行状態または実行準備状態のある、指定のシーン型のインスタンスを検索します
        /// </summary>
        /// <typeparam name="TScene">検索するシーン型</typeparam>
        /// <returns>最も新しい実行状態または実行準備状態の指定シーンが見つかった場合は、そのインスタンスを返します。見つからなかった場合は null を返します</returns>
        public TScene FindNewestScene<TScene>() where TScene : TSceneBase
        {
            // 検索する型の取得
            var sceneType = typeof(TScene);


            // 管理情報の数分末尾から回る
            for (int i = sceneManagementContextList.Count - 1; i >= 0; --i)
            {
                // シーンの状態が Ready か Running 以外なら
                var state = sceneManagementContextList[i].State;
                if (!(IsReady(state) || IsRunning(state)))
                {
                    // 状態が稼働状態でないなら、型が一致しても収拾する気はない
                    continue;
                }


                // シーンの型が一致したのなら
                var scene = sceneManagementContextList[i].Scene;
                if (scene.GetType() == sceneType)
                {
                    // このシーンを返す
                    return scene as TScene;
                }
            }


            // ループから抜けてきたということは見つからなかったとして null を返す
            return null;
        }


        /// <summary>
        /// 最も古い実行状態または実行準備状態のある、指定のシーン型のインスタンスを検索します
        /// </summary>
        /// <typeparam name="TScene">検索するシーン型</typeparam>
        /// <returns>最も古い実行状態または実行準備状態の指定シーンが見つかった場合は、そのインスタンスを返します。見つからなかった場合は null を返します</returns>
        public TScene FindOldestScene<TScene>() where TScene : TSceneBase
        {
            // 検索する型の取得
            var sceneType = typeof(TScene);


            // 管理情報の数分先頭から回る
            foreach (var sceneManagementContext in sceneManagementContextList)
            {
                // シーンの状態が Ready か Running 以外なら
                var state = sceneManagementContext.State;
                if (!(IsReady(state) || IsRunning(state)))
                {
                    // 状態が稼働状態でないなら、型が一致しても収拾する気はない
                    continue;
                }


                // シーンの型が一致したのなら
                var scene = sceneManagementContext.Scene;
                if (scene.GetType() == sceneType)
                {
                    // このシーンを返す
                    return scene as TScene;
                }
            }


            // ループから抜けてきたということは見つからなかったとして null を返す
            return null;
        }


        /// <summary>
        /// このサービスが保持している実行状態または実行準備のある、指定されたシーン型のインスタンスをすべて検索します
        /// </summary>
        /// <remarks>
        /// この関数が返す配列の順序は、新しく実行要求のあった順に整列しています。
        /// </remarks>
        /// <typeparam name="TScene">検索するシーン型</typeparam>
        /// <returns>シーンインスタンスを見つけた場合は、見つけたインスタンスの配列を返します。しかし、見つけられなかった場合は長さ 0 の配列を返します。</returns>
        public TScene[] FindAllScene<TScene>() where TScene : TSceneBase
        {
            // 検索する型の取得と検索結果を一時的に蓄えるリスト
            var sceneType = typeof(TScene);
            var resultList = new List<TScene>(sceneManagementContextList.Count);


            // 管理情報の数分末尾から回る
            for (int i = sceneManagementContextList.Count - 1; i >= 0; --i)
            {
                // もしシーンの状態が Ready か Running 以外なら
                var state = sceneManagementContextList[i].State;
                if (!(IsReady(state) || IsRunning(state)))
                {
                    // 状態が稼働状態でないなら、型が一致しても収拾する気はない
                    continue;
                }


                // シーンの型が一致したのなら
                var scene = sceneManagementContextList[i].Scene;
                if (scene.GetType() == sceneType)
                {
                    // このシーンを返す候補に詰める
                    resultList.Add(scene as TScene);
                }
            }


            // 配列として返す
            return resultList.ToArray();
        }


        /// <summary>
        /// このサービスが保持している実行状態または実行準備のある、指定されたシーン型のインスタンスをすべて検索します
        /// </summary>
        /// <remarks>
        /// この関数は、殆ど FindAllScene() と、同じ挙動ですが、違いとしては検索結果を格納するバッファを呼び出し元から受け取る点にあります。
        /// 非常に、高い頻度でシーン全体検索を行う場合において FindAllScene() 関数は内部で結果バッファを生成することにより
        /// ヒープを沢山消費してしまいパフォーマンスに悪影響を与えてしまうため、事前にバッファを用意することで、この問題を回避します。
        /// また、バッファサイズが実際に検索結果の数を下回ったとしても、バッファを超える検索は行わずバッファが満たされた状態で関数は終了します。
        /// </remarks>
        /// <typeparam name="TScene">検索するシーン型</typeparam>
        /// <param name="result">検索結果を格納する GameScene バッファ</param>
        /// <returns>result 引数に与えられたバッファに格納した総数を返します。</returns>
        /// <exception cref="ArgumentNullException">result が null です</exception>
        public int FindAllSceneNonAlloc<TScene>(TScene[] result) where TScene : TSceneBase
        {
            // nullを渡されてしまったら
            if (result == null)
            {
                // そんな要求は受け付けられない！
                throw new ArgumentNullException(nameof(result));
            }


            // バッファサイズが0なら
            if (result.Length == 0)
            {
                // 直ちに終了
                return 0;
            }


            // 検索する型の取得
            var sceneType = typeof(TScene);


            // 管理情報の数分末尾から回る
            var insertIndex = 0;
            for (int i = sceneManagementContextList.Count - 1; i >= 0; --i)
            {
                // もし渡されたバッファを満たしている状態なら
                if (result.Length == insertIndex)
                {
                    // ループから脱出して検索を終了する
                    break;
                }


                // もしシーンの状態が Ready か Running 以外なら
                var state = sceneManagementContextList[i].State;
                if (!(IsReady(state) || IsRunning(state)))
                {
                    // 状態が稼働状態でないなら、型が一致しても収拾する気はない
                    continue;
                }


                // シーンの型が一致したのなら
                var scene = sceneManagementContextList[i].Scene;
                if (scene.GetType() == sceneType)
                {
                    // バッファに情報を入れて挿入インデックス位置をずらす
                    result[insertIndex] = scene as TScene;
                    ++insertIndex;
                }
            }


            // 取得した件数を返す（挿入インデックスの位置がちょうど検索件数と一致する）
            return insertIndex;
        }


        /// <summary>
        /// 指定されたシーンが、このサービスによって管理され含まれているか確認をします。
        /// また、含まれているかどうかについては、シーンの状態が 実行状態 か 実行準備完了状態 の場合に限ります。
        /// </summary>
        /// <param name="scene">確認をするシーン</param>
        /// <returns>該当のシーンが、サービスに含まれている場合は true を、含まれていない場合は false を返します</returns>
        /// <exception cref="ArgumentNullException">scene が null です</exception>
        public bool ContainsScene(TSceneBase scene)
        {
            // nullを渡されてしまったら
            if (scene == null)
            {
                // そのような確認は許されない！
                throw new ArgumentNullException(nameof(scene));
            }


            // 管理情報の数分回る
            foreach (var sceneManagementContext in sceneManagementContextList)
            {
                // もし参照が一致するシーンの管理情報が見つかったのなら
                if (sceneManagementContext.Scene == scene)
                {
                    // 直ちに見つかったことを返したいが、破棄対象になっているのなら
                    if (IsDestroy(sceneManagementContext.State))
                    {
                        // 見つかったとしてもこれは破棄される予定なので次を探す
                        continue;
                    }


                    // 破棄対象でも無いのなら存在していることを返す
                    return true;
                }
            }


            // ループから抜けてきてしまったのなら存在していないことになる
            return false;
        }


        /// <summary>
        /// 指定されたシーンのひとつ前に存在する、動作可能な状態のシーンを取得します
        /// </summary>
        /// <remarks>
        /// この関数は、一つ前のシーンが動作可能状態で有ることを保証したシーンを取得します
        /// </remarks>
        /// <param name="scene">取り出すシーンの一つ次に存在するシーン。このシーンは破棄対象になったシーンでも構いません</param>
        /// <returns>指定されたシーンのひとつ前の、動作可能な状態のシーンを返します</returns>
        /// <exception cref="ArgumentNullException">scene が null です</exception>
        /// <exception cref="InvalidOperationException">指定された scene は、管理対象になっていません</exception>
        /// <exception cref="InvalidOperationException">指定された scene より前に動作可能なシーンが存在しません</exception>
        public TSceneBase GetPreviousScene(TSceneBase scene)
        {
            // nullを渡されてしまったら
            if (scene == null)
            {
                // そのような確認は許されない！
                throw new ArgumentNullException(nameof(scene));
            }


            // 管理情報の数分末尾から回る
            for (int i = sceneManagementContextList.Count - 1; i >= 0; --i)
            {
                // もしシーンが一致するインデックスを見つけたのなら
                if (sceneManagementContextList[i].Scene == scene)
                {
                    // ただしインデックス0なら
                    if (i == 0)
                    {
                        // 一つ前にシーンが存在していない事を吐く
                        throw new InvalidOperationException($"'{scene.GetType().Name}'より前に動作可能なシーンが存在しません");
                    }


                    // さらにここから動作可能なシーンを割り出す
                    for (i = i - 1; i >= 0; --i)
                    {
                        // もしシーンの状態が Ready か Running なら
                        var state = sceneManagementContextList[i].State;
                        if (IsReady(state) || IsRunning(state))
                        {
                            // このシーンを返す
                            return sceneManagementContextList[i].Scene;
                        }
                    }


                    // ループから抜けてきてしまったのなら、一つ前にシーンが存在していない事を吐く
                    throw new InvalidOperationException($"'{scene.GetType().Name}'より前に動作可能なシーンが存在しません");
                }
            }


            // そもそも外枠のループから抜けてきたということは、管理対象ですらない事を吐く
            throw new InvalidOperationException($"'{scene.GetType().Name}'は、管理対象のシーンではありません");
        }
        #endregion
    }
}