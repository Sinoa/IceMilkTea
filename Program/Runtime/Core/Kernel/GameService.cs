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
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;

namespace IceMilkTea.Core
{
    /// <summary>
    /// PlayerLoopSystemに登録する際に必要になる型情報を定義したGameService用構造体です
    /// </summary>
    public struct GameServiceUpdate
    {
        /// <summary>
        /// MainLoopHead 用型定義
        /// </summary>
        public struct GameServiceMainLoopHead { }

        /// <summary>
        /// PreFixedUpdate 用型定義
        /// </summary>
        public struct GameServicePreFixedUpdate { }

        /// <summary>
        /// PostFixedUpdate 用型定義
        /// </summary>
        public struct GameServicePostFixedUpdate { }

        /// <summary>
        /// PostPhysicsSimulation 用型定義
        /// </summary>
        public struct GameServicePostPhysicsSimulation { }

        /// <summary>
        /// PostWaitForFixedUpdate 用型定義
        /// </summary>
        public struct GameServicePostWaitForFixedUpdate { }

        /// <summary>
        /// PreProcessSynchronizationContext 用型定義
        /// </summary>
        public struct GameServicePreProcessSynchronizationContext { }

        /// <summary>
        /// PostProcessSynchronizationContext 用型定義
        /// </summary>
        public struct GameServicePostProcessSynchronizationContext { }

        /// <summary>
        /// PreUpdate 用型定義
        /// </summary>
        public struct GameServicePreUpdate { }

        /// <summary>
        /// PostUpdate 用型定義
        /// </summary>
        public struct GameServicePostUpdate { }

        /// <summary>
        /// PreAnimation 用型定義
        /// </summary>
        public struct GameServicePreAnimation { }

        /// <summary>
        /// PostAnimation 用型定義
        /// </summary>
        public struct GameServicePostAnimation { }

        /// <summary>
        /// PreLateUpdate 用型定義
        /// </summary>
        public struct GameServicePreLateUpdate { }

        /// <summary>
        /// PostLateUpdate 用型定義
        /// </summary>
        public struct GameServicePostLateUpdate { }

        /// <summary>
        /// PreDrawPresent 用型定義
        /// </summary>
        public struct GameServicePreDrawPresent { }

        /// <summary>
        /// PostDrawPresent 用型定義
        /// </summary>
        public struct GameServicePostDrawPresent { }

        /// <summary>
        /// MainLoopTail 用型定義
        /// </summary>
        public struct GameServiceMainLoopTail { }
    }



    /// <summary>
    /// サービスが動作するための更新タイミングを表します
    /// </summary>
    [Flags]
    public enum GameServiceUpdateTiming : UInt32
    {
        /// <summary>
        /// メインループ最初のタイミング。
        /// ただし、Time.frameCountや入力情報の更新直後となります。
        /// </summary>
        MainLoopHead = (1 << 0),

        /// <summary>
        /// MonoBehaviour.FixedUpdate直前のタイミング
        /// </summary>
        PreFixedUpdate = (1 << 1),

        /// <summary>
        /// MonoBehaviour.FixedUpdate直後のタイミング
        /// </summary>
        PostFixedUpdate = (1 << 2),

        /// <summary>
        /// 物理シミュレーション直後のタイミング。
        /// ただし、シミュレーションによる物理イベントキューが全て処理された直後となります。
        /// </summary>
        PostPhysicsSimulation = (1 << 3),

        /// <summary>
        /// WaitForFixedUpdate直後のタイミング。
        /// </summary>
        PostWaitForFixedUpdate = (1 << 4),

        /// <summary>
        /// UnitySynchronizationContextにPostされた関数キューが処理される直前のタイミング
        /// </summary>
        PreProcessSynchronizationContext = (1 << 5),

        /// <summary>
        /// UnitySynchronizationContextにPostされた関数キューが処理された直後のタイミング
        /// </summary>
        PostProcessSynchronizationContext = (1 << 6),

        /// <summary>
        /// MonoBehaviour.Update直前のタイミング
        /// </summary>
        PreUpdate = (1 << 7),

        /// <summary>
        /// MonoBehaviour.Update直後のタイミング
        /// </summary>
        PostUpdate = (1 << 8),

        /// <summary>
        /// UnityのAnimator(UpdateMode=Normal)によるポージング処理される直前のタイミング
        /// </summary>
        PreAnimation = (1 << 9),

        /// <summary>
        /// UnityのAnimator(UpdateMode=Normal)によるポージング処理された直後のタイミング
        /// </summary>
        PostAnimation = (1 << 10),

        /// <summary>
        /// MonoBehaviour.LateUpdate直前のタイミング
        /// </summary>
        PreLateUpdate = (1 << 11),

        /// <summary>
        /// MonoBehaviour.LateUpdate直後のタイミング
        /// </summary>
        PostLateUpdate = (1 << 12),

