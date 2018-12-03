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

namespace IceMilkTeaEditor.LayoutSystem
{
    /// <summary>
    /// エディタのレイアウト構成を持つUIで、ルートに位置するUIクラスです
    /// </summary>
    public sealed class ImtEditorUiRoot : ImtEditorUi
    {
        // メンバ変数定義
        private List<ImtEditorUi> uiList;



        /// <summary>
        /// ImtEditorUiRoot クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="ownerWindow">所属するオーナーウィンドウ</param>
        public ImtEditorUiRoot(ImtEditorWindow ownerWindow) : base(ownerWindow)
        {
            // UIリストの初期化をする
            uiList = new List<ImtEditorUi>();
        }


        /// <summary>
        /// レンダリングを行います
        /// </summary>
        protected internal override void Render()
        {
            // 追加されたUIを順番に回る
            foreach (var ui in uiList)
            {
                // UIレンダリングを呼ぶ
                ui.Render();
            }
        }


        /// <summary>
        /// レンダリング対象となるUIを追加します。
        /// ただし、UIは多重追加は出来ません
        /// </summary>
        /// <param name="ui">追加するUI</param>
        /// <exception cref="ArgumentNullException">ui が null です</exception>
        public void AddUi(ImtEditorUi ui)
        {
            // null を渡されたら
            if (ui == null)
            {
                // 何を描画すればええんじゃ
                throw new ArgumentNullException(nameof(ui));
            }


            // すでにリストにいるなら
            if (uiList.Contains(ui))
            {
                // 何もせず終了
                return;
            }


            // リストにUIを追加
            uiList.Add(ui);
        }


        /// <summary>
        /// レンダリング対象となっていたUIを削除します
        /// </summary>
        /// <param name="ui">削除するUI</param>
        public void RemoveUi(ImtEditorUi ui)
        {
            // そのままリストの削除を呼ぶ
            uiList.Remove(ui);
        }
    }
}