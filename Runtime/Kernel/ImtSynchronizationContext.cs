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
        public const int DefaultMessageQueueCapacity = 64;

        // メンバ変数定義
        private SynchronizationContext previousContext;
        private Queue<Message> messageQueue;
        private List<Exception> errorList;
        private int myStartupThreadId;



        #region コンストラクタ＆インストール・アンインストール
        /// <summary>
        /// ImtSynchronizationContext のインスタンスを初期化します。
        /// </summary>
        /// <remarks>
        /// この同期コンテキストは messagePumpHandler が呼び出されない限りメッセージを蓄え続けます。
        /// メッセージを処理するためには、必ず messagePumpHandler を定期的に呼び出してください。
        /// </remarks>
        /// <param name="messagePumpHandler">この同期コンテキストに送られてきたメッセージを処理するための、メッセージポンプハンドラを受け取ります</param>
        public ImtSynchronizationContext(out Action messagePumpHandler)
        {
            previousContext = AsyncOperationManager.SynchronizationContext;
            messageQueue = new Queue<Message>(DefaultMessageQueueCapacity);
            errorList = new List<Exception>(DefaultMessageQueueCapacity);
            myStartupThreadId = Thread.CurrentThread.ManagedThreadId;
            messagePumpHandler = DoProcessMessage;
        }


        /// <summary>
        /// ImtSynchronizationContext のインスタンスを生成と同時に、同期コンテキストの設定も行います。
        /// </summary>
        /// <param name="messagePumpHandler">コンストラクタの messagePumpHandler に渡す参照</param>
        /// <returns>インスタンスの生成と設定が終わった、同期コンテキストを返します。</returns>
        public static ImtSynchronizationContext Install(out Action messagePumpHandler)
        {
            // 新しい同期コンテキストのインスタンスを生成して、設定した後に返す
            var context = new ImtSynchronizationContext(out messagePumpHandler);
            AsyncOperationManager.SynchronizationContext = context;
            return context;
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
            // 同じスレッドからの送信なら
            if (Thread.CurrentThread.ManagedThreadId == myStartupThreadId)
            {
                // 直ちにコールバックを叩いて終了
                callback(state);
                return;
            }


            // メッセージ処理待ち用同期プリミティブを用意
            using (var waitHandle = new ManualResetEvent(false))
            {
                // メッセージキューをロック
                lock (messageQueue)
                {
                    // 処理して欲しいコールバックを登録
                    messageQueue.Enqueue(new Message(callback, state, waitHandle));
                }


                // 登録したコールバックが処理されるまで待機
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
            // メッセージキューをロック
            lock (messageQueue)
            {
                // 処理して欲しいコールバックを登録
                messageQueue.Enqueue(new Message(callback, state, null));
            }
        }
        #endregion


        #region メッセージ処理関数群
        /// <summary>
        /// 同期コンテキストに、送られてきたメッセージを処理します。
        /// </summary>
        /// <exception cref="ObjectDisposedException">既にオブジェクトが解放済みです</exception>
        private void DoProcessMessage()
        {
            // エラーリストをクリアする
            errorList.Clear();


            // メッセージキューをロック
            lock (messageQueue)
            {
                // メッセージ処理中にポストされても次回になるよう、今回処理するべきメッセージ件数の取得
                var processCount = messageQueue.Count;


                // 今回処理するべきメッセージの件数分だけループ
                for (int i = 0; i < processCount; ++i)
                {
                    try
                    {
                        // メッセージを呼ぶ
                        messageQueue.Dequeue().Invoke();
                    }
                    catch (Exception exception)
                    {
                        // エラーが発生したらエラーリストに詰める
                        errorList.Add(exception);
                    }
                }
            }


            // エラーリストに要素が1つでも存在したら
            if (errorList.Count > 0)
            {
                // エラーリストの内容全てを包んでまとめて例外を投げる
                throw new AggregateException($"メッセージ処理中に {errorList.Count} 件のエラーが発生しました", errorList.ToArray());
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