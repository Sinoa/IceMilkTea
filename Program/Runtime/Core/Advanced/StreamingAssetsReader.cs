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
        private FileStream fileStream;
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


            // 基本クラスのDisposeも呼ぶ
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
    internal class CopyedTempStreamingAssetsReadStream : StreamingAssetsReader
    {
        public override long Length => 0;

        /// <summary>
        /// ファイルの位置の取得設定をします
        /// </summary>
        public override long Position
        {
            get { return 0; }
            set { }//value; }
        }

        protected override IAwaitable<StreamingAssetsReader> InternalOpenAsync(string fullAssetPath)
        {
            throw new NotImplementedException();
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }
    }
}