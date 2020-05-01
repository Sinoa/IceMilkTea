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
using System.IO;
using UnityEngine;

namespace IceMilkTea.SubSystem
{
    #region インターフェイスと基本型
    /// <summary>
    /// アセットストレージのアクセス方法を列挙します
    /// </summary>
    [Flags]
    public enum AssetStorageAccess : byte
    {
        /// <summary>
        /// 読み取りとしてアクセスします
        /// </summary>
        Read = FileAccess.Read,

        /// <summary>
        /// 書き込みとしてアクセスします
        /// </summary>
        Write = FileAccess.Write,
    }



    /// <summary>
    /// アセットとカタログ、データベースの貯蔵を行うインターフェイスです
    /// </summary>
    public interface IAssetStorage
    {
        /// <summary>
        /// 指定されたURIにアセットが存在するか否かを確認します
        /// </summary>
        /// <param name="localUri">確認するアセットのローカルURI</param>
        /// <returns>アセットが存在する場合は true を、存在しない場合は false を返します</returns>
        bool ExistsAsset(Uri localUri);


        /// <summary>
        /// アセットデータベースが存在するか否かを確認します
        /// </summary>
        /// <returns>アセットデータベースが存在する場合は true を、存在しない場合は false を返します</returns>
        bool ExistsAssetDatabase();


        /// <summary>
        /// 指定した名前のカタログが存在するか否かを確認します
        /// </summary>
        /// <param name="name">確認するカタログ名</param>
        /// <returns>カタログが存在する場合は true を、存在しない場合は false を返します</returns>
        bool ExistsCatalog(string name);


        /// <summary>
        /// アセットのストリームを開きます
        /// </summary>
        /// <param name="localUri">開くアセットのローカルURI</param>
        /// <param name="access">アセットへのアクセス方法</param>
        /// <returns>ストリームとして開けた場合は Stream のインスタンスを、開けなかった場合は null を返します</returns>
        Stream OpenAsset(Uri localUri, AssetStorageAccess access);


        /// <summary>
        /// アセットデータベースのストリームを開きます
        /// </summary>
        /// <param name="access">アセットデータベースへのアクセス方法</param>
        /// <returns>ストリームとして開けた場合は Stream のインスタンスを、開けなかった場合は null を返します</returns>
        Stream OpenAssetDatabase(AssetStorageAccess access);


        /// <summary>
        /// カタログのストリームを開きます
        /// </summary>
        /// <param name="name">開くカタログの名前</param>
        /// <param name="access">カタログへのアクセス方法</param>
        /// <returns>ストリームとして開けた場合は Stream のインスタンスを、開けなかった場合は null を返します</returns>
        Stream OpenCatalog(string name, AssetStorageAccess access);


        /// <summary>
        /// ストレージが管理しているアセットを削除します
        /// </summary>
        /// <param name="localUri">削除するアセットのローカルURI</param>
        void DeleteAsset(Uri localUri);


        /// <summary>
        /// ストレージが管理しているアセットデータベースを削除します
        /// </summary>
        void DeleteAssetDatabase();


        /// <summary>
        /// ストレージが管理しているカタログを削除します
        /// </summary>
        /// <param name="name">削除するカタログの名前</param>
        void DeleteCatalog(string name);


        /// <summary>
        /// ストレージが管理しているすべてのデータを削除します
        /// </summary>
        void DeleteAll();

        string ToAssetFilePath(Uri uri);
    }
    #endregion



    #region ファイルシステム実装
    /// <summary>
    /// 標準的なファイルシステムを利用して動作するファイルシステムアセットストレージクラスです
    /// </summary>
    public class FileSystemAssetStorage : IAssetStorage
    {
        // 定数定義
        public const string AssetDatabaseFileName = "package.db";
        public const string AssetDirectoryName = "assets";
        public const string CatalogDirectoryName = "catalog";
        public const int DefaultFileBufferSize = (16 << 10); // for iOS I/O size.

        // メンバ変数定義
        private readonly DirectoryInfo baseDirectoryInfo;
        private readonly DirectoryInfo assetDirectoryInfo;
        private readonly DirectoryInfo catalogDirectoryInfo;
        private readonly FileInfo assetDatabaseFileInfo;



