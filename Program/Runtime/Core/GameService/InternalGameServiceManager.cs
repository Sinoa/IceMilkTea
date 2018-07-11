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

using System.Collections.Generic;

namespace IceMilkTea.Core
{
    /// <summary>
    /// IceMilkTeaが標準で提供するサービスマネージャクラスです。
    /// </summary>
    internal sealed class InternalGameServiceManager : GameServiceManager
    {
        /// <summary>
        /// サービスの状態を表します
        /// </summary>
        private enum ServiceStatus
        {
        }


        /// <summary>
        /// サービスマネージャが管理するサービスの管理情報を保持するデータクラスです
        /// </summary>
        private class ServiceManagementInfo
        {
        }



        // メンバ変数定義
        private List<GameService> serviceList;



        /// <summary>
        /// GameServiceManager の初期化を行います
        /// </summary>
        public InternalGameServiceManager()
        {
            // メンバの初期化をする
            serviceList = new List<GameService>();
        }


        #region 起動と停止
        /// <summary>
        /// サービスマネージャの起動をします。
        /// このクラスでは何もしません。
        /// </summary>
        protected internal override void Startup()
        {
        }


        /// <summary>
        /// サービスマネージャの停止をします。
        /// </summary>
        protected internal override void Shutdown()
        {
        }
        #endregion


        #region コントロール系
        /// <summary>
        /// 指定されたサービスのアクティブ状態を設定します。
        /// </summary>
        /// <typeparam name="T">アクティブ状態を設定する対象のサービスの型</typeparam>
        /// <param name="active">設定する状態（true=アクティブ false=非アクティブ）</param>
        /// <exception cref="GameServiceNotFoundException">指定された型のサービスが見つかりませんでした</exception>
        public override void SetActiveService<T>(bool active)
        {
        }


        /// <summary>
        /// 指定されたサービスがアクティブかどうかを確認します。
        /// </summary>
        /// <typeparam name="T">アクティブ状態を確認するサービスの型</typeparam>
        /// <returns>アクティブの場合は true を、非アクティブの場合は false を返します</returns>
        /// <exception cref="GameServiceNotFoundException">指定された型のサービスが見つかりませんでした</exception>
        public override bool IsActiveService<T>()
        {
            throw new System.NotImplementedException();
        }
        #endregion


        #region 更新系
        /// <summary>
        /// Addされたサービスの起動処理を行います。
        /// </summary>
        protected internal override void StartupServices()
        {
        }