        /// <summary>
        /// メインスレッドにおける描画デバイスのPresentする直前のタイミング
        /// </summary>
        PreDrawPresent = (1 << 13),

        /// <summary>
        /// メインスレッドにおける描画デバイスのPresentされた直後のタイミング
        /// </summary>
        PostDrawPresent = (1 << 14),

        /// <summary>
        /// メインループの最後のタイミング。
        /// </summary>
        MainLoopTail = (1 << 15),

        /// <summary>
        /// Unityプレイヤーのフォーカスが得られたときのタイミング。
        /// OnApplicationFocus(true)。
        /// </summary>
        OnApplicationFocusIn = (1 << 16),

        /// <summary>
        /// Unityプレイヤーのフォーカスが失われたときのタイミング。
        /// OnApplicationFocus(false)。
        /// </summary>
        OnApplicationFocusOut = (1 << 17),

        /// <summary>
        /// Unityプレイヤーのメインループが一時停止したときのタイミング。
        /// OnApplicationPause(true)。
        /// </summary>
        OnApplicationSuspend = (1 << 18),

        /// <summary>
        /// Unityプレイヤーのメインループが再開したときのタイミング。
        /// OnApplicationPause(false)。
        /// </summary>
        OnApplicationResume = (1 << 19),

        /// <summary>
        /// あらゆるカメラのカリングが行われる直前のタイミング。
        /// ただし、カメラが存在する数分１フレームで複数回呼び出される可能性があります。
        /// さらに、スレッドはメインスレッド上におけるタイミングとなります。
        /// </summary>
        CameraPreCulling = (1 << 20),

        /// <summary>
        /// あらゆるカメラのレンダリングが行われる直前のタイミング。
        /// ただし、カメラが存在する数分１フレームで複数回呼び出される可能性があります。
        /// さらに、スレッドはメインスレッド上におけるタイミングとなります。
        /// </summary>
        CameraPreRendering = (1 << 21),

        /// <summary>
        /// あらゆるカメラのレンダリングが行われた直後のタイミング。
        /// ただし、カメラが存在する数分１フレームで複数回呼び出される可能性があります。
        /// さらに、スレッドはメインスレッド上におけるタイミングとなります。
        /// </summary>
        CameraPostRendering = (1 << 22),
    }



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
    /// ゲームサービスが動作を開始するための情報を保持する構造体です
    /// </summary>
    public struct GameServiceStartupInfo
    {
        /// <summary>
        /// サービスが更新処理として必要としている更新関数テーブル
        /// </summary>
        public Dictionary<GameServiceUpdateTiming, Action> UpdateFunctionTable { get; set; }
    }



    /// <summary>
    /// ゲームのサブシステムをサービスとして提供するための基本クラスです。
    /// ゲームのサブシステムを実装する場合は、このクラスを継承し適切な振る舞いを実装してください。
    /// </summary>
    public abstract class GameService
    {
        /// <summary>
        /// サービスを起動します。
        /// </summary>
        /// <param name="info">サービスが起動する時に必要とする情報を設定します</param>
        protected internal virtual void Startup(out GameServiceStartupInfo info)
        {
            // 特に何もしない起動情報を設定して修了
            info = new GameServiceStartupInfo()
            {
                UpdateFunctionTable = null,
            };
        }


        /// <summary>
        /// ゲームアプリケーションが終了することを許可するかどうかを判断します。
        /// </summary>
        /// <returns>アプリケーションが終了することを許可する場合は GameShutdownAnswer.Approve を、許可しない場合は GameShutdownAnswer.Reject を返します</returns>
        protected internal virtual GameShutdownAnswer JudgeGameShutdown()
        {
            // 通常は許可をする
            return GameShutdownAnswer.Approve;
        }


        /// <summary>
        /// サービスをシャットダウンします
        /// </summary>
        protected internal virtual void Shutdown()
        {
        }
    }



    /// <summary>
    /// IceMilkTeaのゲームサービスを管理及び制御を行うクラスです。
    /// </summary>
    public class GameServiceManager
    {
        /// <summary>
        /// サービスの状態を表します
        /// </summary>
        protected enum ServiceStatus
        {
            /// <summary>
            /// サービスが生成され、動作を開始する準備が出来ました。
            /// </summary>
            Ready,

            /// <summary>
            /// サービスが生成され、動作を開始する準備が出来ていますが、休止中です。
            /// </summary>
            ReadyButSleeping,

            /// <summary>
            /// サービスは動作中です。
            /// </summary>
            Running,

            /// <summary>
            /// サービスは休止中です。
            /// </summary>
            Sleeping,

            /// <summary>
            /// サービスは破棄対象としてマークされ、シャットダウン状態になりました。
            /// </summary>
            Shutdown,

            /// <summary>
            /// サービスは破棄対象としてマークされましたが、シャットダウン処理は実行されずそのまま破棄される状態になりました。
            /// </summary>
            SilentShutdown,
        }



