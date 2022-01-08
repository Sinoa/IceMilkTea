﻿// zlib/libpng License
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
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;

namespace IceMilkTea.Core
{
    #region IO処理中にハッシュ計算するストリーム
    /// <summary>
    /// ストリームの入出力をそのままハッシュ計算機に流し込むストリームクラスです
    /// </summary>
    public class HashStream : Stream
    {
        // メンバ変数定義
        private HashAlgorithm hash;
        private bool finish;
        private bool leaveOpen;
        private bool disposed;



        #region プロパティ
        /// <summary>
        /// 参照ストリームの CanRead をそのまま返します
        /// </summary>
        public override bool CanRead => BaseStream.CanRead;


        /// <summary>
        /// 参照ストリームの CanSeek をそのまま返します
        /// </summary>
        public override bool CanSeek => BaseStream.CanSeek;


        /// <summary>
        /// 参照ストリームの CanWrite をそのまま返します
        /// </summary>
        public override bool CanWrite => BaseStream.CanWrite;


        /// <summary>
        /// 参照ストリームの Length をそのまま返します
        /// </summary>
        public override long Length => BaseStream.Length;


        /// <summary>
        /// 参照ストリームの Position にそのままアクセスします
        /// </summary>
        public override long Position { get => BaseStream.Position; set => BaseStream.Position = value; }


        /// <summary>
        /// このインスタンスが参照している基本ストリームを取得します
        /// </summary>
        public Stream BaseStream { get; private set; }
        #endregion



        #region 初期化＆解放
        /// <summary>
        /// HashStream クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="baseStream">ハッシュ計算を行う対象となるストリーム</param>
        /// <param name="hash">使用するハッシュアルゴリズム</param>
        /// <exception cref="ArgumentNullException">baseStream が null です</exception>
        /// <exception cref="ArgumentNullException">hash が null です</exception>
        public HashStream(Stream baseStream, HashAlgorithm hash) : this(baseStream, hash, false)
        {
        }


        /// <summary>
        /// HashStream クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="baseStream">ハッシュ計算を行う対象となるストリーム</param>
        /// <param name="hash">使用するハッシュアルゴリズム</param>
        /// <param name="leaveOpen">このインスタンスが破棄される時に baseStream を継続して開き続ける場合は true を、破棄する場合は false を指定。既定は false です。</param>
        /// <exception cref="ArgumentNullException">baseStream が null です</exception>
        /// <exception cref="ArgumentNullException">hash が null です</exception>
        public HashStream(Stream baseStream, HashAlgorithm hash, bool leaveOpen)
        {
            // 初期化をする
            BaseStream = baseStream ?? throw new ArgumentNullException(nameof(baseStream));
            this.hash = hash ?? throw new ArgumentNullException(nameof(hash));
            this.leaveOpen = leaveOpen;
        }


        /// <summary>
        /// 現在使用しているリソースを破棄します
        /// </summary>
        /// <param name="disposing">マネージオブジェクトの破棄の場合は true それ以外は false</param>
        protected override void Dispose(bool disposing)
        {
            // すでに破棄済みなら
            if (disposed)
            {
                // 何もせず終了
                return;
            }


            // マネージの破棄なら
            if (disposing)
            {
                // もしこのインスタンスが破棄される時に参照ストリームを破棄するなら
                if (!leaveOpen)
                {
                    // 参照ストリームのDisposeも呼ぶ
                    BaseStream.Dispose();
                }


                // ハッシュインスタンスを破棄
                hash.Dispose();
            }


            // 破棄済みをマーク
            disposed = true;


            // 基本クラスのDisposeも呼ぶ
            base.Dispose(disposing);
        }
        #endregion


        #region ハッシュ操作関数
        /// <summary>
        /// すべてのストリーム操作が終わった時に、計算されたハッシュデータを取得します
        /// </summary>
        /// <param name="result">取得されたハッシュデータを受け取る配列の参照</param>
        public void GetHashData(out byte[] result)
        {
            // 新しい計算済みハッシュコードの受け取り配列を生成する
            ThrowExceptionIfObjectDisposed();


            // すでに計算済みなら
            if (finish)
            {
                // ハッシュデータのコピーだけを行う
                result = (byte[])hash.Hash.Clone();
                return;
            }


            // ハッシュ計算を最後の処理をしてコピーを行う
            hash.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
            result = (byte[])hash.Hash.Clone();
            finish = true;
        }
        #endregion


