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

namespace IceMilkTea.Core
{
    #region 最基底CRCクラス定義
    /// <summary>
    /// 巡回冗長検査アルゴリズムの基本となる実装を提供するクラスです。
    /// </summary>
    /// <remarks>
    /// CRCの実装については "https://en.wikipedia.org/wiki/Cyclic_redundancy_check" を参照して下さい
    /// </remarks>
    /// <typeparam name="T">CRCビットサイズに該当する符号なし整数型を指定します</typeparam>
    public abstract class CrcBase<T>
    {
        /// <summary>
        /// CRCテーブルを生成します
        /// </summary>
        /// <param name="polynomial">CRCで使用する多項式の値</param>
        /// <returns>生成したCRCテーブルを返します</returns>
        public abstract T[] CreateTable(T polynomial);


        /// <summary>
        /// 指定されたバッファ全体を、CRCの計算をします
        /// </summary>
        /// <remarks>
        /// この関数は、継続的にCRC計算をするのではなく、この関数の呼び出し一回で終了される事を想定します。
        /// </remarks>
        /// <param name="buffer">計算する対象のバッファ</param>
        /// <returns>計算された結果を返します</returns>
        public abstract T Calculate(byte[] buffer);


        /// <summary>
        /// 指定されたバッファの範囲を、CRCの計算を行います
        /// </summary>
        /// <remarks>
        /// この関数は、継続的にCRC計算をするのではなく、この関数の呼び出し一回で終了される事を想定します。
        /// </remarks>
        /// <param name="buffer">計算する対象のバッファ</param>
        /// <param name="index">バッファの開始位置</param>
        /// <param name="count">バッファから取り出す量</param>
        /// <returns>計算された結果を返します</returns>
        public abstract T Calculate(byte[] buffer, int index, int count);


        /// <summary>
        /// 指定されたバッファ全体を、CRCの計算をします
        /// </summary>
        /// <param name="continusHash">前回計算したハッシュ値、存在しない場合は既定値を指定</param>
        /// <param name="buffer">計算する対象のバッファ</param>
        /// <returns>計算された結果を返します</returns>
        public abstract T Calculate(T continusHash, byte[] buffer);


        /// <summary>
        /// 指定されたバッファの範囲を、CRCの計算を行います
        /// </summary>
        /// <param name="continusHash">前回計算したハッシュ値、存在しない場合は既定値を指定</param>
        /// <param name="buffer">計算する対象のバッファ</param>
        /// <param name="index">バッファの開始位置</param>
        /// <param name="count">バッファから取り出す量</param>
        /// <returns>計算された結果を返します</returns>
        public abstract T Calculate(T continusHash, byte[] buffer, int index, int count);


        /// <summary>
        /// 指定されたバッファの、CRC計算を行います
        /// </summary>
        /// <param name="buffer">計算する対象のポインタ</param>
        /// <param name="count">計算するサイズ</param>
        /// <returns>計算された結果を返します</returns>
        unsafe public abstract T Calculate(byte* buffer, int count);


        /// <summary>
        /// 指定されたバッファの、CRC計算を行います
        /// </summary>
        /// <param name="continusHash">前回計算したハッシュ値、存在しない場合は既定値を指定</param>
        /// <param name="buffer">計算する対象のポインタ</param>
        /// <param name="count">計算するサイズ</param>
        /// <returns>計算された結果を返します</returns>
        unsafe public abstract T Calculate(T continusHash, byte* buffer, int count);
    }
    #endregion


    #region CRC32基本クラス定義
    /// <summary>
    /// CRC32向けの基本クラスです。
    /// CRCの32bit長のクラスを実装する場合はこのクラスを継承して下さい。
    /// </summary>
    public abstract class Crc32Base : CrcBase<uint>
    {
        // 以下メンバ変数定義
        protected uint[] table;



