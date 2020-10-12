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

namespace IceMilkTea.Core
{
    /// <summary>
    /// 値の変換を行う拡張関数実装用クラスです
    /// </summary>
    public static class ValueConvertExtensions
    {
        /// <summary>
        /// 符号なし32bit整数からカラーへ変換します（RGBA32bit）
        /// </summary>
        /// <param name="value">RGBA32形式の符号なし32bit整数</param>
        /// <returns>変換されたカラーオブジェクトを返します</returns>
        public static Color ToColor(this uint value)
        {
            // ビット演算を活用してRGBA変換して返す
            return new Color32((byte)((value >> 24) & 0xFF), (byte)((value >> 16) & 0xFF), (byte)((value >> 8) & 0xFF), (byte)(value & 0xFF));
        }
    }
}