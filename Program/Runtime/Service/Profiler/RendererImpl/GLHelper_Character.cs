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

namespace IceMilkTea.Profiler
{
    /// <summary>
    /// GLクラスを使った描画を簡単に行うためのヘルパーです。
    /// </summary>
    internal partial class GLHelper
    {
        /// <summary>
        /// 文字の描画を簡単に行うためのヘルパーです。
        /// </summary>
        public class CharacterHelper
        {
            private readonly Font font;
            private readonly int fontSize;
            private readonly Vector2 screenSize;

            /// <summary>
            /// CharacterHelperのコンストラクタです。
            /// </summary>
            /// <param name="font">描画するフォント</param>
            /// <param name="fontSize">フォントサイズ</param>
            /// <param name="screenSize">描画する画面のサイズ</param>
            public CharacterHelper(Font font, int fontSize, Vector2 screenSize)
            {
                this.font = font;
                this.fontSize = fontSize;
                this.screenSize = screenSize;
            }

            /// <summary>
            /// 文字列の描画を行います。
            /// </summary>
            /// <param name="characters">文字列</param>
            /// <param name="position">描画座標</param>
            /// <param name="color">描画する色</param>
            /// <param name="scale">描画倍率</param>
            /// <returns>描画した文字列の右側のx座標</returns>
            public float DrawString(string characters, Vector3 position, Color color, float scale = 1)
            {
                if (characters.Length <= 0)
                    return position.x;

                var uvPosition = position / this.screenSize;
                GLCharacterInfo lastDrawCharacterInfo;
                var lastDrawCharacterXposition = uvPosition.x;

                for (var i = 0; i < characters.Length; i++)
                {
                    var character = characters[i];

                    lastDrawCharacterInfo = this.GetGLCharacterInfo(character, uvPosition.y, lastDrawCharacterXposition, scale);

                    if (lastDrawCharacterInfo.IsValidInfo)
                    {
                        this.DrawCharacter(color, lastDrawCharacterInfo);

                        //最後に描画した文字の右側のx座標を保持しておく
                        lastDrawCharacterXposition = lastDrawCharacterInfo.DrawUvBottomRight.x;
                    }
                }
                return lastDrawCharacterXposition * this.screenSize.x;
            }

            /// <summary>
            /// 数値(double)の描画を行います
            /// </summary>
            /// <param name="value">値</param>
            /// <param name="position">描画座標</param>
            /// <param name="color">描画する色</param>
            /// <param name="scale">描画倍率</param>
            /// <returns>描画した数値の右側のx座標</returns>
            public float DrawDouble(double value, Vector3 position, Color color, float scale = 1)
            {
                var uvPosition = position / this.screenSize;
                GLCharacterInfo lastDrawCharacterInfo;
                var lastDrawCharacterXposition = uvPosition.x;

                //小数点第4位まで表示
                var drawNum = (int)(value * 10000);
                var digits = Math.Max(this.GetDigits(drawNum), 5);

                //-の描画
                if (value < 0)
                {
                    lastDrawCharacterInfo = this.GetGLCharacterInfo('-', uvPosition.y, lastDrawCharacterXposition, scale);
                    if (lastDrawCharacterInfo.IsValidInfo)
                    {
                        this.DrawCharacter(color, lastDrawCharacterInfo);
                        lastDrawCharacterXposition = lastDrawCharacterInfo.DrawUvBottomRight.x;
                    }
                }

                var lastDrawDigits = digits;
                for (var i = digits; i > 0; i--)
                {
                    //.の描画
                    if (lastDrawDigits == 5 && i == 4)
                    {
                        lastDrawCharacterInfo = this.GetGLCharacterInfo('.', uvPosition.y, lastDrawCharacterXposition, scale);
                        if (lastDrawCharacterInfo.IsValidInfo)
                        {
                            this.DrawCharacter(color, lastDrawCharacterInfo);
                            lastDrawCharacterXposition = lastDrawCharacterInfo.DrawUvBottomRight.x;
                        }
                    }

                    var powNum = (int)Math.Pow(10, i - 1);
                    var currentDrawNum = (drawNum / powNum);
                    var currentDrawCharacter = (char)(currentDrawNum + 48);//'0'を加算する

                    lastDrawCharacterInfo = this.GetGLCharacterInfo(currentDrawCharacter, uvPosition.y, lastDrawCharacterXposition, scale);
                    if (lastDrawCharacterInfo.IsValidInfo)
                    {
                        this.DrawCharacter(color, lastDrawCharacterInfo);
                        lastDrawCharacterXposition = lastDrawCharacterInfo.DrawUvBottomRight.x;
                    }

                    drawNum -= currentDrawNum * powNum;
                    lastDrawDigits = i;
                }

                return lastDrawCharacterXposition * this.screenSize.x;
            }

