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
using System.Threading.Tasks;

namespace IceMilkTea.Module
{
    /// <summary>
    /// アセットの実際のデプロイを行う抽象クラスです
    /// </summary>
    public abstract class AssetDeployDriver : IAssetDeployDriver
    {
        /// <summary>
        /// AssetDeployDriver クラスのインスタンスの解放をします
        /// </summary>
        ~AssetDeployDriver()
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
        /// アセットデプロイを行う前にドライバの動作準備を非同期で行います
        /// </summary>
        /// <param name="deployer">このドライバを起動するデプロイヤ</param>
        /// <returns>デプロイ先に出力するためのストリーム動作準備を行っているタスクを返します</returns>
        public abstract Task<Stream> PrepareAsync(AssetDeployer deployer);
    }
}