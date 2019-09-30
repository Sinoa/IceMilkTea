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

namespace IceMilkTea.SubSystem
{
    /// <summary>
    /// 実際のストリーム操作を提供するストレージコントローラインターフェイスです
    /// </summary>
    public interface IImtStorageController
    {
        /// <summary>
        /// ストレージ名を取得します
        /// </summary>
        string Name { get; }



        /// <summary>
        /// 指定されたURIのデータが存在するか否かを確認します
        /// </summary>
        /// <param name="uri">存在を確認するURI</param>
        /// <returns>データが存在する場合は true を、存在しない場合は false を返します</returns>
        bool Exists(Uri uri);


        /// <summary>
        /// 指定されたURIのストリームを開きます
        /// </summary>
        /// <param name="uri">開く対象となるURI</param>
        /// <param name="mode">ストリームを開くモード</param>
        /// <param name="access">ストリームへのアクセス方法</param>
        /// <param name="share">ストリームの共有設定</param>
        /// <param name="cacheSize">ストリームが持つキャッシュサイズ</param>
        /// <param name="useAsync">ストリームの非同期操作を使用するか否か</param>
        /// <returns>正しくストリームを開けた場合はストリームを返しますが、開けなかった場合は null を返します</returns>
        Stream Open(Uri uri, FileMode mode, FileAccess access, FileShare share, int cacheSize, bool useAsync);


        /// <summary>
        /// 指定されたURIのデータを削除します
        /// </summary>
        /// <param name="uri">削除するURI</param>
        void Delete(Uri uri);


        /// <summary>
        /// 管理しているデータすべてを削除します
        /// </summary>
        void DeleteAll();
    }
}