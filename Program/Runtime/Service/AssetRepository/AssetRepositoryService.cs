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
using System.Text;
using IceMilkTea.Core;
using UnityEngine;

namespace IceMilkTea.Service
{
    #region サービス本体
    /// <summary>
    /// ゲーム自身が利用するゲームアセットの貯蔵状況の確認が行える機能を提供するサービスクラスです
    /// </summary>
    public class AssetRepositoryService : GameService
    {
        // メンバ変数定義
        private ManifestSerializer serializer;



        /// <summary>
        /// AssetRepositoryService のインスタンスを初期化します
        /// </summary>
        /// <param name="serializer">マニフェストの直列データを扱うためのシリアライザ</param>
        public AssetRepositoryService(ManifestSerializer serializer)
        {
            // nullを渡されたら
            if (serializer == null)
            {
                // シリアライザはないとだめ
                throw new ArgumentNullException(nameof(serializer));
            }


            // シリアライズを受け取る
            this.serializer = serializer;
        }
    }
    #endregion



    #region マニフェスト構造体
    /// <summary>
    /// AssetRepositoryService が扱うマニフェストのルート構造を持った構造体です
    /// </summary>
    [Serializable]
    public struct ManifestRoot
    {
        /// <summary>
        /// マニフェスト名
        /// </summary>
        public string ManifestName;

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
        public string AssetName;

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
        public abstract void Save(Stream stream, ref ManifestRoot manifest);


        /// <summary>
        /// ストリームから入力してマニフェストをデシリアライズします
        /// </summary>
        /// <param name="manifest">デシリアライズされたマニフェストを書き込む参照</param>
        /// <param name="stream">入力ストリーム</param>
        public abstract void Load(Stream stream, out ManifestRoot manifest);
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
        public override void Save(Stream stream, ref ManifestRoot manifest)
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
        public override void Load(Stream stream, out ManifestRoot manifest)
        {
            // ストリームリーダにストリームを渡して、UTF8BOM無しですべての文字列を読み込む
            var jsonData = string.Empty;
            using (var streamReader = new StreamReader(stream, new UTF8Encoding(false), false, 4 << 10, true))
            {
                // すべての文字列を読み込む
                jsonData = streamReader.ReadToEnd();
            }


            // Jsonからマニフェストとしてデシリアライズする
            manifest = JsonUtility.FromJson<ManifestRoot>(jsonData);
        }
    }
    #endregion
}