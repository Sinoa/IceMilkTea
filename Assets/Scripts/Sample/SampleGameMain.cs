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

using IceMilkTea.Core;
using UnityEngine;

namespace IceMilkTea.Sample
{
    /// <summary>
    /// Domain Reload / Scene Reload を無効化した状態での動作検証に用いる <see cref="GameMain"/> のサンプル実装です。
    /// </summary>
    public class SampleGameMain : GameMain
    {
        // メインループの実行回数（更新関数が動作していることの確認用）
        private int updateCount;


        /// <summary>
        /// ゲームの起動処理を行い、検証用サービスを登録します。
        /// </summary>
        protected override void Startup()
        {
            Debug.Log("[SampleGameMain] Startup");
            ServiceManager.AddService(new SampleService());
        }


        /// <summary>
        /// ゲームの終了処理を行います。
        /// </summary>
        protected override void Shutdown()
        {
            Debug.Log($"[SampleGameMain] Shutdown (updateCount={updateCount})");
        }


        /// <summary>
        /// ゲームのメインループ処理を行います。
        /// </summary>
        protected override void Update()
        {
            // PlayerLoop 注入経路のため GC Alloc を避け、カウンタのインクリメントのみ行う
            ++updateCount;
        }
    }
}
