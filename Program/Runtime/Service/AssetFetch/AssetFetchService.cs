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
using IceMilkTea.Core;

namespace IceMilkTea.Service
{
    #region サービス本体の実装
    /// <summary>
    /// ゲームアセットをゲームで利用できるようにするために取り込む機能を提供するサービスクラスです
    /// </summary>
    public class AssetFetchService : GameService
    {
        // クラス変数宣言
        private static readonly IProgress<AssetFetchProgressInfo> DefaultProgress = new Progress<AssetFetchProgressInfo>();

        // メンバ変数定義
        private AssetFetcherProvider fetcherProvider;
        private AssetInstallerProvider installerProvider;



        /// <summary>
        /// AssetFetchService のインスタンスを初期化します
        /// </summary>
        public AssetFetchService()
        {
            // サブシステムの生成
            fetcherProvider = new AssetFetcherProvider();
            installerProvider = new AssetInstallerProvider();
        }


        /// <summary>
        /// AssetFetcher を登録します
        /// </summary>
        /// <param name="fetcher">登録する AssetFetcher</param>
        public void RegisterFetcher(AssetFetcher fetcher)
        {
            // プロバイダにそのまま流し込む
            fetcherProvider.AddFetcher(fetcher);
        }


        /// <summary>
        /// AssetInstaller を登録します
        /// </summary>
        /// <param name="installer">登録する AssetInstaller</param>
        public void RegisterInstaller(AssetInstaller installer)
        {
            // プロバイダにそのまま流し込む
            installerProvider.AddInstaller(installer);
        }


        /// <summary>
        /// 指定されたフェッチURLからインストールURLに、アセットを非同期でフェッチします。
        /// </summary>
        /// <param name="fetchUrl">フェッチする基になるフェッチURL</param>
        /// <param name="installUrl">フェッチしたアセットをインストールするインストール先URL</param>
        /// <param name="progress">フェッチの進捗通知を受ける IProgress 不要な場合は null の指定が可能です</param>
        /// <returns>アセットのフェッチを非同期操作しているタスクを返します</returns>
        public IAwaitable FetchAssetAsync(string fetchUrl, string installUrl, IProgress<AssetFetchProgressInfo> progress)
        {
            // assetUrlが文字列として不適切なら
            if (string.IsNullOrWhiteSpace(fetchUrl))
            {
                // 例外を吐く
                throw new ArgumentException("指定されたフェッチURLが無効です", nameof(fetchUrl));
            }


            // installUrlが文字列として不適切なら
            if (string.IsNullOrWhiteSpace(installUrl))
            {
                // 例外を吐く
                throw new ArgumentException("指定されたインストールURLが無効です", nameof(installUrl));
            }


            // フェッチURLとインストールURLのインスタンスを生成
            var fetchUri = new Uri(fetchUrl);
            var installUri = new Uri(installUrl);


            // フェッチURLからアセットフェッチするフェッチャーを取得するが、担当が見つからなかったら
            var fetcher = fetcherProvider.GetFetcher(fetchUri);
            if (fetcher == null)
            {
                // ごめんなさい、フェッチ出来ません
                throw new InvalidOperationException("指定されたフェッチURLの対応が可能な fetcher が見つかりませんでした");
            }


            // インストールURLからアセットのインストーラを取得するが、担当が見つからなかったら
            var installer = installerProvider.GetInstaller(installUri);
            if (installer == null)
            {
                // ごめんなさい、インストールできません
                throw new InvalidOperationException("指定されたインストールURLの対応が可能な installer が見つかりませんでした");
            }


            // インストーラからインストールストリームを開いてもらい、フェッチャーに渡してアセットフェッチを開始
            var installStream = installer.Open(installUri);
            var result = fetcher.FetchAssetAsync(fetchUri, installStream, progress ?? DefaultProgress);
            installer.Close(installStream);


            // 非同期操作タスクを返す
            return result;
        }
    }
    #endregion



    #region 進捗通知情報の定義
    /// <summary>
    /// アセットフェッチ進捗の通知内容を保持した構造体です
    /// </summary>
    public struct AssetFetchProgressInfo
    {
        /// <summary>
        /// フェッチ中のフェッチURL
        /// </summary>
        public string FetchUrl { get; set; }


        /// <summary>
        /// フェッチ中のインストールURL
        /// </summary>
        public string InstallUrl { get; set; }


        /// <summary>
        /// アセットのフェッチ進捗率を正規化した値
        /// </summary>
        public double Progress { get; set; }
    }
    #endregion



