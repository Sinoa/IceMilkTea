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
    public interface IAwaitable<TResult> : IAwaitable
    {
        /// <summary>
        /// 待機をするための、値の返すことのできる汎用待機オブジェクト ImtAwaiter<typeparamref name="TResult"/> を取得します。
        /// </summary>
        /// <returns>汎用待機オブジェクト ImtAwaiter<typeparamref name="TResult"/> のインスタンスを返します</returns>
        new ImtAwaiter<TResult> GetAwaiter();


        /// <summary>
        /// 待機した結果を取得します。
        /// </summary>
        /// <returns>継続動作時に取得される結果を返します</returns>
        TResult GetResult();
    }
    #endregion



    #region Awaiter継続関数ハンドラクラス
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
        /// 登録された継続関数の数
        /// </summary>
        public int HandlerCount => GetHandlerCount();



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
        /// 登録された継続関数の数を取得します
        /// </summary>
        /// <returns>登録された継続関数の数を返します</returns>
        public int GetHandlerCount()
        {
            // キューをロック
            lock (handlerQueue)
            {
                // 登録済みハンドラの数を返す
                return handlerQueue.Count;
            }
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
    /// シグナル操作をして待機状態をコントロールすることの出来る、値を返す待機可能な抽象クラスです。
    /// </summary>
    /// <remarks>
    /// 単純なシグナル操作による、待機制御を実現する場合には有用です。
    /// </remarks>
    public abstract class ImtAwaitableWaitHandle<TResult> : ImtAwaitableWaitHandle, IAwaitable<TResult>
    {
        /// <summary>
        /// ImtAwaitableWaitHandle<typeparamref name="TResult"/> のインスタンスを初期化します
        /// </summary>
        /// <param name="initialSignal">初期のシグナル状態</param>
        public ImtAwaitableWaitHandle(bool initialSignal) : base(initialSignal)
        {
        }


        /// <summary>
        /// このオブジェクトの待機オブジェクトを取得します
        /// </summary>
        /// <exception cref="ObjectDisposedException">待機ハンドルは解放済みです</exception>
        /// <returns>待機オブジェクトを返します</returns>
        public new ImtAwaiter<TResult> GetAwaiter()
        {
            // 解放済み例外の処理をしておく
            ThrowIfDisposed();


            // 単純なAwaiterを返す
            return new ImtAwaiter<TResult>(this);
        }


        /// <summary>
        /// 非シグナル状態の時の結果を取得します
        /// </summary>
        /// <returns>現在の結果の値を返します</returns>
        public abstract TResult GetResult();
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
        /// また ResetSignal() を呼び出さない限り、ずっと待機されない状態になります。
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
    /// シグナル状態をマニュアルコントロールする待機可能な、値を返せる待機ハンドラクラスです。
    /// </summary>
    public class ImtAwaitableManualReset<TResult> : ImtAwaitableManualReset, IAwaitable<TResult>
    {
        // メンバ変数定義
        private TResult result;



        /// <summary>
        /// ImtAwaitableManualReset<typeparamref name="TResult"/> のインスタンスを初期化します
        /// </summary>
        /// <param name="initialSignal">初期のシグナル状態</param>
        public ImtAwaitableManualReset(bool initialSignal) : base(initialSignal)
        {
        }


        /// <summary>
        /// 待機した結果の値を、事前に設定します。
        /// </summary>
        /// <param name="result">準備する結果</param>
        public void PrepareResult(TResult result)
        {
            // 結果を覚えておく
            this.result = result;
        }


        /// <summary>
        /// 待機結果を設定してから、待機ハンドラをシグナル状態にして、待機オブジェクトの待機を解除します。
        /// また ResetSignal() を呼び出さない限り、ずっと待機されない状態になります。
        /// 再び、待機状態にさせるには ResetSignal() を呼び出して下さい。
        /// </summary>
        /// <param name="result">待機した結果として設定する値</param>
        /// <exception cref="ObjectDisposedException">待機ハンドルは解放済みです</exception>
        /// <see cref="ResetSignal"/>
        public void SetSignal(TResult result)
        {
            // 結果を設定して基本クラスのSetSignalを呼ぶ
            this.result = result;
            base.SetSignal();
        }


        /// <summary>
        /// このオブジェクトの待機オブジェクトを取得します
        /// </summary>
        /// <exception cref="ObjectDisposedException">待機ハンドルは解放済みです</exception>
        /// <returns>待機オブジェクトを返します</returns>
        public new ImtAwaiter<TResult> GetAwaiter()
        {
            // 解放済み例外の処理をしておく
            ThrowIfDisposed();


            // 単純なAwaiterを返す
            return new ImtAwaiter<TResult>(this);
        }


        /// <summary>
        /// タスクの待機結果を取得します
        /// </summary>
        /// <returns>待機結果を返します</returns>
        public TResult GetResult()
        {
            // 結果をそのまま返す
            return result;
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



    /// <summary>
    /// シグナル状態を自動コントロールする待機可能な、値を返せる待機ハンドラクラスです
    /// </summary>
    public class ImtAwaitableAutoReset<TResult> : ImtAwaitableAutoReset, IAwaitable<TResult>
    {
        // メンバ変数定義
        private TResult result;



        /// <summary>
        /// 待機した結果の値を、事前に設定します。
        /// </summary>
        /// <param name="result">準備する結果</param>
        public void PrepareResult(TResult result)
        {
            // 結果を覚えておく
            this.result = result;
        }


        /// <summary>
        /// 待機結果を設定してから、待機ハンドラをシグナル状態にして、待機オブジェクトの待機を解除します。
        /// また ResetSignal() を呼び出さない限り、ずっと待機されない状態になります。
        /// 再び、待機状態にさせるには ResetSignal() を呼び出して下さい。
        /// </summary>
        /// <param name="result">待機した結果として設定する値</param>
        /// <exception cref="ObjectDisposedException">待機ハンドルは解放済みです</exception>
        /// <see cref="ResetSignal"/>
        public void SetSignal(TResult result)
        {
            // 結果を設定して基本クラスのSetSignalを呼ぶ
            this.result = result;
            base.SetSignal();
        }


        /// <summary>
        /// このオブジェクトの待機オブジェクトを取得します
        /// </summary>
        /// <exception cref="ObjectDisposedException">待機ハンドルは解放済みです</exception>
        /// <returns>待機オブジェクトを返します</returns>
        public new ImtAwaiter<TResult> GetAwaiter()
        {
            // 解放済み例外の処理をしておく
            ThrowIfDisposed();


            // 単純なAwaiterを返す
            return new ImtAwaiter<TResult>(this);
        }


        /// <summary>
        /// タスクの待機結果を取得します
        /// </summary>
        /// <returns>待機結果を返します</returns>
        public TResult GetResult()
        {
            // 結果をそのまま返す
            return result;
        }
    }
    #endregion



    #region AwaitableFromEvent
    /// <summary>
    /// イベント機構、コールバック機構のコードを、待機可能なコードに変換する待機可能なクラスです。
    /// </summary>
    /// <typeparam name="TEventDelegate">イベント または コールバック で使用する関数のシグネチャを示す型</typeparam>
    /// <typeparam name="TResult">イベント または コールバック または オブジェクト状態 で得られた結果の型</typeparam>
    public class ImtAwaitableFromEvent<TEventDelegate, TResult> : IAwaitable<TResult>
    {
        // メンバ変数定義
        private AwaiterContinuationHandler awaiterHandler;
        private Func<bool> isCompleted;
        private Action<TEventDelegate> register;
        private Action<TEventDelegate> unregister;
        private TEventDelegate handler;
        private bool completeState;
        private bool autoReset;
        private TResult result;



        /// <summary>
        /// タスクが完了しているかどうか
        /// </summary>
        public bool IsCompleted => isCompleted != null ? isCompleted() : completeState;



        /// <summary>
        /// ImtAwaitableFromEvent のインスタンスを初期化します。
        /// </summary>
        /// <remarks>
        /// completed を null に指定子た場合は、待機オブジェクトの完了状態が内部で保持するようになりますが、改めて
        /// 待機し直す場合は、状態をリセットする必要がありますので、その場合は ResetCompleteState() 関数を呼び出してください。
        /// または autoReset パラメータに true を設定すれば継続処理直後に自動的に解除されます。
        /// </remarks>
        /// <param name="completed">待機オブジェクトが、タスクの完了を扱うための関数。内部の完了状態を利用する場合は null の指定が可能です</param>
        /// <param name="autoReset">内部の完了状態を利用する場合に、イベント完了後に自動的にリセットするかどうか</param>
        /// <param name="convert">待機オブジェクト内部の継続関数を、イベントハンドラから呼び出せるようにするための変換関数</param>
        /// <param name="eventRegister">実際のイベントに登録するための関数</param>
        /// <param name="eventUnregister">実際のイベントから登録を解除するための関数</param>
        /// <see cref="ResetCompleteState"/>
        public ImtAwaitableFromEvent(Func<bool> completed, bool autoReset, Func<Action<TResult>, TEventDelegate> convert, Action<TEventDelegate> eventRegister, Action<TEventDelegate> eventUnregister)
        {
            // 待機オブジェクトハンドラの生成
            awaiterHandler = new AwaiterContinuationHandler();


            // ユーザー関数を覚えるのと、イベントハンドラを作る
            isCompleted = completed;
            register = eventRegister;
            unregister = eventUnregister;
            handler = convert(OnEventHandle);


            // 待機状態の初期化と自動リセットの値を受け取る
            completeState = false;
            this.autoReset = autoReset;
        }


        /// <summary>
        /// 内部の完了状態をリセットし、再び待機可能な状態にします。
        /// </summary>
        public void ResetCompleteState()
        {
            // 状態をリセット
            completeState = false;
        }


        /// <summary>
        /// この待機可能クラスの、待機オブジェクトを取得します。
        /// </summary>
        /// <returns>待機オブジェクトを返します</returns>
        public ImtAwaiter<TResult> GetAwaiter()
        {
            // 待機オブジェクトを返す
            return new ImtAwaiter<TResult>(this);
        }


        /// <summary>
        /// この待機可能クラスの、待機オブジェクトを取得します。
        /// </summary>
        /// <returns>待機オブジェクトを返します</returns>
        ImtAwaiter IAwaitable.GetAwaiter()
        {
            // 待機オブジェクトを返す
            return new ImtAwaiter(this);
        }


        /// <summary>
        /// 待機オブジェクトの継続関数を登録します
        /// </summary>
        /// <param name="continuation">登録する継続関数</param>
        public void RegisterContinuation(Action continuation)
        {
            // まだ継続関数が未登録状態なら
            if (awaiterHandler.HandlerCount == 0)
            {
                // イベントハンドラを登録
                register(handler);
            }


            // 待機オブジェクトハンドラに継続関数を登録
            awaiterHandler.RegisterContinuation(continuation);
        }


        /// <summary>
        /// イベント または コールバック で得られた結果を取得します
        /// </summary>
        /// <returns>イベント または コールバック で得られた結果を返します</returns>
        public TResult GetResult()
        {
            // 得た結果を返す
            return result;
        }


        /// <summary>
        /// イベント または コールバック のハンドリングを行います。
        /// </summary>
        /// <param name="result">イベント または コールバック からの結果</param>
        private void OnEventHandle(TResult result)
        {
            // 完了状態の設定と、結果の保存をする
            completeState = true;
            this.result = result;


            // 待機オブジェクトハンドラのシグナルを設定してイベントの解除
            awaiterHandler.SetSignal();
            unregister(handler);


            // もし自動リセットがONなら
            if (autoReset)
            {
                // リセットする
                ResetCompleteState();
            }
        }
    }



    /// <summary>
    /// イベント機構、コールバック機構のコードを、待機可能なコードに変換する待機可能なクラスです。
    /// </summary>
    /// <typeparam name="TEventDelegate">イベント または コールバック で使用する関数のシグネチャを示す型</typeparam>
    public class ImtAwaitableFromEvent<TEventDelegate> : ImtAwaitableFromEvent<TEventDelegate, object>
    {
        /// <summary>
        /// ImtAwaitableFromEvent のインスタンスを初期化します。
        /// </summary>
        /// <remarks>
        /// completed を null に指定子た場合は、待機オブジェクトの完了状態が内部で保持するようになりますが、改めて
        /// 待機し直す場合は、状態をリセットする必要がありますので、その場合は ResetCompleteState() 関数を呼び出してください。
        /// </remarks>
        /// <param name="completed">待機オブジェクトが、タスクの完了を扱うための関数。内部の完了状態を利用する場合は null の指定が可能です。</param>
        /// <param name="autoReset">内部の完了状態を利用する場合に、イベント完了後に自動的にリセットするかどうか</param>
        /// <param name="convert">待機オブジェクト内部の継続関数を、イベントハンドラから呼び出せるようにするための変換関数</param>
        /// <param name="eventRegister">実際のイベントに登録するための関数</param>
        /// <param name="eventUnregister">実際のイベントから登録を解除するための関数</param>
        /// <see cref="ResetCompleteState"/>
        public ImtAwaitableFromEvent(Func<bool> completed, bool autoReset, Func<Action<object>, TEventDelegate> convert, Action<TEventDelegate> eventRegister, Action<TEventDelegate> eventUnregister) : base(completed, autoReset, convert, eventRegister, eventUnregister)
        {
        }
    }
    #endregion



    #region Awaitable Utility
    /// <summary>
    /// IAwaitable なオブジェクトを待機する時のヘルパー実装を提供します
    /// </summary>
    public class ImtAwaitHelper
    {
        /// <summary>
        /// 全ての待機オブジェクトを待機する、待機クラスです。
        /// </summary>
        private class AwaitableWhenAll : IAwaitable
        {
            // クラス変数宣言
            private static readonly SendOrPostCallback cache = new SendOrPostCallback(_ => ((Action)_)());

            // メンバ変数定義
            private AwaiterContinuationHandler awaiterHandler;
            private SynchronizationContext currentContext;
            private ImtAwaitHelper helper;
            private Action update;



            /// <summary>
            /// タスクが完了したかどうか
            /// </summary>
            public bool IsCompleted { get; private set; }



            /// <summary>
            /// AwaitableWhenAll のインスタンスを初期化します
            /// </summary>
            /// <param name="helper">このインスタンスを保持する ImtAwaitHelper</param>
            public AwaitableWhenAll(ImtAwaitHelper helper)
            {
                // もろもろ初期化
                awaiterHandler = new AwaiterContinuationHandler();
                currentContext = AsyncOperationManager.SynchronizationContext;
                this.helper = helper;
                update = Update;
                IsCompleted = true;
            }


            /// <summary>
            /// 内部の状態更新をします。
            /// </summary>
            public void Update()
            {
                // まだ未完了
                IsCompleted = false;


                // そもそもリストが空なら
                if (helper.awaitableList.Count == 0)
                {
                    // 完了状態にして、待機中のオブジェクトの待機を解除
                    IsCompleted = true;
                    awaiterHandler.SetSignal();
                    return;
                }


                // ヘルパーが持っている待機オブジェクトの数分ループ
                bool isAllFinish = true;
                foreach (var awaitable in helper.awaitableList)
                {
                    // もし未完了状態なら
                    if (!awaitable.IsCompleted)
                    {
                        // 全て完了フラグをへし折ってループから抜ける
                        isAllFinish = false;
                        break;
                    }
                }


                // もし全ての待機オブジェクトが完了状態なら
                if (isAllFinish)
                {
                    // もうリストを空にする
                    helper.awaitableList.Clear();


                    // 完了状態にして、待機中のオブジェクトの待機を解除
                    IsCompleted = true;
                    awaiterHandler.SetSignal();
                    return;
                }


                // まだ未完了なら、同期コンテキストに再び自分を呼び出してもらうようにポストする
                // TODO : 本来ならスケジューラなどを実装してスケジューリングされるようにしたほうが良いが、今は雑に同期コンテキストにループのようなことをしてもらう
                currentContext.Post(cache, update);
            }


            /// <summary>
            /// この待機可能クラスの待機オブジェクトを取得します
            /// </summary>
            /// <returns>待機オブジェクトを返します</returns>
            public ImtAwaiter GetAwaiter()
            {
                // 待機オブジェクトを生成して返す
                return new ImtAwaiter(this);
            }


            /// <summary>
            /// 待機オブジェクトからの継続関数を登録します
            /// </summary>
            /// <param name="continuation">登録する継続関数</param>
            public void RegisterContinuation(Action continuation)
            {
                // 待機ハンドラに継続関数を登録する
                awaiterHandler.RegisterContinuation(continuation);
            }
        }



        /// <summary>
        /// いずれかの待機オブジェクトを待機する、待機クラスです
        /// </summary>
        private class AwaitableWhenAny : IAwaitable<IAwaitable>
        {
            // クラス変数宣言
            private static readonly SendOrPostCallback cache = new SendOrPostCallback(_ => ((Action)_)());

            // メンバ変数定義
            private AwaiterContinuationHandler awaiterHandler;
            private SynchronizationContext currentContext;
            private ImtAwaitHelper helper;
            private Action update;
            private IAwaitable firstFinishAwaitable;



            /// <summary>
            /// タスクが完了したかどうか
            /// </summary>
            public bool IsCompleted { get; private set; }



            /// <summary>
            /// AwaitableWhenAny のインスタンスを初期化します
            /// </summary>
            /// <param name="helper">このインスタンスを保持する ImtAwaitHelper</param>
            public AwaitableWhenAny(ImtAwaitHelper helper)
            {
                // もろもろ初期化
                awaiterHandler = new AwaiterContinuationHandler();
                currentContext = AsyncOperationManager.SynchronizationContext;
                this.helper = helper;
                update = Update;
                IsCompleted = true;
            }


            /// <summary>
            /// 内部の状態更新をします。
            /// </summary>
            public void Update()
            {
                // まだ未完了
                IsCompleted = false;


                // そもそもリストが空なら
                if (helper.awaitableList.Count == 0)
                {
                    // 完了状態にして、待機中のオブジェクトの待機を解除
                    IsCompleted = true;
                    awaiterHandler.SetSignal();
                    return;
                }


                // ヘルパーが持っている待機オブジェクトの数分ループ
                firstFinishAwaitable = null;
                foreach (var awaitable in helper.awaitableList)
                {
                    // もし完了状態なら
                    if (awaitable.IsCompleted)
                    {
                        // 最初に完了した待機オブジェクトとして覚えて抜ける
                        firstFinishAwaitable = awaitable;
                        break;
                    }
                }


                // もし完了オブジェクトが見つからなかったら
                if (firstFinishAwaitable == null)
                {
                    // まだ未完了なら、同期コンテキストに再び自分を呼び出してもらうようにポストする
                    // TODO : 本来ならスケジューラなどを実装してスケジューリングされるようにしたほうが良いが、今は雑に同期コンテキストにループのようなことをしてもらう
                    currentContext.Post(cache, update);
                    return;
                }


                // リストから完了オブジェクトを削除
                helper.awaitableList.Remove(firstFinishAwaitable);


                // 完了タスクを１つ目を見つけたので、シグナルを送る
                awaiterHandler.SetSignal();


                // もしリストが空になったのなら
                if (helper.awaitableList.Count == 0)
                {
                    // 完了状態にして終了
                    IsCompleted = true;
                    return;
                }


                // まだ未完了なら、同期コンテキストに再び自分を呼び出してもらうようにポストする
                // TODO : 本来ならスケジューラなどを実装してスケジューリングされるようにしたほうが良いが、今は雑に同期コンテキストにループのようなことをしてもらう
                currentContext.Post(cache, update);
            }


            /// <summary>
            /// この待機可能クラスの待機オブジェクトを取得します
            /// </summary>
            /// <returns>待機オブジェクトを返します</returns>
            public ImtAwaiter<IAwaitable> GetAwaiter()
            {
                // 待機オブジェクトを生成して返す
                return new ImtAwaiter<IAwaitable>(this);
            }


            /// <summary>
            /// この待機可能クラスの待機オブジェクトを取得します
            /// </summary>
            /// <returns>待機オブジェクトを返します</returns>
            ImtAwaiter IAwaitable.GetAwaiter()
            {
                // 待機オブジェクトを生成して返す
                return new ImtAwaiter(this);
            }


            /// <summary>
            /// タスクの結果を取得します
            /// </summary>
            /// <returns>タスクの結果を返します</returns>
            public IAwaitable GetResult()
            {
                // 最初に完了した待機オブジェクトを返す
                return firstFinishAwaitable;
            }


            /// <summary>
            /// 待機オブジェクトからの継続関数を登録します
            /// </summary>
            /// <param name="continuation">登録する継続関数</param>
            public void RegisterContinuation(Action continuation)
            {
                // 待機ハンドラに継続関数を登録する
                awaiterHandler.RegisterContinuation(continuation);
            }
        }



        // メンバ変数定義
        private AwaitableWhenAll whenAllOperator;
        private AwaitableWhenAny whenAnyOperator;
        private List<IAwaitable> awaitableList;



        /// <summary>
        /// ImtAwaitHelper のインスタンスを初期化します
        /// </summary>
        public ImtAwaitHelper()
        {
            // 各種待機オブジェクトの生成をする
            whenAllOperator = new AwaitableWhenAll(this);
            whenAnyOperator = new AwaitableWhenAny(this);
            awaitableList = new List<IAwaitable>();
        }


        /// <summary>
        /// 待機ヘルパに待機オブジェクトを追加します。
        /// 既に追加済みの待機オブジェクトは無視されます。
        /// </summary>
        /// <param name="awaitable">追加する待機オブジェクト</param>
        public void AddAwaitable(IAwaitable awaitable)
        {
            // 既に追加済みなら
            if (awaitableList.Contains(awaitable))
            {
                // 何もせず終了
                return;
            }


            // 待機オブジェクトを追加する
            awaitableList.Add(awaitable);
        }


        /// <summary>
        /// 追加された待機オブジェクトの全てが、完了するまで待機する、待機オブジェクトを提供します。
        /// </summary>
        /// <returns>追加された待機オブジェクトの全てを待機する、待機オブジェクトを返します</returns>
        public IAwaitable WhenAll()
        {
            // 何も追加しない引数で WhenAll を叩く
            return WhenAll(null);
        }


        /// <summary>
        /// 追加された待機オブジェクトと、引数に指定された待機オブジェクト配列の中を更に追加して
        /// 全てが完了するまで待機する、待機オブジェクトを提供します。
        /// </summary>
        /// <param name="awaitables">追加する待機オブジェクトの配列。追加しない場合は null の指定が可能です</param>
        /// <returns>追加された待機オブジェクトの全てを待機する、待機オブジェクトを返します</returns>
        public IAwaitable WhenAll(IAwaitable[] awaitables)
        {
            // もし配列の指定があるなら
            if (awaitables != null)
            {
                // 配列の中を全て回る
                foreach (var awaitable in awaitables)
                {
                    // 追加する
                    AddAwaitable(awaitable);
                }
            }


            // WhenAll制御が完了状態なら
            if (whenAllOperator.IsCompleted == true)
            {
                // 更新を開始する
                whenAllOperator.Update();
            }


            // WhenAll待機オブジェクトを返す
            return whenAllOperator;
        }


        /// <summary>
        /// 追加された待機オブジェクトのいずれかを、完了するまで待機する、待機オブジェクトを提供します。
        /// </summary>
        /// <returns>追加された待機オブジェクトのいずれかを、完了するまで待機する、待機オブジェクトを返します</returns>
        public IAwaitable<IAwaitable> WhenAny()
        {
            // 何も追加しない引数で WhenAny を叩く
            return WhenAny(null);
        }


        /// <summary>
        /// 追加された待機オブジェクトのいずれかを、完了するまで待機する、待機オブジェクトを提供します。
        /// また、引数で指定された待機オブジェクトの配列内も追加します。
        /// </summary>
        /// <param name="awaitables">追加する待機オブジェクトの配列。追加しない場合は null の指定が可能です</param>
        /// <returns>追加された待機オブジェクトのいずれかを、完了するまで待機する、待機オブジェクトを返します</returns>
        public IAwaitable<IAwaitable> WhenAny(IAwaitable[] awaitables)
        {
            // もし配列の指定があるなら
            if (awaitables != null)
            {
                // 配列の中を全て回る
                foreach (var awaitable in awaitables)
                {
                    // 追加する
                    AddAwaitable(awaitable);
                }
            }


            // WhenAny制御が完了状態なら
            if (whenAnyOperator.IsCompleted == true)
            {
                // 更新を開始する
                whenAnyOperator.Update();
            }


            // WheAny待機オブジェクトを返す
            return whenAnyOperator;
        }
    }
    #endregion
}