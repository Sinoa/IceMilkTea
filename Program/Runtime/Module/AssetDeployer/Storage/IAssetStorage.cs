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
using System.Collections.Generic;
using System.IO;

namespace IceMilkTea.Module
{
    /// <summary>
    /// アセットの貯蔵を行うインターフェイスです
    /// </summary>
    public interface IAssetStorage
    {
        /// <summary>
        /// アセットストレージ名
        /// </summary>
        string Name { get; }



        /// <summary>
        /// このストレージインスタンスが管理している全てのアセットURIを取得します
        /// </summary>
        /// <returns>取得されたURIの全てを列挙できる IEnumerable のインスタンスとして返します。1つもアセットがない場合は長さ0の IEnumerable を返します。</returns>
        IEnumerable<Uri> GetAssetUris();


        /// <summary>
        /// 指定したアセットURIのアセットが存在するかどうか確認します
        /// </summary>
        /// <param name="assetUri">確認するアセットURI</param>
        /// <returns>アセットが存在する場合は true を、存在しない場合は false を返します</returns>
        bool Exists(Uri assetUri);


        /// <summary>
        /// 指定したアセットURIのアセットを読み込みストリームとして開きます
        /// </summary>
        /// <param name="assetUri">ストリームとして開きたいアセットURI</param>
        /// <returns>ストリームとして開けた場合はストリームのインスタンスを、開けなかった場合は null を返します</returns>
        Stream OpenRead(Uri assetUri);


        /// <summary>
        /// 指定したアセットURIのアセットを書き込みストリームとして開きます
        /// </summary>
        /// <param name="assetUri">ストリームとして開きたいアセットURI</param>
        /// <returns>ストリームとして開けた場合はストリームのインスタンスを、開けなかった場合は null を返します</returns>
        Stream OpenWrite(Uri assetUri);


        /// <summary>
        /// 指定したアセットURIのアセットを削除します
        /// </summary>
        /// <param name="assetUri">削除するアセットURI</param>
        void Delete(Uri assetUri);


        /// <summary>
        /// このストレージインスタンスが管理している全てのアセットを削除します
        /// </summary>
        void DeleteAll();
    }
}