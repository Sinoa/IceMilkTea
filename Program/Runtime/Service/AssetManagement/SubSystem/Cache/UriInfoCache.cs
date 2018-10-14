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
using System.Collections.Generic;

namespace IceMilkTea.Service
{
    /// <summary>
    /// UriInfo インスタンスの生成コストを回避するために、極力キャッシュから取り出せるようにするためのキャッシュクラスです。
    /// </summary>
    internal class UriInfoCache
    {
        // 定数定義
        private const int DefaultCapacity = 2 << 10;

        // メンバ変数定義
        private Dictionary<string, UriInfo> uriCacheTable;



        /// <summary>
        /// UriCache のインスタンスを初期化します
        /// </summary>
        public UriInfoCache()
        {
            // キャッシュ用テーブルを生成
            uriCacheTable = new Dictionary<string, UriInfo>(DefaultCapacity);
        }


        /// <summary>
        /// 指定されたURI文字列から、生成経験のある UriInfo インスタンスが存在すれば取得し、
        /// まだ生成経験がない場合は、新しく UriInfo インスタンスを生成します。
        /// </summary>
        /// <param name="uriText">UriInfo のインスタンスを取得または生成するURI文字列</param>
        /// <returns>取得または生成した UriInfo インスタンスを返します</returns>
        /// <exception cref="ArgumentNullException">uriText が null です</exception>
        /// <exception cref="ArgumentException">指定されたURIは有効な書式ではありません</exception>
        public UriInfo GetOrCreateUri(string uriText)
        {
            // もし null を渡されたら
            if (uriText == null)
            {
                // nullは取り扱えない
                throw new ArgumentNullException(nameof(uriText));
            }


            // キャッシュテーブルから生成済み UriInfo インスタンスの取得を試みて、取得できたのなら
            UriInfo uriInfo;
            if (uriCacheTable.TryGetValue(uriText, out uriInfo))
            {
                // 取得できたインスタンスを返す
                return uriInfo;
            }


            // 新しく Uri インスタンスの生成を試みるが出来ないなら
            Uri uri;
            if (!Uri.TryCreate(uriText, UriKind.Absolute, out uri))
            {
                // 正しいURIである必要がある例外を吐く
                throw new ArgumentException($"指定されたURIは有効な書式ではありません urlText='{uriText}'");
            }


            // 生成されたインスタンスを覚えて返す
            return uriCacheTable[uriText] = new UriInfo(uri);
        }
    }
}