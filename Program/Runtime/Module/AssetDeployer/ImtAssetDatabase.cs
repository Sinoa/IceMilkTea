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
using System.Linq;
using System.Text;
using IceMilkTea.Core;

namespace IceMilkTea.SubSystem
{
    /// <summary>
    /// アセットの管理情報をデータベースとして取り扱うクラスです
    /// </summary>
    public class ImtAssetDatabase
    {
        // 定数定義
        private const int InitialCapacity = 5 << 10;

        // メンバ変数定義
        private IAssetStorage assetStorage;
        private List<AssetInfoRecord> recordList;
        private AssetInfoRecord[] records;
        private ILookup<string, AssetInfoRecord> catalogLookup;
        private ILookup<string, AssetInfoRecord> assetNameLookup;



        /// <summary>
        /// ImtAssetDatabase クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="storage">アセットデータベースの実体管理を行っているストレージ</param>
        /// <exception cref="ArgumentNullException">storage が null です</exception>
        public ImtAssetDatabase(IAssetStorage storage)
        {
            // ストレージの参照を覚えてロードをする
            assetStorage = storage;
            Load();
        }


        public void Update(string catalogName, ICatalog catalog)
        {
        }


        public IEnumerable<AssetDiffInfo> GetDiff(string catalogName, ICatalog catalog)
        {
            // 返すための差分リストの生成
            var diffList = new List<AssetDiffInfo>();


            // アセットデータベース側のカタログがないなら
            if (!catalogLookup.Contains(catalogName))
            {
                // 受け取ったカタログはすべて新規という形で返す
                foreach (var item in catalog.GetItemAll())
                {
                    // 差分情報を生成してリストに追加
                    diffList.Add(new AssetDiffInfo(AssetDiffStatus.New, new AssetInfoRecord(catalogName, item)));
                }


                // リストを返す
                return diffList;
            }


            // カタログ側のアイテムを列挙
            foreach (var item in catalog.GetItemAll())
            {
                // データベース側にそもそも同じアセット名が存在しないなら
                if (!assetNameLookup.Contains(item.Name))
                {
                    // 新規で増えたアセットとして差分リストに追加して次へ
                    diffList.Add(new AssetDiffInfo(AssetDiffStatus.New, new AssetInfoRecord(catalogName, item)));
                    continue;
                }


                // 同じアセット名内で回る
                bool hitFlag = false;
                foreach (var record in assetNameLookup[item.Name])
                {
                    // 同じカタログ名なら
                    if (record.CatalogName == catalogName)
                    {
                        // カタログ名が一致したフラグをON
                        hitFlag = true;


                        // 同じハッシュコードの場合は
                        if (record.HashData.IsSameAll(item.HashData))
                        {
                            // 変化なしとする
                            diffList.Add(new AssetDiffInfo(AssetDiffStatus.Stable, new AssetInfoRecord(catalogName, item)));
                            break;
                        }


                        // 更新が必要
                        diffList.Add(new AssetDiffInfo(AssetDiffStatus.Update, new AssetInfoRecord(catalogName, item)));
                        break;
                    }
                }


                // 結局カタログ名が一致していないのなら
                if (!hitFlag)
                {
                    // 新規で増えたアセットとなる
                    diffList.Add(new AssetDiffInfo(AssetDiffStatus.New, new AssetInfoRecord(catalogName, item)));
                }
            }


            return diffList;
        }


        /// <summary>
        /// ストレージからアセットデータベースをロードします
        /// </summary>
        public void Load()
        {
            // ストレージにアセットデータベースが存在するか確認する
            if (!assetStorage.ExistsAssetDatabase())
            {
                // 存在しないなら空の状態で初期化する
                records = Array.Empty<AssetInfoRecord>();
                MakeLookup();
            }


            // ストレージからアセットデータベースを開いてバイナリリーダーで読み込む
            using (var databaseStream = assetStorage.OpenAssetDatabase(AssetStorageAccess.Read))
            using (var reader = new BinaryReader(databaseStream, new UTF8Encoding(false)))
            {
                // レコード数を読み込んでデータ配列を作る
                var recordNum = reader.ReadInt32();
                records = new AssetInfoRecord[recordNum];


                // レコード数の数分ループ
                for (int i = 0; i < recordNum; ++i)
                {
                    // レコードインスタンスを生成してデータを読み込む
                    var record = new AssetInfoRecord();
                    record.CatalogName = reader.ReadString();
                    record.AssetName = reader.ReadString();
                    record.RemoteUri = new Uri(reader.ReadString());
                    record.LocalUri = new Uri(reader.ReadString());
                    record.HashData = reader.ReadBytes(reader.ReadInt32());
                    record.HashName = reader.ReadString();


                    // データ配列に設定する
                    records[i] = record;
                }
            }
        }


