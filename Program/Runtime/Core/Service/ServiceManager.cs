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
    /// IceMilkTeaのサービスを管理及び制御を行うクラスです
    /// </summary>
    public sealed class ServiceManager
    {
        // メンバ変数定義
        private List<ServiceProvider> serviceList;



        /// <summary>
        /// ServiceManagerの初期化を行います
        /// </summary>
        internal ServiceManager()
        {
            // メンバの初期化をする
            serviceList = new List<ServiceProvider>();
        }


        #region リスト操作系
        /// <summary>
        /// 指定されたサービスプロバイダの追加をします。
        /// また、サービスプロバイダの型が同じインスタンスが存在する場合は例外がスローされます。
        /// </summary>
        /// <param name="service">追加するサービスプロバイダのインスタンス</param>
        /// <exception cref="ServiceAlreadyExistsException">既に同じ型のサービスプロバイダが追加されています</exception>
        public void AddService(ServiceProvider service)
        {
            // 既にサービスが存在するなら
            if (IsExistsService(service))
            {
                // 例外を投げる
                throw new ServiceAlreadyExistsException(service.GetType());
            }


            // まだ追加されていないので追加する
            serviceList.Add(service);
        }


        /// <summary>
        /// 指定されたサービスプロバイダの追加をします。
        /// この関数は AddService() 関数と違い、同じ型のサービスの追加は出来ませんが、例外をスローしません。
        /// </summary>
        /// <param name="service">追加するサービスプロバイダのインスタンス</param>
        /// <returns>サービスの追加が出来た場合は true を、出来なかった場合は false を返します</returns>
        public bool TryAddService(ServiceProvider service)
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
        /// 指定された型のサービスプロバイダを取得します。
        /// また、サービスプロバイダが見つけられなかった場合は例外がスローされます。
        /// </summary>
        /// <typeparam name="T">取得するサービスプロバイダの型</typeparam>
        /// <returns>見つけられたサービスプロバイダのインスタンスを返します</returns>
        /// <exception cref="ServiceNotFoundException">指定された型のサービスプロバイダが見つかりませんでした</exception>
        public T GetService<T>() where T : ServiceProvider
        {
            // サービスを探して見つけられたのなら
            var inService = FindService<T>();
            if (inService != null)
            {
                // サービスを返す
                return inService;
            }


            // ここまで来てしまったのなら例外を吐く
            throw new ServiceNotFoundException(typeof(T));
        }


        /// <summary>
        /// 指定された型のサービスプロバイダを取得します
        /// </summary>
        /// <typeparam name="T">取得するサービスプロバイダの型</typeparam>
        /// <param name="service">見つけられたサービスプロバイダのインスタンスを設定しますが、見つけられなかった場合はnullが設定されます</param>
        /// <returns>サービスを取得できた場合は true を、出来なかった場合は false を返します</returns>
        public bool TryGetService<T>(out T service) where T : ServiceProvider
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
        /// 指定された型のサービスプロバイダを削除します
        /// </summary>
        /// <typeparam name="T">削除するサービスプロバイダの型</typeparam>
        public void RemoveService<T>() where T : ServiceProvider
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


        /// <summary>
        /// サービスマネージャが保持しているすべてのサービスプロバイダを削除します
        /// </summary>
        internal void RemoveAllService()
        {
            // サービスの数分回る
            foreach (var inService in serviceList)
            {
                // サービスのシャットダウンをする
                inService.Shutdown();
            }


            // リストを空っぽにする
            serviceList.Clear();
        }


        /// <summary>
        /// 指定されたサービスプロバイダが存在するか否かを調べます
        /// </summary>
        /// <param name="service">調べるサービス</param>
        /// <returns>存在するなら true を、存在しないなら false を返します</returns>
        private bool IsExistsService(ServiceProvider service)
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
        /// 指定された型のサービスプロバイダを検索します
        /// </summary>
        /// <typeparam name="T">検索するサービスプロバイダの型</typeparam>
        /// <returns>見つけられた場合は、サービスプロバイダのインスタンスを返しますが、見つけられなかった場合はnullを返します</returns>
        private T FindService<T>() where T : ServiceProvider
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
    }
}