        /// <summary>
        /// CRC32向けのテーブルを生成します
        /// </summary>
        /// <param name="polynomial">CRC32で利用する多項式の値</param>
        /// <returns>生成したCRCテーブルを返します</returns>
        public override uint[] CreateTable(uint polynomial)
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
        /// 指定されたバッファ全体を、CRC32の計算を行います
        /// </summary>
        /// <remarks>
        /// バッファが複数に分かれて、継続して計算する場合は、この関数が返したハッシュ値をそのまま continusHash パラメータに渡して計算を行って下さい。
        /// また、初回の計算をする前に continusHash へ uint.MaxValue をセットし、すべてのバッファ処理が終了後 uint.MaxValue の XOR 反転を行って下さい。
        /// </remarks>
        /// <param name="continusHash">前回計算したハッシュ値、存在しない場合は既定値を指定</param>
        /// <param name="buffer">計算する対象のバッファ</param>
        /// <returns>CRC32計算された結果を返します</returns>
        /// <exception cref="ArgumentNullException">buffer が null です</exception>
        unsafe public override uint Calculate(uint continusHash, byte[] buffer)
        {
            // バッファのアドレスを取得
            fixed (byte* p = buffer)
            {
                // ポインタ版関数を呼ぶ
                return Calculate(continusHash, p, buffer.Length);
            }
        }


        /// <summary>
        /// 指定されたバッファの範囲を、CRC32の計算を行います
        /// </summary>
        /// <remarks>
        /// バッファが複数に分かれて、継続して計算する場合は、この関数が返したハッシュ値をそのまま continusHash パラメータに渡して計算を行って下さい。
        /// また、初回の計算をする前に continusHash へ uint.MaxValue をセットし、すべてのバッファ処理が終了後 uint.MaxValue の XOR 反転を行って下さい。
        /// </remarks>
        /// <param name="continusHash">前回計算したハッシュ値、存在しない場合は既定値を指定</param>
        /// <param name="buffer">計算する対象のバッファ</param>
        /// <param name="index">バッファの開始位置</param>
        /// <param name="count">バッファから取り出す量</param>
        /// <returns>CRC32計算された結果を返します</returns>
        /// <exception cref="ArgumentNullException">buffer が null です</exception>
        /// <exception cref="ArgumentOutOfRangeException">index または index, count 合計値がbufferの範囲を超えます</exception>
        unsafe public override uint Calculate(uint continusHash, byte[] buffer, int index, int count)
        {
            // もし buffer が null なら
            if (buffer == null)
            {
                // そもそも計算が出来ない
                throw new ArgumentNullException(nameof(buffer));
            }


            // 指定された index と count で境界を超えないか確認して、超えるなら
            if (index < 0 || buffer.Length <= index + count)
            {
                // 境界を超えるアクセスは非常に危険
                throw new ArgumentOutOfRangeException($"{nameof(index)} or {nameof(count)}", $"指定された範囲では {nameof(buffer)} の範囲を超えます");
            }


            // バッファのアドレスを取得
            fixed (byte* p = buffer)
            {
                // ポインタ版関数を呼ぶ
                return Calculate(continusHash, p + index, count);
            }
        }


        /// <summary>
        /// 指定されたバッファの範囲を、CRC32の計算を行います
        /// </summary>
        /// <remarks>
        /// バッファが複数に分かれて、継続して計算する場合は、この関数が返したハッシュ値をそのまま continusHash パラメータに渡して計算を行って下さい。
        /// また、初回の計算をする前に continusHash へ uint.MaxValue をセットし、すべてのバッファ処理が終了後 uint.MaxValue の XOR 反転を行って下さい。
        /// </remarks>
        /// <param name="continusHash">前回計算したハッシュ値、存在しない場合は既定値を指定</param>
        /// <param name="buffer">計算する対象のポインタ</param>
        /// <param name="count">計算するバイトの数</param>
        /// <returns>CRC32計算された結果を返します</returns>
        /// <exception cref="ArgumentNullException">buffer が null です</exception>
        unsafe public override uint Calculate(uint continusHash, byte* buffer, int count)
        {
            // もし buffer が null なら
            if (buffer == null)
            {
                // そもそも計算が出来ない
                throw new ArgumentNullException(nameof(buffer));
            }


            // 指定バッファ範囲分ループする
            for (int i = 0; i < count; ++i)
            {
                // CRC計算をする
                continusHash = table[(*buffer ^ continusHash) & 0xFF] ^ (continusHash >> 8);
                ++buffer;
            }


            // 計算結果を返す
            return continusHash;
        }
    }
    #endregion