        /// <summary>
        /// このストレージが管理するベースディレクトリが存在するか否か
        /// </summary>
        public bool ExistsBaseDirectory { get { baseDirectoryInfo.Refresh(); return baseDirectoryInfo.Exists; } }


        /// <summary>
        /// アセット格納用ディレクトリが存在するか否か
        /// </summary>
        public bool ExistsAssetDirectory { get { assetDirectoryInfo.Refresh(); return assetDirectoryInfo.Exists; } }


        /// <summary>
        /// カタログ格納用ディレクトリが存在するか否か
        /// </summary>
        public bool ExistsCatalogDirectory { get { catalogDirectoryInfo.Refresh(); return catalogDirectoryInfo.Exists; } }



        /// <summary>
        /// FileSystemAssetStorage クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="baseDirectoryInfo">このストレージが管理するベースディレクトリ</param>
        /// <exception cref="ArgumentNullException">baseDirectoryInfo が null です</exception>
        public FileSystemAssetStorage(DirectoryInfo baseDirectoryInfo)
        {
            // そのまま受け取って各種ディレクトリ情報とアセットデータベースファイル情報の初期化
            this.baseDirectoryInfo = baseDirectoryInfo ?? throw new ArgumentNullException(nameof(baseDirectoryInfo));
            assetDirectoryInfo = new DirectoryInfo(GetAssetDirectoryPath(baseDirectoryInfo));
            catalogDirectoryInfo = new DirectoryInfo(GetCatalogDirectoryPath(baseDirectoryInfo));
            assetDatabaseFileInfo = new FileInfo(GetAssetDatabasePath(baseDirectoryInfo));
        }


        #region ユーティリティ関数
        /// <summary>
        /// アセットデータベースパスを取得します
        /// </summary>
        /// <param name="baseDirectoryInfo">このストレージが管理するベースディレクトリ</param>
        /// <returns>アセットデータベースへのパスを返します</returns>
        protected virtual string GetAssetDatabasePath(DirectoryInfo baseDirectoryInfo)
        {
            // ベースディレクトリのフルパスにアセットデータベースファイル名を結合して返す
            return Path.Combine(baseDirectoryInfo.FullName, AssetDatabaseFileName);
        }


        /// <summary>
        /// アセット格納ディレクトリパスを取得します
        /// </summary>
        /// <param name="baseDirectoryInfo">このストレージが管理するベースディレクトリ</param>
        /// <returns>アセット格納ディレクトリパスを返します</returns>
        protected virtual string GetAssetDirectoryPath(DirectoryInfo baseDirectoryInfo)
        {
            // ベースディレクトリのフルパスにアセットディレクトリ名を結合して返す
            return Path.Combine(baseDirectoryInfo.FullName, AssetDirectoryName);
        }


        /// <summary>
        /// カタログ格納ディレクトリパスを取得します
        /// </summary>
        /// <param name="baseDirectoryInfo">このストレージが管理するベースディレクトリ</param>
        /// <returns>カタログ格納ディレクトリパスを返します</returns>
        protected virtual string GetCatalogDirectoryPath(DirectoryInfo baseDirectoryInfo)
        {
            // ベースディレクトリのフルパスにカタログディレクトリ名を結合して返す
            return Path.Combine(baseDirectoryInfo.FullName, CatalogDirectoryName);
        }


        /// <summary>
        /// URIからアセットファイルパスへ変換します
        /// </summary>
        /// <param name="uri">変換するURI</param>
        /// <returns>変換されたアセットファイルパスを返します</returns>
        public virtual string ToAssetFilePath(Uri uri)
        {
            // アセット格納ディレクトリパスにURIローカルパスを結合して返す
            var relativePath = uri.IsAbsoluteUri ? uri.LocalPath : uri.ToString();
            return Path.Combine(assetDirectoryInfo.FullName, relativePath.TrimStart('/'));
        }