        /// <summary>
        /// サービスマネージャが管理するサービスの管理情報を保持するデータクラスです
        /// </summary>
        protected class ServiceManagementInfo
        {
            /// <summary>
            /// サービス本体への参照
            /// </summary>
            public GameService Service { get; set; }


            /// <summary>
            /// サービスの状態
            /// </summary>
            public ServiceStatus Status { get; set; }


            /// <summary>
            /// 管理しているサービス本体のクラスが継承している型で、GameService型を直接継承している基本となるサービスの型
            /// </summary>
            public Type BaseGameServiceType { get; set; }


            /// <summary>
            /// このサービスが利用している更新関数テーブル
            /// </summary>
            public Dictionary<GameServiceUpdateTiming, Action> UpdateFunctionTable { get; set; }
        }



        // メンバ変数定義
        protected List<ServiceManagementInfo> serviceManageList;



        /// <summary>
        /// GameServiceManager の初期化を行います
        /// </summary>
        public GameServiceManager()
        {
            // サービス管理用リストのインスタンスを生成
            serviceManageList = new List<ServiceManagementInfo>();
        }


        #region 起動と停止
        /// <summary>
        /// サービスマネージャの起動をします。
        /// </summary>
        protected internal virtual void Startup()
        {
            // 各種更新関数のLoopSystemを生成する
            var mainLoopHead = new ImtPlayerLoopSystem(typeof(GameServiceUpdate.GameServiceMainLoopHead), () => DoUpdateService(GameServiceUpdateTiming.MainLoopHead));
            var preFixedUpdate = new ImtPlayerLoopSystem(typeof(GameServiceUpdate.GameServicePreFixedUpdate), () => DoUpdateService(GameServiceUpdateTiming.PreFixedUpdate));
            var postFixedUpdate = new ImtPlayerLoopSystem(typeof(GameServiceUpdate.GameServicePostFixedUpdate), () => DoUpdateService(GameServiceUpdateTiming.PostFixedUpdate));
            var postPhysicsSimulation = new ImtPlayerLoopSystem(typeof(GameServiceUpdate.GameServicePostPhysicsSimulation), () => DoUpdateService(GameServiceUpdateTiming.PostPhysicsSimulation));
            var postWaitForFixedUpdate = new ImtPlayerLoopSystem(typeof(GameServiceUpdate.GameServicePostWaitForFixedUpdate), () => DoUpdateService(GameServiceUpdateTiming.PostWaitForFixedUpdate));
            var preUpdate = new ImtPlayerLoopSystem(typeof(GameServiceUpdate.GameServicePreUpdate), () => DoUpdateService(GameServiceUpdateTiming.PreUpdate));
            var postUpdate = new ImtPlayerLoopSystem(typeof(GameServiceUpdate.GameServicePostUpdate), () => DoUpdateService(GameServiceUpdateTiming.PostUpdate));
            var preProcessSynchronizationContext = new ImtPlayerLoopSystem(typeof(GameServiceUpdate.GameServicePreProcessSynchronizationContext), () => DoUpdateService(GameServiceUpdateTiming.PreProcessSynchronizationContext));
            var postProcessSynchronizationContext = new ImtPlayerLoopSystem(typeof(GameServiceUpdate.GameServicePostProcessSynchronizationContext), () => DoUpdateService(GameServiceUpdateTiming.PostProcessSynchronizationContext));
            var preAnimation = new ImtPlayerLoopSystem(typeof(GameServiceUpdate.GameServicePreAnimation), () => DoUpdateService(GameServiceUpdateTiming.PreAnimation));
            var postAnimation = new ImtPlayerLoopSystem(typeof(GameServiceUpdate.GameServicePostAnimation), () => DoUpdateService(GameServiceUpdateTiming.PostAnimation));
            var preLateUpdate = new ImtPlayerLoopSystem(typeof(GameServiceUpdate.GameServicePreLateUpdate), () => DoUpdateService(GameServiceUpdateTiming.PreLateUpdate));
            var postLateUpdate = new ImtPlayerLoopSystem(typeof(GameServiceUpdate.GameServicePostLateUpdate), () => DoUpdateService(GameServiceUpdateTiming.PostLateUpdate));
            var preDrawPresent = new ImtPlayerLoopSystem(typeof(GameServiceUpdate.GameServicePreDrawPresent), () => DoUpdateService(GameServiceUpdateTiming.PreDrawPresent));
            var postDrawPresent = new ImtPlayerLoopSystem(typeof(GameServiceUpdate.GameServicePostDrawPresent), () => DoUpdateService(GameServiceUpdateTiming.PostDrawPresent));
            var mainLoopTail = new ImtPlayerLoopSystem(typeof(GameServiceUpdate.GameServiceMainLoopTail), () => DoUpdateService(GameServiceUpdateTiming.MainLoopTail));


            // 処理を差し込むためのPlayerLoopSystemを取得して、処理を差し込んで構築する
            var loopSystem = ImtPlayerLoopSystem.GetLastBuildLoopSystem();
            loopSystem.InsertLoopSystem<GameMain.GameServiceManagerStartup>(InsertTiming.AfterInsert, mainLoopHead);
            loopSystem.InsertLoopSystem<FixedUpdate.ScriptRunBehaviourFixedUpdate>(InsertTiming.BeforeInsert, preFixedUpdate);
            loopSystem.InsertLoopSystem<FixedUpdate.ScriptRunBehaviourFixedUpdate>(InsertTiming.AfterInsert, postFixedUpdate);
            loopSystem.InsertLoopSystem<FixedUpdate.DirectorFixedUpdatePostPhysics>(InsertTiming.AfterInsert, postPhysicsSimulation);
            loopSystem.InsertLoopSystem<FixedUpdate.ScriptRunDelayedFixedFrameRate>(InsertTiming.AfterInsert, postWaitForFixedUpdate);
            loopSystem.InsertLoopSystem<Update.ScriptRunBehaviourUpdate>(InsertTiming.BeforeInsert, preUpdate);
            loopSystem.InsertLoopSystem<Update.ScriptRunBehaviourUpdate>(InsertTiming.AfterInsert, postUpdate);
#if UNITY_2018_1
            loopSystem.InsertLoopSystem<FixedUpdate.ScriptRunDelayedTasks>(InsertTiming.BeforeInsert, preProcessSynchronizationContext);
            loopSystem.InsertLoopSystem<FixedUpdate.ScriptRunDelayedTasks>(InsertTiming.AfterInsert, postProcessSynchronizationContext);
#else
            loopSystem.InsertLoopSystem<Update.ScriptRunDelayedTasks>(InsertTiming.BeforeInsert, preProcessSynchronizationContext);
            loopSystem.InsertLoopSystem<Update.ScriptRunDelayedTasks>(InsertTiming.AfterInsert, postProcessSynchronizationContext);
#endif
            loopSystem.InsertLoopSystem<Update.DirectorUpdate>(InsertTiming.BeforeInsert, preAnimation);
            loopSystem.InsertLoopSystem<Update.DirectorUpdate>(InsertTiming.AfterInsert, postAnimation);
            loopSystem.InsertLoopSystem<PreLateUpdate.ScriptRunBehaviourLateUpdate>(InsertTiming.BeforeInsert, preLateUpdate);
            loopSystem.InsertLoopSystem<PreLateUpdate.ScriptRunBehaviourLateUpdate>(InsertTiming.AfterInsert, postLateUpdate);
            loopSystem.InsertLoopSystem<PostLateUpdate.PresentAfterDraw>(InsertTiming.BeforeInsert, preDrawPresent);
            loopSystem.InsertLoopSystem<PostLateUpdate.PresentAfterDraw>(InsertTiming.AfterInsert, postDrawPresent);
            loopSystem.InsertLoopSystem<GameMain.GameServiceManagerCleanup>(InsertTiming.BeforeInsert, mainLoopTail);
            loopSystem.BuildAndSetUnityPlayerLoop();


            // 永続ゲームオブジェクトを生成してアプリケーションのフォーカス、ポーズのハンドラを登録する
            var persistentGameObject = ImtUnityUtility.CreatePersistentGameObject();
            MonoBehaviourEventBridge.Attach(persistentGameObject, OnApplicationFocus, OnApplicationPause);


            // カメラのハンドラを登録する
            Camera.onPreCull += OnCameraPreCulling;
            Camera.onPreRender += OnCameraPreRendering;
            Camera.onPostRender += OnCameraPostRendering;


            // アプリケーション終了要求ハンドラを登録する
            Application.wantsToQuit += OnApplicationWantsToQuit;
        }


