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

using System.Collections.Generic;

namespace IceMilkTea.Core
{
    /// <summary>
    /// 配列(Array) の拡張関数実装用クラスです
    /// </summary>
    public static class ArrayExtensions
    {
        /// <summary>
        /// 比較元配列の内容と比較先配列の内容がすべて一致するか否かを判定します
        /// </summary>
        /// <typeparam name="T">比較する配列の元の型</typeparam>
        /// <param name="left">比較する元の配列</param>
        /// <param name="right">比較する先の配列</param>
        /// <returns>配列のすべてが一致する場合は true を、異なる場合は false を返します</returns>
        public static bool IsSameAll<T>(this T[] left, T[] right)
        {
            // どちらも同じ参照を持つようなら
            if (ReferenceEquals(left, right))
            {
                // 同じ配列であることを返す
                return true;
            }


            // どちらかが null なら
            if (right == null || left == null)
            {
                // そもそも比較しようがないので異なることを返す
                return false;
            }


            // 長さが一致しないなら
            if (left.Length != right.Length)
            {
                // 長さが異なるなら一致するはずがない
                return false;
            }


            // T型の比較オブジェクトを用意
            var comparer = EqualityComparer<T>.Default;


            // 素直に頭からループ
            for (int i = 0; i < left.Length; ++i)
            {
                // もし異なる値を示したのなら
                if (!comparer.Equals(left[i], right[i]))
                {
                    // この時点で異なる値として返す
                    return false;
                }
            }


            // ループから抜けてきたというのなら一致していることになる
            return true;
        }
    }
}