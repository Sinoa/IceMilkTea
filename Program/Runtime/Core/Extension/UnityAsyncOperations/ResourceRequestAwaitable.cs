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
    public class ResourceRequestAwaitable<TResult> : ImtAwaitableUpdateBehaviour<TResult> where TResult : UnityEngine.Object
    {
        // メンバ変数定義
        private ResourceRequest request;
        private IProgress<float> progress;



        /// <summary>
        /// ResourceRequest が完了しているかどうか
        /// </summary>
        public override bool IsCompleted => request.isDone;



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


            // 引数を受け取る
            this.request = request;
            this.progress = progress;
        }


        /// <summary>
        /// 動作を開始します
        /// </summary>
        protected internal override void Start()
        {
            // 完了イベントを登録する
            request.completed += OnCompleted;
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
            progress?.Report(request.progress);
            return true;
        }


        /// <summary>
        /// ロード結果を取得します
        /// </summary>
        /// <returns>ロード結果を返します</returns>
        public override TResult GetResult()
        {
            // そのまま結果を帰す
            return (TResult)request.asset;
        }


        /// <summary>
        /// ResourceRequest の完了イベントをハンドリングします
        /// </summary>
        /// <param name="operation">完了した ResourceRequest</param>
        private void OnCompleted(AsyncOperation operation)
        {
            // イベントを解除して、シグナルを設定する
            request.completed -= OnCompleted;
            SetSignal();
        }
    }
}