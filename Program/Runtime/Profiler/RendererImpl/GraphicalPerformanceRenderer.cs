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
            builtinFont = Resources.Load<Font>("Fonts/FOT-NewCinemaBStd-D");
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

        private void SetQuads(Vector3 vertex, Color color)
        {

        }
    }
}