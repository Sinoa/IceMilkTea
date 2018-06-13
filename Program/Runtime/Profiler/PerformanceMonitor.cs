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
using System.Collections.Generic;

namespace IceMilkTea.Profiler
{
    /// <summary>
    /// ゲームのパフォーマンスを監視し、パフォーマンス情報を保持するクラスです
    /// </summary>
    public sealed class PerformanceMonitor
    {
        // シングルトン実装
        public static PerformanceMonitor Instance { get; } = new PerformanceMonitor();



        // メンバ変数宣言
        private List<PerformanceProbe> performanceProbeList;
        private List<PerformanceRenderer> performanceRendererList;
        private ProfileFetchResult[] profileFetchResultsCache;



        /// <summary>
        /// メンバの初期化を行います
        /// </summary>
        private PerformanceMonitor()
        {
            // メンバ変数のインスタンスを生成
            performanceProbeList = new List<PerformanceProbe>();
            performanceRendererList = new List<PerformanceRenderer>();
            profileFetchResultsCache = new ProfileFetchResult[0];
        }


        /// <summary>
        /// パフォーマンスモニタにプローブを追加します
        /// </summary>
        /// <param name="probe">追加するプローブ</param>
        /// <exception cref="NullReferenceException">probeがnullです</exception>
        public void AddProbe(PerformanceProbe probe)
        {
            // 追加しようとしているプローブがnullなら
            if (probe == null)
            {
                // そんな追加は許されない！
                throw new NullReferenceException($"{nameof(probe)}がnullです");
            }


            // プローブを追加する
            performanceProbeList.Add(probe);
        }


        /// <summary>
        /// パフォーマンスモニタにレンダラを追加します
        /// </summary>
        /// <param name="renderer">追加するレンダラ</param>
        /// <exception cref="NullReferenceException">rendererがnullです</exception>
        public void AddRenderer(PerformanceRenderer renderer)
        {
            // 追加しようとしているレンダラがnullなら
            if (renderer == null)
            {
                // そんな追加は許されない！
                throw new NullReferenceException($"{nameof(renderer)}がnullです");
            }
        }


        private void StartProfiler()
        {
        }


        private void EndProfiler()
        {
        }


        private void Draw()
        {
        }
    }
}