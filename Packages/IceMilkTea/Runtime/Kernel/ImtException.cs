// zlib/libpng License
//
// Copyright (c) 2020 Sinoa
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
using System.Runtime.Serialization;

namespace IceMilkTea.Core
{
    /// <summary>
    /// IceMilkTea が提供する IceMilkTea 内の独自定義例外クラスの既定となる例外クラスです
    /// </summary>
    public class ImtException : Exception
    {
        /// <summary>
        /// ImtException クラスのインスタンスを初期化します
        /// </summary>
        public ImtException()
        {
        }


        /// <summary>
        /// ImtException クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="message">例外のメッセージ</param>
        public ImtException(string message) : base(message)
        {
        }


        /// <summary>
        /// ImtException クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="message">例外のメッセージ</param>
        /// <param name="innerException">この例外を発生させる原因となった例外</param>
        public ImtException(string message, Exception innerException) : base(message, innerException)
        {
        }


        /// <summary>
        /// ImtException クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="info">シリアル下されたオブジェクトの情報を保持するオブジェクト</param>
        /// <param name="context">コンテキスト情報</param>
        protected ImtException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
