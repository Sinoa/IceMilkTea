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
using System.Text;
using IceMilkTea.Core;

namespace IceMilkTea.SubSystem
{
    #region データベース本体クラス
    /// <summary>
    /// アセットの管理情報をデータベースとして取り扱うクラスです
    /// </summary>
    public class ImtAssetDatabase
    {
        // メンバ変数定義
        private readonly Dictionary<string, ImtCatalog> catalogTable;
        private readonly IAssetStorage storage;



        /// <summary>
        /// ImtAssetDatabase クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="storage">アセットデータベースの実体管理を行っているストレージ</param>
        /// <exception cref="ArgumentNullException">storage が null です</exception>
        public ImtAssetDatabase(IAssetStorage storage)
        {
            // 初期化をする
            this.storage = storage ?? throw new ArgumentNullException(nameof(storage));
            catalogTable = new Dictionary<string, ImtCatalog>();
        }


        #region カタログ操作関数
        /// <summary>
        /// 指定されたカタログ名がアセットデータベースに含まれているか確認をします
        /// </summary>
        /// <param name="catalogName">確認をするカタログ名</param>
        /// <returns>カタログがある場合は true を、ない場合は false を返します</returns>
        /// <exception cref="ArgumentException">無効なカタログ名です</exception>
        public bool ContainsCatalog(string catalogName)
        {
            // ContainKeyの結果をそのまま返す
            ThrowExceptionIfInvalidCatalogName(catalogName);
            return catalogTable.ContainsKey(catalogName);
        }


        /// <summary>
        /// 指定されたカタログ名のカタログを取得します
        /// </summary>
        /// <param name="catalogName">取得するカタログ名</param>
        /// <returns>指定されたカタログがあればインスタンスを返しますが、ない場合は null を返します</returns>
        /// <exception cref="ArgumentException">無効なカタログ名です</exception>
        public ImtCatalog GetCatalog(string catalogName)
        {
            // TryGetの結果をそのまま返す
            ThrowExceptionIfInvalidCatalogName(catalogName);
            catalogTable.TryGetValue(catalogName, out var catalog);
            return catalog;
        }


        /// <summary>
        /// 指定されたカタログ名で、カタログの更新を行います。
        /// </summary>
        /// <param name="catalogName">更新するカタログ名</param>
        /// <param name="catalog">更新するカタログ</param>
        /// <exception cref="ArgumentException">無効なカタログ名です</exception>
        /// <exception cref="ArgumentNullException">catalog が null です</exception>
        public void UpdateCatalog(string catalogName, ICatalog catalog)
        {
            // テーブルにそのままカタログを登録する
            ThrowExceptionIfInvalidCatalogName(catalogName);
            catalogTable[catalogName] = new ImtCatalog(catalog ?? throw new ArgumentNullException(nameof(catalog)));
        }


        /// <summary>
        /// 指定されたカタログ名とカタログからアセットデータベースの差分を取得します。
        /// </summary>
        /// <param name="catalogName">差分の取得をするカタログ名</param>
        /// <param name="catalog">差分の抽出対象となるカタログ</param>
        /// <param name="differenceList">抽出結果を受け取るリスト</param>
        /// <exception cref="ArgumentException">無効なカタログ名です</exception>
        /// <exception cref="ArgumentNullException">catalog が null です</exception>
        /// <exception cref="ArgumentNullException">differenceList が null です</exception>
        public void GetCatalogDifference(string catalogName, ICatalog catalog, IList<AssetDifference> differenceList)
        {
            // 例外処理をする
            ThrowExceptionIfInvalidCatalogName(catalogName);
            catalog = new ImtCatalog(catalog ?? throw new ArgumentNullException(nameof(catalog)));
            if (differenceList == null)
            {
                // 何に追加すればよいのか
                throw new ArgumentNullException(nameof(differenceList));
            }


            // もしカタログ自体持っていないなら
            if (!ContainsCatalog(catalogName))
            {
                // 受け取ったカタログすべては新規追加とする
                foreach (var item in catalog.GetItemAll())
                {
                    // 新規追加としてリストに追加
                    differenceList.Add(new AssetDifference(AssetDifferenceStatus.New, catalogName, item));
                }


                // 終了
                return;
            }


            // データベース側のカタログの取得とアイテムすべてをリスト化
            var databaseCatalog = GetCatalog(catalogName);
            var removeTargetList = new List<ImtCatalogItem>(databaseCatalog.GetItemAll());


            // 比較対象となるカタログのアイテム分回る
            foreach (var item in catalog.GetItemAll())
            {
                // もし同じ名前のアイテムが見つからないなら
                var dbItem = databaseCatalog.GetItem(item.Name);
                if (dbItem == null)
                {
                    // 新規追加アイテムとして追加して次へ
                    differenceList.Add(new AssetDifference(AssetDifferenceStatus.New, catalogName, item));
                    continue;
                }


                // この段階で削除対象リストから情報を削除する
                removeTargetList.Remove(dbItem);


                // データベース側と比較対象アイテムのハッシュが一致した場合は
                if (dbItem.HashData.IsSameAll(item.HashData))
                {
                    // 変更なしとして追加して次へ
                    differenceList.Add(new AssetDifference(AssetDifferenceStatus.Stable, catalogName, item));
                    continue;
                }


                // 上書きが更新が必要として追加
                differenceList.Add(new AssetDifference(AssetDifferenceStatus.Update, catalogName, item));
            }


            // 削除対象リストに残ったアイテムはすべて削除対象となる
            foreach (var item in removeTargetList)
            {
                // 削除対象として追加
                differenceList.Add(new AssetDifference(AssetDifferenceStatus.Delete, catalogName, item));
            }
        }
        #endregion


