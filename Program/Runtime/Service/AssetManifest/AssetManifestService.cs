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
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using IceMilkTea.Core;
using UnityEngine;

namespace IceMilkTea.Service
{
    #region サービス本体
    /// <summary>
    /// ゲーム自身が利用するゲームアセットの管理方法の状態を確認する機能を提供するサービスクラスです
    /// </summary>
    public class AssetManifestService : GameService
    {
        // 定数定義
        private const int DefaultCapacity = 1 << 10;

        // メンバ変数定義
        private List<AssetManifestFetcher> fetcherList;
        private Dictionary<ulong, AssetEntry> assetEntryTable;



        /// <summary>
        /// AssetManifestService のインスタンスを初期化します
        /// </summary>
        public AssetManifestService()
        {
            // もろもろ初期化
            fetcherList = new List<AssetManifestFetcher>();
            assetEntryTable = new Dictionary<ulong, AssetEntry>(DefaultCapacity);
        }


        /// <summary>
        /// AssetManifestFetcher を登録します
        /// </summary>
        /// <param name="fetcher">登録する AssetManifestFetcher</param>
        /// <exception cref="ArgumentNullException">fetcher が null です</exception>
        /// <exception cref="InvalidOperationException">既に登録済みの fetcher です</exception>
        public void RegisterAssetManifestFetcher(AssetManifestFetcher fetcher)
        {
            // null を渡されたら
            if (fetcher == null)
            {
                // それは受け付けられない
                throw new ArgumentNullException(nameof(fetcher));
            }


            // 既に同じインスタンスが存在していたら
            if (fetcherList.Contains(fetcher))
            {
                // 同じインスタンスの登録は許されない
                throw new InvalidOperationException($"既に登録済みの {nameof(fetcher)} です");
            }


            // 追加する
            fetcherList.Add(fetcher);
        }


        /// <summary>
        /// マニフェストのフェッチを非同期で行います
        /// </summary>
        /// <param name="progress">フェッチの進捗通知を受ける IProgress 不要の場合は null の指定が可能です</param>
        /// <returns>マニフェストのフェッチを非同期で操作しているタスクを返します</returns>
        public async Task FetchManifestAsync(IProgress<double> progress)
        {
            // 管理しているマニフェストの数で1を割って1フェッチャーあたりの進捗率を求める
            var unitProgressRate = 1.0 / fetcherList.Count;


            // 進捗オフセット及び進捗率を扱う進捗通知受付オブジェクトを生成する
            var progressOffset = 0.0;
            var fetcherProgress = new Progress<double>(x => progress?.Report(x * unitProgressRate + progressOffset));


            // フェッチャーの数分ループ
            for (int i = 0; i < fetcherList.Count; ++i)
            {
                // 進捗オフセットを求めて、フェッチャーの非同期実行を行う
                progressOffset = i * unitProgressRate;
                var manifest = await fetcherList[i].FetchAssetManifestAsync(fetcherProgress);


                // アセットエントリが存在しないなら
                if (manifest.AssetEntries == null)
                {
                    // 何事もなかったかのように次へ
                    continue;
                }


                // アセットエントリの数分回る
                for (int j = 0; j < manifest.AssetEntries.Length; ++j)
                {
                    // アセットエントリの名前からIDを作ってエントリテーブルに設定する
                    var assetEntryId = manifest.AssetEntries[j].Name.ToCrc64Code();
                    assetEntryTable[assetEntryId] = manifest.AssetEntries[j];
                }
            }
        }


        /// <summary>
        /// 指定されたアセット名から、フェッチ済みアセットエントリを取得します。
        /// </summary>
        /// <param name="assetName">取得するアセットエントリのアセット名</param>
        /// <param name="entry">取得されたアセットエントリを格納します</param>
        /// <exception cref="KeyNotFoundException">'{assetName}'のアセットエントリが見つかりませんでした</exception>
        public void GetAssetEntry(string assetName, out AssetEntry entry)
        {
            // アセット名からIDを作って値の取得を試みるが、失敗したら
            if (!assetEntryTable.TryGetValue(assetName.ToCrc64Code(), out entry))
            {
                // 取得出来なかったことを例外で吐く
                throw new KeyNotFoundException($"'{assetName}'のアセットエントリが見つかりませんでした");
            }
        }
    }
    #endregion



    #region マニフェスト関連構造体
    /// <summary>
    /// AssetManifestService が扱うマニフェストのルート構造を持った構造体です
    /// </summary>
    [Serializable]
    public struct AssetManifestRoot
    {
        /// <summary>
        /// マニフェスト名
        /// </summary>
        public string Name;

