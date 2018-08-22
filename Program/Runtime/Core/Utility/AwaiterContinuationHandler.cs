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
using System.ComponentModel;
using System.Threading;

namespace IceMilkTea.Core
{
    /// <summary>
    /// 比較的スタンダードな Awaiter の継続関数をハンドリングする構造体です。
    /// この構造体は、多数の Awaiter の継続関数を登録することが可能で、継続関数を登録とシグナル設定をするだけで動作します。
    /// </summary>
    public struct AwaiterContinuationHandler
    {
        /// <summary>
        /// 登録された Awaiter の継続関数と、その登録した時の同期コンテキストを保持する構造体です
        /// </summary>
        private struct Handler
        {
            /// <summary>
            /// 継続関数登録時の同期コンテキスト
            /// </summary>
            public SynchronizationContext Context;

            /// <summary>
            /// 登録された継続関数
            /// </summary>
            public Action continuation;
        }



        // 定数定義
        private const int MinBufferSize = 8;



        // 読み取り専用構造体変数宣言
        private static readonly SendOrPostCallback cache = new SendOrPostCallback(_ => ((Action)_)());



        // メンバ変数定義
        private Handler[] handlers;
        private int handleCount;



        /// <summary>
        /// AwaiterContinuationHandler のインスタンスを指定された容量で初期化します。
        /// </summary>
        /// <param name="capacity">登録する継続関数の初期容量</param>
        public AwaiterContinuationHandler(int capacity)
        {
            // ハンドラ配列の初期化
            handlers = new Handler[capacity];
            handleCount = 0;
        }


        /// <summary>
        /// Awaiter の継続関数を登録します。
        /// 登録した継続関数は SetSignal() 関数にて継続を行うことが可能です。
        /// </summary>
        /// <param name="continuation">登録する継続関数</param>
        public void RegisterContinuation(Action continuation)
        {
            // まだハンドラ配列が未生成なら
            if (handlers == null)
            {
                // 初期の最小容量で初期化
                handlers = new Handler[MinBufferSize];
                handleCount = 0;
            }


            // もしハンドラ配列の長さが登録ハンドル数に到達しているのなら
            if (handlers.Length == handleCount)
            {
                // 倍のサイズで新しい容量を確保する
                EnsureCapacity(handlers.Length * 2);
            }


            // 継続関数をハンドラ配列に追加する
            handlers[handleCount++] = new Handler()
            {
                // ハンドラ構造体の初期化（このタイミングで同期コンテキストを拾う）
                Context = AsyncOperationManager.SynchronizationContext,
                continuation = continuation,
            };
        }


        /// <summary>
        /// 登録された継続関数を、登録時の同期コンテキストを通じて呼び出されるようにします。
        /// また、一度シグナルした継続処理の参照は消失するため、再度 Awaite するには、改めて継続関数を登録する必要があります。
        /// </summary>
        public void SetSignal()
        {
            // 登録されたハンドラの数分回る
            for (int i = 0; i < handleCount; ++i)
            {
                // 同期コンテキストに継続関数をポストして（実際はキャッシュされたSendOrPostCallbackを通す）参照を忘れる
                handlers[i].Context.Post(cache, handlers[i].continuation);
                handlers[i].Context = null;
                handlers[i].continuation = null;
            }


            // ハンドラを空にする
            handleCount = 0;
        }


        /// <summary>
        /// ハンドラ配列を指定された容量で新しく確保します
        /// </summary>
        /// <param name="newCapacity">新しいハンドラの容量（既定値より小さい値の場合は既定値に設定されます）</param>
        private void EnsureCapacity(int newCapacity)
        {
            // 既定値より小さいなら
            if (newCapacity < MinBufferSize)
            {
                // 規定値に強制的に設定する
                newCapacity = MinBufferSize;
            }


            // 新しい容量が、既に使用済み容量未満の場合なら
            if (newCapacity < handleCount)
            {
                // 新しく確保できない事を吐く
                throw new ArgumentException("指定された新しい容量が、使用済み容量未満です", nameof(newCapacity));
            }


            // もし新しい容量が、現在の容量と同じなら
            if (newCapacity == handlers.Length)
            {
                // 既に同サイズの容量なので何もせず終了
                return;
            }


            // 新しい配列を確保して、旧配列から使用済みデータをコピー後、参照を新しい配列に設定する
            var newHandlers = new Handler[newCapacity];
            Array.Copy(handlers, newHandlers, handleCount);
            handlers = newHandlers;
        }
    }
}