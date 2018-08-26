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
    /// 汎用的な、値の返却をしない待機可能クラスを実装をする場合には、このクラスを継承して下さい。
    /// </summary>
    public abstract class ImtAwaitable : IAwaitable
    {
        // メンバ変数定義
        private AwaiterContinuationHandler awaiterHandler;



        /// <summary>
        /// タスクが完了しているかどうか
        /// </summary>
        public abstract bool IsCompleted { get; }



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
        /// この待機可能クラスの、待機オブジェクトを取得します
        /// </summary>
        /// <returns>待機オブジェクトを返します</returns>
        public virtual ImtAwaiter GetAwaiter()
        {
            // 新しい待機オブジェクトを生成して返す
            return new ImtAwaiter(this);
        }


        /// <summary>
        /// 待機オブジェクトからの継続関数を登録します
        /// </summary>
        /// <param name="continuation">登録する継続関数</param>
        public virtual void RegisterContinuation(Action continuation)
        {
            // 待機オブジェクトハンドラに継続関数を登録する
            awaiterHandler.RegisterContinuation(continuation);
        }


        /// <summary>
        /// 登録された継続関数にシグナルを設定して、継続関数が呼び出されるようにします。
        /// </summary>
        protected virtual void SetSignal()
        {
            // 待機オブジェクトハンドラのシグナルを設定する
            awaiterHandler.SetSignal();
        }
    }



    /// <summary>
    /// 値の返却をする、待機可能なクラスを実装するための基本抽象クラスです。
    /// 汎用的な、値の返却をする待機可能クラスを実装をする場合には、このクラスを継承して下さい。
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
        public new ImtAwaiter<TResult> GetAwaiter()
        {
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
        public override bool IsCompleted => isCompleted != null ? isCompleted() : completeState;



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
            completeState = false;
            this.autoReset = autoReset;


            // イベントハンドラの登録
            register(handler);
        }


        /// <summary>
        /// 内部の完了状態をリセットし、再び待機可能な状態にします。
        /// </summary>
        public void ResetCompleteState()
        {
            // もし既に非シグナル状態なら
            if (!completeState)
            {
                // 既にリセット済み状態のため終了
                return;
            }


            // 状態をリセットしてハンドラを登録する
            completeState = false;
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
            // イベントハンドラの解除
            unregister(handler);


            // 完了状態の設定と、結果の保存をする
            completeState = true;
            this.result = result;


            // 待機オブジェクトハンドラのシグナルを設定
            SetSignal();


            // もし自動リセットがONなら
            if (autoReset)
            {
                // リセットする
                ResetCompleteState();
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


                    // 継続を返却され続ける間ループ
                    while (behaviour.Update())
                    {
                        // 休ませる
                        Thread.Sleep(0);
                    }


                    // 停止処理を呼ぶ
                    behaviour.Stop();
                }
                catch (Exception exception)
                {
                    // ImtAwaitableUpdateBehaviour にエラーをもたせて終了
                    behaviour.internalError = new AggregateException(exception);
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
                try
                {
                    // 開始処理を呼ぶ
                    targetParameter.behaviour.Start();


                    // もし継続を返却されたら
                    if (targetParameter.behaviour.Update())
                    {
                        // 内部更新関数をポストする
                        targetParameter.context.Post(updateCache, targetParameter);
                    }


                    // 停止処理を呼ぶ
                    targetParameter.behaviour.Stop();
                }
                catch (Exception exception)
                {
                    // ImtAwaitableUpdateBehaviour にエラーをもたせて終了
                    targetParameter.behaviour.internalError = new AggregateException(exception);
                    return;
                }
            }


            /// <summary>
            /// ImtAwaitableUpdateBehaviour を更新するための内部状態更新関数です
            /// </summary>
            /// <param name="targetParameter">更新対象のパラメータ</param>
            private static void InternalUpdate(UpdateTargetParameter targetParameter)
            {
                try
                {
                    // もし継続を返却されたら
                    if (targetParameter.behaviour.Update())
                    {
                        // 内部更新関数をポストする
                        targetParameter.context.Post(updateCache, targetParameter);
                    }


                    // 停止処理を呼ぶ
                    targetParameter.behaviour.Stop();
                }
                catch (Exception exception)
                {
                    // ImtAwaitableUpdateBehaviour にエラーをもたせて終了
                    targetParameter.behaviour.internalError = new AggregateException(exception);
                    return;
                }
            }
        }



        // クラス変数宣言
        private static readonly ImtAwaitableUpdateBehaviourScheduler threadPoolScheduler = new ThreadPoolUpdateBehaviourScheduler();



        /// <summary>
        /// デフォルトスケジューラ
        /// </summary>
        public static ImtAwaitableUpdateBehaviourScheduler DefaultScheduler => GetCurrentSynchronizationContextScheduler();



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
    }
    #endregion



    #region AwaitableUpdateBehaviour
    /// <summary>
    /// 自己更新が可能な、待機可能クラスです。
    /// </summary>
    public abstract class ImtAwaitableUpdateBehaviour : ImtAwaitable
    {
        // メンバ変数定義
        internal Exception internalError;



        /// <summary>
        /// ImtAwaitableUpdateBehaviour のインスタンスを既定のスケジューラを用いて初期化します
        /// </summary>
        public ImtAwaitableUpdateBehaviour() : this(ImtAwaitableUpdateBehaviourScheduler.DefaultScheduler)
        {
        }


        /// <summary>
        /// ImtAwaitableUpdateBehaviour のインスタンスを指定されたスケジューラを用いて初期化します
        /// </summary>
        /// <param name="scheduler">この ImtAwaitableUpdateBehaviour をスケジュールするスケジューラ</param>
        public ImtAwaitableUpdateBehaviour(ImtAwaitableUpdateBehaviourScheduler scheduler)
        {
            // 自分をスケジュールしてもらう
            scheduler.ScheduleBehaviour(this);
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
        protected internal virtual void Stop()
        {
        }


        /// <summary>
        /// 更新処理でエラーが発生した時のハンドリングを行います
        /// </summary>
        protected internal virtual void OnError()
        {
            // 既定動作は再スロー（元の例外は AggregateException に包まれている）
            throw internalError;
        }


        /// <summary>
        /// 登録された継続関数にシグナルを設定して、継続関数が呼び出されるようにします。
        /// </summary>
        protected new virtual void SetSignal()
        {
            // 通常の SetSignal を叩く
            base.SetSignal();


            // もしエラーが発生していたのなら
            if (internalError != null)
            {
                // エラーハンドリングを行う
                OnError();
            }
        }
    }



    /// <summary>
    /// 自己更新が可能な、値を返す事ができる待機可能クラスです。
    /// </summary>
    /// <typeparam name="TResult">返す値の型</typeparam>
    public abstract class ImtAwaitableUpdateBehaviour<TResult> : ImtAwaitableUpdateBehaviour, IAwaitable<TResult>
    {
        /// <summary>
        /// ImtAwaitableUpdateBehaviour のインスタンスを既定のスケジューラを用いて初期化します
        /// </summary>
        public ImtAwaitableUpdateBehaviour() : base()
        {
        }


        /// <summary>
        /// ImtAwaitableUpdateBehaviour のインスタンスを指定されたスケジューラを用いて初期化します
        /// </summary>
        /// <param name="scheduler">この ImtAwaitableUpdateBehaviour をスケジュールするスケジューラ</param>
        public ImtAwaitableUpdateBehaviour(ImtAwaitableUpdateBehaviourScheduler scheduler) : base(scheduler)
        {
        }


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
    /// <summary>
    /// 非同期で動作するタスクを提供する待機可能クラスです
    /// </summary>
    public class ImtTask : ImtAwaitableUpdateBehaviour
    {
        // メンバ変数定義
        private Action<object> work;
        private object status;
        private bool isFinish;



        /// <summary>
        /// タスクが完了したかどうか
        /// </summary>
        public override bool IsCompleted => isFinish;



        /// <summary>
        /// 指定された作業を行うための ImtTask のインスタンスを初期化します。
        /// </summary>
        /// <param name="work">作業を行う関数の内容</param>
        public ImtTask(Action<object> work) : this(work, null)
        {
        }


        /// <summary>
        /// 指定された作業を行うための ImtTask のインスタンスを初期化します。
        /// </summary>
        /// <param name="work">作業を行う関数の内容</param>
        /// <param name="status">work に渡す状態オブジェクト</param>
        public ImtTask(Action<object> work, object status) : base()
        {
            // 共通化された初期化を呼ぶ
            Initialize(work, status);
        }


        /// <summary>
        /// 指定された作業を行うための ImtTask のインスタンスを初期化します。
        /// </summary>
        /// <param name="work">作業を行う関数の内容</param>
        /// <param name="status">work に渡す状態オブジェクト</param>
        /// <param name="scheduler">このタスクを実行する環境をスケジュールするスケジューラ</param>
        public ImtTask(Action<object> work, object status, ImtAwaitableUpdateBehaviourScheduler scheduler) : base(scheduler)
        {
            // 共通化された初期化を呼ぶ
            Initialize(work, status);
        }


        /// <summary>
        /// 指定された作業関数と状態オブジェクトでインスタンスを初期化します
        /// </summary>
        /// <param name="work">作業を行う関数の内容</param>
        /// <param name="status">work に渡す状態オブジェクト</param>
        private void Initialize(Action<object> work, object status)
        {
            // 初期化
            this.work = work;
            this.status = status;
        }


        /// <summary>
        /// タスクの処理を行います
        /// </summary>
        protected internal override void Start()
        {
            try
            {
                // タスクを処理して状態を更新
                work(status);
            }
            catch (Exception)
            {
                // タスクを完了とシグナルの設定をしてエラー内容を覚える
                isFinish = true;
                SetSignal();
                throw;
            }
            finally
            {
                // タスクは完了した
                isFinish = true;
            }
        }


        /// <summary>
        /// 直ちに終了します
        /// </summary>
        /// <returns>常に false を返します</returns>
        protected internal override bool Update()
        {
            // 継続せずすぐに終了
            return false;
        }


        /// <summary>
        /// 待機オブジェクトにシグナルを設定します
        /// </summary>
        protected internal override void Stop()
        {
            // シグナルを設定する
            SetSignal();
        }
    }



    /// <summary>
    /// 非同期で動作するタスクを提供する、値の返却が可能な待機可能クラスです
    /// </summary>
    public class ImtTask<TResult> : ImtAwaitableUpdateBehaviour<TResult>
    {
        // メンバ変数定義
        private Func<object, TResult> work;
        private object status;
        private bool isFinish;
        private TResult result;



        /// <summary>
        /// タスクが完了したかどうか
        /// </summary>
        public override bool IsCompleted => isFinish;



        /// <summary>
        /// 指定された作業を行うための ImtTask のインスタンスを初期化します。
        /// </summary>
        /// <param name="work">作業を行う関数の内容</param>
        public ImtTask(Func<object, TResult> work) : this(work, null)
        {
        }


        /// <summary>
        /// 指定された作業を行うための ImtTask のインスタンスを初期化します。
        /// </summary>
        /// <param name="work">作業を行う関数の内容</param>
        /// <param name="status">work に渡す状態オブジェクト</param>
        public ImtTask(Func<object, TResult> work, object status) : base()
        {
            // 共通化された初期化を呼ぶ
            Initialize(work, status);
        }


        /// <summary>
        /// 指定された作業を行うための ImtTask のインスタンスを初期化します。
        /// </summary>
        /// <param name="work">作業を行う関数の内容</param>
        /// <param name="status">work に渡す状態オブジェクト</param>
        /// <param name="scheduler">このタスクを実行する環境をスケジュールするスケジューラ</param>
        public ImtTask(Func<object, TResult> work, object status, ImtAwaitableUpdateBehaviourScheduler scheduler) : base(scheduler)
        {
            // 共通化された初期化を呼ぶ
            Initialize(work, status);
        }


        /// <summary>
        /// 指定された作業関数と状態オブジェクトでインスタンスを初期化します
        /// </summary>
        /// <param name="work">作業を行う関数の内容</param>
        /// <param name="status">work に渡す状態オブジェクト</param>
        private void Initialize(Func<object, TResult> work, object status)
        {
            // 初期化
            this.work = work;
            this.status = status;
        }


        /// <summary>
        /// タスクの処理を行います
        /// </summary>
        protected internal override void Start()
        {
            try
            {
                // タスクを処理して状態を更新
                result = work(status);
            }
            catch (Exception)
            {
                // タスクを完了とシグナルの設定をしてエラー内容を覚える
                isFinish = true;
                SetSignal();
                throw;
            }
            finally
            {
                // タスクは完了した
                isFinish = true;
            }
        }


        /// <summary>
        /// 直ちに終了します
        /// </summary>
        /// <returns>常に false を返します</returns>
        protected internal override bool Update()
        {
            // 継続せずすぐに終了
            return false;
        }


        /// <summary>
        /// 待機オブジェクトにシグナルを設定します
        /// </summary>
        protected internal override void Stop()
        {
            // シグナルを設定する
            SetSignal();
        }


        /// <summary>
        /// タスクの結果を取得します
        /// </summary>
        /// <returns>タスクの結果を返します</returns>
        public override TResult GetResult()
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
        private class AwaitableWhenAll : IAwaitable
        {
            // メンバ変数定義
            private AwaiterContinuationHandler awaiterHandler;
            private SynchronizationContext currentContext;
            private ImtAwaitableHelper helper;
            private Action update;



            /// <summary>
            /// タスクが完了したかどうか
            /// </summary>
            public bool IsCompleted { get; private set; }



            /// <summary>
            /// AwaitableWhenAll のインスタンスを初期化します
            /// </summary>
            /// <param name="helper">このインスタンスを保持する ImtAwaitHelper</param>
            public AwaitableWhenAll(ImtAwaitableHelper helper)
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
                currentContext.Post(ImtSynchronizationContextHelper.CachedSendOrPostCallback, update);
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
            // メンバ変数定義
            private AwaiterContinuationHandler awaiterHandler;
            private SynchronizationContext currentContext;
            private ImtAwaitableHelper helper;
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
            public AwaitableWhenAny(ImtAwaitableHelper helper)
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
                    currentContext.Post(ImtSynchronizationContextHelper.CachedSendOrPostCallback, update);
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
                currentContext.Post(ImtSynchronizationContextHelper.CachedSendOrPostCallback, update);
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
        /// <param name="awaitable">追加する待機オブジェクトの IList</param>
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
        /// <param name="awaitable">追加する待機オブジェクトの IList</param>
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
        public IAwaitable<IAwaitable> WhenAny(IList<IAwaitable> awaitables)
        {
            // もし配列の指定があるなら
            if (awaitables != null)
            {
                // 追加する
                AddRange(awaitables);
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