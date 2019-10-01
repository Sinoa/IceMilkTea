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

namespace IceMilkTea.SubSystem
{
    /// <summary>
    /// IceMilkTea が提供するプラットフォーム間共通ストレージクラスです
    /// </summary>
    public static class ImtStorage
    {
        // 定数定義
        public const int DefaultBufferSize = (16 << 10); // iOS 向けバッファサイズ（他のシステムなら4KiBでも良いかもしれないけどiOSに合わせる)

        // クラス変数宣言
        private static readonly Dictionary<string, ImtStorageController> controllerTable;



        /// <summary>
        /// ImtStorage クラスの初期化をします
        /// </summary>
        static ImtStorage()
        {
            // コントローラテーブルのインスタンスを生成
            controllerTable = new Dictionary<string, ImtStorageController>();
        }


        #region コントローラ操作関数
        /// <summary>
        /// ストレージにストレージコントローラを追加します。すでに同じ名前のコントローラがある場合は何もしません。
        /// </summary>
        /// <param name="controller">追加するコントローラ</param>
        /// <exception cref="ArgumentNullException">controller が null です</exception>
        /// <exception cref="ArgumentException">追加しようとしたコントローラの名前が、空白文字列または、空文字列または null です</exception>
        public static void AddController(ImtStorageController controller)
        {
            // コントローラ名が取り扱えない名前なら
            var name = (controller ?? throw new ArgumentNullException(nameof(controller))).Name;
            if (string.IsNullOrWhiteSpace(name))
            {
                // 取り扱えない名前は拒否
                throw new ArgumentException("追加しようとしたコントローラの名前が、空白文字列または、空文字列または null です", nameof(controller));
            }


            // コントローラがすでに存在しているのなら
            if (controllerTable.ContainsKey(name))
            {
                // 何もせず終了
                return;
            }


            // 新しく追加する
            controllerTable[name] = controller;
        }


        /// <summary>
        /// 指定された名前のストレージコントローラを取得します
        /// </summary>
        /// <typeparam name="TController">取得するストレージコントローラの型</typeparam>
        /// <param name="name">取得するストレージコントローラの名前</param>
        /// <returns>指定された名前のストレージコントローラを見つけた場合は返しますが、見つからなかったまたは取得出来ない場合は null を返します</returns>
        public static TController GetController<TController>(string name) where TController : ImtStorageController
        {
            // 名前が取り扱えない名前なら
            if (string.IsNullOrWhiteSpace(name))
            {
                // 取り扱えない名前は拒否
                throw new ArgumentException("指定されたコントローラ名が、空白文字列または、空文字列または null です", nameof(name));
            }


            // 名前から取得を試みて、取得出来たのなら
            if (controllerTable.TryGetValue(name, out var controller))
            {
                // キャストして返す
                return controller as TController;
            }


            // ここまで来たのなら無理だったということなので null を返す
            return null;
        }


        /// <summary>
        /// 内部で使用する GetController 関数の代用です
        /// </summary>
        /// <param name="name">取得するストレージコントローラの名前</param>
        /// <returns>指定された名前のストレージコントローラを返します</returns>
        /// <exception cref="ImtStorageControllerNotFoundException">'{name}' の名前のストレージコントローラが見つかりませんでした</exception>
        private static ImtStorageController GetControllerCore(string name)
        {
            // 名前からそのまま取得して問題なければ返すが、nullになった場合は例外を吐く
            controllerTable.TryGetValue(name, out var controller);
            return controller ?? throw new ImtStorageControllerNotFoundException($"'{name}' の名前のストレージコントローラが見つかりませんでした", name);
        }
        #endregion


