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
using UnityEngine;

namespace IceMilkTea.Core
{
    /// <summary>
    /// AsyncOperation クラスの拡張関数実装用構造体です
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
    }



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
            }
            else
            {
                // 現在の同期コンテキストを取り出してコールバックの準備もする
                var context = System.Threading.SynchronizationContext.Current;
                var callback = new System.Threading.SendOrPostCallback(_ => continuation());


                // 非同期操作の完了イベントのハンドリング
                asyncOperation.completed += (AsyncOperation _) =>
                {
                    // 同期コンテキストのポストを行う様に仕向ける
                    context.Post(callback, null);
                };
            }
        }


        /// <summary>
        /// タスクの完了を待機し、結果を同期的に取得する処理を実行します
        /// </summary>
        public void GetResult()
        {
            // 非同期操作が終わっているなら
            if (IsCompleted)
            {
                // 直ちに抜ける
                return;
            }


            // いま現状のUnityでAsyncOperationを同期的に待機する方法が見いだせない（待機可能とは）
            throw new InvalidOperationException("UnityのAsyncOperationを同一フレームで待つことが出来ません");
        }
    }
}