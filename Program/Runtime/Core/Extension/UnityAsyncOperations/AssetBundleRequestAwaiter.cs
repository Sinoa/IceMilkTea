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
    /// AssetBundleRequest クラスの拡張関数実装用クラスです
    /// </summary>
    public static partial class AssetBundleRequestExtension
    {
        /// <summary>
        /// AssetBundleRequest の待機オブジェクトを取得します
        /// </summary>
        /// <param name="request">待機する AssetBundleRequest</param>
        /// <returns>AssetBundleRequest の待機オブジェクトを返します</returns>
        public static AssetBundleRequestAwaiter GetAwaiter(this AssetBundleRequest request)
        {
            // AssetBundleRequest待機オブジェクトを生成して返す
            return new AssetBundleRequestAwaiter(request);
        }
    }



    /// <summary>
    /// AssetBundleRequest クラスの待機構造体です
    /// </summary>
    public struct AssetBundleRequestAwaiter : INotifyCompletion
    {
        // 構造体変数宣言
        private static SendOrPostCallback cache = new SendOrPostCallback(_ => ((Action)_)());



        // メンバ変数定義
        private AssetBundleRequest request;



        /// <summary>
        /// タスクが完了したかどうか
        /// </summary>
        public bool IsCompleted => request.isDone;



        /// <summary>
        /// AssetBundleRequestAwaiter のインスタンスを初期化します
        /// </summary>
        /// <param name="request">待機する AssetBundleRequest</param>
        public AssetBundleRequestAwaiter(AssetBundleRequest request)
        {
            // 覚える
            this.request = request;
        }


        /// <summary>
        /// タスクの完了処理を行います
        /// </summary>
        /// <param name="continuation">タスクの継続関数</param>
        public void OnCompleted(Action continuation)
        {
            // タスクがすでに完了しているのなら
            if (IsCompleted)
            {
                // 継続関数を直ちに呼び出して終了
                continuation();
                return;
            }


            // 現在の同期コンテキストを取得して、ロード完了イベントからコンテキストにPostするように登録する
            var context = SynchronizationContext.Current;
            request.completed += _ => context.Post(cache, continuation);
        }


        /// <summary>
        /// タスクの結果を取得します
        /// </summary>
        /// <returns>タスクの結果を返します</returns>
        public UnityEngine.Object GetResult()
        {
            // 結果を返す
            return request.asset;
        }
    }
}