        /// <summary>
        /// サービスマネージャの停止をします。
        /// </summary>
        protected internal virtual void Shutdown()
        {
            // 終了要求ハンドラを解除する
            Application.wantsToQuit -= OnApplicationWantsToQuit;


            // カメラのハンドラを解除する
            Camera.onPreCull -= OnCameraPreCulling;
            Camera.onPreRender -= OnCameraPreRendering;
            Camera.onPostRender -= OnCameraPostRendering;


            // サービスの数分ループ
            for (int i = 0; i < serviceManageList.Count; ++i)
            {
                // サービスの状態が Running, Shutdown, Sleeping 以外なら
                var serviceInfo = serviceManageList[i];
                if (!(serviceInfo.Status == ServiceStatus.Running || serviceInfo.Status == ServiceStatus.Shutdown || serviceInfo.Status == ServiceStatus.Sleeping))
                {
                    // 次へ
                    continue;
                }


                // サービスのシャットダウンを呼ぶ
                serviceInfo.Service.Shutdown();
            }


            // 管理リストをクリアする
            serviceManageList.Clear();
        }
        #endregion


        #region 更新系
        /// <summary>
        /// Addされたサービスの起動処理を行います。
        /// </summary>
        protected internal virtual void StartupServices()
        {
            // サービスの起動情報を受け取る変数を用意
            var serviceStartupInfo = default(GameServiceStartupInfo);


            // サービスの数分ループ
            for (int i = 0; i < serviceManageList.Count; ++i)
            {
                // サービスの状態がReady以外なら
                if (serviceManageList[i].Status != ServiceStatus.Ready)
                {
                    // 次の項目へ
                    continue;
                }


                // サービスを起動状態に設定、サービスの起動処理を実行して更新関数テーブルのキャッシュをする
                serviceManageList[i].Status = ServiceStatus.Running;
                serviceManageList[i].Service.Startup(out serviceStartupInfo);
                serviceManageList[i].UpdateFunctionTable = serviceStartupInfo.UpdateFunctionTable ?? new Dictionary<GameServiceUpdateTiming, Action>();
            }
        }


