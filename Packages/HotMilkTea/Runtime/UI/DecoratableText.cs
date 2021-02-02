// zlib/libpng License
//
// Copyright (c) 2020 Sinoa
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

using UnityEngine;
using UnityEngine.UI;

namespace IceMilkTea.UI
{
    /// <summary>
    /// 一文字ずつ装飾制御が可能となるテキストUI基底クラスです
    /// </summary>
    public abstract class DecoratableText : ImgAbstractUI
    {
        // メンバ変数定義
        private string text;



        /// <summary>
        /// レンダリングに用いるメインテクスチャを返します
        /// </summary>
        public override Texture mainTexture => GetFontTexture();


        /// <summary>
        /// テキストを取得設定します
        /// </summary>
        public string Text { get => text; set => SetText(value); }



        #region General
        /// <summary>
        /// 指定された文字列を描画するテキストとして設定します。
        /// 設定する文字列が前回と変化がない場合は何もしません。
        /// </summary>
        /// <param name="text">設定する文字列</param>
        private void SetText(string text)
        {
            // 現在の文字列と変化がない場合は何もしない
            if (this.text != text) return;


            // 変化があったイベントを起こして変化を受け入れる
            OnTextChanged(this.text, text);
            this.text = text;


            // このグラフィックオブジェクトが汚れたことをマーク
            SetAllDirty();
        }


        /// <summary>
        /// フォントテクスチャを取得します。
        /// </summary>
        /// <returns>有効なフォントテクスチャを返しますが、フォントが設定されていない場合は基底のメインテクスチャを返します</returns>
        private Texture GetFontTexture()
        {
            // フォントが設定されているならフォントテクスチャを返す
            //return fontData.Font != null ? fontData.Font.material.mainTexture : base.mainTexture;
            throw new System.NotImplementedException();
        }
        #endregion


        #region DecoratableText EventHandler
        /// <summary>
        /// Text プロパティに変化があった時の処理を行います。
        /// </summary>
        /// <param name="oldText">変化する前の文字列</param>
        /// <param name="newText">変化した後の文字列</param>
        protected virtual void OnTextChanged(string oldText, string newText)
        {
            // 何もしない
        }
        #endregion
    }
}