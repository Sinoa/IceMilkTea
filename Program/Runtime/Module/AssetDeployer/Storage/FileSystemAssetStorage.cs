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
using IceMilkTea.Core;

namespace IceMilkTea.Module
{
    /// <summary>
    /// 標準的なファイルシステムを利用して動作するファイルシステムアセットストレージクラスです
    /// </summary>
    public class FileSystemAssetStorage : IAssetStorage
    {
        // メンバ変数定義
        private DirectoryInfo baseDirectoryInfo;



        /// <summary>
        /// このストレージの名前を取得します
        /// </summary>
        public string Name { get; protected set; }


        /// <summary>
        /// このストレージが管理するベースディレクトリが存在するか否か
        /// </summary>
        public bool ExistsBaseDirectory { get { baseDirectoryInfo.Refresh(); return baseDirectoryInfo.Exists; } }



        /// <summary>
        /// FileSystemAssetStorage クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="baseDirectoryInfo">このストレージが管理するベースディレクトリ情報</param>
        /// <exception cref="ArgumentNullException">baseDirectoryInfo が null です</exception>
        public FileSystemAssetStorage(DirectoryInfo baseDirectoryInfo)
        {
            // ベースディレクトリ情報を受け取って自分のクラス名をそのままストレージ名にする
            this.baseDirectoryInfo = baseDirectoryInfo ?? throw new ArgumentNullException(nameof(baseDirectoryInfo));
            Name = GetType().Name;
        }


        /// <summary>
        /// 指定されたアセットURIからこのストレージが管理するファイルパスへ変換します。もしURIがフルパスを表現している場合はそのまま返すことに気をつけてください。
        /// </summary>
        /// <param name="assetUri">ストレージが管理するファイルパスとして表現するアセットURI</param>
        /// <returns>生成されたファイルパスの文字列を返します</returns>
        /// <exception cref="ArgumentNullException">assetUri が null です</exception>
        public string ToFilePath(Uri assetUri)
        {
            // ローカルパスを取得して（念の為先頭にセパレータがある場合は削除するようにする）実際のファイルパスを生成する
            var localPath = (assetUri ?? throw new ArgumentNullException(nameof(assetUri))).LocalPath.TrimStart('/');
            return Path.Combine(baseDirectoryInfo.FullName, localPath);
        }


        /// <summary>
        /// 指定されたファイル名からアセットURIへ変換します
        /// </summary>
        /// <param name="fileName">アセットURIとして表現するファイル名（またはパス）</param>
        /// <returns>生成されたアセットURIのインスタンスを返しますが fileName が null または 空文字列 を表現する場合は null を返します。</returns>
        public Uri ToAssetUri(string fileName)
        {
            // 有効なファイル名ではないのなら
            if (string.IsNullOrWhiteSpace(fileName))
            {
                // URIを表現出来ないので null を返す
                return null;
            }


            // ファイルスキームとして文字列を作ってURIとして返す
            return new Uri($"file:///{fileName}");
        }


        /// <summary>
        /// このストレージインスタンスが管理している全てのアセットURIを取得します
        /// </summary>
        /// <returns>取得されたURIの全てを列挙できる IEnumerable のインスタンスとして返します。1つもアセットがない場合は長さ0の IEnumerable を返します。</returns>
        public virtual IEnumerable<Uri> GetAssetUris()
        {
            // ベースディレクトリが無いのなら
            if (!ExistsBaseDirectory)
            {
                // そもそも管理しているアセットが無いので直ちに長さ0で返す
                return Array.Empty<Uri>();
            }


            // ベースディレクトリに含まれる全階層全てのファイルを列挙する
            var fileInfos = baseDirectoryInfo.GetFileSystemInfos("*", SearchOption.AllDirectories);
            var uriList = new List<Uri>(fileInfos.Length);
            foreach (var fileInfo in fileInfos)
            {
                // もしディレクトリ属性が付いていたら
                if (fileInfo.Attributes.HasFlag(FileAttributes.Directory))
                {
                    // ファイルのみ列挙したいので次へ
                    continue;
                }


                // フルネーム付きでURIインスタンスを生成してリストに追加
                uriList.Add(ToAssetUri(fileInfo.FullName));
            }


            // 列挙されたURIのリストをそのまま帰す
            return uriList;
        }


        /// <summary>
        /// 指定したアセットURIのアセットが存在するかどうか確認します
        /// </summary>
        /// <param name="assetUri">確認するアセットURI</param>
        /// <returns>アセットが存在する場合は true を、存在しない場合は false を返します</returns>
        /// <exception cref="ArgumentNullException">assetUri が null です</exception>
        public virtual bool Exists(Uri assetUri)
        {
            // ベースディレクトリがない または ファイルスキームではないのなら
            if (!ExistsBaseDirectory || !(assetUri ?? throw new ArgumentException(nameof(assetUri))).IsFileScheme())
            {
                // そもそも存在し得ない
                return false;
            }


            // 指定されたファイルパスが存在するかどうかの結果をそのまま返す
            return File.Exists(ToFilePath(assetUri));
        }


        /// <summary>
        /// 指定したアセットURIのアセットを読み込みストリームとして開きます
        /// </summary>
        /// <param name="assetUri">ストリームとして開きたいアセットURI</param>
        /// <returns>ストリームとして開けた場合はストリームのインスタンスを、開けなかった場合は null を返します</returns>
        /// <exception cref="ArgumentNullException">assetUri が null です</exception>
        public virtual Stream OpenRead(Uri assetUri)
        {
            // ファイルが存在しないのなら
            if (!Exists(assetUri))
            {
                // ストリームとして開けないので null を返す
                return null;
            }


            // ファイルパスとして取得して読み取りストリームとして返す(16KBファイルキャッシュなのはiOS向けに合わせているだけです)
            var filePath = ToFilePath(assetUri);
            return new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 16 << 10, true);
        }


        /// <summary>
        /// 指定したアセットURIのアセットを書き込みストリームとして開きます
        /// </summary>
        /// <param name="assetUri">ストリームとして開きたいアセットURI</param>
        /// <returns>ストリームとして開けた場合はストリームのインスタンスを、開けなかった場合は null を返します</returns>
        /// <exception cref="ArgumentNullException">assetUri が null です</exception>
        public virtual Stream OpenWrite(Uri assetUri)
        {
            // ベースディレクトリが存在しないなら
            if (!ExistsBaseDirectory)
            {
                // 新しく作る
                baseDirectoryInfo.Create();
            }


            // ファイルパスとして取得して書き込みストリームとして返す(16KBファイルキャッシュなのはiOS向けに合わせているだけです)
            var filePath = ToFilePath(assetUri);
            return new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite, 16 << 10, true);
        }


        /// <summary>
        /// 指定したアセットURIのアセットを削除します
        /// </summary>
        /// <param name="assetUri">削除するアセットURI</param>
        /// <exception cref="ArgumentNullException">assetUri が null です</exception>
        public virtual void Delete(Uri assetUri)
        {
            // ファイルが存在しないのなら
            if (!Exists(assetUri))
            {
                // 何もせず終了
                return;
            }


            // ファイルパスとして取得して削除をする
            File.Delete(ToFilePath(assetUri));
        }


        /// <summary>
        /// このストレージインスタンスが管理している全てのアセットを削除します
        /// </summary>
        public virtual void DeleteAll()
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
    }
}