        /// <summary>
        /// Removeされたサービスの停止処理を行います。
        /// </summary>
        protected internal virtual void CleanupServices()
        {
            // 実際の破棄そのもののステップ必要かどうかを検知するための変数を用意
            var needDeleteStep = false;


            // サービスの数分ループ
            for (int i = 0; i < serviceManageList.Count; ++i)
            {
                // サービスの状態がShutdownでないなら
                if (serviceManageList[i].Status != ServiceStatus.Shutdown)
                {
                    // サービスの状態がサイレントシャットダウンなら
                    if (serviceManageList[i].Status == ServiceStatus.SilentShutdown)
                    {
                        // シャットダウン関数を呼びはしないが破棄ステップでは破棄されるようにマーク
                        needDeleteStep = true;
                    }


                    // 次の項目へ
                    continue;
                }


                // サービスの停止処理を実行する（が、このタイミングでは破棄しない、破棄のタイミングは次のステップで行う）
                serviceManageList[i].Service.Shutdown();


                // 破棄処理を行うようにマーク
                needDeleteStep = true;
            }


            // もし破棄処理をしないなら
            if (!needDeleteStep)
            {
                // ここで終了
                return;
            }


            // サービスの数分ループ
            for (int i = serviceManageList.Count - 1; i >= 0; --i)
            {
                // リストからサービスをパージする
                serviceManageList.RemoveAt(i);
            }
        }
        #endregion


        #region コントロール系
        /// <summary>
        /// 指定されたサービスのアクティブ状態を設定します。
        /// </summary>
        /// <typeparam name="T">アクティブ状態を設定する対象のサービスの型</typeparam>
        /// <param name="active">設定する状態（true=アクティブ false=非アクティブ）</param>
        /// <exception cref="GameServiceNotFoundException">指定された型のサービスが見つかりませんでした</exception>
        /// <exception cref="InvalidOperationException">指定された型のサービスは見つかりましたが、シャットダウン状態になっています</exception>
        public virtual void SetActiveService<T>(bool active) where T : GameService
        {
            // 指定された型から管理情報を取得するが、取得に失敗または取得したがキャスト不可の型なら
            var serviceInfo = GetServiceInfo(typeof(T));
            if (serviceInfo == null || !(serviceInfo.Service is T))
            {
                // サービスを見つけられなかったとして例外を吐く
                throw new GameServiceNotFoundException(typeof(T));
            }


            // もしサービスがシャットダウン予定になっているのなら
            var shutdownStatus = (serviceInfo.Status == ServiceStatus.Shutdown || serviceInfo.Status == ServiceStatus.SilentShutdown);
            if (shutdownStatus)
            {
                // 無効な操作として例外を吐く
                throw new InvalidOperationException($"サービス'{typeof(T).Name}'は、シャットダウン状態です。");
            }


            // アクティブにするなら
            if (active)
            {
                // 現在の状態によって遷移先状態を変える
                serviceInfo.Status = serviceInfo.Status == ServiceStatus.ReadyButSleeping ? ServiceStatus.Ready : ServiceStatus.Running;
            }
            else
            {
                // 現在の状態によって遷移先状態を変える
                serviceInfo.Status = serviceInfo.Status == ServiceStatus.Ready ? ServiceStatus.ReadyButSleeping : ServiceStatus.Sleeping;
            }
        }


        /// <summary>
        /// 指定されたサービスがアクティブかどうかを確認します。
        /// </summary>
        /// <typeparam name="T">アクティブ状態を確認するサービスの型</typeparam>
        /// <returns>アクティブの場合は true を、非アクティブの場合は false を返します</returns>
        /// <exception cref="GameServiceNotFoundException">指定された型のサービスが見つかりませんでした</exception>
        public virtual bool IsActiveService<T>() where T : GameService
        {
            // 指定された型から管理情報を取得するが、取得に失敗または取得したがキャスト不可の型なら
            var serviceInfo = GetServiceInfo(typeof(T));
            if (serviceInfo == null || !(serviceInfo.Service is T))
            {
                // サービスを見つけられなかったとして例外を吐く
                throw new GameServiceNotFoundException(typeof(T));
            }


            // Running, Ready以外は全部非アクティブ
            return serviceInfo.Status == ServiceStatus.Ready || serviceInfo.Status == ServiceStatus.Running;
        }
        #endregion


