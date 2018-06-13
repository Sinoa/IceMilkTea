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
    /// Unityの標準ループのパフォーマンスを計測するプローブクラスです。
    /// </summary>
    public class UnityStandardLoopPerformanceProbe : PerformanceProbe
    {
        // メンバ変数宣言
        private Stopwatch stopwatch;
        private long updateStartCount;
        private long updateEndCount;
        private long fixedUpdateStartCount;
        private long fixedUpdateEndCount;
        private long lateUpdateStartCount;
        private long lateUpdateEndCount;
        private long renderingStartCount;
        private long renderingEndCount;
        private long textureRenderingStartCount;
        private long textureRenderingEndCount;



        /// <summary>
        /// パフォーマンス計測結果を取得します
        /// </summary>
        public override ProfileFetchResult ProfileResult { get; protected set; } = new UnityStandardLoopProfileResult();



        /// <summary>
        /// インスタンスの初期化を行います
        /// </summary>
        public UnityStandardLoopPerformanceProbe()
        {
            // 計測用ストップウォッチを生成
            stopwatch = Stopwatch.StartNew();
        }


        /// <summary>
        /// 計測を開始します
        /// </summary>
        public override void Start()
        {
            // 計測カウンタの初期化をする
            renderingStartCount = long.MaxValue;
            renderingEndCount = 0;
            textureRenderingStartCount = long.MaxValue;
            textureRenderingEndCount = 0;
        }


        /// <summary>
        /// 計測を終了します
        /// </summary>
        public override void Stop()
        {
        }
    }
}