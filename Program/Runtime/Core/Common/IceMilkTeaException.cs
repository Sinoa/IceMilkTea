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
    /// IceMilkTea側の例外基本クラスです。
    /// IceMilkTeaの例外を実装する場合は、このクラスを継承してください。
    /// </summary>
    public class IceMilkTeaException : Exception
    {
        /// <summary>
        /// パラメータを特に必要としない他純な例外コンストラクタです
        /// </summary>
        public IceMilkTeaException()
        {
        }


        /// <summary>
        /// 例外メッセージを受けるための例外コンストラクタです
        /// </summary>
        /// <param name="message">例外メッセージ</param>
        public IceMilkTeaException(string message) : base(message)
        {
        }


        /// <summary>
        /// この例外を発生させる理由になった内部例外付きメッセージの例外コンストラクタです
        /// </summary>
        /// <param name="message">例外メッセージ</param>
        /// <param name="inner">この例外を発生させる原因となった内部例外</param>
        public IceMilkTeaException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}