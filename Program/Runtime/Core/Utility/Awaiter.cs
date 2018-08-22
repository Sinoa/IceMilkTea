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
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;

namespace IceMilkTea.Core
{
    #region Awaitableインターフェイス
    /// <summary>
    /// 値を返さない、待機可能なオブジェクトが実装する、インターフェイスを定義しています。
    /// </summary>
    public interface IAwaitable
    {
        /// <summary>
        /// タスクが完了している場合は true を、完了していない場合は false を取り出します。
        /// </summary>
        bool IsCompleted { get; }



        /// <summary>
        /// 待機をするための、汎用待機オブジェクト ImtAwaiter を取得します。
        /// </summary>
        /// <returns>汎用待機オブジェクト ImtAwaiter のインスタンスを返します</returns>
        ImtAwaiter GetAwaiter();


        /// <summary>
        /// Awaiter が待機を完了した時に継続動作するための、継続関数を登録します。
        /// </summary>
        /// <param name="continuation">登録する継続関数</param>
        void RegisterContinuation(Action continuation);
    }



    /// <summary>
    /// 値を返す、待機可能なオブジェクトが実装する、インターフェイスを定義しています。
    /// </summary>
    /// <typeparam name="TResult">待機可能オブジェクトが返す値の型</typeparam>
    public interface IAwaitable<TResult>
    {
        /// <summary>
        /// タスクが完了している場合は true を、完了していない場合は false を取り出します。
        /// </summary>
        bool IsCompleted { get; }



        /// <summary>
        /// 待機をするための、汎用待機オブジェクト ImtAwaiter<typeparamref name="TResult"/> を取得します。
        /// </summary>
        /// <returns>汎用待機オブジェクト ImtAwaiter<typeparamref name="TResult"/> のインスタンスを返します</returns>
        ImtAwaiter<TResult> GetAwaiter();


        /// <summary>
        /// Awaiter が待機を完了した時に継続動作するための、継続関数を登録します。
        /// </summary>
        /// <param name="continuation">登録する継続関数</param>
        void RegisterContinuation(Action continuation);


        /// <summary>
        /// 待機した結果を取得します。
        /// </summary>
        /// <returns>継続動作時に取得される結果を返します</returns>
        TResult GetResult();
    }
    #endregion



    #region AwaitableWaitHandle
    /// <summary>
    /// シグナル操作をして待機状態をコントロールすることの出来る、待機可能な抽象クラスです。
    /// </summary>
    /// <remarks>
    /// 単純なシグナル操作による、待機制御を実現する場合には有用です。
    /// </remarks>
    public abstract class ImtAwaitableWaitHandle : IAwaitable, IDisposable
    {
        // メンバ変数定義
        protected AwaiterContinuationHandler awaiterHandler;
        private bool disposed;



        /// <summary>
        /// シグナル状態を表します。
        /// true がシグナル状態 false が非シグナル状態です。
        /// </summary>
        public bool IsCompleted { get; protected set; }



        /// <summary>
        /// ImtAwaitableWaitHandle のインスタンスを初期化します
        /// </summary>
        /// <param name="initialSignal">初期のシグナル状態</param>
        public ImtAwaitableWaitHandle(bool initialSignal)
        {
            // 待機オブジェクトハンドラの生成とシグナル状態を初期化
            awaiterHandler = new AwaiterContinuationHandler();
            IsCompleted = initialSignal;
        }


        /// <summary>
        /// ImtAwaitableWaitHandle のデストラクタです
        /// </summary>
        ~ImtAwaitableWaitHandle()
        {
            // Disposeを叩く
            Dispose();
        }


        /// <summary>
        /// すべての待機オブジェクトにシグナルを送信して
        /// いつでも破棄されるようにします。
        /// </summary>
        public void Dispose()
        {
            // 既に解放済みなら
            if (disposed)
            {
                // なにもしない
                return;
            }


            // すべての待機オブジェクトを処理して、解放済みマークを付ける
            awaiterHandler.SetSignal();
            disposed = true;


            // ファイナライザを呼ばないようにしてもらう
            GC.SuppressFinalize(this);
        }


