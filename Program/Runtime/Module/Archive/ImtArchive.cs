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
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using IceMilkTea.Core;

namespace IceMilkTea.Module
{
    /// <summary>
    /// IceMilkTeaArchive のアーカイブを総合的に制御を行うクラスです。
    /// アーカイブからエントリを読み込むためのストリーム取得や、エントリの更新もこのクラスで行います。
    /// </summary>
    public class ImtArchive : IDisposable
    {
        #region 待機系
        /// <summary>
        /// アーカイブのエントリインストール状態を監視を行うクラスです
        /// </summary>
        public class ImtArchiveEntryInstallMonitor
        {
            /// <summary>
            /// 待機オブジェクトの継続情報を保持します
            /// </summary>
            private struct AwaiterContinuInfo
            {
                /// <summary>
                /// 待機オブジェクトの継続する同期コンテキスト
                /// </summary>
                public SynchronizationContext context;

                /// <summary>
                /// 待機オブジェクトの継続関数
                /// </summary>
                public Action callback;
            }



            /// <summary>
            /// インストールの全てが完了した時に呼び出されます
            /// </summary>
            public event Action InstallFinished;



            // クラス変数宣言
            private static SendOrPostCallback postContinuationFunctionCache = new SendOrPostCallback(_ => ((Action)_)());



            // メンバ変数定義
            private List<AwaiterContinuInfo> awaiterContinuInfoList;



            /// <summary>
            /// このモニタが監視しているアーカイブのインスタンスを取得します
            /// </summary>
            public ImtArchive archive { get; private set; }



            /// <summary>
            /// ImtArchiveEntryInstallMonitor のインスタンスを初期化します
            /// </summary>
            /// <param name="archive">監視する ImtArchive のインスタンス</param>
            /// <exception cref="ArgumentNullException">archive が null です</exception>
            public ImtArchiveEntryInstallMonitor(ImtArchive archive)
            {
                // 担当アーカイブがnullなら
                if (archive == null)
                {
                    // 何を監視すれば良いんじゃ
                    throw new ArgumentNullException(nameof(archive));
                }


                // 担当アーカイブを覚える
                this.archive = archive;
                awaiterContinuInfoList = new List<AwaiterContinuInfo>();
            }


            /// <summary>
            /// アーカイブのインストール待機オブジェクトを取得します
            /// </summary>
            /// <returns>インストール待機オブジェクトを返します</returns>
            public ImtArchiveEntryInstallAwaiter GetAwaiter()
            {
                // インストール待機オブジェクトを返す
                return new ImtArchiveEntryInstallAwaiter(this);
            }


            /// <summary>
            /// 待機オブジェクトからの継続関数の登録を行います
            /// </summary>
            /// <param name="continuation"></param>
            internal void RegisterContinuFunction(Action continuation)
            {
                // このタイミングの同期コンテキストを取得して継続関数情報リストに追加
                awaiterContinuInfoList.Add(new AwaiterContinuInfo()
                {
                    context = SynchronizationContext.Current,
                    callback = continuation,
                });
            }


            /// <summary>
            /// 全インストーラが終了した通知を行います
            /// </summary>
            internal void NotifyInstallAllFinish()
            {
                // 待機オブジェクトの同期コンテキスト呼び出しを行う
                for (int i = 0; i < awaiterContinuInfoList.Count; ++i)
                {
                    // 同期オブジェクトに継続関数をポストする
                    var continuInfo = awaiterContinuInfoList[i];
                    continuInfo.context.Post(postContinuationFunctionCache, continuInfo.callback);
                }


                // リストをクリア
                awaiterContinuInfoList.Clear();


                // イベントがあれば呼び出して全て解除する
                InstallFinished?.Invoke();
                InstallFinished = null;
            }
        }


        /// <summary>
        /// エントリインストールの待機構造体です。
        /// アーカイブのインストール状況を待機します。
        /// </summary>
        public struct ImtArchiveEntryInstallAwaiter : INotifyCompletion
        {
            // メンバ変数定義
            private ImtArchiveEntryInstallMonitor monitor;



            /// <summary>
            /// インストールが完了したかどうか
            /// </summary>
            public bool IsCompleted => !monitor.archive.installStarted;