        #region ストリーム操作関数
        /// <summary>
        /// 指定されたURIのデータが存在するか否かを確認します
        /// </summary>
        /// <param name="uri">存在を確認するURI</param>
        /// <returns>データが存在する場合は true を、存在しない場合は false を返します</returns>
        /// <exception cref="ArgumentNullException">uri が null です</exception>
        /// <exception cref="ArgumentException">uri は絶対URIパスでなければなりません</exception>
        /// <exception cref="ImtStorageControllerNotFoundException">'{name}' の名前のストレージコントローラが見つかりませんでした</exception>
        public static bool Exists(Uri uri)
        {
            // 例外チェックをしてホスト名からコントローラを取得後Exists関数を叩いた結果を返す
            ThrowExceptionIfInvalidUri(uri);
            return GetControllerCore(uri.Host).Exists(uri);
        }


        /// <summary>
        /// 指定されたURIのストリームを開きます
        /// </summary>
        /// <param name="uri">開く対象となるURI</param>
        /// <param name="mode">ストリームを開くモード</param>
        /// <param name="access">ストリームへのアクセス方法</param>
        /// <returns>正しくストリームを開けた場合はストリームを返しますが、開けなかった場合は null を返します</returns>
        /// <exception cref="ArgumentNullException">uri が null です</exception>
        /// <exception cref="ArgumentException">uri は絶対URIパスでなければなりません</exception>
        /// <exception cref="ImtStorageControllerNotFoundException">'{name}' の名前のストレージコントローラが見つかりませんでした</exception>
        public static Stream Open(Uri uri, FileMode mode, FileAccess access)
        {
            // 共有設定はなし、既定バッファサイズ、非同期操作を利用しないでストリームのオープンをする
            return Open(uri, mode, access, FileShare.None, DefaultBufferSize, false);
        }


        /// <summary>
        /// 指定されたURIのストリームを開きます
        /// </summary>
        /// <param name="uri">開く対象となるURI</param>
        /// <param name="mode">ストリームを開くモード</param>
        /// <param name="access">ストリームへのアクセス方法</param>
        /// <param name="share">ストリームの共有設定。既定は None です。</param>
        /// <returns>正しくストリームを開けた場合はストリームを返しますが、開けなかった場合は null を返します</returns>
        /// <exception cref="ArgumentNullException">uri が null です</exception>
        /// <exception cref="ArgumentException">uri は絶対URIパスでなければなりません</exception>
        /// <exception cref="ImtStorageControllerNotFoundException">'{name}' の名前のストレージコントローラが見つかりませんでした</exception>
        public static Stream Open(Uri uri, FileMode mode, FileAccess access, FileShare share)
        {
            // 既定バッファサイズと非同期操作を利用しないでストリームのオープンをする
            return Open(uri, mode, access, share, DefaultBufferSize, false);
        }


        /// <summary>
        /// 指定されたURIのストリームを開きます
        /// </summary>
        /// <param name="uri">開く対象となるURI</param>
        /// <param name="mode">ストリームを開くモード</param>
        /// <param name="access">ストリームへのアクセス方法</param>
        /// <param name="share">ストリームの共有設定。既定は None です。</param>
        /// <param name="bufferSize">ストリームが持つバッファサイズ。既定は DefaultBufferSize です。</param>
        /// <returns>正しくストリームを開けた場合はストリームを返しますが、開けなかった場合は null を返します</returns>
        /// <exception cref="ArgumentNullException">uri が null です</exception>
        /// <exception cref="ArgumentException">uri は絶対URIパスでなければなりません</exception>
        /// <exception cref="ImtStorageControllerNotFoundException">'{name}' の名前のストレージコントローラが見つかりませんでした</exception>
        /// <exception cref="ArgumentException">bufferSize が 0 以下です。bufferSize='{bufferSize}'</exception>
        public static Stream Open(Uri uri, FileMode mode, FileAccess access, FileShare share, int bufferSize)
        {
            // 起動機操作を利用しないでストリームのオープンをする
            return Open(uri, mode, access, share, bufferSize, false);
        }


