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

using UnityEngine;

namespace IceMilkTea.Profiler
{
    /// <summary>
    /// GLクラスを使った描画を簡単に行うためのヘルパーです。
    /// </summary>
    internal partial class GLHelper
    {
        /// <summary>
        /// 四角形の図形描画を行います。
        /// </summary>
        /// <param name="position">描画するポジション</param>
        /// <param name="color">色</param>
        /// <param name="width">横幅</param>
        /// <param name="height">縦幅</param>
        /// <param name="screenSize">画面サイズ</param>
        /// <returns>バーの右側のx座標</returns>
        public static float DrawBar(Vector3 position, Color color, float width, float height, Vector2 screenSize)
        {
            var uvPosition = position / screenSize;
            var uvWidth = width / screenSize.x;
            var uvHeight = height / screenSize.y;

            var bl = uvPosition;
            var tl = new Vector3(uvPosition.x, uvPosition.y + uvHeight);
            var tr = new Vector3(uvPosition.x + uvWidth, uvPosition.y + uvHeight);
            var br = new Vector3(uvPosition.x + uvWidth, uvPosition.y);

            SetVertex(color, Vector3.zero, bl);
            SetVertex(color, Vector3.zero, tl);
            SetVertex(color, Vector3.zero, tr);
            SetVertex(color, Vector3.zero, br);

            return tr.x * screenSize.x;
        }

        /// <summary>
        /// 文字を描画するためのCharacterHelperインスタンスを取得します。
        /// </summary>
        /// <param name="font">使用するフォント</param>
        /// <param name="fontSize">フォントサイズ</param>
        /// <param name="screenSize">描画する画面のサイズ</param>
        /// <returns>CharacterHelperインスタンス</returns>
        public static CharacterHelper CreateCharacterHelper(Font font, int fontSize, Vector2 screenSize)
        {
            return new CharacterHelper(font, fontSize, screenSize);
        }

        /// <summary>
        /// 1つの頂点情報をセットします。
        /// </summary>
        /// <param name="color">色</param>
        /// <param name="texCoord">uv座標</param>
        /// <param name="vertex">頂点座標</param>
        private static void SetVertex(Color color, Vector3 texCoord, Vector3 vertex)
        {
            GL.Color(color);
            GL.TexCoord(texCoord);
            GL.Vertex(vertex);
        }
    }
}