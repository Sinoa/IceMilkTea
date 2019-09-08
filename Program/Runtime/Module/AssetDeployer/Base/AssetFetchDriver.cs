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
    /// アセットの実際のフェッチを行う抽象クラスです
    /// </summary>
    public abstract class AssetFetchDriver : IAssetFetchDriver
    {
        /// <summary>
        /// AssetFetchDriver クラスのインスタンスの解放をします
        /// </summary>
        ~AssetFetchDriver()
        {
            // 本来のDisposeを呼び出す
            Dispose(false);
        }


        /// <summary>
        /// リソースの解放をします
        /// </summary>
        public void Dispose()
        {
            // 本来のDisposeを呼び出して自身のファイナライザを呼ばないように指示
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        /// <summary>
        /// 実際のリソースを解放します
        /// </summary>
        /// <param name="disposing">マネージおよびアンマネージの解放なら true を、アンマネージのみの場合は false</param>
        protected virtual void Dispose(bool disposing)
        {
            // このクラスでは解放すべき処理は無いため空実装となります
            // 継承先クラスにて解放が必要な場合に、正しいDisposeパターンの継承クラスを実装してください
        }


        /// <summary>
        /// アセットのフェッチを非同期で行い対象のストリームに出力します
        /// </summary>
        /// <param name="outStream">出力先のストリーム</param>
        /// <returns>フェッチ処理を実行しているタスクを返します</returns>
        public Task FetchAsync(Stream outStream)
        {
            // キャンセルトークンなしで呼び出す
            return FetchAsync(outStream, CancellationToken.None);
        }


        /// <summary>
        /// アセットのフェッチを非同期で行い対象のストリームに出力します
        /// </summary>
        /// <param name="outStream">出力先のストリーム</param>
        /// <param name="cancellationToken">キャンセル要求を監視するためのトークン。既定は None です。</param>
        /// <returns>フェッチ処理を実行しているタスクを返します</returns>
        public abstract Task FetchAsync(Stream outStream, CancellationToken cancellationToken);
    }
}