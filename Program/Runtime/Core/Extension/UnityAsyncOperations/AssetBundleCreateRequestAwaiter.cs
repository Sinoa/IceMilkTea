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
    /// <summary>
    /// AssetBundleCreateRequest クラスの拡張関数実装用クラスです
    /// </summary>
    public static partial class AssetBundleCreateRequestExtension
    {
        /// <summary>
        /// AssetBundleCreateRequest の待機オブジェクトを取得します
        /// </summary>
        /// <param name="createRequest">待機する AssetBundleCreateRequest</param>
        /// <returns>AssetBundleCreateRequest の待機オブジェクトを返します</returns>
        public static AssetBundleCreateRequestAwaiter GetAwaiter(this AssetBundleCreateRequest createRequest)
        {
            // AssetBundleCreateRequest の待機オブジェクトを生成して返す
            return new AssetBundleCreateRequestAwaiter(createRequest);
        }
    }



    /// <summary>
    /// AssetBundleCreateRequest クラスの待機構造体です
    /// </summary>
    public struct AssetBundleCreateRequestAwaiter : INotifyCompletion
    {
        // 構造体変数宣言
        private static SendOrPostCallback cache = new SendOrPostCallback(_ => ((Action)_)());



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


            // 現在の同期コンテキストを拾い上げて、アセットバンドル読み込み完了イベントでPostするように登録する
            var context = SynchronizationContext.Current;
            createRequest.completed += _ => context.Post(cache, continuation);
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
}