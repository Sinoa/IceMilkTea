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
            public SynchronizationContext Context;

            /// <summary>
            /// 登録された継続関数
            /// </summary>
            public Action continuation;
        }



        // 定数定義
        private const int DefaultBufferSize = 8;



        // 読み取り専用構造体変数宣言
        private static readonly SendOrPostCallback cache = new SendOrPostCallback(_ => ((Action)_)());



        // メンバ変数定義
        private Handler[] handlers;
        private int handleCount;



        /// <summary>
        /// AwaiterContinuationHandler のインスタンスを初期化します。
        /// </summary>
        public AwaiterContinuationHandler() : this(DefaultBufferSize)
        {
        }


        /// <summary>
        /// AwaiterContinuationHandler のインスタンスを指定された容量で初期化します。
        /// </summary>
        /// <param name="capacity">登録する継続関数の初期容量</param>
        public AwaiterContinuationHandler(int capacity)
        {
            // ハンドラ配列の初期化
            handlers = new Handler[capacity];
            handleCount = 0;
        }


        /// <summary>
        /// Awaiter の継続関数を登録します。
        /// 登録した継続関数は SetSignal() 関数にて継続を行うことが可能です。
        /// </summary>
        /// <param name="continuation">登録する継続関数</param>
        public void RegisterContinuation(Action continuation)
        {
            // 配列をロック
            lock (handlers)
            {
                // もしハンドラ配列の長さが登録ハンドル数に到達しているのなら
                if (handlers.Length == handleCount)
                {
                    // 倍のサイズで新しい容量を確保する
                    EnsureCapacity(handlers.Length * 2);
                }


                // 継続関数をハンドラ配列に追加する
                handlers[handleCount++] = new Handler()
                {
                    // ハンドラ構造体の初期化（このタイミングで同期コンテキストを拾う）
                    Context = AsyncOperationManager.SynchronizationContext,
                    continuation = continuation,
                };
            }
        }


        /// <summary>
        /// 登録された継続関数を、登録時の同期コンテキストを通じて呼び出されるようにします。
        /// また、一度シグナルした継続処理の参照は消失するため、再度 Awaite するには、改めて継続関数を登録する必要があります。
        /// </summary>
        public void SetSignal()
        {
            // 配列をロック
            lock (handlers)
            {
                // 登録されたハンドラの数分回る
                for (int i = 0; i < handleCount; ++i)
                {
                    // 同期コンテキストに継続関数をポストして（実際はキャッシュされたSendOrPostCallbackを通す）参照を忘れる
                    handlers[i].Context.Post(cache, handlers[i].continuation);
                    handlers[i].Context = null;
                    handlers[i].continuation = null;
                }


                // ハンドラを空にする
                handleCount = 0;
            }
        }


        /// <summary>
        /// ハンドラ配列を指定された容量で新しく確保します
        /// </summary>
        /// <param name="newCapacity">新しいハンドラの容量（既定値より小さい値の場合は既定値に設定されます）</param>
        private void EnsureCapacity(int newCapacity)
        {
            // 既定値より小さいなら
            if (newCapacity < DefaultBufferSize)
            {
                // 規定値に強制的に設定する
                newCapacity = DefaultBufferSize;
            }


            // 新しい容量が、既に使用済み容量未満の場合なら
            if (newCapacity < handleCount)
            {
                // 新しく確保できない事を吐く
                throw new ArgumentException("指定された新しい容量が、使用済み容量未満です", nameof(newCapacity));
            }


            // もし新しい容量が、現在の容量と同じなら
            if (newCapacity == handlers.Length)
            {
                // 既に同サイズの容量なので何もせず終了
                return;
            }


            // 新しい配列を確保して、旧配列から使用済みデータをコピー後、参照を新しい配列に設定する
            var newHandlers = new Handler[newCapacity];
            Array.Copy(handlers, newHandlers, handleCount);
            handlers = newHandlers;
        }
    }
    #endregion
}