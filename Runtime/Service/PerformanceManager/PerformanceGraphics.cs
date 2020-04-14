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

namespace IceMilkTea.Service
{
    public class PerformanceGraphics
    {
        private Material mat = new Material(Shader.Find("GUI/Text Shader"));
        private Font font;
        private Vector2 virtualResolution;
        private Matrix4x4 toScreenMatrix;



        public PerformanceGraphics()
        {
            font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            virtualResolution = new Vector2(720.0f, 1280.0f);

            var scaleRate = Screen.width / virtualResolution.x;
            var finalResolution = virtualResolution * scaleRate;
            finalResolution = new Vector2(1.0f / finalResolution.x, 1.0f / finalResolution.y);
            toScreenMatrix = Matrix4x4.Scale(new Vector3(finalResolution.x, finalResolution.y, 1.0f));
            Debug.Log(toScreenMatrix * new Vector3(710.0f, 1270.0f, 0.0f));
        }


        public void Render()
        {
            var scaleRate = Screen.width / virtualResolution.x;
            var finalResolution = virtualResolution * scaleRate;
            toScreenMatrix = Matrix4x4.Scale(new Vector3(finalResolution.x, finalResolution.y, 1.0f));


            GL.PushMatrix();
            GL.LoadOrtho();

            // バーの描画で必要な最低限の処理箇所
            {
                mat.SetPass(0);
                GL.Begin(GL.QUADS);
                GL.Color(new Color(0.0f, 0.0f, 0.0f, 0.5f));
                GL.Vertex(toScreenMatrix * new Vector3(10.0f, 10.0f, 0.0f));
                GL.Vertex(toScreenMatrix * new Vector3(10.0f, 1270.0f, 0.0f));
                GL.Vertex(toScreenMatrix * new Vector3(710.0f, 1270.0f, 0.0f));
                GL.Vertex(toScreenMatrix * new Vector3(710.0f, 10.0f, 0.0f));
                GL.End();
            }

            // テキスト描画で必要な最低限の処理箇所
            {
                font.material.SetPass(0);
                GL.Begin(GL.TRIANGLES);
                GL.End();
            }


            GL.PopMatrix();
        }
    }
}