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
using System.Runtime.Serialization;

namespace IceMilkTea.SubSystem
{
    /// <summary>
    /// ストレージコントローラを見つけられなかった場合の例外です
    /// </summary>
    [Serializable]
    public class ImtStorageControllerNotFoundException : Exception
    {
        /// <summary>
        /// 見つけられなかったストレージ名
        /// </summary>
        public string StorageName { get; private set; }



        /// <summary>
        /// ImtStorageControllerNotFoundException クラスのインスタンスを初期化します
        /// </summary>
        public ImtStorageControllerNotFoundException() : base()
        {
            // 例外パラメータの初期化
            InitializeExceptionParameter(null);
        }


        /// <summary>
        /// ImtStorageControllerNotFoundException クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="message">この例外を発生させたメッセージ</param>
        public ImtStorageControllerNotFoundException(string message) : base(message)
        {
            // 例外パラメータの初期化
            InitializeExceptionParameter(null);
        }


        /// <summary>
        /// ImtStorageControllerNotFoundException クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="message">この例外を発生させたメッセージ</param>
        /// <param name="innerException">この例外の発生となった原因の例外</param>
        public ImtStorageControllerNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
            // 例外パラメータの初期化
            InitializeExceptionParameter(null);
        }


        /// <summary>
        /// ImtStorageControllerNotFoundException クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="message">この例外を発生させたメッセージ</param>
        /// <param name="storageName">見つけられなかったストレージ名</param>
        public ImtStorageControllerNotFoundException(string message, string storageName) : base(message)
        {
            // 例外パラメータの初期化
            InitializeExceptionParameter(storageName);
        }


        /// <summary>
        /// ImtStorageControllerNotFoundException クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="message">この例外を発生させたメッセージ</param>
        /// <param name="storageName">見つけられなかったストレージ名</param>
        /// <param name="innerException">この例外の発生となった原因の例外</param>
        public ImtStorageControllerNotFoundException(string message, string storageName, Exception innerException) : base(message, innerException)
        {
            // 例外パラメータの初期化
            InitializeExceptionParameter(storageName);
        }


        /// <summary>
        /// 例外パラメータの初期化をします
        /// </summary>
        /// <param name="storageName">見つけられなかったストレージ名。</param>
        private void InitializeExceptionParameter(string storageName)
        {
            // 見つけられなかったストレージ名を設定
            StorageName = storageName;
        }


        /// <summary>
        /// 指定したシリアル化情報とコンテキスト情報を使用して、ImtStorageControllerNotFoundException クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="info">スローされる例外に関するシリアル化されたオブジェクト データを保持するオブジェクト。</param>
        /// <param name="context">転送元または転送先に関するコンテキスト情報を含むオブジェクト。</param>
        protected ImtStorageControllerNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}