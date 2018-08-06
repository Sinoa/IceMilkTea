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
    #region CRC実装
    /// <summary>
    /// 巡回冗長検査アルゴリズムの中心となる実装を提供するクラスです。
    /// </summary>
    /// <remarks>
    /// CRCの実装については "https://en.wikipedia.org/wiki/Cyclic_redundancy_check" を参照して下さい
    /// </remarks>
    public static class CrcCore
    {
        /// <summary>
        /// ポピュラーな、右回りCRC32多項式の定数値です
        /// </summary>
        public const uint PolynomialCrc32 = 0xEDB88320U;

        /// <summary>
        /// ISOで定義された、右回りCRC64多項式の定数値です
        /// </summary>
        public const ulong PolynomialCrc64Iso = 0xD800000000000000UL;

        /// <summary>
        /// ECMAで定義された、右回りCRC64多項式の定数値です
        /// </summary>
        public const ulong PolynomialCrc64Ecma = 0xC96C5795D7870F42UL;

        /// <summary>
        /// CRC32計算をする際に渡す最初のハッシュ値
        /// </summary>
        public const uint InitialCrc32HashValue = 0xFFFFFFFFU;

        /// <summary>
        /// CRC64計算をする際に渡す最初のハッシュ値
        /// </summary>
        public const ulong InitialCrc64HashValue = 0x0UL;



        // クラス変数宣言
        private static uint[] crc32Table;
        private static ulong[] crc64Table;



        /// <summary>
        /// CrcCore クラスの初期化を行います。
        /// 内部のCRCテーブルの既定初期化は、CRC32、CRC64Isoです。
        /// </summary>
        static CrcCore()
        {
            // CRCテーブルを構築する
            RebuildTable(PolynomialCrc32);
            RebuildTable(PolynomialCrc64Iso);
        }


        /// <summary>
        /// CRC32用のテーブルを再構築します
        /// </summary>
        /// <param name="polynomial">CRC32で利用する多項式の値</param>
        public static void RebuildTable(uint polynomial)
        {
            // テーブルを作る
            crc32Table = CreateTable(polynomial);
        }


        /// <summary>
        /// CRC64用のテーブルを再構築します
        /// </summary>
        /// <param name="polynomial">CRC64で利用する多項式の値</param>
        public static void RebuildTable(ulong polynomial)
        {
            // テーブルを作る
            crc64Table = CreateTable(polynomial);
        }


        /// <summary>
        /// CRC32向けのテーブルを生成します
        /// </summary>
        /// <param name="polynomial">CRC32で利用する多項式の値</param>
        /// <returns>生成されたCRCテーブルを返します</returns>
        public static uint[] CreateTable(uint polynomial)
        {
            // テーブル用配列を生成
            var table = new uint[256];


            // テーブルの要素分ループする
            for (uint i = 0U; i < (uint)table.Length; ++i)
            {
                // 要素の計算を行う
                uint num = ((i & 1) * polynomial) ^ (i >> 1);
                num = ((num & 1) * polynomial) ^ (num >> 1);
                num = ((num & 1) * polynomial) ^ (num >> 1);
                num = ((num & 1) * polynomial) ^ (num >> 1);
                num = ((num & 1) * polynomial) ^ (num >> 1);
                num = ((num & 1) * polynomial) ^ (num >> 1);
                num = ((num & 1) * polynomial) ^ (num >> 1);
                num = ((num & 1) * polynomial) ^ (num >> 1);


                // 計算結果を入れる
                table[i] = num;
            }


            // 生成されたテーブルを返す
            return table;
        }


        /// <summary>
        /// CRC64向けのテーブルを生成します
        /// </summary>
        /// <param name="polynomial">CRC64で利用する多項式の値</param>
        /// <returns>生成されたCRCテーブルを返します</returns>
        public static ulong[] CreateTable(ulong polynomial)
        {
            // テーブル用配列を生成
            var table = new ulong[256];


            // テーブルの要素分ループする
            for (ulong i = 0UL; i < (ulong)table.Length; ++i)
            {
                // 要素の計算を行う
                ulong num = ((i & 1) * polynomial) ^ (i >> 1);
                num = ((num & 1) * polynomial) ^ (num >> 1);
                num = ((num & 1) * polynomial) ^ (num >> 1);
                num = ((num & 1) * polynomial) ^ (num >> 1);
                num = ((num & 1) * polynomial) ^ (num >> 1);
                num = ((num & 1) * polynomial) ^ (num >> 1);
                num = ((num & 1) * polynomial) ^ (num >> 1);
                num = ((num & 1) * polynomial) ^ (num >> 1);


                // 計算結果を入れる
                table[i] = num;
            }


            // 生成されたテーブルを返す
            return table;
        }


        /// <summary>
        /// 指定されたバッファ全体を、CRC32の計算を行います
        /// </summary>
        /// <remarks>
        /// バッファが複数に分かれて、継続して計算する場合は、この関数が返したハッシュ値をそのまま continusHash パラメータに渡して計算を行って下さい。
        /// また、CRC32を計算する場合に continusHash に InitialCrc32HashValue を入れてから、すべてのバッファの処理が終わったら InitialCrc32HashValue の XOR 反転を行って下さい。
        /// </remarks>
        /// <param name="continusHash">前回計算したハッシュ値、存在しない場合は既定値を指定</param>
        /// <param name="buffer">計算する対象のバッファ</param>
        /// <returns>CRC32計算された結果を返します</returns>
        public static uint Calculate(uint continusHash, byte[] buffer)
        {
            // 渡されたバッファ全体を計算する
            return Calculate(continusHash, buffer, 0, buffer.Length);
        }


        /// <summary>
        /// 指定されたバッファ全体を、CRC64の計算を行います
        /// </summary>
        /// <remarks>
        /// バッファが複数に分かれて、継続して計算する場合は、この関数が返したハッシュ値をそのまま continusHash パラメータに渡して計算を行って下さい。
        /// </remarks>
        /// <param name="continusHash">前回計算したハッシュ値、存在しない場合は既定値を指定</param>
        /// <param name="buffer">計算する対象のバッファ</param>
        /// <returns>CRC64計算された結果を返します</returns>
        public static ulong Calculate(ulong continusHash, byte[] buffer)
        {
            // 渡されたバッファ全体を計算する
            return Calculate(continusHash, buffer, 0, buffer.Length);
        }


        /// <summary>
        /// 指定されたバッファの範囲を、CRC32の計算を行います
        /// </summary>
        /// <remarks>
        /// バッファが複数に分かれて、継続して計算する場合は、この関数が返したハッシュ値をそのまま continusHash パラメータに渡して計算を行って下さい。
        /// また、CRC32を計算する場合に continusHash に InitialCrc32HashValue を入れてから、すべてのバッファの処理が終わったら InitialCrc32HashValue の XOR 反転を行って下さい。
        /// </remarks>
        /// <param name="continusHash">前回計算したハッシュ値、存在しない場合は既定値を指定</param>
        /// <param name="buffer">計算する対象のバッファ</param>
        /// <param name="index">バッファの開始位置</param>
        /// <param name="count">バッファから取り出す量</param>
        /// <returns>CRC32計算された結果を返します</returns>
        public static uint Calculate(uint continusHash, byte[] buffer, int index, int count)
        {
            // 指定バッファ範囲分ループする
            var limit = index + count;
            for (int i = index; i < limit; ++i)
            {
                // 右回りCRC計算をする
                continusHash = crc32Table[(buffer[i] ^ continusHash) & 0xFF] ^ (continusHash >> 8);
            }


            // 計算結果を返す
            return continusHash;
        }


        /// <summary>
        /// 指定されたバッファの範囲を、CRC64の計算を行います
        /// </summary>
        /// <remarks>
        /// バッファが複数に分かれて、継続して計算する場合は、この関数が返したハッシュ値をそのまま continusHash パラメータに渡して計算を行って下さい。
        /// </remarks>
        /// <param name="continusHash">前回計算したハッシュ値、存在しない場合は既定値を指定</param>
        /// <param name="buffer">計算する対象のバッファ</param>
        /// <param name="index">バッファの開始位置</param>
        /// <param name="count">バッファから取り出す量</param>
        /// <returns>CRC64計算された結果を返します</returns>
        public static ulong Calculate(ulong continusHash, byte[] buffer, int index, int count)
        {
            // 指定バッファ範囲分ループする
            var limit = index + count;
            for (int i = 0; i < limit; ++i)
            {
                // 右回りCRC計算をする
                continusHash = crc64Table[(buffer[i] ^ continusHash) & 0xFF] ^ (continusHash >> 8);
            }


            // 計算結果を返す
            return continusHash;
        }
    }
    #endregion
}