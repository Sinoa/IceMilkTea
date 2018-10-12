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
using System.Threading.Tasks;
using IceMilkTea.Core;
using UnityEngine;

namespace IceMilkTea.Service
{
    /// <summary>
    /// IceMilkTeaが提供する単純なアセットバンドルマニフェストストレージクラスです
    /// </summary>
    public class ImtSimpleAssetBundleManifestStorage : AssetBundleManifestStorage
    {
        /// <summary>
        /// マニフェストの管理情報を保持したクラスです
        /// </summary>
        private class ManifestManagementInfo
        {
            /// <summary>
            /// 管理しているマニフェスト名の配列
            /// </summary>
            public string[] ManifestNames;
        }



        // 定数定義
        private const string ManifestManagementInfoFileName = "ImtManifestInfo.json";

        // メンバ変数定義
        private DirectoryInfo baseDirectoryInfo;
        private Dictionary<string, ImtAssetBundleManifest> manifestTable;



        /// <summary>
        /// ImtSimpleAssetBundleManifestStorage のインスタンスを初期化します
        /// </summary>
        /// <param name="baseDirectoryPath">マニフェストを格納するベースディレクトリパス</param>
        /// <exception cref="ArgumentNullException">baseDirectoryPath が null です</exception>
        /// <exception cref="ArgumentException">マニフェストストレージディレクトリパスに利用できない文字が含まれています</exception>
        public ImtSimpleAssetBundleManifestStorage(string baseDirectoryPath)
        {
            // もしnullを渡されたら
            if (baseDirectoryPath == null)
            {
                // nullは流石に受け入れられない
                throw new ArgumentNullException(nameof(baseDirectoryPath));
            }


            // パスに使えない文字が含まれていたら
            if (baseDirectoryPath.ContainInvalidPathChars())
            {
                // パスとして使えない文字が含まれていることを例外として吐く
                throw new ArgumentException($"マニフェストストレージディレクトリパスに利用できない文字が含まれています");
            }


            // ディレクトリ情報のインスタンスを生成する
            baseDirectoryInfo = new DirectoryInfo(baseDirectoryPath);


            // マニフェストテーブルのインスタンスを生成する
            manifestTable = new Dictionary<string, ImtAssetBundleManifest>();
        }


        /// <summary>
        /// 管理している全てのマニフェストの名前を取得します
        /// </summary>
        /// <returns>管理しているマニフェストの名前の配列を返します。1件もない場合でも長さ0の配列として返します</returns>
        public override string[] GetAllManifestName()
        {
            // マニフェストテーブルの管理件数が0件の場合は
            if (manifestTable.Count == 0)
            {
                // 長さ0の配列を返す
                Array.Empty<string>();
            }


            // テーブルに存在するキーを配列として返す
            var names = new string[manifestTable.Count];
            manifestTable.Keys.CopyTo(names, 0);
            return names;
        }


        /// <summary>
        /// マニフェスト管理情報ファイルの情報を取得します
        /// </summary>
        /// <returns>マニフェスト管理情報ファイルのインスタンスを返します</returns>
        private FileInfo GetManifestManagementInfoFileInfo()
        {
            // ベースパスとファイル名を結合したパスでファイル情報を生成して返す
            return new FileInfo(Path.Combine(baseDirectoryInfo.FullName, ManifestManagementInfoFileName));
        }


        /// <summary>
        /// 永続化されたマニフェストを非同期でロードします。
        /// 必ずストレージとして利用する場合は先にロードを行うようにしてください。
        /// </summary>
        /// <param name="progress">ロードの進捗通知を受ける IProgress</param>
        /// <returns>マニフェストを非同期でロードしているタスクを返します</returns>
        public override Task LoadAsync(IProgress<float> progress)
        {
            // ベースディレクトリ情報を更新して、ディレクトリが存在しないなら
            baseDirectoryInfo.Refresh();
            if (!baseDirectoryInfo.Exists)
            {
                // そもそもロードするものがないとして終了
                return Task.CompletedTask;
            }


            // マニフェスト管理情報ファイル情報を取得して存在しないなら
            var manifestManagementInfoFileInfo = GetManifestManagementInfoFileInfo();
            manifestManagementInfoFileInfo.Refresh();
            if (!manifestManagementInfoFileInfo.Exists)
            {
                // 読み込むものがないので終了
                return Task.CompletedTask;
            }


            // 全体を非同期タスクとして動かす
            return Task.Factory.StartNew(async () =>
            {
                // マニフェスト管理情報ファイルを開く
                var manifestManagementInfo = default(ManifestManagementInfo);
                using (var fileStream = new StreamReader(File.OpenRead(manifestManagementInfoFileInfo.FullName)))
                {
                    // Jsonデシリアライズして読み込む
                    manifestManagementInfo = JsonUtility.FromJson<ManifestManagementInfo>(await fileStream.ReadToEndAsync());
                }


                // マニフェストの名前の数分回る
                foreach (var manifestName in manifestManagementInfo.ManifestNames)
                {
                    // マニフェストの名前でマニフェストファイル名を生成して開く
                    var manifestFilePath = Path.Combine(baseDirectoryInfo.FullName, manifestName + ".json");
                    using (var fileStream = new StreamReader(File.OpenRead(manifestFilePath)))
                    {
                        // jsonデータを非同期で読み込む
                        var manifest = JsonUtility.FromJson<ImtAssetBundleManifest>(await fileStream.ReadToEndAsync());


                        // テーブルに読み込んだマニフェストを設定する
                        SetManifest(ref manifest);
                    }
                }
            });
        }


