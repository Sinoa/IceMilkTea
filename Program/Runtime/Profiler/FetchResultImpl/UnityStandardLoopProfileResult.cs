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

namespace IceMilkTea.Profiler
{
    /// <summary>
    /// UnityStandardLoopPerformanceProbe クラスによるパフォーマンス計測結果を保持するクラスです
    /// </summary>
    public class UnityStandardLoopProfileResult : ProfileFetchResult
    {
        /// <summary>
        /// Update更新ループに要した時間（ミリ秒）
        /// </summary>
        public double UpdateTime { get; set; }

        /// <summary>
        /// FixedUpdate更新ループに要した時間（ミリ秒）
        /// </summary>
        public double FixedUpdateTime { get; set; }

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
    }
}