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

using System.Text;
using UnityEngine;

namespace IceMilkTea.Profiler
{
    /// <summary>
    /// パフォーマンス計測結果をDebugLogの機能を使って出力をします
    /// </summary>
    public class DebugLogPerformanceRenderer : PerformanceRenderer
    {
        // メンバ変数宣言
        private ProfileFetchResult[] results;



        /// <summary>
        /// 出力の準備を行います。
        /// </summary>
        /// <param name="profileFetchResults"></param>
        public override void Begin(ProfileFetchResult[] profileFetchResults)
        {
            // プロファイル結果を覚える
            results = profileFetchResults;
        }


        /// <summary>
        /// 出力を終了します。
        /// このクラスでは、何もしません
        /// </summary>
        public override void End()
        {
        }


        /// <summary>
        /// プロファイル結果を出力します
        /// </summary>
        public override void Render()
        {
            // 文字列バッファ
            var buffer = new StringBuilder();


            // 書き込まれた文字列を吐き出す
            Debug.Log(buffer.ToString());
        }
    }
}