        #region IO関数
        /// <summary>
        /// 参照ストリームの Write をそのまま呼び出します
        /// </summary>
        /// <param name="buffer">ストリームに書き込むデータを保持したバッファ</param>
        /// <param name="offset">ストリームに書き込むバッファの開始位置</param>
        /// <param name="count">ストリームに書き込むデータの数</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            // ハッシュ計算に流し込んで本来の関数を呼ぶ
            ThrowExceptionIfObjectDisposed();
            hash.TransformBlock(buffer, offset, count, null, 0);
            BaseStream.Write(buffer, offset, count);
        }


        /// <summary>
        /// 参照ストリームの Read をそのまま呼び出します
        /// </summary>
        /// <param name="buffer">ストリームから読みだした結果を受け取るバッファ</param>
        /// <param name="offset">バッファが受け取る開始位置</param>
        /// <param name="count">受け取るデータの数</param>
        /// <returns>参照ストリームの結果をそのまま返します</returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            // 本来の関数を呼んでもし読み取りサイズが有効なら
            ThrowExceptionIfObjectDisposed();
            var readSize = BaseStream.Read(buffer, offset, count);
            if (readSize > 0)
            {
                // ハッシュ計算に流し込む
                hash.TransformBlock(buffer, offset, readSize, null, 0);
            }


