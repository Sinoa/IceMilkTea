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
            serviceList = new List<ServiceProvider>();
        }


        internal void Update()
        {
            throw new System.NotImplementedException();
        }


        public T AddService<T>()
        {
            throw new System.NotImplementedException();
        }


        public T GetService<T>()
        {
            throw new System.NotImplementedException();
        }


        public void RemoveService<T>()
        {
            throw new System.NotImplementedException();
        }
    }
}