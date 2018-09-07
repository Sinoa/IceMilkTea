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
        private static readonly Crc64TextCoder crc64TextCorder;
        private static readonly char[] integerToAsciiArray;



        /// <summary>
        /// StringExtensions クラスの初期化をします
        /// </summary>
        static StringExtensions()
        {
            // CRCテキストコーダの生成
            crc64TextCorder = new Crc64TextCoder();


            // 整数から16進数変換用配列の初期化
            integerToAsciiArray = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };
        }


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


        /// <summary>
        /// 文字列からCRC64計算された符号値の16進数表記へ変換します
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string ToCrc64HexText(this string text)
        {
            // まずは普通にCRC計算をする
            var code = ToCrc64Code(text);


            // 16桁の文字バッファを用意してループする
            var buffer = new char[16];
            for (int i = 0; i < buffer.Length; ++i)
            {
                // 最下位4bitから16進数の文字へ変換しバッファの後ろから詰めて、ビットシフトして繰り返す
                buffer[buffer.Length - (i + 1)] = integerToAsciiArray[code & 0x0F];
                code >>= 4;
            }


            // 出来上がったバッファを文字列として返す
            return new string(buffer);
        }
    }
}