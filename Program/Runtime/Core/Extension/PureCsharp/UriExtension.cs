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
        // クラス変数宣言
        private static readonly char[] StringSplitPattern = new char[] { '&' };



        /// <summary>
        /// UriクラスのQueryプロパティをDictionaryとしてアクセスできるように取得します
        /// </summary>
        /// <param name="uri">クエリが存在するUri</param>
        /// <returns>取得されたクエリのDictionaryを返します。クエリが空であっても件数が0件のDictionaryインスタンスを返します</returns>
        public static Dictionary<string, string> GetQueryDictionary(this Uri uri)
        {
            // Dictionaryインスタンスを生成して、引数として受け取る ToQueryDictionary 関数を叩いて返す
            var result = new Dictionary<string, string>();
            GetQueryDictionary(uri, result);
            return result;
        }


        /// <summary>
        /// UriクラスのQueryプロパティをDictionaryとしてアクセスできるように取得します
        /// </summary>
        /// <param name="uri">クエリが存在するUri</param>
        /// <param name="result">クエリの取得した結果を受けるDictionary</param>
        public static void GetQueryDictionary(this Uri uri, Dictionary<string, string> result)
        {
            // クエリ文字列の先頭の'?'を削除
            var queryString = uri.Query.TrimStart('?');


            // 各クエリの分割をしてループ（やってることは .Split().Select(x => x.Split()).ToDictionary() と同じ）
            var queries = queryString.Split(StringSplitPattern, StringSplitOptions.RemoveEmptyEntries);
            foreach (var query in queries)
            {
                // 更にキーと値を分離して結果テーブルに詰める
                var keyValue = query.Split('=');
                result[keyValue[0]] = keyValue[1];
            }
        }


        /// <summary>
        /// URIのスキームがHTTP系かどうかを確認します。
        /// 判定されるスキームは "http" または "https" です。
        /// </summary>
        /// <param name="uri">HTTPスキームかどうか調べるURI</param>
        /// <returns>HTTPスキームである場合は true を、違う場合は false を返します</returns>
        public static bool IsHttpScheme(this Uri uri)
        {
            // そもそも絶対パスでないなら
            if (!uri.IsAbsoluteUri)
            {
                // 判定は出来ない
                return false;
            }


            // スキームがHTTPかHTTPSかの判定を返す
            return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;
        }
    }
}