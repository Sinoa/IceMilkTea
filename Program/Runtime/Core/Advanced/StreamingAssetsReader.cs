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
using UnityEngine;
using UnityEngine.Networking;

namespace IceMilkTea.Core
{
    /// <summary>
    /// Unity の StreamingAssets から プラットフォーム共通な読み込みアクセスを提供する抽象ストリームクラスです
    /// </summary>
    public abstract class StreamingAssetsReader : Stream
    {
        // 読み取り専用クラス変数宣言
        private static readonly Dictionary<RuntimePlatform, Func<StreamingAssetsReader>> ReaderFactoryTable;
        private static readonly Func<StreamingAssetsReader> DefaultReaderFactory;



        /// <summary>
        /// 常に true を返します
        /// </summary>
        public override bool CanRead => true;


        /// <summary>
        /// 常に true を返します
        /// </summary>
        public override bool CanSeek => true;


        /// <summary>
        /// 常に false を返します
        /// </summary>
        public override bool CanWrite => false;



        /// <summary>
        /// StreamingAssetsReader の初期化を行います
        /// </summary>
        static StreamingAssetsReader()
        {
            // 各プラットフォーム毎の生成関数を登録
            ReaderFactoryTable = new Dictionary<RuntimePlatform, Func<StreamingAssetsReader>>()
            {
                // Android、iOSの追加
                {RuntimePlatform.Android, () => new CopyedTempStreamingAssetsReadStream()},
                {RuntimePlatform.IPhonePlayer, () => new DirectStreamingAssetsReadStream()},
            };


            // こちらが把握しているプラットフォーム以外の既定値の生成関数を登録
            DefaultReaderFactory = () => new DirectStreamingAssetsReadStream();
        }


        /// <summary>
        /// 指定された StreamingAssetパス のファイルを非同期に開きます
        /// </summary>
        /// <param name="assetPath">非同期に開く StreamingAssetパス</param>
        /// <returns>ファイルを非同期に開くのを待機する IAwaitable インスタンスを返します</returns>
        public static IAwaitable<StreamingAssetsReader> OpenAsync(string assetPath)
        {
            // 実際に開くべきアセットへのパスを用意する
            var fullAssetPath = Path.Combine(Application.streamingAssetsPath, assetPath);


            // 実行しているプラットフォームによって StreamingAssetsReader ファクトリ関数を引っ張る
            Func<StreamingAssetsReader> createReader;
            if (ReaderFactoryTable.TryGetValue(Application.platform, out createReader))
            {
                // 目的のファクトリ関数が取り出せたのならインスタンスを生成して非同期オープン関数を叩く
                return createReader().InternalOpenAsync(fullAssetPath);
            }


            // 対応可能なプラットフォームではないのなら既定のファクトリ関数を使って、非同期オープン関数を叩く
            return DefaultReaderFactory().InternalOpenAsync(fullAssetPath);
        }


        /// <summary>
        /// 実際の非同期オープン関数を実行します
        /// </summary>
        /// <param name="fullAssetPath">開くべき StreamingAsset へのフルパス</param>
        /// <returns>ファイルを非同期に開くのを待機する IAwaitable インスタンスを返します</returns>
        protected abstract IAwaitable<StreamingAssetsReader> InternalOpenAsync(string fullAssetPath);


        /// <summary>
        /// StreamingAssetsReader は、この関数をサポートしていません。常に NotSupportedException をスローします。
        /// </summary>
        /// <param name="value">サポートされていません</param>
        public override void SetLength(long value)
        {
            // サポートしていない例外を吐く
            throw new NotSupportedException("StreamingAssetsReader は SetLength をサポートしていません");
        }


