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

using System.Linq;
using UnityEngine;

namespace IceMilkTea.Profiler
{
    /// <summary>
    /// パフォーマンス計測結果をユーザーが視認出来るディスプレイへ、グラフィカルにレンダリングするクラスです
    /// </summary>
    public class GraphicalPerformanceRenderer : PerformanceRenderer
    {
        const int FontSize = 20;
        const float BarHeight = 20;
        const float BarMaxWidth = 500;
        const float FontMarginLeft = 10;
        const float BarMarginLeft = 100;

        const float MaxMillisecondPerFrame = 33; //バーで計測できる1フレーム毎の実行時間(ミリ秒)

        // メンバ変数宣言
        private UnityStandardLoopProfileResult result;
        private Font builtinFont;
        private Material barMaterial;
        private Material fontMaterial;
        private Vector2 screenSize;

        public GraphicalPerformanceRenderer()
        {
            builtinFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
            builtinFont.RequestCharactersInTexture("0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ-&_=:;%()[]?{}|/.,", FontSize);

            barMaterial = new Material(Shader.Find("GUI/Text Shader"));
            fontMaterial = new Material(Shader.Find("UI/Default"));

            screenSize = new Vector2(640, 1280);
        }

        /// <summary>
        /// 出力の準備を行います
        /// </summary>
        /// <param name="profileFetchResults">パフォーマンスモニタから渡されるすべての計測結果の配列</param>
        public override void Begin(ProfileFetchResult[] profileFetchResults)
        {
            if (result == null)
            {
                // プロファイル結果を覚える
                result = profileFetchResults.First(x => x is UnityStandardLoopProfileResult) as UnityStandardLoopProfileResult;
            }
        }


        /// <summary>
        /// 出力を終了します
        /// </summary>
        public override void End()
        {

        }



        /// <summary>
        /// プロファイル結果をレンダリングします
        /// </summary>
        public override void Render()
        {
            var marginTop = BarHeight + 10; //バーの高さ+α

            GL.PushMatrix();
            GL.LoadOrtho();

            //builtinFont.material.SetPass(0);
            barMaterial.SetPass(0);
            GL.Begin(GL.QUADS);

            var fontHelper = GLHelper.CreateFontHelper(builtinFont, Color.black, FontSize, screenSize);

            //1段目:Update
            fontHelper.DrawCharacters(this.result.UpdateTime.ToString(), new Vector3(FontMarginLeft, screenSize.y - marginTop));
            GLHelper.DrawBar(new Vector3(BarMarginLeft, screenSize.y - marginTop), Color.yellow, (float)(this.result.UpdateTime / MaxMillisecondPerFrame * BarMaxWidth), BarHeight, screenSize);

            //2段目:LateUpdate
            marginTop += BarHeight + 10; //バーの高さ+α
            fontHelper.DrawCharacters(this.result.LateUpdateTime.ToString(), new Vector3(FontMarginLeft, screenSize.y - marginTop));
            GLHelper.DrawBar(new Vector3(BarMarginLeft, screenSize.y - marginTop), Color.blue, (float)(this.result.LateUpdateTime / MaxMillisecondPerFrame * BarMaxWidth), BarHeight, screenSize);

            //3段目:Rendering
            marginTop += FontSize + 10; //フォントサイズ+α
            fontHelper.DrawCharacters(this.result.RenderingTime.ToString(), new Vector3(FontMarginLeft, screenSize.y - marginTop));
            GLHelper.DrawBar(new Vector3(BarMarginLeft, screenSize.y - marginTop), Color.green, (float)(this.result.RenderingTime / MaxMillisecondPerFrame * BarMaxWidth), BarHeight, screenSize);

            GL.End();
            GL.PopMatrix();
        }

        public class GLHelper
        {
            public static void DrawBar(Vector3 position, Color color, float width, float height, Vector2 screenSize)
            {
                var uvPosition = position / screenSize;
                var uvWidth = width / screenSize.x;
                var uvHeight = height / screenSize.y;

                var bl = uvPosition;
                var tl = new Vector3(uvPosition.x, uvPosition.y + uvHeight);
                var tr = new Vector3(uvPosition.x + uvWidth, uvPosition.y + uvHeight);
                var br = new Vector3(uvPosition.x + uvWidth, uvPosition.y);

                DrawBar(color, bl, tl, tr, br);
            }
            private static void DrawBar(Color color, params Vector3[] vertex)
            {
                for (var i = 0; i < vertex.Length; i++)
                {
                    SetVertex(color, Vector3.zero, vertex[i]);
                }
            }

            public static FontHelper CreateFontHelper(Font font, Color color, int fontSize, Vector2 screenSize)
            {
                return new FontHelper(font, color, fontSize, screenSize);
            }


            private static void SetVertex(Color color, Vector3 texCoord, Vector3 vertex)
            {
                GL.Color(color);
                GL.TexCoord(texCoord);
                GL.Vertex(vertex);
            }

            public class FontHelper
            {
                private readonly Font font;
                private readonly int fontSize;
                private readonly Vector2 screenSize;
                private readonly Color color;

                public FontHelper(Font font, Color color, int fontSize, Vector2 screenSize)
                {
                    this.font = font;
                    this.fontSize = fontSize;
                    this.screenSize = screenSize;
                    this.color = color;
                }

                public void DrawCharacters(string characters, Vector3 position, float scale = 1)
                {
                    var uvPosition = position / this.screenSize;
                    Vector3 lastbl = uvPosition;
                    Vector3 lasttl = uvPosition;
                    Vector3 lasttr = uvPosition;
                    Vector3 lastbr = uvPosition;

                    //高さを確保
                    lasttl.y += this.fontSize / screenSize.y * scale;
                    lasttr.y += this.fontSize / screenSize.y * scale;

                    for (var i = 0; i < characters.Length; i++)
                    {
                        var character = characters[i];
                        CharacterInfo ci;
                        if (font.GetCharacterInfo(character, out ci, this.fontSize))
                        {
                            //文字列分、右上と右下の頂点を横にずらす
                            lasttr.x = lasttr.x + ci.advance / screenSize.x * scale;
                            lastbr.x = lastbr.x + ci.advance / screenSize.x * scale;

                            DrawCharacter(character, ci, lastbl, lasttl, lasttr, lastbr);

                            //最後に描画した文字の右上、右下の頂点を、次の文字の左上、左下とする
                            lastbl.x = lastbr.x;
                            lasttl.x = lastbr.x;
                        }
                        else
                            Debug.LogError($"Font Not Found, Character:{character}, FontSize:{this.fontSize}");


                    }
                }

                /// <summary>
                /// 左下から時計回りでvertexを渡す
                /// </summary>
                /// <param name="character"></param>
                /// <param name="vertex"></param>
                private void DrawCharacter(char character, CharacterInfo ci, Vector3 vBottomLeft, Vector3 vTopLevt, Vector3 vTopRight, Vector3 vBottomRight)
                {
                    SetVertex(color, ci.uvBottomLeft, vBottomLeft);
                    SetVertex(color, ci.uvTopLeft, vTopLevt);
                    SetVertex(color, ci.uvTopRight, vTopRight);
                    SetVertex(color, ci.uvBottomRight, vBottomRight);
                }
            }
        }
    }
}