        /// <summary>
        /// ストレージへアセットデータベースをセーブします
        /// </summary>
        public void Save()
        {
            // レコード数がそもそも0件なら
            if (records.Length == 0)
            {
                // 何もしない
                return;
            }


            // ストレージからアセットデータベースを開いてバイナリライターで書き込む
            using (var databaseStream = assetStorage.OpenAssetDatabase(AssetStorageAccess.Write))
            using (var writer = new BinaryWriter(databaseStream, new UTF8Encoding(false)))
            {
                // レコード数を書き込む
                writer.Write(records.Length);


                // レコードの数分回る
                foreach (var record in records)
                {
                    // ひたすら書き込む
                    writer.Write(record.CatalogName);
                    writer.Write(record.AssetName);
                    writer.Write(record.RemoteUri.OriginalString);
                    writer.Write(record.LocalUri.OriginalString);
                    writer.Write(record.HashData.Length);
                    writer.Write(record.HashData);
                    writer.Write(record.HashName);
                }
            }
        }


        /// <summary>
        /// ルックアップコレクションを作成します
        /// </summary>
        private void MakeLookup()
        {
            // ToLookupで作る
            catalogLookup = records.ToLookup(record => record.CatalogName);
            assetNameLookup = records.ToLookup(record => record.AssetName);
        }
    }



    /// <summary>
    /// データベース内に書き込まれているアセット情報レコードクラスです
    /// </summary>
    public class AssetInfoRecord
    {
        /// <summary>
        /// このアセット情報を持っていたカタログの名前
        /// </summary>
        public string CatalogName { get; set; }


        /// <summary>
        /// アセット名
        /// </summary>
        public string AssetName { get; set; }


        /// <summary>
        /// アセットをフェッチする参照リモートURI
        /// </summary>
        public Uri RemoteUri { get; set; }


        /// <summary>
        /// アセットをストレージにアクセスする際に使用するローカルURI
        /// </summary>
        public Uri LocalUri { get; set; }


        /// <summary>
        /// アセットのハッシュデータ
        /// </summary>
        public byte[] HashData { get; set; }


        /// <summary>
        /// ハッシュデータを計算する際に利用したハッシュ名
        /// </summary>
        public string HashName { get; set; }



        /// <summary>
        /// AssetInfoRecord クラスのインスタンスを初期化します
        /// </summary>
        public AssetInfoRecord()
        {
        }


        /// <summary>
        /// AssetInfoRecord クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="catalogName">カタログ名</param>
        /// <param name="catalogItem">インスタンスの生成元になるカタログアイテム</param>
        public AssetInfoRecord(string catalogName, ICatalogItem catalogItem)
        {
            // 適切に初期化する
            CatalogName = catalogName ?? string.Empty;
            AssetName = catalogItem.Name ?? string.Empty;
            RemoteUri = catalogItem.RemoteUri;
            LocalUri = catalogItem.LocalUri;
            HashName = catalogItem.HashName ?? string.Empty;
            HashData = catalogItem.HashData ?? Array.Empty<byte>();
        }
    }



    /// <summary>
    /// アセットの差分状態を列挙します
    /// </summary>
    public enum AssetDiffStatus
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
    public class AssetDiffInfo
    {
        /// <summary>
        /// 差分状態
        /// </summary>
        public AssetDiffStatus DiffStatus { get; set; }


        /// <summary>
        /// 差分情報の対象となったアセットレコード情報
        /// </summary>
        public AssetInfoRecord InfoRecord { get; set; }



        /// <summary>
        /// AssetDiffInfo クラスのインスタンスを初期化します
        /// </summary>
        public AssetDiffInfo()
        {
        }


        /// <summary>
        /// AssetDiffInfo クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="status">差分ステータス</param>
        /// <param name="record">多分対象となったアセット情報レコード</param>
        public AssetDiffInfo(AssetDiffStatus status, AssetInfoRecord record)
        {
            // そのまま受け取る
            DiffStatus = status;
            InfoRecord = record;
        }
    }
}