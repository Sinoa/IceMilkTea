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
using System.IO;

namespace IceMilkTea.Module
{
    /// <summary>
    /// アーカイブにエントリを追記するためのインストーラの抽象クラスです。
    /// IceMilkTeaArchive にてエントリの追記をインストールと呼びます。
    /// </summary>
    /// <remarks>
    /// アーカイブにエントリを新たに追記する場合はこのクラスを継承し、アーカイブにデータを書き込むためのフローを制御して下さい。
    /// </remarks>
    public abstract class ImtArchiveEntryInstaller
    {
        /// <summary>
        /// これからインストールするエントリの名前
        /// </summary>
        public abstract string EntryName { get; }


        /// <summary>
        /// これからインストールするエントリのサイズ
        /// </summary>
        public abstract long EntrySize { get; }



        /// <summary>
        /// インストールを開始します。
        /// </summary>
        /// <remarks>
        /// この関数でインストールを完了する必要はありません。
        /// 引数で渡された installStream の ImtArchiveEntryInstallStream.FinishInstall() 関数を呼び出すまでは、
        /// アーカイバはインストール処理が継続しているものとして動作します。
        /// </remarks>
        /// <param name="installStream">アーカイバにエントリデータを書き込むためのインストールストリーム</param>
        public abstract void DoInstall(ImtArchiveEntryInstallStream installStream);
    }



    /// <summary>
    /// ファイルからアーカイブに、エントリをインストールするクラスです。
    /// </summary>
    public class ImtArchiveEntryFileInstaller : ImtArchiveEntryInstaller
    {
        // 以下メンバ変数定義
        private string filePath;
        private string entryName;
        private long entrySize;



        /// <summary>
        /// エントリ名を取得します
        /// </summary>
        public override string EntryName => entryName;


        /// <summary>
        /// エントリのサイズを取得します
        /// </summary>
        public override long EntrySize => entrySize;



        /// <summary>
        /// ImtArchiveEntryFileInstaller のインスタンスを初期化します
        /// </summary>
        /// <param name="filePath">アーカイブにインストールするファイルへのパス</param>
        /// <exception cref="ArgumentException">filePath が null または、扱えない文字列が指定されました</exception>
        /// <exception cref="FileNotFoundException">インストールする該当のファイルが見つかりませんでした</exception>
        public ImtArchiveEntryFileInstaller(string filePath)
        {
            // そもそも無効な文字列が渡されていたら
            if (string.IsNullOrWhiteSpace(filePath))
            {
                // パスとしては使えない文字列は受け入れられない
                throw new ArgumentException($"{nameof(filePath)} が null または、扱えない文字列が指定されました");
            }


            // 指定されたファイルが見つからなかったら
            if (!File.Exists(filePath))
            {
                // ファイルが見つからなかった例外を吐く
                throw new FileNotFoundException("インストールする該当のファイルが見つかりませんでした", filePath);
            }


            // それぞれの変数を初期化する
            this.filePath = filePath;
            entryName = Path.GetFileName(filePath);
            entrySize = new FileInfo(filePath).Length;
        }


        /// <summary>
        /// インストールを行います
        /// </summary>
        /// <param name="installStream">インストールするためのストリーム</param>
        public override void DoInstall(ImtArchiveEntryInstallStream installStream)
        {
            // この段階でファイルが見つからなかったら
            if (!File.Exists(filePath))
            {
                // インストールに失敗として通知して終了
                installStream.FinishInstall(ImtArchiveEntryInstallResult.Failed);
                return;
            }


            // インストールするファイルを開く
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                // ファイルの読み込みバッファを用意
                var buffer = new byte[1 << 10];


                // 無便ループ
                while (true)
                {
                    // ファイルを読み込んでもし末尾に到達したのなら
                    var readSize = fileStream.Read(buffer, 0, buffer.Length);
                    if (readSize == 0)
                    {
                        // もう終わり
                        break;
                    }


                    // インストールストリームに読み込んだデータを書き込む
                    installStream.Write(buffer, 0, readSize);
                }


                // ループから抜けてきたのならインストールは完了ということを通知する
                installStream.FinishInstall(ImtArchiveEntryInstallResult.Success);
            }
        }
    }
}