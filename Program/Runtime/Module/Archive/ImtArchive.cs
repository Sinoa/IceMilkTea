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
エントリの実体そのものは連続した状態で格納されなければなりませんが、エントリの実体の順番や
実際の格納サイズ自体はエントリ情報の内容と順番に一致しなくてもよいです。

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

namespace IceMilkTea.Module
{
    /// <summary>
    /// IceMilkTeaArchive のアーカイブを総合的に制御を行うクラスです。
    /// アーカイブからエントリを読み込むためのストリーム取得や、エントリの更新もこのクラスで行います。
    /// </summary>
    public class ImtArchive : IDisposable
    {
        // メンバ変数定義
        private ImtArchiveHeader header;
        private ImtArchiveEntryInfo[] entries;
        private ImtArchiveReader archiveReader;
        private ImtArchiveWriter archiveWriter;
        private long readStreamHeadPosition;
        private long writeStreamHeadPosition;
        private bool disposed;
        private bool leaveOpen;



        /// <summary>
        /// 現在、保持しているエントリ情報の数
        /// </summary>
        public int EntryInfoCount
        {
            get { return GetEntryInfoCount(); }
        }



        #region コンストラクタ＆デストラクタ＆Dispose
        /// <summary>
        /// ImtArchive のインスタンスをファイルパスを基に初期化します。
        /// </summary>
        /// <remarks>
        /// このコンストラクタは内部で、読み込み用と書き込み用のストリームを、ファイル共有が非常に緩い状態でオープンします。
        /// つまり、他のプロセスなどからも該当のファイルを、ReadやWriteでオープンすることは可能です。
        /// また、コンストラクタでアーカイブをオープンしても、データの読み込みをしないのでエントリ情報は持っていません。
        /// エントリ情報の取得をする前に必ず FetchManageData() 関数を呼び出してください
        /// </remarks>
        /// <param name="path">アーカイブをファイルとして扱うパス</param>
        public ImtArchive(string path)
        {
            // 指定されたパスのファイルのFileStreamを、緩いファイル共有で開く
            var stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite, 4 << 10, FileOptions.RandomAccess);
            Initialize(stream, stream, false);
        }


        /// <summary>
        /// ImtArchive のインスタンスをストリームから初期化します。
        /// </summary>
        /// <remarks>
        /// 引数で受け取る readStream および writeStream は、同一である必要ではありません。
        /// また、コンストラクタでアーカイブをオープンしても、データの読み込みをしないのでエントリ情報は持っていません。
        /// エントリ情報の取得をする前に必ず FetchManageData() 関数を呼び出してください
        /// </remarks>
        /// <param name="readStream">アーカイブデータを読み取るストリーム。ストリームは CanRead および CanSeek をサポートしなければなりません</param>
        /// <param name="writeStream">アーカイブデータに書き込むストリーム。ストリームは CanWrite および CanSeek をサポートしなければなりません</param>
        /// <param name="leaveOpen">ImtArchive オブジェクトを破棄した後に readStream および writeStream を開いたままにする場合は true を、それ以外の場合は false を指定</param>
        /// <exception cref="ArgumentNullException">readStream または writeStream が null です</exception>
        /// <exception cref="NotSupportedException">readStream が CanRead または CanSeek をサポートしていません</exception>
        /// <exception cref="NotSupportedException">writeStream が CanWrite または CanSeek をサポートしていません</exception>
        public ImtArchive(Stream readStream, Stream writeStream, bool leaveOpen)
        {
            // 初期化関数を呼ぶ
            Initialize(readStream, writeStream, leaveOpen);
        }