        /// <summary>
        /// このオブジェクトの待機オブジェクトを取得します
        /// </summary>
        /// <exception cref="ObjectDisposedException">待機ハンドルは解放済みです</exception>
        /// <returns>待機オブジェクトを返します</returns>
        public ImtAwaiter GetAwaiter()
        {
            // 解放済み例外の処理をしておく
            ThrowIfDisposed();


            // 単純なAwaiterを返す
            return new ImtAwaiter(this);
        }


        /// <summary>
        /// 継続関数を登録します
        /// </summary>
        /// <param name="continuation">登録する継続関数</param>
        /// <exception cref="ObjectDisposedException">待機ハンドルは解放済みです</exception>
        public virtual void RegisterContinuation(Action continuation)
        {
            // 解放済み例外の処理をしておく
            ThrowIfDisposed();


            // 継続関数を登録する
            awaiterHandler.RegisterContinuation(continuation);
        }


        /// <summary>
        /// この待機ハンドルのシグナルを設定して。
        /// 待機状態を解除します。
        /// </summary>
        public abstract void SetSignal();


        /// <summary>
        /// この大気ハンドルのシグナルを解除して。
        /// オブジェクトが待機状態になるようにします。
        /// </summary>
        public abstract void ResetSignal();


        /// <summary>
        /// 解放済みの場合、解放済みの例外を送出します。
        /// </summary>
        /// <exception cref="ObjectDisposedException">待機ハンドルは解放済みです</exception>
        protected void ThrowIfDisposed()
        {
            // 解放済みなら
            if (disposed)
            {
                // 解放済み例外を投げる
                throw new ObjectDisposedException("待機ハンドルは解放済みです");
            }
        }
    }



    /// <summary>
    /// シグナル状態をマニュアルコントロールする待機可能な、待機ハンドラクラスです
    /// </summary>
    public class ImtAwaitableManualReset : ImtAwaitableWaitHandle
    {
        /// <summary>
        /// ImtAwaitableManualReset のインスタンスを初期化します
        /// </summary>
        /// <param name="initialSignal">初期のシグナル状態</param>
        public ImtAwaitableManualReset(bool initialSignal) : base(initialSignal)
        {
        }


        /// <summary>
        /// 待機ハンドラをシグナル状態にして、待機オブジェクトの待機を解除します。
        /// また、 ResetSignal() を呼び出さない限り、ずっと待機されない状態になります。
        /// 再び、待機状態にさせるには ResetSignal() を呼び出して下さい。
        /// </summary>
        /// <exception cref="ObjectDisposedException">待機ハンドルは解放済みです</exception>
        /// <see cref="ResetSignal"/>
        public override void SetSignal()
        {
            // 解放済み例外の処理をしておく
            ThrowIfDisposed();


            // シグナル状態を設定して、継続関数を呼び出す
            IsCompleted = true;
            awaiterHandler.SetSignal();
        }


        /// <summary>
        /// 待機ハンドラを非シグナル状態にして、待機オブジェクトに待機してもらうようにします。
        /// </summary>
        /// <exception cref="ObjectDisposedException">待機ハンドルは解放済みです</exception>
        public override void ResetSignal()
        {
            // 解放済み例外の処理をしておく
            ThrowIfDisposed();


            // 非シグナル状態にする
            IsCompleted = false;
        }
    }



    /// <summary>
    /// シグナル状態を自動コントロールする待機可能な、待機ハンドラクラスです
    /// </summary>
    public class ImtAwaitableAutoReset : ImtAwaitableWaitHandle
    {
        /// <summary>
        /// ImtAwaitableAutoReset のインスタンスを初期化します
        /// </summary>
        public ImtAwaitableAutoReset() : base(false)
        {
        }


