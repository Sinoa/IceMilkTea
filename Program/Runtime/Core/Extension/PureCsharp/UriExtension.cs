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

namespace IceMilkTea.Core
{
    /// <summary>
    /// System.Uri クラスの拡張関数実装用クラスです
    /// </summary>
    public static class UriExtensions
    {
        /// <summary>
        /// UriクラスのQueryプロパティをDictionaryとしてアクセスできるように変換します
        /// </summary>
        /// <param name="uri">変換するUri</param>
        /// <returns>変換したDictionaryを返します。クエリが空であっても件数が0件のDictionaryインスタンスを返します</returns>
        public static Dictionary<string, string> ToQueryDictionary(this Uri uri)
        {
            // Dictionaryインスタンスを生成して、引数として受け取る ToQueryDictionary 関数を叩いて返す
            var result = new Dictionary<string, string>();
            ToQueryDictionary(uri, result);
            return result;
        }


        /// <summary>
        /// UriクラスのQueryプロパティをDictionaryとしてアクセスできるように変換します
        /// </summary>
        /// <param name="uri">変換するUri</param>
        /// <param name="result">変換した結果を受けるDictionary</param>
        public static void ToQueryDictionary(this Uri uri, Dictionary<string, string> result)
        {
            // クエリ文字列の先頭の'?'を削除
            var queryString = uri.Query.TrimStart('?');


            // 各クエリの分割をしてループ（やってることは .Split().Select(x => x.Split()).ToDictionary() と同じ）
            var queries = queryString.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var query in queries)
            {
                // 更にキーと値を分離して結果テーブルに詰める
                var keyValue = query.Split('=');
                result[keyValue[0]] = keyValue[1];
            }
        }
    }
}