        /// <summary>
        /// マニフェストバージョン
        /// </summary>
        public int Version;

        /// <summary>
        /// マニフェストを生成したUNIXタイムスタンプ（ミリ秒）
        /// </summary>
        public long CreatedTimeStamp;

        /// <summary>
        /// マニフェストに登録されている
        /// </summary>
        public AssetEntry[] AssetEntries;
    }



    /// <summary>
    /// マニフェストに含まれるアセットの情報を持った構造体です
    /// </summary>
    [Serializable]
    public struct AssetEntry
    {
        /// <summary>
        /// アセット名
        /// </summary>
        public string Name;

        /// <summary>
        /// アセットURL
        /// </summary>
        public string AssetUrl;

        /// <summary>
        /// フェッチURL
        /// </summary>
        public string FetchUrl;

        /// <summary>
        /// インストールURL
        /// </summary>
        public string InstallUrl;

        /// <summary>
        /// アセットサイズ
        /// </summary>
        public long Size;

        /// <summary>
        /// 分割されたファイルの分割された総数。
        /// </summary>
        public int DivideTotalCount;

        /// <summary>
        /// 分割されたファイルの分割インデックス番号
        /// </summary>
        public int PartIndex;

        /// <summary>
        /// アセットハッシュ
        /// </summary>
        public byte[] AssetHash;
    }
    #endregion



    #region ManifestFetcher
    /// <summary>
    /// AssetManifestRoot をフェッチするフェッチャー抽象クラスです
    /// </summary>
    public abstract class AssetManifestFetcher
    {
        /// <summary>
        /// アセットマニフェストを非同期でフェッチします
        /// </summary>
        /// <param name="progress">マニフェストのフェッチの進捗通知を受ける IProgress</param>
        /// <returns>マニフェストを非同期でフェッチするタスクを返します</returns>
        public abstract Task<AssetManifestRoot> FetchAssetManifestAsync(IProgress<double> progress);
    }
    #endregion



    #region ManifestSerializer ＆ DefaultSerializer
    /// <summary>
    /// マニフェストのシリアライザ抽象クラスです
    /// </summary>
    public abstract class ManifestSerializer
    {
        /// <summary>
        /// マニフェストをシリアライズしストリームに出力します
        /// </summary>
        /// <param name="manifest">保存するマニフェストへの参照</param>
        /// <param name="stream">出力先ストリーム</param>
        public abstract void Save(Stream stream, ref AssetManifestRoot manifest);


        /// <summary>
        /// ストリームから入力してマニフェストをデシリアライズします
        /// </summary>
        /// <param name="manifest">デシリアライズされたマニフェストを書き込む参照</param>
        /// <param name="stream">入力ストリーム</param>
        public abstract void Load(Stream stream, out AssetManifestRoot manifest);
    }



    /// <summary>
    /// Unity組み込みのJsonUtilityを用いたマニフェストのシリアライザクラスです
    /// </summary>
    public class UnityJsonManifestSerializer : ManifestSerializer
    {
        /// <summary>
        /// マニフェストをシリアライズしストリームに出力します
        /// </summary>
        /// <param name="manifest">保存するマニフェストへの参照</param>
        /// <param name="stream">出力先ストリーム</param>
        public override void Save(Stream stream, ref AssetManifestRoot manifest)
        {
            // まずはJsonデータとしてシリアライズする
            var jsonData = JsonUtility.ToJson(manifest);


            // UTF8BOM無しでストリームに書き込む
            var utf8 = new UTF8Encoding(false);
            var buffer = utf8.GetBytes(jsonData);
            stream.Write(buffer, 0, buffer.Length);
        }


        /// <summary>
        /// ストリームから入力してマニフェストをデシリアライズします
        /// </summary>
        /// <param name="manifest">デシリアライズされたマニフェストを書き込む参照</param>
        /// <param name="stream">入力ストリーム</param>
        public override void Load(Stream stream, out AssetManifestRoot manifest)
        {
            // ストリームリーダにストリームを渡して、UTF8BOM無しですべての文字列を読み込む
            var jsonData = string.Empty;
            using (var streamReader = new StreamReader(stream, new UTF8Encoding(false), false, 4 << 10, true))
            {
                // すべての文字列を読み込む
                jsonData = streamReader.ReadToEnd();
            }


            // Jsonからマニフェストとしてデシリアライズする
            manifest = JsonUtility.FromJson<AssetManifestRoot>(jsonData);
        }
    }
    #endregion
}