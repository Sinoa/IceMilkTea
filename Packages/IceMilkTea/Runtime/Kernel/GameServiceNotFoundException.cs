﻿// zlib/libpng License
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
    /// サービスが見つからなかった場合にスローされる例外クラスです
    /// </summary>
    public class GameServiceNotFoundException : ImtException
    {
        /// <summary>
        /// GameServiceNotFoundException インスタンスの初期化をします
        /// </summary>
        /// <param name="serviceType">見つけられなかったサービスのタイプ</param>
        public GameServiceNotFoundException(Type serviceType) : base($"'{serviceType.Name}'のサービスを見つけられませんでした")
        {
        }


        /// <summary>
        /// GameServiceNotFoundException インスタンスの初期化をします
        /// </summary>
        /// <param name="serviceType">見つけられなかったサービスのタイプ</param>
        /// <param name="inner">この例外がスローされる原因となった例外</param>
        public GameServiceNotFoundException(Type serviceType, Exception inner) : base($"'{serviceType.Name}'のサービスを見つけられませんでした", inner)
        {
        }
    }
}