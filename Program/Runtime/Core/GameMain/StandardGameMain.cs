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

using IceMilkTea.Profiler;
using UnityEngine;

namespace IceMilkTea.Core
{
    /// <summary>
    /// IceMilkTeaが提供する標準なゲームメインクラスを提供します。
    /// IceMilkTeaのあらゆるシステムを簡単にセットアップし構築する場合に有用です。
    /// </summary>
    public class StandardGameMain : GameMain
    {
        // インスペクタ公開用メンバ変数定義
        [SerializeField]
        private bool useProfiler; //!< プロファイラを使用するか否か



        /// <summary>
        /// GameMainの起動処理を行います
        /// </summary>
        protected override void Startup()
        {
            // もしプロファイラを使うのなら
            if (useProfiler)
            {
                // プロファイラの初期化とUnityの標準計測プロファイラを設定する
                PerformanceMonitor.Instance.Initialize();
                PerformanceMonitor.Instance.AddProbe(UnityStandardLoopPerformanceProbe.Instance);
                PerformanceMonitor.Instance.AddRenderer(new GraphicalPerformanceRenderer());
            }
        }
    }
}