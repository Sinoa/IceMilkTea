// zlib/libpng License
//
// Copyright (c) 2026 Sinoa
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
using System.Collections.Generic;
using IceMilkTea.Core;
using UnityEngine;

namespace IceMilkTea.Sample
{
    /// <summary>
    /// Domain Reload / Scene Reload を無効化した状態での動作検証に用いる <see cref="GameService"/> のサンプル実装です。
    /// </summary>
    public class SampleService : GameService
    {
        // 各更新タイミングの実行回数（更新関数が動作していることの確認用）
        private int mainLoopHeadCount;
        private int focusInCount;


        /// <summary>
        /// サービスを起動し、更新関数テーブルを構築します。
        /// </summary>
        /// <param name="info">サービスが起動する時に必要とする情報を設定します</param>
        protected override void Startup(out GameServiceStartupInfo info)
        {
            Debug.Log("[SampleService] Startup");


            // Awaiter のスケジューラ静的状態がリセットされているかの確認用ログ。
            // currentScheduler が正しくリセットされていれば、毎回の Play Mode で既定スケジューラ型が表示される。
            // リセットされていなければ、2 回目以降の Play Mode で前回 SetScheduler した型が残存して表示される。
            Debug.Log($"[SampleService] CurrentScheduler = {ImtAwaitableUpdateBehaviourScheduler.CurrentOrDefault.GetType().Name}");
            ImtAwaitableUpdateBehaviourScheduler.SetScheduler(ImtAwaitableUpdateBehaviourScheduler.GetThreadPoolScheduler());


            // MainLoopHead は純粋な PlayerLoop 注入経路、 OnApplicationFocusIn は MonoBehaviourEventBridge 経路を検証する
            info = new GameServiceStartupInfo()
            {
                UpdateFunctionTable = new Dictionary<GameServiceUpdateTiming, Action>()
                {
                    { GameServiceUpdateTiming.MainLoopHead, OnMainLoopHead },
                    { GameServiceUpdateTiming.OnApplicationFocusIn, OnFocusIn },
                },
            };
        }


        /// <summary>
        /// サービスをシャットダウンします。
        /// </summary>
        protected override void Shutdown()
        {
            Debug.Log($"[SampleService] Shutdown (mainLoopHeadCount={mainLoopHeadCount}, focusInCount={focusInCount})");
        }


        /// <summary>
        /// メインループ先頭タイミングの更新処理です。
        /// </summary>
        private void OnMainLoopHead()
        {
            // PlayerLoop 注入経路のため GC Alloc を避け、カウンタのインクリメントのみ行う
            ++mainLoopHeadCount;
        }


        /// <summary>
        /// アプリケーションフォーカス取得タイミングの更新処理です。
        /// </summary>
        private void OnFocusIn()
        {
            // MonoBehaviourEventBridge 経由のため GC Alloc を避け、カウンタのインクリメントのみ行う
            ++focusInCount;
        }
    }
}
