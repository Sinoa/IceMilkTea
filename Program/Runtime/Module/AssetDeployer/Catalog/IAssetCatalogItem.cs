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

namespace IceMilkTea.SubSystem
{
    /// <summary>
    /// カタログに含まれるアセットの情報を表現するインターフェイスです
    /// </summary>
    public interface IAssetCatalogItem
    {
        /// <summary>
        /// アセット名
        /// </summary>
        string Name { get; }


        /// <summary>
        /// フェッチする参照先アセットURI
        /// </summary>
        Uri RemoteAssetUri { get; }


        /// <summary>
        /// ストレージからアセットをアクセスするためのアセットURI
        /// </summary>
        Uri LocalAssetUri { get; }


        /// <summary>
        /// このアセットが貯蔵されるべきストレージの名前
        /// </summary>
        string StorageName { get; }


        /// <summary>
        /// このアセットのハッシュデータ
        /// </summary>
        byte[] HashData { get; }


        /// <summary>
        /// ハッシュ検証を行うセキュリティ名
        /// </summary>
        string SecurityName { get; }
    }
}