        /// <summary>
        /// 待機ハンドラをシグナル状態にして、最初に待機した１つの待機オブジェクトの待機を解除します。
        /// また、待機オブジェクトの待機が解除された直後に、直ちに非シグナル状態になるため
        /// すべての待機オブジェクトの待機を解除するためには、再び SetSignal() を呼び出す必要があります。
        /// </summary>
        /// <exception cref="ObjectDisposedException">待機ハンドルは解放済みです</exception>
        public override void SetSignal()
        {
            // 解放済み例外の処理をしておく
            ThrowIfDisposed();


            // シグナル状態を設定して、継続関数を１つだけ呼び出した後、直ちに非シグナル状態にする
            IsCompleted = true;
            awaiterHandler.SetOneShotSignal();
            ResetSignal();
        }


        /// <summary>
        /// 待機ハンドラを非シグナル状態にして、待機オブジェクトに待機してもらうようにします。
        /// </summary>
        /// <exception cref="ObjectDisposedException">待機ハンドルは解放済みです</exception>
        public override void ResetSignal()
        {
            // 解放済み例外の処理をしておく
            ThrowIfDisposed();


            // 非シグナル状態にする
            IsCompleted = false;
        }
    }
    #endregion



    #region Awaiter構造体
    /// <summary>
    /// 値を返さない、汎用的な待機構造体です。
    /// </summary>
    public struct ImtAwaiter : INotifyCompletion
    {
        // メンバ変数定義
        private IAwaitable awaitableContext;



        /// <summary>
        /// IAwaitable.IsCompleted の値を取り出します
        /// </summary>
        public bool IsCompleted => awaitableContext.IsCompleted;



        /// <summary>
        /// ImtAwaiter のインスタンスを初期化します
        /// </summary>
        /// <param name="context">この待機オブジェクトを保持する IAwaitable</param>
        public ImtAwaiter(IAwaitable context)
        {
            // 保持する担当を覚える
            awaitableContext = context;
        }


        /// <summary>
        /// タスクが完了した時のハンドリングを行います。
        /// </summary>
        /// <param name="continuation">タスクを継続動作させるための継続関数</param>
        public void OnCompleted(Action continuation)
        {
            // 既にタスクが完了しているのなら
            if (IsCompleted)
            {
                // 直ちに継続関数を叩いて終了
                continuation();
                return;
            }


            // 継続関数を登録する
            awaitableContext.RegisterContinuation(continuation);
        }


        /// <summary>
        /// タスクの結果を取得しますが、この構造体は常に結果は操作しません。
        /// </summary>
        public void GetResult()
        {
            // No handling...
        }
    }



    /// <summary>
    /// 値を返す、汎用的な待機構造体です。
    /// </summary>
    /// <typeparam name="TResult">待機可能オブジェクトが返す値の型</typeparam>
    public struct ImtAwaiter<TResult> : INotifyCompletion
    {
        // メンバ変数定義
        private IAwaitable<TResult> awaitableContext;



        /// <summary>
        /// IAwaitable<typeparamref name="TResult"/>.IsCompleted の値を取り出します
        /// </summary>
        public bool IsCompleted => awaitableContext.IsCompleted;



        /// <summary>
        /// ImtAwaiter のインスタンスを初期化します
        /// </summary>
        /// <param name="context">この待機オブジェクトを保持する IAwaitable<typeparamref name="TResult"/></param>
        public ImtAwaiter(IAwaitable<TResult> context)
        {
            // 保持する担当を覚える
            awaitableContext = context;
        }


        /// <summary>
        /// タスクが完了した時のハンドリングを行います。
        /// </summary>
        /// <param name="continuation">タスクを継続動作させるための継続関数</param>
        public void OnCompleted(Action continuation)
        {
            // 既にタスクが完了しているのなら
            if (IsCompleted)
            {
                // 直ちに継続関数を叩いて終了
                continuation();
                return;
            }


            // 継続関数を登録する
            awaitableContext.RegisterContinuation(continuation);
        }


        /// <summary>
        /// タスクの結果を取得します。
        /// </summary>
        /// <returns>IAwaitable<typeparamref name="TResult"/>.GetResult() の結果を返します</returns>
        public TResult GetResult()
        {
            // 待機結果を取得して返す
            return awaitableContext.GetResult();
        }
    }
    #endregion