        /// <summary>
        /// カタログ名からカタログファイルパスへ変換します
        /// </summary>
        /// <param name="name">変換するカタログ名</param>
        /// <returns>変換されたカタログファイルパスを返します</returns>
        protected virtual string ToCatalogFilePath(string name)
        {
            // カタログ格納ディレクトリパスにカタログファイル名を結合して返す
            return Path.Combine(catalogDirectoryInfo.FullName, $"{name}.catalog");
        }


        /// <summary>
        /// アセットのローカルURIが利用できるURIか否かを調べます
        /// </summary>
        /// <param name="localUri">調べるURI</param>
        /// <returns>利用できるURIの場合は true を、利用できない場合は false を返します</returns>
        protected virtual bool ValidateLocalUri(Uri localUri)
        {
            if (localUri is null) return false;
            if (!localUri.IsAbsoluteUri) return true; //相対UriならOK

            //絶対URIならfile://スキーム縛り
            return localUri.IsFile;
        }


        /// <summary>
        /// カタログ名が利用できる名前か否かを調べます
        /// </summary>
        /// <param name="name">調べるカタログ名</param>
        /// <returns>利用できる名前の場合は true を、利用できない場合は false を返します</returns>
        protected virtual bool ValidateCatalogName(string name)
        {
            // 有効な文字列かどうかの結果をそのまま返す
            return !string.IsNullOrWhiteSpace(name);
        }
        #endregion


        #region 存在確認関数
        /// <summary>
        /// 指定されたURIにアセットが存在するか否かを確認します
        /// </summary>
        /// <param name="localUri">確認するアセットのローカルURI</param>
        /// <returns>アセットが存在する場合は true を、存在しない場合は false を返します</returns>
        public bool ExistsAsset(Uri localUri)
        {
            // URIが扱えないURIなら
            if (!ValidateLocalUri(localUri))
            {
                // 存在しないとする
                return false;
            }


            // ファイルが存在するか否かの結果をそのまま返す
            return File.Exists(ToAssetFilePath(localUri));
        }


        /// <summary>
        /// アセットデータベースが存在するか否かを確認します
        /// </summary>
        /// <returns>アセットデータベースが存在する場合は true を、存在しない場合は false を返します</returns>
        public bool ExistsAssetDatabase()
        {
            // ファイルが存在するか否かの結果をそのまま返す
            assetDatabaseFileInfo.Refresh();
            return assetDatabaseFileInfo.Exists;
        }


        /// <summary>
        /// 指定した名前のカタログが存在するか否かを確認します
        /// </summary>
        /// <param name="name">確認するカタログ名</param>
        /// <returns>カタログが存在する場合は true を、存在しない場合は false を返します</returns>
        public bool ExistsCatalog(string name)
        {
            // カタログ名が扱えない名前なら
            if (!ValidateCatalogName(name))
            {
                // 存在しないとする
                return false;
            }


            // ファイルが存在するか否かの結果をそのまま返す
            return File.Exists(ToCatalogFilePath(name))
;
        }
        #endregion


        #region オープン関数
        /// <summary>
        /// アセットのストリームを開きます
        /// </summary>
        /// <param name="localUri">開くアセットのローカルURI</param>
        /// <param name="access">アセットへのアクセス方法</param>
        /// <returns>ストリームとして開けた場合は Stream のインスタンスを、開けなかった場合は null を返します</returns>
        public Stream OpenAsset(Uri localUri, AssetStorageAccess access)
        {
            // もし取り扱えないURIなら
            if (!ValidateLocalUri(localUri))
            {
                // 開くことすらかなわない
                return null;
            }


            // パスを取得する
            var path = ToAssetFilePath(localUri);


            // もし読み取りなら
            if (access == AssetStorageAccess.Read)
            {
                // ファイルが存在しないときは
                if (!ExistsAsset(localUri))
                {
                    // 開くことは出来ない
                    return null;
                }


                // iOSの場合のみ
                if (Application.platform == RuntimePlatform.IPhonePlayer)
                {
                    // 読み取りストリームとして開いて返す
                    return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, DefaultFileBufferSize, true);
                }


                // それ以外は通常のオープンを使用
                return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            }
            else if (access == AssetStorageAccess.Write)
            {
                //必要に応じてDirectoryを作成する
                Directory.CreateDirectory(Path.GetDirectoryName(path));


                // iOSの場合のみ
                if (Application.platform == RuntimePlatform.IPhonePlayer)
                {
                    // 書き込みストリームとして開いて返す
                    return new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite, DefaultFileBufferSize, true);
                }