    #region CRC64基本クラス定義
    /// <summary>
    /// CRC64向けの基本クラスです。
    /// CRCの64bit長のクラスを実装する場合はこのクラスを継承して下さい。
    /// </summary>
    public abstract class Crc64Base : CrcBase<ulong>
    {
        // 以下メンバ変数定義
        protected ulong[] table;



        /// <summary>
        /// CRC64向けのテーブルを生成します
        /// </summary>
        /// <param name="polynomial">CRC64で利用する多項式の値</param>
        /// <returns>生成したCRCテーブルを返します</returns>
        public override ulong[] CreateTable(ulong polynomial)
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
        /// 指定されたバッファ全体を、CRC64の計算を行います
        /// </summary>
        /// <remarks>
        /// バッファが複数に分かれて、継続して計算する場合は、この関数が返したハッシュ値をそのまま continusHash パラメータに渡して計算を行って下さい。
        /// また、初回の計算をする前に continusHash へ ulong.MaxValue をセットし、すべてのバッファ処理が終了後 ulong.MaxValue の XOR 反転を行って下さい。
        /// </remarks>
        /// <param name="continusHash">前回計算したハッシュ値、存在しない場合は既定値を指定</param>
        /// <param name="buffer">計算する対象のバッファ</param>
        /// <returns>CRC64計算された結果を返します</returns>
        /// <exception cref="ArgumentNullException">buffer が null です</exception>
        unsafe public override ulong Calculate(ulong continusHash, byte[] buffer)
        {
            // バッファのアドレスを取得
            fixed (byte* p = buffer)
            {
                // ポインタ版関数を呼ぶ
                return Calculate(continusHash, p, buffer.Length);
            }
        }


        /// <summary>
        /// 指定されたバッファの範囲を、CRC64の計算を行います
        /// </summary>
        /// <remarks>
        /// バッファが複数に分かれて、継続して計算する場合は、この関数が返したハッシュ値をそのまま continusHash パラメータに渡して計算を行って下さい。
        /// また、初回の計算をする前に continusHash へ ulong.MaxValue をセットし、すべてのバッファ処理が終了後 ulong.MaxValue の XOR 反転を行って下さい。
        /// </remarks>
        /// <param name="continusHash">前回計算したハッシュ値、存在しない場合は既定値を指定</param>
        /// <param name="buffer">計算する対象のバッファ</param>
        /// <param name="index">バッファの開始位置</param>
        /// <param name="count">バッファから取り出す量</param>
        /// <returns>CRC64計算された結果を返します</returns>
        /// <exception cref="ArgumentNullException">buffer が null です</exception>
        /// <exception cref="ArgumentOutOfRangeException">index または index, count 合計値がbufferの範囲を超えます</exception>
        unsafe public override ulong Calculate(ulong continusHash, byte[] buffer, int index, int count)
        {
            // もし buffer が null なら
            if (buffer == null)
            {
                // そもそも計算が出来ない
                throw new ArgumentNullException(nameof(buffer));
            }


            // 指定された index と count で境界を超えないか確認して、超えるなら
            if (index < 0 || buffer.Length <= index + count)
            {
                // 境界を超えるアクセスは非常に危険
                throw new ArgumentOutOfRangeException($"{nameof(index)} or {nameof(count)}", $"指定された範囲では {nameof(buffer)} の範囲を超えます");
            }


            // バッファのアドレスを取得
            fixed (byte* p = buffer)
            {
                // ポインタ版関数を呼ぶ
                return Calculate(continusHash, p + index, count);
            }
        }