    #region EventToAwaiter構造体
    /// <summary>
    /// イベント実装のコードを、待機可能なコードに変換する構造体です
    /// </summary>
    /// <remarks>
    /// この構造体は、メモリのAllocコストがそこそこあるので、適所を見極めて使うようにして下さい。
    /// イベントの発生頻度が低く、メモリAllocがあまり気にならないタイミングなどでは、強力に発揮します。
    /// </remarks>
    /// <typeparam name="TEventDelegate">イベントのデリゲートのシグネチャ</typeparam>
    public struct ImtAwaiterFromEvent<TEventDelegate> : INotifyCompletion
    {
        // 構造体変数宣言
        private static readonly SendOrPostCallback cache = new SendOrPostCallback(_ => ((Action)_)());



        // メンバ変数定義
        private Func<bool> isCompleted;
        private Action<TEventDelegate> register;
        private Action<TEventDelegate> unregister;
        private Func<Action, TEventDelegate> eventFrom;
        private TEventDelegate eventHandler;
        private SynchronizationContext context;
        private Action continuation;



        /// <summary>
        /// タスクが完了したかどうか
        /// </summary>
        public bool IsCompleted => isCompleted();



        /// <summary>
        /// ImtAwaiterFromEvent のインスタンスを初期化します
        /// </summary>
        /// <param name="completed">待機オブジェクトが、タスクの完了を扱うための関数</param>
        /// <param name="convert">待機オブジェクト内部の継続関数を、イベントハンドラから呼び出せるようにするための変換関数</param>
        /// <param name="eventRegister">実際のイベントに登録するための関数</param>
        /// <param name="eventUnregister">実際のイベントから登録を解除するための関数</param>
        public ImtAwaiterFromEvent(Func<bool> completed, Func<Action, TEventDelegate> convert, Action<TEventDelegate> eventRegister, Action<TEventDelegate> eventUnregister)
        {
            // 初期化
            isCompleted = completed;
            eventFrom = convert;
            register = eventRegister;
            unregister = eventUnregister;
            eventHandler = default(TEventDelegate);
            context = null;
            continuation = null;
        }


        /// <summary>
        /// タスクが完了した時のハンドリングを行います。
        /// </summary>
        /// <param name="continuation">タスクを継続動作させるための継続関数</param>
        public void OnCompleted(Action continuation)
        {
            // 既にタスクが完了しているのなら
            if (IsCompleted)
            {
                // 直ちに継続関数を叩いて終了
                continuation();
                return;
            }


            // 現在の同期コンテキストの取得と継続関数を覚えておく
            context = AsyncOperationManager.SynchronizationContext;
            this.continuation = continuation;



            // イベントハンドラから継続関数を呼び出すための、関数の変換（イベントハンドラ -> 継続関数）をしてイベントの登録をする
            eventHandler = eventFrom(DoEventHandle);
            register(eventHandler);
        }


        /// <summary>
        /// タスクの結果を取得しますが、この構造体は常に結果は操作しません。
        /// </summary>
        public void GetResult()
        {
            // No handling...
        }


        /// <summary>
        /// この構造体を待機可能オブジェクトとして、自身のインスタンスコピーを取得します
        /// </summary>
        /// <returns>自身のコピーを返します</returns>
        public ImtAwaiterFromEvent<TEventDelegate> GetAwaiter()
        {
            // コピーを返す
            // TODO : できればコピーではなくもっと賢い方法があれば変更したい
            return this;
        }


        /// <summary>
        /// イベントハンドラによって呼び出され、同期コンテキストに継続関数をポストします。
        /// </summary>
        private void DoEventHandle()
        {
            // イベントの解除を行い、同期コンテキストに継続関数をポストする
            unregister(eventHandler);
            context.Post(cache, continuation);
        }
    }



