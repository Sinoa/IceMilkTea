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

namespace IceMilkTea.Module
{
    public class StandardAssetDriverFactory : IAssetDriverFactory
    {
        /// <summary>
        /// フェッチするアセットの情報からフェッチドライバを生成します
        /// </summary>
        /// <param name="info">フェッチドライバを生成するためのフェッチ情報</param>
        /// <returns>生成されたフェッチドライバを返します</returns>
        /// <exception cref="ArgumentNullException">info が null です</exception>
        public IAssetFetchDriver CreateFetchDriver(IAssetFetchInfo info)
        {
            throw new System.NotImplementedException();
        }


        /// <summary>
        /// デプロイするアセットの情報からデプロイドライバを生成します
        /// </summary>
        /// <param name="info">デプロイドライバを生成するためのデプロイ情報</param>
        /// <returns>生成されたデプロイドライバを返します</returns>
        /// <exception cref="ArgumentNullException">info が null です</exception>
        public IAssetDeployDriver CreateDeployDriver(IAssetDeployInfo info)
        {
            throw new System.NotImplementedException();
        }
    }
}