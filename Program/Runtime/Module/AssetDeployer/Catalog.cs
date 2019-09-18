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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace IceMilkTea.SubSystem
{
    #region インターフェイス
    /// <summary>
    /// カタログに含まれるアイテムの情報を表現するインターフェイスです
    /// </summary>
    public interface ICatalogItem
    {
        /// <summary>
        /// アイテム名
        /// </summary>
        string Name { get; }


        /// <summary>
        /// アイテムの内容の長さ（バイト数）
        /// </summary>
        long ContentLength { get; }


        /// <summary>
        /// フェッチする参照先アイテムURI
        /// </summary>
        Uri RemoteUri { get; }


        /// <summary>
        /// ストレージからアイテムをアクセスするためのアイテムURI
        /// </summary>
        Uri LocalUri { get; }


        /// <summary>
        /// このアイテムのハッシュデータ
        /// </summary>
        byte[] HashData { get; }


        /// <summary>
        /// ハッシュ計算に使用したハッシュ名
        /// </summary>
        string HashName { get; }
    }



    /// <summary>
    /// 一覧を表現するインターフェイスです
    /// </summary>
    public interface ICatalog
    {
        /// <summary>
        /// カタログに含まれるアイテム数
        /// </summary>
        int ItemCount { get; }



        /// <summary>
        /// 指定された名前のカタログアイテムが含まれているか確認します
        /// </summary>
        /// <param name="name">確認するカタログアイテムの名前</param>
        /// <returns>含まれている場合は true を、含まれていない場合は false を返します</returns>
        bool ContainItem(string name);


        /// <summary>
        /// 指定した名前のカタログアイテムを取得します
        /// </summary>
        /// <param name="name">取得するアイテム名</param>
        /// <returns>指定された名前からカタログアイテムを取得された場合はインスタンスを返しますが、見つからない場合は null を返します</returns>
        ICatalogItem GetItem(string name);


        /// <summary>
        /// カタログに含まれている全てのカタログアイテムを取得して列挙可能なオブジェクトを取得します
        /// </summary>
        /// <returns>全てのカタログアイテムを列挙可能なオブジェクトを返します</returns>
        IEnumerable<ICatalogItem> GetItemAll();
    }



    /// <summary>
    /// 一覧を表現し内容の型も定義できるインターフェイスです
    /// </summary>
    /// <typeparam name="TItem">カタログが持つアイテムの型</typeparam>
    public interface ICatalog<TItem> : ICatalog where TItem : ICatalogItem
    {
        /// <summary>
        /// 指定した名前のカタログアイテムを取得します
        /// </summary>
        /// <param name="name">取得するアイテム名</param>
        /// <returns>指定された名前からカタログアイテムを取得された場合はインスタンスを返しますが、見つからない場合は null を返します</returns>
        new TItem GetItem(string name);


        /// <summary>
        /// カタログに含まれている全てのカタログアイテムを取得して列挙可能なオブジェクトを取得します
        /// </summary>
        /// <returns>全てのカタログアイテムを列挙可能なオブジェクトを返します</returns>
        new IEnumerable<TItem> GetItemAll();
    }



    /// <summary>
    /// ストリームからカタログを読み込むインターフェイスです
    /// </summary>
    public interface ICatalogReader
    {
        /// <summary>
        /// 指定されたストリームからカタログを非同期で読み込みます
        /// </summary>
        /// <param name="stream">カタログのデータを読み込むストリーム</param>
        /// <returns>正常に読み込まれた場合はカタログのインスタンスを返しますが、読み込まれなかった場合は null を返します</returns>
        Task<ICatalog> ReadCatalogAsync(Stream stream);
    }



    /// <summary>
    /// ストリームからカタログを読み込むインターフェイスです
    /// </summary>
    /// <typeparam name="TCatalog">読み込まれるカタログの型</typeparam>
    public interface ICatalogReader<TCatalog> : ICatalogReader where TCatalog : ICatalog
    {
        /// <summary>
        /// 指定されたストリームからカタログを非同期で読み込みます
        /// </summary>
        /// <param name="stream">カタログのデータを読み込むストリーム</param>
        /// <returns>正常に読み込まれた場合はカタログのインスタンスを返しますが、読み込まれなかった場合は null を返します</returns>
        new Task<TCatalog> ReadCatalogAsync(Stream stream);
    }
    #endregion



    #region シンプルな実装クラス
    /// <summary>
    /// 単純なカタログアイテムクラスです
    /// </summary>
    public class ImtCatalogItem : ICatalogItem
    {
        /// <summary>
        /// アイテム名
        /// </summary>
        public string Name { get; protected set; }


        /// <summary>
        /// アイテムの内容の長さ（バイト数）
        /// </summary>
        public long ContentLength { get; protected set; }


        /// <summary>
        /// フェッチする参照先リモートURI
        /// </summary>
        public Uri RemoteUri { get; protected set; }


        /// <summary>
        /// ストレージの参照先ローカルURI
        /// </summary>
        public Uri LocalUri { get; protected set; }


        /// <summary>
        /// このアイテムのハッシュデータ
        /// </summary>
        public byte[] HashData { get; protected set; }


        /// <summary>
        /// ハッシュ計算に使用したハッシュ名
        /// </summary>
        public string HashName { get; protected set; }



        /// <summary>
        /// ImtCatalogItem クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="item">インスタンスの初期化元になる、カタログアイテムのインターフェイス</param>
        public ImtCatalogItem(ICatalogItem item) : this(item.Name, item.ContentLength, item.RemoteUri, item.LocalUri, item.HashData, item.HashName)
        {
        }


        /// <summary>
        /// ImtCatalogItem クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="name">アイテム名</param>
        /// <param name="contentLength">コンテンツの長さ（バイト数）。もし負の値が指定された場合は 0 として扱われます。</param>
        /// <param name="remoteUri">フェッチする参照先リモートURI</param>
        /// <param name="localUri">ストレージの参照先ローカルURI</param>
        /// <param name="hashData">ハッシュデータ。もし null の場合は内部で長さ0の配列として初期化されます。</param>
        /// <param name="hashName">ハッシュデータを生成する際に利用したハッシュアルゴリズムの名前。もし null の場合は空文字列として初期化されます。</param>
        /// <exception cref="ArgumentException">name が null または 空文字列 です。</exception>
        /// <exception cref="ArgumentNullException">remoteUri が null です。</exception>
        /// <exception cref="ArgumentNullException">localUri が null です。</exception>
        public ImtCatalogItem(string name, long contentLength, Uri remoteUri, Uri localUri, byte[] hashData, string hashName)
        {
            // もしアイテム名が扱えない文字列なら
            if (string.IsNullOrWhiteSpace(name))
            {
                // 例外を吐く
                throw new ArgumentException($"{nameof(name)} が null または 空文字列 です。", nameof(name));
            }


            // 初期化をする
            Name = name;
            ContentLength = Math.Max(contentLength, 0);
            RemoteUri = remoteUri ?? throw new ArgumentNullException(nameof(remoteUri));
            LocalUri = localUri ?? throw new ArgumentNullException(nameof(localUri));
            HashData = hashData == null ? Array.Empty<byte>() : (byte[])hashData.Clone();
            HashName = string.IsNullOrWhiteSpace(hashName) ? string.Empty : hashName;
        }
    }



    /// <summary>
    /// 単純なカタログクラスです
    /// </summary>
    public class ImtCatalog : ICatalog<ImtCatalogItem>
    {
        // メンバ変数定義
        private Dictionary<string, ImtCatalogItem> itemTable;



        /// <summary>
        /// カタログに含まれるアイテム数
        /// </summary>
        public int ItemCount => itemTable.Count;



        /// <summary>
        /// ImtCatalog クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="catalog">複製元のカタログ</param>
        /// <exception cref="ArgumentNullException">catalog が null です</exception>
        public ImtCatalog(ICatalog catalog) : this((catalog ?? throw new ArgumentNullException(nameof(catalog))).GetItemAll())
        {
        }


        /// <summary>
        /// ImtCatalog クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="catalogItems">カタログに追加するカタログアイテムの列挙可能オブジェクト</param>
        /// <exception cref="ArgumentNullException">catalogItems が null です</exception>
        public ImtCatalog(IEnumerable<ICatalogItem> catalogItems)
        {
            // すべてImtCatalogItemとしてテーブルを生成する
            itemTable = (catalogItems ?? throw new ArgumentNullException(nameof(catalogItems)))
                .Select(x => new ImtCatalogItem(x))
                .ToDictionary(x => x.Name);
        }


        /// <summary>
        /// 指定された名前のカタログアイテムが含まれているか確認します
        /// </summary>
        /// <param name="name">確認するカタログアイテムの名前</param>
        /// <returns>含まれている場合は true を、含まれていない場合は false を返します</returns>
        public bool ContainItem(string name)
        {
            // ContainKeyの結果をそのまま返す
            return itemTable.ContainsKey(name);
        }


        /// <summary>
        /// 指定された名前のカタログアイテムを取得します
        /// </summary>
        /// <param name="name">取得するカタログアイテムの名前</param>
        /// <returns>指定された名前のカタログアイテムを見つけた場合はその参照を、見つけられなかった場合は null を返します</returns>
        public ImtCatalogItem GetItem(string name)
        {
            // TryGet関数の結果をそのまま使用する
            itemTable.TryGetValue(name, out var value);
            return value;
        }


        /// <summary>
        /// カタログに含まれている全てのカタログアイテムを取得して列挙可能なオブジェクトを取得します
        /// </summary>
        /// <returns>全てのカタログアイテムを列挙可能なオブジェクトを返します</returns>
        public IEnumerable<ImtCatalogItem> GetItemAll()
        {
            // テーブルの値を返す
            return itemTable.Values;
        }


        /// <summary>
        /// 指定された名前のカタログアイテムを取得します
        /// </summary>
        /// <param name="name">取得するカタログアイテムの名前</param>
        /// <returns>指定された名前のカタログアイテムを見つけた場合はその参照を、見つけられなかった場合は null を返します</returns>
        ICatalogItem ICatalog.GetItem(string name)
        {
            // 実装関数をそのまま使用する
            return GetItem(name);
        }


        /// <summary>
        /// カタログに含まれている全てのカタログアイテムを取得して列挙可能なオブジェクトを取得します
        /// </summary>
        /// <returns>全てのカタログアイテムを列挙可能なオブジェクトを返します</returns>
        IEnumerable<ICatalogItem> ICatalog.GetItemAll()
        {
            // 実装関数をそのまま使用する
            return GetItemAll();
        }
    }
    #endregion
}