        /// <summary>
        /// ImtArchive クラスの共通コンストラクタです
        /// </summary>
        /// <param name="readStream">アーカイブデータを読み取るストリーム。ストリームは CanRead および CanSeek をサポートしなければなりません</param>
        /// <param name="writeStream">アーカイブデータに書き込むストリーム。ストリームは CanWrite および CanSeek をサポートしなければなりません</param>
        /// <param name="leaveOpen">ImtArchive オブジェクトを破棄した後に readStream および writeStream を開いたままにする場合は true を、それ以外の場合は false を指定</param>
        /// <exception cref="ArgumentNullException">readStream または writeStream が null です</exception>
        /// <exception cref="NotSupportedException">readStream が CanRead または CanSeek をサポートしていません</exception>
        /// <exception cref="NotSupportedException">writeStream が CanWrite または CanSeek をサポートしていません</exception>
        private void Initialize(Stream readStream, Stream writeStream, bool leaveOpen)
        {
            // readStream または writeStream が null なら
            if (readStream == null || writeStream == null)
            {
                // どちらも参照は渡さないとダメ
                throw new ArgumentNullException($"'{nameof(readStream)}' または '{nameof(writeStream)}' が null です");
            }


            // readStream が CanRead または CanSeek をサポートしていないなら
            if (!(readStream.CanRead && readStream.CanSeek))
            {
                // 読み込みストリームなのに読み込みが出来ない
                throw new NotSupportedException($"'{nameof(readStream)}'が CanRead または CanSeek をサポートしていません");
            }


            // writeStream が CanWrite または CanSeek をサポートしていないなら
            if (!(writeStream.CanWrite && writeStream.CanSeek))
            {
                // 書き込みストリームなのに書き込みが出来ない
                throw new NotSupportedException($"'{nameof(readStream)}'が CanWrite または CanSeek をサポートしていません");
            }


            // 読み込みと書き込みのストリームを覚える
            archiveReader = new ImtArchiveReader(readStream);
            archiveWriter = new ImtArchiveWriter(writeStream);


            // ストリームの渡された時点を先頭位置として覚える
            readStreamHeadPosition = readStream.Position;
            writeStreamHeadPosition = writeStream.Position;


            // 一度、ヘッダとエントリ情報は空情報として初期化しておく
            ImtArchiveHeader.CreateArchiveHeader(out header);
            entries = new ImtArchiveEntryInfo[0];


            // 破棄後のオープン状態をどうするかを覚える
            this.leaveOpen = leaveOpen;
        }


        /// <summary>
        /// ImtArchive のインスタンスを破棄します
        /// </summary>
        ~ImtArchive()
        {
            // デストラクタからのDispose呼び出し
            Dispose(false);
        }