        #region リスト操作系
        /// <summary>
        /// 指定されたサービスの追加をします。
        /// また、サービスの型が同じインスタンスまたは同一継承元インスタンスが存在する場合は例外がスローされます。
        /// ただし、サービスは直ちには起動せずフレーム開始のタイミングで起動することに注意してください。
        /// さらに、シャットダウン対象となっているサービスの場合は無効な操作として例外がスローされます。
        /// </summary>
        /// <param name="service">追加するサービスのインスタンス</param>
        /// <exception cref="ArgumentNullException">service が null です</exception>
        /// <exception cref="GameServiceAlreadyExistsException">既に同じ型のサービスが追加されています</exception>
        /// <exception cref="InvalidOperationException">追加しようとしたサービスが既にシャットダウン状態です</exception>
        public virtual void AddService(GameService service)
        {
            // null が渡されちゃったら
            if (service == null)
            {
                // そんな処理は許されない
                throw new ArgumentNullException(nameof(service));
            }


            // まずは管理リストから情報が取り出せるなら
            var serviceInfo = GetServiceInfo(service.GetType());
            if (serviceInfo != null)
            {
                // 既に存在しているサービスの型を取り出す
                var existsServiceType = serviceInfo.Service.GetType();


                // もし既にシャットダウン状態でかつ、追加しようとしたサービスの型がキャスト可能な型なら
                var shutdownState = (serviceInfo.Status == ServiceStatus.Shutdown || serviceInfo.Status == ServiceStatus.SilentShutdown);
                if (shutdownState && service.GetType().IsAssignableFrom(existsServiceType))
                {
                    // サービスはシャットダウン状態である例外を吐く
                    throw new InvalidOperationException($"サービス'{service.GetType().Name}'は、既にシャットダウン状態です。");
                }


                // 既に同じサービスがあるとして例外を吐く
                throw new GameServiceAlreadyExistsException(existsServiceType, serviceInfo.BaseGameServiceType);
            }


            // 管理リストから情報を取り出せないのなら追加が可能なサービスとして追加する
            serviceManageList.Add(new ServiceManagementInfo()
            {
                Status = ServiceStatus.Ready,
                Service = service,
                BaseGameServiceType = GetBaseGameServiceType(service),
            });
        }


        /// <summary>
        /// 指定されたサービスの追加をします。
        /// この関数は AddService() 関数と違い、同じ型のサービスまたは同一継承元インスタンスの追加は出来ませんが、例外をスローしません。
        /// ただし、サービスは直ちには起動せずフレーム開始のタイミングで起動することに注意してください。
        /// </summary>
        /// <param name="service">追加するサービスのインスタンス</param>
        /// <returns>サービスの追加が出来た場合は true を、出来なかった場合は false を返します</returns>
        /// <exception cref="ArgumentNullException">service が null です</exception>
        public virtual bool TryAddService(GameService service)
        {
            // null が渡されちゃったら
            if (service == null)
            {
                // いくらTry系関数とは言えど、そんな処理は許されない
                throw new ArgumentNullException(nameof(service));
            }


            // 管理リストから情報が取り出せるなら
            var serviceInfo = GetServiceInfo(service.GetType());
            if (serviceInfo != null)
            {
                // 既に登録済みの何かのサービスがいるとしてfalseを返す
                return false;
            }


            // 管理リストから情報を取り出せないのなら追加が可能なサービスとして追加する
            serviceManageList.Add(new ServiceManagementInfo()
            {
                Status = ServiceStatus.Ready,
                Service = service,
                BaseGameServiceType = GetBaseGameServiceType(service),
            });


            // 追加が出来たということを返す
            return true;
        }


        /// <summary>
        /// 指定された型のサービスを取得します。
        /// また、サービスが見つけられなかった場合は例外がスローされます。
        /// </summary>
        /// <typeparam name="T">取得するサービスの型</typeparam>
        /// <returns>見つけられたサービスのインスタンスを返します</returns>
        /// <exception cref="GameServiceNotFoundException">指定された型のサービスが見つかりませんでした</exception>
        public virtual T GetService<T>() where T : GameService
        {
            // 指定された型から管理情報を取得するが、取得に失敗または取得したがキャスト不可の型なら
            var serviceInfo = GetServiceInfo(typeof(T));
            if (serviceInfo == null || !(serviceInfo.Service is T))
            {
                // サービスを見つけられなかったとして例外を吐く
                throw new GameServiceNotFoundException(typeof(T));
            }


            // 見つけたサービスを返す
            return (T)serviceInfo.Service;
        }


