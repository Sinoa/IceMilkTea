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
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace IceMilkTea.Module
{
    /// <summary>
    /// ファイルシステムを用いたアセットフェッチャクラスです
    /// </summary>
    public class FileAssetFetcher : IAssetFetcher
    {
        // メンバ変数定義
        private FileInfo assetFileInfo;



        /// <summary>
        /// フェッチの進捗を割合で取得します
        /// </summary>
        public double Progress { get; private set; }



        /// <summary>
        /// FileAssetFetcher クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="assetFileInfo">コピー元となるファイルアセット情報</param>
        /// <exception cref="ArgumentNullException">assetFileInfo が null です</exception>
        public FileAssetFetcher(FileInfo assetFileInfo)
        {
            // ファイル情報を受け取る
            this.assetFileInfo = assetFileInfo ?? throw new ArgumentNullException(nameof(assetFileInfo));
        }


        /// <summary>
        /// アセットのフェッチを非同期で行い対象のストリームに出力します
        /// </summary>
        /// <param name="outStream">出力先のストリーム</param>
        /// <returns>フェッチ処理を実行しているタスクを返します</returns>
        /// <exception cref="OperationCanceledException">非同期の操作がキャンセルされました</exception>
        /// <exception cref="ArgumentNullException">outStream が null です</exception>
        public Task FetchAsync(Stream outStream)
        {
            // キャンセルはしない同じ関数を叩く
            return FetchAsync(outStream, CancellationToken.None);
        }


        /// <summary>
        /// アセットのフェッチを非同期で行い対象のストリームに出力します
        /// </summary>
        /// <param name="outStream">出力先のストリーム</param>
        /// <param name="cancellationToken">キャンセル要求を監視するためのトークン。既定は None です。</param>
        /// <returns>フェッチ処理を実行しているタスクを返します</returns>
        /// <exception cref="OperationCanceledException">非同期の操作がキャンセルされました</exception>
        /// <exception cref="ArgumentNullException">outStream が null です</exception>
        /// <exception cref="FileNotFoundException">コピー元となるアセットファイル '{assetFilePath}' が見つかりません</exception>
        public async Task FetchAsync(Stream outStream, CancellationToken cancellationToken)
        {
            // 進捗率をリセット
            Progress = 0.0;


            // この時点でのキャンセルリクエストを判定してさらに出力先ストリームが無いなら
            cancellationToken.ThrowIfCancellationRequested();
            if (outStream == null)
            {
                // 出力先ストリームが無いとどうすればよいのか
                throw new ArgumentNullException(nameof(outStream));
            }


            // コピー元のアセットファイルのフルパスを取得する
            var assetFilePath = assetFileInfo.FullName;


            // コピー元となるアセットファイルが存在しないなら
            assetFileInfo.Refresh();
            if (!assetFileInfo.Exists)
            {
                // 例外を吐く
                throw new FileNotFoundException($"コピー元となるアセットファイル '{assetFilePath}' が見つかりません", assetFilePath);
            }


            // ファイルを開く(キャッシュサイズが16KBなのはiOSに合わせているだけです)
            using (var fileStream = new FileStream(assetFilePath, FileMode.Open, FileAccess.Read, FileShare.None, 16 << 10, true))
            {
                // 必要な情報を用意
                int readSize = 0;
                int totalReadSize = 0;
                long contentLength = fileStream.Length;
                var buffer = new byte[1 << 20];


                // 読み切るまでループ
                while ((readSize = await fileStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                {
                    // 出力先ストリームに書き込んで合計読み込みサイズに加算
                    await outStream.WriteAsync(buffer, 0, readSize);
                    totalReadSize += readSize;


                    // コピー進捗を更新する
                    Progress = totalReadSize / (double)contentLength;
                }


                // 最後は無条件に1.0を進捗率に設定
                Progress = 1.0;
            }
        }
    }
}