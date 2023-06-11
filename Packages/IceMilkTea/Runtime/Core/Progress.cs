// zlib/libpng License
//
// Copyright (c) 2019 Sinoa
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
using System.ComponentModel;
using System.Threading;

namespace IceMilkTea.Core
{
    /// <summary>
    /// 高速にレポートされる進捗通知内容を維持しながら、同期コンテキストが処理されるまで新たにポストされないようにする進捗クラスです。
    /// </summary>
    /// <typeparam name="T">進捗通知する内容の型</typeparam>
    public class ThrottleableProgress<T> : IProgress<T>
    {
        // 読み取り専用クラス変数宣言
        private static readonly SendOrPostCallback CachedCallback = new SendOrPostCallback(_ => ((Action)_)());

        // メンバ変数定義
        private SynchronizationContext context;
        private Action process;
        private Action<T> handler;
        private bool posted;
        private T result;



        /// <summary>
        /// 進捗通知を処理しない空の進捗オブジェクトです
        /// </summary>
        public static ThrottleableProgress<T> Empty { get; private set; } = new ThrottleableProgress<T>();



        /// <summary>
        /// ThrottleableProgress クラスのインスタンスを初期化します
        /// </summary>
        public ThrottleableProgress() : this(null)
        {
        }


        /// <summary>
        /// ThrottleableProgress クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="handler">進捗通知を受けた時に通知の処理を行うハンドラ</param>
        public ThrottleableProgress(Action<T> handler)
        {
            // このインスタンスを生成した同期コンテキストを取得してハンドラを設定
            context = AsyncOperationManager.SynchronizationContext;
            this.handler = handler ?? new Action<T>(_ => { });
            process = DoProcess;
        }


        /// <summary>
        /// 指定された内容で進捗通知をします。
        /// </summary>
        /// <param name="value">通知する内容</param>
        public void Report(T value)
        {
            // 最新の状態を受け取ってもしポスト済み状態なら
            result = value;
            if (posted)
            {
                // 新たにポストする必要ないので終了
                // このタイミングでDoProcessが呼ばれてしまっても許容とする
                return;
            }


            // ポスト済み状態にして同期コンテキストに通知ハンドラの実行ハンドラを実行されるようにする
            posted = true;
            context.Post(CachedCallback, process);
        }


        /// <summary>
        /// 同期コンテキストによって実行されます
        /// </summary>
        private void DoProcess()
        {
            // 同期コンテキストによって通知が処理されたことを示してハンドラを実行する
            posted = false;
            handler(result);
        }
    }
}