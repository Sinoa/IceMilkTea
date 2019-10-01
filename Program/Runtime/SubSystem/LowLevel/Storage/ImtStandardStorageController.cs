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
    /// IceMilkTea が提供する標準ストレージコントローラクラスです
    /// </summary>
    public class ImtStandardStorageController : ImtStorageController
    {
        // メンバ変数定義
        private DirectoryInfo baseDirectoryInfo;
        private DirectoryInfo assetBundleDirectoryInfo;



        /// <summary>
        /// ストレージコントローラ名
        /// </summary>
        public override string Name => "imt";



        public ImtStandardStorageController(DirectoryInfo baseDirectoryInfo)
        {
        }


        /// <summary>
        /// 指定されたURIのデータが存在するか否かを確認します
        /// </summary>
        /// <param name="uri">存在を確認するURI</param>
        /// <returns>データが存在する場合は true を、存在しない場合は false を返します</returns>
        /// <exception cref="ArgumentNullException">uri が null です</exception>
        /// <exception cref="ArgumentException">ホスト名が、空白文字列または、空文字列または null です</exception>
        /// <exception cref="ArgumentException">ホスト名とストレージコントローラ名が一致しません。StorageName='{Name}' HostName='{hostName}'</exception>
        public override bool Exists(Uri uri)
        {
            // 例外処理をする
            ThrowExceptionIfNullUriOrInvalidHostName(uri);
            throw new NotImplementedException();
        }


        public override Stream Open(Uri uri, FileMode mode, FileAccess access, FileShare share, int bufferSize, bool useAsync)
        {
            // 例外処理をする
            ThrowExceptionIfNullUriOrInvalidHostName(uri);
            throw new NotImplementedException();
        }


        public override void Delete(Uri uri)
        {
            // 例外処理をする
            ThrowExceptionIfNullUriOrInvalidHostName(uri);
            throw new NotImplementedException();
        }


        public override void DeleteAll()
        {
            throw new NotImplementedException();
        }



        /// <summary>
        /// 実際のIOコントロールを行う基本抽象クラスです
        /// </summary>
        private abstract class IOControlBase
        {
            /// <summary>
            /// 指定されたURIのデータが存在するか否かを確認します
            /// </summary>
            /// <param name="uri">存在を確認するURI</param>
            /// <returns>データが存在する場合は true を、存在しない場合は false を返します</returns>
            public abstract bool Exists(Uri uri);


            /// <summary>
            /// 指定されたURIのストリームを開きます
            /// </summary>
            /// <param name="uri">開く対象となるURI</param>
            /// <param name="mode">ストリームを開くモード</param>
            /// <param name="access">ストリームへのアクセス方法</param>
            /// <param name="share">ストリームの共有設定</param>
            /// <param name="bufferSize">ストリームが持つバッファサイズ</param>
            /// <param name="useAsync">ストリームの非同期操作を使用するか否か</param>
            /// <returns>正しくストリームを開けた場合はストリームを返しますが、開けなかった場合は null を返します</returns>
            public abstract Stream Open(Uri uri, FileMode mode, FileAccess access, FileShare share, int bufferSize, bool useAsync);


            /// <summary>
            /// 指定されたURIのデータを削除します
            /// </summary>
            /// <param name="uri">削除するURI</param>
            public abstract void Delete(Uri uri);
        }



        private class AssetBundleIOControl : IOControlBase
        {
        }
    }
}