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
using System.Runtime.ExceptionServices;
using IceMilkTea.Core;
using UnityEngine;

namespace IceMilkTea.Component
{
    /// <summary>
    /// Unity の MonoBehaviour クラスを待機可能にしたコンポーネントクラスです
    /// </summary>
    public abstract class AwaitableMonoBehaviour : MonoBehaviour, IAwaitable
    {
        // メンバ変数定義
        private AwaiterContinuationHandler awaiterHandler = new AwaiterContinuationHandler();
        private ExceptionDispatchInfo exceptionInfo;



        /// <summary>
        /// タスクが完了したかどうか
        /// </summary>
        public virtual bool IsCompleted { get; protected set; }



        /// <summary>
        /// 登録された継続関数にシグナルを設定して、継続関数が呼び出されるようにします。
        /// </summary>
        protected void SetSignal()
        {
            // 待機オブジェクトハンドラのシグナルを設定する
            awaiterHandler.SetSignal();
        }


        /// <summary>
        /// 待機状態が完了するとともに、登録された継続関数にシグナルを設定して、継続関数が呼び出されるようにします。
        /// </summary>
        protected void SetSignalWithCompleted()
        {

            // 完了状態にして、待機オブジェクトハンドラのシグナルを設定する
            IsCompleted = true;
            awaiterHandler.SetSignal();
        }


        /// <summary>
        /// 待機可能コンポーネントが、例外を発生させてしまった場合、その例外を設定します。
        /// この関数で設定した例外は、適切なタイミングで報告されます。
        /// また、原則としてこの関数を利用した直後は SetSignal() 関数を呼び出して
        /// 直ちに継続関数を解放するようにしてください。
        /// </summary>
        /// <param name="exception">設定する例外</param>
        protected void SetException(Exception exception)
        {
            // 例外をキャプチャして保持する
            exceptionInfo = ExceptionDispatchInfo.Capture(exception);
        }


        /// <summary>
        /// この待機可能コンポーネントの待機オブジェクトを取得します
        /// </summary>
        /// <returns>待機オブジェクトを返します</returns>
        public ImtAwaiter GetAwaiter()
        {
            // 待機オブジェクトを生成して返す
            return new ImtAwaiter(this);
        }


        /// <summary>
        /// この待機可能コンポーネントで発生した、保持済みエラーを取得します。
        /// </summary>
        /// <returns>現在保持しているエラー情報を返します</returns>
        public ExceptionDispatchInfo GetError()
        {
            // 例外を返す
            return exceptionInfo;
        }


        /// <summary>
        /// 待機オブジェクトの継続関数を登録します
        /// </summary>
        /// <param name="continuation">登録する継続関数</param>
        public void RegisterContinuation(Action continuation)
        {
            // 待機オブジェクトハンドラに継続関数を登録する
            awaiterHandler.RegisterContinuation(continuation);
        }
    }



    /// <summary>
    /// Unity の MonoBehaviour クラスを、値返却が出来る待機可能にしたコンポーネントクラスです
    /// </summary>
    /// <typeparam name="TResult">返却する値の型</typeparam>
    public abstract class AwaitableMonoBehaviour<TResult> : AwaitableMonoBehaviour, IAwaitable<TResult>
    {
        /// <summary>
        /// 待機した結果を取得します。
        /// </summary>
        /// <returns>待機した結果を返します</returns>
        public abstract TResult GetResult();


        /// <summary>
        /// この待機可能コンポーネントの待機オブジェクトを取得します
        /// </summary>
        /// <returns>待機オブジェクトを返します</returns>
        public new ImtAwaiter<TResult> GetAwaiter()
        {
            // 値返却が可能なAwaiterを生成して返す
            return new ImtAwaiter<TResult>(this);
        }
    }
}