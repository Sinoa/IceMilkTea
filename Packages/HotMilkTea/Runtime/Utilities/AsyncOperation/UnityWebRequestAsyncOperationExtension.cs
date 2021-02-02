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
using UnityEngine.Networking;

namespace IceMilkTea.Core
{
    #region Extension Define
    /// <summary>
    /// UnityWebRequestAsyncOperation クラスの拡張関数実装用のクラスです
    /// </summary>
    public static class UnityWebRequestAsyncOperationExtension
    {
        /// <summary>
        /// UnityWebRequestAsyncOperation クラスの待機オブジェクトを取得します
        /// </summary>
        /// <param name="operation">待機する UnityWebRequestAsyncOperation</param>
        /// <returns>UnityWebRequestAsyncOperation クラスの待機オブジェクトを返します</returns>
        public static UnityWebRequestAsyncOperationAwaiter GetAwaiter(this UnityWebRequestAsyncOperation operation)
        {
            // UnityWebRequestAsyncOperationAwaiter を生成して返す
            return new UnityWebRequestAsyncOperationAwaiter(operation);
        }


        /// <summary>
        /// UnityWebRequestAsyncOperation を FireAndForgetするために警告を潰す関数です
        /// </summary>
        /// <param name="operation">FireAndForgetする UnityWebRequestAsyncOperation</param>
        public static void Forget(this UnityWebRequestAsyncOperation operation)
        {
            // 無の境地
        }


        /// <summary>
        /// UnityWebRequestAsyncOperation を待機可能にした UnityWebRequestAsyncOperationAwaitable のインスタンスを生成します。
        /// </summary>
        /// <param name="operation">UnityWebRequestAsyncOperationAwaitable コンストラクタに渡す UnityWebRequestAsyncOperation</param>
        /// <returns>生成された UnityWebRequestAsyncOperationAwaitable のインスタンスを返します</returns>
        public static UnityWebRequestAsyncOperationAwaitable ToAwaitable(this UnityWebRequestAsyncOperation operation)
        {
            // インスタンスを生成して返す
            return ToAwaitable(operation, null);
        }


        /// <summary>
        /// UnityWebRequestAsyncOperation を待機可能にした UnityWebRequestAsyncOperationAwaitable のインスタンスを生成します。
        /// </summary>
        /// <param name="operation">UnityWebRequestAsyncOperationAwaitable コンストラクタに渡す UnityWebRequestAsyncOperation</param>
        /// <param name="progress">UnityWebRequestAsyncOperation の進捗通知を受ける IProgress</param>
        /// <returns>生成された UnityWebRequestAsyncOperationAwaitable のインスタンスを返します</returns>
        public static UnityWebRequestAsyncOperationAwaitable ToAwaitable(this UnityWebRequestAsyncOperation operation, IProgress<float> progress)
        {
            // インスタンスを生成して返す
            return new UnityWebRequestAsyncOperationAwaitable(operation, progress).Run<UnityWebRequestAsyncOperationAwaitable>();
        }
    }
    #endregion



    #region Awaiter
    /// <summary>
    /// UnityWebRequestAsyncOperation の待機構造体です
    /// </summary>
    public struct UnityWebRequestAsyncOperationAwaiter : INotifyCompletion
    {
        // 定数定義
        private const string DefaultErrorMessage = "不明な'UniWebRequet'のエラーが発生しました";

        // メンバ変数定義
        private UnityWebRequestAsyncOperation operation;



        /// <summary>
        /// タスクが完了したかどうか
        /// </summary>
        public bool IsCompleted => operation.isDone;



        /// <summary>
        /// UnityWebRequestAsyncOperationAwaiter のインスタンスを初期化します
        /// </summary>
        /// <param name="operation">待機する UnityWebRequestAsyncOperation</param>
        public UnityWebRequestAsyncOperationAwaiter(UnityWebRequestAsyncOperation operation)
        {
            // 覚える
            this.operation = operation;
        }


