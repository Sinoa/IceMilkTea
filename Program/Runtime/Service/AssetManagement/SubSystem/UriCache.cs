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
using System.Collections.ObjectModel;
using IceMilkTea.Core;

namespace IceMilkTea.Service
{
    /// <summary>
    /// Uriとクエリの情報を持つクラスです
    /// </summary>
    internal class UriInfo
    {
        /// <summary>
        /// Uriインスタンスへの参照
        /// </summary>
        public Uri Uri { get; private set; }


        /// <summary>
        /// Uriインスタンスに含まれるクエリのテーブルアクセス
        /// </summary>
        public ReadOnlyDictionary<string, string> QueryTable { get; private set; }



        /// <summary>
        /// UriInfo インスタンスの初期化を行います
        /// </summary>
        /// <param name="uri">情報として保持するUriインスタンス</param>
        public UriInfo(Uri uri)
        {
            // Uriのインスタンスを覚えて、読み込み専用クエリテーブルのインスタンスを生成する
            Uri = uri;
            QueryTable = new ReadOnlyDictionary<string, string>(uri.GetQueryDictionary());
        }
    }



    /// <summary>
    /// Uriインスタンスの生成コストを回避するために、極力キャッシュから取り出せるようにするためのキャッシュクラスです。
    /// </summary>
    internal class UriCache
    {
        // 定数定義
        private const int DefaultCapacity = 2 << 10;

        // メンバ変数定義
        private Dictionary<string, UriInfo> uriCacheTable;



        /// <summary>
        /// UriCache のインスタンスを初期化します
        /// </summary>
        public UriCache()
        {
            // キャッシュ用テーブルを生成
            uriCacheTable = new Dictionary<string, UriInfo>(DefaultCapacity);
        }


        /// <summary>
        /// 指定されたURI文字列から、生成経験のあるUriインスタンスが存在すれば取得し、
        /// まだ生成経験がない場合は、新しくインスタンスを生成します。
        /// </summary>
        /// <param name="uriText">Uri のインスタンスを取得または生成するURI文字列</param>
        /// <returns>取得または生成した Uri インスタンスを含む UriInfo インスタンスを返します</returns>
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


            // キャッシュテーブルから生成済みUriインスタンスの取得を試みて、取得できたのなら
            UriInfo uriInfo;
            if (uriCacheTable.TryGetValue(uriText, out uriInfo))
            {
                // 取得できたインスタンスを返す
                return uriInfo;
            }


            // 新しくUriインスタンスの生成を試みるが出来ないなら
            Uri uri;
            if (!Uri.TryCreate(uriText, UriKind.Absolute, out uri))
            {
                // 正しいURIである必要がある例外を吐く
                throw new ArgumentException($"指定されたURIは有効な書式ではありません urlText='{uriText}'");
            }


            // 生成されたインスタンスを覚えて返す
            uriInfo = new UriInfo(uri);
            uriCacheTable[uriText] = uriInfo;
            return uriInfo;
        }
    }
}