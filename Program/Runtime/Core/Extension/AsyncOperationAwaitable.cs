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
using System.Threading;
using UnityEngine;

namespace IceMilkTea.Core
{
    /// <summary>
    /// AsyncOperation クラスの拡張関数実装用クラスです
    /// </summary>
    public static partial class AsyncOperationExtension
    {
        /// <summary>
        /// AsyncOperation の待機可能にした AsyncOperationAwaitable クラスのインスタンスを生成します。
        /// </summary>
        /// <param name="operation">AsyncOperationAwaitable コンストラクタに渡す AsyncOperation</param>
        /// <returns>生成された AsyncOperationAwaitable のインスタンスを返します</returns>
        public static AsyncOperationAwaitable ToAwaitable(this AsyncOperation operation)
        {
            // AsyncOperationの待機可能クラスのインスタンスを生成して返す
            return new AsyncOperationAwaitable(operation, null);
        }


        /// <summary>
        /// AsyncOperation の待機可能にした AsyncOperationAwaitable クラスのインスタンスを生成します。
        /// </summary>
        /// <param name="operation">AsyncOperationAwaitable コンストラクタに渡す AsyncOperation</param>
        /// <param name="progress">AsyncOperation の進捗通知を受け取る IProgress</param>
        /// <returns>生成された AsyncOperationAwaitable のインスタンスを返します</returns>
        public static AsyncOperationAwaitable ToAwaitable(this AsyncOperation operation, IProgress<float> progress)
        {
            // AsyncOperationの待機可能クラスのインスタンスを生成して返す
            return new AsyncOperationAwaitable(operation, progress);
        }
    }



    /// <summary>
    /// AsyncOperation を待機可能にした待機可能クラスです。
    /// </summary>
    public class AsyncOperationAwaitable : IAwaitable
    {
        // メンバ変数定義
        private AwaiterContinuationHandler awaitHandler;
        private AsyncOperation operation;
        private IProgress<float> progress;
        private SynchronizationContext context;
        private Action update;



        /// <summary>
        /// AsyncOperation のタスクが完了したかどうか
        /// </summary>
        public bool IsCompleted => operation.isDone;



        /// <summary>
        /// AsyncOperationAwaitable のインスタンスを初期化します
        /// </summary>
        /// <param name="operation">待機可能にしたい AsyncOperation</param>
        /// <param name="progress">AsyncOperation の進捗通知を受ける IProgress</param>
        /// <exception cref="ArgumentNullException">operation が null です</exception>
        public AsyncOperationAwaitable(AsyncOperation operation, IProgress<float> progress)
        {
            // operationがnullなら
            if (operation == null)
            {
                // なにも出来ない
                throw new ArgumentNullException(nameof(operation));
            }


            // 待機オブジェクトハンドラの生成
            awaitHandler = new AwaiterContinuationHandler();


            // 引数を受け取る
            this.operation = operation;
            this.progress = progress;


            // 定期更新用同期コンテキストの取得と更新関数の取得
            context = System.ComponentModel.AsyncOperationManager.SynchronizationContext;
            update = Update;
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
        /// 待機オブジェクトの継続関数を登録します
        /// </summary>
        /// <param name="continuation">登録する継続関数</param>
        public void RegisterContinuation(Action continuation)
        {
            // AsyncOperationの完了イベントを登録して継続関数を登録する
            operation.completed += OnComplete;
            awaitHandler.RegisterContinuation(continuation);


            // 同期コンテキストに定期更新用関数を送る
            PostUpdate();
        }


        /// <summary>
        /// AsyncOperation の完了イベントをハンドリングします
        /// </summary>
        /// <param name="operation">イベントを呼び出したAsyncOperation</param>
        private void OnComplete(AsyncOperation operation)
        {
            // 完了イベントの登録を解除して、継続関数のシグナルを設定する
            operation.completed -= OnComplete;
            awaitHandler.SetSignal();
        }


        /// <summary>
        /// AsyncOperation の進捗を監視するための、状態更新を行います
        /// </summary>
        private void Update()
        {
            // 進捗の通知をする
            progress?.Report(operation.progress);


            // タスクが完了しているのなら
            if (IsCompleted)
            {
                // もう、更新はしない
                return;
            }


            // まだ更新関数を送る
            PostUpdate();
        }


        /// <summary>
        /// 更新関数を同期コンテキストにポストします
        /// </summary>
        private void PostUpdate()
        {
            // 更新関数を送る
            context.Post(ImtSynchronizationContextHelper.CachedSendOrPostCallback, update);
        }
    }
}