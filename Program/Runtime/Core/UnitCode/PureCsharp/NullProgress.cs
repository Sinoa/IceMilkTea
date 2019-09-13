// zlib/libpng License
//
// Copyright (c) 2019 Sinoa
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
    /// すべての進捗通知を無視する進捗オブジェクトです
    /// </summary>
    /// <typeparam name="T">進捗通知する内容の型</typeparam>
    public class NullProgress<T> : IProgress<T>
    {
        /// <summary>
        /// 進捗通知を処理しない空の進捗オブジェクトです
        /// </summary>
        public static NullProgress<T> Null { get; private set; } = new NullProgress<T>();



        /// <summary>
        /// 何もしません
        /// </summary>
        /// <param name="value">通知の内容</param>
        public void Report(T value)
        {
        }
    }
}