        /// <summary>
        /// 指定されたバッファの範囲を、CRC64の計算を行います
        /// </summary>
        /// <remarks>
        /// バッファが複数に分かれて、継続して計算する場合は、この関数が返したハッシュ値をそのまま continusHash パラメータに渡して計算を行って下さい。
        /// また、初回の計算をする前に continusHash へ ulong.MaxValue をセットし、すべてのバッファ処理が終了後 ulong.MaxValue の XOR 反転を行って下さい。
        /// </remarks>
        /// <param name="continusHash">前回計算したハッシュ値、存在しない場合は既定値を指定</param>
        /// <param name="buffer">計算する対象のポインタ</param>
        /// <param name="count">計算するバイトの数</param>
        /// <returns>CRC64計算された結果を返します</returns>
        /// <exception cref="ArgumentNullException">buffer が null です</exception>
        unsafe public override ulong Calculate(ulong continusHash, byte* buffer, int count)
        {
            // もし buffer が null なら
            if (buffer == null)
            {
                // そもそも計算が出来ない
                throw new ArgumentNullException(nameof(buffer));
            }


            // 指定バッファ範囲分ループする
            for (int i = 0; i < count; ++i)
            {
                // 右回りCRC計算をする
                continusHash = table[(*buffer ^ continusHash) & 0xFF] ^ (continusHash >> 8);
                ++buffer;
            }


            // 計算結果を返す
            return continusHash;
        }
    }
    #endregion


    #region ポピュラーなCRC32
    /// <summary>
    /// 非常にポピュラーなCRC32を提供するクラスです
    /// </summary>
    public class Crc32 : Crc32Base
    {
        /// <summary>
        /// ポピュラーな、右回りCRC32多項式の定数値です
        /// </summary>
        public const uint Polynomial = 0xEDB88320U;



        // クラス変数定義
        private static uint[] crcTable;



        /// <summary>
        /// CRC32のインスタンスの初期化を行います
        /// </summary>
        public Crc32()
        {
            // まだ共通のCRCテーブルが未生成なら
            if (crcTable == null)
            {
                // テーブルを構築する
                crcTable = CreateTable(Polynomial);
            }


            // 共通のテーブルの参照を設定する
            table = crcTable;
        }


        /// <summary>
        /// 指定されたバッファ全体を、CRCの計算をします
        /// </summary>
        /// <remarks>
        /// この関数は、継続的にCRC計算をするのではなく、この関数の呼び出し一回で終了される事を想定します。
        /// </remarks>
        /// <param name="buffer">計算する対象のバッファ</param>
        /// <returns>計算された結果を返します</returns>
        /// <exception cref="ArgumentNullException">buffer が null です</exception>
        unsafe public override uint Calculate(byte[] buffer)
        {
            // バッファのアドレスを取得
            fixed (byte* p = buffer)
            {
                // uint.MaxValueによるXOR反転を利用した計算を行い結果を返す
                return Calculate(uint.MaxValue, p, buffer.Length) ^ uint.MaxValue;
            }
        }


        /// <summary>
        /// 指定されたバッファの範囲を、CRCの計算を行います
        /// </summary>
        /// <remarks>
        /// この関数は、継続的にCRC計算をするのではなく、この関数の呼び出し一回で終了される事を想定します。
        /// </remarks>
        /// <param name="buffer">計算する対象のバッファ</param>
        /// <param name="index">バッファの開始位置</param>
        /// <param name="count">バッファから取り出す量</param>
        /// <returns>計算された結果を返します</returns>
        /// <exception cref="ArgumentNullException">buffer が null です</exception>
        /// <exception cref="ArgumentOutOfRangeException">index または index, count 合計値がbufferの範囲を超えます</exception>
        unsafe public override uint Calculate(byte[] buffer, int index, int count)
        {
            // もし buffer が null なら
            if (buffer == null)
            {
                // そもそも計算が出来ない
                throw new ArgumentNullException(nameof(buffer));
            }


            // 指定された index と count で境界を超えないか確認して、超えるなら
            if (index < 0 || buffer.Length <= index + count)
            {
                // 境界を超えるアクセスは非常に危険
                throw new ArgumentOutOfRangeException($"{nameof(index)} or {nameof(count)}", $"指定された範囲では {nameof(buffer)} の範囲を超えます");
            }


            // バッファのアドレスを取得
            fixed (byte* p = buffer)
            {
                // uint.MaxValueによるXOR反転を利用した計算を行い結果を返す
                return Calculate(uint.MaxValue, p + index, count) ^ uint.MaxValue;
            }
        }


