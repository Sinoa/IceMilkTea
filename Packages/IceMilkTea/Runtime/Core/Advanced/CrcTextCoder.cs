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

using System;
using System.Text;

namespace IceMilkTea.Core
{
    /// <summary>
    /// CRCを用いて、文字列を特定の整数型に符号化するクラスです
    /// </summary>
    /// <typeparam name="TPrimitiveType">符号化する整数の型</typeparam>
    public abstract class CrcTextCoder<TPrimitiveType>
    {
        // メンバ変数定義
        private Encoding utf8Encode;



        /// <summary>
        /// CrcTextCoder のインスタンスを初期化します
        /// </summary>
        public CrcTextCoder()
        {
            // エンコーディングとバッファの生成
            utf8Encode = new UTF8Encoding(false);
        }


        /// <summary>
        /// 文字列のエンコードを行ってからCRC計算を行います
        /// </summary>
        /// <param name="text">エンコードとCRC計算を行う文字列</param>
        /// <returns>計算された結果を返します</returns>
        unsafe protected TPrimitiveType ExecuteEncodeAndCalculate(string text)
        {
            // 文字列のポインタを拾う
            fixed (char* p = text)
            {
                // 必要とされるバッファのサイズを求めてバッファを確保する
                var needEncodeSize = utf8Encode.GetByteCount(text);
                var encodeBuffer = stackalloc byte[needEncodeSize];


                // エンコードをしてからCRC計算をして返す
                var encodedSize = utf8Encode.GetBytes(p, text.Length, encodeBuffer, needEncodeSize);
                return DoCrcCalculate(encodeBuffer, encodedSize);
            }
        }


        /// <summary>
        /// 指定されたバッファからCRC計算を行います
        /// </summary>
        /// <param name="buffer">計算対象のポインタ</param>
        /// <param name="count">計算するバイトの数</param>
        /// <returns>計算されたCRCの値を返します</returns>
        unsafe protected abstract TPrimitiveType DoCrcCalculate(byte* buffer, int count);


        /// <summary>
        /// 指定された文字列の符号値を取得します
        /// </summary>
        /// <param name="text">符号化する文字列</param>
        /// <returns>符号化された値を返します</returns>
        public abstract TPrimitiveType GetCode(string text);
    }



    /// <summary>
    /// CRC32を用いて、文字列を特定の整数型に符号化するクラスです
    /// </summary>
    public class Crc32TextCoder : CrcTextCoder<uint>
    {
        // メンバ変数定義
        private Crc32 crc;



        /// <summary>
        /// Crc32TextCoder のインスタンスを初期化します
        /// </summary>
        public Crc32TextCoder()
        {
            // CRCのインスタンスを生成する
            crc = new Crc32();
        }


        /// <summary>
        /// 指定されたバッファからCRC計算を行います
        /// </summary>
        /// <param name="buffer">計算対象のポインタ</param>
        /// <param name="count">計算するバイトの数</param>
        /// <returns>計算されたCRCの値を返します</returns>
        unsafe protected override uint DoCrcCalculate(byte* buffer, int count)
        {
            // CRC計算した結果を返す
            return crc.Calculate(buffer, count);
        }


        /// <summary>
        /// 指定された文字列の符号値を取得します
        /// </summary>
        /// <param name="text">符号化する文字列</param>
        /// <returns>符号化された値を返します</returns>
        /// <exception cref="ArgumentNullException">text が null です</exception>
        public override uint GetCode(string text)
        {
            // null を渡されたら
            if (text == null)
            {
                // 処理は出来ない
                throw new ArgumentNullException(nameof(text));
            }


            // エンコードとCRC計算した結果を返す
            return ExecuteEncodeAndCalculate(text);
        }
    }



    /// <summary>
    /// CRC64-Ecmaを用いて、文字列を特定の整数型に符号化するクラスです
    /// </summary>
    public class Crc64TextCoder : CrcTextCoder<ulong>
    {
        // メンバ変数定義
        private Crc64Ecma crc;



        /// <summary>
        /// Crc64TextCoder のインスタンスを初期化します
        /// </summary>
        public Crc64TextCoder()
        {
            // CRCのインスタンスを生成する
            crc = new Crc64Ecma();
        }


        /// <summary>
        /// 指定されたバッファからCRC計算を行います
        /// </summary>
        /// <param name="buffer">計算対象のポインタ</param>
        /// <param name="count">計算するバイトの数</param>
        /// <returns>計算されたCRCの値を返します</returns>
        unsafe protected override ulong DoCrcCalculate(byte* buffer, int count)
        {
            // CRC計算した結果を返す
            return crc.Calculate(buffer, count);
        }


        /// <summary>
        /// 指定された文字列の符号値を取得します
        /// </summary>
        /// <param name="text">符号化する文字列</param>
        /// <returns>符号化された値を返します</returns>
        /// <exception cref="ArgumentNullException">text が null です</exception>
        public override ulong GetCode(string text)
        {
            // null を渡されたら
            if (text == null)
            {
                // 処理は出来ない
                throw new ArgumentNullException(nameof(text));
            }


            // エンコードとCRC計算した結果を返す
            return ExecuteEncodeAndCalculate(text);
        }
    }
}