    #region AssetInstallerの抽象クラスとProviderの実装
    /// <summary>
    /// 複数の AssetInstaller を保持し指定されたインストールURLから AssetInstaller を提供するクラスです
    /// </summary>
    internal class AssetInstallerProvider
    {
        // メンバ変数定義
        private List<AssetInstaller> installerList;



        /// <summary>
        /// AssetInstallerProvider のインスタンスを初期化します
        /// </summary>
        public AssetInstallerProvider()
        {
            // インストーラリストの生成
            installerList = new List<AssetInstaller>();
        }


        /// <summary>
        /// 指定されたインストーラを管理リストに追加します
        /// </summary>
        /// <param name="installer">追加するインストーラ</param>
        /// <exception cref="ArgumentNullException">installer が null です</exception>
        /// <exception cref="InvalidOperationException">既に登録済みの installer です</exception>
        public void AddInstaller(AssetInstaller installer)
        {
            // null を渡されたら
            if (installer == null)
            {
                // 何もできない
                throw new ArgumentNullException(nameof(installer));
            }


            // 既に追加済みの installer なら
            if (installerList.Contains(installer))
            {
                // もう追加出来ない
                throw new InvalidOperationException("既に登録済みの installer です");
            }


            // インストーラを追加する
            installerList.Add(installer);
        }


        /// <summary>
        /// 指定されたインストールURLから対応可能なら AssetInstaller を取得します
        /// </summary>
        /// <param name="url">インストールする予定のインストールURL</param>
        /// <returns>対応可能な AssetInstaller のインスタンスを返しますが、見つからなかった場合は null を返します</returns>
        /// <exception cref="ArgumentNullException">url が null です</exception>
        public AssetInstaller GetInstaller(Uri url)
        {
            // null を渡されていたら
            if (url == null)
            {
                // どんなインストーラをご所望ですか
                throw new ArgumentNullException(nameof(url));
            }


            // 管理しているインストーラの数分ループ
            foreach (var installer in installerList)
            {
                // インストーラから指定されたURLは対応可能と返却されたら
                if (installer.CanResolve(url))
                {
                    // このインストーラを返す
                    return installer;
                }
            }


            // 結局見つからなかった
            return null;
        }
    }



    /// <summary>
    /// アセットを実際にインストールするインストーラクラスです
    /// </summary>
    public abstract class AssetInstaller
    {
        /// <summary>
        /// 要求されたインストールURLの解決が可能かどうかを判断します
        /// </summary>
        /// <param name="installUrl">要求されているインストールURL</param>
        /// <returns>要求されているURLのインストールが可能な場合は true を、不可能であれば false を返します</returns>
        public abstract bool CanResolve(Uri installUrl);


        /// <summary>
        /// 指定されたインストールURLのインストールストリームを開きます
        /// </summary>
        /// <param name="installUrl">要求されているインストールURL</param>
        /// <returns>指定されたインストールURLにインストールするためのストリームインスタンスを返します</returns>
        public abstract Stream Open(Uri installUrl);


        /// <summary>
        /// 開いたインストールストリームを閉じます
        /// </summary>
        public abstract void Close(Stream stream);
    }
    #endregion



    #region AssetFetcherの抽象クラスとProviderの実装
    /// <summary>
    /// 複数の AssetFetcher を保持し指定されたフェッチURLから AssetFetcher を提供するクラスです
    /// </summary>
    internal class AssetFetcherProvider
    {
        // メンバ変数定義
        private List<AssetFetcher> fetcherList;



        /// <summary>
        /// AssetFetcherProvider のインスタンスを初期化します
        /// </summary>
        public AssetFetcherProvider()
        {
            // フェッチャーリストの初期化
            fetcherList = new List<AssetFetcher>();
        }


        /// <summary>
        /// 指定された AssetFetcher を管理リストに追加します
        /// </summary>
        /// <param name="fetcher">追加する AssetFetcher</param>
        /// <exception cref="ArgumentNullException">fetcher が null です</exception>
        /// <exception cref="InvalidOperationException">既に登録済みの fetcher です</exception>
        public void AddFetcher(AssetFetcher fetcher)
        {
            // null が渡されたら
            if (fetcher == null)
            {
                // 何も出来ません
                throw new ArgumentNullException(nameof(fetcher));
            }


            // 既に追加済みなら
            if (fetcherList.Contains(fetcher))
            {
                // 多重登録は許されない
                throw new InvalidOperationException("既に登録済みの fetcher です");
            }


            // AssetFetcherを追加する
            fetcherList.Add(fetcher);
        }