        #region データIO関数
        /// <summary>
        /// ストレージからアセットデータベースをロードします
        /// </summary>
        public void Load()
        {
            // アセットデータベースのファイルが無いなら
            if (!storage.ExistsAssetDatabase())
            {
                // 何もせず諦める
                return;
            }


            // テーブルを空にしてコピー用アイテムリストを作っておく
            catalogTable.Clear();
            var bufferList = new List<ImtCatalogItem>();


            // ストレージからデータベースストリームを受け取ってバイナリリーダーに渡す
            using (var reader = new BinaryReader(storage.OpenAssetDatabase(AssetStorageAccess.Read), new UTF8Encoding(false)))
            {
                // カタログ数を読み込む
                var catalogCount = reader.ReadInt32();


                // カタログ数分回る
                for (int i = 0; i < catalogCount; ++i)
                {
                    // カタログ名とアイテム数を読み込む
                    var catalogName = reader.ReadString();
                    var itemCount = reader.ReadInt32();


                    // コピー用リストをクリアしてアイテム数分回る
                    bufferList.Clear();
                    for (int j = 0; j < itemCount; ++j)
                    {
                        // 順序よく読み込む
                        var name = reader.ReadString();
                        var contentLength = reader.ReadInt64();
                        var remoteUri = reader.ReadString();
                        var localUri = reader.ReadString();
                        var hashName = reader.ReadString();
                        var hashData = reader.ReadBytes(reader.ReadInt32());


                        // コピー用リストにアイテムを追加
                        bufferList.Add(new ImtCatalogItem(name, contentLength, new Uri(remoteUri), new Uri(localUri, UriKind.Relative), hashData, hashName));
                    }


                    // 新しくカタログを作ってテーブルに追加
                    catalogTable[catalogName] = new ImtCatalog(bufferList);
                }
            }
        }


        /// <summary>
        /// ストレージへアセットデータベースをセーブします
        /// </summary>
        public void Save()
        {
            // ストレージからデータベースストリームを受け取ってバイナリライターに渡す
            using (var writer = new BinaryWriter(storage.OpenAssetDatabase(AssetStorageAccess.Write), new UTF8Encoding(false)))
            {
                // カタログ数を書き込む
                writer.Write(catalogTable.Count);


                // カタログの数分回る
                foreach (var record in catalogTable)
                {
                    // カタログ名とアイテム数を書き込む
                    writer.Write(record.Key);
                    writer.Write(record.Value.ItemCount);


                    // カタログのアイテム数分回る
                    foreach (var item in record.Value.GetItemAll())
                    {
                        // アイテムの内容を順序よく書き込む
                        writer.Write(item.Name);
                        writer.Write(item.ContentLength);
                        writer.Write(item.RemoteUri.OriginalString);
                        writer.Write(item.LocalUri.OriginalString);
                        writer.Write(item.HashName);
                        writer.Write(item.HashData.Length);
                        writer.Write(item.HashData);
                    }
                }
            }
        }
        #endregion


        #region 例外関数
        /// <summary>
        /// カタログ名が無効な名前だった場合に例外を送出します
        /// </summary>
        /// <param name="name">確認するカタログ名</param>
        /// <exception cref="ArgumentException">無効なカタログ名です</exception>
        private void ThrowExceptionIfInvalidCatalogName(string name)
        {
            // 無効な名前を渡されたら
            if (string.IsNullOrWhiteSpace(name))
            {
                // 無効な引数である例外を吐く
                throw new ArgumentException("無効なカタログ名です", nameof(name));
            }
        }
        #endregion
    }
    #endregion



    #region アセット差分情報の定義
    /// <summary>
    /// アセットの差分状態を列挙します
    /// </summary>
    public enum AssetDifferenceStatus
    {
        /// <summary>
        /// 変更がありません
        /// </summary>
        Stable,

        /// <summary>
        /// 上書き更新が必要です
        /// </summary>
        Update,

        /// <summary>
        /// 削除が必要です
        /// </summary>
        Delete,

        /// <summary>
        /// 新規で増えました
        /// </summary>
        New,
    }



    /// <summary>
    /// アセットの差分状態情報を保持したクラスです
    /// </summary>
    public class AssetDifference
    {
        /// <summary>
        /// 差分状態
        /// </summary>
        public AssetDifferenceStatus Status { get; private set; }


        /// <summary>
        /// 差分情報の対象となったアセット情報
        /// </summary>
        public ImtCatalogItem Asset { get; private set; }


        /// <summary>
        /// 差分確認をしたカタログの名前
        /// </summary>
        public string CatalogName { get; private set; }



        /// <summary>
        /// AssetDifference クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="status">差分ステータス</param>
        /// <param name="catalogName">差分の元になったカタログ名</param>
        /// <param name="asset">対象となったアセット情報</param>
        public AssetDifference(AssetDifferenceStatus status, string catalogName, ICatalogItem asset)
        {
            // そのまま受け取る
            Status = status;
            CatalogName = catalogName;
            Asset = new ImtCatalogItem(asset);
        }


        /// <summary>
        /// AssetDifference クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="status">差分ステータス</param>
        /// <param name="catalogName">差分の元になったカタログ名</param>
        /// <param name="asset">対象となったアセット情報</param>
        public AssetDifference(AssetDifferenceStatus status, string catalogName, ImtCatalogItem asset)
        {
            // そのまま受け取る
            Status = status;
            CatalogName = catalogName;
            Asset = asset;
        }
    }
    #endregion
}