    /// <summary>
    /// イベント実装のコードを、待機可能なコードに変換して、結果を取得する構造体です。
    /// </summary>
    /// <remarks>
    /// この構造体は、メモリのAllocコストがそこそこあるので、適所を見極めて使うようにして下さい。
    /// イベントの発生頻度が低く、メモリAllocがあまり気にならないタイミングなどでは、強力に発揮します。
    /// </remarks>
    /// <typeparam name="TEventDelegate">イベントのデリゲートのシグネチャ</typeparam>
    /// <typeparam name="TResult">待機したタスクの結果の型</typeparam>
    public struct ImtAwaiterFromEvent<TEventDelegate, TResult> : INotifyCompletion
    {
        // 構造体変数宣言
        private static readonly SendOrPostCallback cache = new SendOrPostCallback(_ => ((Action)_)());



        // メンバ変数定義
        private Func<bool> isCompleted;
        private Func<TResult> getResult;
        private Action<TEventDelegate> register;
        private Action<TEventDelegate> unregister;
        private Func<Action, TEventDelegate> eventFrom;
        private TEventDelegate eventHandler;
        private SynchronizationContext context;
        private Action continuation;



        /// <summary>
        /// タスクが完了したかどうか
        /// </summary>
        public bool IsCompleted => isCompleted();



        /// <summary>
        /// ImtAwaiterFromEvent のインスタンスを初期化します
        /// </summary>
        /// <param name="completed">待機オブジェクトが、タスクの完了を扱うための関数</param>
        /// <param name="resultCapture">タスクが完了した時の結果を取得する関数</param>
        /// <param name="convert">待機オブジェクト内部の継続関数を、イベントハンドラから呼び出せるようにするための変換関数</param>
        /// <param name="eventRegister">実際のイベントに登録するための関数</param>
        /// <param name="eventUnregister">実際のイベントから登録を解除するための関数</param>
        public ImtAwaiterFromEvent(Func<bool> completed, Func<TResult> resultCapture, Func<Action, TEventDelegate> convert, Action<TEventDelegate> eventRegister, Action<TEventDelegate> eventUnregister)
        {
            // 初期化
            isCompleted = completed;
            getResult = resultCapture;
            eventFrom = convert;
            register = eventRegister;
            unregister = eventUnregister;
            eventHandler = default(TEventDelegate);
            context = null;
            continuation = null;
        }


        /// <summary>
        /// タスクが完了した時のハンドリングを行います。
        /// </summary>
        /// <param name="continuation">タスクを継続動作させるための継続関数</param>
        public void OnCompleted(Action continuation)
        {
            // 既にタスクが完了しているのなら
            if (IsCompleted)
            {
                // 直ちに継続関数を叩いて終了
                continuation();
                return;
            }


            // 現在の同期コンテキストの取得と継続関数を覚えておく
            context = AsyncOperationManager.SynchronizationContext;
            this.continuation = continuation;



            // イベントハンドラから継続関数を呼び出すための、関数の変換（イベントハンドラ -> 継続関数）をしてイベントの登録をする
            eventHandler = eventFrom(DoEventHandle);
            register(eventHandler);
        }


        /// <summary>
        /// タスクの結果を取得します
        /// </summary>
        /// <returns>タスクの結果を返します</returns>
        public TResult GetResult()
        {
            // 結果を返す
            return getResult();
        }


        /// <summary>
        /// この構造体を待機可能オブジェクトとして、自身のインスタンスコピーを取得します
        /// </summary>
        /// <returns>自身のコピーを返します</returns>
        public ImtAwaiterFromEvent<TEventDelegate, TResult> GetAwaiter()
        {
            // コピーを返す
            // TODO : できればコピーではなくもっと賢い方法があれば変更したい
            return this;
        }


        /// <summary>
        /// イベントハンドラによって呼び出され、同期コンテキストに継続関数をポストします。
        /// </summary>
        private void DoEventHandle()
        {
            // イベントの解除を行い、同期コンテキストに継続関数をポストする
            unregister(eventHandler);
            context.Post(cache, continuation);
        }
    }
    #endregion



