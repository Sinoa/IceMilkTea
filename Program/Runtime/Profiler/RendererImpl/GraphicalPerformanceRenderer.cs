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

using UnityEngine;

namespace IceMilkTea.Profiler
{
    /// <summary>
    /// パフォーマンス計測結果をユーザーが視認出来るディスプレイへ、グラフィカルにレンダリングするクラスです
    /// </summary>
    public class GraphicalPerformanceRenderer : PerformanceRenderer
    {
        /// <summary>
        /// 出力の準備を行います
        /// </summary>
        /// <param name="profileFetchResults">パフォーマンスモニタから渡されるすべての計測結果の配列</param>
        public override void Begin(ProfileFetchResult[] profileFetchResults)
        {
        }


        /// <summary>
        /// 出力を終了します
        /// </summary>
        public override void End()
        {
        }


        /// <summary>
        /// プロファイル結果をレンダリングします
        /// </summary>
        public override void Render()
        {
        }
    }
}