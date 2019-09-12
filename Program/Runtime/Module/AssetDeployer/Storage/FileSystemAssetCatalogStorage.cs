// zlib/libpng License
//
// Copyright (c) 2019 Sinoa
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
using System.Collections.Generic;
using System.IO;

namespace IceMilkTea.SubSystem
{
    /// <summary>
    /// ファイルシステムを利用したアセットカタログを貯蔵するストレージクラスです
    /// </summary>
    public class FileSystemAssetCatalogStorage : IAssetCatalogStorage
    {
        // メンバ変数定義
        private DirectoryInfo baseDirectoryInfo;
        private DirectoryInfo tempDirectoryInfo;



        /// <summary>
        /// このストレージが管理するベースディレクトリが存在するか否か
        /// </summary>
        public bool ExistsBaseDirectory { get { baseDirectoryInfo.Refresh(); return baseDirectoryInfo.Exists; } }


        /// <summary>
        /// このストレージが管理する一時ディレクトリが存在するか否か
        /// </summary>
        public bool ExistsTempDirectory { get { tempDirectoryInfo.Refresh(); return tempDirectoryInfo.Exists; } }



        /// <summary>
        /// FileSystemAssetCatalogStorage クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="baseDirectoryInfo">このストレージが管理するベースディレクトリ情報</param>
        /// <param name="tempDirectoryInfo">このストレージが管理する一時カタログ格納用ディレクトリ情報</param>
        /// <exception cref="ArgumentNullException">baseDirectoryInfo が null です</exception>
        /// <exception cref="ArgumentNullException">tempDirectoryInfo が null です</exception>
        public FileSystemAssetCatalogStorage(DirectoryInfo baseDirectoryInfo, DirectoryInfo tempDirectoryInfo)
        {
            // ベースディレクトリ情報を受け取る
            this.baseDirectoryInfo = baseDirectoryInfo ?? throw new ArgumentNullException(nameof(baseDirectoryInfo));
            this.tempDirectoryInfo = tempDirectoryInfo ?? throw new ArgumentNullException(nameof(tempDirectoryInfo));
        }


        /// <summary>
        /// このストレージが管理している全てのカタログ名を取得します
        /// </summary>
        /// <returns>取得されたカタログ名を列挙できる IEnumerable のインスタンスを返します</returns>
        public IEnumerable<string> GetCatalogs()
        {
            // そもそもベースディレクトリが無いなら
            if (!ExistsBaseDirectory)
            {
                // 長さ0の配列として返す
                return Array.Empty<string>();
            }


            // ディレクトリトップのみのファイルを列挙する
            var fileInfos = baseDirectoryInfo.GetFiles("*", SearchOption.TopDirectoryOnly);
            var nameList = new List<string>(fileInfos.Length);
            foreach (var fileInfo in fileInfos)
            {
                // もしディレクトリ属性が付いていたら
                if (fileInfo.Attributes.HasFlag(FileAttributes.Directory))
                {
                    // ファイルのみ列挙したいので次へ
                    continue;
                }


                // ファイル名だけ詰めていく
                nameList.Add(fileInfo.Name);
            }


            // リストアップした名前のリストを返す
            return nameList;
        }


        /// <summary>
        /// 指定した名前のカタログがあるか否かを調べます
        /// </summary>
        /// <param name="name">存在を確認するカタログ名</param>
        /// <returns>指定された名前のカタログがある場合は true を、ない場合は false を返します</returns>
        public bool Exists(string name)
        {
            // そもそもベースディレクトリが無いなら
            if (!ExistsBaseDirectory)
            {
                // そもそも格納先が存在しない
                return false;
            }


            // ファイルが存在するかの結果をそのまま返す
            return File.Exists(Path.Combine(baseDirectoryInfo.FullName, name));
        }


        /// <summary>
        /// 指定された名前のカタログを読み込むためのストリームを開きます
        /// </summary>
        /// <param name="name">読み込むカタログ名</param>
        /// <returns>指定されたカタログ名のストリームを開けた場合はインスタンスを返しますが、開けなかった場合は null を返します</returns>
        public Stream OpenRead(string name)
        {
            // カタログが存在しないなら
            if (!Exists(name))
            {
                // 開けないよ
                return null;
            }


            // ファイルをストリームとして開いて返す
            var path = Path.Combine(baseDirectoryInfo.FullName, name);
            return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 16 << 10, true);
        }


