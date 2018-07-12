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
using UnityEngine.Experimental.PlayerLoop;

namespace IceMilkTea.Core
{
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
            loopSystem.InsertLoopSystem<Update.ScriptRunDelayedTasks>(InsertTiming.BeforeInsert, preProcessSynchronizationContext);
            loopSystem.InsertLoopSystem<Update.ScriptRunDelayedTasks>(InsertTiming.AfterInsert, postProcessSynchronizationContext);
            loopSystem.InsertLoopSystem<Update.DirectorUpdate>(InsertTiming.BeforeInsert, preAnimation);
            loopSystem.InsertLoopSystem<Update.DirectorUpdate>(InsertTiming.AfterInsert, postAnimation);
            loopSystem.InsertLoopSystem<PreLateUpdate.ScriptRunBehaviourLateUpdate>(InsertTiming.BeforeInsert, preLateUpdate);
            loopSystem.InsertLoopSystem<PreLateUpdate.ScriptRunBehaviourLateUpdate>(InsertTiming.AfterInsert, postLateUpdate);
            loopSystem.InsertLoopSystem<PostLateUpdate.PresentAfterDraw>(InsertTiming.BeforeInsert, preDrawPresent);
            loopSystem.InsertLoopSystem<PostLateUpdate.PresentAfterDraw>(InsertTiming.AfterInsert, postDrawPresent);
            loopSystem.InsertLoopSystem<GameMain.GameServiceManagerCleanup>(InsertTiming.BeforeInsert, mainLoopTail);
            loopSystem.BuildAndSetUnityPlayerLoop();
        }


        /// <summary>
        /// サービスマネージャの停止をします。
        /// </summary>
        protected internal virtual void Shutdown()
        {
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
        /// <exception cref="InvalidOperationException">指定された型のサービスは見つかりましたが、破棄状態になっています</exception>
        public virtual void SetActiveService<T>(bool active)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// 指定されたサービスがアクティブかどうかを確認します。
        /// </summary>
        /// <typeparam name="T">アクティブ状態を確認するサービスの型</typeparam>
        /// <returns>アクティブの場合は true を、非アクティブの場合は false を返します</returns>
        /// <exception cref="GameServiceNotFoundException">指定された型のサービスが見つかりませんでした</exception>
        public virtual bool IsActiveService<T>()
        {
            throw new NotImplementedException();
        }
        #endregion


        #region リスト操作系
        /// <summary>
        /// 指定されたサービスの追加をします。
        /// また、サービスの型が同じインスタンスまたは同一継承元インスタンスが存在する場合は例外がスローされます。
        /// ただし、サービスは直ちには起動せずフレーム開始のタイミングで起動することに注意してください。
        /// </summary>
        /// <param name="service">追加するサービスのインスタンス</param>
        /// <exception cref="GameServiceAlreadyExistsException">既に同じ型のサービスが追加されています</exception>
        public virtual void AddService(GameService service)
        {
        }


        /// <summary>
        /// 指定されたサービスの追加をします。
        /// この関数は AddService() 関数と違い、同じ型のサービスまたは同一継承元インスタンスの追加は出来ませんが、例外をスローしません。
        /// ただし、サービスは直ちには起動せずフレーム開始のタイミングで起動することに注意してください。
        /// </summary>
        /// <param name="service">追加するサービスのインスタンス</param>
        /// <returns>サービスの追加が出来た場合は true を、出来なかった場合は false を返します</returns>
        public virtual bool TryAddService(GameService service)
        {
            return false;
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
            return null;
        }


        /// <summary>
        /// 指定された型のサービスを取得します
        /// </summary>
        /// <typeparam name="T">取得するサービスの型</typeparam>
        /// <param name="service">見つけられたサービスのインスタンスを設定しますが、見つけられなかった場合はnullが設定されます</param>
        public virtual bool TryGetService<T>(out T service) where T : GameService
        {
            service = null;
            return false;
        }


        /// <summary>
        /// 指定された型のサービスを削除します。
        /// しかし、サービスは直ちには削除されずフレーム終了のタイミングで削除されることに注意してください。
        /// </summary>
        /// <typeparam name="T">削除するサービスの型</typeparam>
        public virtual void RemoveService<T>() where T : GameService
        {
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
        /// 指定されたサービスが存在するか否かを調べます
        /// </summary>
        /// <param name="service">調べるサービス</param>
        /// <returns>存在するなら true を、存在しないなら false を返します</returns>
        private bool IsExistsService(GameService service)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// 指定された型のサービスを検索します
        /// </summary>
        /// <typeparam name="T">検索するサービスの型</typeparam>
        /// <returns>見つけられた場合は、サービスのインスタンスを返しますが、見つけられなかった場合はnullを返します</returns>
        private T FindService<T>() where T : GameService
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}