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
using System.Threading.Tasks;

namespace IceMilkTea.SubSystem
{
    #region インターフェイス
    /// <summary>
    /// 一覧を表現するインターフェイスです
    /// </summary>
    public interface ICatalog
    {
        /// <summary>
        /// 指定した名前のカタログアイテムを取得します
        /// </summary>
        /// <param name="name">取得するアイテム名</param>
        /// <returns>指定された名前からカタログアイテムを取得された場合はインスタンスを返しますが、見つからない場合は null を返します</returns>
        ICatalogItem GetItem(string name);


        /// <summary>
        /// カタログに含まれている全てのカタログアイテムを取得して列挙可能なオブジェクトを取得します
        /// </summary>
        /// <returns>全てのカタログアイテムを列挙可能なオブジェクトを返します</returns>
        IEnumerable<ICatalogItem> GetItemAll();
    }



    /// <summary>
    /// カタログに含まれるアイテムの情報を表現するインターフェイスです
    /// </summary>
    public interface ICatalogItem
    {
        /// <summary>
        /// アイテム名
        /// </summary>
        string Name { get; }


        /// <summary>
        /// フェッチする参照先アイテムURI
        /// </summary>
        Uri RemoteAssetUri { get; }


        /// <summary>
        /// ストレージからアイテムをアクセスするためのアイテムURI
        /// </summary>
        Uri LocalAssetUri { get; }


        /// <summary>
        /// このアイテムのハッシュデータ
        /// </summary>
        byte[] HashData { get; }
    }



    /// <summary>
    /// ストリームからカタログを読み込むインターフェイスです
    /// </summary>
    public interface ICatalogReader
    {
        /// <summary>
        /// 指定されたストリームからカタログを非同期で読み込みます
        /// </summary>
        /// <param name="stream">カタログのデータを読み込むストリーム</param>
        /// <returns>正常に読み込まれた場合はカタログのインスタンスを返しますが、読み込まれなかった場合は null を返します</returns>
        Task<ICatalog> ReadCatalogAsync(Stream stream);
    }
    #endregion
}