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
using System.Diagnostics;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace IceMilkTea.Core
{
    /// <summary>
    /// IceMilkTeaのゲームサービスを管理及び制御を行うクラスです。
    /// </summary>
    public class GameServiceManager
    {
        /// <summary>
        /// ゲームサービスマネージャのサービス起動ルーチンを実行する型です
        /// </summary>
        public struct GameServiceManagerStartup { }



        /// <summary>
        /// ゲームサービスマネージャのサービス終了ルーチンを実行する型です
        /// </summary>
        public struct GameServiceManagerCleanup { }



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
        private readonly Stopwatch stopwatch;
        private readonly List<ServiceManagementInfo> serviceManageList;
        private long serviceProcessTick;



        /// <summary>
        /// 1フレームで消費したサービスの処理時間をマイクロ秒で取得します
        /// </summary>
        public double ServiceProcessTime => (serviceProcessTick / (double)Stopwatch.Frequency) * 1000.0 * 1000.0;



        /// <summary>
        /// GameServiceManager の初期化を行います
        /// </summary>
        public GameServiceManager()
        {
            // サービス管理用リストのインスタンスを生成
            serviceManageList = new List<ServiceManagementInfo>();
            stopwatch = new Stopwatch();
        }


        #region 起動と停止
        /// <summary>
        /// サービスマネージャの起動をします。
        /// </summary>
        protected internal virtual void Startup()
        {
            // サービスマネージャの開始と終了のループシステムを生成
            var startupGameServiceLoopSystem = new ImtPlayerLoopSystem(typeof(GameServiceManagerStartup), StartupServices);
            var cleanupGameServiceLoopSystem = new ImtPlayerLoopSystem(typeof(GameServiceManagerCleanup), CleanupServices);


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
            var loopSystem = ImtPlayerLoopSystem.GetCurrentPlayerLoop();
            loopSystem.Insert<Initialization.DirectorSampleTime>(InsertTiming.BeforeInsert, startupGameServiceLoopSystem);
            loopSystem.Insert<GameServiceManagerStartup>(InsertTiming.AfterInsert, mainLoopHead);
            loopSystem.Insert<FixedUpdate.ScriptRunBehaviourFixedUpdate>(InsertTiming.BeforeInsert, preFixedUpdate);
            loopSystem.Insert<FixedUpdate.ScriptRunBehaviourFixedUpdate>(InsertTiming.AfterInsert, postFixedUpdate);
            loopSystem.Insert<FixedUpdate.DirectorFixedUpdatePostPhysics>(InsertTiming.AfterInsert, postPhysicsSimulation);
            loopSystem.Insert<FixedUpdate.ScriptRunDelayedFixedFrameRate>(InsertTiming.AfterInsert, postWaitForFixedUpdate);
            loopSystem.Insert<Update.ScriptRunBehaviourUpdate>(InsertTiming.BeforeInsert, preUpdate);
            loopSystem.Insert<Update.ScriptRunBehaviourUpdate>(InsertTiming.AfterInsert, postUpdate);
            loopSystem.Insert<Update.ScriptRunDelayedTasks>(InsertTiming.BeforeInsert, preProcessSynchronizationContext);
            loopSystem.Insert<Update.ScriptRunDelayedTasks>(InsertTiming.AfterInsert, postProcessSynchronizationContext);
            loopSystem.Insert<Update.DirectorUpdate>(InsertTiming.BeforeInsert, preAnimation);
            loopSystem.Insert<Update.DirectorUpdate>(InsertTiming.AfterInsert, postAnimation);
            loopSystem.Insert<PreLateUpdate.ScriptRunBehaviourLateUpdate>(InsertTiming.BeforeInsert, preLateUpdate);
            loopSystem.Insert<PreLateUpdate.ScriptRunBehaviourLateUpdate>(InsertTiming.AfterInsert, postLateUpdate);
            loopSystem.Insert<PostLateUpdate.PresentAfterDraw>(InsertTiming.BeforeInsert, preDrawPresent);
            loopSystem.Insert<PostLateUpdate.PresentAfterDraw>(InsertTiming.AfterInsert, postDrawPresent);
            loopSystem.Insert<PostLateUpdate.ExecuteGameCenterCallbacks>(InsertTiming.AfterInsert, cleanupGameServiceLoopSystem);
            loopSystem.Insert<GameServiceManagerCleanup>(InsertTiming.BeforeInsert, mainLoopTail);
            loopSystem.BuildAndSetUnityPlayerLoop();


            // 永続ゲームオブジェクトを生成してアプリケーションのフォーカス、ポーズのハンドラを登録する
            var persistentGameObject = ImtUnityUtility.CreatePersistentGameObject();
            var eventBridge = MonoBehaviourEventBridge.Attach(persistentGameObject);
            eventBridge.SetApplicationFocusFunction(OnApplicationFocus);
            eventBridge.SetApplicationPauseFunction(OnApplicationPause);
            eventBridge.SetEndOfFrameFunction(OnEndOfFrame);


            // カメラのハンドラを登録する
            Camera.onPreCull += OnCameraPreCulling;
            Camera.onPreRender += OnCameraPreRendering;
            Camera.onPostRender += OnCameraPostRendering;
        }


        /// <summary>
        /// サービスマネージャの停止をします。
        /// </summary>
        protected internal virtual void Shutdown()
        {
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
            stopwatch.Start();


            // サービスの数分ループ
            for (int i = 0; i < serviceManageList.Count; ++i)
            {
                // サービスの状態がReady以外なら
                if (serviceManageList[i].Status != ServiceStatus.Ready)
                {
                    // 次の項目へ
                    continue;
                }


                try
                {
                    // サービスを起動状態に設定、サービスの起動処理を実行して更新関数テーブルのキャッシュをする
                    serviceManageList[i].Status = ServiceStatus.Running;
                    serviceManageList[i].Service.Startup(out var serviceStartupInfo);
                    serviceManageList[i].UpdateFunctionTable = serviceStartupInfo.UpdateFunctionTable ?? new Dictionary<GameServiceUpdateTiming, Action>();
                }
                catch
                {
                    stopwatch.Stop();
                    throw;
                }
            }


            stopwatch.Stop();
        }


        /// <summary>
        /// Removeされたサービスの停止処理を行います。
        /// </summary>
        protected internal virtual void CleanupServices()
        {
            // サービスの処理時間計測用ストップウォッチの再起動
            stopwatch.Restart();


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
                try
                {
                    serviceManageList[i].Service.Shutdown();
                }
                catch
                {
                    stopwatch.Stop();
                    throw;
                }


                needDeleteStep = true;
            }


            // もし破棄処理をしないなら
            if (!needDeleteStep)
            {
                // 処理時間計測用ストップウォッチの一時停止ここで終了
                stopwatch.Stop();
                serviceProcessTick = stopwatch.ElapsedTicks;
                return;
            }


            // サービスの数分ケツからループ
            for (int i = serviceManageList.Count - 1; i >= 0; --i)
            {
                var status = serviceManageList[i].Status;
                var isShutdown = status == ServiceStatus.Shutdown || status == ServiceStatus.SilentShutdown;
                if (isShutdown)
                {
                    serviceManageList.RemoveAt(i);
                }
            }


            // 処理時間計測用ストップウォッチの一時停止
            stopwatch.Stop();
            serviceProcessTick = stopwatch.ElapsedTicks;
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

            RemoveService(serviceInfo);
        }

        private void RemoveService(ServiceManagementInfo serviceInfo)
        {
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

        public virtual void RemoveAllServices()
        {
            for (int i = 0; i < serviceManageList.Count; ++i)
            {
                var serviceInfo = serviceManageList[i];
                RemoveService(serviceInfo);
            }
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


        /// <summary>
        /// 現在管理しているサービスの反復処理を実行します
        /// </summary>
        /// <param name="action">サービスに対して処理する関数</param>
        /// <exception cref="ArgumentNullException">action が null です</exception>
        public void ServiceForEach(Action<GameService> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }


            foreach (var serviceInfo in serviceManageList)
            {
                action(serviceInfo.Service);
            }
        }
        #endregion


        #region Unityイベントハンドラ
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
        /// UnityプレイヤーのWaitForEndOfFrameの継続処理を行います
        /// </summary>
        private void OnEndOfFrame()
        {
            // EndOfFrameのサービス呼び出しをする
            DoUpdateService(GameServiceUpdateTiming.OnEndOfFrame);
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
            // 処理負荷計測用ストップウォッチを再開する
            stopwatch.Start();


            for (int i = 0; i < serviceManageList.Count; ++i)
            {
                var serviceInfo = serviceManageList[i];
                if (serviceInfo.Status == ServiceStatus.Running && serviceInfo.UpdateFunctionTable.TryGetValue(timing, out var updateFunction))
                {
                    try
                    {
                        updateFunction();
                    }
                    catch
                    {
                        stopwatch.Stop();
                        throw;
                    }
                }
            }


            // 処理負荷計測用ストップウォッチを一時停止する
            stopwatch.Stop();
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