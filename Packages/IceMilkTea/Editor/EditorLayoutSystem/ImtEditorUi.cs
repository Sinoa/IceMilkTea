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

namespace IceMilkTeaEditor.LayoutSystem
{
    /// <summary>
    /// IceMilkTeaのエディタウィンドウで、最小となるUIを表現するクラスです
    /// </summary>
    public abstract class ImtEditorUi
    {
        /// <summary>
        /// 自身が所属するオーナーウィンドウの取得をします
        /// </summary>
        protected ImtEditorWindow OwnerWindow { get; private set; }



        /// <summary>
        /// ImtEditorUi クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="ownerWindow">所属するオーナーウィンドウ</param>
        public ImtEditorUi(ImtEditorWindow ownerWindow)
        {
            // 所属するウィンドウを覚える
            OwnerWindow = ownerWindow;
        }


        /// <summary>
        /// このUIのレンダリングを行います
        /// </summary>
        protected internal virtual void Render()
        {
        }


        /// <summary>
        /// UIからの処理すべきメッセージをポストします
        /// </summary>
        /// <param name="callback">処理するべきメッセージのコールバック</param>
        /// <param name="state">コールバックに渡される任意のオブジェクト</param>
        /// <exception cref="ArgumentNullException">callback が null です</exception>
        protected void PostMessage(Action<object> callback, object state)
        {
            // オーナーウィンドウにメッセージをポストする
            OwnerWindow.PostMessage(callback, state);
        }
    }
}