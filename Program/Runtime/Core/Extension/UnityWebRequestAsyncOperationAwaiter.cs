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
using UnityEngine.Networking;

namespace IceMilkTea.Core
{
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
    }



    /// <summary>
    /// UnityWebRequestAsyncOperation の待機構造体です
    /// </summary>
    public struct UnityWebRequestAsyncOperationAwaiter : INotifyCompletion
    {
        // 構造体変数宣言
        private static SendOrPostCallback cache = new SendOrPostCallback(_ => ((Action)_)());



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


            // 同期コンテキストを取得して、イベントハンドラ経由から継続関数を叩いてもらうように登録する
            var context = SynchronizationContext.Current;
            operation.completed += _ => context.Post(cache, continuation);
        }


        /// <summary>
        /// タスクの結果を取得します
        /// </summary>
        /// <returns>タスクの結果を返します</returns>
        public UnityWebRequest GetResult()
        {
            // 結果を返す
            return operation.webRequest;
        }
    }
}