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
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.ExceptionServices;

namespace IceMilkTea.Core
{
    /// <summary>
    /// 再試行処理可能なワーカー抽象クラスです
    /// </summary>
    public abstract class RetryableWorker
    {
        // クラス変数宣言
        private static readonly Progress<int> EmptyProgress;



        /// <summary>
        /// RetryableWorker クラスの初期化をします
        /// </summary>
        static RetryableWorker()
        {
            // 空進捗通知オブジェクトを生成しておく
            EmptyProgress = new Progress<int>();
        }



        #region DoWork関数
        /// <summary>
        /// 指定された関数を実行します
        /// </summary>
        /// <param name="work">実行する同期関数</param>
        /// <returns>指定された関数を実行しているタスクを返します</returns>
        /// <exception cref="ArgumentNullException">work が null です</exception>
        public Task DoWork(Action work)
        {
            // 空進捗オブジェクトでキャンセルなしで同じ関数を呼ぶ
            return DoWork(work, EmptyProgress, CancellationToken.None);
        }


        /// <summary>
        /// 指定された非同期関数を実行します
        /// </summary>
        /// <param name="work">実行する同期関数</param>
        /// <returns>指定された関数を実行しているタスクを返します</returns>
        /// <exception cref="ArgumentNullException">work が null です</exception>
        public Task DoWork(Task work)
        {
            // 空進捗オブジェクトでキャンセルなしで同じ関数を呼ぶ
            return DoWork(work, EmptyProgress, CancellationToken.None);
        }


        /// <summary>
        /// 指定された値を返す関数を実行します
        /// </summary>
        /// <typeparam name="TResult">返す値の型</typeparam>
        /// <param name="work">実行する同期関数</param>
        /// <returns>指定された関数を実行しているタスクを返します</returns>
        /// <exception cref="ArgumentNullException">work が null です</exception>
        public Task<TResult> DoWork<TResult>(Func<TResult> work)
        {
            // 空進捗オブジェクトでキャンセルなしで同じ関数を呼ぶ
            return DoWork(work, EmptyProgress, CancellationToken.None);
        }


        /// <summary>
        /// 指定された値を返す非同期関数を実行します
        /// </summary>
        /// <typeparam name="TResult">返す値の型</typeparam>
        /// <param name="work">実行する非同期関数</param>
        /// <returns>指定された関数を実行しているタスクを返します</returns>
        /// <exception cref="ArgumentNullException">work が null です</exception>
        public Task<TResult> DoWork<TResult>(Task<TResult> work)
        {
            // 空進捗オブジェクトでキャンセルなしで同じ関数を呼ぶ
            return DoWork(work, EmptyProgress, CancellationToken.None);
        }


        /// <summary>
        /// 指定された関数を実行します
        /// </summary>
        /// <param name="work">実行する同期関数</param>
        /// <param name="progress">リトライ時の進捗通知オブジェクト。既定は空通知オブジェクトです。</param>
        /// <returns>指定された関数を実行しているタスクを返します</returns>
        /// <exception cref="ArgumentNullException">work が null です</exception>
        /// <exception cref="ArgumentNullException">progress が null です</exception>
        public Task DoWork(Action work, IProgress<int> progress)
        {
            // キャンセルなしで同じ関数を呼ぶ
            return DoWork(work, progress, CancellationToken.None);
        }


        /// <summary>
        /// 指定された非同期関数を実行します
        /// </summary>
        /// <param name="work">実行する同期関数</param>
        /// <param name="progress">リトライ時の進捗通知オブジェクト。既定は空通知オブジェクトです。</param>
        /// <returns>指定された関数を実行しているタスクを返します</returns>
        /// <exception cref="ArgumentNullException">work が null です</exception>
        /// <exception cref="ArgumentNullException">progress が null です</exception>
        public Task DoWork(Task work, IProgress<int> progress)
        {
            // キャンセルなしで同じ関数を呼ぶ
            return DoWork(work, progress, CancellationToken.None);
        }


        /// <summary>
        /// 指定された値を返す関数を実行します
        /// </summary>
        /// <typeparam name="TResult">返す値の型</typeparam>
        /// <param name="work">実行する同期関数</param>
        /// <param name="progress">リトライ時の進捗通知オブジェクト。既定は空通知オブジェクトです。</param>
        /// <returns>指定された関数を実行しているタスクを返します</returns>
        /// <exception cref="ArgumentNullException">work が null です</exception>
        /// <exception cref="ArgumentNullException">progress が null です</exception>
        public Task<TResult> DoWork<TResult>(Func<TResult> work, IProgress<int> progress)
        {
            // キャンセルなしで同じ関数を呼ぶ
            return DoWork(work, progress, CancellationToken.None);
        }



