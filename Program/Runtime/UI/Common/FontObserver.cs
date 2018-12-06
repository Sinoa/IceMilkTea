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
using UnityEngine;

namespace IceMilkTea.UI
{
    /// <summary>
    /// UnityEngineのフォントを監視するクラスです。
    /// 主に、フォントの再構築などを検知して所属オブジェクトに通知します。
    /// </summary>
    internal static class FontObserver
    {
        // クラス変数宣言
        private static Dictionary<Font, HashSet<IFontUser>> fontUserTable = new Dictionary<Font, HashSet<IFontUser>>();



        /// <summary>
        /// 監視オブジェクトにフォント利用オブジェクトを追加します。
        /// 多重追加することは出来ません。
        /// </summary>
        /// <param name="fontUser">追加するフォント利用オブジェクト</param>
        /// <exception cref="ArgumentNullException">fontUser が null です</exception>
        /// <exception cref="InvalidOperationException">fontUser.Font が null です</exception>
        public static void AddUser(IFontUser fontUser)
        {
            // 例外ハンドリングをする
            ThrowIfFontNotFound(fontUser);


            // もしはじめての追加なら　
            if (fontUserTable.Count == 0)
            {
                // フォント再構築ハンドラを登録する
                Font.textureRebuilt += OnFontRebuilded;
            }


            // フォント利用者テーブルからフォント利用者リストを取り出せないなら
            HashSet<IFontUser> fontUserList;
            if (!fontUserTable.TryGetValue(fontUser.Font, out fontUserList))
            {
                // フォント利用者リストを作ってテーブルに追加しておく
                fontUserList = new HashSet<IFontUser>();
                fontUserTable[fontUser.Font] = fontUserList;
            }


            // フォント利用者リストにすでに存在するなら
            if (fontUserList.Contains(fontUser))
            {
                // 何もせず終了
                return;
            }


            // リストにフォント利用者を追加する
            fontUserList.Add(fontUser);
        }


        /// <summary>
        /// 監視オブジェクトからフォント利用オブジェクトを削除します。
        /// </summary>
        /// <param name="fontUser">削除するフォント利用オブジェクト</param>
        /// <exception cref="ArgumentNullException">fontUser が null です</exception>
        /// <exception cref="InvalidOperationException">fontUser.Font が null です</exception>
        public static void RemoveUser(IFontUser fontUser)
        {
            // 例外ハンドリングをする
            ThrowIfFontNotFound(fontUser);


            // フォント利用者テーブルが空なら
            if (fontUserTable.Count == 0)
            {
                // そもそも何もしない
                return;
            }


            // フォント利用者テーブルからフォント利用者リストを取り出せないなら
            HashSet<IFontUser> fontUserList;
            if (!fontUserTable.TryGetValue(fontUser.Font, out fontUserList))
            {
                // 何もしないで終了
                return;
            }


            // リストからフォント利用者を削除して空になったら
            fontUserList.Remove(fontUser);
            if (fontUserList.Count == 0)
            {
                // フォント利用者テーブルからレコードを削除して、更にテーブルも空になったら
                fontUserTable.Remove(fontUser.Font);
                if (fontUserTable.Count == 0)
                {
                    // フォント再構築ハンドラを解除する
                    Font.textureRebuilt -= OnFontRebuilded;
                }
            }
        }


        /// <summary>
        /// フォントテクスチャの再生成のハンドリングをします
        /// </summary>
        /// <param name="font">再生成されたフォント</param>
        private static void OnFontRebuilded(Font font)
        {
            // 該当のフォントの利用者が居ないなら
            HashSet<IFontUser> fontUserList;
            if (!fontUserTable.TryGetValue(font, out fontUserList))
            {
                // 何もせず終了
                return;
            }


            // フォント利用オブジェクト分回る
            foreach (var user in fontUserList)
            {
                // 再構築されたことを通知する
                user.OnFontRebuilded(font);
            }
        }


        /// <summary>
        /// フォント利用オブジェクト自体及び、その利用オブジェクトのフォントの参照がない場合例外をスローします
        /// </summary>
        /// <param name="fontUser">確認するフォント利用オブジェクト</param>
        /// <exception cref="ArgumentNullException">fontUser が null です</exception>
        /// <exception cref="InvalidOperationException">fontUser.Font が null です</exception>
        private static void ThrowIfFontNotFound(IFontUser fontUser)
        {
            // nullを渡されたら
            if (fontUser == null)
            {
                // nullは許されない
                throw new ArgumentNullException(nameof(fontUser));
            }


            // フォント利用オブジェクトのフォントがそもそも未設定なら
            if (fontUser.Font == null)
            {
                // 何を扱えばよいのか
                throw new InvalidOperationException($"{nameof(fontUser.Font)} が null です");
            }
        }
    }
}