        /// <summary>
        /// 指定されたバッファの範囲を、CRCの計算を行います
        /// </summary>
        /// <remarks>
        /// この関数は、継続的にCRC計算をするのではなく、この関数の呼び出し一回で終了される事を想定します。
        /// </remarks>
        /// <param name="buffer">計算する対象のポインタ</param>
        /// <param name="count">計算するバイトの数</param>
        /// <returns>計算された結果を返します</returns>
        /// <exception cref="ArgumentNullException">buffer が null です</exception>
        unsafe public override uint Calculate(byte* buffer, int count)
        {
            // uint.MaxValueによるXOR反転を利用した計算を行い結果を返す
            return Calculate(uint.MaxValue, buffer, count) ^ uint.MaxValue;
        }
    }
    #endregion


    #region CRC64-ISO
    /// <summary>
    /// CRC64-ISOを提供するクラスです
    /// </summary>
    public class Crc64Iso : Crc64Base
    {
        /// <summary>
        /// ISOで定義された、右回りCRC64多項式の定数値です
        /// </summary>
        public const ulong Polynomial = 0xD800000000000000UL;



        // クラス変数定義
        private static ulong[] crcTable;



        /// <summary>
        /// CRC64Isoのインスタンスの初期化を行います
        /// </summary>
        public Crc64Iso()
        {
            // まだ共通のCRCテーブルが未生成なら
            if (crcTable == null)
            {
                // テーブルを構築する
                crcTable = CreateTable(Polynomial);
            }


            // 共通のテーブルの参照を設定する
            table = crcTable;
        }


        /// <summary>
        /// 指定されたバッファ全体を、CRCの計算をします
        /// </summary>
        /// <remarks>
        /// この関数は、継続的にCRC計算をするのではなく、この関数の呼び出し一回で終了される事を想定します。
        /// </remarks>
        /// <param name="buffer">計算する対象のバッファ</param>
        /// <returns>計算された結果を返します</returns>
        /// <exception cref="ArgumentNullException">buffer が null です</exception>
        unsafe public override ulong Calculate(byte[] buffer)
        {
            // バッファのアドレスを取得
            fixed (byte* p = buffer)
            {
                // ulong.MaxValueによるXOR反転を利用した計算を行い結果を返す
                return Calculate(ulong.MaxValue, p, buffer.Length) ^ ulong.MaxValue;
            }
        }


        /// <summary>
        /// 指定されたバッファの範囲を、CRCの計算を行います
        /// </summary>
        /// <remarks>
        /// この関数は、継続的にCRC計算をするのではなく、この関数の呼び出し一回で終了される事を想定します。
        /// </remarks>
        /// <param name="buffer">計算する対象のバッファ</param>
        /// <param name="index">バッファの開始位置</param>
        /// <param name="count">バッファから取り出す量</param>
        /// <returns>計算された結果を返します</returns>
        /// <exception cref="ArgumentNullException">buffer が null です</exception>
        /// <exception cref="ArgumentOutOfRangeException">index または index, count 合計値がbufferの範囲を超えます</exception>
        unsafe public override ulong Calculate(byte[] buffer, int index, int count)
        {
            // もし buffer が null なら
            if (buffer == null)
            {
                // そもそも計算が出来ない
                throw new ArgumentNullException(nameof(buffer));
            }


            // 指定された index と count で境界を超えないか確認して、超えるなら
            if (index < 0 || buffer.Length <= index + count)
            {
                // 境界を超えるアクセスは非常に危険
                throw new ArgumentOutOfRangeException($"{nameof(index)} or {nameof(count)}", $"指定された範囲では {nameof(buffer)} の範囲を超えます");
            }


            // バッファのアドレスを取得
            fixed (byte* p = buffer)
            {
                // ulong.MaxValueによるXOR反転を利用した計算を行い結果を返す
                return Calculate(ulong.MaxValue, p + index, count) ^ ulong.MaxValue;
            }
        }


        /// <summary>
        /// 指定されたバッファの範囲を、CRCの計算を行います
        /// </summary>
        /// <remarks>
        /// この関数は、継続的にCRC計算をするのではなく、この関数の呼び出し一回で終了される事を想定します。
        /// </remarks>
        /// <param name="buffer">計算する対象のポインタ</param>
        /// <param name="count">計算するバイトの数</param>
        /// <returns>計算された結果を返します</returns>
        /// <exception cref="ArgumentNullException">buffer が null です</exception>
        unsafe public override ulong Calculate(byte* buffer, int count)
        {
            // ulong.MaxValueによるXOR反転を利用した計算を行い結果を返す
            return Calculate(ulong.MaxValue, buffer, count) ^ ulong.MaxValue;
        }
    }
    #endregion


