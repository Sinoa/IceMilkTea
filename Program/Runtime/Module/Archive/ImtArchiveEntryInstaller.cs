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
}