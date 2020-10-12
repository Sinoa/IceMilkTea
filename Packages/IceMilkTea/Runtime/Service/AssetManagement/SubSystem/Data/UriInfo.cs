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
using System.Collections.ObjectModel;
using IceMilkTea.Core;

namespace IceMilkTea.Service
{
    /// <summary>
    /// Uriとクエリの情報を持つクラスです
    /// </summary>
    internal class UriInfo : IEquatable<UriInfo>
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


        /// <summary>
        /// Uriのクエリに含まれる特定のキーの値を取得に試みます
        /// </summary>
        /// <param name="key">取得するキー</param>
        /// <param name="value">取得した値を格納する参照</param>
        /// <returns>取得に成功した場合は true を、失敗した場合は false を返します</returns>
        public bool TryGetQuery(string key, out string value)
        {
            // テーブルのTryGetValueをそのまま呼び出して結果を返す
            return QueryTable.TryGetValue(key, out value);
        }


        /// <summary>
        /// UriInfo インスタンスのハッシュ値を取得します
        /// </summary>
        /// <returns>UriInfo インスタンスのハッシュ値を返します</returns>
        public override int GetHashCode()
        {
            //Uri.GetHashCode is too slow, so use string.GetHashCode for Dictionary Key
            return Uri.OriginalString.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj);
        }

        public bool Equals(UriInfo other)
        {
            if (other is null)
                return false;
            
            //see GetHashCode
            return this.Uri.OriginalString == other.Uri.OriginalString;
        }
    }
}