        /// <summary>
        /// StreamingAssetsReader は、この関数をサポートしていません。常に NotSupportedException をスローします。
        /// </summary>
        /// <param name="buffer">サポートされていません</param>
        /// <param name="offset">サポートされていません</param>
        /// <param name="count">サポートされていません</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            // サポートしていない例外を吐く
            throw new NotSupportedException("StreamingAssetsReader は Write をサポートしていません");
        }
    }



    /// <summary>
    /// StreamingAssets へ直接アクセスするストリームクラスです
    /// </summary>
    internal class DirectStreamingAssetsReadStream : StreamingAssetsReader
    {
        // メンバ変数定義
        protected FileStream fileStream;
        private bool disposed;



        /// <summary>
        /// ファイルの長さを取得します
        /// </summary>
        public override long Length => fileStream.Length;


        /// <summary>
        /// ファイルの位置の取得設定をします
        /// </summary>
        public override long Position
        {
            get { return fileStream.Position; }
            set { fileStream.Position = value; }
        }



        /// <summary>
        /// リソースの解放を行います
        /// </summary>
        /// <param name="disposing">マネージ解放かどうか</param>
        protected override void Dispose(bool disposing)
        {
            // 既に解放済みなら
            if (disposed)
            {
                // 何もしない
                return;
            }


            // マネージ解放なら
            if (disposing)
            {
                // ファイルを解放する
                fileStream.Dispose();
            }


            // 解放済みマークをつけて基本クラスのDisposeも呼ぶ
            disposed = true;
            base.Dispose(disposing);
        }


        /// <summary>
        /// 指定された StreamingAssetパス のファイルを非同期に開きます
        /// </summary>
        /// <param name="fullAssetPath">開く StreamingAsset へのフルパス</param>
        /// <returns>ファイルを非同期に開くのを待機する IAwaitable インスタンスを返します</returns>
        protected override IAwaitable<StreamingAssetsReader> InternalOpenAsync(string fullAssetPath)
        {
            // 指定されたファイルを開く
            fileStream = new FileStream(fullAssetPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);


            // 既に直接開けるので完了済みManualResetを生成して返す
            // TODO : ここでも Completed な Awaitable が欲しい
            var completedWaitHandle = new ImtAwaitableManualReset<StreamingAssetsReader>(true);
            completedWaitHandle.PrepareResult(this);
            return completedWaitHandle;
        }


        /// <summary>
        /// ストリームのバッファをフラッシュします
        /// </summary>
        /// <exception cref="ObjectDisposedException">オブジェクトは既に解放済みです</exception>
        public override void Flush()
        {
            // 解放済み例外送出関数を呼ぶ
            ThrowIfDisposed();


            // そのまま本体のストリームをフラッシュする
            fileStream.Flush();
        }


        /// <summary>
        /// ストリームからデータを読み込みます
        /// </summary>
        /// <param name="buffer">読み込んだデータを書き込む先のバッファ</param>
        /// <param name="offset">バッファに書き込む開始位置</param>
        /// <param name="count">バッファから読み取るサイズ</param>
        /// <returns>実際に読み取ったデータのサイズを返しますが、末尾に到達している場合は 0 を返すことがあります</returns>
        /// <exception cref="ObjectDisposedException">オブジェクトは既に解放済みです</exception>
        public override int Read(byte[] buffer, int offset, int count)
        {
            // 解放済み例外送出関数を呼ぶ
            ThrowIfDisposed();


            // 本体のストリームのReadを叩く
            return fileStream.Read(buffer, offset, count);
        }


        /// <summary>
        /// ストリームの位置を移動します
        /// </summary>
        /// <param name="offset">移動する origin からのオフセット</param>
        /// <param name="origin">移動する基準</param>
        /// <returns>最終的な移動した位置を返します</returns>
        /// <exception cref="ObjectDisposedException">オブジェクトは既に解放済みです</exception>
        public override long Seek(long offset, SeekOrigin origin)
        {
            // 解放済み例外送出関数を呼ぶ
            ThrowIfDisposed();


            // 本体のストリームのSeekを叩く
            return fileStream.Seek(offset, origin);
        }


        /// <summary>
        /// 既にオブジェクが解放済みの場合例外を送出します
        /// </summary>
        /// <exception cref="ObjectDisposedException">オブジェクトは既に解放済みです</exception>
        private void ThrowIfDisposed()
        {
            // 既に解放済みなら
            if (disposed)
            {
                // 例外を吐く
                throw new ObjectDisposedException(GetType().FullName);
            }
        }
    }



    /// <summary>
    /// 一時領域にコピーされた StreamingAsset へアクセスするストリームクラスです
    /// </summary>
    internal class CopyedTempStreamingAssetsReadStream : DirectStreamingAssetsReadStream
    {
        // メンバ変数定義
        private ImtAwaitableManualReset<StreamingAssetsReader> waitHandle;



        /// <summary>
        /// 指定された StreamingAssetパス のファイルを非同期に開きます
        /// </summary>
        /// <param name="fullAssetPath">開く StreamingAsset へのフルパス</param>
        /// <returns>ファイルを非同期に開くのを待機する IAwaitable インスタンスを返します</returns>
        protected override IAwaitable<StreamingAssetsReader> InternalOpenAsync(string fullAssetPath)
        {
            // ファイルパスからCRC64ファイル名を作り出して、一時フォルダへのパスを用意する
            var fileName = string.Concat(fullAssetPath.ToCrc64HexText(), ".dat");
            var filePath = Path.Combine(Application.temporaryCachePath, fileName);


            // 待機ハンドルを生成して、一時ファイルコピーとオープンを非同期に行って、ハンドルを返す
            waitHandle = new ImtAwaitableManualReset<StreamingAssetsReader>(false);
            CopyAndOpenTemporaryFile(fullAssetPath, filePath);
            return waitHandle;
        }


        /// <summary>
        /// 非同期で StreamingAsset のファイルを一度一時ファイルとしてコピーしてから、コピーした一時ファイルを開きます
        /// </summary>
        /// <param name="fullAssetPath">StreamingAsset へのフルパス</param>
        /// <param name="temporaryFilePath">コピーする先の一時ファイルパス</param>
        private async void CopyAndOpenTemporaryFile(string fullAssetPath, string temporaryFilePath)
        {
            // ストリーミングアセットのファイルを一度 UniWebRequest として開いて
            // StreamingDownloadHandler を設定して、非同期ダウンロードで待機する
            var request = UnityWebRequest.Get(fullAssetPath);
            request.downloadHandler = new StreamingDownloadHandler(temporaryFilePath);
            await request.SendWebRequest();


            // ダウンロードしたファイルを開いて、待機ハンドルのシグナルを設定する
            fileStream = new FileStream(temporaryFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            waitHandle.Set(this);
        }



        /// <summary>
        /// 指定されたファイルへダウンロードデータを書き込みます
        /// </summary>
        private class StreamingDownloadHandler : DownloadHandlerScript
        {
            // メンバ変数定義
            private FileStream fileStream;
            private int contentLength;
            private int downloadedLength;



            /// <summary>
            /// StreamingDownloadHandler のインスタンスを初期化します
            /// </summary>
            /// <param name="filePath">ダウンロードする先のファイルパス</param>
            public StreamingDownloadHandler(string filePath)
            {
                // ファイルを書き込みとして開く
                fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            }


            /// <summary>
            /// ダウンロードするべきサイズを受け取ります
            /// </summary>
            /// <param name="contentLength">ダウンロードするべきコンテンツの長さ</param>
            protected override void ReceiveContentLength(int contentLength)
            {
                // コンテンツの最大長を覚えると共にファイルも伸長しておく
                this.contentLength = contentLength;
                fileStream.SetLength(contentLength);
            }


            /// <summary>
            /// ダウンロードするデータを受信します
            /// </summary>
            /// <param name="data">ダウンロードしたデータへのバッファ</param>
            /// <param name="dataLength">ダウンロードした長さ</param>
            /// <returns>ダウンロードを継続する場合は true を、中断する場合は false を返します</returns>
            protected override bool ReceiveData(byte[] data, int dataLength)
            {
                // ダウンロード済みの長さを加算してファイルにもデータを書き込んで続行
                downloadedLength += dataLength;
                fileStream.Write(data, 0, dataLength);
                return true;
            }


            /// <summary>
            /// ダウンロードの完了を受け付けます
            /// </summary>
            protected override void CompleteContent()
            {
                // ファイルを閉じる
                fileStream.Dispose();
            }


            /// <summary>
            /// 現在のダウンロード進捗を取得します
            /// </summary>
            /// <returns>現在のダウンロード進捗を返します</returns>
            protected override float GetProgress()
            {
                // 現在のダウンロード済みサイズと、最大長の割合を返す
                return downloadedLength / (float)contentLength;
            }


            /// <summary>
            /// ダウンロードしたデータを取得しますが
            /// このクラスでは、常に長さ0のbyteを返します
            /// </summary>
            /// <returns>長さ0のbyte配列を返します</returns>
            protected override byte[] GetData()
            {
                // 空の配列を返す
                return Array.Empty<byte>();
            }


            /// <summary>
            /// ダウンロードしたデータから文字列を取得しますが
            /// このクラスでは、常に空文字列を返します
            /// </summary>
            /// <returns>空文字列を返します</returns>
            protected override string GetText()
            {
                // 空文字列を返す
                return string.Empty;
            }
        }
    }
}