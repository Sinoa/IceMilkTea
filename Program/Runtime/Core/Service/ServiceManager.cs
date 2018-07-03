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
        /// <exception cref="ServiceExistsAlreadyException">既に同じ型のサービスプロバイダが追加されています</exception>
        public void AddService(ServiceProvider service)
        {
        }


        /// <summary>
        /// 指定されたサービスプロバイダの追加をします。
        /// この関数は AddService() 関数と違い、同じ型のサービスの追加は出来ませんが、例外をスローしません。
        /// </summary>
        /// <param name="service">追加するサービスプロバイダのインスタンス</param>
        /// <returns>サービスの追加が出来た場合は true を、出来なかった場合は false を返します</returns>
        public bool TryAddService(ServiceProvider service)
        {
            throw new System.NotImplementedException();
        }


        /// <summary>
        /// 指定された型のサービスプロバイダを取得します。
        /// また、サービスプロバイダが見つけられなかった場合は例外がスローされます。
        /// </summary>
        /// <typeparam name="T">取得するサービスプロバイダの型</typeparam>
        /// <returns>見つけられたサービスプロバイダのインスタンスを返します</returns>
        /// <exception cref="ServiceNotFoundException">指定された型のサービスプロバイダが見つかりませんでした</exception>
        public T GetService<T>()
        {
            throw new System.NotImplementedException();
        }


        /// <summary>
        /// 指定された型のサービスプロバイダを取得します
        /// </summary>
        /// <typeparam name="T">取得するサービスプロバイダの型</typeparam>
        /// <param name="service">見つけられたサービスプロバイダのインスタンスを設定しますが、見つけられなかった場合はnullが設定されます</param>
        /// <returns>サービスを取得できた場合は true を、出来なかった場合は false を返します</returns>
        public bool TryGetService<T>(out T service)
        {
            throw new System.NotImplementedException();
        }


        /// <summary>
        /// 指定された型のサービスプロバイダを削除します
        /// </summary>
        /// <typeparam name="T">削除するサービスプロバイダの型</typeparam>
        public void RemoveService<T>()
        {
        }


        /// <summary>
        /// サービスマネージャが保持しているすべてのサービスプロバイダを削除します
        /// </summary>
        internal void RemoveAllService()
        {
        }
        #endregion
    }
}