            /// <summary>
            /// ImtArchiveEntryInstallAwaiter の初期化を行います
            /// </summary>
            /// <param name="monitor">この待機オブジェクトを生成したモニタ</param>
            public ImtArchiveEntryInstallAwaiter(ImtArchiveEntryInstallMonitor monitor)
            {
                // メンバ変数の初期化
                this.monitor = monitor;
            }


            /// <summary>
            /// タスクの完了処理を行います
            /// </summary>
            /// <param name="continuation">タスク完了後の継続処理を行う対象の関数</param>
            public void OnCompleted(Action continuation)
            {
                // 完了していれば
                if (IsCompleted)
                {
                    // 継続関数を直ちに呼んで終わり
                    continuation();
                    return;
                }


                // 継続関数の登録をして終わり
                monitor.RegisterContinuFunction(continuation);
            }


            /// <summary>
            /// タスクの結果を取得します
            /// </summary>
            public ImtArchiveEntryInstallResult GetResult()
            {
                // 結果を返す
                return monitor.archive.InstallFailed ? ImtArchiveEntryInstallResult.Failed : ImtArchiveEntryInstallResult.Success;
            }
        }
        #endregion



        /// <summary>
        /// 内部文字エンコーディング用バッファサイズ
        /// </summary>
        private const int EncodingBufferSize = (1 << 10);

        /// <summary>
        /// エントリ検索で見つからなかった時のインデックス値
        /// </summary>
        private const int EntryIndexNotFound = (-1);



        // メンバ変数定義
        private Crc64Base crc;
        private Encoding encoding;
        private ImtArchiveHeader header;
        private ImtArchiveEntryInfo[] entries;
        private ImtArchiveReader archiveReader;
        private ImtArchiveWriter archiveWriter;
        private Queue<ImtArchiveEntryInstaller> installerQueue;
        private ImtArchiveEntryInstallMonitor installMonitor;
        private long installOffset;
        private bool installStarted;
        private byte[] encodingBuffer;
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


        /// <summary>
        /// 前回のインストールが失敗したかどうかを取得します
        /// </summary>
        public bool InstallFailed { get; private set; }



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


            // 他メンバ変数の初期化を行う
            encodingBuffer = new byte[EncodingBufferSize];
            encoding = new UTF8Encoding(false);
            crc = new Crc64Ecma();
            installerQueue = new Queue<ImtArchiveEntryInstaller>();
            installMonitor = new ImtArchiveEntryInstallMonitor(this);
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
        /// エントリ名からエントリIDを計算します。
        /// </summary>
        /// <remarks>
        /// この関数は、実際にエントリ情報に格納されているIDを求めるために利用されます。
        /// </remarks>
        /// <param name="entryName">エントリIDを計算する、エントリ名</param>
        /// <returns>計算されたエントリIDを返します</returns>
        public ulong CalculateEntryId(string entryName)
        {
            // CRC計算をした結果を返す
            var encodeSize = encoding.GetBytes(entryName, 0, entryName.Length, encodingBuffer, 0);
            return crc.Calculate(encodingBuffer, 0, encodeSize);
        }


        /// <summary>
        /// 指定されたエントリIDの、エントリストリームを取得します。
        /// ストリームは、コンストラクタに渡された readStream をベースストリームとして使用します。
        /// </summary>
        /// <remarks>
        /// この関数が返す、エントリのストリームは共有権限などの制御は無いため
        /// ストリームが必要な、実装ごとにストリームの取得をしても問題はありません。
        /// </remarks>
        /// <param name="entryId">エントリストリームとして取得したい、エントリのID</param>
        /// <returns>生成されたエントリストリームを返します</returns>
        /// <exception cref="ArgumentException">エントリID '{entryId}' のエントリ情報が見つけられませんでした</exception>
        /// <exception cref="ArgumentException">エントリID '{entryId}' のエントリ情報はありますが、エントリの実体が存在しません</exception>
        public ImtArchiveEntryStream GetEntryStream(ulong entryId)
        {
            // 解放済みかどうかの処理を挟む
            IfDisposedThenException();


            // まずはエントリIDの該当インデックスを引っ張り込むが、見つけられなかった場合は
            var entryIndex = FindEntryIndex(entryId);
            if (entryIndex == EntryIndexNotFound)
            {
                // 見つけられなかったよー
                throw new ArgumentException($"エントリID '{entryId}' のエントリ情報が見つけられませんでした", nameof(entryId));
            }


            // エントリの実体が無いなら
            if (!entries[entryIndex].IsContainEntryData)
            {
                // 実体がないよー
                throw new ArgumentException($"エントリID '{entryId}' のエントリ情報はありますが、エントリの実体が存在しません", nameof(entryId));
            }


            // 見つけたのならストリームを生成して返す
            return new ImtArchiveEntryStream(entries[entryIndex], archiveReader.BaseStream);
        }


