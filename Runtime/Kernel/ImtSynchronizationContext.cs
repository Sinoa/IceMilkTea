// zlib/libpng License
//
// Copyright (C) 2018 Sinoa
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
using System.ComponentModel;
using System.Threading;

namespace IceMilkTea.Core
{
    /// <summary>
    /// IceMilkTea 自身が提供する同期コンテキストクラスです。
    /// 独立したスレッドの同期コンテキストとして利用したり、特定コード範囲の同期コンテキストとして利用出来ます。
    /// </summary>
    internal sealed class ImtSynchronizationContext : SynchronizationContext
    {
        // 定数定義
        public const int DefaultMessageQueueCapacity = 128;

        // メンバ変数定義
        private readonly SynchronizationContext previousContext;
        private readonly Queue<Message> waitQueue;
        private readonly Queue<Message> processQueue;
        private readonly int myThreadId;



        #region コンストラクタ＆インストール・アンインストール
        /// <summary>
        /// ImtSynchronizationContext のインスタンスを初期化します。
        /// </summary>
        /// <remarks>
        /// この同期コンテキストは messagePumpHandler が呼び出されない限りメッセージを蓄え続けます。
        /// メッセージを処理するためには、必ず messagePumpHandler を定期的に呼び出してください。
        /// </remarks>
        /// <param name="messagePumpHandler">この同期コンテキストに送られてきたメッセージを処理するための、メッセージポンプハンドラを受け取ります</param>
        /// <param name="handler">この同期コンテキストによって発生したイベントをハンドリングするオブジェクト</param>
        /// <exception cref="ArgumentNullException">handler が null です</exception>
        private ImtSynchronizationContext(out Action messagePumpHandler)
        {
            previousContext = AsyncOperationManager.SynchronizationContext;
            waitQueue = new Queue<Message>();
            processQueue = new Queue<Message>();
            myThreadId = Thread.CurrentThread.ManagedThreadId;
            messagePumpHandler = DoProcessMessage;
        }


        /// <summary>
        /// 現在のスレッドの同期コンテキストに ImtSynchronizationContext 同期コンテキストをインストールします。
        /// </summary>
        /// <remarks>
        /// 既に ImtSynchronizationContext 同期コンテキストがインストール済みの場合はインストールに失敗を返しますが、メッセージポンプハンドラは取得されます。
        /// </remarks>
        /// <param name="messagePumpHandler">この同期コンテキストに送られてきたメッセージを処理するための、メッセージポンプハンドラを受け取ります</param>
        /// <returns>正しくインストール出来た場合は true を、既にインストール済みによってインストールが出来なかった場合は false を返します。</returns>
        /// <exception cref="ArgumentNullException">handler が null です</exception>
        public static bool Install(out Action messagePumpHandler)
        {
            if (AsyncOperationManager.SynchronizationContext is ImtSynchronizationContext context)
            {
                messagePumpHandler = context.DoProcessMessage;
                return false;
            }


            context = new ImtSynchronizationContext(out messagePumpHandler);
            messagePumpHandler = context.DoProcessMessage;
            AsyncOperationManager.SynchronizationContext = context;
            return true;
        }


        /// <summary>
        /// 現在のスレッドの同期コンテキストに ImtSynchronizationContext 同期コンテキストがインストールされている場合はアンインストールします。
        /// </summary>
        /// <remarks>
        /// アンインストールが行われた場合は、インストール時に設定されていた同期コンテキストに戻します。
        /// </remarks>
        public static void Uninstall()
        {
            if (!(AsyncOperationManager.SynchronizationContext is ImtSynchronizationContext context))
            {
                return;
            }


            AsyncOperationManager.SynchronizationContext = context.previousContext;
        }
        #endregion


        #region メッセージ処理関数群
        public override void Send(SendOrPostCallback callback, object state)
        {
            if (Thread.CurrentThread.ManagedThreadId == myThreadId)
            {
                callback(state);
                return;
            }


            using (var waitHandle = new ManualResetEvent(false))
            {
                EnqueueMessage(new Message(callback, state, waitHandle));
                waitHandle.WaitOne();
            }
        }


        public override void Post(SendOrPostCallback callback, object state)
        {
            EnqueueMessage(new Message(callback, state, null));
        }


        private void EnqueueMessage(in Message message)
        {
            lock (waitQueue)
            {
                waitQueue.Enqueue(message);
            }
        }


        private void AcceptMessage()
        {
            lock (waitQueue)
            {
                while (waitQueue.Count > 0)
                {
                    processQueue.Enqueue(waitQueue.Dequeue());
                }
            }
        }


        private void DoProcessMessage()
        {
            AcceptMessage();
            while (processQueue.Count > 0)
            {
                processQueue.Dequeue().Invoke();
            }
        }
        #endregion



        #region 同期コンテキストのメッセージ型定義
        /// <summary>
        /// 同期コンテキストに送られてきたコールバックを、メッセージとして保持する構造体です。
        /// </summary>
        private readonly struct Message
        {
            // メンバ変数定義
            private readonly SendOrPostCallback callback;
            private readonly ManualResetEvent waitHandle;
            private readonly object state;



            /// <summary>
            /// Message のインスタンスを初期化します。
            /// </summary>
            /// <param name="callback">呼び出すべきコールバック関数</param>
            /// <param name="state">コールバックに渡すオブジェクト</param>
            /// <param name="waitHandle">コールバックの呼び出しを待機するために、利用する待機ハンドル</param>
            public Message(SendOrPostCallback callback, object state, ManualResetEvent waitHandle)
            {
                this.callback = callback;
                this.waitHandle = waitHandle;
                this.state = state;
            }


            /// <summary>
            /// メッセージに設定されたコールバックを呼び出します。
            /// また、待機ハンドルが設定されている場合は、待機ハンドルのシグナルを設定します。
            /// </summary>
            public void Invoke()
            {
                try
                {
                    callback(state);
                }
                finally
                {
                    waitHandle?.Set();
                }
            }
        }
        #endregion
    }
}