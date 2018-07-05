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

namespace IceMilkTea.Core
{
    /// <summary>
    /// サービスが既に存在している場合にスローされる例外クラスです
    /// </summary>
    public class GameServiceAlreadyExistsException : IceMilkTeaException
    {
        /// <summary>
        /// GameServiceAlreadyExistsException インスタンスの初期化をします
        /// </summary>
        /// <param name="serviceType">既に存在しているサービスのタイプ</param>
        /// <param name="baseType">存在しているサービスの基本となるタイプ</param>
        public GameServiceAlreadyExistsException(Type serviceType, Type baseType) : base($"'{serviceType.Name}'のサービスは既に、'{baseType.Name}'として存在しています")
        {
        }


        /// <summary>
        /// GameServiceAlreadyExistsException インスタンスの初期化をします
        /// </summary>
        /// <param name="serviceType">既に存在しているサービスのタイプ</param>
        /// <param name="baseType">存在しているサービスの基本となるタイプ</param>
        /// <param name="inner">この例外がスローされる原因となったら例外</param>
        public GameServiceAlreadyExistsException(Type serviceType, Type baseType, Exception inner) : base($"'{serviceType.Name}'のサービスは既に、'{baseType.Name}'として存在しています", inner)
        {
        }
    }
}