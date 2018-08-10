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
using System.Runtime.InteropServices;

namespace IceMilkTea.Module
{
    /// <summary>
    /// アーカイブヘッダの ImtArchiveHeader.Validate() 関数の結果を表す列挙型です。
    /// </summary>
    public enum ImtArchiveHeaderValidateResult
    {
        /// <summary>
        /// アーカイブヘッダに問題はありません
        /// </summary>
        NoProblem,

        /// <summary>
        /// マジックナンバーが壊れています
        /// </summary>
        BrokenMagicNumber,

        /// <summary>
        /// アーカイブバージョンが不正です
        /// </summary>
        InvalidVersion,

        /// <summary>
        /// アーカイブ情報が壊れています
        /// </summary>
        BrokenArchiveInfo,

        /// <summary>
        /// エントリ情報リストオフセットが不正です
        /// </summary>
        InvalidEntryInfoListOffset,

        /// <summary>
        /// エントリ情報の値が不正です
        /// </summary>
        InvalidEntryInfoCount,

        /// <summary>
        /// 予約領域が壊れています
        /// </summary>
        BrokenReserved,
    }



    /// <summary>
    /// アーカイブ本体のヘッダ構造を表現する構造体です
    /// </summary>
    /// <remarks>
    /// 実際に情報として利用する場合は CreateArchiveHeader() 関数から生成することを検討してください
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public struct ImtArchiveHeader
    {
        /// <summary>
        /// IceMilkTeaArchiveの仕様バージョン
        /// </summary>
        public const byte IceMilkTeaArchiveSpecVersion = 1;



        /// <summary>
        /// アーカイブファイルのファイル識別子データです。
        /// データは([0]=0x49, [1]=0x4D, [2]=0x54, [3]=0x41)の順でセットされていなければなりません
        /// </summary>
        public byte[] MagicNumber;

        /// <summary>
        /// アーカイブ情報を格納した符号なし32bit整数です。
        /// </summary>
        /// <remarks>
        /// 通常はこのフィールドを直接触れるのではなく必要なプロパティからアクセスするようにしてください。
        /// </remarks>
        public uint ArchiveInfo;

        /// <summary>
        /// アーカイブファイルに含まれたエントリ情報リストへのオフセット。
        /// ファイルの先頭からのへの、オフセットとなります。
        /// </summary>
        public long EntryInfoListOffset;

        /// <summary>
        /// アーカイブファイルに含まれたエントリ情報リストの要素数。
        /// エントリの実体の数とは一致しない可能性がある事に気をつけてください。
        /// </summary>
        public int EntryInfoCount;

        /// <summary>
        /// 予約領域です。常にゼロであるべきです。
        /// </summary>
        public uint Reserved;



        /// <summary>
        /// アーカイブバージョンの取り出しと設定をします
        /// </summary>
        /// <remarks>
        /// このプロパティは ArchiveInfo フィードを操作するので
        /// </remarks>
        public byte ArchiveVersion
        {
            get
            {
                // ArchiveInfoの下位1バイトがバージョンデータが含まれる
                return (byte)(ArchiveInfo & 0xFF);
            }
            set
            {
                // エンディアン関係なく下位1バイトにバージョンデータを入れる
                ArchiveInfo = (ArchiveInfo & 0xFFFFFF00) | value;
            }
        }


        /// <summary>
        /// ファイルに書き込む際のサイズを確認します
        /// </summary>
        public static int HeaderSize
        {
            get
            {
                // 本当は sizeof(ImtArchiveHeader)でサクッとやりたいけど unsafe コードになるので、力技
                return
                    sizeof(byte) * 4 + // MagicNumber
                    sizeof(uint) + // ArchiveInfo
                    sizeof(long) + // EntryInfoListOffset
                    sizeof(int) + // EntryInfoCount
                    sizeof(uint); // Reserved
            }
        }



        /// <summary>
        /// 新しいアーカイブヘッダを生成します。
        /// </summary>
        /// <param name="header">生成したヘッダを受け取る ImtArchiveHeader の参照</param>
        public static void CreateArchiveHeader(out ImtArchiveHeader header)
        {
            // マジックナンバーの初期化
            header.MagicNumber = new byte[4];
            CreateMagicNumber(header.MagicNumber, 0);


            // アーカイブ情報はバージョンを設定するだけ
            header.ArchiveInfo = IceMilkTeaArchiveSpecVersion;


            // エントリ情報リストのオフセットは最低でもアーカイブヘッダの長さ分は存在しないとおかしい
            header.EntryInfoListOffset = HeaderSize;


            // エントリ情報の数は0件
            header.EntryInfoCount = 0;


            // 予約領域は0でなければならない
            header.Reserved = 0;
        }


        /// <summary>
        /// 指定されたバッファと位置から、マジックナンバーを生成します。
        /// </summary>
        /// <param name="buffer">マジックナンバーを受け入れるバッファ</param>
        /// <param name="index">マジックナンバーを設定する位置</param>
        /// <exception cref="ArgumentException">buffer の長さが最低でも4以上なければなりません</exception>
        /// <exception cref="ArgumentNullException">buffer が null です</exception>
        /// <exception cref="ArgumentOutOfRangeException">指定された位置では、配列の境界を超えてしまいます</exception>
        public static void CreateMagicNumber(byte[] buffer, int index)
        {
            // そもそもbufferがnullなら
            if (buffer == null)
            {
                // 受付できないです
                throw new ArgumentNullException(nameof(buffer));
            }


            // バッファの長さが4未満なら
            if (buffer.Length < 4)
            {
                // マジックナンバーを受け入れられる空きがない
                throw new ArgumentException($"{nameof(buffer)} の長さが最低でも4以上でなければなりません", nameof(buffer));
            }


            // 指定されたインデックスからの場合の長さが4未満または、負の値なら
            if (index < 0 || (buffer.Length - index) < 4)
            {
                // 配列の境界を超えることは許さない
                throw new ArgumentOutOfRangeException(nameof(index), "指定された位置では、配列の境界を超えてしまいます");
            }


            // バッファにマジックナンバー（'I' 'M' 'T' 'A'）を込める
            buffer[index + 0] = 0x49; // 'I'
            buffer[index + 1] = 0x4D; // 'M'
            buffer[index + 2] = 0x54; // 'T'
            buffer[index + 3] = 0x41; // 'A'
        }


        /// <summary>
        /// ヘッダ情報が有効かどうか検証をします。
        /// </summary>
        /// <remarks>
        /// ヘッダの整合性を確認するだけであり、アーカイブファイルそのものの整合性を保証するものではありません。
        /// </remarks>
        /// <param name="header">整合性を検証するヘッダ構造体の参照</param>
        /// <returns>ヘッダ情報の検証結果に対する結果を返します。問題がなければ ImtArchiveHeaderValidateResult.NoProblem を返します。</returns>
        public static ImtArchiveHeaderValidateResult Validate(ref ImtArchiveHeader header)
        {
            // マジックナンバー配列がそもそもnullまたは、長さが4以外なら
            if (header.MagicNumber == null || header.MagicNumber.Length != 4)
            {
                // 未初期化も長さ一致しないのもダメ
                return ImtArchiveHeaderValidateResult.BrokenMagicNumber;
            }


            // マジックナンバーの各要素が既定値以外なら
            if (header.MagicNumber[0] != 0x49 || header.MagicNumber[1] != 0x4D || header.MagicNumber[2] != 0x54 || header.MagicNumber[3] != 0x41)
            {
                // マジックナンバーの状態がよろしくない
                return ImtArchiveHeaderValidateResult.BrokenMagicNumber;
            }


            // アーカイブ情報のバージョンが仕様バージョンと一致していないなら
            // （将来のバージョンで下位互換などがある場合は、判定方法を変える）
            if (header.ArchiveVersion != IceMilkTeaArchiveSpecVersion)
            {
                // バージョン不一致はダメ
                return ImtArchiveHeaderValidateResult.InvalidVersion;
            }


            // アーカイブ情報のバージョンビット以外(Reserved bit)に0初期化されていないのなら
            if ((header.ArchiveInfo & 0xFFFFFFFF00) != 0)
            {
                // アーカイブ情報が壊れている
                return ImtArchiveHeaderValidateResult.BrokenArchiveInfo;
            }


            // エントリ情報リストのオフセットがアーカイブヘッダサイズ未満なら
            if (header.EntryInfoListOffset < HeaderSize)
            {
                // エントリ情報リストオフセットは不正な値を持っている
                return ImtArchiveHeaderValidateResult.InvalidEntryInfoListOffset;
            }


            // エントリ情報の数が負の値なら
            if (header.EntryInfoCount < 0)
            {
                // エントリ情報の数が不正な値を持っている
                return ImtArchiveHeaderValidateResult.InvalidEntryInfoCount;
            }


            // 予約領域が0以外なら
            if (header.Reserved != 0)
            {
                // 予約領域が壊れている
                return ImtArchiveHeaderValidateResult.BrokenReserved;
            }


            // 上記全ての判定をパスしたのならヘッダに問題は無いとする
            return ImtArchiveHeaderValidateResult.NoProblem;
        }
    }
}