        /// <summary>
        /// 指定された型のサービスを取得します
        /// </summary>
        /// <typeparam name="T">取得するサービスの型</typeparam>
        /// <param name="service">見つけられたサービスのインスタンスを設定しますが、見つけられなかった場合はnullが設定されます</param>
        public virtual bool TryGetService<T>(out T service) where T : GameService
        {
            // 指定された型から管理情報を取得するが、取得に失敗または取得したがキャスト不可の型なら
            var serviceInfo = GetServiceInfo(typeof(T));
            if (serviceInfo == null || !(serviceInfo.Service is T))
            {
                // サービスにnullを設定して取得できなかったことを返す
                service = null;
                return false;
            }


            // 見つけたサービスを設定して見つけられた事を返す
            service = (T)serviceInfo.Service;
            return true;
        }


        /// <summary>
        /// 指定された型のサービスを削除します。
        /// しかし、サービスは直ちには削除されずフレーム終了のタイミングで削除されることに注意してください。
        /// </summary>
        /// <typeparam name="T">削除するサービスの型</typeparam>
        public virtual void RemoveService<T>() where T : GameService
        {
            // 指定された型から管理情報を取得するが、取得に失敗または取得したがキャスト不可の型なら
            var serviceInfo = GetServiceInfo(typeof(T));
            if (serviceInfo == null || !(serviceInfo.Service is T))
            {
                // 何事もなかったかのように終了する
                return;
            }


            // サービスの状態がまだReady系なら静かに死ぬようにマークする
            var readyStatus = (serviceInfo.Status == ServiceStatus.Ready || serviceInfo.Status == ServiceStatus.ReadyButSleeping);
            if (readyStatus)
            {
                // 静かに消えるがよい
                serviceInfo.Status = ServiceStatus.SilentShutdown;
                return;
            }


            // 通常は普通に死ぬようにマークする
            serviceInfo.Status = ServiceStatus.Shutdown;
        }


        /// <summary>
        /// 指定された型のサービスが、単純に存在するか確認します。
        /// この関数は、シャットダウンされうかどうかの状態を考慮しないことに気をつけて下さい。
        /// </summary>
        /// <typeparam name="T">存在を確認するサービスの型</typeparam>
        /// <returns>サービスが存在している場合は true を、存在しない場合は false を返します</returns>
        public virtual bool Exists<T>() where T : GameService
        {
            // 指定された型から管理情報を取得するが、取得に失敗または取得したがキャスト不可の型なら
            var serviceInfo = GetServiceInfo(typeof(T));
            if (serviceInfo == null || !(serviceInfo.Service is T))
            {
                // サービスが無いことを返す
                return false;
            }


            // 見つけたサービスを見つけられたことを返す
            return true;
        }
        #endregion


        #region Unityイベントハンドラ
        /// <summary>
        /// アプリケーションが終了を要求してきた時の処理を行います
        /// </summary>
        /// <returns>サービスが終了を許可した場合は true を、拒否された場合は false を返します</returns>
        private bool OnApplicationWantsToQuit()
        {
            // サービスの数分回る
            for (int i = 0; i < serviceManageList.Count; ++i)
            {
                // サービスの状態が Running 以外なら
                var service = serviceManageList[i];
                if (service.Status != ServiceStatus.Running)
                {
                    // 次へ
                    continue;
                }


                // サービスに終了判断をしてもらい、拒否されたら
                if (service.Service.JudgeGameShutdown() == GameShutdownAnswer.Reject)
                {
                    // この段階でfalseを返す
                    return false;
                }
            }


            // 最後まで回りきったら全員が許可したとしてtrueを返す
            return true;
        }


        /// <summary>
        /// Unityプレイヤーのフォーカス状態に変化があった時の処理を行います
        /// </summary>
        /// <param name="focus">フォーカスを得られたときは true を、得られなかったときは false が渡されます</param>
        private void OnApplicationFocus(bool focus)
        {
            // もしフォーカスを得られたのなら
            if (focus)
            {
                // FocusInのサービス呼び出しをする
                DoUpdateService(GameServiceUpdateTiming.OnApplicationFocusIn);
                return;
            }


            // FocusOutのサービス呼び出しをする
            DoUpdateService(GameServiceUpdateTiming.OnApplicationFocusOut);
        }


        /// <summary>
        /// Unityプレイヤーの再生状態に変化があった時の処理を行います
        /// </summary>
        /// <param name="pause">一時停止した場合は true を、再開した場合は false が渡されます</param>
        private void OnApplicationPause(bool pause)
        {
            // もし一時停止なら
            if (pause)
            {
                // Suspendのサービス呼び出しをする
                DoUpdateService(GameServiceUpdateTiming.OnApplicationSuspend);
                return;
            }


            // Resumeのサービス呼び出しをする
            DoUpdateService(GameServiceUpdateTiming.OnApplicationResume);
        }


