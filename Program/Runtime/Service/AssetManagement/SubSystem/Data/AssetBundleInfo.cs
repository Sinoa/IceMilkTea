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

namespace IceMilkTea.Service
{
    /// <summary>
    /// アセットバンドルの情報を保持した構造体です
    /// </summary>
    public struct AssetBundleInfo
    {
        /// <summary>
        /// アセットバンドル名
        /// </summary>
        public string Name;


        /// <summary>
        /// アセットバンドル名を含まない、アセットバンドルへアクセスするためのローカルパス
        /// </summary>
        public string LocalPath;


        /// <summary>
        /// アセットバンドル名を含まない、アセットバンドルへアクセスするためのリモートパス
        /// </summary>
        public string RemotePath;


        /// <summary>
        /// アセットバンドルのデータサイズ
        /// </summary>
        public long Size;


        /// <summary>
        /// アセットバンドルのハッシュ値
        /// </summary>
        public byte[] Hash;


        /// <summary>
        /// このアセットバンドル情報に付ける追加のユーザーデータ。
        /// この値は、アプリケーション側で自由に利用することが出来ます。
        /// </summary>
        public ulong UserData;
    }
}