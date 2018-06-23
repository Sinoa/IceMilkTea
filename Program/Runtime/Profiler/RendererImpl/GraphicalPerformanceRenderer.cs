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
        const int FontSize = 16;

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
            var pos = new Vector2(10, 10);
            var siz = new Vector2(620, 20);

            var npos = new Vector2(pos.x, screenSize.y - pos.y) / screenSize;
            var nsiz = siz / screenSize;

            var bl = new Vector3(npos.x, npos.y - nsiz.y, 0);
            var tl = new Vector3(npos.x, npos.y, 0);
            var tr = new Vector3(npos.x + nsiz.x, npos.y, 0);
            var br = new Vector3(npos.x + nsiz.x, npos.y - nsiz.y);


            CharacterInfo ci;
            builtinFont.GetCharacterInfo('0', out ci, FontSize);

            GL.PushMatrix();
            GL.LoadOrtho();

            //builtinFont.material.SetPass(0);
            barMaterial.SetPass(0);
            GL.Begin(GL.QUADS);


            GL.Color(Color.yellow);
            GL.TexCoord(Vector3.zero);
            GL.Vertex(bl);

            GL.Color(Color.yellow);
            GL.TexCoord(Vector3.zero);
            GL.Vertex(tl);

            GL.Color(Color.yellow);
            GL.TexCoord(Vector3.zero);
            GL.Vertex(tr);

            GL.Color(Color.yellow);
            GL.TexCoord(Vector3.zero);
            GL.Vertex(br);


            bl.y -= 0.1f;
            GL.Color(Color.blue);
            GL.TexCoord(Vector3.zero);
            GL.Vertex(bl);

            tl.y -= 0.1f;
            GL.Color(Color.blue);
            GL.TexCoord(Vector3.zero);
            GL.Vertex(tl);

            tr.y -= 0.1f;
            GL.Color(Color.blue);
            GL.TexCoord(Vector3.zero);
            GL.Vertex(tr);

            br.y -= 0.1f;
            GL.Color(Color.blue);
            GL.TexCoord(Vector3.zero);
            GL.Vertex(br);



            GL.End();
            GL.PopMatrix();
        }

        public static class GLHelper
        {
            public static void DrawBar(Color color, params Vector3[] vertex)
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

                public void DrawCharacters(string characters, Vector3 position)
                {
                    var uvPosition = position / this.screenSize;
                    Vector3 lastbl = uvPosition;
                    Vector3 lasttl = uvPosition;
                    Vector3 lasttr = uvPosition;
                    Vector3 lastbr = uvPosition;

                    //高さを確保
                    lasttl.y += this.fontSize / screenSize.y;
                    lasttr.y += this.fontSize / screenSize.y;

                    for (var i = 0; i < characters.Length; i++)
                    {
                        var character = characters[i];
                        CharacterInfo ci;
                        if (font.GetCharacterInfo(character, out ci, this.fontSize))
                        {
                            //文字列分、右上と右下の頂点を横にずらす
                            lasttr.x = lasttr.x + ci.advance / screenSize.x;
                            lastbr.x = lastbr.x + ci.advance / screenSize.x;

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