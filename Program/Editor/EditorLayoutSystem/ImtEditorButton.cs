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
using UnityEngine;

namespace IceMilkTeaEditor.LayoutSystem
{
    /// <summary>
    /// ボタンを表現するエディタUIです
    /// </summary>
    public class ImtEditorButton : ImtEditorUi
    {
        // メンバ変数定義
        private string text;



        /// <summary>
        /// ボタンの有効状態の取得設定をします
        /// </summary>
        public bool Enable { get; set; }


        /// <summary>
        /// ボタンに表示する文字列の取得設定をします
        /// </summary>
        /// <exception cref="ArgumentNullException">value が null です</exception>
        public string Text
        {
            get
            {
                // テキストはそのまま返す
                return text;
            }
            set
            {
                // null が 渡されたら
                if (value == null)
                {
                    // どういうテキストを描画すればよいのか
                    throw new ArgumentNullException(nameof(value));
                }


                // 描画するべきテキストを設定
                text = value;
            }
        }



        /// <summary>
        /// ボタンがクリックされた時のイベントです
        /// </summary>
        public event Action<ImtEditorButton> Click;



        /// <summary>
        /// ImtEditorButton クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="ownerWindow">所属するオーナーウィンドウ</param>
        public ImtEditorButton(ImtEditorWindow ownerWindow) : base(ownerWindow)
        {
            // 初期状態は有効状態
            Enable = true;
        }


        /// <summary>
        /// ImtEditorButton クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="ownerWindow">所属するオーナーウィンドウ</param>
        /// <param name="text">ボタンに表示するテキスト</param>
        public ImtEditorButton(ImtEditorWindow ownerWindow, string text) : base(ownerWindow)
        {
            // テキストを設定する
            Enable = true;
            Text = text;
        }


        /// <summary>
        /// ボタンのレンダリングを行います
        /// </summary>
        protected internal override void Render()
        {
            // 現在のGUIカラーを覚えて、有効無効時のカラーを設定する
            var currentColor = GUI.color;
            GUI.color = Enable ? GUI.color : Color.gray;


            // ボタンを描画して、有効かつ押されたのなら
            if (GUILayout.Button(Text) && Enable)
            {
                // ボタンクリックメッセージをポストする
                PostMessage(button => Click?.Invoke((ImtEditorButton)button), this);
            }


            // GUIカラーを戻す
            GUI.color = currentColor;
        }
    }
}