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
    /// IceMilkTea が提供するUI実装の抽象クラスです
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public abstract class ImgAbstractUI : MaskableGraphic, ILayoutElement
    {
        // メンバ変数定義
        private Rect cachedBoundingRect;



        /// <summary>
        /// 表示に必要な横幅を返します。
        /// </summary>
        public float preferredWidth => cachedBoundingRect.width;


        /// <summary>
        /// 表示に必要な縦幅を返します。
        /// </summary>
        public float preferredHeight => cachedBoundingRect.height;


        /// <summary>
        /// 最小の横幅を返します。常に 0.0 を返します。
        /// </summary>
        public float minWidth => 0.0f;


        /// <summary>
        /// 最小の縦幅を返します。常に 0.0 を返します
        /// </summary>
        public float minHeight => 0.0f;


        /// <summary>
        /// 残りのサイズに対して割り当てる割合。常に -1.0(disable) を返します
        /// </summary>
        public float flexibleWidth => -1.0f;


        /// <summary>
        /// 残りのサイズに対して割り当てる割合。常に -1.0(disable) を返します
        /// </summary>
        public float flexibleHeight => -1.0f;


        /// <summary>
        /// レイアウト計算の優先順位。常に 0 を返します
        /// </summary>
        public int layoutPriority => 0;



        /// <summary>
        /// UIの境界矩形を設定します
        /// </summary>
        /// <param name="rect">設定する境界の矩形</param>
        protected void SetBoundingRect(Rect rect)
        {
            // そのまま受け入れる
            cachedBoundingRect = rect;
        }


        /// <summary>
        /// 横幅のレイアウト計算を行いますが、この関数は何もしません。
        /// </summary>
        public virtual void CalculateLayoutInputHorizontal()
        {
        }


        /// <summary>
        /// 縦幅のレイアウト計算を行いますが、この関数は何もしません。
        /// </summary>
        public virtual void CalculateLayoutInputVertical()
        {
        }
    }
}