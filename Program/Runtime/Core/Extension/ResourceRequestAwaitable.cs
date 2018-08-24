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
    /// ResourceRequest クラスの拡張関数実装用クラスです
    /// </summary>
    public static partial class ResourceRequestExtension
    {
        /// <summary>
        /// ResourceRequest を待機可能にした ResourceRequestAwaitable クラスのインスタンスを生成します。
        /// </summary>
        /// <typeparam name="TResult">ResourceRequest のロード結果の型</typeparam>
        /// <param name="request">ResourceRequestAwaitable コンストラクタに渡す ResourceRequest</param>
        /// <returns>生成された ResourceRequestAwaitable のインスタンスを返します</returns>
        public static ResourceRequestAwaitable<TResult> ToAwaitable<TResult>(this ResourceRequest request) where TResult : UnityEngine.Object
        {
            // インスタンスを生成して返す
            return new ResourceRequestAwaitable<TResult>(request, null);
        }


        /// <summary>
        /// ResourceRequest を待機可能にした ResourceRequestAwaitable クラスのインスタンスを生成します。
        /// </summary>
        /// <typeparam name="TResult">ResourceRequest のロード結果の型</typeparam>
        /// <param name="request">ResourceRequestAwaitable コンストラクタに渡す ResourceRequest</param>
        /// <param name="progress">ResourceRequest の進捗通知を受け取る IProgress</param>
        /// <returns>生成された ResourceRequestAwaitable のインスタンスを返します</returns>
        public static ResourceRequestAwaitable<TResult> ToAwaitable<TResult>(this ResourceRequest request, IProgress<float> progress) where TResult : UnityEngine.Object
        {
            // インスタンスを生成して返す
            return new ResourceRequestAwaitable<TResult>(request, progress);
        }
    }



    /// <summary>
    /// ResourceRequest を待機可能にした待機可能クラスです。
    /// </summary>
    /// <typeparam name="TResult">ResourceRequest のロード結果の型</typeparam>
    public class ResourceRequestAwaitable<TResult> : IAwaitable<TResult> where TResult : UnityEngine.Object
    {
        // メンバ変数定義
        private AwaiterContinuationHandler awaiterHandler;
        private ResourceRequest request;
        private IProgress<float> progress;
        private SynchronizationContext context;
        private Action update;



        /// <summary>
        /// ResourceRequest が完了しているかどうか
        /// </summary>
        public bool IsCompleted => request.isDone;



        /// <summary>
        /// ResourceRequestAwaitable のインスタンスを初期化します
        /// </summary>
        /// <param name="request">待機可能にしたい ResourceRequest</param>
        /// <param name="progress">ResourceRequest の進捗通知を受け取る IProgress</param>
        /// <exception cref="ArgumentNullException">request が null です</exception>
        public ResourceRequestAwaitable(ResourceRequest request, IProgress<float> progress)
        {
            // もしrequestがnullなら
            if (request == null)
            {
                // なにも出来ない
                throw new ArgumentNullException(nameof(request));
            }


            // 待機オブジェクトハンドラの生成
            awaiterHandler = new AwaiterContinuationHandler();


            // 引数を受け取る
            this.request = request;
            this.progress = progress;


            // 定期更新用同期コンテキストの取得と、更新関数を覚える
            context = System.ComponentModel.AsyncOperationManager.SynchronizationContext;
            update = Update;
        }


        /// <summary>
        /// この待機可能クラスの待機オブジェクトを取得します
        /// </summary>
        /// <returns>待機オブジェクトを返します</returns>
        public ImtAwaiter<TResult> GetAwaiter()
        {
            // 待機オブジェクトを生成して返す
            return new ImtAwaiter<TResult>(this);
        }


        /// <summary>
        /// この待機可能クラスの待機オブジェクトを取得します
        /// </summary>
        /// <returns>待機オブジェクトを返します</returns>
        ImtAwaiter IAwaitable.GetAwaiter()
        {
            return new ImtAwaiter(this);
        }


        /// <summary>
        /// ロード結果を取得します
        /// </summary>
        /// <returns>ロード結果を返します</returns>
        public TResult GetResult()
        {
            // そのまま結果を帰す
            return (TResult)request.asset;
        }


        /// <summary>
        /// 待機オブジェクトの継続関数を登録します
        /// </summary>
        /// <param name="continuation">登録する継続関数</param>
        public void RegisterContinuation(Action continuation)
        {
            // 完了イベントを登録して、継続関数も登録
            request.completed += OnCompleted;
            awaiterHandler.RegisterContinuation(continuation);


            // 更新関数を送る
            PostUpdate();
        }


        /// <summary>
        /// ResourceRequest の完了イベントをハンドリングします
        /// </summary>
        /// <param name="operation">完了した ResourceRequest</param>
        private void OnCompleted(AsyncOperation operation)
        {
            // イベントを解除して、シグナルを設定する
            request.completed -= OnCompleted;
            awaiterHandler.SetSignal();
        }


        /// <summary>
        /// ResourceRequest の進捗を監視するための状態を更新します
        /// </summary>
        private void Update()
        {
            // もしタスクが完了しているなら
            if (IsCompleted)
            {
                // もう終わり
                return;
            }


            // 進捗を通知して、再び更新関数をポストする
            progress?.Report(request.progress);
            PostUpdate();
        }


        /// <summary>
        /// 更新関数を同期コンテキストにポストします
        /// </summary>
        private void PostUpdate()
        {
            // 更新関数をポストする
            context.Post(ImtSynchronizationContextHelper.CachedSendOrPostCallback, update);
        }
    }
}