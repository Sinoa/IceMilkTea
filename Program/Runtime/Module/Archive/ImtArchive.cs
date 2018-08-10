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

#region IceMilkTeaArchive Specification Document
/**********************************************************************
--------------------------------
IceMilkTeaArchive 仕様書 ver 0.1
--------------------------------

目次（Index）
 0. べき、なければならない、もよい、ます（ません）、について
 1. 背景＆目的（Introduction）
 2. アーカイブファイルフォーマット（ArchiveFile Format）
 3. エントリ情報フォーマット（EntryInfo Format）



0. べき、ならない、もよい、ます（ません）、について
------------------------------------------------------------
　この仕様書では、各文に含まれる語句に
「～べき」「～べきである」「～ならない」「～もよい」「～ます」
といった、単語を使い分けています。
仕様書をよく読んで、独自で実装する場合は、単語の使い分けに注意してください。

各単語の表現は次のように定義しています。
- ～べき、～べきである、～ならない、～なりません
 - 内容の事柄について、厳守しなければなりません、内容と異なる動作をした場合は
   仕様に対する動作違反として、見做されます。

- ～もよい、～です、～ます
 - 内容の事柄について、仕様として定義しておりますが、該当の挙動や動作を表現するのではなく
   実装の方針を決定しやすくするために、記述していることがしばしばあります。
   場合によっては、動作を実装しなくても良い場合もありますし、動作を強要することもありません。
   しかし、実際として厳守する内容ではなくとも、逸脱した実装は許容しません。
   つまり、実際の挙動によって、この仕様書で厳守すべき定義された内容の一部でも異なってしまう場合は
   許される事ではありません。

　また、ここに列挙した単語以外にも上記に近い単語で表現する箇所がありますが
極力、仕様に対して正確な実装をするのが、良いです。



1. 背景＆目的（Introduction）
------------------------------------------------------------
　近年、コンピュータのストレージのハードウェア構成がかなり成長し
非常に小型でありながら、大容量、高通信レート、高耐久性、を実現されており
大抵の場合、ファイルの操作に対するユーザーストレスは格段に下がっています。

　しかしながら、現在においてハードウェアが進化すると共に、ファイルシステム自体も
機能の肥大化（いわゆるジャーナリング、暗号保護といったソフトウェア的肥大化）しており
結果としては、ソフトウェア動作に対するパフォーマンスの多大な影響を与えている事があります。
実際のところゲームのアセットデータを扱う場合、実装次第ではまだまだ
パフォーマンスに悪影響を与えることがあります。

　IceMilkTeaArchiveは、主にUnityのアセットバンドルを一つのアーカイブデータとして
管理と保持を行い、ゲームに対してあたかも普通の１アセットバンドルファイルかのように、機能を提供します。
アーカイブファイルとして持つことにより、ファイルシステムそのものの影響を極力抑えることが可能になります。

　機能としては、Unityのアセットバンドルに最適な実装をする事がありますが
決してUnityの機能そのものを使った、実装を提供することはありません。
理由については、後述しています。

　また、IceMilkTeaArchiveは、通常のアーカイブファイルとして振る舞うため
アセットバンドル以外のファイルも格納が可能で、通常のファイルのように振る舞います。
つまり、Unityとしての機能ではなく、単純なアーカイバとして振る舞う機能を提供します。
なので、Unityの機能を使った実装を行うことはありません。

　さらに、ファイルを読み込みながら別のデータを同時に書き込む事が可能で、オンデマンドな
データ更新をする機能も提供します。



2. アーカイブファイルフォーマット（ArchiveFile Format）
------------------------------------------------------------
　IceMilkTeaArchiveのアーカイブファイルの全体構造は以下に定義されており
その様に格納されていなければなりません。
また、アーカイブファイルのデータはリトルエンディアンとして格納されていなければなりません。

+-----------------------------------------------------------+
|          (Entire) IceMilkTeaArchive File Format           |
+-------------------------+--------------+------------------+
|  MagicNumber (Fixed)    | 1 byte * 4   |                  |
+-------------------------+--------------+                  |
|  Archive Info           | 4 byte       |                  |
+-------------------------+--------------+                  |
|  EntryInfoList Offset   | 8 byte       |  Archive Header  |
+-------------------------+--------------+                  |
|  EntryInfoCount         | 4 byte       |                  |
+-------------------------+--------------+                  |
|  Reserved               | 4 byte       |                  |
+-------------------------+--------------+------------------+
|  Sequential Entry Data  | n byte * m   |  Data Container  |
+-------------------------+--------------+------------------+
|  EntryInfo List         | 32 byte * m  |  Info Container  |
+-------------------------+--------------+------------------+

各要素の説明は以下の通りになります。

* MagicNumber (Fixed)
　このファイルが、IceMilkTeaArchiveである事を示すファイル識別子です。
ファイルオープン時に、最初に読み込まれ、このマジックナンバーが一致しない場合
後続のデータが正しく格納されていても、ファイルとして正しくない物として扱われます。
識別コードは「'I' 'M' 'T' 'A' (0x49, 0x4D, 0x54, 0x41)」の順番で格納されるべきです。

データ型は、符号なし8bit整数の配列を使用します。


* Archive Info
　IceMilkTeaArchiveの基本情報を格納した領域になります。

データの型は、符号なし32bit整数を使用します。

また、32bitデータの内、各ビットの割当は次のようになっているべきです。
+------------+-----------+
|    Name    |    bit    |
+------------+-----------+
|  Version   |   0 - 7   |
+------------+-----------+
|  Reserved  |   8 - 31  |
+------------+-----------+

- Version
 - この領域には、IceMilkTeaArchive仕様に対するバージョンが書き込まれるべきです。
   実際の、文字列バージョンの値とは一致しておらず、新しい使用バージョンが確定した時に
   値が決定されます。

- Reserved
 - この領域は、予約されています。ゼロで初期化されていなければなりません。


* EntryInfoList Offset
　アーカイブに格納された実際のコンテンツデータを "エントリ" と表現し
そのエントリ自体の情報（いわゆるヘッダ情報）のリストが格納されている
ファイルの先頭から始まるオフセットを格納しなければなりません。

データ型は、符号付き64bit整数を使用します。


* EntryInfoCount
　アーカイブに格納された "エントリ情報" の数を格納します。
ただし "エントリの実体" がアーカイブに格納されている保証はしていません

　例えば、"EntryInfoCount"に'10'が格納されていても
後述する、"Sequential Entry Data"の領域に、'10'のエントリが
入っている保証はされていません、このデータを意味するのは、"EntryInfo List"の
領域に入っている "エントリ情報の数" であるべきです。

　実体が入っているかどうかを確認するには、"3. エントリ情報フォーマット（EntryInfo Format）"
を参照してください。

データ型は、符号付き32bit整数を使用します。


* Reserved
　現在の仕様では、予約領域として定義されています。
全てが、ゼロで初期化されていなければなりません。

データ型は、符号なし32bit整数を使用します。


* Sequential Entry Data
　アーカイブに格納された、実際のコンテンツのデータ、つまり "エントリの実体" が格納されます。
個々のエントリは連続した状態で、格納されなければなりませんが、エントリの実体の順番自体は
エントリ情報の順番と一致しなくてもよいです。

　エントリの実体の順番は、実装側が最適と思われる配置をすることも許されます。
また、"エントリの情報"が存在していたとしても、エントリの実体が存在しなくてもよいです。
エントリの実体が存在するかの保証は、"エントリの情報"がするべきです。

データ型は、符号なし8bit整数の配列がエントリの実体数分となります。
n : エントリの実体のサイズ（byte[n]）
m : エントリの実体の数（byte[m][n]）


* EntryInfo List
　アーカイブに格納された、"エントリの情報"のリストが格納されます。
"エントリの実体"を参照するには、このリストに含まれる"エントリの情報"を
使って参照することで可能になります。

　また、後述しますが、"エントリの情報"には、"エントリのID"が定義されており
このIDは、符号なし64bit整数として表現されます。
そして、このIDに基づいて、"エントリの情報"のリストは"昇順"で
ソートされた状態で、格納されていなければなりません。

データ型は、後述する「3. エントリ情報フォーマット（EntryInfo Format）」を参照してください。


3. エントリ情報フォーマット（EntryInfo Format）
------------------------------------------------------------
　エントリ情報フォーマットは、前述した "エントリの情報" そのもののフォーマットを定義しており
その様に格納されていなければなりません。

+-------------------------------+
|   (Entire) EntryInfo Format   |
+----------------+--------------+
|  Entry ID      | 8 byte       |
+----------------+--------------+
|  Entry Offset  | 8 byte       |
+----------------+--------------+
|  Entry Size    | 8 byte       |
+----------------+--------------+
|  Reserved      | 8 byte       |
+----------------+--------------+

各要素の説明は以下の通りになります。

* Entry ID
　アーカイブの実体と１：１になるようなユニークなIDになっています。そしてこのIDは非衝突保証をしなければなりません。
また、IDの生成ルールについては "CRC64(Ecma-182 Based)" を用いて
格納するファイル名（拡張子がある場合は含む）を、UTF-8エンコードしたバイト配列を計算する
実装に基づかなければなりません。

例：
Path="/mydrive/mygame/assets/mypicture.png"; // Archive target file.
Name=GetFileNameWithExt(Path); // "mypicture.png"
Data=Utf8EncodingWithoutBom(Name); // "mypicture.png" to byte array
EntryID=CRC64_Caluc(Name); // Result[0xBEE6EE99D28108A1]

CRC64の参考URL
[Ecma-182](https://www.ecma-international.org/publications/standards/Ecma-182.htm)

　さらに、前述していますが、このIDに基づき "エントリの情報のリスト" は昇順にソートされなければなりません。
そして、このIDは、ファイルの実体がアーカイブファイルに含まれていなくても、"確定しなければなりません"。

データ型は、符号なし64bit整数を使用します。


* Entry Offset
　アーカイブファイル内の"エントリの実体"が、実際に格納されている、ファイルの先頭からのオフセットが格納されます。
通常は、アーカイブファイル内に"エントリの実体"が入っている状態ですが、実装の都合、または、データ到着の遅延
など、様々な理由により、アーカイブファイルに実体が存在しない場合については、ゼロで初期化されているべきです。
もし、この値がゼロで初期化されている場合は、いかなる理由があっても、絶対に実体への参照するコードを実行してはなりません。
つまり、後述する "Entry Size" にいかなる値が入っていても、無視するべきです。

データ型は、符号つき64bit整数を使用します。


* Entry Size
　アーカイブファイル内の"エントリの実体"が、実際に格納されているサイズが格納されます。
通常は、アーカイブファイル内に"エントリの実体"が入っている状態ですが
前述した"Entry Offset"の項目の通りの理由などで"エントリの実体"が存在しない
場合に付いては、ゼロで初期化されている状態が好ましいです。

　しかし、実体が存在していなくても、予めサイズだけ格納することは許されます。
"エントリの実体"が存在しているかどうかの保証は、"Entry Offset"がするべきです。

　ただし、負の値は認めるべきではありません。

データ型は、符号つき64bit整数を使用します。


* Reserved
　この項目の内容に付いては、現在の仕様では予約として定義しています。
ゼロで初期化されているべきです。

データ型は、符号なし64bit整数を使用します。
**********************************************************************/
#endregion