        /// <summary>
        /// 指定されたエントリIDの、エントリストリームを取得します。
        /// ストリームは、コンストラクタに渡された readStream をベースストリームとして使用します。
        /// </summary>
        /// <remarks>
        /// この関数が返す、エントリのストリームは共有権限などの制御は無いため
        /// ストリームが必要な、実装ごとにストリームの取得をしても問題はありません。
        /// </remarks>
        /// <param name="entryId">エントリストリームとして取得したい、エントリのID</param>
        /// <param name="stream">エントリの取得に成功した場合は、エントリストリームを設定しますが、見つけられなかった場合は 既定値(null) として初期化されます</param>
        /// <returns>指定されたエントリのストリームを返します</returns>
        public bool TryGetEntryStream(ulong entryId, out ImtArchiveEntryStream stream)
        {
            // 解放済みかどうかの処理を挟む
            IfDisposedThenException();


            // まずはエントリIDの該当インデックスを引っ張り込むが、見つけられなかった場合は
            var entryIndex = FindEntryIndex(entryId);
            if (entryIndex == EntryIndexNotFound)
            {
                // 取得できなかったことを返す
                stream = default(ImtArchiveEntryStream);
                return false;
            }


            // 該当のエントリの実体が無いなら
            if (!entries[entryIndex].IsContainEntryData)
            {
                // 取得できなかったことを返す
                stream = default(ImtArchiveEntryStream);
                return false;
            }


            // 見つけたのならストリームを生成して見つけたことを返す
            stream = new ImtArchiveEntryStream(entries[entryIndex], archiveReader.BaseStream);
            return true;
        }