        /// <summary>
        /// 指定されたフェッチURLの対応可能な AssetFetcher を取得します
        /// </summary>
        /// <param name="url">要求されているフェッチURL</param>
        /// <returns>対応可能な AssetFetcher のインスタンスを返しますが、見つからなかった場合は null を返します</returns>
        /// <exception cref="ArgumentNullException">url が null です</exception>
        public AssetFetcher GetFetcher(Uri url)
        {
            // null を渡されたら
            if (url == null)
            {
                // どんなフェッチャーがお望みですか
                throw new ArgumentNullException(nameof(url));
            }


            // 管理しているフェッチャーの数分ループする
            foreach (var fetcher in fetcherList)
            {
                // 対応可能と返答が来たのなら
                if (fetcher.CanResolve(url))
                {
                    // この AssetFetcher を返す
                    return fetcher;
                }
            }


            // 対応可能な AssetFetcher がいなかった
            return null;
        }
    }



    /// <summary>
    /// アセットを実際にフェッチするフェッチャークラスです
    /// </summary>
    public abstract class AssetFetcher
    {
        /// <summary>
        /// 指定されたフェッチURLが対応可能かどうか判断します
        /// </summary>
        /// <param name="fetchUrl">要求されているフェッチURL</param>
        /// <returns>要求されているフェッチURLの対応が可能な場合は true を、不可能の場合は false を返します</returns>
        public abstract bool CanResolve(Uri fetchUrl);


        /// <summary>
        /// 非同期に、指定されたフェッチURLからアセットをフェッチし、インストーラから渡されたストリームに書き込みます
        /// </summary>
        /// <param name="fetchUrl">フェッチするURL</param>
        /// <param name="installStream">フェッチしたデータを書き込むインストーラが開いたインストールストリーム</param>
        /// <param name="progress">フェッチ状況の進捗通知をする IProgress</param>
        /// <returns>アセットのフェッチを非同期しているタスクを返します</returns>
        public abstract IAwaitable FetchAssetAsync(Uri fetchUrl, Stream installStream, IProgress<AssetFetchProgressInfo> progress);
    }
    #endregion



    #region AssetInstaller for FileStream
    public class FileStreamAssetInstaller : AssetInstaller
    {
        public override bool CanResolve(Uri installUrl)
        {
            throw new NotImplementedException();
        }

        public override void Close(Stream stream)
        {
            throw new NotImplementedException();
        }

        public override Stream Open(Uri installUrl)
        {
            throw new NotImplementedException();
        }
    }
    #endregion



    #region AssetFetcher for UnityWebRequest
    /// <summary>
    /// UnityのUnityWebRequestを使ったアセットデータのフェッチを行うフェッチャークラスです
    /// </summary>
    public class UnityWebRequestAssetFetcher : AssetFetcher
    {
        /// <summary>
        /// 指定されたフェッチURLが対応可能かどうか判断します
        /// </summary>
        /// <param name="fetchUrl">対応するフェッチURL</param>
        /// <returns>対応可能な場合は true を、不可能な場合は false を返します</returns>
        public override bool CanResolve(Uri fetchUrl)
        {
            throw new NotImplementedException();
        }

        public override IAwaitable FetchAssetAsync(Uri fetchUrl, Stream installStream, IProgress<AssetFetchProgressInfo> progress)
        {
            throw new NotImplementedException();
        }
    }
    #endregion



    #region AssetFetcher for WebRequest
    public class WebRequestAssetFetcher : AssetFetcher
    {
        public override bool CanResolve(Uri fetchUrl)
        {
            throw new NotImplementedException();
        }

        public override IAwaitable FetchAssetAsync(Uri fetchUrl, Stream installStream, IProgress<AssetFetchProgressInfo> progress)
        {
            throw new NotImplementedException();
        }
    }
    #endregion



    #region AssetFetcher for StreamingAssetReader
    public class StreamingAssetReaderAssetFetcher : AssetFetcher
    {
        public override bool CanResolve(Uri fetchUrl)
        {
            throw new NotImplementedException();
        }

        public override IAwaitable FetchAssetAsync(Uri fetchUrl, Stream installStream, IProgress<AssetFetchProgressInfo> progress)
        {
            throw new NotImplementedException();
        }
    }
    #endregion
}