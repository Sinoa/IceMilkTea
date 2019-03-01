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
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

namespace IceMilkTea.Core
{
    #region Extension Define
    /// <summary>
    /// AsyncOperation クラスの拡張関数実装用クラスです
    /// </summary>
    public static class AsyncOperationExtension
    {
        /// <summary>
        /// AsyncOperation の待機可能なオブジェクトを取得します
        /// </summary>
        /// <param name="asyncOperation">待機する対象の AsyncOperation</param>
        /// <returns>AsyncOperation の待機可能なオブジェクトを返します</returns>
        public static AsyncOperationAwaiter GetAwaiter(this AsyncOperation asyncOperation)
        {
            // AsyncOperationのAwaiterのインスタンスを返す
            return new AsyncOperationAwaiter(asyncOperation);
        }


        /// <summary>
        /// AsyncOperationをFire And Forgetします。await可能だという警告を潰すことができます
        /// </summary>
        /// <param name="asyncOperation">Fire and Forgetする対象のAsyncOperation</param>
        public static void Forget(this AsyncOperation asyncOperation)
        {
            // Fire And Forget
        }


        /// <summary>
        /// AsyncOperation の待機可能にした AsyncOperationAwaitable クラスのインスタンスを生成します。
        /// </summary>
        /// <param name="operation">AsyncOperationAwaitable コンストラクタに渡す AsyncOperation</param>
        /// <returns>生成された AsyncOperationAwaitable のインスタンスを返します</returns>
        public static AsyncOperationAwaitable ToAwaitable(this AsyncOperation operation)
        {
            // AsyncOperationの待機可能クラスのインスタンスを生成して返す
            return ToAwaitable(operation, null);
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
            return new AsyncOperationAwaitable(operation, progress).Run<AsyncOperationAwaitable>();
        }
    }
    #endregion



    #region Awaiter
    /// <summary>
    /// Unity の AsyncOperation による非同期制御同期オブジェクトを async - await に対応させた Awaiter 構造体です
    /// </summary>
    public struct AsyncOperationAwaiter : INotifyCompletion
    {
        // メンバ変数定義
        private AsyncOperation asyncOperation;



        /// <summary>
        /// タスクが完了しているかどうか
        /// </summary>
        public bool IsCompleted => asyncOperation.isDone;



        /// <summary>
        /// AsyncOperationAwaiter オブジェクトの初期化を行います
        /// </summary>
        /// <param name="operation">待機処理をする対象の AsyncOperation インスタンス</param>
        public AsyncOperationAwaiter(AsyncOperation operation)
        {
            // 覚える
            asyncOperation = operation;
        }


        /// <summary>
        /// タスクの完了処理を行います
        /// </summary>
        /// <param name="continuation">タスク完了後の継続処理を行う対象の関数</param>
        public void OnCompleted(Action continuation)
        {
            // 既に完了状態で呼び出されたのなら
            if (IsCompleted)
            {
                // 直ちに後続処理を叩く
                continuation();
                return;
            }


            // コールバックのタイミングで直ちに継続関数を叩くようにする
            asyncOperation.completed += _ => continuation();
        }


        /// <summary>
        /// タスクの完了を待機し、結果を同期的に取得する処理を実行します
        /// </summary>
        public void GetResult()
        {
        }
    }
    #endregion



    #region Awaitable
    /// <summary>
    /// AsyncOperation を待機可能にした待機可能クラスです。
    /// </summary>
    public class AsyncOperationAwaitable : ImtAwaitableUpdateBehaviour
    {
        // メンバ変数定義
        private AsyncOperation operation;
        private IProgress<float> progress;



        /// <summary>
        /// AsyncOperation のタスクが完了したかどうか
        /// </summary>
        public override bool IsCompleted { get { return operation.isDone; } protected set { } }



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


            // 引数を受け取る
            this.operation = operation;
            this.progress = progress;
        }


        /// <summary>
        /// 動作を開始します
        /// </summary>
        protected internal override void Start()
        {
            // 完了イベントを登録する
            operation.completed += OnCompleted;
        }


        /// <summary>
        /// 進捗監視をするための状態を更新します
        /// </summary>
        /// <returns></returns>
        protected internal override bool Update()
        {
            // タスクが完了しているなら
            if (IsCompleted)
            {
                // 更新はもうしない
                return false;
            }


            // 進捗通知をして更新を継続する
            progress?.Report(operation.progress);
            return true;
        }


        /// <summary>
        /// 完了イベントをハンドリングします
        /// </summary>
        /// <param name="operation">イベントを通知したオブジェクト</param>
        private void OnCompleted(AsyncOperation operation)
        {
            // 完了イベントを解除して、待機オブジェクトハンドラのシグナルを設定する
            this.operation.completed -= OnCompleted;
            SetSignal();
        }
    }
    #endregion
}