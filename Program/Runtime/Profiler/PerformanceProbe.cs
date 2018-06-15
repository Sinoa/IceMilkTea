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
    /// 特定のパフォーマンスを計測するプローブの表現をする抽象クラスです。
    /// オリジナルのパフォーマンス計測をする場合は、このプローブクラスを継承し実装をしてください。
    /// </summary>
    public abstract class PerformanceProbe
    {
        /// <summary>
        /// パフォーマンスの計測結果を取得します。
        /// </summary>
        public abstract ProfileFetchResult ProfileResult { get; protected set; }



        /// <summary>
        /// パフォーマンスの計測を開始します。
        /// この関数が呼び出されるのは、Unityのおおよそのフレーム開始タイミングにて呼び出されます。
        /// </summary>
        public abstract void Start();


        /// <summary>
        /// パフォーマンスの計測停止します。
        /// この関数が呼び出されるのは、Unityのおおよそのフレーム終了タイミングにて呼び出されます。
        /// </summary>
        public abstract void Stop();
    }
}