        /// <summary>
        /// Removeされたサービスの停止処理を行います。
        /// </summary>
        protected internal override void CleanupServices()
        {
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
        public override void AddService(GameService service)
        {
            // 既にサービスが存在するなら
            if (IsExistsService(service))
            {
                // 例外を投げる
                throw new GameServiceAlreadyExistsException(service.GetType(), service.GetType());
            }


            // まだ追加されていないので追加する
            serviceList.Add(service);
        }


        /// <summary>
        /// 指定されたサービスの追加をします。
        /// この関数は AddService() 関数と違い、同じ型のサービスまたは同一継承元インスタンスの追加は出来ませんが、例外をスローしません。
        /// ただし、サービスは直ちには起動せずフレーム開始のタイミングで起動することに注意してください。
        /// </summary>
        /// <param name="service">追加するサービスのインスタンス</param>
        /// <returns>サービスの追加が出来た場合は true を、出来なかった場合は false を返します</returns>
        public override bool TryAddService(GameService service)
        {
            // 既にサービスが存在するなら
            if (IsExistsService(service))
            {
                // 追加出来なかったことを返す
                return false;
            }


            // サービスを追加して追加出来たことを返す
            serviceList.Add(service);
            return true;
        }


        /// <summary>
        /// 指定された型のサービスを取得します。
        /// また、サービスが見つけられなかった場合は例外がスローされます。
        /// </summary>
        /// <typeparam name="T">取得するサービスの型</typeparam>
        /// <returns>見つけられたサービスのインスタンスを返します</returns>
        /// <exception cref="GameServiceNotFoundException">指定された型のサービスが見つかりませんでした</exception>
        public override T GetService<T>()
        {
            // サービスを探して見つけられたのなら
            var inService = FindService<T>();
            if (inService != null)
            {
                // サービスを返す
                return inService;
            }


            // ここまで来てしまったのなら例外を吐く
            throw new GameServiceNotFoundException(typeof(T));
        }


        /// <summary>
        /// 指定された型のサービスを取得します
        /// </summary>
        /// <typeparam name="T">取得するサービスの型</typeparam>
        /// <param name="service">見つけられたサービスのインスタンスを設定しますが、見つけられなかった場合はnullが設定されます</param>
        public override bool TryGetService<T>(out T service)
        {
            // サービスを探して見つけられたのなら
            var inService = FindService<T>();
            if (inService != null)
            {
                // サービスを設定して成功を返す
                service = inService;
                return true;
            }


            // ここまで来てしまったのならnullを設定して失敗を返す
            service = null;
            return false;
        }


        /// <summary>
        /// 指定された型のサービスを削除します。
        /// しかし、サービスは直ちには削除されずフレーム終了のタイミングで削除されることに注意してください。
        /// </summary>
        /// <typeparam name="T">削除するサービスの型</typeparam>
        public override void RemoveService<T>()
        {
            // サービスの数分回る
            for (int i = 0; i < serviceList.Count; ++i)
            {
                // もし指定された型のサービスなら
                if (serviceList[i].GetType() == typeof(T))
                {
                    // 該当インデックスのサービスをシャットダウンして削除する
                    var inService = serviceList[i];
                    inService.Shutdown();
                    serviceList.RemoveAt(i);
                    return;
                }
            }
        }
        #endregion


        #region ユーティリティ系
        /// <summary>
        /// 指定されたサービスが存在するか否かを調べます
        /// </summary>
        /// <param name="service">調べるサービス</param>
        /// <returns>存在するなら true を、存在しないなら false を返します</returns>
        private bool IsExistsService(GameService service)
        {
            // サービスの型情報を取り出す
            var serviceType = service.GetType();


            // 現在所持中のサービス分ループ
            foreach (var inService in serviceList)
            {
                // 指定されたサービスの型があるなら
                if (inService.GetType() == serviceType)
                {
                    // 存在していることを返す
                    return true;
                }
            }


            // ここまで到達したのなら存在しないとする
            return false;
        }


        /// <summary>
        /// 指定された型のサービスを検索します
        /// </summary>
        /// <typeparam name="T">検索するサービスの型</typeparam>
        /// <returns>見つけられた場合は、サービスのインスタンスを返しますが、見つけられなかった場合はnullを返します</returns>
        private T FindService<T>() where T : GameService
        {
            // 調べたい型
            var serviceType = typeof(T);


            // サービスの数分ループ
            foreach (var inService in serviceList)
            {
                // もし取得したい型なら
                if (inService.GetType() == serviceType)
                {
                    // そのサービスを返す
                    return (T)inService;
                }
            }


            // ここまで到達したのならnullを返す
            return null;
        }
        #endregion


        #region 各種更新関数
        private void MainLoopHead()
        {
        }


        private void PreFixedUpdate()
        {
        }


        private void PostFixedUpdate()
        {
        }


        private void PostPhysicsSimulation()
        {
        }


        private void PostWaitForFixedUpdate()
        {
        }


        private void PreProcessSynchronizationContext()
        {
        }


        private void PostProcessSynchronizationContext()
        {
        }


        private void PreUpdate()
        {
        }


        private void PostUpdate()
        {
        }


        private void PreAnimation()
        {
        }


        private void PostAnimation()
        {
        }


        private void PreLateUpdate()
        {
        }


        private void PostLateUpdate()
        {
        }


        private void PreRendering()
        {
        }


        private void PostRendering()
        {
        }


        private void MainLoopTail()
        {
        }


        private void OnApplicationFocusIn()
        {
        }


        private void OnApplicationFocusOut()
        {
        }


        private void OnApplicationSuspend()
        {
        }


        private void OnApplicationResume()
        {
        }


        private void CameraPreCulling()
        {
        }


        private void CameraPreRendering()
        {
        }


        private void CameraPostRendering()
        {
        }
        #endregion
    }
}