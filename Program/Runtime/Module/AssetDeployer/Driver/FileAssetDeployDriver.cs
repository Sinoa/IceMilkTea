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
    /// ファイルシステムにアセットをデプロイするドライバクラスです
    /// </summary>
    public class FileAssetDeployDriver : AssetDeployDriver
    {
        // メンバ変数定義
        private FileInfo fileInfo;



        /// <summary>
        /// FileAssetDeployDriver クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="path">デプロイ先のファイルパス</param>
        /// <exception cref="ArgumentNullException">path が null です</exception>
        public FileAssetDeployDriver(string path)
        {
            // 情報クラスとしてインスタンスを生成する
            fileInfo = new FileInfo(path);
        }


        /// <summary>
        /// アセットをデプロイするすためのストリームを非同期で開きます
        /// </summary>
        /// <param name="cancellationToken">キャンセル要求を監視するためのトークン。既定は None です。</param>
        /// <returns>デプロイ先に出力するためのストリーム動作準備を行っているタスクを返します</returns>
        public override Task<Stream> OpenAsync(CancellationToken cancellationToken)
        {
            // 非同期IOになるべく最適なファイルを書き込みとして開いてすぐに返す（バッファサイズが16KiBなのはiOS向けに雑に大きい方に合わせただけです）
            var fileStream = new FileStream(fileInfo.FullName, FileMode.Create, FileAccess.ReadWrite, FileShare.None, 16 << 10, true);
            return Task.FromResult((Stream)fileStream);
        }
    }
}