            // 読み込んだ結果を返す
            return readSize;
        }


        /// <summary>
        /// 参照ストリームの Seek をそのまま呼び出します
        /// </summary>
        /// <param name="offset">origin を基準にしたシークする長さのオフセット値</param>
        /// <param name="origin">シークする基準</param>
        /// <returns>参照ストリームの結果をそのまま返します</returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            // そのまま受け取ってそのまま返す
            ThrowExceptionIfObjectDisposed();
            return BaseStream.Seek(offset, origin);
        }


        /// <summary>
        /// 参照ストリームの SetLength をそのまま呼び出します
        /// </summary>
        /// <param name="value">ストリームに設定する新しい長さ</param>
        public override void SetLength(long value)
        {
            // そのまま受け取る
            ThrowExceptionIfObjectDisposed();
            BaseStream.SetLength(value);
        }


        /// <summary>
        /// 参照ストリームの Flush を呼び出します
        /// </summary>
        public override void Flush()
        {
            // そのまま呼び出す
            ThrowExceptionIfObjectDisposed();
            BaseStream.Flush();
        }
        #endregion


        #region 例外スロー関数
        /// <summary>
        /// もし、このクラスのインスタンスが破棄済みなら例外をスローします
        /// </summary>
        /// <exception cref="ObjectDisposedException">インスタンスが破棄済みです</exception>
        private void ThrowExceptionIfObjectDisposed()
        {
            // 既に破棄済みなら
            if (disposed)
            {
                // 破棄済み例外を吐く
                throw new ObjectDisposedException(null);
            }
        }
        #endregion
    }
    #endregion



    #region
    /// <summary>
    /// ストリームの入出力をそのままCRC64計算機に流し込むストリームクラスです
    /// </summary>
    /// <typeparam name="TCrc">使用するCRC64</typeparam>
    public class Crc64Stream<TCrc> : Stream where TCrc : Crc64Base
    {
        // メンバ変数定義
        private readonly TCrc crc;
        private readonly bool leaveOpen;
        private bool disposed;
        private ulong hash;



        #region プロパティ
        /// <summary>
        /// 参照ストリームの CanRead をそのまま返します
        /// </summary>
        public override bool CanRead => BaseStream.CanRead;


        /// <summary>
        /// 参照ストリームの CanSeek をそのまま返します
        /// </summary>
        public override bool CanSeek => BaseStream.CanSeek;


        /// <summary>
        /// 参照ストリームの CanWrite をそのまま返します
        /// </summary>
        public override bool CanWrite => BaseStream.CanWrite;


        /// <summary>
        /// 参照ストリームの Length をそのまま返します
        /// </summary>
        public override long Length => BaseStream.Length;


        /// <summary>
        /// 参照ストリームの Position にそのままアクセスします
        /// </summary>
        public override long Position { get => BaseStream.Position; set => BaseStream.Position = value; }


        /// <summary>
        /// このインスタンスが参照している基本ストリームを取得します
        /// </summary>
        public Stream BaseStream { get; private set; }
        #endregion



        #region 初期化＆解放
        /// <summary>
        /// Crc64Stream クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="baseStream">CRC計算を行う対象となるストリーム</param>
        /// <param name="crc">使用するCRCアルゴリズム</param>
        /// <exception cref="ArgumentNullException">baseStream が null です</exception>
        /// <exception cref="ArgumentNullException">crc が null です</exception>
        public Crc64Stream(Stream baseStream, TCrc crc) : this(baseStream, crc, false)
        {
        }


        /// <summary>
        /// Crc64Stream クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="baseStream">CRC計算を行う対象となるストリーム</param>
        /// <param name="crc">使用するCRCアルゴリズム</param>
        /// <param name="leaveOpen">このインスタンスが破棄される時に baseStream を継続して開き続ける場合は true を、破棄する場合は false を指定。既定は false です。</param>
        /// <exception cref="ArgumentNullException">baseStream が null です</exception>
        /// <exception cref="ArgumentNullException">crc が null です</exception>
        public Crc64Stream(Stream baseStream, TCrc crc, bool leaveOpen)
        {
            // 初期化をする
            BaseStream = baseStream ?? throw new ArgumentNullException(nameof(baseStream));
            this.crc = crc ?? throw new ArgumentNullException(nameof(crc));
            this.leaveOpen = leaveOpen;
            hash = ulong.MaxValue;
        }


        /// <summary>
        /// 現在使用しているリソースを破棄します
        /// </summary>
        /// <param name="disposing">マネージオブジェクトの破棄の場合は true それ以外は false</param>
        protected override void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }


            if (disposing && !leaveOpen)
            {
                BaseStream.Dispose();
            }


            disposed = true;
            base.Dispose(disposing);
        }
        #endregion


        #region CRC操作関数
        /// <summary>
        /// 現在のCRCハッシュ値を取得します
        /// </summary>
        /// <returns>現在のCRCハッシュ値を返します</returns>
        public ulong GetHashData()
        {
            // 新しい計算済みハッシュコードの受け取り配列を生成する
            ThrowExceptionIfObjectDisposed();
            return hash ^ ulong.MaxValue;
        }
        #endregion


        #region IO関数
        /// <summary>
        /// 参照ストリームの Write をそのまま呼び出します
        /// </summary>
        /// <param name="buffer">ストリームに書き込むデータを保持したバッファ</param>
        /// <param name="offset">ストリームに書き込むバッファの開始位置</param>
        /// <param name="count">ストリームに書き込むデータの数</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            // ハッシュ計算に流し込んで本来の関数を呼ぶ
            ThrowExceptionIfObjectDisposed();
            hash = crc.Calculate(hash, buffer, offset, count);
            BaseStream.Write(buffer, offset, count);
        }


        /// <summary>
        /// 参照ストリームの Read をそのまま呼び出します
        /// </summary>
        /// <param name="buffer">ストリームから読みだした結果を受け取るバッファ</param>
        /// <param name="offset">バッファが受け取る開始位置</param>
        /// <param name="count">受け取るデータの数</param>
        /// <returns>参照ストリームの結果をそのまま返します</returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            // 本来の関数を呼んでもし読み取りサイズが有効なら
            ThrowExceptionIfObjectDisposed();
            var readSize = BaseStream.Read(buffer, offset, count);
            if (readSize > 0)
            {
                // ハッシュ計算に流し込む
                hash = crc.Calculate(hash, buffer, offset, readSize);
            }


            // 読み込んだ結果を返す
            return readSize;
        }


        /// <summary>
        /// 参照ストリームの Seek をそのまま呼び出します
        /// </summary>
        /// <param name="offset">origin を基準にしたシークする長さのオフセット値</param>
        /// <param name="origin">シークする基準</param>
        /// <returns>参照ストリームの結果をそのまま返します</returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            // そのまま受け取ってそのまま返す
            ThrowExceptionIfObjectDisposed();
            return BaseStream.Seek(offset, origin);
        }


        /// <summary>
        /// 参照ストリームの SetLength をそのまま呼び出します
        /// </summary>
        /// <param name="value">ストリームに設定する新しい長さ</param>
        public override void SetLength(long value)
        {
            // そのまま受け取る
            ThrowExceptionIfObjectDisposed();
            BaseStream.SetLength(value);
        }


        /// <summary>
        /// 参照ストリームの Flush を呼び出します
        /// </summary>
        public override void Flush()
        {
            // そのまま呼び出す
            ThrowExceptionIfObjectDisposed();
            BaseStream.Flush();
        }
        #endregion


        #region 例外スロー関数
        /// <summary>
        /// もし、このクラスのインスタンスが破棄済みなら例外をスローします
        /// </summary>
        /// <exception cref="ObjectDisposedException">インスタンスが破棄済みです</exception>
        private void ThrowExceptionIfObjectDisposed()
        {
            // 既に破棄済みなら
            if (disposed)
            {
                // 破棄済み例外を吐く
                throw new ObjectDisposedException(null);
            }
        }
        #endregion
    }
    #endregion



    #region 監視可能なストリーム
    /// <summary>
    /// 指定したストリームの監視を可能にするストリームクラスです
    /// </summary>
    public class MonitorableStream : Stream
    {
        // 定数定義
        public const int DefaultMeasureUpdateIntervalTime = 1000;

        // メンバ変数定義
        private Stopwatch readStopwatch;
        private Stopwatch writeStopwatch;
        private long previousTotalReadSize;
        private long previousTotalWriteSize;
        private bool leaveOpen;
        private bool disposed;



        #region プロパティ
        /// <summary>
        /// 参照ストリームの CanRead をそのまま返します
        /// </summary>
        public override bool CanRead => BaseStream.CanRead;


        /// <summary>
        /// 参照ストリームの CanSeek をそのまま返します
        /// </summary>
        public override bool CanSeek => BaseStream.CanSeek;


        /// <summary>
        /// 参照ストリームの CanWrite をそのまま返します
        /// </summary>
        public override bool CanWrite => BaseStream.CanWrite;


        /// <summary>
        /// 参照ストリームの Length をそのまま返します
        /// </summary>
        public override long Length => BaseStream.Length;


        /// <summary>
        /// 参照ストリームの Position にそのままアクセスします
        /// </summary>
        public override long Position { get => BaseStream.Position; set => BaseStream.Position = value; }


        /// <summary>
        /// このインスタンスが参照している基本ストリームを取得します
        /// </summary>
        public Stream BaseStream { get; private set; }


        /// <summary>
        /// 参照ストリームによる合計の読み込みサイズを取得します
        /// </summary>
        public long TotalReadSize { get; private set; }


        /// <summary>
        /// 参照ストリームによる合計の書き込みサイズを取得します
        /// </summary>
        public long TotalWriteSize { get; private set; }


        /// <summary>
        /// 参照ストリームによる前回の計測合計読み込みサイズの変化量を取得します
        /// </summary>
        public long DeltaReadSize { get; private set; }


        /// <summary>
        /// 参照ストリームによる前回の計測合計書き込みサイズの変化量を取得します
        /// </summary>
        public long DeltaWriteSize { get; private set; }


        /// <summary>
        /// 参照ストリームによる読み込みビットレート（bps）を取得します
        /// </summary>
        public long ReadBitRate { get; private set; }


        /// <summary>
        /// 参照ストリームによる書き込みビットレート（bps）を取得します
        /// </summary>
        public long WriteBitRate { get; private set; }


        /// <summary>
        /// 計測更新間隔時間をミリ秒で指定できます。（更新間隔が短いほど誤差が広くなる傾向があります）
        /// </summary>
        public int MeasureUpdateIntervalTime { get; set; }
        #endregion



        #region 初期化＆破棄
        /// <summary>
        /// MonitorableStream クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="baseStream">監視対象となる Stream の参照</param>
        /// <exception cref="ArgumentNullException">baseStream が null です</exception>
        public MonitorableStream(Stream baseStream) : this(baseStream, DefaultMeasureUpdateIntervalTime, false)
        {
        }


        /// <summary>
        /// MonitorableStream クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="baseStream">監視対象となる Stream の参照</param>
        /// <param name="measureUpdateIntervalTime">計測更新間隔時間をミリ秒で指定。既定は DefaultMeasureUpdateIntervalTime です。</param>
        /// <exception cref="ArgumentNullException">baseStream が null です</exception>
        public MonitorableStream(Stream baseStream, int measureUpdateIntervalTime) : this(baseStream, measureUpdateIntervalTime, false)
        {
        }


        /// <summary>
        /// MonitorableStream クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="baseStream">監視対象となる Stream の参照</param>
        /// <param name="measureUpdateIntervalTime">計測更新間隔時間をミリ秒で指定。既定は DefaultMeasureUpdateIntervalTime です。</param>
        /// <param name="leaveOpen">このインスタンスが破棄される時に baseStream を継続して開き続ける場合は true を、破棄する場合は false を指定。既定は false です。</param>
        /// <exception cref="ArgumentNullException">baseStream が null です</exception>
        public MonitorableStream(Stream baseStream, int measureUpdateIntervalTime, bool leaveOpen)
        {
            // 計測用ストップウォッチのインスタンスを作っておく
            readStopwatch = Stopwatch.StartNew();
            writeStopwatch = Stopwatch.StartNew();


            // 受け取ったパラメータをそのまま覚える
            BaseStream = baseStream ?? throw new ArgumentNullException(nameof(baseStream));
            this.leaveOpen = leaveOpen;


            // 計測更新間隔の既定値を設定
            MeasureUpdateIntervalTime = measureUpdateIntervalTime;
        }


        /// <summary>
        /// 現在使用しているリソースを破棄します
        /// </summary>
        /// <param name="disposing">マネージオブジェクトの破棄の場合は true それ以外は false</param>
        protected override void Dispose(bool disposing)
        {
            // すでに破棄済みなら
            if (disposed)
            {
                // 何もせず終了
                return;
            }


            // マネージの破棄なら
            if (disposing)
            {
                // もしこのインスタンスが破棄される時に参照ストリームを破棄するなら
                if (!leaveOpen)
                {
                    // 参照ストリームのDisposeも呼ぶ
                    BaseStream.Dispose();
                }
            }


            // 破棄済みをマーク
            disposed = true;


            // 基本クラスのDisposeも呼ぶ
            base.Dispose(disposing);
        }
        #endregion


        #region IO関数
        /// <summary>
        /// 参照ストリームの Write をそのまま呼び出します
        /// </summary>
        /// <param name="buffer">ストリームに書き込むデータを保持したバッファ</param>
        /// <param name="offset">ストリームに書き込むバッファの開始位置</param>
        /// <param name="count">ストリームに書き込むデータの数</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            // もし初めての書き込みなら
            if (TotalWriteSize == 0)
            {
                // 書き込み計測ストップウォッチを再起動する
                writeStopwatch.Restart();
            }


            // そのまま受け取って計測する
            ThrowExceptionIfObjectDisposed();
            BaseStream.Write(buffer, offset, count);
            MeasureWriteInformation(count);
        }


        /// <summary>
        /// 参照ストリームの Read をそのまま呼び出します
        /// </summary>
        /// <param name="buffer">ストリームから読みだした結果を受け取るバッファ</param>
        /// <param name="offset">バッファが受け取る開始位置</param>
        /// <param name="count">受け取るデータの数</param>
        /// <returns>参照ストリームの結果をそのまま返します</returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            // もし初めての読み込みなら
            if (TotalReadSize == 0)
            {
                // 読み込み計測ストップウォッチを再起動する
                readStopwatch.Restart();
            }


            // そのまま受け取って計測してからそのまま返す
            ThrowExceptionIfObjectDisposed();
            var readSize = BaseStream.Read(buffer, offset, count);
            MeasureReadInformation(readSize);
            return readSize;
        }


        /// <summary>
        /// 参照ストリームの Seek をそのまま呼び出します
        /// </summary>
        /// <param name="offset">origin を基準にしたシークする長さのオフセット値</param>
        /// <param name="origin">シークする基準</param>
        /// <returns>参照ストリームの結果をそのまま返します</returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            // そのまま受け取ってそのまま返す
            ThrowExceptionIfObjectDisposed();
            return BaseStream.Seek(offset, origin);
        }


        /// <summary>
        /// 参照ストリームの SetLength をそのまま呼び出します
        /// </summary>
        /// <param name="value">ストリームに設定する新しい長さ</param>
        public override void SetLength(long value)
        {
            // そのまま受け取る
            ThrowExceptionIfObjectDisposed();
            BaseStream.SetLength(value);
        }


        /// <summary>
        /// 参照ストリームの Flush を呼び出します
        /// </summary>
        public override void Flush()
        {
            // そのまま呼び出す
            ThrowExceptionIfObjectDisposed();
            BaseStream.Flush();
        }
        #endregion


        #region 計測関数
        /// <summary>
        /// 参照ストリームの読み込み情報を計測します
        /// </summary>
        /// <param name="readSize">ストリームが実際に読み込んだデータサイズ</param>
        private void MeasureReadInformation(int readSize)
        {
            // 初めての計測なのかを求めて合計読み込みサイズに加算
            var firstMeasure = TotalReadSize == 0;
            TotalReadSize += readSize;


            // もし初めての計測または、計測更新時間間隔超過なら
            var elapsed = readStopwatch.ElapsedMilliseconds;
            if (firstMeasure || elapsed >= MeasureUpdateIntervalTime)
            {
                // 読み込みサイズの、差分、前回の合計値、ビットレートを計算する
                DeltaReadSize = TotalReadSize - previousTotalReadSize;
                previousTotalReadSize = TotalReadSize;
                ReadBitRate = (long)(DeltaReadSize * (1.0 / (elapsed / 1000.0)) * 8.0);


                // 読み込み計測更新用ストップウォッチを再起動
                readStopwatch.Restart();
            }
        }


        /// <summary>
        /// 参照ストリームの書き込み情報を計測します
        /// </summary>
        /// <param name="writeSize">ストリームが実際に書き込んだデータサイズ</param>
        private void MeasureWriteInformation(int writeSize)
        {
            // 初めての計測なのかを求めて合計書き込みサイズに加算
            var firstMeasure = TotalWriteSize == 0;
            TotalWriteSize += writeSize;


            // もし初めての計測または、計測更新時間間隔超過なら
            var elapsed = writeStopwatch.ElapsedMilliseconds;
            if (firstMeasure || elapsed >= MeasureUpdateIntervalTime)
            {
                // 書き込みサイズの、差分、前回の合計値、ビットレートを計算する
                DeltaWriteSize = TotalWriteSize - previousTotalWriteSize;
                previousTotalWriteSize = TotalWriteSize;
                WriteBitRate = (long)(DeltaWriteSize * (1.0 / (elapsed / 1000.0)) * 8.0);


                // 書き込み計測更新用ストップウォッチを再起動
                writeStopwatch.Restart();
            }
        }
        #endregion


        #region 例外スロー関数
        /// <summary>
        /// もし、このクラスのインスタンスが破棄済みなら例外をスローします
        /// </summary>
        /// <exception cref="ObjectDisposedException">インスタンスが破棄済みです</exception>
        private void ThrowExceptionIfObjectDisposed()
        {
            // 既に破棄済みなら
            if (disposed)
            {
                // 破棄済み例外を吐く
                throw new ObjectDisposedException(null);
            }
        }
        #endregion
    }
    #endregion
}