    #region Awaiter継続関数ハンドラ構造体
    /// <summary>
    /// 比較的スタンダードな Awaiter の継続関数をハンドリングするクラスです。
    /// このクラスは、多数の Awaiter の継続関数を登録することが可能で、継続関数を登録とシグナル設定をするだけで動作します。
    /// </summary>
    public class AwaiterContinuationHandler
    {
        /// <summary>
        /// 登録された Awaiter の継続関数と、その登録した時の同期コンテキストを保持する構造体です
        /// </summary>
        private struct Handler
        {
            /// <summary>
            /// 継続関数登録時の同期コンテキスト
            /// </summary>
            private SynchronizationContext context;

            /// <summary>
            /// 登録された継続関数
            /// </summary>
            private Action continuation;



            /// <summary>
            /// Handler のインスタンスを初期化します
            /// </summary>
            /// <param name="context">利用する同期コンテキスト</param>
            /// <param name="continuation">同期コンテキストにPostする継続関数</param>
            public Handler(SynchronizationContext context, Action continuation)
            {
                // 初期化
                this.context = context;
                this.continuation = continuation;
            }


            /// <summary>
            /// 同期コンテキストに継続関数をPostします
            /// </summary>
            /// <param name="callback">同期コンテキストにPostするためのコールバック関数</param>
            public void DoPost(SendOrPostCallback callback)
            {
                // 同期コンテキストに継続関数をポスト素r
                context.Post(callback, continuation);
            }
        }



        // 読み取り専用構造体変数宣言
        private static readonly SendOrPostCallback cache = new SendOrPostCallback(_ => ((Action)_)());



        // メンバ変数定義
        private Queue<Handler> handlerQueue;



        /// <summary>
        /// AwaiterContinuationHandler のインスタンスを既定サイズで初期化します。
        /// </summary>
        public AwaiterContinuationHandler() : this(capacity: 8)
        {
        }


        /// <summary>
        /// AwaiterContinuationHandler のインスタンスを指定された容量で初期化します。
        /// </summary>
        /// <param name="capacity">登録する継続関数の初期容量</param>
        public AwaiterContinuationHandler(int capacity)
        {
            // ハンドラキューの生成
            handlerQueue = new Queue<Handler>(capacity);
        }


        /// <summary>
        /// Awaiter の継続関数を登録します。
        /// 登録した継続関数は SetSignal() または SetOneShotSignal() 関数にて継続を行うことが可能です。
        /// </summary>
        /// <param name="continuation">登録する継続関数</param>
        public void RegisterContinuation(Action continuation)
        {
            // キューをロック
            lock (handlerQueue)
            {
                // 継続関数をハンドラキューに追加する
                handlerQueue.Enqueue(new Handler(AsyncOperationManager.SynchronizationContext, continuation));
            }
        }


        /// <summary>
        /// 登録された継続関数を、登録時の同期コンテキストを通じて呼び出されるようにします。
        /// また、一度シグナルした継続処理の参照は消失するため、再度 Awaite するには、改めて継続関数を登録する必要があります。
        /// </summary>
        public void SetSignal()
        {
            // キューをロック
            lock (handlerQueue)
            {
                // キューが空になるまでループ
                while (handlerQueue.Count > 0)
                {
                    // キューからハンドラをデキューして継続関数をポストする
                    handlerQueue.Dequeue().DoPost(cache);
                }
            }
        }


        /// <summary>
        /// 登録された複数の継続関数のうち１つだけ継続関数を呼び出します。
        /// 複数の待機オブジェクトが存在している場合は、先に待機したオブジェクトから継続関数を呼びます。
        /// </summary>
        public void SetOneShotSignal()
        {
            // キューをロック
            lock (handlerQueue)
            {
                // キューは既に空であれば
                if (handlerQueue.Count == 0)
                {
                    // 終了
                    return;
                }


                // キューからハンドラをデキューして継続関数をポストする
                handlerQueue.Dequeue().DoPost(cache);
            }
        }
    }
    #endregion
}