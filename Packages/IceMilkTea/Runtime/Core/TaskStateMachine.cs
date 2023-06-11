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
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace IceMilkTea.Core
{
    /// <summary>
    /// コンテキストを持つことが出来るタスク駆動型ステートマシンです
    /// </summary>
    /// <typeparam name="TContext">持つコンテキストの型</typeparam>
    public class TaskStateMachine<TContext>
    {
        // メンバ変数定義
        private HashSet<Func<Task>> stateHashSet;



        /// <summary>
        /// TaskStateMachine クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="context">このステートマシンがもつコンテキスト</param>
        public TaskStateMachine(TContext context)
        {
        }



        /// <summary>
        /// ステートマシンの状態を持つクラスです
        /// </summary>
        public class State
        {
            internal protected virtual Task DoProcessAsync(CancellationToken token)
            {
                return Task.CompletedTask;
            }
        }
    }
}