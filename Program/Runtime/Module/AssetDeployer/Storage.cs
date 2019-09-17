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
using System.Threading.Tasks;

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
    /// ストレージの削除進捗レポートの内容を持つ構造体です
    /// </summary>
    public readonly struct StorageDeleteReport
    {
        /// <summary>
        /// 削除する予定のトータル
        /// </summary>
        public int TotalDeleteCount { get; }


        /// <summary>
        /// 削除したデータ数
        /// </summary>
        public int DeletedCount { get; }


        /// <summary>
        /// カタログを削除している場合のカタログ名。カタログ以外の削除をしている場合は null になります。
        /// </summary>
        public string CatalogName { get; }


        /// <summary>
        /// アセットを削除している場合のアセットローカルURI。アセット以外の削除をしている場合は null になります。
        /// </summary>
        public Uri LocalUri { get; }



        /// <summary>
        /// StorageDeleteReport 構造体のインスタンスを初期化します
        /// </summary>
        /// <param name="total">削除する予定のトータル</param>
        /// <param name="deleted">削除したデータ数</param>
        /// <param name="catalogName">削除しているカタログ名</param>
        /// <param name="localUri">削除しているアセットローカルURI</param>
        public StorageDeleteReport(int total, int deleted, string catalogName, Uri localUri)
        {
            // そのまま受け取る
            TotalDeleteCount = total;
            DeletedCount = deleted;
            CatalogName = catalogName;
            LocalUri = localUri;
        }
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


        /// <summary>
        /// ストレージが管理しているすべてのデータを非同期で削除します
        /// </summary>
        /// <param name="progress">削除の進捗通知を受ける進捗オブジェクト</param>
        /// <returns>削除を実行しているタスクを返します</returns>
        Task DeleteAllAsync(IProgress<StorageDeleteReport> progress);
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

        // メンバ変数定義
        private DirectoryInfo baseDirectoryInfo;
        private DirectoryInfo assetDirectoryInfo;
        private DirectoryInfo catalogDirectoryInfo;
        private FileInfo assetDatabaseFileInfo;



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
        protected virtual string ToAssetFilePath(Uri uri)
        {
            // アセット格納ディレクトリパスにURIローカルパスを結合して返す
            return Path.Combine(assetDirectoryInfo.FullName, uri.LocalPath.TrimStart('/'));
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
            if (localUri == null || !localUri.IsFile)
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
            if (string.IsNullOrWhiteSpace(name))
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
            throw new NotImplementedException();
        }


        /// <summary>
        /// アセットデータベースのストリームを開きます
        /// </summary>
        /// <param name="access">アセットデータベースへのアクセス方法</param>
        /// <returns>ストリームとして開けた場合は Stream のインスタンスを、開けなかった場合は null を返します</returns>
        public Stream OpenAssetDatabase(AssetStorageAccess access)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// カタログのストリームを開きます
        /// </summary>
        /// <param name="name">開くカタログの名前</param>
        /// <param name="access">カタログへのアクセス方法</param>
        /// <returns>ストリームとして開けた場合は Stream のインスタンスを、開けなかった場合は null を返します</returns>
        public Stream OpenCatalog(string name, AssetStorageAccess access)
        {
            throw new NotImplementedException();
        }
        #endregion


        #region 削除関数
        /// <summary>
        /// ストレージが管理しているアセットを削除します
        /// </summary>
        /// <param name="localUri">削除するアセットのローカルURI</param>
        public void DeleteAsset(Uri localUri)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// ストレージが管理しているアセットデータベースを削除します
        /// </summary>
        public void DeleteAssetDatabase()
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// ストレージが管理しているカタログを削除します
        /// </summary>
        /// <param name="name">削除するカタログの名前</param>
        public void DeleteCatalog(string name)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// ストレージが管理しているすべてのデータを削除します
        /// </summary>
        public void DeleteAll()
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// ストレージが管理しているすべてのデータを非同期で削除します
        /// </summary>
        /// <param name="progress">削除の進捗通知を受ける進捗オブジェクト</param>
        /// <returns>削除を実行しているタスクを返します</returns>
        public Task DeleteAllAsync(IProgress<StorageDeleteReport> progress)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
    #endregion
}