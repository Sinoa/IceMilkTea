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

namespace IceMilkTea.Core
{
    /// <summary>
    /// System.String(string) クラスの拡張関数実装用クラスです
    /// </summary>
    public static class StringExtensions
    {
        // クラス変数宣言
        private static readonly Crc64TextCoder crc64TextCorder = new Crc64TextCoder();



        /// <summary>
        /// 文字列からCRC64計算された符号値へ変換します
        /// </summary>
        /// <param name="text">符号変換する文字列</param>
        /// <returns>符号変換された値を返します</returns>
        public static ulong ToCrc64Code(this string text)
        {
            // CRC64で符号化したものを返す
            return crc64TextCorder.GetCode(text);
        }
    }
}