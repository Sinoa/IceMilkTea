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
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace IceMilkTea.Service
{
    /// <summary>
    /// アセットバンドルマニフェストを制御、管理を行うマネージャクラスです
    /// </summary>
    internal class AssetBundleManifestManager
    {
        /// <summary>
        /// 更新対象となったコンテンツグループの参照インデックスを保持する構造体です
        /// </summary>
        private struct UpdateContentGroupReferenceIndex
        {
            /// <summary>
            /// 更新対象となったコンテンツグループ名
            /// </summary>
            public string ContentGroupName;


            /// <summary>
            /// 更新対象となった新しいマニフェスト側の参照インデックス
            /// </summary>
            public int NewerContentGroupIndex;


            /// <summary>
            /// 更新対象となった古いマニフェスト側の参照インデックス
            /// </summary>
            public int OlderContentGroupIndex;



            /// <summary>
            /// UpdateContentGroupReferenceIndex のインスタンスを初期化します
            /// </summary>
            /// <param name="groupName">コンテンツグループ名</param>
            /// <param name="newerIndex">新しいマニフェスト側の参照インデックス</param>
            /// <param name="olderIndex">古いマニフェスト側の参照インデックス</param>
            public UpdateContentGroupReferenceIndex(string groupName, int newerIndex, int olderIndex)
            {
                // 値を受け取る
                ContentGroupName = groupName;
                NewerContentGroupIndex = newerIndex;
                OlderContentGroupIndex = olderIndex;
            }
        }



        // 定数定義
        private const string ManifestFileName = "AssetBundle.manifest";

        // メンバ変数定義
        private AssetBundleManifestFetcher fetcher;
        private ImtAssetBundleManifest manifest;
        private DirectoryInfo saveDirectoryInfo;



        /// <summary>
        /// AssetBundleManifestManager のインスタンスを初期化します
        /// </summary>
        /// <param name="fetcher">マニフェストの取り込みを行うフェッチャー</param>
        /// <param name="saveDirectoryInfo">マニフェストを保存するディレクトリ情報</param>
        /// <exception cref="ArgumentNullException">fetcher が null です</exception>
        /// <exception cref="ArgumentNullException">saveDirectoryInfo が null です</exception>
        public AssetBundleManifestManager(AssetBundleManifestFetcher fetcher, DirectoryInfo saveDirectoryInfo)
        {
            // もし null を渡された場合は
            if (fetcher == null)
            {
                // どうやってマニフェストを取り出そうか
                throw new ArgumentNullException(nameof(fetcher));
            }


            // 保存先ディレクトリ情報がnullなら
            if (saveDirectoryInfo == null)
            {
                // どこに保存すればいいんじゃ
                throw new ArgumentNullException(nameof(saveDirectoryInfo));
            }


            // 受け取る
            this.fetcher = fetcher;
            this.saveDirectoryInfo = saveDirectoryInfo;


            // マニフェストは空の状態で初期化
            manifest.LastUpdateTimeStamp = 0;
            manifest.ContentGroups = Array.Empty<AssetBundleContentGroup>();
        }


        #region Load and Save
        /// <summary>
        /// マニフェストファイルからマニフェストを非同期にロードします。
        /// あらゆる、操作の前に一度だけ実行するようにして下さい。
        /// </summary>
        /// <returns>非同期でロードしているタスクを返します</returns>
        public Task LoadManifestAsync()
        {
            // 保存ディレクトリ情報の更新とマニフェストファイルパスの用意
            saveDirectoryInfo.Refresh();
            var manifestFilePath = Path.Combine(saveDirectoryInfo.FullName, ManifestFileName);


            // マニフェストファイルが存在しないなら
            if (!File.Exists(manifestFilePath))
            {
                // ということはロードするものが無いので、完了タスクを返す
                return Task.CompletedTask;
            }


            // マニフェストのロード及びデシリアライズをタスクとして起動して返す
            return Task.Run(() =>
            {
                // マニフェストファイル内の文字列データをすべて読み込んで
                // Jsonデシリアライズしたものを自身の環境データとして初期化する
                var jsonData = File.ReadAllText(manifestFilePath);
                manifest = JsonUtility.FromJson<ImtAssetBundleManifest>(jsonData);
            });
        }


        /// <summary>
        /// 管理中のマニフェストをマニフェストファイルに非同期でセーブします。
        /// </summary>
        /// <returns>非同期でセーブしているタスクを返します</returns>
        private Task SaveManifestAsync()
        {
            // 保存ディレクトリ情報の更新を行ってディレクトリが存在していないなら
            saveDirectoryInfo.Refresh();
            if (!saveDirectoryInfo.Exists)
            {
                // ディレクトリを生成する
                saveDirectoryInfo.Create();
            }


            // マニフェストファイルパスの用意
            var manifestFilePath = Path.Combine(saveDirectoryInfo.FullName, ManifestFileName);


            // マニフェストのシリアリズ及びセーブをタスクとして起動して返す
            return Task.Run(() =>
            {
                // Jsonシリアライズしたものを、文字列データとしてマニフェストファイルに書き込む
                var jsonData = JsonUtility.ToJson(manifest);
                File.WriteAllText(manifestFilePath, jsonData);
            });
        }
        #endregion


        #region Manifest Fetch and Check and Update
        /// <summary>
        /// マニフェストの取り込みを非同期で行います。取り込んだマニフェストは、内部データに反映はされません。
        /// データとして更新が必要かどうかについては GetUpdatableAssetBundlesAsync() を用いてください。
        /// </summary>
        /// <returns>取り込みに成功した場合は、有効な参照の ImtAssetBundleManifest のインスタンスを返しますが、失敗した場合は null を返すタスクを返します</returns>
        public async Task<ImtAssetBundleManifest?> FetchManifestAsync()
        {
            // フェッチャーのフェッチをそのまま呼ぶ
            return await fetcher.FetchAsync();
        }


        /// <summary>
        /// 指定された新しいマニフェストを基に更新の必要のあるアセットバンドル情報の取得を非同期で行います。
        /// また、進捗通知はファイルチェック毎ではなく内部実装の既定に従った間隔で通知されます。
        /// </summary>
        /// <remarks>
        /// 最初の進捗通知が行われるよりも、先にチェックが完了した場合は一度も進捗通知がされないことに注意してください
        /// </remarks>
        /// <param name="newerManifest">新しいとされるマニフェスト</param>
        /// <param name="progress">チェック進捗通知を受ける Progress。もし通知を受けない場合は null の指定が可能です。</param>
        /// <returns>現在管理しているマニフェスト情報から、新しいマニフェスト情報で更新の必要なるアセットバンドル情報の配列を、操作しているタスクを返します。更新件数が 0 件でも長さ 0 の配列を返します</returns>
        /// <exception cref="ArgumentException">新しいマニフェストの '{nameof(ImtAssetBundleManifest.ContentGroups)}' が null です</exception>
        public Task<UpdatableAssetBundleInfo[]> GetUpdatableAssetBundlesAsync(ImtAssetBundleManifest newerManifest, IProgress<CheckAssetBundleProgress> progress)
        {
            // 渡されたアセットバンドルマニフェストが無効なカテゴリ配列を持っていた場合は
            if (newerManifest.ContentGroups == null)
            {
                // 引数の情報としてはあってはならないのでこれは例外とする
                throw new ArgumentException($"新しいマニフェストの '{nameof(ImtAssetBundleManifest.ContentGroups)}' が null です", nameof(newerManifest));
            }


            // もし新しいマニフェストと言うなの古いマニフェストなら
            if (manifest.LastUpdateTimeStamp >= newerManifest.LastUpdateTimeStamp)
            {
                // 更新する必要性がないとして長さ0の結果のタスクを返す
                return Task.FromResult(Array.Empty<UpdatableAssetBundleInfo>());
            }


            // 現在のフレームレートを知るが未設定の場合は30FPSと想定し、通知間隔のミリ秒を求める（約2フレーム間隔とする）
            var currentTargetFrameRate = Application.targetFrameRate == -1 ? 30 : Application.targetFrameRate;
            var notifyIntervalTime = (int)(1.0 / currentTargetFrameRate * 2000.0);


            // 進捗通知インターバル計測用ストップウォッチを生成して、進捗通知用ハンドラの生成
            var notifyIntervalStopwatch = Stopwatch.StartNew();
            Action<AssetBundleCheckStatus, string, int, int> notifyProgress = (status, name, total, checkedCount) =>
            {
                // もし進捗通知経過時間に到達していないなら
                if (notifyIntervalStopwatch.ElapsedMilliseconds < notifyIntervalTime)
                {
                    // まだ通知しない
                    return;
                }


                // 受け取ったパラメータで通知を行いストップウォッチを再起動する
                progress?.Report(new CheckAssetBundleProgress(status, name, total, checkedCount));
                notifyIntervalStopwatch.Restart();
            };


            // マニフェストの更新チェックを行うタスクを生成して返す
            return Task.Run(() =>
            {
                // 古いコンテンツグループと新しいコンテンツグループの参照を拾う
                var olderContentGroups = manifest.ContentGroups;
                var newerContentGroups = newerManifest.ContentGroups;


                // 今のうちに、新しいグループリスト、継続グループリスト、削除グループリストを生成しておく
                var newGroupIndexList = new List<int>(newerContentGroups.Length);
                var removeGroupIndexList = new List<int>(olderContentGroups.Length);
                var continuationGroupList = new List<UpdateContentGroupReferenceIndex>(newerContentGroups.Length);


                // 新しいマニフェストのグループ分回る
                for (int newerIndex = 0; newerIndex < newerContentGroups.Length; ++newerIndex)
                {
                    // 進捗通知を行う
                    notifyProgress(AssetBundleCheckStatus.NewerAndContinuationContentGroupCheck, newerContentGroups[newerIndex].Name, newerContentGroups.Length, newerIndex);


                    // 古いマニフェストのグループ分回る
                    var isNewGroupName = true;
                    var referenceOlderIndex = 0;
                    for (int olderIndex = 0; olderIndex < olderContentGroups.Length; ++olderIndex)
                    {
                        // もし同名のグループ名が存在するなら
                        if (newerContentGroups[newerIndex].Name == olderContentGroups[olderIndex].Name)
                        {
                            // 古いコンテンツグループのインデックスを覚えて、新しいグループ名ではないこと（つまり継続）を示してループから抜ける
                            referenceOlderIndex = olderIndex;
                            isNewGroupName = false;
                            break;
                        }
                    }


                    // もし新しいグループ名なら
                    if (isNewGroupName)
                    {
                        // 新しいグループ名リストに追加
                        newGroupIndexList.Add(newerIndex);
                    }
                    else
                    {
                        // 継続グループ名リストに追加
                        continuationGroupList.Add(new UpdateContentGroupReferenceIndex(newerContentGroups[newerIndex].Name, newerIndex, referenceOlderIndex));
                    }
                }


                // 古いマニフェストのグループ分回る
                for (int i = 0; i < olderContentGroups.Length; ++i)
                {
                    // 進捗通知を行う
                    notifyProgress(AssetBundleCheckStatus.FindRemoveContentGroupCheck, olderContentGroups[i].Name, olderContentGroups.Length, i);


                    // 新しいマニフェストグループ分回る
                    var exists = false;
                    for (int j = 0; j < newerContentGroups.Length; ++j)
                    {
                        // もし同名が存在するなら
                        if (olderContentGroups[i].Name == newerContentGroups[j].Name)
                        {
                            // まだ存在していることをを示してループから抜ける
                            exists = true;
                            break;
                        }
                    }


                    // 存在しないなら
                    if (!exists)
                    {
                        // 削除対象グループ名リストに追加
                        removeGroupIndexList.Add(i);
                    }
                }


                // 新旧アセットバンドル情報量分のキャパシティで更新が必要になるアセットバンドルリストの用意
                var capacity = newerManifest.TotalAssetBundleInfoCount + manifest.TotalAssetBundleInfoCount;
                var updatableAssetBundleInfoList = new List<UpdatableAssetBundleInfo>(capacity);


                // 新しいグループの列挙分回る
                foreach (var index in newGroupIndexList)
                {
                    // コンテンツグループ内のアセットバンドル情報分回る
                    var assetBundleInfos = newerContentGroups[index].AssetBundleInfos;
                    for (int i = 0; i < assetBundleInfos.Length; ++i)
                    {
                        // 進捗通知を行う
                        notifyProgress(AssetBundleCheckStatus.GetNewerAndRemoveAssetBundleInfo, assetBundleInfos[i].Name, assetBundleInfos.Length, i);


                        // 無条件で新規追加として覚える
                        updatableAssetBundleInfoList.Add(new UpdatableAssetBundleInfo(AssetBundleUpdateType.New, newerContentGroups[index].Name, ref assetBundleInfos[i]));
                    }
                }


                // 古いグループの列挙分回る
                foreach (var index in removeGroupIndexList)
                {
                    // コンテンツグループ内のアセットバンドル情報分回る
                    var assetBundleInfos = olderContentGroups[index].AssetBundleInfos;
                    for (int i = 0; i < assetBundleInfos.Length; ++i)
                    {
                        // 進捗通知を行う
                        notifyProgress(AssetBundleCheckStatus.GetNewerAndRemoveAssetBundleInfo, assetBundleInfos[i].Name, assetBundleInfos.Length, i);


                        // 無条件で削除として覚える
                        updatableAssetBundleInfoList.Add(new UpdatableAssetBundleInfo(AssetBundleUpdateType.Remove, newerContentGroups[index].Name, ref assetBundleInfos[i]));
                    }
                }


                // 新旧どちらとも存在する継続グループの列挙分回る
                foreach (var continuationInfo in continuationGroupList)
                {
                    // 新旧共にアセットバンドル情報とグループ名を取得する
                    var newerContentGroupIndex = continuationInfo.NewerContentGroupIndex;
                    var olderContentGroupIndex = continuationInfo.OlderContentGroupIndex;
                    var newerAssetBundleInfos = newerManifest.ContentGroups[newerContentGroupIndex].AssetBundleInfos;
                    var olderAssetBundleInfos = manifest.ContentGroups[olderContentGroupIndex].AssetBundleInfos;
                    var groupName = manifest.ContentGroups[olderContentGroupIndex].Name;


                    // 新しいアセットバンドル情報から回る
                    for (int newerIndex = 0; newerIndex < newerAssetBundleInfos.Length; ++newerIndex)
                    {
                        // 進捗通知を行う
                        notifyProgress(AssetBundleCheckStatus.CompareAssetBundleHash, newerAssetBundleInfos[newerIndex].Name, newerAssetBundleInfos.Length, newerIndex);


                        // 古いアセットバンドル情報分回る
                        for (int olderIndex = 0; olderIndex < olderAssetBundleInfos.Length; ++olderIndex)
                        {
                            // もし同じ名前のアセットバンドルが見つかった場合は
                            if (newerAssetBundleInfos[newerIndex].Name == olderAssetBundleInfos[olderIndex].Name)
                            {
                                // お互いのハッシュ配列の長さが違うなら
                                var newerHash = newerAssetBundleInfos[newerIndex].Hash;
                                var olderHash = olderAssetBundleInfos[olderIndex].Hash;
                                if (newerHash.Length != olderHash.Length)
                                {
                                    // 無条件で更新対象として覚えて内側ループを抜ける
                                    updatableAssetBundleInfoList.Add(new UpdatableAssetBundleInfo(AssetBundleUpdateType.Update, groupName, ref newerAssetBundleInfos[newerIndex]));
                                    break;
                                }


                                // 更にお互いのハッシュ値を比較するループをする
                                for (int i = 0; i < newerHash.Length; ++i)
                                {
                                    // もし異なる値が出てきたのなら
                                    if (newerHash[i] != olderHash[i])
                                    {
                                        // 更新対象として覚えてループを抜ける
                                        updatableAssetBundleInfoList.Add(new UpdatableAssetBundleInfo(AssetBundleUpdateType.Update, groupName, ref newerAssetBundleInfos[newerIndex]));
                                        break;
                                    }
                                }
                            }
                        }
                    }


                    // 古いアセットバンドル情報を回る
                    for (int olderIndex = 0; olderIndex < olderAssetBundleInfos.Length; ++olderIndex)
                    {
                        // 進捗通知を行う
                        notifyProgress(AssetBundleCheckStatus.CompareAssetBundleHash, olderAssetBundleInfos[olderIndex].Name, olderAssetBundleInfos.Length, olderIndex);


                        // 新しいアセットバンドル情報分回る
                        var isRemove = true;
                        for (int newerIndex = 0; newerIndex < newerAssetBundleInfos.Length; ++newerIndex)
                        {
                            // もし同じ名前のアセットバンドルが見つかった場合は
                            if (olderAssetBundleInfos[olderIndex].Name == newerAssetBundleInfos[newerIndex].Name)
                            {
                                // 削除対象ではないフラグを付けて内側ループから抜ける
                                isRemove = false;
                                break;
                            }
                        }


                        // もし削除対象として判定されたのなら
                        if (isRemove)
                        {
                            // 削除対象として覚えてループを抜ける
                            updatableAssetBundleInfoList.Add(new UpdatableAssetBundleInfo(AssetBundleUpdateType.Remove, groupName, ref olderAssetBundleInfos[olderIndex]));
                        }
                    }
                }


                // 列挙された更新可能な情報のリストを配列として返す
                return updatableAssetBundleInfoList.ToArray();
            });
        }


        /// <summary>
        /// 指定された新しいマニフェストで、現在管理しているマニフェストに非同期で更新します。
        /// </summary>
        /// <param name="newerManifest">新しいとされるマニフェスト</param>
        /// <returns>マニフェストの更新を行っているタスクを返します</returns>
        /// <exception cref="ArgumentException">古いマニフェストで更新しようとしました</exception>
        public Task UpdateManifestAsync(ImtAssetBundleManifest newerManifest)
        {
            // もし新しいマニフェストと言うなの古いマニフェストなら
            if (manifest.LastUpdateTimeStamp >= newerManifest.LastUpdateTimeStamp)
            {
                // 古いマニフェストで更新するのは良くない
                throw new ArgumentException("古いマニフェストで更新しようとしました", nameof(newerManifest));
            }


            // 問題なさそうなら新しいマニフェストとして上書きして保存するタスクを返す
            manifest = newerManifest;
            return SaveManifestAsync();
        }
        #endregion


        #region Get informations
        /// <summary>
        /// 指定された名前のアセットバンドル情報を取得します
        /// </summary>
        /// <param name="assetBundleName">アセットバンドル情報を取得する、アセットバンドル名</param>
        /// <param name="assetBundleInfo">取得されたアセットバンドルの情報を格納する参照</param>
        /// <exception cref="ArgumentNullException">assetBundleName が null です</exception>
        /// <exception cref="ArgumentException">アセットバンドル名 '{assetBundleName}' のアセットバンドル情報が見つかりませんでした</exception>
        public void GetAssetBundleInfo(string assetBundleName, out AssetBundleInfo assetBundleInfo)
        {
            // 名前に null が渡されたら
            if (assetBundleName == null)
            {
                // どうしろってんだい
                throw new ArgumentNullException(nameof(assetBundleName));
            }


            // 現在のマニフェストに含まれるコンテンツグループ分回る
            var contentGrops = manifest.ContentGroups;
            for (int i = 0; i < contentGrops.Length; ++i)
            {
                // コンテンツグループ内にあるアセットバンドル情報の数分回る
                var assetBundleInfos = contentGrops[i].AssetBundleInfos;
                for (int j = 0; j < assetBundleInfos.Length; ++j)
                {
                    // アセットバンドル名が一致したのなら
                    if (assetBundleInfos[j].Name == assetBundleName)
                    {
                        // この情報を渡して終了
                        assetBundleInfo = assetBundleInfos[j];
                        return;
                    }
                }
            }


            // ループから抜けてきたということは見つからなかったということ
            throw new ArgumentException($"アセットバンドル名 '{assetBundleName}' のアセットバンドル情報が見つかりませんでした", nameof(assetBundleName));
        }


        public string[] GetContentGroupNames()
        {
            throw new NotImplementedException();
        }


        public void GetContentGroupInfo(string contentGroupName, out AssetBundleContentGroup contentGroup)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}