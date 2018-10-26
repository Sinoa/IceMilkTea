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

using System.Diagnostics;

namespace IceMilkTea.Profiler
{
    /// <summary>
    /// UnityStandardLoopPerformanceProbe クラスによるパフォーマンス計測結果を保持するクラスです
    /// </summary>
    public class UnityStandardLoopProfileResult : ProfileFetchResult
    {
        /// <summary>
        /// FixedUpdate更新ループに要したチックカウント
        /// </summary>
        public long FixedUpdateTickCount { get; set; }

        /// <summary>
        /// Update更新ループに要したチックカウント
        /// </summary>
        public long UpdateTickCount { get; set; }

        /// <summary>
        /// LateUpdate更新ループに要したチックカウント
        /// </summary>
        public long LateUpdateTickCount { get; set; }

        /// <summary>
        /// レンダリングに要したチックカウント
        /// ただし、レンダースレッドのチックカウントではなくメインスレッド上でのレンダリングチックカウントとなる
        /// </summary>
        public long RenderingTickCount { get; set; }

        /// <summary>
        /// レンダーテクスチャのレンダリングに要したチックカウント
        /// ただし、レンダースレッドのチックカウントではなくメインスレッド上でのレンダリングチックカウントとなる
        /// </summary>
        public long TextureRenderingTickCount { get; set; }

        /// <summary>
        /// FixedUpdate更新ループに要した時間（ミリ秒）
        /// </summary>
        public double FixedUpdateTime { get; set; }

        /// <summary>
        /// Update更新ループに要した時間（ミリ秒）
        /// </summary>
        public double UpdateTime { get; set; }

        /// <summary>
        /// LateUpdate更新ループに要した時間（ミリ秒）
        /// </summary>
        public double LateUpdateTime { get; set; }

        /// <summary>
        /// レンダリングに要した時間（ミリ秒）
        /// ただし、レンダースレッドの時間ではなくメインスレッド上でのレンダリング時間となる
        /// </summary>
        public double RenderingTime { get; set; }

        /// <summary>
        /// レンダーテクスチャのレンダリングに要した時間（ミリ秒）
        /// ただし、レンダースレッドの時間ではなくメインスレッド上でのレンダリング時間となる
        /// </summary>
        public double TextureRenderingTime { get; set; }



        /// <summary>
        /// 計測結果を更新します
        /// </summary>
        /// <param name="fixedCount">FixedUpdateのチックカウント</param>
        /// <param name="updateCount">Updateのチックカウント</param>
        /// <param name="lateCount">LateUpdateのチックカウント</param>
        /// <param name="renderingCount">レンダリングのチックカウント</param>
        /// <param name="renderTextureRenderingCount">レンダーテクスチャのレンダリングチックカウント</param>
        public void UpdateResult(long fixedCount, long updateCount, long lateCount, long renderingCount, long renderTextureRenderingCount)
        {
            // チックカウントの更新
            FixedUpdateTickCount = fixedCount;
            UpdateTickCount = updateCount;
            LateUpdateTickCount = lateCount;
            RenderingTickCount = renderingCount;
            TextureRenderingTickCount = renderTextureRenderingCount;


            // チックカウントからミリ秒へ計算して更新
            var tickToMillisec = Stopwatch.Frequency / 1000.0;
            FixedUpdateTime = fixedCount / tickToMillisec;
            UpdateTime = updateCount / tickToMillisec;
            LateUpdateTime = lateCount / tickToMillisec;
            RenderingTime = renderingCount / tickToMillisec;
            TextureRenderingTime = renderTextureRenderingCount / tickToMillisec;
        }
    }
}