        /// <summary>
        /// タスクの完了処理を行います
        /// </summary>
        /// <param name="continuation">タスクの継続関数</param>
        public void OnCompleted(Action continuation)
        {
            // すでにタスクが完了しているのなら
            if (IsCompleted)
            {
                // 直ちに継続関数を呼ぶ
                continuation();
                return;
            }


            // コールバックのタイミングで直ちに継続関数を叩くようにする
            operation.completed += _ => continuation();
        }


        /// <summary>
        /// タスクの結果を取得します
        /// </summary>
        /// <returns>タスクの結果を返します</returns>
        public UnityWebRequest GetResult()
        {
            // 結果を受け取る
            var result = operation.webRequest;


            // もしネットワークエラーが発生していたのなら
            if (result.isNetworkError)
            {
                // エラー文字列を受け取って例外を吐く（NetworkならIOエラーでええか）
                throw new System.IO.IOException(result.error ?? DefaultErrorMessage);
            }


            // もしHttpエラーが発生していたのなら
            if (result.isHttpError)
            {
                // エラー文字列を受け取って例外を吐く（HTTPならWebExceptionでええか）
                throw new System.Net.WebException(result.error ?? DefaultErrorMessage);
            }


            // 結果を返す
            return result;
        }
    }
    #endregion



    #region Awaitable
    /// <summary>
    /// UnityWebRequestAsyncOperation を待機可能にした待機可能クラスです。
    /// </summary>
    public class UnityWebRequestAsyncOperationAwaitable : ImtAwaitableUpdateBehaviour<UnityWebRequest>
    {
        // 定数定義
        private const string DefaultErrorMessage = "不明な'UniWebRequet'のエラーが発生しました";

        // メンバ変数定義
        private UnityWebRequestAsyncOperation operation;
        private IProgress<float> progress;



        /// <summary>
        /// UnityWebRequestAsyncOperation のタスクが完了したかどうか
        /// </summary>
        public override bool IsCompleted { get { return operation.isDone; } protected set { } }



        /// <summary>
        /// UnityWebRequestAsyncOperationAwaitable のインスタンスを初期化します
        /// </summary>
        /// <param name="operation">待機可能にしたい UnityWebRequestAsyncOperation</param>
        /// <param name="progress">UnityWebRequestAsyncOperation の進捗通知を受け取る IProgress</param>
        public UnityWebRequestAsyncOperationAwaitable(UnityWebRequestAsyncOperation operation, IProgress<float> progress)
        {
            // もしoperationがnullなら
            if (operation == null)
            {
                // 何も出来ない
                throw new ArgumentNullException(nameof(operation));
            }


            // パラメータを受け取る
            this.operation = operation;
            this.progress = progress;
        }


        /// <summary>
        /// 動作を開始します
        /// </summary>
        protected override void Start()
        {
            // 完了イベントを登録する
            operation.completed += OnCompleted;
        }


        /// <summary>
        /// 進捗監視をするための状態を更新します
        /// </summary>
        /// <returns></returns>
        protected override bool Update()
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
        /// UnityWebRequestAsyncOperation の待機した結果を取得します
        /// </summary>
        /// <returns>待機した結果を返します</returns>
        public override UnityWebRequest GetResult()
        {
            // 結果を受け取る
            var result = operation.webRequest;


            // もしネットワークエラーが発生していたのなら
            if (result.isNetworkError)
            {
                // エラー文字列を受け取って例外を吐く（NetworkならIOエラーでええか）
                throw new System.IO.IOException(result.error ?? DefaultErrorMessage);
            }


            // もしHttpエラーが発生していたのなら
            if (result.isHttpError)
            {
                // エラー文字列を受け取って例外を吐く（HTTPならWebExceptionでええか）
                throw new System.Net.WebException(result.error ?? DefaultErrorMessage);
            }


            // 結果を返す
            return result;
        }


        /// <summary>
        /// UnityWebRequestAsyncOperation の完了イベントをハンドリングします
        /// </summary>
        /// <param name="sender">呼び出した UnityWebRequestAsyncOperation</param>
        private void OnCompleted(AsyncOperation sender)
        {
            // 完了イベントを解除して、待機オブジェクトのシグナルを設定する
            operation.completed -= OnCompleted;
            SetSignal();
        }
    }
    #endregion
}