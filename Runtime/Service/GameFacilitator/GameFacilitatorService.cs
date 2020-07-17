// zlib/libpng License
//
// Copyright (c) 2018 - 2019 Sinoa
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
    #region シーンクラス
    /// <summary>
    /// ゲームのシーンとして制御する基本クラスです
    /// </summary>
    public abstract class GameScene
    {
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
        /// シーンが一時停止する時の処理を行います
        /// </summary>
        protected internal virtual void Sleep()
        {
        }


        /// <summary>
        /// 別のシーンがDropされシーンが再開される時の処理を行います
        /// </summary>
        protected internal virtual void Restart()
        {
        }


        /// <summary>
        /// 別のシーンがDropされこのシーンがシーントップになる時の処理を行います
        /// </summary>
        protected internal virtual void GotTopSceneFocus()
        {
        }
    }
    #endregion



    /// <summary>
    /// ゲーム進行を行うサービスクラスです。
    /// シーンという単位でゲーム進行管理を行い、シーンはまるでスタックのように管理します。
    /// </summary>
    public class GameFacilitatorService<TSceneBase> : GameService where TSceneBase : GameScene
    {
        #region シーン管理情報の型定義
        /// <summary>
        /// リストに存在するシーンのステータスを表します
        /// </summary>
        public enum SceneState
        {
            /// <summary>
            /// シーンの開始準備が完了しました
            /// </summary>
            Ready,

            /// <summary>
            /// シーンは稼働中です
            /// </summary>
            Running,

            /// <summary>
            /// シーンは一時停止します
            /// </summary>
            Sleeping,

            /// <summary>
            /// シーンは一時停止しています
            /// </summary>
            Sleeped,

            /// <summary>
            /// シーンが一時停止から稼動します
            /// </summary>
            Wakeup,

            /// <summary>
            /// シーンはトップシーンの状態を取得しました
            /// </summary>
            GotTopSceneFocus,

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
        /// シーンの管理状態を保持するコンテキストクラスです
        /// </summary>
        private class SceneContext
        {
            /// <summary>
            /// 管理対象になっているシーン本体
            /// </summary>
            public TSceneBase Scene { get; set; }


            /// <summary>
            /// シーンの管理状態
            /// </summary>
            public SceneState State { get; set; }


            /// <summary>
            /// シーンの状態が準備完了であるか
            /// </summary>
            public bool IsReady => State == SceneState.Ready;


            /// <summary>
            /// シーンの状態が稼働中であるか
            /// </summary>
            public bool IsRunning => State == SceneState.Running || State == SceneState.GotTopSceneFocus;


            /// <summary>
            /// シーンの状態が再起動であるか
            /// </summary>
            public bool IsWakeup => State == SceneState.Wakeup;


            /// <summary>
            /// シーンの状態が一時停止であるか
            /// </summary>
            public bool IsSleep => State == SceneState.Sleeped || State == SceneState.Sleeping;


            /// <summary>
            /// シーンの状態が破棄であるか
            /// </summary>
            public bool IsDestroy => State == SceneState.Destroy || State == SceneState.ReadyedButDestroy;
        }
        #endregion



        // メンバ変数定義
        private List<SceneContext> sceneContextList;



        #region プロパティ
        /// <summary>
        /// 管理しているシーンの数を取得します
        /// </summary>
        public int SceneCount => sceneContextList.Count;


        /// <summary>
        /// 動作中のシーンの数を取得します
        /// </summary>
        public int RunningSceneCount
        {
            get
            {
                // 管理しているシーンコンテキストの数分ループ
                int count = 0;
                for (int i = 0; i < sceneContextList.Count; ++i)
                {
                    // 稼働中なら
                    if (sceneContextList[i].IsRunning)
                    {
                        // インクリメント
                        ++count;
                    }
                }


                // カウント結果を返す
                return count;
            }
        }


        /// <summary>
        /// 現在動作中である最新のシーンを取得します。動作中のシーンがない場合は null を取得することがあります。
        /// </summary>
        public TSceneBase RunningTopScene
        {
            get
            {
                // 管理しているシーンコンテキストの数分逆ループ
                for (int i = sceneContextList.Count - 1; i >= 0; --i)
                {
                    // コンテキストの取得
                    var sceneContext = sceneContextList[i];


                    // 稼働中または再起動のシーンなら
                    if (sceneContext.IsRunning || sceneContext.IsWakeup)
                    {
                        // このシーンを返す
                        return sceneContext.Scene;
                    }
                }


                // ループから抜けてきたということは動作中のシーンが見つからなかった
                return null;
            }
        }
        #endregion



        #region コンストラクタと起動停止処理部
        /// <summary>
        /// GameFacilitatorService のインスタンスを初期化します
        /// </summary>
        public GameFacilitatorService()
        {
            // シーン管理リストの初期化
            sceneContextList = new List<SceneContext>();
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
                }
            };
        }


        /// <summary>
        /// サービスの停止処理を行います
        /// </summary>
        protected internal override void Shutdown()
        {
            // 管理情報の数分末尾から回る
            for (int i = sceneContextList.Count - 1; i >= 0; --i)
            {
                // もし準備完了または準備完了破棄対象または一時停止中のステータスなら
                if (sceneContextList[i].IsReady || sceneContextList[i].State == SceneState.ReadyedButDestroy || sceneContextList[i].IsSleep)
                {
                    // 解放処理は実行しないで次へ
                    continue;
                }


                // 準備完了以外なら無条件で解放処理を呼ぶ
                sceneContextList[i].Scene.Terminate();
            }


            // 管理リストを空にする
            sceneContextList.Clear();
        }
        #endregion


        #region サービスの更新
        /// <summary>
        /// サービスの更新を行います
        /// </summary>
        private void UpdateService()
        {
            // 現在のシーンの数分ループ
            var currentSceneCount = SceneCount;
            for (int i = 0; i < currentSceneCount; ++i)
            {
                // シーンコンテキストを取得する
                var sceneContext = sceneContextList[i];


                // これから起きるなら
                if (sceneContext.IsWakeup)
                {
                    // 稼働中状態にして、シーンが起きたイベントを実行して続行
                    sceneContext.State = SceneState.Running;
                    sceneContext.Scene.Restart();
                }


                // シーントップのフォーカスを得た状態なら
                if (sceneContext.State == SceneState.GotTopSceneFocus)
                {
                    // 稼働中状態にして、シーンのトップシーンフォーカスを得たイベントを実行して続行
                    sceneContext.State = SceneState.Running;
                    sceneContext.Scene.GotTopSceneFocus();
                }


                // 稼働中なら
                if (sceneContext.IsRunning)
                {
                    // シーンの更新を読んで次へ
                    sceneContext.Scene.Update();
                    continue;
                }


                // 動作開始準備なら
                if (sceneContext.IsReady)
                {
                    // 初期化処理を呼び出して実行状態に設定した後、次へ
                    sceneContext.State = SceneState.Running;
                    sceneContext.Scene.Initialize();
                    continue;
                }
            }
        }


        /// <summary>
        /// フレーム最後のサービス更新を行います
        /// </summary>
        private void FinalFrameUpdate()
        {
            // 管理情報リストの先頭から回る
            for (int i = 0; i < sceneContextList.Count; ++i)
            {
                // 一時停止開始状態なら
                if (sceneContextList[i].State == SceneState.Sleeping)
                {
                    // 一時停止処理を読んで一時停止状態にする
                    sceneContextList[i].Scene.Sleep();
                    sceneContextList[i].State = SceneState.Sleeped;
                }
            }


            // 管理情報の数分末尾から回る
            var destroyExecuted = false;
            for (int i = sceneContextList.Count - 1; i >= 0; --i)
            {
                // 破棄対象なら
                if (sceneContextList[i].IsDestroy)
                {
                    // 本当の破棄処理なら
                    if (sceneContextList[i].State == SceneState.Destroy)
                    {
                        // 破棄処理を呼ぶ
                        sceneContextList[i].Scene.Terminate();
                    }


                    // 要素を削除して削除処理をしたことを示す
                    sceneContextList.RemoveAt(i);
                    destroyExecuted = true;
                }
            }


            // もし削除処理の経験があって末尾のシーンがもし稼働中なら
            if (destroyExecuted && sceneContextList.Count > 0 && sceneContextList[sceneContextList.Count - 1].IsRunning)
            {
                // フォーカスを得た状態にする
                sceneContextList[sceneContextList.Count - 1].State = SceneState.GotTopSceneFocus;
            }


            // リストの末尾にあるシーンがもし一時停止中なら
            if (sceneContextList.Count > 0 && sceneContextList[sceneContextList.Count - 1].IsSleep)
            {
                // 再起動状態にする
                sceneContextList[sceneContextList.Count - 1].State = SceneState.Wakeup;
            }
        }
        #endregion


        #region シーンリスト操作系
        /// <summary>
        /// RequestDropScene() 関数の呼び出し後 RequestNextScene() 関数を呼び出します
        /// </summary>
        /// <param name="scene">新しく切り替えるシーン</param>
        /// <exception cref="ArgumentNullException">scene が null です</exception>
        public void ChangeScene(TSceneBase scene)
        {
            ThrowIfArgumentNullException(scene, nameof(scene));


            // 破棄要求関数呼び出し後、シーン実行要求関数を呼ぶだけ
            RequestDropScene();
            RequestNextScene(scene);
        }


        /// <summary>
        /// 指定されたシーンを次に実行するシーンとしてリクエストします。
        /// また、既定動作としてはトップシーンが一時停止するようになっています。
        /// </summary>
        /// <remarks>
        /// 同一フレーム内で複数のリクエストをすることが可能であり、初期化処理は要求した順に行われます。
        /// また、初期化処理から実際に更新処理が実行されるタイミングは、次のフレームの開始タイミングとなります。
        /// </remarks>
        /// <param name="scene">次に実行するシーン</param>
        /// <exception cref="ArgumentNullException">scene が null です</exception>
        public void RequestNextScene(TSceneBase scene)
        {
            // 自身のシーンが一時停止するRequestNextScene関数呼び出しをする
            RequestNextScene(scene, true);
        }


        /// <summary>
        /// 指定されたシーンを次に実行するシーンとしてリクエストします。
        /// </summary>
        /// <remarks>
        /// 同一フレーム内で複数のリクエストをすることが可能であり、初期化処理は要求した順に行われます。
        /// また、初期化処理から実際に更新処理が実行されるタイミングは、次のフレームの開始タイミングとなります。
        /// </remarks>
        /// <param name="scene">次に実行するシーン</param>
        /// <param name="sleepTopRunningScene">トップシーンを一時停止する場合は true を、継続動作する場合は false を指定</param>
        /// <exception cref="ArgumentNullException">scene が null です</exception>
        public void RequestNextScene(TSceneBase scene, bool sleepTopRunningScene)
        {
            ThrowIfArgumentNullException(scene, nameof(scene));


            // もしトップシーンを寝かす指示が出ていて、かつシーンの数が空でないなら
            if (sleepTopRunningScene && SceneCount > 0)
            {
                // 末尾のシーンコンテキストを取得して稼働中なら
                var sceneContext = sceneContextList[SceneCount - 1];
                if (sceneContext.IsRunning)
                {
                    // 寝かせる
                    RequestSleepScene(sceneContext.Scene);
                }
            }


            // 初期化準備完了ステータスとしてシーンを管理リストに追加する
            sceneContextList.Add(new SceneContext()
            {
                // 管理情報の設定
                Scene = scene,
                State = SceneState.Ready,
            });
        }


        /// <summary>
        /// 指定されたシーンを一時停止要求をします。
        /// ただし、指定されたシーンは既に動作経験がある必要があります。
        /// さらに、破棄対象となったシーンは一時停止することは出来ません。
        /// </summary>
        /// <remarks>
        /// 任意のタイミングでシーンを一時停止する事が出来ますが、全てのシーンが一時停止してしまい
        /// ゲームの進行が停止してしまわないように注意をして下さい。
        /// また、この関数はシーンの管理対象でありかつ動作中である必要があります。
        /// </remarks>
        /// <param name="scene">一時停止要求するシーン</param>
        /// <exception cref="ArgumentNullException">scene が null です</exception>
        public void RequestSleepScene(TSceneBase scene)
        {
            // シーンの数分ループする
            foreach (var sceneContext in sceneContextList)
            {
                // もしシーンの参照が一致しないなら
                if (sceneContext.Scene != scene)
                {
                    // 次へ
                    continue;
                }


                // 動作中以外なら
                if (!sceneContext.IsRunning)
                {
                    // 何もせずループを終了
                    break;
                }


                // 一時停止することを示す
                sceneContext.State = SceneState.Sleeping;
            }
        }


        /// <summary>
        /// 指定されたシーンの再起動を要求をします。
        /// ただし、指定されたシーンは既に一時停止中である必要があります。
        /// </summary>
        /// <param name="scene">再起動要求するシーン</param>
        public void RequestWakeUpScene(TSceneBase scene)
        {
            // シーンの数分ループする
            foreach (var sceneContext in sceneContextList)
            {
                // もしシーンの参照が一致しないなら
                if (sceneContext.Scene != scene)
                {
                    // 次へ
                    continue;
                }

                // 一時停止中以外なら
                if (!sceneContext.IsSleep)
                {
                    // 何もせずループを終了
                    break;
                }

                // 再起動状態にする
                sceneContext.State = SceneState.Wakeup;
            }
        }


        /// <summary>
        /// 指定されたシーンの破棄要求をします。
        /// </summary>
        /// <remarks>
        /// 破棄要求のあったシーンが、実際に破棄されるタイミングはフレームの最後のタイミングとなります
        /// </remarks>
        /// <param name="scene">破棄要求するシーン</param>
        public void RequestDropScene(TSceneBase scene)
        {
            // シーンの数分ループする
            foreach (var sceneContext in sceneContextList)
            {
                // もしシーンの参照が一致しないなら
                if (sceneContext.Scene != scene)
                {
                    // 次へ
                    continue;
                }


                // ステータスが Destroy系 なら
                if (sceneContext.IsDestroy)
                {
                    // 何もせずループを終了
                    break;
                }


                // もし動作中状態または一時停止なら
                if (sceneContext.IsRunning || sceneContext.IsSleep || sceneContext.IsWakeup)
                {
                    // 通常の破棄ステータスを設定して終了
                    sceneContext.State = SceneState.Destroy;
                    break;
                }


                // もし準備完了状態なら
                if (sceneContext.IsReady)
                {
                    // 準備完了だが破棄されるというステータスを設定して終了
                    sceneContext.State = SceneState.ReadyedButDestroy;
                    break;
                }
            }
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
            for (int i = sceneContextList.Count - 1; i >= 0; --i)
            {
                // シーンコンテキストを取得する
                var sceneContext = sceneContextList[i];


                // ステータスが Destroy系 なら
                if (sceneContext.IsDestroy)
                {
                    // 次のシーン管理情報へ
                    continue;
                }


                // もし動作中状態または一時停止なら
                if (sceneContext.IsRunning || sceneContext.IsSleep || sceneContext.IsWakeup)
                {
                    // 通常の破棄ステータスを設定して終了
                    sceneContext.State = SceneState.Destroy;
                    return;
                }


                // もし準備完了状態なら
                if (sceneContext.IsReady)
                {
                    // 準備完了だが破棄されるというステータスを設定して終了
                    sceneContext.State = SceneState.ReadyedButDestroy;
                    return;
                }
            }
        }


        /// <summary>
        /// 全てのシーンを破棄するように要求します
        /// </summary>
        public void RequestDropAllScene()
        {
            // 管理情報の数分回る
            foreach (var sceneContext in sceneContextList)
            {
                // もし動作中、一時停止中、再起動状態なら
                if (sceneContext.IsRunning || sceneContext.IsSleep || sceneContext.IsWakeup)
                {
                    // 通常の破棄ステータスを設定して次へ
                    sceneContext.State = SceneState.Destroy;
                    continue;
                }


                // もし準備完了状態なら
                if (sceneContext.IsReady)
                {
                    // 準備完了だが破棄されるというステータスを設定して次へ
                    sceneContext.State = SceneState.ReadyedButDestroy;
                    continue;
                }
            }
        }


        /// <summary>
        /// 指定されたシーンのひとつ前に存在する、動作可能な状態のシーンを取得します
        /// </summary>
        /// <remarks>
        /// この関数は、一つ前のシーンが動作可能状態で有ることを保証したシーンを取得します
        /// </remarks>
        /// <param name="scene">取り出すシーンの一つ次に存在するシーン。このシーンは破棄対象になったシーンでも構いません</param>
        /// <returns>指定されたシーンのひとつ前の、動作可能な状態のシーンを返しますが、シーンを見つけられなかった場合は null を返します</returns>
        /// <exception cref="ArgumentNullException">scene が null です</exception>
        /// <exception cref="InvalidOperationException">指定された scene は、管理対象になっていません</exception>
        public TSceneBase GetPreviousScene(TSceneBase scene)
        {
            ThrowIfArgumentNullException(scene, nameof(scene));


            // 管理情報の数分末尾から回る
            for (int i = sceneContextList.Count - 1; i >= 0; --i)
            {
                // もしシーンが一致するインデックスを見つけたのなら
                if (sceneContextList[i].Scene == scene)
                {
                    // さらにここから動作可能なシーンを割り出す
                    for (i -= 1; i >= 0; --i)
                    {
                        // シーンの状態がまだ生きているなら
                        if (!sceneContextList[i].IsDestroy)
                        {
                            // このシーンを返す
                            return sceneContextList[i].Scene;
                        }
                    }


                    // ループから抜けてきてしまったのなら、一つ前にシーンが存在していないのでnullを返す
                    return null;
                }
            }


            // そもそも外枠のループから抜けてきたということは、管理対象ですらない事を吐く
            throw new InvalidOperationException($"'{scene.GetType().Name}'は、管理対象のシーンではありません");
        }


        /// <summary>
        /// 現在のシーン管理状態の各要素を指定の処理で実行します
        /// </summary>
        /// <param name="action">シーン管理状態の要素に対して実行する関数</param>
        public void SceneContextForEach(Action<TSceneBase, SceneState> action)
        {
            foreach (var sceneContext in sceneContextList)
            {
                action(sceneContext.Scene, sceneContext.State);
            }
        }


        /// <summary>
        /// 現在の管理されているシーンのすべてを取得します
        /// </summary>
        /// <returns>現在の管理下にあるシーンの全てを保持した配列を返します</returns>
        public TSceneBase[] GetSceneAll()
        {
            var sceneArray = new TSceneBase[sceneContextList.Count];
            for (int i = 0; i < sceneArray.Length; ++i)
            {
                sceneArray[i] = sceneContextList[i].Scene;
            }


            return sceneArray;
        }


        /// <summary>
        /// 指定されたシーン状態条件判定関数に一致するシーンを取得し、指定された結果配列に設定します。
        /// </summary>
        /// <param name="results">指定された条件に一致するシーンの結果を設定する配列、一致したシーンの数が配列の長さを超えても配列の長さまでしか格納しません</param>
        /// <param name="match">シーン状態を判定する関数</param>
        /// <returns>指定されたシーン状態に一致したシーンを results に設定した数を返します。もし results より大きい条件一致があっても超えることはありません</returns>
        /// <exception cref="ArgumentNullException">results が null です</exception>
        /// <exception cref="ArgumentNullException">match が null です</exception>
        public int GetSceneList(TSceneBase[] results, Func<SceneState, bool> match)
        {
            ThrowIfArgumentNullException(results, nameof(results));
            ThrowIfArgumentNullException(match, nameof(match));


            // もし結果格納バッファの長さが0なら
            if (results.Length == 0)
            {
                // そもそも回らず直ちに0を返す
                return 0;
            }


            // 管理情報の数分回る
            var resultCount = 0;
            for (int i = 0; i < sceneContextList.Count; ++i)
            {
                // もし指定された条件を満たすシーンなら
                var sceneContext = sceneContextList[i];
                if (match(sceneContext.State))
                {
                    // 結果配列にシーンを追加してバッファの長さいっぱいになったら
                    results[resultCount++] = sceneContext.Scene;
                    if (results.Length == resultCount)
                    {
                        // このまま結果の数を返す
                        return resultCount;
                    }
                }
            }


            // 回りきったら現在の結果の数を返す
            return resultCount;
        }


        /// <summary>
        /// 実行中とされるシーン状態を取得し、指定された結果配列に設定します
        /// </summary>
        /// <param name="results">実行中シーンの結果を設定する配列、シーンの取得数が配列の長さを超えても配列の長さまでしか格納しません</param>
        /// <returns>取得された実行中シーンを results に設定した数を返します。もし results より大きい条件一致があっても超えることはありません</returns>
        /// <exception cref="ArgumentNullException">results が null です</exception>
        public int GetRunningSceneList(TSceneBase[] results)
        {
            ThrowIfArgumentNullException(results, nameof(results));


            if (results.Length == 0)
            {
                return 0;
            }


            // 管理情報の数分回る
            var resultCount = 0;
            for (int i = 0; i < sceneContextList.Count; ++i)
            {
                // もし指定された条件を満たすシーンなら
                var sceneContext = sceneContextList[i];
                if (sceneContext.IsRunning)
                {
                    // 結果配列にシーンを追加してバッファの長さいっぱいになったら
                    results[resultCount++] = sceneContext.Scene;
                    if (results.Length == resultCount)
                    {
                        // このまま結果の数を返す
                        return resultCount;
                    }
                }
            }


            // 回りきったら現在の結果の数を返す
            return resultCount;
        }
        #endregion


        #region 例外判定関数系
        private void ThrowIfArgumentNullException(object argument, string name)
        {
            if (argument == null)
            {
                throw new ArgumentNullException(name);
            }
        }
        #endregion
    }
}