        /// <summary>
        /// 指定されたURIのストリームを開きます
        /// </summary>
        /// <param name="uri">開く対象となるURI</param>
        /// <param name="mode">ストリームを開くモード</param>
        /// <param name="access">ストリームへのアクセス方法</param>
        /// <param name="share">ストリームの共有設定。既定は None です。</param>
        /// <param name="bufferSize">ストリームが持つバッファサイズ。既定は DefaultBufferSize です。</param>
        /// <param name="useAsync">ストリームの非同期操作を使用するか否か。既定は false です。</param>
        /// <returns>正しくストリームを開けた場合はストリームを返しますが、開けなかった場合は null を返します</returns>
        /// <exception cref="ArgumentNullException">uri が null です</exception>
        /// <exception cref="ArgumentException">uri は絶対URIパスでなければなりません</exception>
        /// <exception cref="ImtStorageControllerNotFoundException">'{name}' の名前のストレージコントローラが見つかりませんでした</exception>
        /// <exception cref="ArgumentException">bufferSize が 0 以下です。bufferSize='{bufferSize}'</exception>
        public static Stream Open(Uri uri, FileMode mode, FileAccess access, FileShare share, int bufferSize, bool useAsync)
        {
            // 例外判定をしてコントロラーを取得後Openをそのまま叩く
            ThrowExceptionIfInvalidUri(uri);
            ThrowExceptionIfInvalidBufferSize(bufferSize);
            return GetControllerCore(uri.Host).Open(uri, mode, access, share, bufferSize, useAsync);
        }


        /// <summary>
        /// 指定されたURIのデータを削除します
        /// </summary>
        /// <param name="uri">削除するURI</param>
        /// <exception cref="ArgumentNullException">uri が null です</exception>
        /// <exception cref="ArgumentException">uri は絶対URIパスでなければなりません</exception>
        /// <exception cref="ImtStorageControllerNotFoundException">'{name}' の名前のストレージコントローラが見つかりませんでした</exception>
        public static void Delete(Uri uri)
        {
            // 例外判定をしてコントローラの取得後Deleteをそのまま叩く
            ThrowExceptionIfInvalidUri(uri);
            GetControllerCore(uri.Host).Delete(uri);
        }


        /// <summary>
        /// 管理しているデータすべてを削除します
        /// </summary>
        public static void DeleteAll()
        {
            // 全コントローラ分回る
            foreach (var controller in controllerTable.Values)
            {
                // DeleteAllを叩く
                controller.DeleteAll();
            }
        }
        #endregion


        #region 例外関数
        /// <summary>
        /// 指定されたURIが無効な指定の場合は例外をスローします
        /// </summary>
        /// <param name="uri">確認するURI</param>
        /// <exception cref="ArgumentNullException">uri が null です</exception>
        /// <exception cref="ArgumentException">uri は絶対URIパスでなければなりません</exception>
        private static void ThrowExceptionIfInvalidUri(Uri uri)
        {
            // nullまたは絶対URIでないなら
            if (!(uri ?? throw new ArgumentNullException(nameof(uri))).IsAbsoluteUri)
            {
                // 取り扱えないURIであることを例外で投げる
                throw new ArgumentException("uri は絶対URIパスでなければなりません", nameof(uri));
            }
        }


        /// <summary>
        /// バッファサイズが0以下の場合は例外をスローします
        /// </summary>
        /// <param name="bufferSize">確認するバッファサイズの値</param>
        /// <exception cref="ArgumentException">bufferSize が 0 以下です。bufferSize='{bufferSize}'</exception>
        private static void ThrowExceptionIfInvalidBufferSize(int bufferSize)
        {
            // バッファサイズが0以下なら
            if (bufferSize <= 0)
            {
                // 0以下のバッファってどんなバッファですか
                throw new ArgumentException($"bufferSize が 0 以下です。bufferSize='{bufferSize}'", nameof(bufferSize));
            }
        }
        #endregion
    }
}