            /// <summary>
            /// 数値の桁数を取得する
            /// </summary>
            /// <param name="value">値</param>
            /// <returns>桁数</returns>
            private int GetDigits(int value)
            {
                var result = 1;
                var tmp = Math.Abs(value);

                while (true)
                {
                    tmp /= 10;
                    if (tmp == 0)
                        break;

                    result++;
                }
                return result;
            }

            /// <summary>
            /// 直前に描画した文字情報を基に、次に表示する文字情報を取得します
            /// </summary>
            /// <param name="character">描画する文字</param>
            /// <param name="charactersYposition">描画する文字のY座標</param>
            /// <param name="lastDrawCharacterXposition">直前に描画した文字の右側のx座標</param>
            /// <param name="scale">文字の描画倍率</param>
            /// <returns>描画する文字の頂点座標等の情報</returns>
            private GLCharacterInfo GetGLCharacterInfo(char character, float charactersYposition, float lastDrawCharacterXposition, float scale)
            {
                CharacterInfo ci;
                if (this.font.GetCharacterInfo(character, out ci, this.fontSize))
                {
                    //文字の高さを設定
                    var bottom = charactersYposition + ci.minY / screenSize.y * scale;
                    var top = charactersYposition + ci.maxY / screenSize.y * scale;

                    //文字列分、右上と右下の頂点を横にずらす
                    var left = lastDrawCharacterXposition + ci.minX / screenSize.x * scale;
                    var right = left + ci.glyphWidth / screenSize.x * scale;

                    return new GLCharacterInfo(ci, new Vector3(left, bottom), new Vector3(left, top), new Vector3(right, top), new Vector3(right, bottom));
                }
                else
                {
                    Debug.LogError($"Font Not Found, Character:{character}, FontSize:{this.fontSize}");
                    return new GLCharacterInfo();
                }

            }

            /// <summary>
            /// 1文字の描画を行います。
            /// </summary>
            /// <param name="color">描画する文字の色</param>
            /// <param name="drawCharacterInfo">描画する文字情報</param>
            private void DrawCharacter(Color color, GLCharacterInfo drawCharacterInfo)
            {
                SetVertex(color, drawCharacterInfo.CharacterInfo.uvBottomLeft, drawCharacterInfo.DrawUvBottomLeft);
                SetVertex(color, drawCharacterInfo.CharacterInfo.uvTopLeft, drawCharacterInfo.DrawUvTopLeft);
                SetVertex(color, drawCharacterInfo.CharacterInfo.uvTopRight, drawCharacterInfo.DrawUvTopRight);
                SetVertex(color, drawCharacterInfo.CharacterInfo.uvBottomRight, drawCharacterInfo.DrawUvBottomRight);
            }

            /// <summary>
            /// 1文字を描画するための情報を持ちます。
            /// </summary>
            private struct GLCharacterInfo
            {
                /// <summary>
                /// GLCharacterInfoのコンストラクタです。
                /// </summary>
                /// <param name="characterInfo">Fontクラスから取得した、描画する文字のCharacterInfo</param>
                /// <param name="drawUvBottomLeft">文字の左下の座標</param>
                /// <param name="drawUvTopLeft">文字の左上の座標</param>
                /// <param name="drawUvTopRight">文字の右上の座標</param>
                /// <param name="drawUvBottomRight">文字の右下の座標</param>
                public GLCharacterInfo(CharacterInfo characterInfo, Vector3 drawUvBottomLeft, Vector3 drawUvTopLeft, Vector3 drawUvTopRight, Vector3 drawUvBottomRight)
                {
                    this.CharacterInfo = characterInfo;
                    this.DrawUvBottomLeft = drawUvBottomLeft;
                    this.DrawUvTopLeft = drawUvTopLeft;
                    this.DrawUvTopRight = drawUvTopRight;
                    this.DrawUvBottomRight = drawUvBottomRight;
                    this.IsValidInfo = true;
                }

                public CharacterInfo CharacterInfo { get; private set; }
                public Vector3 DrawUvBottomLeft { get; private set; }
                public Vector3 DrawUvTopLeft { get; private set; }
                public Vector3 DrawUvTopRight { get; private set; }
                public Vector3 DrawUvBottomRight { get; private set; }

                public bool IsValidInfo { get; private set; }
            }
        }
    }
}