        /// <summary>
        /// 指定された名前のカタログを書き込むためのストリームを開きます
        /// </summary>
        /// <param name="name">書き込むカタログ名</param>
        /// <returns>指定されたカタログ名のストリームを開けた場合はインスタンスを返しますが、開けなかった場合は null を返します</returns>
        public Stream OpenWrite(string name)
        {
            // ベースディレクトリが存在しないなら
            if (ExistsBaseDirectory)
            {
                // 新しくディレクトリを作る
                baseDirectoryInfo.Create();
            }


            // ファイルをストリームとして開いて返す
            var path = Path.Combine(baseDirectoryInfo.FullName, name);
            return new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite, 16 << 10, true);
        }


        /// <summary>
        /// 指定された名前のカタログを一時的に読み込むためのストリームを開きます
        /// </summary>
        /// <param name="name">一時的に保持されたカタログの名前</param>
        /// <returns>指定されたカタログ名のストリームを開けた場合はインスタンスを返しますが、開けなかった場合は null を返します</returns>
        /// <exception cref="ArgumentNullException">name が null です</exception>
        public Stream OpenTemporaryRead(string name)
        {
            // 一時ディレクトリまたは一時カタログファイルが無いなら
            var path = Path.Combine(tempDirectoryInfo.FullName, name);
            if (!ExistsTempDirectory || !File.Exists(path))
            {
                // 何も出来ない
                return null;
            }


            // ストリームを開いて返す
            return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 16 << 10, true);
        }


        /// <summary>
        /// 指定された名前のカタログを一時的に書き込むためのストリームを開きます
        /// </summary>
        /// <param name="name">一時的に書き込むためのカタログの名前</param>
        /// <returns>指定されたカタログ名のストリームを開けた場合はインスタンスを返しますが、開けなかった場合は null を返します</returns>
        public Stream OpenTemporaryWrite(string name)
        {
            // 一時ディレクトリが無いなら
            if (!ExistsTempDirectory)
            {
                // 新しく作る
                tempDirectoryInfo.Create();
            }


            // ストリームを開いて返す
            var path = Path.Combine(tempDirectoryInfo.FullName, name);
            return new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite, 16 << 10, true);
        }


        /// <summary>
        /// 指定された名前のカタログを削除します
        /// </summary>
        /// <param name="name">削除するカタログ</param>
        public void Delete(string name)
        {
            // ファイルが存在しないのなら
            if (!Exists(name))
            {
                // 何もせず終了
                return;
            }


            // ファイルパスとして取得して削除をする
            File.Delete(Path.Combine(baseDirectoryInfo.FullName, name));
        }


        /// <summary>
        /// このストレージが管理している全てのカタログを削除します
        /// </summary>
        public void DeleteAll()
        {
            // ベースディレクトリが存在しないのなら
            if (!ExistsBaseDirectory)
            {
                // 何もせず終了
                return;
            }


            // ディレクトリごと削除する
            baseDirectoryInfo.Delete(true);
        }


        /// <summary>
        /// 指定された名前の一時カタログを削除します
        /// </summary>
        /// <param name="name">削除する一時カタログの名前</param>
        public void DeleteTemporary(string name)
        {
            // ファイルが存在しないのなら
            var path = Path.Combine(tempDirectoryInfo.FullName, name);
            if (!File.Exists(path))
            {
                // 何もせず終了
                return;
            }


            // ファイルを削除する
            File.Delete(path);
        }


        /// <summary>
        /// このストレージが管理している全ての一時カタログを削除します
        /// </summary>
        public void DeleteAllTemporary()
        {
            // 一時ディレクトリが存在しないのなら
            if (!ExistsTempDirectory)
            {
                // 何もせず終了
                return;
            }


            // ディレクトリごと削除する
            tempDirectoryInfo.Delete(true);
        }
    }
}