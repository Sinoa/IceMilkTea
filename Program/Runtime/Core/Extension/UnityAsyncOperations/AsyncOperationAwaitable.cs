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
            return new AsyncOperationAwaitable(operation, progress);
        }
    }



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
        public override bool IsCompleted => operation.isDone;



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
}