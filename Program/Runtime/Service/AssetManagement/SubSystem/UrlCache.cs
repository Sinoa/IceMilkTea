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
using IceMilkTea.Core;

namespace IceMilkTea.Service
{
    /// <summary>
    /// Uriインスタンスをキャッシュしておくクラスです
    /// </summary>
    internal class UrlCache
    {
        // 定数定義
        private const int DefaultCapacity = 1 << 10;

        // メンバ変数定義
        private Dictionary<ulong, Uri> urlCacheTable;



        /// <summary>
        /// UrlCache のインスタンスを初期化します
        /// </summary>
        public UrlCache()
        {
            // キャッシュ用テーブルを生成
            urlCacheTable = new Dictionary<ulong, Uri>(DefaultCapacity);
        }


        /// <summary>
        /// Url文字列からURLIDを取得します
        /// </summary>
        /// <param name="urlText">IDの取得をするURL文字列</param>
        /// <returns>URL文字列から取得されたIDを返します</returns>
        /// <exception cref="ArgumentNullException">urlText が null です</exception>
        public ulong GetUrlId(string urlText)
        {
            // null を渡されたら
            if (urlText == null)
            {
                // 何のIDを求めればよいのか
                throw new ArgumentNullException(nameof(urlText));
            }


            // CRC64計算した結果をそのまま返す
            return urlText.ToCrc64Code();
        }


        /// <summary>
        /// 指定されたIDでUriインスタンスをキャッシュします
        /// </summary>
        /// <param name="urlId">キャッシュするURLのID</param>
        /// <param name="url">キャッシュするUriインスタンス</param>
        /// <exception cref="ArgumentNullException">url が null です</exception>
        public void CacheUrl(ulong urlId, Uri url)
        {
            // null を渡されたら
            if (url == null)
            {
                // 何をキャッシュするのか
                throw new ArgumentNullException(nameof(url));
            }


            // 指定されたIDにUriを打ち込む
            urlCacheTable[urlId] = url;
        }


        /// <summary>
        /// 指定されたIDのキャッシュされたUriインスタンスの取得を試みます
        /// </summary>
        /// <param name="urlId">取得したいURLのID</param>
        /// <param name="url">取得されたUriインスタンスを格納します</param>
        /// <returns>指定されたIDのUriインスタンスが存在し取得ができた場合は true を、取得できなかった場合は false を返します</returns>
        public bool TryGetUrl(ulong urlId, out Uri url)
        {
            // 指定されたIDのUriインスタンスの取得を試みる
            return urlCacheTable.TryGetValue(urlId, out url);
        }
    }
}