        /// <summary>
        /// ImtArchive のリソースを破棄します
        /// </summary>
        public void Dispose()
        {
            // DisposeからのDispose呼び出しをしてデストラクタを呼ばないようにしてもらう
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        /// <summary>
        /// 実際のリソース破棄処理を行います。
        /// </summary>
        /// <param name="disposing">Disposeからの呼び出しの場合は true を、それ以外の場合は false を指定</param>
        protected virtual void Dispose(bool disposing)
        {
            // 既に破棄済みなら
            if (disposed)
            {
                // 何もせず終了
                return;
            }


            // Disposeからの呼び出しなら
            if (disposing)
            {
                // 破棄処理でストリームを開きっぱ指定にはなってないのなら
                if (!leaveOpen)
                {
                    // もし読み込みと書き込みが同じストリームなら
                    if (archiveReader.BaseStream == archiveWriter.BaseStream)
                    {
                        // 片方だけストリームを閉じる
                        archiveWriter.BaseStream.Dispose();
                    }
                    else
                    {
                        // 両方閉じる
                        archiveReader.BaseStream.Dispose();
                        archiveWriter.BaseStream.Dispose();
                    }
                }
            }


            // 解放済みであることを示す
            disposed = true;
        }
        #endregion


        #region 情報取得系関数群
        /// <summary>
        /// 読み込みストリームの長さから、アーカイブの管理情報を取り出せるか判断します。
        /// </summary>
        /// <remarks>
        /// この関数は、読み込みストリームの長さが、アーカイブヘッダの長さに届くかどうかの判定を行うため
        /// 実際のデータが正しく読み込める保証はしていません。
        /// </remarks>
        /// <returns>アーカイブヘッダを読み込むのに十分な長さがある場合は true を、長さが足りない場合は false を返します</returns>
        /// <exception cref="InvalidOperationException">このアーカイブは既に解放済みです</exception>
        public bool CanFetchManageData()
        {
            // 解放済みかどうかの処理を挟む
            IfDisposedThenException();


            // 読み込みストリームの長さから、読み取り位置を差し引いて、アーカイブヘッダのサイズに届くかどうかの結果を返す
            return (archiveReader.BaseStream.Length - readStreamHeadPosition) >= ImtArchiveHeader.HeaderSize;
        }


        /// <summary>
        /// 読み込みストリームからアーカイブの管理情報を取り出し、
        /// このインスタンス上にアーカイブ情報やエントリ情報を構築します。
        /// </summary>
        /// <remarks>
        /// この関数で、管理情報を取り出した際に、管理データに損傷が無いかの検証も行われます。
        /// よって、この関数が無事に終了した場合は、管理情報に問題はないと判断しても構いません。
        /// また、この関数は読み込みストリームをロックするため、既にエントリストリームが動作している場合は
        /// エントリストリームのReadにパフォーマンス影響が発生することに気をつけてください。
        /// </remarks>
        /// <exception cref="InvalidOperationException">このアーカイブは既に解放済みです</exception>
        /// <exception cref="InvalidOperationException">アーカイブヘッダに {'errorContent'} の問題が発生しました</exception>
        /// <exception cref="InvalidOperationException">エントリ情報が、ID昇順で格納されていません</exception>
        /// <exception cref="InvalidOperationException">エントリ情報に '{errorContent}' の問題が発生しました</exception>
        public void FetchManageData()
        {
            // 解放済みかどうかの処理を挟む
            IfDisposedThenException();


            // 一時的にヘッダを覚えておくための変数を宣言
            var tempHeader = default(ImtArchiveHeader);


            // 読み込みストリームのロック
            lock (archiveReader.BaseStream)
            {
                // まずは、読み込みストリームを先頭（コンストラクタのタイミングで取った位置）に移動してヘッダを読み込む
                archiveReader.BaseStream.Seek(readStreamHeadPosition, SeekOrigin.Begin);
                archiveReader.Read(out tempHeader);
            }


            // ヘッダの整合性をチェックして問題が発生したのなら
            var headerValidateResult = ImtArchiveHeader.Validate(ref tempHeader);
            if (headerValidateResult != ImtArchiveHeaderValidateResult.NoProblem)
            {
                // ヘッダの検証に問題が発生したことを吐く
                throw new InvalidOperationException($"アーカイブヘッダに '{headerValidateResult.ToString()}' の問題が発生しました");
            }


            // エントリ情報も新しく読み取るため新しいエントリ情報配列を生成
            // TODO : I/O改善のために、エントリ情報を読み込むためのバッファを一気にロードするためのバッファを用意とかしたいなぁ
            var tempEntries = new ImtArchiveEntryInfo[tempHeader.EntryInfoCount];


            // 読み込みストリームのロック
            lock (archiveReader.BaseStream)
            {
                // エントリ情報のリストのオフセットに移動して、必要個数分読み込む
                // TODO : 上記にも書いてあるとおりできれは、ここで構築作業をせず lock スコープ外で byte[] からエントリ情報を構築出来るようにするといいかも
                archiveReader.BaseStream.Seek(readStreamHeadPosition + tempHeader.EntryInfoListOffset, SeekOrigin.Begin);
                for (int i = 0; i < tempEntries.Length; ++i)
                {
                    // ひたすら詰める
                    archiveReader.Read(out tempEntries[i]);
                }
            }


            // 読み込まれたエントリ情報の全てをちゃんとした情報か整合性のチェックを行う
            var previousEntryId = 0UL;
            for (int i = 0; i < tempEntries.Length; ++i)
            {
                // 一つ前のエントリIDより小さいのなら
                if (previousEntryId > tempEntries[i].Id)
                {
                    // ID昇順にデータを格納する仕様違反になるので例外を吐く
                    throw new InvalidOperationException("エントリ情報が、ID昇順で格納されていません");
                }


                // エントリ情報の整合性をチェックして問題が発生したのなら
                var entryInfoValidateResult = ImtArchiveEntryInfo.Validate(ref tempEntries[i]);
                if (entryInfoValidateResult != ImtArchiveEntryInfoValidateResult.NoProblem)
                {
                    // エントリ情報の検証に問題が発生したことを吐く
                    throw new InvalidOperationException($"エントリ情報に '{entryInfoValidateResult.ToString()}' の問題が発生しました");
                }


                // 一つ前エントリIDの更新
                previousEntryId = tempEntries[i].Id;
            }


            // 上記すべての検証を終えたら、正しい情報として判断し記憶する
            header = tempHeader;
            entries = tempEntries;
        }


        /// <summary>
        /// 現在保持しているエントリの数を取得します
        /// </summary>
        /// <returns>現在保持しているエントリの数を返します</returns>
        /// <exception cref="InvalidOperationException">このアーカイブは既に解放済みです</exception>
        public int GetEntryInfoCount()
        {
            // 解放済みかどうかの処理を挟む
            IfDisposedThenException();


            // エントリの数を返す
            return entries.Length;
        }
        #endregion


        #region 共通処理
        /// <summary>
        /// 既に解放済みの場合は、例外を送出します。
        /// </summary>
        /// <exception cref="InvalidOperationException">このアーカイブは既に解放済みです</exception>
        private void IfDisposedThenException()
        {
            // もし既に解放済みなら
            if (disposed)
            {
                // このアーカイブクラスは解放済みなので何も出来ないということを吐く
                throw new InvalidOperationException("このアーカイブは既に解放済みです");
            }
        }
        #endregion
    }
}