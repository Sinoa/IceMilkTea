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
    /// AssetBundleCreateRequest クラスの拡張関数実装用クラスです
    /// </summary>
    public static class AssetBundleCreateRequestExtension
    {
        /// <summary>
        /// AssetBundleCreateRequest の待機オブジェクトを取得します
        /// </summary>
        /// <param name="request">待機する AssetBundleCreateRequest</param>
        /// <returns>AssetBundleCreateRequest の待機オブジェクトを返します</returns>
        public static AssetBundleCreateRequestAwaiter GetAwaiter(this AssetBundleCreateRequest request)
        {
            // AssetBundleCreateRequest の待機オブジェクトを生成して返す
            return new AssetBundleCreateRequestAwaiter(request);
        }


        /// <summary>
        /// AssetBundleCreateRequest を待機可能にした AssetBundleCreateRequestAwaitable のインスタンスを生成します
        /// </summary>
        /// <param name="request">AssetBundleCreateRequestAwaitable のコンストラクタに渡す AssetBundleCreateRequest</param>
        /// <returns>生成された AssetBundleCreateRequestAwaitable のインスタンスを返します</returns>
        public static AssetBundleCreateRequestAwaitable ToAwaitable(this AssetBundleCreateRequest request)
        {
            // 進捗通知を受けないToAwaitableを叩く
            return ToAwaitable(request, null);
        }


        /// <summary>
        /// AssetBundleCreateRequest を待機可能にした AssetBundleCreateRequestAwaitable のインスタンスを生成します
        /// </summary>
        /// <param name="request">AssetBundleCreateRequestAwaitable のコンストラクタに渡す AssetBundleCreateRequest</param>
        /// <param name="progress">AssetBundleCreateRequest の進捗通知を受け取る IProgress</param>
        /// <returns>生成された AssetBundleCreateRequestAwaitable のインスタンスを返します</returns>
        public static AssetBundleCreateRequestAwaitable ToAwaitable(this AssetBundleCreateRequest request, IProgress<float> progress)
        {
            // インスタンスを生成して返す
            return new AssetBundleCreateRequestAwaitable(request, progress).Run<AssetBundleCreateRequestAwaitable>();
        }
    }
    #endregion



    #region Awaiter
    /// <summary>
    /// AssetBundleCreateRequest クラスの待機構造体です
    /// </summary>
    public struct AssetBundleCreateRequestAwaiter : INotifyCompletion
    {
        // メンバ変数定義
        private AssetBundleCreateRequest createRequest;



        /// <summary>
        /// タスクが完了したかどうか
        /// </summary>
        public bool IsCompleted => createRequest.isDone;



        /// <summary>
        /// AssetBundleCreateRequestAwaiter のインスタンスを初期化します
        /// </summary>
        /// <param name="createRequest">待機する AssetBundleCreateRequest</param>
        public AssetBundleCreateRequestAwaiter(AssetBundleCreateRequest createRequest)
        {
            // 覚える
            this.createRequest = createRequest;
        }


        /// <summary>
        /// タスクの完了処理を行います
        /// </summary>
        /// <param name="continuation">処理の継続関数</param>
        public void OnCompleted(Action continuation)
        {
            // すでにタスクが完了していたら
            if (IsCompleted)
            {
                // 継続関数を直ちに呼ぶ
                continuation();
                return;
            }


            // コールバックのタイミングで直ちに継続関数を叩くようにする
            createRequest.completed += _ => continuation();
        }


        /// <summary>
        /// タスクの結果を取得します
        /// </summary>
        /// <returns>タスクの結果を返します</returns>
        public AssetBundle GetResult()
        {
            // 結果を返す
            return createRequest.assetBundle;
        }
    }
    #endregion



    #region Awaitable
    /// <summary>
    /// AssetBundleCreateRequest を待機可能にした待機可能クラスです
    /// </summary>
    public class AssetBundleCreateRequestAwaitable : ImtAwaitableUpdateBehaviour<AssetBundle>
    {
        // メンバ変数定義
        private AssetBundleCreateRequest request;
        private IProgress<float> progress;



        /// <summary>
        /// アセットバンドルの準備ができたかどうか
        /// </summary>
        public override bool IsCompleted { get { return request.isDone; } protected set { } }



        /// <summary>
        /// AssetBundleCreateRequestAwaitable のインスタンスを初期化します
        /// </summary>
        /// <param name="request">待機したい AssetBundleCreateRequest</param>
        /// <param name="progress">AssetBundleCreateRequest の進捗通知を受け取る IProgress</param>
        /// <exception cref="ArgumentNullException">request が null です</exception>
        public AssetBundleCreateRequestAwaitable(AssetBundleCreateRequest request, IProgress<float> progress)
        {
            // もし request が null なら
            if (request == null)
            {
                // 処理が出来ない
                throw new ArgumentNullException(nameof(request));
            }


            // パラメータを受け取る
            this.request = request;
            this.progress = progress;
        }


        /// <summary>
        /// 動作の開始を行います
        /// </summary>
        protected override void Start()
        {
            // 完了イベントを登録する
            request.completed += OnCompleted;
        }


        /// <summary>
        /// 進捗監視を行うための状態更新を行います
        /// </summary>
        /// <returns>動作を継続する場合は true を、停止する場合は false を返します</returns>
        protected override bool Update()
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
        /// AssetBundleCreateRequest の完了イベントをハンドリングします
        /// </summary>
        /// <param name="operation">通知した AssetBundleCreateRequest</param>
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
        public override AssetBundle GetResult()
        {
            // アセットバンドルを返す
            return request.assetBundle;
        }
    }
    #endregion
}