                // それ以外は通常のオープンを使用
                return new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            }


            // ここには到達することはないはずだが、万が一来てしまった場合は null を返す
            return null;
        }


        /// <summary>
        /// アセットデータベースのストリームを開きます
        /// </summary>
        /// <param name="access">アセットデータベースへのアクセス方法</param>
        /// <returns>ストリームとして開けた場合は Stream のインスタンスを、開けなかった場合は null を返します</returns>
        public Stream OpenAssetDatabase(AssetStorageAccess access)
        {
            // パスを取得する
            var path = assetDatabaseFileInfo.FullName;


            // もし読み取りなら
            if (access == AssetStorageAccess.Read)
            {
                // 存在しないときは
                if (!ExistsAssetDatabase())
                {
                    // 開くことは出来ない
                    return null;
                }


                // 読み取りストリームとして開いて返す
                return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, DefaultFileBufferSize, true);
            }
            else if (access == AssetStorageAccess.Write)
            {
                // 書き込みストリームとして開いて返す
                return new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite, DefaultFileBufferSize, true);
            }


            // ここには到達することはないはずだが、万が一来てしまった場合は null を返す
            return null;
        }


        /// <summary>
        /// カタログのストリームを開きます
        /// </summary>
        /// <param name="name">開くカタログの名前</param>
        /// <param name="access">カタログへのアクセス方法</param>
        /// <returns>ストリームとして開けた場合は Stream のインスタンスを、開けなかった場合は null を返します</returns>
        public Stream OpenCatalog(string name, AssetStorageAccess access)
        {
            // もし取り扱えないカタログ名なら
            if (!ValidateCatalogName(name))
            {
                // 開くことはかなわない
                return null;
            }


            // パスを取得する
            var path = ToCatalogFilePath(name);


            // もし読み取りなら
            if (access == AssetStorageAccess.Read)
            {
                // 存在しないときは
                if (!ExistsCatalog(name))
                {
                    // 開くことは出来ない
                    return null;
                }


                // 読み取りストリームとして開いて返す
                return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, DefaultFileBufferSize, true);
            }
            else if (access == AssetStorageAccess.Write)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                // 書き込みストリームとして開いて返す
                return new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite, DefaultFileBufferSize, true);
            }

            // ここには到達することはないはずだが、万が一来てしまった場合は null を返す
            return null;
        }
        #endregion


        #region 削除関数
        /// <summary>
        /// ストレージが管理しているアセットを削除します
        /// </summary>
        /// <param name="localUri">削除するアセットのローカルURI</param>
        public void DeleteAsset(Uri localUri)
        {
            // 存在するなら
            if (ExistsAsset(localUri))
            {
                // 削除する
                File.Delete(ToAssetFilePath(localUri));
            }
        }


        /// <summary>
        /// ストレージが管理しているアセットデータベースを削除します
        /// </summary>
        public void DeleteAssetDatabase()
        {
            // 存在するなら
            if (ExistsAssetDatabase())
            {
                // 削除する
                File.Delete(assetDatabaseFileInfo.FullName);
            }
        }


        /// <summary>
        /// ストレージが管理しているカタログを削除します
        /// </summary>
        /// <param name="name">削除するカタログの名前</param>
        public void DeleteCatalog(string name)
        {
            // 存在するなら
            if (ExistsCatalog(name))
            {
                // 削除する
                File.Delete(ToCatalogFilePath(name));
            }
        }


        /// <summary>
        /// ストレージが管理しているすべてのデータを削除します
        /// </summary>
        public void DeleteAll()
        {
            // ベースディレクトリが存在するなら
            if (ExistsBaseDirectory)
            {
                // ベースディレクトリごと削除する
                baseDirectoryInfo.Delete(true);
            }
        }
        #endregion
    }
    #endregion
}