using System;
using System.IO;
using System.Runtime.InteropServices;

namespace IceMilkTea.Module
{
    #region ヘッダ構造体
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
        /// <returns>ヘッダ情報の検証結果に対する結果を返します。問題がなければ ImtArchiveHeaderValidateResult.NoProblem を返します。</returns>
        public ImtArchiveHeaderValidateResult Validate()
        {
            // マジックナンバー配列がそもそもnullまたは、長さが4以外なら
            if (MagicNumber == null || MagicNumber.Length != 4)
            {
                // 未初期化も長さ一致しないのもダメ
                return ImtArchiveHeaderValidateResult.BrokenMagicNumber;
            }


            // マジックナンバーの各要素が既定値以外なら
            if (MagicNumber[0] != 0x49 || MagicNumber[1] != 0x4D || MagicNumber[2] != 0x54 || MagicNumber[3] != 0x41)
            {
                // マジックナンバーの状態がよろしくない
                return ImtArchiveHeaderValidateResult.BrokenMagicNumber;
            }


            // アーカイブ情報のバージョンが仕様バージョンと一致していないなら
            // （将来のバージョンで下位互換などがある場合は、判定方法を変える）
            if (ArchiveVersion != IceMilkTeaArchiveSpecVersion)
            {
                // バージョン不一致はダメ
                return ImtArchiveHeaderValidateResult.InvalidVersion;
            }


            // アーカイブ情報のバージョンビット以外(Reserved bit)に0初期化されていないのなら
            if ((ArchiveInfo & 0xFFFFFFFF00) != 0)
            {
                // アーカイブ情報が壊れている
                return ImtArchiveHeaderValidateResult.BrokenArchiveInfo;
            }


            // エントリ情報リストのオフセットがアーカイブヘッダサイズ未満なら
            if (EntryInfoListOffset < HeaderSize)
            {
                // エントリ情報リストオフセットは不正な値を持っている
                return ImtArchiveHeaderValidateResult.InvalidEntryInfoListOffset;
            }


            // エントリ情報の数が負の値なら
            if (EntryInfoCount < 0)
            {
                // エントリ情報の数が不正な値を持っている
                return ImtArchiveHeaderValidateResult.InvalidEntryInfoCount;
            }


            // 予約領域が0以外なら
            if (Reserved != 0)
            {
                // 予約領域が壊れている
                return ImtArchiveHeaderValidateResult.BrokenReserved;
            }


            // 上記全ての判定をパスしたのならヘッダに問題は無いとする
            return ImtArchiveHeaderValidateResult.NoProblem;
        }
    }
    #endregion



    #region エントリ情報構造体
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
        /// ヘッダの整合性を確認するだけであり、アーカイブファイルそのものの整合性を保証するものではありません。
        /// </remarks>
        /// <returns>ヘッダ情報の検証結果に対する結果を返します。問題がなければ ImtArchiveHeaderValidateResult.NoProblem を返します。</returns>
        public ImtArchiveEntryInfoValidateResult Validate()
        {
            // エントリIDが0なら
            if (Id == 0)
            {
                // エントリIDが壊れている
                return ImtArchiveEntryInfoValidateResult.BrokenEntryId;
            }


            // オフセットが0以外で、アーカイブファイルヘッダ未満の値なら
            if (Offset != 0 && Offset < ImtArchiveHeader.HeaderSize)
            {
                // 明らかな不正値である
                return ImtArchiveEntryInfoValidateResult.InvalidEntryOffset;
            }


            // サイズが負の値なら
            if (Size < 0)
            {
                // 明らかな不正値である
                return ImtArchiveEntryInfoValidateResult.InvalidEntrySize;
            }


            // 予約領域が0以外なら
            if (Reserved != 0)
            {
                // 予約領域が壊れている
                return ImtArchiveEntryInfoValidateResult.BrokenReserved;
            }


            // 上記の検証を全てパスしたのなら、ひとまず問題はない
            return ImtArchiveEntryInfoValidateResult.NoProblem;
        }
    }
    #endregion



    #region ストリーム
    /// <summary>
    /// アーカイブ内に含まれるエントリの実体を読み込む為のストリームクラスです
    /// </summary>
    /// <remarks>
    /// このストリームは、読み取り専用で書き込むことは出来ません、さらに長さの変更などが出来ないことにも注意してください。
    /// 書き込みや長さの変更を行おうとすると NotSupportedException がスローされます。
    /// </remarks>
    public class ImtArchiveReadStream : Stream
    {
        // 以下メンバ変数定義
        private ImtArchiveEntryInfo entryInfo;
        private Stream originalStream;
        private long virtualPosition;



        /// <summary>
        /// ストリームの読み込みが可能かどうか
        /// </summary>
        public override bool CanRead => originalStream.CanRead;


        /// <summary>
        /// ストリームのシークが可能かどうか
        /// </summary>
        public override bool CanSeek => originalStream.CanSeek;


        /// <summary>
        /// このストリームは書き込みが出来ません。
        /// 常に false を返します。
        /// </summary>
        public override bool CanWrite => false;


        /// <summary>
        /// ストリームの長さを取得します
        /// </summary>
        public override long Length => entryInfo.Size;


        /// <summary>
        /// ストリームの操作位置を設定取得をします
        /// </summary>
        public override long Position
        {
            get
            {
                // 仮想の位置を返す
                return virtualPosition;
            }
            set
            {
                // 負の値が来たら
                if (value < 0)
                {
                    // ストリームの負の位置ってどこですか
                    throw new ArgumentOutOfRangeException(nameof(Position));
                }


                // ストリーム本来の長さ以上に設定されようとしたら
                if (value >= entryInfo.Size)
                {
                    // ストリームの末尾を超えた設定は許されない
                    throw new EndOfStreamException();
                }


                // 位置は仮想の位置に設定する
                virtualPosition = value;
            }
        }



        /// <summary>
        /// ストリームのバッファをクリアします
        /// </summary>
        public override void Flush()
        {
            // ストリームをロック
            lock (originalStream)
            {
                // オリジナルのストリームをフラッシュする
                originalStream.Flush();
            }
        }


        /// <summary>
        /// ストリームから、指定バイト分読み込み、指定されたバッファにデータを書き込みます。
        /// </summary>
        /// <param name="buffer">読み取られたデータを書き込むバッファ</param>
        /// <param name="offset">バッファの書き込む位置のオフセット</param>
        /// <param name="count">バッファに書き込むバイト数</param>
        /// <returns>
        /// バッファに書き込んだバイト数を返しますが、指定されたバイト数未満を返すことがあります。
        /// また、ストリームの末尾に到達している場合は 0 を返すことがあります。
        /// </returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            // ストリームをロック
            int readSize = 0;
            lock (originalStream)
            {
                // ストリームからデータを読み込む
                readSize = originalStream.Read(buffer, offset, count);
            }


            // 読み込んだサイズを返す
            return readSize;
        }


        /// <summary>
        /// ストリームの位置を、指定された位置に設定します。
        /// </summary>
        /// <param name="offset">設定する位置</param>
        /// <param name="origin">指定された offset がどの位置からを示すかを表します</param>
        /// <returns>設定された新しいストリームの位置を返します</returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            // 仮想位置に設定をして結果を返す
            virtualPosition = offset;
            return virtualPosition;
        }


        /// <summary>
        /// このストリームでは、長さの設定はサポートをしていません。
        /// 常に NotSupportedException をスローします。
        /// </summary>
        /// <param name="value">長さの設定はサポートしていません</param>
        /// <exception cref="NotSupportedException">このストリームでは、長さの設定はサポートをしていません。</exception>
        public override void SetLength(long value)
        {
            // サポートはしていない
            throw new NotSupportedException("このストリームでは、長さの設定はサポートをしていません。");
        }


        /// <summary>
        /// このストリームでは、書き込みのサポートをしていません。
        /// 常に NotSupportedException をスローします。
        /// </summary>
        /// <param name="buffer">書き込みをサポートしていません</param>
        /// <param name="offset">書き込みをサポートしていません</param>
        /// <param name="count">書き込みをサポートしていません</param>
        /// <exception cref="NotSupportedException">このストリームでは、書き込みのサポートをしていません。</exception>
        public override void Write(byte[] buffer, int offset, int count)
        {
            // サポートはしていない
            throw new NotSupportedException("このストリームでは、書き込みのサポートをしていません。");
        }


        /// <summary>
        /// このインスタンスが保持しているエントリ情報のコピーを取得します
        /// </summary>
        /// <param name="result">エントリの情報を受け取りたい構造体への参照</param>
        public void GetEntryInfo(out ImtArchiveEntryInfo result)
        {
            // 全てコピー（そのまま代入でも良いけど）
            result.Id = entryInfo.Id;
            result.Offset = entryInfo.Offset;
            result.Size = entryInfo.Size;
            result.Reserved = entryInfo.Reserved;
        }
    }
    #endregion



    public class ImtArchive
    {
        private ImtArchiveEntryInfo[] entries;
        private object streamLockObject;



        public ImtArchive(Stream stream)
        {
        }
    }
}