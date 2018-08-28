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
using System.Runtime.ExceptionServices;
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
        private ExceptionDispatchInfo exceptionInfo;



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
            // 保持する担当を覚えて、もろもろ初期化
            awaitableContext = context;
            exceptionInfo = null;
        }


        /// <summary>
        /// タスクが完了した時のハンドリングを行います。
        /// </summary>
        /// <param name="continuation">タスクを継続動作させるための継続関数</param>
        public void OnCompleted(Action continuation)
        {
            try
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
            catch (Exception exception)
            {
                // 例外をキャプチャして覚えておいて、直ちに継続関数を叩く
                exceptionInfo = ExceptionDispatchInfo.Capture(exception);
                continuation();
            }
        }


        /// <summary>
        /// タスクの結果を取得しますが、この構造体は常に結果は操作しません。
        /// </summary>
        public void GetResult()
        {
            // もし例外情報を持っていたら
            if (exceptionInfo != null)
            {
                // ここで例外を投げてメソッドビルダーに例外を任せる
                exceptionInfo.Throw();
            }


            // 待機可能クラスが例外情報を持っているなら
            var error = awaitableContext.GetError();
            if (error != null)
            {
                // ここで吐き出す
                error.Throw();
            }
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
        private ExceptionDispatchInfo exceptionInfo;



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
            // 保持する担当を覚えて、もろもろ初期化
            awaitableContext = context;
            exceptionInfo = null;
        }


        /// <summary>
        /// タスクが完了した時のハンドリングを行います。
        /// </summary>
        /// <param name="continuation">タスクを継続動作させるための継続関数</param>
        public void OnCompleted(Action continuation)
        {
            try
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
            catch (Exception exception)
            {
                // 例外をキャプチャして覚えておいて、直ちに継続関数を叩く
                exceptionInfo = ExceptionDispatchInfo.Capture(exception);
                continuation();
            }
        }


        /// <summary>
        /// タスクの結果を取得します。
        /// </summary>
        /// <returns>IAwaitable<typeparamref name="TResult"/>.GetResult() の結果を返します</returns>
        public TResult GetResult()
        {
            // もし例外情報を持っていたら
            if (exceptionInfo != null)
            {
                // ここで例外を投げてメソッドビルダーに例外を任せる
                exceptionInfo.Throw();
            }


            // 待機可能クラスが例外情報を持っているなら
            var error = awaitableContext.GetError();
            if (error != null)
            {
                // ここで吐き出す
                error.Throw();
                return default(TResult);
            }


            // 待機結果を取得して返す（この関数が例外を出しても、メソッドビルダーに拾い上げられる）
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


        /// <summary>
        /// Awaitable が持っているエラー情報となる例外情報を取得します。
        /// </summary>
        /// <returns>エラーを保持している場合は、その例外情報を返します。持っていない場合は null を返します。</returns>
        ExceptionDispatchInfo GetError();
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
            public void DoPost()
            {
                // 同期コンテキストに継続関数をポスト素r
                context.Post(ImtSynchronizationContextHelper.CachedSendOrPostCallback, continuation);
            }
        }



        // メンバ変数定義
        private Queue<Handler> handlerQueue;



        /// <summary>
        /// 登録された継続関数の数
        /// </summary>
        public int HandlerCount => GetHandlerCount();



        /// <summary>
        /// AwaiterContinuationHandler のインスタンスを既定サイズで初期化します。
        /// </summary>
        public AwaiterContinuationHandler() : this(capacity: 32)
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
                    handlerQueue.Dequeue().DoPost();
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
                handlerQueue.Dequeue().DoPost();
            }
        }
    }
    #endregion



    #region AwaitableBase
    /// <summary>
    /// 値の返却をしない、待機可能なクラスを実装するための基本抽象クラスです。
    /// 汎用的な通常の、値の返却をしない待機可能クラスを実装をする場合には、このクラスを継承して下さい。
    /// </summary>
    public abstract class ImtAwaitable : IAwaitable, IDisposable
    {
        // メンバ変数定義
        private AwaiterContinuationHandler awaiterHandler;
        private ExceptionDispatchInfo exceptionInfo;



        /// <summary>
        /// タスクが完了しているかどうか
        /// </summary>
        public virtual bool IsCompleted { get; protected set; }


        /// <summary>
        /// このオブジェクトが解放済みかどうか
        /// </summary>
        public bool Disposed { get; private set; }



        /// <summary>
        /// ImtAwaitable のインスタンスを初期化します
        /// </summary>
        public ImtAwaitable()
        {
            // 待機オブジェクトハンドラの生成
            awaiterHandler = new AwaiterContinuationHandler();
        }


        /// <summary>
        /// ImtAwaitable のインスタンスを初期化します
        /// </summary>
        /// <param name="capacity">待機オブジェクトハンドラの初期容量</param>
        public ImtAwaitable(int capacity)
        {
            // 待機オブジェクトハンドラの生成
            awaiterHandler = new AwaiterContinuationHandler(capacity);
        }


        /// <summary>
        /// ImtAwaitable のファイナライザを実行します
        /// </summary>
        ~ImtAwaitable()
        {
            // ファイナライザからのDispose呼び出し
            Dispose(false);
        }


        /// <summary>
        /// リソースの解放を行います。
        /// また、待機中のオブジェクトが存在する場合は、シグナルが直ちに設定されます。
        /// </summary>
        public void Dispose()
        {
            // DisposeからのDispose呼び出しをして、ファイナライズキューに入らないようにしてもらう
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        /// <summary>
        /// 実際のリソース解放を行います。
        /// </summary>
        /// <param name="disposing">Diisposeからの呼び出しの場合は true が、ファイナライザからの呼び出しの場合は false が設定されます</param>
        protected virtual void Dispose(bool disposing)
        {
            // 既に解放済みなら
            if (Disposed)
            {
                // 何もせず終了
                return;
            }


            // 無条件にIsCompletedをtrueに設定して、すべての待機オブジェクトにシグナルを送る
            IsCompleted = true;
            awaiterHandler.SetSignal();


            // 解放済みマーク
            Disposed = true;
        }


        /// <summary>
        /// この待機可能クラスの、待機オブジェクトを取得します
        /// </summary>
        /// <returns>待機オブジェクトを返します</returns>
        /// <exception cref="ObjectDisposedException">この待機クラスは既に破棄されています</exception>
        public virtual ImtAwaiter GetAwaiter()
        {
            // 解放済み例外関数を叩く
            ThrowIfDisposed();


            // 新しい待機オブジェクトを生成して返す
            return new ImtAwaiter(this);
        }


        /// <summary>
        /// 待機オブジェクトからの継続関数を登録します
        /// </summary>
        /// <param name="continuation">登録する継続関数</param>
        /// <exception cref="ObjectDisposedException">この待機クラスは既に破棄されています</exception>
        public virtual void RegisterContinuation(Action continuation)
        {
            // 解放済み例外関数を叩く
            ThrowIfDisposed();


            // 待機オブジェクトハンドラに継続関数を登録する
            awaiterHandler.RegisterContinuation(continuation);
        }


        /// <summary>
        /// 登録された継続関数にシグナルを設定して、継続関数が呼び出されるようにします。
        /// </summary>
        /// <exception cref="ObjectDisposedException">この待機クラスは既に破棄されています</exception>
        protected internal virtual void SetSignal()
        {
            // 解放済み例外関数を叩く
            ThrowIfDisposed();


            // 待機オブジェクトハンドラのシグナルを設定する
            awaiterHandler.SetSignal();
        }


        /// <summary>
        /// 待機状態が完了するとともに、登録された継続関数にシグナルを設定して、継続関数が呼び出されるようにします。
        /// </summary>
        /// <exception cref="ObjectDisposedException">この待機クラスは既に破棄されています</exception>
        protected internal virtual void SetSignalWithCompleted()
        {
            // 解放済み例外関数を叩く
            ThrowIfDisposed();


            // 完了状態にして、待機オブジェクトハンドラのシグナルを設定する
            IsCompleted = true;
            awaiterHandler.SetSignal();
        }


        /// <summary>
        /// 登録された複数の継続関数のうち１つだけ継続関数を呼び出します。
        /// 複数の待機オブジェクトが存在している場合は、先に待機したオブジェクトから継続関数を呼びます。
        /// </summary>
        /// <exception cref="ObjectDisposedException">この待機クラスは既に破棄されています</exception>
        protected virtual void SetOneShotSignal()
        {
            // 解放済み例外関数を叩く
            ThrowIfDisposed();


            // 待機オブジェクトハンドラのシグナルを設定する
            awaiterHandler.SetOneShotSignal();
        }


        /// <summary>
        /// 待機状態が完了するとともに、登録された複数の継続関数のうち１つだけ継続関数を呼び出します。
        /// 複数の待機オブジェクトが存在している場合は、先に待機したオブジェクトから継続関数を呼びます。
        /// </summary>
        /// <exception cref="ObjectDisposedException">この待機クラスは既に破棄されています</exception>
        protected virtual void SetOneShotSignalWithCompleted()
        {
            // 解放済み例外関数を叩く
            ThrowIfDisposed();


            // 完了状態にして、待機オブジェクトハンドラのシグナルを設定する
            IsCompleted = true;
            awaiterHandler.SetOneShotSignal();
        }


        /// <summary>
        /// 待機可能クラスが、例外を発生させてしまった場合、その例外を設定します。
        /// この関数で設定した例外は、適切なタイミングで報告されます。
        /// また、原則としてこの関数を利用した直後は SetSignal() 関数を呼び出して
        /// 直ちに継続関数を解放するようにしてください。
        /// </summary>
        /// <param name="exception">設定する例外</param>
        protected internal void SetException(Exception exception)
        {
            // 例外をキャプチャして保持する
            exceptionInfo = ExceptionDispatchInfo.Capture(exception);
        }


        /// <summary>
        /// 待機可能クラスが、発生した例外の情報を取得します。
        /// </summary>
        /// <returns>発生した例外の情報を返します</returns>
        public ExceptionDispatchInfo GetError()
        {
            // 持っている例外情報を返す
            return exceptionInfo;
        }


        /// <summary>
        /// オブジェクトが解放済みの場合は、例外を送出します
        /// </summary>
        /// <exception cref="ObjectDisposedException">この待機クラスは既に破棄されています</exception>
        protected void ThrowIfDisposed()
        {
            // 既に破棄済みなら
            if (Disposed)
            {
                // 例外を吐く
                throw new ObjectDisposedException(null);
            }
        }
    }



    /// <summary>
    /// 値の返却をする、待機可能なクラスを実装するための基本抽象クラスです。
    /// 汎用的な通常の、値の返却をする待機可能クラスを実装をする場合には、このクラスを継承して下さい。
    /// </summary>
    public abstract class ImtAwaitable<TResult> : ImtAwaitable, IAwaitable<TResult>
    {
        /// <summary>
        /// ImtAwaitable のインスタンスを初期化します
        /// </summary>
        public ImtAwaitable() : base()
        {
        }


        /// <summary>
        /// ImtAwaitable のインスタンスを初期化します
        /// </summary>
        /// <param name="capacity">待機オブジェクトハンドラの初期容量</param>
        public ImtAwaitable(int capacity) : base(capacity)
        {
        }


        /// <summary>
        /// この待機可能クラスの、待機オブジェクトを取得します
        /// </summary>
        /// <returns>待機オブジェクトを返します</returns>
        /// <exception cref="ObjectDisposedException">この待機クラスは既に破棄されています</exception>
        public new ImtAwaiter<TResult> GetAwaiter()
        {
            // 解放済み例外関数を叩く
            ThrowIfDisposed();


            // 新しい待機オブジェクトを生成して返す
            return new ImtAwaiter<TResult>(this);
        }


        /// <summary>
        /// この待機可能クラスの結果を取得します
        /// </summary>
        /// <returns>結果を返します</returns>
        public abstract TResult GetResult();
    }
    #endregion



    #region AwaitableWaitHandle
    /// <summary>
    /// シグナル操作をして待機状態をコントロールすることの出来る、待機可能な抽象クラスです。
    /// </summary>
    /// <remarks>
    /// 単純なシグナル操作による、待機制御を実現する場合には有用です。
    /// </remarks>
    public abstract class ImtAwaitableWaitHandle : ImtAwaitable
    {
        /// <summary>
        /// ImtAwaitableWaitHandle のインスタンスを初期化します
        /// </summary>
        /// <param name="initialSignal">初期のシグナル状態</param>
        public ImtAwaitableWaitHandle(bool initialSignal)
        {
            // シグナル状態を初期化
            IsCompleted = initialSignal;
        }


        /// <summary>
        /// この待機ハンドルのシグナルを設定して。
        /// 待機オブジェクトの待機を解除します。
        /// </summary>
        public abstract void Set();


        /// <summary>
        /// この待機ハンドルのシグナルを解除して。
        /// オブジェクトが待機状態になるようにします。
        /// </summary>
        /// <exception cref="ObjectDisposedException">待機ハンドルは解放済みです</exception>
        public virtual void Reset()
        {
            // 解放済み例外の処理をしておく
            ThrowIfDisposed();


            // 非シグナル状態にする
            IsCompleted = false;
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
        /// また ResetSignal() を呼び出さない限り、ずっと待機されない状態になります。
        /// 再び、待機状態にさせるには ResetSignal() を呼び出して下さい。
        /// </summary>
        /// <exception cref="ObjectDisposedException">待機ハンドルは解放済みです</exception>
        /// <see cref="Reset"/>
        public override void Set()
        {
            // シグナル状態を設定して、継続関数を呼び出す
            SetSignalWithCompleted();
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
        public void Set(TResult result)
        {
            // 結果を設定して基本クラスのSetを呼ぶ
            this.result = result;
            Set();
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
        /// すべての待機オブジェクトの待機を解除するためには、再び Set() を呼び出す必要があります。
        /// </summary>
        /// <exception cref="ObjectDisposedException">待機ハンドルは解放済みです</exception>
        public override void Set()
        {
            // シグナル状態を設定して、継続関数を１つだけ呼び出した後、直ちに非シグナル状態にする
            IsCompleted = true;
            SetOneShotSignal();
            Reset();
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
        /// また Reset() を呼び出さない限り、ずっと待機されない状態になります。
        /// 再び、待機状態にさせるには Reset() を呼び出して下さい。
        /// </summary>
        /// <param name="result">待機した結果として設定する値</param>
        /// <exception cref="ObjectDisposedException">待機ハンドルは解放済みです</exception>
        /// <see cref="Reset"/>
        public void Set(TResult result)
        {
            // 結果を設定して基本クラスのSetを呼ぶ
            this.result = result;
            base.Set();
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
    public class ImtAwaitableFromEvent<TEventDelegate, TResult> : ImtAwaitable<TResult>
    {
        // メンバ変数定義
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
        public override bool IsCompleted
        {
            get { return isCompleted != null ? isCompleted() : completeState; }
            protected set { completeState = value; }
        }



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
            // ユーザー関数を覚えるのと、イベントハンドラを作る
            isCompleted = completed;
            register = eventRegister;
            unregister = eventUnregister;
            handler = convert(OnEventHandle);


            // 待機状態の初期化と自動リセットの値を受け取る
            IsCompleted = false;
            this.autoReset = autoReset;


            // イベントハンドラの登録
            register(handler);
        }


        /// <summary>
        /// 内部の完了状態をリセットし、再び待機可能な状態にします。
        /// しかし、コンストラクタで completed パラメータに渡している関数が
        /// 非シグナル状態を返し続けてしまう場合はリセットが出来ません。
        /// </summary>
        public void ResetCompleteState()
        {
            // もし既に非シグナル状態なら
            if (!IsCompleted)
            {
                // 既にリセット済み状態のため終了
                return;
            }


            // 状態をリセットしてハンドラを登録する
            IsCompleted = false;
            register(handler);
        }


        /// <summary>
        /// イベント または コールバック で得られた結果を取得します
        /// </summary>
        /// <returns>イベント または コールバック で得られた結果を返します</returns>
        public override TResult GetResult()
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
            try
            {
                // イベントハンドラの解除をして結果を保存
                unregister(handler);
                this.result = result;


                // 待機オブジェクトハンドラのシグナルを設定
                SetSignalWithCompleted();


                // もし自動リセットがONなら
                if (autoReset)
                {
                    // リセットする
                    ResetCompleteState();
                }
            }
            catch (Exception exception)
            {
                // もしイベントハンドラ解除や、自動リセット時のリセット状態に
                // 問題が発生したら無条件にエラー設定をして直ちにシグナルを設定する
                SetException(exception);
                SetSignalWithCompleted();
                return;
            }
        }
    }
    #endregion



    #region AwaitableUpdateBehaviourScheduler
    // TODO : もはやこれは只のオレオレTaskだな？（将来的に程よいTaskを検討）
    /// <summary>
    /// ImtAwaitableUpdateBehaviour の実行環境をスケジュールするスケジューラクラスです
    /// </summary>
    public abstract class ImtAwaitableUpdateBehaviourScheduler
    {
        /// <summary>
        /// スレッドプールを使った ImtAwaitableUpdateBehaviour のスケジュールを行うクラスです
        /// </summary>
        private class ThreadPoolUpdateBehaviourScheduler : ImtAwaitableUpdateBehaviourScheduler
        {
            // クラス変数宣言
            private static readonly WaitCallback cache = new WaitCallback(behaviour => InternalUpdate((ImtAwaitableUpdateBehaviour)behaviour));



            /// <summary>
            /// 指定された ImtAwaitableUpdateBehaviour をスレッドプール上にスケジュールします
            /// </summary>
            /// <param name="behaviour">スケジュールする ImtAwaitableUpdateBehaviour</param>
            protected internal override void ScheduleBehaviour(ImtAwaitableUpdateBehaviour behaviour)
            {
                // スレッドプールに内部更新関数をスケジュールして引数に behaviour を渡す
                ThreadPool.QueueUserWorkItem(cache, behaviour);
            }


            /// <summary>
            /// ImtAwaitableUpdateBehaviour を更新するための内部状態更新関数です
            /// </summary>
            /// <param name="behaviour">更新対象の ImtAwaitableUpdateBehaviour</param>
            private static void InternalUpdate(ImtAwaitableUpdateBehaviour behaviour)
            {
                try
                {
                    // 開始処理を呼ぶ
                    behaviour.Start();


                    // 強制停止がOFFかつ継続を返却され続ける間ループ
                    while (!forceShutdown && behaviour.Update())
                    {
                        // 休ませる
                        Thread.Sleep(0);
                    }


                    // 停止処理を呼ぶ
                    behaviour.InternalStop();
                }
                catch (Exception exception)
                {
                    // エラーが発生したことを設定してシグナルを強制的に設定する
                    behaviour.SetException(exception);
                    behaviour.SetSignalWithCompleted();
                    return;
                }
            }
        }



        /// <summary>
        /// 同期コンテキストを使った ImtAwaitableUpdateBehaviour のスケジュールを行うクラスです
        /// </summary>
        private class SynchronizationContextUpdateBehaviourScheduler : ImtAwaitableUpdateBehaviourScheduler
        {
            /// <summary>
            /// 同期コンテキストが処理するべきパラメータを保持するクラスです
            /// </summary>
            private class UpdateTargetParameter
            {
                /// <summary>
                /// 担当している同期コンテキスト
                /// </summary>
                public SynchronizationContext context;


                /// <summary>
                /// 処理するべき ImtAwaitableUpdateBehaviour
                /// </summary>
                public ImtAwaitableUpdateBehaviour behaviour;
            }



            // クラス変数宣言
            private static readonly SendOrPostCallback wakeupCache = new SendOrPostCallback(target => InternalWakeup((UpdateTargetParameter)target));
            private static readonly SendOrPostCallback updateCache = new SendOrPostCallback(target => InternalUpdate((UpdateTargetParameter)target));
            private SynchronizationContext currentContext;



            /// <summary>
            /// SynchronizationContextUpdateBehaviourScheduler のインスタンスを初期化します
            /// </summary>
            internal SynchronizationContextUpdateBehaviourScheduler()
            {
                // 現在の同期コンテキストを拾う
                currentContext = AsyncOperationManager.SynchronizationContext;
            }


            /// <summary>
            /// 指定された ImtAwaitableUpdateBehaviour を同期コンテキスト上にスケジュールします
            /// </summary>
            /// <param name="behaviour">スケジュールする ImtAwaitableUpdateBehaviour</param>
            protected internal override void ScheduleBehaviour(ImtAwaitableUpdateBehaviour behaviour)
            {
                // 更新すべきパラメータの初期化をする
                var parameter = new UpdateTargetParameter()
                {
                    context = currentContext,
                    behaviour = behaviour,
                };


                // コンテキストに起動関数をポストする
                currentContext.Post(wakeupCache, parameter);
            }


            /// <summary>
            /// ImtAwaitableUpdateBehaviour を起動するための内部起動関数です
            /// </summary>
            /// <param name="targetParameter">更新対象のパラメータ</param>
            private static void InternalWakeup(UpdateTargetParameter targetParameter)
            {
                // 更新待機オブジェクトとコンテキストの取得
                var behaviour = targetParameter.behaviour;
                var context = targetParameter.context;


                try
                {
                    // 開始処理を呼ぶ
                    behaviour.Start();


                    // 強制停止フラグはOFFまたは、継続を返却されたら
                    if (!forceShutdown && behaviour.Update())
                    {
                        // 内部更新関数をポストする
                        context.Post(updateCache, targetParameter);
                    }


                    // 停止処理を呼ぶ
                    behaviour.InternalStop();
                }
                catch (Exception exception)
                {
                    // エラーが発生したことを設定してシグナルを強制的に設定する
                    behaviour.SetException(exception);
                    behaviour.SetSignalWithCompleted();
                    return;
                }
            }


            /// <summary>
            /// ImtAwaitableUpdateBehaviour を更新するための内部状態更新関数です
            /// </summary>
            /// <param name="targetParameter">更新対象のパラメータ</param>
            private static void InternalUpdate(UpdateTargetParameter targetParameter)
            {
                // 更新待機オブジェクトとコンテキストの取得
                var behaviour = targetParameter.behaviour;
                var context = targetParameter.context;


                try
                {
                    // 強制停止フラグはOFFまたは、継続を返却されたら
                    if (!forceShutdown && behaviour.Update())
                    {
                        // ふたたび更新関数をポストする
                        context.Post(updateCache, targetParameter);
                    }


                    // 停止処理を呼ぶ
                    behaviour.InternalStop();
                }
                catch (Exception exception)
                {
                    // エラーが発生したことを設定してシグナルを強制的に設定する
                    behaviour.SetException(exception);
                    behaviour.SetSignalWithCompleted();
                    return;
                }
            }
        }



        // クラス変数宣言
        private static readonly ImtAwaitableUpdateBehaviourScheduler threadPoolScheduler = new ThreadPoolUpdateBehaviourScheduler();
        private static ImtAwaitableUpdateBehaviourScheduler currentScheduler;
        private static bool forceShutdown;



        /// <summary>
        /// デフォルトスケジューラ
        /// </summary>
        public static ImtAwaitableUpdateBehaviourScheduler DefaultScheduler => GetCurrentSynchronizationContextScheduler();


        /// <summary>
        /// 現在設定されているスケジューラ または デフォルト。
        /// このプロパティは SetScheduler() 関数によって設定された内容を返却しますが null になる場合は、デフォルトスケジューラを取り出します。
        /// </summary>
        public static ImtAwaitableUpdateBehaviourScheduler CurrentOrDefault => currentScheduler ?? DefaultScheduler;



        /// <summary>
        /// 指定された ImtAwaitableUpdateBehaviour をスケジュールします
        /// </summary>
        /// <param name="behaviour">スケジュールする ImtAwaitableUpdateBehaviour</param>
        protected internal abstract void ScheduleBehaviour(ImtAwaitableUpdateBehaviour behaviour);


        /// <summary>
        /// スレッドプールを使ったスケジューラを取得します
        /// </summary>
        /// <returns>スレッドプールを使ったスケジューラのインスタンスを返します</returns>
        public static ImtAwaitableUpdateBehaviourScheduler GetThreadPoolScheduler()
        {
            // 生成済みのスケジューラを渡す
            return threadPoolScheduler;
        }


        /// <summary>
        /// 現在の同期コンテキストを使ったスケジューラを取得します
        /// </summary>
        /// <returns>現在の同期コンテキストを使ったスケジューラのインスタンスを返します</returns>
        public static ImtAwaitableUpdateBehaviourScheduler GetCurrentSynchronizationContextScheduler()
        {
            // 同期コンテキストスケジューラを生成して返す
            return new SynchronizationContextUpdateBehaviourScheduler();
        }


        /// <summary>
        /// 現在のスケジューラを、指定されたスケジューラで設定します。
        /// </summary>
        /// <param name="scheduler">設定するスケジューラを渡しますが null が設定された場合は、内部の既定スケジューラが使われるようになります</param>
        public static void SetScheduler(ImtAwaitableUpdateBehaviourScheduler scheduler)
        {
            // 素直に受け取る
            currentScheduler = scheduler;
        }


        /// <summary>
        /// あらゆるスケジューラの動作を停止させます。
        /// また、この関数は一時的なもので、実装の変更が入る恐れがあります。
        /// </summary>
        public static void ForceShutdown()
        {
            // 強制停止フラグを立てる
            // TODO : 本来なら、スケジューラでループするのではなく、別の場所での停止ハンドリング（CancellationTokenなど）で出来るようにするべき
            forceShutdown = true;
        }
    }
    #endregion



    #region AwaitableUpdateBehaviour
    // TODO : もはやこれは只のオレオレTaskだな？（将来的に程よいTaskを検討）
    /// <summary>
    /// 自己更新が可能な、更新待機可能クラスです。
    /// </summary>
    public abstract class ImtAwaitableUpdateBehaviour : ImtAwaitable
    {
        /// <summary>
        /// 更新処理が起動中かどうか。
        /// この値は Awaitable の IsCompleted とは関係ありません
        /// </summary>
        public bool IsRunning { get; protected set; }



        /// <summary>
        /// 現在のスケジューラを用いて、状態更新を開始します。
        /// </summary>
        /// <exception cref="InvalidOperationException">この更新待機クラスは起動中です</exception>
        /// <returns>起動を開始した自身を返します</returns>
        public ImtAwaitableUpdateBehaviour Run()
        {
            // 現在設定されているスケジューラを用いてスケジュールする
            return Run(ImtAwaitableUpdateBehaviourScheduler.CurrentOrDefault);
        }


        /// <summary>
        /// 指定されたスケジューラにて、状態更新を開始します。
        /// </summary>
        /// <param name="scheduler">この、更新待機クラスが実行される環境を提供するスケジューラ</param>
        /// <exception cref="InvalidOperationException">この更新待機クラスは起動中です</exception>
        /// <returns>起動を開始した自身を返します</returns>
        public ImtAwaitableUpdateBehaviour Run(ImtAwaitableUpdateBehaviourScheduler scheduler)
        {
            // 既に起動中なら
            if (IsRunning)
            {
                // もう起動済みです
                throw new InvalidOperationException($"この更新待機クラスは起動中です");
            }


            // スケジューリングしてもらって起動状態にして自身を返す
            scheduler.ScheduleBehaviour(this);
            IsRunning = true;
            return this;
        }


        /// <summary>
        /// 現在のスケジューラを用いて、状態更新を開始します。
        /// </summary>
        /// <typeparam name="T">実際に起動する更新待機クラスの型</typeparam>
        /// <exception cref="InvalidOperationException">この更新待機クラスは起動中です</exception>
        /// <returns>起動を開始した自身を返します</returns>
        public T Run<T>() where T : ImtAwaitableUpdateBehaviour
        {
            // 通常の起動関数を叩く
            return (T)Run();
        }


        /// <summary>
        /// 指定されたスケジューラにて、状態更新を開始します。
        /// </summary>
        /// <typeparam name="T">実際に起動する更新待機クラスの型</typeparam>
        /// <param name="scheduler">この、更新待機クラスが実行される環境を提供するスケジューラ</param>
        /// <exception cref="InvalidOperationException">この更新待機クラスは起動中です</exception>
        /// <returns>起動を開始した自身を返します</returns>
        public T Run<T>(ImtAwaitableUpdateBehaviourScheduler scheduler) where T : ImtAwaitableUpdateBehaviour
        {
            // 通常の起動関数を叩く
            return (T)Run(scheduler);
        }


        /// <summary>
        /// 更新の開始処理を行います
        /// </summary>
        protected internal virtual void Start()
        {
        }


        /// <summary>
        /// 状態の更新処理を行います
        /// </summary>
        /// <returns>更新を継続する場合は true を、更新を停止する場合は false を返します</returns>
        protected internal virtual bool Update()
        {
            // 既定動作は直ちに終了
            return false;
        }


        /// <summary>
        /// 更新の終了処理を行います
        /// </summary>
        protected virtual void Stop()
        {
        }


        /// <summary>
        /// 更新の終了処理を実行します
        /// </summary>
        internal void InternalStop()
        {
            // 停止関数をたたいて停止状態にする
            Stop();
            IsRunning = false;
        }
    }



    /// <summary>
    /// 自己更新が可能な、値を返す事ができる待機可能クラスです。
    /// </summary>
    /// <typeparam name="TResult">返す値の型</typeparam>
    public abstract class ImtAwaitableUpdateBehaviour<TResult> : ImtAwaitableUpdateBehaviour, IAwaitable<TResult>
    {
        /// <summary>
        /// タスクの結果を取得します
        /// </summary>
        /// <returns>タスクの結果を返します</returns>
        public abstract TResult GetResult();


        /// <summary>
        /// この待機可能クラスの、待機オブジェクトを取得します
        /// </summary>
        /// <returns>待機オブジェクトを返します</returns>
        public new ImtAwaiter<TResult> GetAwaiter()
        {
            // 待機オブジェクトを生成して返す
            return new ImtAwaiter<TResult>(this);
        }
    }
    #endregion



    #region Task
    // TODO : もはやこれは只のオレオレTaskだな？（将来的に程よいTaskを検討）
    /// <summary>
    /// 非同期で動作するタスクを提供する待機可能クラスです
    /// </summary>
    public class ImtTask : ImtAwaitableUpdateBehaviour
    {
        // メンバ変数定義
        protected Delegate work;
        protected object status;



        /// <summary>
        /// 指定された作業を行うための ImtTask のインスタンスを初期化します。
        /// </summary>
        /// <param name="work">作業を行う関数の内容</param>
        /// <exception cref="ArgumentNullException">work が null です</exception>
        public ImtTask(Action<object> work) : this((Delegate)work, null)
        {
        }


        /// <summary>
        /// 指定された作業を行うための ImtTask のインスタンスを初期化します。
        /// </summary>
        /// <param name="work">作業を行う関数の内容</param>
        /// <param name="status">work に渡す状態オブジェクト</param>
        /// <exception cref="ArgumentNullException">work が null です</exception>
        public ImtTask(Action<object> work, object status) : this((Delegate)work, status)
        {
        }


        /// <summary>
        /// 指定された作業を行うための ImtTask のインスタンスを初期化します。
        /// </summary>
        /// <param name="work">作業を行う関数</param>
        /// <param name="status">work に渡す状態オブジェクト</param>
        /// <exception cref="ArgumentNullException">work が null です</exception>
        internal ImtTask(Delegate work, object status)
        {
            // 呼び出すべき作業関数がnullなら
            if (work == null)
            {
                // 何をすればよいのですか
                throw new ArgumentNullException(nameof(work));
            }


            // 呼び出すべき作業関数と状態を設定
            this.work = work;
            this.status = status;
        }


        /// <summary>
        /// タスクの処理を行います
        /// </summary>
        protected internal override void Start()
        {
            // タスクを処理して状態を更新
            ((Action<object>)work)(status);
        }


        /// <summary>
        /// 待機オブジェクトにシグナルを設定します
        /// </summary>
        protected override void Stop()
        {
            // シグナルを設定する
            SetSignalWithCompleted();
        }
    }



    /// <summary>
    /// 非同期で動作するタスクを提供する、値の返却が可能な待機可能クラスです
    /// </summary>
    public class ImtTask<TResult> : ImtTask, IAwaitable<TResult>
    {
        // メンバ変数定義
        private TResult result;



        /// <summary>
        /// 指定された作業を行うための ImtTask のインスタンスを初期化します。
        /// </summary>
        /// <param name="work">作業を行う関数の内容</param>
        /// <exception cref="ArgumentNullException">work が null です</exception>
        public ImtTask(Func<object, TResult> work) : base(work, null)
        {
        }


        /// <summary>
        /// 指定された作業を行うための ImtTask のインスタンスを初期化します。
        /// </summary>
        /// <param name="work">作業を行う関数の内容</param>
        /// <param name="status">work に渡す状態オブジェクト</param>
        /// <exception cref="ArgumentNullException">work が null です</exception>
        public ImtTask(Func<object, TResult> work, object status) : base(work, status)
        {
        }


        /// <summary>
        /// 現在のスケジューラを用いて、タスクを開始します。
        /// </summary>
        /// <exception cref="InvalidOperationException">このタスクは起動中です</exception>
        /// <returns>起動を開始した自身を返します</returns>
        public new ImtTask<TResult> Run()
        {
            // 現在設定されているスケジューラを用いてスケジュールする
            return Run<ImtTask<TResult>>(ImtAwaitableUpdateBehaviourScheduler.CurrentOrDefault);
        }


        /// <summary>
        /// 指定されたスケジューラにて、タスクを開始します。
        /// </summary>
        /// <param name="scheduler">この、タスクが実行される環境を提供するスケジューラ</param>
        /// <exception cref="InvalidOperationException">このタスクは起動中です</exception>
        /// <returns>起動を開始した自身を返します</returns>
        public new ImtTask<TResult> Run(ImtAwaitableUpdateBehaviourScheduler scheduler)
        {
            // 指定されたスケジューラを用いてスケジュールする
            return Run<ImtTask<TResult>>(scheduler);
        }


        /// <summary>
        /// タスクの処理を行います
        /// </summary>
        protected internal override void Start()
        {
            // 作業関数を呼ぶ
            result = ((Func<object, TResult>)work)(status);
        }


        /// <summary>
        /// この待機クラスの、待機オブジェクトを取得します
        /// </summary>
        /// <returns>待機オブジェクトを返します</returns>
        public new ImtAwaiter<TResult> GetAwaiter()
        {
            // 待機オブジェクトを生成して返す
            return new ImtAwaiter<TResult>(this);
        }


        /// <summary>
        /// タスクの結果を取得します
        /// </summary>
        /// <returns>タスクの結果を返します</returns>
        public TResult GetResult()
        {
            // 結果を返す
            return result;
        }
    }
    #endregion



    #region AwaitableUtility
    /// <summary>
    /// IAwaitable なオブジェクトを待機する時のヘルパー実装を提供します
    /// </summary>
    public class ImtAwaitableHelper
    {
        /// <summary>
        /// 全ての待機オブジェクトを待機する、待機クラスです。
        /// </summary>
        private class AwaitableWhenAll : ImtAwaitableUpdateBehaviour
        {
            // メンバ変数定義
            private ImtAwaitableHelper helper;



            /// <summary>
            /// AwaitableWhenAll のインスタンスを初期化します
            /// </summary>
            /// <param name="helper">このインスタンスを保持する ImtAwaitHelper</param>
            public AwaitableWhenAll(ImtAwaitableHelper helper)
            {
                // ヘルパーを覚えて、初期状態は完了状態にしておく
                this.helper = helper;
                IsCompleted = true;
            }


            /// <summary>
            /// 完了状態をリセットします
            /// </summary>
            public void Reset()
            {
                // 未完了状態にする
                IsCompleted = false;
            }


            /// <summary>
            /// 内部の状態更新をします。
            /// </summary>
            protected internal override bool Update()
            {
                // まだ未完了
                IsCompleted = false;


                // そもそもリストが空なら
                if (helper.awaitableList.Count == 0)
                {
                    // 完了状態にして、待機中のオブジェクトの待機を解除
                    SetSignalWithCompleted();
                    return true;
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
                    SetSignalWithCompleted();
                    return false;
                }


                // まだ継続動作する
                return true;
            }
        }



        /// <summary>
        /// いずれかの待機オブジェクトを待機する、待機クラスです
        /// </summary>
        private class AwaitableWhenAny : ImtAwaitableUpdateBehaviour<IAwaitable>
        {
            // メンバ変数定義
            private ImtAwaitableHelper helper;
            private IAwaitable firstFinishAwaitable;



            /// <summary>
            /// AwaitableWhenAny のインスタンスを初期化します
            /// </summary>
            /// <param name="helper">このインスタンスを保持する ImtAwaitHelper</param>
            public AwaitableWhenAny(ImtAwaitableHelper helper)
            {
                // ヘルパーを覚えて、初期状態は完了状態にしておく
                this.helper = helper;
                IsCompleted = true;
            }


            /// <summary>
            /// 完了状態をリセットします
            /// </summary>
            public void Reset()
            {
                // 未完了状態にする
                IsCompleted = false;
            }


            /// <summary>
            /// 内部の状態更新をします。
            /// </summary>
            protected internal override bool Update()
            {
                // まだ未完了
                IsCompleted = false;


                // そもそもリストが空なら
                if (helper.awaitableList.Count == 0)
                {
                    // 完了状態にして、待機中のオブジェクトの待機を解除
                    SetSignalWithCompleted();
                    return false;
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
                    // 継続動作する
                    return true;
                }


                // リストから完了オブジェクトを削除
                helper.awaitableList.Remove(firstFinishAwaitable);


                // 完了タスクを１つ目を見つけたので、シグナルを送る
                SetSignal();


                // もしリストが空になったのなら
                if (helper.awaitableList.Count == 0)
                {
                    // 完了状態にして終了
                    IsCompleted = true;
                    return false;
                }


                // 継続して更新する
                return true;
            }


            /// <summary>
            /// タスクの結果を取得します
            /// </summary>
            /// <returns>タスクの結果を返します</returns>
            public override IAwaitable GetResult()
            {
                // 最初に完了した待機オブジェクトを返す
                return firstFinishAwaitable;
            }
        }



        // メンバ変数定義
        private AwaitableWhenAll whenAllOperator;
        private AwaitableWhenAny whenAnyOperator;
        private List<IAwaitable> awaitableList;



        /// <summary>
        /// ImtAwaitHelper のインスタンスを初期化します
        /// </summary>
        public ImtAwaitableHelper()
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
        /// 待機ヘルパに待機オブジェクトを追加します。
        /// 既に追加済みの待機オブジェクトは無視されます。
        /// </summary>
        /// <param name="awaitables">追加する待機オブジェクトの IList</param>
        /// <exception cref="ArgumentNullException">awaitables が null です</exception>
        public void AddRange(IList<IAwaitable> awaitables)
        {
            // awaitablesがnullなら
            if (awaitables == null)
            {
                // 何を追加するんですか
                throw new ArgumentNullException(nameof(awaitables));
            }


            // リスト内をすべて回る
            foreach (var awaitable in awaitables)
            {
                // 追加する
                AddAwaitable(awaitable);
            }
        }


        /// <summary>
        /// 待機ヘルパに待機オブジェクトを追加します。
        /// 既に追加済みの待機オブジェクトは無視されます。
        /// </summary>
        /// <param name="awaitables">追加する待機オブジェクトの IList</param>
        /// <exception cref="ArgumentNullException">awaitables が null です</exception>
        public void AddRange<TResult>(IList<IAwaitable<TResult>> awaitables)
        {
            // awaitablesがnullなら
            if (awaitables == null)
            {
                // 何を追加するんですか
                throw new ArgumentNullException(nameof(awaitables));
            }


            // リスト内をすべて回る
            foreach (var awaitable in awaitables)
            {
                // 追加する
                AddAwaitable(awaitable);
            }
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
        public IAwaitable WhenAll(IList<IAwaitable> awaitables)
        {
            // もし配列の指定があるなら
            if (awaitables != null)
            {
                // 追加する
                AddRange(awaitables);
            }


            // WhenAllを実行する
            return DoWhenAll();
        }


        /// <summary>
        /// 追加された待機オブジェクトと、引数に指定された待機オブジェクト配列の中を更に追加して
        /// 全てが完了するまで待機する、待機オブジェクトを提供します。
        /// </summary>
        /// <typeparam name="TResult">待機可能クラスの返却する値の型</typeparam>
        /// <param name="awaitables">追加する待機オブジェクトの配列。追加しない場合は null の指定が可能です</param>
        /// <returns>追加された待機オブジェクトの全てを待機する、待機オブジェクトを返します</returns>
        public IAwaitable WhenAll<TResult>(IList<IAwaitable<TResult>> awaitables)
        {
            // もし配列の指定があるなら
            if (awaitables != null)
            {
                // 追加する
                AddRange(awaitables);
            }


            // WhenAllを実行する
            return DoWhenAll();
        }


        /// <summary>
        /// WhenAllを起動します
        /// </summary>
        /// <returns>WhenAllの待機可能クラスのインスタンスを返します</returns>
        private IAwaitable DoWhenAll()
        {
            // WhenAll制御が未起動状態なら
            if (!whenAllOperator.IsRunning)
            {
                // 起動する
                whenAllOperator.Reset();
                whenAllOperator.Run();
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
        public IAwaitable<IAwaitable> WhenAny(IList<IAwaitable> awaitables)
        {
            // もし配列の指定があるなら
            if (awaitables != null)
            {
                // 追加する
                AddRange(awaitables);
            }


            // WhenAnyを実行する
            return DoWhenAny();
        }


        /// <summary>
        /// 追加された待機オブジェクトのいずれかを、完了するまで待機する、待機オブジェクトを提供します。
        /// また、引数で指定された待機オブジェクトの配列内も追加します。
        /// </summary>
        /// <typeparam name="TResult">待機可能クラスの返却する値の型</typeparam>
        /// <param name="awaitables">追加する待機オブジェクトの配列。追加しない場合は null の指定が可能です</param>
        /// <returns>追加された待機オブジェクトのいずれかを、完了するまで待機する、待機オブジェクトを返します</returns>
        public IAwaitable<IAwaitable> WhenAny<TResult>(IList<IAwaitable<TResult>> awaitables)
        {
            // もし配列の指定があるなら
            if (awaitables != null)
            {
                // 追加する
                AddRange(awaitables);
            }


            // WhenAnyを実行する
            return DoWhenAny();
        }


        /// <summary>
        /// WhenAnyを起動します
        /// </summary>
        /// <returns>WhenAnyの待機可能クラスのインスタンスを返します</returns>
        private IAwaitable<IAwaitable> DoWhenAny()
        {
            // WhenAny制御が未起動状態なら
            if (!whenAnyOperator.IsRunning)
            {
                // 起動する
                whenAnyOperator.Reset();
                whenAnyOperator.Run();
            }


            // WheAny待機オブジェクトを返す
            return whenAnyOperator;
        }
    }
    #endregion



    #region 同期コンテキストヘルパ
    /// <summary>
    /// 同期コンテキストのよく利用する操作を提供するクラスです。
    /// </summary>
    public class ImtSynchronizationContextHelper
    {
        /// <summary>
        /// Action を引数に取る、キャッシュ化された SendOrPostCallback です。
        /// 単純な Action を同期コンテキストに Post する場合は、利用することをおすすめします。
        /// </summary>
        public static SendOrPostCallback CachedSendOrPostCallback { get; } = new SendOrPostCallback(_ => ((Action)_)());
    }
    #endregion



    #region Extensions
    /// <summary>
    /// AwaitableやAwaiterなどの拡張関数実装用クラスです
    /// </summary>
    public static class AwaitableAwaiterExtensions
    {
        /// <summary>
        /// 複数の IAwaitable を、すべて待機するための待機可能なインスタンスを生成します。
        /// </summary>
        /// <param name="awaitables">待機する複数の IAwaitable</param>
        /// <returns>生成された、すべての IAwaitable を待機する待機可能なインスタンスを返します</returns>
        public static IAwaitable WhenAll(this IList<IAwaitable> awaitables)
        {
            // WhenAllとして返す
            return new ImtAwaitableHelper().WhenAll(awaitables);
        }


        /// <summary>
        /// 複数の IAwaitable を、すべて待機するための待機可能なインスタンスを生成します。
        /// </summary>
        /// <typeparam name="TResult">待機可能クラスの返却する値の型</typeparam>
        /// <param name="awaitables">待機する複数の IAwaitable</param>
        /// <returns>生成された、すべての IAwaitable を待機する待機可能なインスタンスを返します</returns>
        public static IAwaitable WhenAll<TResult>(this IList<IAwaitable<TResult>> awaitables)
        {
            // WhenAllとして返す
            return new ImtAwaitableHelper().WhenAll(awaitables);
        }


        /// <summary>
        /// 複数の IAwaitable の、いずれかを待機するための待機可能なインスタンスを生成します。
        /// </summary>
        /// <param name="awaitables">待機する複数の IAwaitable</param>
        /// <returns>生成された、いずれかの IAwaitable を待機する待機可能なインスタンスを返します</returns>
        public static IAwaitable<IAwaitable> WhenAny(this IList<IAwaitable> awaitables)
        {
            // WhenAnyとして返す
            return new ImtAwaitableHelper().WhenAny(awaitables);
        }


        /// <summary>
        /// 複数の IAwaitable の、いずれかを待機するための待機可能なインスタンスを生成します。
        /// </summary>
        /// <param name="awaitables">待機する複数の IAwaitable</param>
        /// <returns>生成された、いずれかの IAwaitable を待機する待機可能なインスタンスを返します</returns>
        public static IAwaitable<IAwaitable> WhenAny<TResult>(this IList<IAwaitable<TResult>> awaitables)
        {
            // WhenAnyとして返す
            return new ImtAwaitableHelper().WhenAny(awaitables);
        }
    }
    #endregion
}