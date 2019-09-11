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

using System.Collections.Generic;
using System.IO;

namespace IceMilkTea.Module
{
    /// <summary>
    /// アセットカタログの貯蔵をするインターフェイスです
    /// </summary>
    public interface IAssetCatalogStorage
    {
        /// <summary>
        /// このストレージが管理している全てのカタログ名を取得します
        /// </summary>
        /// <returns>取得されたカタログ名を列挙できる IEnumerable のインスタンスを返します</returns>
        IEnumerable<string> GetCatalogs();


        /// <summary>
        /// 指定した名前のカタログがあるか否かを調べます
        /// </summary>
        /// <param name="name">存在を確認するカタログ名</param>
        /// <returns>指定された名前のカタログがある場合は true を、ない場合は false を返します</returns>
        bool Exists(string name);


        /// <summary>
        /// 指定された名前のカタログを読み込むためのストリームを開きます
        /// </summary>
        /// <param name="name">読み込むカタログ名</param>
        /// <returns>指定されたカタログ名のストリームを開けた場合はインスタンスを返しますが、開けなかった場合は null を返します</returns>
        Stream OpenRead(string name);


        /// <summary>
        /// 指定された名前のカタログを書き込むためのストリームを開きます
        /// </summary>
        /// <param name="name">書き込むカタログ名</param>
        /// <returns>指定されたカタログ名のストリームを開けた場合はインスタンスを返しますが、開けなかった場合は null を返します</returns>
        Stream OpenWrite(string name);


        /// <summary>
        /// 指定された名前のカタログを削除します
        /// </summary>
        /// <param name="name">削除するカタログ</param>
        void Delete(string name);


        /// <summary>
        /// このストレージが管理している全てのカタログを削除します
        /// </summary>
        void DeleteAll();
    }
}