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
using IceMilkTea.Core;
using UnityEngine;

namespace IceMilkTea.GameLogic
{
    /// <summary>
    /// 指定された UnityのTime.time 秒数待機する、待機可能クラスです。
    /// </summary>
    public class GameTimeWaitAwaitable : ImtAwaitableUpdateBehaviour
    {
        // メンバ変数定義
        private ImtAwaitableUpdateBehaviourScheduler scheduler;
        private float startTime;
        private float waitTime;



        /// <summary>
        /// GameTimeWaitAwaitable のインスタンスを初期化します
        /// </summary>
        public GameTimeWaitAwaitable()
        {
            // 同期コンテキストを使ったスケジューラを取得しておく
            scheduler = ImtAwaitableUpdateBehaviourScheduler.GetCurrentSynchronizationContextScheduler();
        }


        /// <summary>
        /// 指定された秒数分を非同期に待機します。単位は（秒）です。
        /// また、ゲームループの特性上、1フレームの時間未満の待機は飽和します。
        /// </summary>
        /// <param name="time">待機する秒数。0.0以下の値の場合は、直ちに完了した状態になります</param>
        /// <returns>指定秒数分待機する設定がされた自身のインスタンスを返します</returns>
        /// <exception cref="InvalidOperationException">既に待機処理中です</exception>
        public IAwaitable WaitTimeAsync(float time)
        {
            // 既に動作中の場合は
            if (IsRunning)
            {
                // 動作中であることの例外を吐く
                throw new InvalidOperationException("既に待機処理中です");
            }


            // 待機秒数が0.0以下なら
            if (time <= 0.0f)
            {
                // 完了状態にして返す
                IsCompleted = true;
                return this;
            }


            // 起動開始時間を取り出して、待機秒数の設定と未完了状態を設定
            startTime = Time.time;
            waitTime = time;
            IsCompleted = false;


            // コンストラクタで拾ったスケジューラで起動して返す
            return Run(scheduler);
        }


        /// <summary>
        /// 待機状態の更新を行います
        /// </summary>
        /// <returns>継続動作する場合は true を、停止する場合は false を返します</returns>
        protected override bool Update()
        {
            // 経過時間が指定待機時間を超過したのなら
            if ((Time.time - startTime) >= waitTime)
            {
                // シグナルと完了をして、動作を停止
                SetSignalWithCompleted();
                return false;
            }


            // まだ継続する
            return true;
        }
    }
}