    #region CRC64-ECMA
    /// <summary>
    /// CRC64-ECMAを提供するクラスです
    /// </summary>
    public class Crc64Ecma : Crc64Base
    {
        /// <summary>
        /// ECMAで定義された、右回りCRC64多項式の定数値です
        /// </summary>
        public const ulong Polynomial = 0xC96C5795D7870F42UL;



        // クラス変数定義
        private static ulong[] crcTable;



        /// <summary>
        /// CRC64Ecmaのインスタンスの初期化を行います
        /// </summary>
        public Crc64Ecma()
        {
            // まだ共通のCRCテーブルが未生成なら
            if (crcTable == null)
            {
                // テーブルを構築する
                crcTable = CreateTable(Polynomial);
            }


            // 共通のテーブルの参照を設定する
            table = crcTable;
        }


        /// <summary>
        /// 指定されたバッファ全体を、CRCの計算をします
        /// </summary>
        /// <remarks>
        /// この関数は、継続的にCRC計算をするのではなく、この関数の呼び出し一回で終了される事を想定します。
        /// </remarks>
        /// <param name="buffer">計算する対象のバッファ</param>
        /// <returns>計算された結果を返します</returns>
        /// <exception cref="ArgumentNullException">buffer が null です</exception>
        unsafe public override ulong Calculate(byte[] buffer)
        {
            // バッファのアドレスを取得
            fixed (byte* p = buffer)
            {
                // ulong.MaxValueによるXOR反転を利用した計算を行い結果を返す
                return Calculate(ulong.MaxValue, p, buffer.Length) ^ ulong.MaxValue;
            }
        }


        /// <summary>
        /// 指定されたバッファの範囲を、CRCの計算を行います
        /// </summary>
        /// <remarks>
        /// この関数は、継続的にCRC計算をするのではなく、この関数の呼び出し一回で終了される事を想定します。
        /// </remarks>
        /// <param name="buffer">計算する対象のバッファ</param>
        /// <param name="index">バッファの開始位置</param>
        /// <param name="count">バッファから取り出す量</param>
        /// <returns>計算された結果を返します</returns>
        /// <exception cref="ArgumentNullException">buffer が null です</exception>
        /// <exception cref="ArgumentOutOfRangeException">index または index, count 合計値がbufferの範囲を超えます</exception>
        unsafe public override ulong Calculate(byte[] buffer, int index, int count)
        {
            // もし buffer が null なら
            if (buffer == null)
            {
                // そもそも計算が出来ない
                throw new ArgumentNullException(nameof(buffer));
            }


            // 指定された index と count で境界を超えないか確認して、超えるなら
            if (index < 0 || buffer.Length <= index + count)
            {
                // 境界を超えるアクセスは非常に危険
                throw new ArgumentOutOfRangeException($"{nameof(index)} or {nameof(count)}", $"指定された範囲では {nameof(buffer)} の範囲を超えます");
            }


            // バッファのアドレスを取得
            fixed (byte* p = buffer)
            {
                // ulong.MaxValueによるXOR反転を利用した計算を行い結果を返す
                return Calculate(ulong.MaxValue, p + index, count) ^ ulong.MaxValue;
            }
        }


        /// <summary>
        /// 指定されたバッファの範囲を、CRCの計算を行います
        /// </summary>
        /// <remarks>
        /// この関数は、継続的にCRC計算をするのではなく、この関数の呼び出し一回で終了される事を想定します。
        /// </remarks>
        /// <param name="buffer">計算する対象のポインタ</param>
        /// <param name="count">計算するバイトの数</param>
        /// <returns>計算された結果を返します</returns>
        /// <exception cref="ArgumentNullException">buffer が null です</exception>
        unsafe public override ulong Calculate(byte* buffer, int count)
        {
            // ulong.MaxValueによるXOR反転を利用した計算を行い結果を返す
            return Calculate(ulong.MaxValue, buffer, count) ^ ulong.MaxValue;
        }
    }
    #endregion
}