        /// <summary>
        /// 管理しているマニフェストを非同期で永続化します。
        /// </summary>
        /// <param name="progress">永続化の進捗通知を受ける IProgress</param>
        /// <returns>マニフェストを非同期で永続化しているタスクを返します</returns>
        public override Task SaveAsync(IProgress<float> progress)
        {
            // 全体を非同期タスクとして動かす
            return Task.Factory.StartNew(async () =>
            {
                // ベースディレクトリ情報を更新して、ディレクトリが存在しないなら
                baseDirectoryInfo.Refresh();
                if (!baseDirectoryInfo.Exists)
                {
                    // ディレクトリを生成する
                    baseDirectoryInfo.Create();
                }


                // マニフェスト管理情報を生成する
                var manifestManagementInfo = new ManifestManagementInfo();
                manifestManagementInfo.ManifestNames = new string[manifestTable.Count];


                // テーブルに含まれるレコード分回る
                int loopCount = 0;
                foreach (var manifest in manifestTable.Values)
                {
                    // マニフェストの名前を覚えてjsonシリアライズをする
                    manifestManagementInfo.ManifestNames[loopCount] = manifest.Name;
                    var jsonData = JsonUtility.ToJson(manifest);


                    // マニフェストの名前でマニフェストファイル名を生成して開く
                    var manifestFilePath = Path.Combine(baseDirectoryInfo.FullName, manifest.Name + ".json");
                    using (var fileStream = new StreamWriter(File.OpenWrite(manifestFilePath)))
                    {
                        // jsonデータを非同期で書き込む
                        await fileStream.WriteAsync(jsonData);
                    }


                    // ループカウントをインクリメントして進捗通知
                    progress?.Report((float)++loopCount / manifestTable.Count);
                }


                // 最後にマニフェスト情報を保存する
                var manifestManagementInfoJsonData = JsonUtility.ToJson(manifestManagementInfo);
                using (var fileStream = new StreamWriter(File.OpenWrite(GetManifestManagementInfoFileInfo().FullName)))
                {
                    // 非同期で書き込む
                    await fileStream.WriteAsync(manifestManagementInfoJsonData);
                }
            });
        }


        /// <summary>
        /// マニフェストストレージに指定されたマニフェストを管理対象にします
        /// </summary>
        /// <param name="manifest">管理対象となるマニフェストの参照</param>
        public override void SetManifest(ref ImtAssetBundleManifest manifest)
        {
            // マニフェストテーブルに設定する
            manifestTable[manifest.Name] = manifest;
        }


        /// <summary>
        /// マニフェストストレージに管理さている、指定されたマニフェスト名のマニフェストを取得に試みます
        /// </summary>
        /// <param name="name">取得したいマニフェストの名前</param>
        /// <param name="manifest">取得したマニフェストを受け取る変数</param>
        /// <returns>マニフェストの取得に成功した場合は true を、失敗した場合は false を返します</returns>
        /// <exception cref="ArgumentNullException">name が null です</exception>
        public override bool TryGetManifest(string name, out ImtAssetBundleManifest manifest)
        {
            // nullを渡されたら
            if (name == null)
            {
                // null では引っ張り出せない
                throw new ArgumentNullException(nameof(name));
            }


            // 名前から取得した結果をそのまま帰す
            return manifestTable.TryGetValue(name, out manifest);
        }
    }
}