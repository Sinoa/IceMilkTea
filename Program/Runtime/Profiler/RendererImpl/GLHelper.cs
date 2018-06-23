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
    class GLHelper
    {
        /// <summary>
        /// 四角形の図形描画を行います。
        /// </summary>
        /// <param name="position">描画するポジション</param>
        /// <param name="color">色</param>
        /// <param name="width">横幅</param>
        /// <param name="height">縦幅</param>
        /// <param name="screenSize">画面サイズ</param>
        public static void DrawBar(Vector3 position, Color color, float width, float height, Vector2 screenSize)
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
        }

        /// <summary>
        /// 文字を描画するためのCharacterHelperインスタンスを取得します。
        /// </summary>
        /// <param name="font">使用するフォント</param>
        /// <param name="color">色</param>
        /// <param name="fontSize">フォントサイズ</param>
        /// <param name="screenSize">描画する画面のサイズ</param>
        /// <returns></returns>
        public static CharacterHelper CreateCharacterHelper(Font font, Color color, int fontSize, Vector2 screenSize)
        {
            return new CharacterHelper(font, color, fontSize, screenSize);
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

        /// <summary>
        /// 文字の描画を簡単に行うためのヘルパーです。
        /// </summary>
        public class CharacterHelper
        {
            private readonly Font font;
            private readonly int fontSize;
            private readonly Vector2 screenSize;
            private readonly Color color;

            public CharacterHelper(Font font, Color color, int fontSize, Vector2 screenSize)
            {
                this.font = font;
                this.fontSize = fontSize;
                this.screenSize = screenSize;
                this.color = color;
            }

            /// <summary>
            /// 文字列の描画を行います。
            /// </summary>
            /// <param name="characters">文字列</param>
            /// <param name="position">描画座標</param>
            /// <param name="scale">描画倍率</param>
            public void DrawCharacters(string characters, Vector3 position, float scale = 1)
            {
                var uvPosition = position / this.screenSize;
                Vector3 lastbl = uvPosition;
                Vector3 lasttl = uvPosition;
                Vector3 lasttr = uvPosition;
                Vector3 lastbr = uvPosition;

                for (var i = 0; i < characters.Length; i++)
                {
                    var character = characters[i];
                    CharacterInfo ci;
                    if (this.font.GetCharacterInfo(character, out ci, this.fontSize))
                    {
                        //文字の高さを設定
                        var bottom = uvPosition.y + ci.minY / screenSize.y * scale;
                        lastbl.y = lastbr.y = bottom;

                        var top = uvPosition.y + ci.maxY / screenSize.y * scale;
                        lasttl.y = lasttr.y = top;

                        //文字列分、右上と右下の頂点を横にずらす
                        var left = lastbl.x + ci.minX / screenSize.x * scale;
                        lastbl.x = lasttl.x = left;

                        var right = left + ci.glyphWidth / screenSize.x * scale;
                        lasttr.x = lastbr.x = right;

                        this.DrawCharacter(character, ci, lastbl, lasttl, lasttr, lastbr);

                        //最後に描画した文字の右側のx座標を、次の文字の左側のx座標とする
                        lastbl.x = lasttl.x = lastbr.x;
                    }
                    else
                        Debug.LogError($"Font Not Found, Character:{character}, FontSize:{this.fontSize}");


                }
            }

            /// <summary>
            /// 1文字の描画を行います。
            /// </summary>
            /// <param name="character">文字</param>
            /// <param name="ci">文字情報</param>
            /// <param name="vBottomLeft">頂点座標(左下)</param>
            /// <param name="vTopLevt">頂点座標(左上)</param>
            /// <param name="vTopRight">頂点座標(右上)</param>
            /// <param name="vBottomRight">頂点座標(右下)</param>
            private void DrawCharacter(char character, CharacterInfo ci, Vector3 vBottomLeft, Vector3 vTopLeft, Vector3 vTopRight, Vector3 vBottomRight)
            {
                SetVertex(color, ci.uvBottomLeft, vBottomLeft);
                SetVertex(color, ci.uvTopLeft, vTopLeft);
                SetVertex(color, ci.uvTopRight, vTopRight);
                SetVertex(color, ci.uvBottomRight, vBottomRight);
            }
        }
    }
}