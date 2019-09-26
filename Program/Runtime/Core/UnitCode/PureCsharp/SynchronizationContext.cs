// zlib/libpng License
//
// Copyright (c) 2018 - 2019 Sinoa
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
    public class ImtSynchronizationContext : SynchronizationContext, IDisposable
    {
        /// <summary>
        /// 同期コンテキストに送られてきたコールバックを、メッセージとして保持する構造体です。
        /// </summary>
        private struct Message
        {
            // メンバ変数定義
            private SendOrPostCallback callback;
            private ManualResetEvent waitHandle;
            private object state;



            /// <summary>
            /// Message のインスタンスを初期化します。
            /// </summary>
            /// <param name="callback">呼び出すべきコールバック関数</param>
            /// <param name="state">コールバックに渡すオブジェクト</param>
            /// <param name="waitHandle">コールバックの呼び出しを待機するために、利用する待機ハンドル</param>
            public Message(SendOrPostCallback callback, object state, ManualResetEvent waitHandle)
            {
                // メンバの初期化
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
                    // コールバックを叩く
                    callback(state);
                }
                finally
                {
                    // もし待機ハンドルがあるなら
                    if (waitHandle != null)
                    {
                        // シグナルを設定する
                        waitHandle.Set();
                    }
                }
            }


            /// <summary>
            /// このメッセージを管理していた同期コンテキストが、何かの理由で管理できなくなった場合
            /// このメッセージを指定された同期コンテキストに、再ポストします。
            /// また、送信メッセージの場合は、直ちに処理され待機ハンドルのシグナルが設定されます。
            /// </summary>
            /// <param name="rePostTargetContext">再ポスト先の同期コンテキスト</param>
            public void Failover(SynchronizationContext rePostTargetContext)
            {
                // 待機ハンドルが存在するなら
                if (waitHandle != null)
                {
                    // コールバックを叩いてシグナルを設定する
                    callback(state);
                    waitHandle.Set();
                    return;
                }


                // 再ポスト先同期コンテキストにポストする
                rePostTargetContext.Post(callback, state);
            }
        }



        // 定数定義
        public const int DefaultMessageQueueCapacity = 32;

        // メンバ変数定義
        private SynchronizationContext previousContext;
        private Queue<Message> messageQueue;
        private List<Exception> errorList;
        private int myStartupThreadId;
        private bool disposed;



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
            // メンバの初期化と、メッセージ処理関数を伝える
            previousContext = AsyncOperationManager.SynchronizationContext;
            messageQueue = new Queue<Message>(DefaultMessageQueueCapacity);
            errorList = new List<Exception>(DefaultMessageQueueCapacity);
            myStartupThreadId = Thread.CurrentThread.ManagedThreadId;
            messagePumpHandler = DoProcessMessage;
        }


        /// <summary>
        /// ImtSynchronizationContext のファイナライザです。
        /// </summary>
        ~ImtSynchronizationContext()
        {
            // ファイナライザからのDispose呼び出し
            Dispose(false);
        }


        /// <summary>
        /// リソースを解放します。また、解放する際にメッセージが残っていた場合は
        /// この同期コンテキストが生成される前に存在していた、同期コンテキストに再ポストされ、同期コンテキストが再設定されます。
        /// </summary>
        public void Dispose()
        {
            // DisposeからのDispose呼び出し
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        /// <summary>
        /// 実際のリソース解放を行います。
        /// </summary>
        /// <param name="disposing">マネージ解放の場合は true を、アンマネージ解放なら false を指定</param>
        protected virtual void Dispose(bool disposing)
        {
            // 既に解放済みなら
            if (disposed)
            {
                // 終了
                return;
            }


            // もし現在の同期コンテキストが自身なら
            if (AsyncOperationManager.SynchronizationContext == this)
            {
                // 同期コンテキストを、インスタンス生成時に覚えたコンテキストに戻す
                AsyncOperationManager.SynchronizationContext = previousContext;
            }


            // メッセージキューをロック
            lock (messageQueue)
            {
                // 全てのメッセージを処理するまでループ
                while (messageQueue.Count > 0)
                {
                    // 一つ前の同期コンテキストにフェイルオーバーする
                    messageQueue.Dequeue().Failover(previousContext);
                }
            }


            // 解放済みマーク
            disposed = true;
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


        /// <summary>
        /// 同期メッセージを送信します。
        /// </summary>
        /// <param name="callback">呼び出すべきメッセージのコールバック</param>
        /// <param name="state">コールバックに渡してほしいオブジェクト</param>
        /// <exception cref="ObjectDisposedException">既にオブジェクトが解放済みです</exception>
        public override void Send(SendOrPostCallback callback, object state)
        {
            // 解放済み例外送出関数を叩く
            ThrowIfDisposed();


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
            // 解放済み例外送出関数を叩く
            ThrowIfDisposed();


            // メッセージキューをロック
            lock (messageQueue)
            {
                // 処理して欲しいコールバックを登録
                messageQueue.Enqueue(new Message(callback, state, null));
            }
        }


        /// <summary>
        /// 同期コンテキストに、送られてきたメッセージを処理します。
        /// </summary>
        /// <exception cref="ObjectDisposedException">既にオブジェクトが解放済みです</exception>
        private void DoProcessMessage()
        {
            // 解放済み例外送出関数を叩く
            ThrowIfDisposed();


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


        /// <summary>
        /// 解放済みの場合に、例外を送出します。
        /// </summary>
        /// <exception cref="ObjectDisposedException">既にオブジェクトが解放済みです</exception>
        private void ThrowIfDisposed()
        {
            // 解放済みなら
            if (disposed)
            {
                // 解放済み例外を投げる
                throw new ObjectDisposedException(null);
            }
        }
    }
}