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
using System.Runtime.ExceptionServices;
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
        private readonly IImtSynchronizationContextEventHandler eventHandler;
        private readonly MessageQueueController messageQueue;
        private readonly List<Exception> errorList;
        private readonly int myStartupThreadId;



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
        private ImtSynchronizationContext(out Action messagePumpHandler, IImtSynchronizationContextEventHandler handler)
        {
            previousContext = AsyncOperationManager.SynchronizationContext;
            messageQueue = new MessageQueueController(DefaultMessageQueueCapacity, ProcessMessage);
            errorList = new List<Exception>(DefaultMessageQueueCapacity);
            myStartupThreadId = Thread.CurrentThread.ManagedThreadId;
            eventHandler = handler ?? throw new ArgumentNullException(nameof(handler));
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
        public static bool Install(out Action messagePumpHandler)
        {
            return Install(out messagePumpHandler, new ImtNullSynchronizationContextEventHandler());
        }


        /// <summary>
        /// 現在のスレッドの同期コンテキストに ImtSynchronizationContext 同期コンテキストをインストールします。
        /// </summary>
        /// <remarks>
        /// 既に ImtSynchronizationContext 同期コンテキストがインストール済みの場合はインストールに失敗を返しますが、メッセージポンプハンドラは取得されます。
        /// </remarks>
        /// <param name="messagePumpHandler">この同期コンテキストに送られてきたメッセージを処理するための、メッセージポンプハンドラを受け取ります</param>
        /// <param name="handler">インストールした ImtSynchronizationContext 同期コンテキストのイベントを処理するハンドラオブジェクト。ただし、インストールに失敗した場合は設定されません。</param>
        /// <returns>正しくインストール出来た場合は true を、既にインストール済みによってインストールが出来なかった場合は false を返します。</returns>
        /// <exception cref="ArgumentNullException">handler が null です</exception>
        public static bool Install(out Action messagePumpHandler, IImtSynchronizationContextEventHandler handler)
        {
            if (AsyncOperationManager.SynchronizationContext is ImtSynchronizationContext context)
            {
                messagePumpHandler = context.DoProcessMessage;
                return false;
            }


            context = new ImtSynchronizationContext(out messagePumpHandler, handler);
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


        #region メッセージ送信関数群
        /// <summary>
        /// 同期メッセージを送信します。
        /// </summary>
        /// <param name="callback">呼び出すべきメッセージのコールバック</param>
        /// <param name="state">コールバックに渡してほしいオブジェクト</param>
        /// <exception cref="ObjectDisposedException">既にオブジェクトが解放済みです</exception>
        public override void Send(SendOrPostCallback callback, object state)
        {
            if (Thread.CurrentThread.ManagedThreadId == myStartupThreadId)
            {
                callback(state);
                return;
            }


            using (var waitHandle = new ManualResetEvent(false))
            {
                messageQueue.Enqueue(new Message(callback, state, waitHandle));
                waitHandle.WaitOne();
            }
        }


        /// <summary>
        /// 非同期メッセージをポストします。
        /// </summary>
        /// <param name="callback">呼び出すべきメッセージのコールバック</param>
        /// <param name="state">コールバックに渡してほしいオブジェクト</param>
        /// <exception cref="ObjectDisposedException">既にオブジェクトが解放済みです</exception>
        public override void Post(SendOrPostCallback callback, object state)
        {
            messageQueue.Enqueue(new Message(callback, state, null));
        }
        #endregion


        #region メッセージ処理関数群
        private void DoProcessMessage()
        {
            errorList.Clear();
            messageQueue.ProcessFrontMessage(errorList);


            if (errorList.Count > 0)
            {
                var exception = new AggregateException($"メッセージ処理中に {errorList.Count} 件のエラーが発生しました", errorList);
                eventHandler.DoErrorHandle(exception, errorList.Count);
            }
        }


        private void ProcessMessage(Message message, object state)
        {
            try
            {
                message.Invoke();
            }
            catch (Exception exception)
            {
                ((List<Exception>)state).Add(exception);
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



        #region 同期コンテキストのメッセージキュー制御クラス定義
        /// <summary>
        /// メッセージキューを効率よく処理するためのキュー制御を提供します
        /// </summary>
        private sealed class MessageQueueController
        {
            // メンバ変数定義
            private readonly object syncObject;
            private readonly Action<Message, object> callback;
            private Queue<Message> backQueue;
            private Queue<Message> frontQueue;



            /// <summary>
            /// MessageQueueController クラスのインスタンスを初期化します
            /// </summary>
            /// <param name="bufferSize">メッセージキューバッファサイズ。内部ではダブルバッファで持つため指定されたサイズの倍のメモリ確保が行われることに注意してください。</param>
            /// <param name="callback">メッセージを処理するコールバック関数</param>
            /// <exception cref="ArgumentNullException">callback が null です</exception>
            public MessageQueueController(int bufferSize, Action<Message, object> callback)
            {
                syncObject = new object();
                backQueue = new Queue<Message>(bufferSize);
                frontQueue = new Queue<Message>(bufferSize);
                this.callback = callback ?? throw new ArgumentNullException(nameof(callback));
            }


            /// <summary>
            /// 処理するべきメッセージをバックバッファに追加します
            /// </summary>
            /// <param name="message">追加するメッセージ</param>
            public void Enqueue(in Message message)
            {
                lock (syncObject)
                {
                    backQueue.Enqueue(message);
                }
            }


            /// <summary>
            /// 内部のバッファをローテーションしフロントバッファのメッセージを処理します
            /// </summary>
            /// <param name="state">コールバック関数に渡す状態オブジェクト</param>
            public void ProcessFrontMessage(object state)
            {
                lock (syncObject)
                {
                    var tmp = frontQueue;
                    frontQueue = backQueue;
                    backQueue = tmp;
                }


                while (frontQueue.Count > 0)
                {
                    callback(frontQueue.Dequeue(), state);
                }
            }
        }
        #endregion
    }



    /// <summary>
    /// ImtSynchronizationContext クラスの内部イベントをハンドリングするためのインターフェイスです
    /// </summary>
    public interface IImtSynchronizationContextEventHandler
    {
        /// <summary>
        /// 同期コンテキストによって処理されたメッセージのいずれかがエラーを発生した場合の処理をします
        /// </summary>
        /// <param name="exception">発生したエラーをまとめた例外</param>
        /// <param name="count">発生したエラー件数</param>
        void DoErrorHandle(AggregateException exception, int count);
    }



    /// <summary>
    /// ImtSynchronizationContext クラスの標準イベントハンドラを提供します
    /// </summary>
    internal sealed class ImtStandardSynchronizationContextEventHandler : IImtSynchronizationContextEventHandler
    {
        /// <summary>
        /// 同期コンテキストで発生した例外を再スローします
        /// </summary>
        /// <param name="exception">発生したエラーをまとめた例外</param>
        /// <param name="count">発生したエラー件数</param>
        public void DoErrorHandle(AggregateException exception, int count)
        {
            ExceptionDispatchInfo.Capture(exception).Throw();
        }
    }



    internal sealed class ImtNullSynchronizationContextEventHandler : IImtSynchronizationContextEventHandler
    {
        public void DoErrorHandle(AggregateException exception, int count)
        {
        }
    }
}