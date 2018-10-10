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
using System.IO;
using System.Threading.Tasks;

namespace IceMilkTea.Service
{
    /// <summary>
    /// アセットバンドルをゲーム内で扱えるようにインストールするインストーラ抽象クラスです
    /// </summary>
    public abstract class AssetBundleInstaller
    {
        /// <summary>
        /// インストーラ名を取得します
        /// </summary>
        public abstract string InstallerName { get; }



        /// <summary>
        /// 指定されたインストールストリームに対して、アセットバンドル情報のアセットバンドルを非同期でインストールします
        /// </summary>
        /// <param name="info">インストールするアセットバンドルの情報</param>
        /// <param name="installStream">インストールする先のストリーム</param>
        /// <param name="progress">インストール進捗通知を受ける IProgress</param>
        /// <returns>アセットバンドルのインストールを非同期で操作しているタスクを返します</returns>
        public abstract Task InstallAsync(AssetBundleInfo info, Stream installStream, IProgress<double> progress);
    }
}