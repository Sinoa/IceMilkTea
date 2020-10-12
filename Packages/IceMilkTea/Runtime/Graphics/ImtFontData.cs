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

using System;
using IceMilkTea.Core;
using UnityEngine;

namespace IceMilkTea.Graphics
{
    /// <summary>
    /// IceMilkTea としてフォントを取り扱うデータを保持した構造体です
    /// </summary>
    public struct ImtFontData : IEquatable<ImtFontData>
    {
        /// <summary>
        /// 使用するフォント本体
        /// </summary>
        public Font Font;

        /// <summary>
        /// フォントサイズ
        /// </summary>
        public int Size;

        /// <summary>
        /// フォントスタイル
        /// </summary>
        public FontStyle Style;



        /// <summary>
        /// ImtFontData 構造体のインスタンスを初期化します
        /// </summary>
        /// <param name="font">使用するフォント</param>
        /// <param name="size">使用するフォントのサイズ</param>
        /// <param name="style">使用するフォントのスタイル</param>
        public ImtFontData(Font font, int size, FontStyle style)
        {
            // 初期化をする
            Font = font;
            Size = size;
            Style = style;
        }


        /// <summary>
        /// ImtFontData の等価確認をします
        /// </summary>
        /// <param name="other">比較対象</param>
        /// <returns>等価の場合は true を、非等価の場合は false を返します</returns>
        public bool Equals(ImtFontData other)
        {
            // すべて比較した結果を返す
            return Font == other.Font && Size == other.Size && Style == other.Style;
        }


        /// <summary>
        /// ImtFontData の等価確認をします
        /// </summary>
        /// <param name="obj">比較対象</param>
        /// <returns>等価の場合は true を、非等価の場合は false を返します</returns>
        public override bool Equals(object obj) => obj is ImtFontData ? Equals((ImtFontData)obj) : false;


        /// <summary>
        /// ハッシュコードを取得します
        /// </summary>
        /// <returns>ハッシュコードを返します</returns>
        public override int GetHashCode() => Font.MergeHashCode(Size).MergeHashCode(Style);


        /// <summary>
        /// 等価演算子のオーバーロードです
        /// </summary>
        /// <param name="left">左の値</param>
        /// <param name="right">右の値</param>
        /// <returns>等価の結果を返します</returns>
        public static bool operator ==(ImtFontData left, ImtFontData right) => left.Equals(right);


        /// <summary>
        /// 非等価演算子のオーバーロードです
        /// </summary>
        /// <param name="left">左の値</param>
        /// <param name="right">右の値</param>
        /// <returns>非等価の結果を返します</returns>
        public static bool operator !=(ImtFontData left, ImtFontData right) => !left.Equals(right);
    }
}