        /// <summary>
        /// アーカイブに、指定されたエントリIDのエントリ情報及び、エントリの実体が含まれるかを調べます。
        /// </summary>
        /// <remarks>
        /// この関数は、エントリIDからエントリ情報を求めますが、エントリ情報が存在しても
        /// エントリの実体がない場合は、エントリが含まれていないとして判断します。
        /// </remarks>
        /// <param name="entryId">エントリが含まれているか確認したい、エントリID</param>
        /// <returns>エントリが含まれている場合は true を、含まれていない場合は false を返します</returns>
        public bool Contain(ulong entryId)
        {
            // 解放済みかどうかの処理を挟む
            IfDisposedThenException();


            // エントリIDからエントリインデックスを探してもらって NotFound なら
            var entryIndex = FindEntryIndex(entryId);
            if (entryIndex == EntryIndexNotFound)
            {
                // 見つけられなかった
                return false;
            }


            // エントリの実体が存在しないなら
            if (!entries[entryIndex].IsContainEntryData)
            {
                // エントリ情報があっても実体が無いなら存在しないとする
                return false;
            }


            // エントリの実体もあるなら問題はなし
            return true;
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


        #region エントリ情報書き込み関数群
        // TODO : インストールコードはインストール管理クラスに分けたほうが良いかもしれない
        /// <summary>
        /// 指定されたインストーラを、インストーラキューに追加します。
        /// </summary>
        /// <remarks>
        /// インストールするエントリの数が複数ある場合は、必要なインストーラの全てを追加することをおすすめします。
        /// 一つずつインストールを実行してしまうと、パフォーマンスに非常な悪影響を及ぼす可能性があります。
        /// </remarks>
        /// <param name="installer">キューに追加するインストーラ</param>
        /// <exception cref="InvalidOperationException">インストールが既に開始されています</exception>
        public void EnqueueInstaller(ImtArchiveEntryInstaller installer)
        {
            // インストールが始まっているのなら
            if (installStarted)
            {
                // インストール関数は呼び出せない
                throw new InvalidOperationException("インストールが既に開始されています");
            }


            // インストーラがnullなら
            if (installer == null)
            {
                // インストールを続行出来ない
                throw new ArgumentNullException(nameof(installer));
            }


            // この段階ではインストーラのプロパティ検査はせずそのままキューに突っ込む
            installerQueue.Enqueue(installer);
        }


        /// <summary>
        /// インストーラキューに待機している全てのインストーラを順次実行していきます。
        /// また、この関数は全てのインストーラが完了することは待たずに、処理を返すことがあります。
        /// </summary>
        /// <returns>インストール状況を監視するオブジェクトを返します</returns>
        /// <exception cref="InvalidOperationException">インストールが既に開始されています</exception>
        /// <exception cref="InvalidOperationException">インストーラが１つもキューに追加されていません</exception>
        /// <exception cref="InvalidOperationException">インストーラが無効なエントリ名を返しました</exception>
        /// <exception cref="InvalidOperationException">インストーラが無効なエントリサイズを返しました</exception>
        public ImtArchiveEntryInstallMonitor InstallEntryAsync()
        {
            // インストールが始まっているのなら
            if (installStarted)
            {
                // インストール関数は呼び出せない
                throw new InvalidOperationException("インストールが既に開始されています");
            }


            // インストーラキューが空なら
            if (installerQueue.Count == 0)
            {
                // 何をインストールすれば良いんじゃ
                throw new InvalidOperationException("インストーラが１つもキューに追加されていません");
            }


            // インストール前の事前情報変数を用意
            var newerEntryInfoList = new List<ImtArchiveEntryInfo>(installerQueue.Count);
            var necessaryFreeSpace = 0L;


            // キューの数分回って検査しつつ新規追加分の数も数える
            foreach (var installer in installerQueue)
            {
                // もしインストーラが無効なエントリ名を差し出してきたら
                if (string.IsNullOrWhiteSpace(installer.EntryName))
                {
                    // そのエントリ名は受け付けられない
                    throw new InvalidOperationException("インストーラが無効なエントリ名を返しました");
                }


                // インストーラが不正なサイズを要求してきたら
                if (installer.EntrySize < 0)
                {
                    // 負のサイズのデータってなんだろうか
                    throw new InvalidOperationException("インストーラが無効なエントリサイズを返しました");
                }


                // エントリ名からエントリIDを作って、必要空きスペースサイズに要求サイズを加算
                var entryId = CalculateEntryId(installer.EntryName);
                necessaryFreeSpace += installer.EntrySize;


                // エントリIDで検索してインデックスが見つからなかったら
                if (FindEntryIndex(entryId) == EntryIndexNotFound)
                {
                    // 新規追加分のエントリとして新規追加エントリリストに追加
                    newerEntryInfoList.Add(new ImtArchiveEntryInfo()
                    {
                        // 各エントリ情報を設定
                        Id = entryId,
                        Offset = 0L,
                        Size = installer.EntrySize,
                        Reserved = 0UL,
                    });
                }
            }


            // インストールを開始したことと、インストールするべきオフセット、インストール失敗のクリアを設定
            installStarted = true;
            InstallFailed = false;
            installOffset = header.EntryInfoListOffset;


            // 新規追加エントリをマージして、直ちに参照を更新する（要素の更新自体は、実際に該当インストールが完了してからになる）
            // TODO : 出来ることならインストールクラスに分けてエントリ情報の更新タイミングとかも適切にハンドリングしたい
            entries = MergeEntryInfo(newerEntryInfoList.ToArray());


            // 書き込みストリームをロック
            lock (archiveWriter.BaseStream)
            {
                // インストールするためにアーカイブの領域を広げるため新しいエントリサイズ分を追加して広げる
                var newArchiveSize = archiveWriter.BaseStream.Length + necessaryFreeSpace + ImtArchiveEntryInfo.InfoSize * entries.Length;
                archiveWriter.BaseStream.SetLength(newArchiveSize);


                // 新しいエントリ情報リストのオフセットに移動して、新しいエントリを一気に書き込む
                archiveWriter.BaseStream.Seek(writeStreamHeadPosition + header.EntryInfoListOffset + necessaryFreeSpace, SeekOrigin.Begin);
                for (int i = 0; i < entries.Length; ++i)
                {
                    // エントリ情報を書き込む
                    archiveWriter.Write(ref entries[i]);
                }


                // 先頭に戻ってエントリオフセット＆カウント更新版のヘッダを書き込む
                header.EntryInfoCount = entries.Length;
                header.EntryInfoListOffset = header.EntryInfoListOffset + necessaryFreeSpace;
                archiveWriter.BaseStream.Seek(writeStreamHeadPosition, SeekOrigin.Begin);
                archiveWriter.Write(ref header);
            }


            // まずはキューの先頭にいるインストーラを取得してインストール開始をポストしてモニタを返す
            SynchronizationContext.Current.Post(x => DoInstall((ImtArchiveEntryInstaller)x), installerQueue.Peek());
            return installMonitor;
        }


        /// <summary>
        /// 指定されたインストーラによるインストールを行います。
        /// </summary>
        /// <param name="installer">インストールを行うインストーラ</param>
        private void DoInstall(ImtArchiveEntryInstaller installer)
        {
            // エントリ情報を取得してインストールオフセットを渡す（ここはコピーなので実際のエントリ情報の更新ではない）
            // TODO : 都度エントリIDを計算するのはもったいないのでどうにかする（そもそもエントリ情報のインデックスも取得済みで良い気もする）
            var entryInfo = entries[FindEntryIndex(CalculateEntryId(installer.EntryName))];
            entryInfo.Offset = writeStreamHeadPosition + installOffset;


            // インストール用ストリームの生成をしてインストーラに渡す
            var installStream = new ImtArchiveEntryInstallStream(entryInfo, archiveWriter.BaseStream, installer, OnEntryInstallFinished);
            installer.DoInstall(installStream);
        }


        /// <summary>
        /// インストーラによるインストールが終わった時の処理を行います
        /// </summary>
        /// <param name="installer">インストール完了通知をしたインストーラ</param>
        /// <param name="result">インストール結果</param>
        private void OnEntryInstallFinished(ImtArchiveEntryInstaller installer, ImtArchiveEntryInstallResult result)
        {
            // キューからでもインストーラを取り出せるが、ここは素直に捨てる
            installerQueue.Dequeue();


            // もしインストール失敗したのなら
            if (result == ImtArchiveEntryInstallResult.Failed)
            {
                // 失敗したマークをつける
                InstallFailed = true;
            }


            // エントリのインデックスを取り出す
            // TODO : 都度エントリIDを計算するのはもったいないのでどうにかする（そもそもエントリ情報のインデックスも取得済みで良い気もする）
            var entryIndex = FindEntryIndex(CalculateEntryId(installer.EntryName));


            // エントリの更新とインストールオフセットを更新
            entries[entryIndex].Offset = installOffset;
            installOffset += installer.EntrySize;


            // エントリのインデックスからアーカイブの更新するべきオフセットを求める
            var updateEntryInfoOffset = writeStreamHeadPosition + header.EntryInfoListOffset + ImtArchiveEntryInfo.InfoSize * entryIndex;


            // 書き込みストリームをロック
            lock (archiveWriter.BaseStream)
            {
                // シークして新しいエントリ情報を書き込む
                archiveWriter.BaseStream.Seek(updateEntryInfoOffset, SeekOrigin.Begin);
                archiveWriter.Write(ref entries[entryIndex]);
            }


            // キューが空ではないなら
            if (installerQueue.Count > 0)
            {
                // インストールを続ける
                SynchronizationContext.Current.Post(x => DoInstall((ImtArchiveEntryInstaller)x), installerQueue.Peek());
                return;
            }


            // キューが空になったのなら真の意味でインストール完了なので、インストール完了状態にしてモニタにインストール完了通知をする
            installStarted = false;
            installMonitor.NotifyInstallAllFinish();
        }
        #endregion


        #region 共通処理
        /// <summary>
        /// エントリ情報の配列から、指定されたエントリIDを含むインデックスを検索します。
        /// </summary>
        /// <param name="entryId">検索するエントリID</param>
        /// <returns>該当のエントリIDを見つけた場合は、そのインデックスを返しますが、見つけられなかった場合は EntryIndexNotFound を返します</returns>
        private int FindEntryIndex(ulong entryId)
        {
            // そもそもエントリの長さが0なら
            if (entries.Length == 0)
            {
                // 見つかるわけがない
                return EntryIndexNotFound;
            }


            // 初回の検索範囲を初期化する
            var head = 0;
            var tail = entries.Length;


            // 見つかるまでループ
            while (head <= tail)
            {
                // 中心位置を求める
                var pivot = (head + tail) / 2;


                // 現在の中心位置に該当のIDを見つけたのなら
                if (entries[pivot].Id == entryId)
                {
                    // 見つけたこの位置を知ってほしい
                    return pivot;
                }
                else if (entries[pivot].Id < entryId)
                {
                    // 現在の中心位置より、要求IDが大きいのなら頭を後ろにずらしてトライ
                    head = pivot + 1;
                }
                else if (entries[pivot].Id > entryId)
                {
                    // 現在の中心位置より、要求IDが小さいならお尻を前にずらしてトライ
                    tail = pivot - 1;
                }
            }


            // ループを抜けてしまったのなら見つけられなかった
            return EntryIndexNotFound;
        }


        /// <summary>
        /// 指定さたら新しいエントリ情報を、現在のエントリ情報配列にマージをします
        /// </summary>
        /// <remarks>
        /// 重複するエントリIDが新しいエントリ情報配列に含まれていた場合の動作は保証していません
        /// </remarks>
        /// <param name="infos">これからマージする新しいエントリ情報の配列（エントリ情報は内部でソートするため、ソート済みである必要はありません）</param>
        /// <returns>マージされた新しいエントリ情報の配列を返します</returns>
        private ImtArchiveEntryInfo[] MergeEntryInfo(ImtArchiveEntryInfo[] infos)
        {
            // 渡された配列の長さが１より大きいなら
            if (infos.Length > 1)
            {
                // IDでソートする
                Array.Sort(infos, (x, y) => x.Id < y.Id ? -1 : x.Id == y.Id ? 0 : 1);
            }


            // 全体を受け取る新しいエントリ配列を用意
            var newEntries = new ImtArchiveEntryInfo[entries.Length + infos.Length];


            // 古い配列と新しい配列をマージする間はループ
            var oldArrayIndex = 0;
            var newArrayIndex = 0;
            while (oldArrayIndex < entries.Length && newArrayIndex < newEntries.Length)
            {
                // もし古いエントリIDの方が小さいなら
                if (entries[oldArrayIndex].Id < infos[newArrayIndex].Id)
                {
                    // 古い方のエントリを入れて古いインデックスを進める
                    newEntries[oldArrayIndex + newArrayIndex] = entries[oldArrayIndex];
                    ++oldArrayIndex;
                }
                else
                {
                    // 新しい方のエントリが小さいなら新しいエントリを入れて新しいインデックスを進める
                    newEntries[oldArrayIndex + newArrayIndex] = infos[newArrayIndex];
                    ++newArrayIndex;
                }
            }


            // 既に古いインデックスが末尾まで達しているのなら
            if (oldArrayIndex == entries.Length)
            {
                // 新しい配列から持ってくるだけの作業
                for (int i = newArrayIndex; i < infos.Length; ++i)
                {
                    // 残りの要素をひたすらコピー
                    newEntries[oldArrayIndex + i] = infos[i];
                }
            }


            // 既に新しいインデックスが末尾まで達しているのなら
            if (newArrayIndex == infos.Length)
            {
                // 古い配列から持ってくるだけの作業
                for (int i = oldArrayIndex; i < entries.Length; ++i)
                {
                    // 残りの要素をひたすらコピー
                    newEntries[newArrayIndex + i] = entries[i];
                }
            }


            // 完成した新しいエントリ情報の配列を変えす
            return newEntries;
        }


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