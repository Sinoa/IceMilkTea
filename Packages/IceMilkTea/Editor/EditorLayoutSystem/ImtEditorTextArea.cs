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
using UnityEditor;
using UnityEngine;

namespace IceMilkTeaEditor.LayoutSystem
{
    /// <summary>
    /// テキストエリアを表現するエディタUIです
    /// </summary>
    public class ImtEditorTextArea : ImtEditorUi
    {
        // メンバ変数定義
        private string text;



        /// <summary>
        /// UIの有効状態の取得設定をします
        /// </summary>
        public bool Enable { get; set; }


        /// <summary>
        /// テキストエリアの内容は読み取り専用かどうか
        /// </summary>
        public bool ReadOnly { get; set; }


        /// <summary>
        /// 表示する文字列の取得設定をします
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
        /// テキストエリア内の文字列が変化した時のイベントです
        /// </summary>
        public event Action<ImtEditorTextArea> TextChanged;



        /// <summary>
        /// ImtEditorTextArea クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="ownerWindow">所属するオーナーウィンドウ</param>
        public ImtEditorTextArea(ImtEditorWindow ownerWindow) : base(ownerWindow)
        {
            // 初期状態は有効状態
            Enable = true;
            Text = string.Empty;
        }


        /// <summary>
        /// UIのレンダリングを行います
        /// </summary>
        protected internal override void Render()
        {
            // 現在のGUIカラーを覚えて、有効無効時のカラーを設定する
            var currentColor = GUI.color;
            GUI.color = Enable ? GUI.color : Color.gray;


            // 現在のテキストエリアの文字列を覚えておく
            var newerText = EditorGUILayout.TextArea(text);


            // テキストエリアの返す文字列と入力前の文字列が異なるなら
            if (text != newerText)
            {
                // テキストが変化したイベントをポストする
                PostMessage(textArea => TextChanged?.Invoke((ImtEditorTextArea)textArea), this);


                // もし 読み取り専用でない かつ 有効 なら
                if (!ReadOnly && Enable)
                {
                    // 新しいテキストを覚える
                    text = newerText;
                }
            }


            // GUIカラーを戻す
            GUI.color = currentColor;
        }
    }
}