        /// <summary>
        /// カメラのカリング処理を始める直前の処理を行います
        /// </summary>
        /// <param name="targetCamera">処理するカメラ</param>
        private void OnCameraPreCulling(Camera targetCamera)
        {
            // CameraPreCullingのサービス呼び出しをする
            DoUpdateService(GameServiceUpdateTiming.CameraPreCulling);
        }


        /// <summary>
        /// カメラのレンダリング処理を始める直前の処理を行います
        /// </summary>
        /// <param name="targetCamera">処理するカメラ</param>
        private void OnCameraPreRendering(Camera targetCamera)
        {
            // CameraPreRenderingのサービス呼び出しをする
            DoUpdateService(GameServiceUpdateTiming.CameraPreRendering);
        }


        /// <summary>
        /// カメラのレンダリング処理が終わった直後の処理を行います
        /// </summary>
        /// <param name="targetCamera"></param>
        private void OnCameraPostRendering(Camera targetCamera)
        {
            // CameraPostRenderingのサービス呼び出しをする
            DoUpdateService(GameServiceUpdateTiming.CameraPostRendering);
        }
        #endregion


        #region 共通ロジック系
        /// <summary>
        /// 指定されたタイミングのサービス更新関数を実行します。
        /// また、実行する対象はステータスに応じた呼び出しを行います。
        /// </summary>
        /// <param name="timing">実行するべきサービスの更新関数のタイミング</param>
        private void DoUpdateService(GameServiceUpdateTiming timing)
        {
            // サービスの数分回る
            for (int i = 0; i < serviceManageList.Count; ++i)
            {
                // サービス情報を取得する
                var serviceInfo = serviceManageList[i];


                // サービスの状態がRunning以外なら
                if (serviceInfo.Status != ServiceStatus.Running)
                {
                    // 次のサービスへ
                    continue;
                }


                // 該当のタイミングの更新関数を持っていないなら
                Action updateFunction;
                if (!serviceInfo.UpdateFunctionTable.TryGetValue(timing, out updateFunction))
                {
                    // 次のサービスへ
                    continue;
                }


                // 該当タイミングの更新関数を持っているのなら更新関数を叩く
                updateFunction();
            }
        }


        /// <summary>
        /// 指定された型からサービス管理情報を取得します。
        /// また、型一致条件は基本クラス一致となります。
        /// </summary>
        /// <param name="serviceType">取得したいサービスの型。ただし、内部ではGameServiceを直接継承している型が採用されます。</param>
        /// <returns>指定された型から取り出せるサービス管理情報を返しますが、取得できなかった場合は null を返します。</returns>
        private ServiceManagementInfo GetServiceInfo(Type serviceType)
        {
            // まずは基本型を取り出すがこの時点で取り出せなかったら
            var baseGameServiceType = GetBaseGameServiceType(serviceType);
            if (baseGameServiceType == null)
            {
                // ダメだったよ
                return null;
            }


            // サービスの数分回る
            for (int i = 0; i < serviceManageList.Count; ++i)
            {
                // 基本型が一致するサービスなら
                var serviceInfo = serviceManageList[i];
                if (serviceInfo.BaseGameServiceType == baseGameServiceType)
                {
                    // この情報を返す
                    return serviceInfo;
                }
            }


            // ループから抜けてきたという事は見つからなかったということ
            return null;
        }


        /// <summary>
        /// 指定されたゲームサービスの、GameService型を直接継承している基本クラスの型を取得します
        /// </summary>
        /// <param name="service">GameService型を直接継承しているクラスの型を取り出したいサービス</param>
        /// <returns>GameService型を直接継承している基本クラスの型を返します</returns>
        private Type GetBaseGameServiceType(GameService service)
        {
            // 渡されたサービスの型を受け取って型取得結果をそのまま返す（GameServiceで受け取っているのでnullになることはない）
            return GetBaseGameServiceType(service.GetType());
        }


        /// <summary>
        /// 指定された型から、GameService型を直接継承している基本クラスの型を取得します
        /// </summary>
        /// <param name="serviceType">GameService型を直接継承しているクラスの型を取り出したい型</param>
        /// <returns>GameService型を直接継承している基本クラスの型が見つけられた場合はその型を返しますが、見つけられなかった場合は null を返します</returns>
        private Type GetBaseGameServiceType(Type serviceType)
        {
            // GameServiceの型を取得
            var gameServiceType = typeof(GameService);


            // 直接GameService型を継承している型にたどり着くまでループ
            while (serviceType.BaseType != gameServiceType)
            {
                // 現在のサービス型の継承元の型を現在のサービスの型にするがnullなら
                serviceType = serviceType.BaseType;
                if (serviceType == null)
                {
                    // そもそも見つけられなかった
                    return null;
                }
            }


            // たどり着いた型を返す
            return serviceType;
        }
        #endregion
    }
}