        /// <summary>
        /// 指定された値を返す非同期関数を実行します
        /// </summary>
        /// <typeparam name="TResult">返す値の型</typeparam>
        /// <param name="work">実行する非同期関数</param>
        /// <param name="progress">リトライ時の進捗通知オブジェクト。既定は空通知オブジェクトです。</param>
        /// <returns>指定された関数を実行しているタスクを返します</returns>
        /// <exception cref="ArgumentNullException">work が null です</exception>
        /// <exception cref="ArgumentNullException">progress が null です</exception>
        public Task<TResult> DoWork<TResult>(Task<TResult> work, IProgress<int> progress)
        {
            // キャンセルなしで同じ関数を呼ぶ
            return DoWork(work, progress, CancellationToken.None);
        }


        /// <summary>
        /// 指定された関数を実行します
        /// </summary>
        /// <param name="work">実行する同期関数</param>
        /// <param name="progress">リトライ時の進捗通知オブジェクト。既定は空通知オブジェクトです。</param>
        /// <param name="cancellationToken">リトライのキャンセル要求を監視するためのトークン。既定は None です。</param>
        /// <returns>指定された関数を実行しているタスクを返します</returns>
        /// <exception cref="ArgumentNullException">work が null です</exception>
        /// <exception cref="ArgumentNullException">progress が null です</exception>
        public Task DoWork(Action work, IProgress<int> progress, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// 指定された非同期関数を実行します
        /// </summary>
        /// <param name="work">実行する同期関数</param>
        /// <param name="progress">リトライ時の進捗通知オブジェクト。既定は空通知オブジェクトです。</param>
        /// <param name="cancellationToken">リトライのキャンセル要求を監視するためのトークン。既定は None です。</param>
        /// <returns>指定された関数を実行しているタスクを返します</returns>
        /// <exception cref="ArgumentNullException">work が null です</exception>
        /// <exception cref="ArgumentNullException">progress が null です</exception>
        public Task DoWork(Task work, IProgress<int> progress, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// 指定された値を返す関数を実行します
        /// </summary>
        /// <typeparam name="TResult">返す値の型</typeparam>
        /// <param name="work">実行する同期関数</param>
        /// <param name="progress">リトライ時の進捗通知オブジェクト。既定は空通知オブジェクトです。</param>
        /// <param name="cancellationToken">リトライのキャンセル要求を監視するためのトークン。既定は None です。</param>
        /// <returns>指定された関数を実行しているタスクを返します</returns>
        /// <exception cref="ArgumentNullException">work が null です</exception>
        /// <exception cref="ArgumentNullException">progress が null です</exception>
        public Task<TResult> DoWork<TResult>(Func<TResult> work, IProgress<int> progress, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// 指定された値を返す非同期関数を実行します
        /// </summary>
        /// <typeparam name="TResult">返す値の型</typeparam>
        /// <param name="work">実行する非同期関数</param>
        /// <param name="progress">リトライ時の進捗通知オブジェクト。既定は空通知オブジェクトです。</param>
        /// <param name="cancellationToken">リトライのキャンセル要求を監視するためのトークン。既定は None です。</param>
        /// <returns>指定された関数を実行しているタスクを返します</returns>
        /// <exception cref="ArgumentNullException">work が null です</exception>
        /// <exception cref="ArgumentNullException">progress が null です</exception>
        public async Task<TResult> DoWork<TResult>(Task<TResult> work, IProgress<int> progress, CancellationToken cancellationToken)
        {
            // 関数の実行開始を知らせる
            BeginWork();


            // 基本は無限ループ
            while (true)
            {
                try
                {
                    return await work;
                }
                catch (Exception error)
                {
                    DoHandleError(error);
                    if (await WaitRetry(cancellationToken))
                    {
                        continue;
                    }
                }
            }
        }
        #endregion


        #region リトライ制御関数
        /// <summary>
        /// DoWork() 関数によって処理される関数が実行される前に呼び出されます
        /// </summary>
        protected virtual void BeginWork()
        {
        }


        /// <summary>
        /// DoWork() 関数によって処理される関数が実行された後に呼び出されます
        /// </summary>
        protected virtual void EndWork()
        {
        }


        /// <summary>
        /// DoWork() 関数によって処理された関数が例外を発生させた時に呼び出されます
        /// </summary>
        /// <param name="error">発生した例外</param>
        /// <returns>発生した例外を処理した場合は true を、処理できない例外だった場合は false を返します</returns>
        protected virtual bool DoHandleError(Exception error)
        {
            // 既定は例外を処理したとして返す
            return true;
        }


        /// <summary>
        /// 関数が失敗してリトライが必要な時にリトライするまでの待機をします
        /// </summary>
        /// <param name="cancellationToken">リトライのキャンセル要求を監視するためのトークン</param>
        /// <returns>リトライを待機するタスクを返します。リトライをしない場合は false を、リトライする場合は true を返します</returns>
        protected abstract Task<bool> WaitRetry(CancellationToken cancellationToken);
        #endregion
    }
}