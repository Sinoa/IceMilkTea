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
    /// AssetBundleRequest クラスの拡張関数実装用クラスです
    /// </summary>
    public static partial class AssetBundleRequestExtension
    {
        /// <summary>
        /// AssetBundleRequest を待機可能にした AssetBundleRequestAwaitable のインスタンスを生成します。
        /// </summary>
        /// <typeparam name="TResult">ロードするアセットの型</typeparam>
        /// <param name="request">AssetBundleRequestAwaitable コンストラクタに渡す AssetBundleRequest</param>
        /// <returns>生成した AssetBundleRequestAwaitable のインスタンスを返します</returns>
        public static AssetBundleRequestAwaitable<TResult> ToAwaitable<TResult>(this AssetBundleRequest request) where TResult : UnityEngine.Object
        {
            // 進捗通知を受け取らないとして ToAwaitable を叩く
            return ToAwaitable<TResult>(request, null);
        }


        /// <summary>
        /// AssetBundleRequest を待機可能にした AssetBundleRequestAwaitable のインスタンスを生成します。
        /// </summary>
        /// <typeparam name="TResult">ロードするアセットの型</typeparam>
        /// <param name="request">AssetBundleRequestAwaitable コンストラクタに渡す AssetBundleRequest</param>
        /// <param name="progress">AssetBundleRequest の進捗通知を受け取る IProgress</param>
        /// <returns>生成した AssetBundleRequestAwaitable のインスタンスを返します</returns>
        public static AssetBundleRequestAwaitable<TResult> ToAwaitable<TResult>(this AssetBundleRequest request, IProgress<float> progress) where TResult : UnityEngine.Object
        {
            // インスタンスを生成して返す
            return new AssetBundleRequestAwaitable<TResult>(request, progress).Run<AssetBundleRequestAwaitable<TResult>>();
        }
    }



    /// <summary>
    /// AssetBundleRequest を待機可能にした待機可能クラスです
    /// </summary>
    /// <typeparam name="TResult">ロードするアセットの型</typeparam>
    public class AssetBundleRequestAwaitable<TResult> : ImtAwaitableUpdateBehaviour<TResult> where TResult : UnityEngine.Object
    {
        // メンバ変数定義
        private AssetBundleRequest request;
        private IProgress<float> progress;



        /// <summary>
        /// アセットのロードが完了しているかどうか
        /// </summary>
        public override bool IsCompleted { get { return request.isDone; } protected set { } }



        /// <summary>
        /// AssetBundleRequestAwaitable のインスタンスを初期化します
        /// </summary>
        /// <param name="request">待機する AssetBundleRequest</param>
        /// <param name="progress">AssetBundleRequest の進捗通知を受け取る IProgress</param>
        /// <exception cref="ArgumentNullException">request が null です</exception>
        public AssetBundleRequestAwaitable(AssetBundleRequest request, IProgress<float> progress)
        {
            // もしrequestがnullなら
            if (request == null)
            {
                // 何も出来ない
                throw new ArgumentNullException(nameof(request));
            }


            // 受け取る
            this.request = request;
            this.progress = progress;
        }


        /// <summary>
        /// 動作の開始を行います
        /// </summary>
        protected internal override void Start()
        {
            // AssetBundleRequest の完了イベントを登録する
            request.completed += OnCompleted;
        }


        /// <summary>
        /// 進捗監視を行うための状態更新を行います
        /// </summary>
        /// <returns>動作を継続する場合は true を、停止する場合は false を返します</returns>
        protected internal override bool Update()
        {
            // タスクが完了している場合は
            if (IsCompleted)
            {
                // もう更新しない
                return false;
            }


            // 進捗通知をして更新を続ける
            progress?.Report(request.progress);
            return true;
        }


        /// <summary>
        /// AssetBundleRequest の完了イベントをハンドリングします
        /// </summary>
        /// <param name="operation">通知した AssetBundleRequest</param>
        private void OnCompleted(AsyncOperation operation)
        {
            // 完了イベントを解除して、待機オブジェクトのシグナルを設定する
            request.completed -= OnCompleted;
            SetSignal();
        }


        /// <summary>
        /// 待機した結果を取得します
        /// </summary>
        /// <returns>待機した結果を返します</returns>
        public override TResult GetResult()
        {
            // ロードしたアセットを返す
            return (TResult)request.asset;
        }
    }
}