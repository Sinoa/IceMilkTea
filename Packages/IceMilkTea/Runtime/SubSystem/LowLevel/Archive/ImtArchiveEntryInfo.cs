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

using System.Runtime.InteropServices;

namespace IceMilkTea.SubSystem
{
    /// <summary>
    /// アーカイブヘッダの ImtArchiveEntryInfo.Validate() 関数の結果を表す列挙型です。
    /// </summary>
    public enum ImtArchiveEntryInfoValidateResult
    {
        /// <summary>
        /// エントリ情報に問題はありません
        /// </summary>
        NoProblem,

        /// <summary>
        /// エントリIDが壊れています
        /// </summary>
        BrokenEntryId,

        /// <summary>
        /// エントリオフセットが不正です
        /// </summary>
        InvalidEntryOffset,

        /// <summary>
        /// エントリサイズが不正です
        /// </summary>
        InvalidEntrySize,

        /// <summary>
        /// 予約領域が壊れています
        /// </summary>
        BrokenReserved,
    }



    /// <summary>
    /// アーカイブに含まれるエントリの情報を表現する構造体です
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ImtArchiveEntryInfo
    {
        /// <summary>
        /// エントリIDです。
        /// 通常は、ファイル名のCRC64-Ecmaによって求められます。
        /// </summary>
        public ulong Id;

        /// <summary>
        /// エントリの実体が入っているアーカイブファイルの先頭からのオフセット。
        /// エントリの実体がない場合は 0 がセットされている必要があります。
        /// </summary>
        public long Offset;

        /// <summary>
        /// エントリの実体のサイズ。
        /// エントリの実体がない場合は 不定値 になる場合がある事に気をつけてください。
        /// </summary>
        public long Size;

        /// <summary>
        /// 予約領域です。
        /// 常にゼロで初期化されていなければいけません。
        /// </summary>
        public ulong Reserved;



        /// <summary>
        /// 該当のエントリ情報から実体がアーカイブに含まれているか確認をします
        /// </summary>
        public bool IsContainEntryData
        {
            get
            {
                // オフセットが0以外なら実体は存在する
                return Offset != 0;
            }
        }


        /// <summary>
        /// ファイルに書き込む際のサイズを確認します
        /// </summary>
        public static int InfoSize
        {
            get
            {
                // 本当は sizeof(ImtArchiveEntryInfo)でサクッとやりたいけど unsafe コードになるので、力技
                return
                    sizeof(ulong) + // Id
                    sizeof(ulong) + // Offset
                    sizeof(ulong) + // Size
                    sizeof(ulong); // Reserved
            }
        }



        /// <summary>
        /// エントリ情報が有効かどうか検証をします。
        /// </summary>
        /// <remarks>
        /// エントリ情報の整合性を確認するだけであり、アーカイブファイルそのものの整合性を保証するものではありません。
        /// </remarks>
        /// <param name="info">検証対象になるエントリ情報の参照</param>
        /// <returns>ヘッダ情報の検証結果に対する結果を返します。問題がなければ ImtArchiveHeaderValidateResult.NoProblem を返します。</returns>
        public static ImtArchiveEntryInfoValidateResult Validate(ref ImtArchiveEntryInfo info)
        {
            // エントリIDが0なら
            if (info.Id == 0)
            {
                // エントリIDが壊れている
                return ImtArchiveEntryInfoValidateResult.BrokenEntryId;
            }


            // オフセットが0以外で、アーカイブファイルヘッダ未満の値なら
            if (info.Offset != 0 && info.Offset < ImtArchiveHeader.HeaderSize)
            {
                // 明らかな不正値である
                return ImtArchiveEntryInfoValidateResult.InvalidEntryOffset;
            }


            // サイズが負の値なら
            if (info.Size < 0)
            {
                // 明らかな不正値である
                return ImtArchiveEntryInfoValidateResult.InvalidEntrySize;
            }


            // 予約領域が0以外なら
            if (info.Reserved != 0)
            {
                // 予約領域が壊れている
                return ImtArchiveEntryInfoValidateResult.BrokenReserved;
            }


            // 上記の検証を全てパスしたのなら、ひとまず問題はない
            return ImtArchiveEntryInfoValidateResult.NoProblem;
        }
    }
}