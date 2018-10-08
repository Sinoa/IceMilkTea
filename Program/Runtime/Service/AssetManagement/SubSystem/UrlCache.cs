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
    /// Uriインスタンスの生成コストを回避するために、極力キャッシュから取り出せるようにするためのキャッシュクラスです。
    /// </summary>
    internal class UrlCache
    {
        // 定数定義
        private const int DefaultCapacity = 2 << 10;

        // メンバ変数定義
        private Dictionary<string, Uri> urlCacheTable;



        /// <summary>
        /// UrlCache のインスタンスを初期化します
        /// </summary>
        public UrlCache()
        {
            // キャッシュ用テーブルを生成
            urlCacheTable = new Dictionary<string, Uri>(DefaultCapacity);
        }


        /// <summary>
        /// 指定されたURL文字列から、生成経験のあるUriインスタンスが存在すれば取得し、
        /// まだ生成経験がない場合は、新しくインスタンスを生成します。
        /// </summary>
        /// <param name="urlText">Uriのインスタンスを取得または生成するURL文字列</param>
        /// <returns>取得または生成したUriインスタンスを返します</returns>
        /// <exception cref="ArgumentNullException">urlText が null です</exception>
        /// <exception cref="ArgumentException">有効なURL書式ではありません</exception>
        public Uri GetOrCreateUri(string urlText)
        {
            // もし null を渡されたら
            if (urlText == null)
            {
                // nullは取り扱えない
                throw new ArgumentNullException(nameof(urlText));
            }


            // キャッシュテーブルから生成済みUriインスタンスの取得を試みて、取得できたのなら
            Uri uri;
            if (urlCacheTable.TryGetValue(urlText, out uri))
            {
                // 取得できたインスタンスを返す
                return uri;
            }


            // 新しくUriインスタンスの生成を試みるが出来ないなら
            if (!Uri.TryCreate(urlText, UriKind.Absolute, out uri))
            {
                // 正しいURLである必要がある例外を吐く
                throw new ArgumentException($"指定されたURLは有効なURL書式ではありません urlText='{urlText}'");
            }


            // 生成されたインスタンスを覚えて返す
            urlCacheTable[urlText] = uri;
            return uri;
        }
    }
}