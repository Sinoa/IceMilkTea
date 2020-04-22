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

using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace IceMilkTea.Service
{
    public class PerformanceGraphics
    {
        private ImtOverlaySimpleUI overlaySimpleUI;
        private ImtOverlayText overlayText;
        private string needString;
        private StringBuilder stringBuffer;
        private List<ImtTextReference> textReferenceList;
        private List<ImtNumberReference> numberReferenceList;
        private List<ImtSquareReference> squareReferenceList;



        public PerformanceGraphics()
        {
            overlaySimpleUI = new ImtOverlaySimpleUI(new Material(Shader.Find("GUI/Text Shader")));
            overlayText = new ImtOverlayText(Resources.GetBuiltinResource<Font>("Arial.ttf"), 25);
            textReferenceList = new List<ImtTextReference>();
            numberReferenceList = new List<ImtNumberReference>();
            squareReferenceList = new List<ImtSquareReference>();
            needString = string.Empty;
            stringBuffer = new StringBuilder();
        }


        public ImtTextReference CreateTextReference()
        {
            needString = null;
            var textReference = new ImtTextReference(x => { needString = null; stringBuffer.Append(x); });
            textReferenceList.Add(textReference);
            return textReference;
        }


        public ImtNumberReference CreateNumberReference()
        {
            var numberReference = new ImtNumberReference();
            numberReferenceList.Add(numberReference);
            return numberReference;
        }


        public ImtSquareReference CreateSquareReference()
        {
            var squareReference = new ImtSquareReference();
            squareReferenceList.Add(squareReference);
            return squareReference;
        }


        public void RemoveTextReference(ImtTextReference item)
        {
            needString = null;
            textReferenceList.Remove(item);
        }


        public void RemoveNumberReference(ImtNumberReference item)
        {
            numberReferenceList.Remove(item);
        }


        public void RemoveSquareReference(ImtSquareReference item)
        {
            squareReferenceList.Remove(item);
        }


        public void Render()
        {
            GL.PushMatrix();
            var sWidth = 720.0f;
            var sHeight = Screen.height * (sWidth / Screen.width);
            GL.LoadPixelMatrix(0.0f, sWidth, 0.0f, sHeight);
            GL.MultMatrix(Matrix4x4.identity);

            // バーの描画で必要な最低限の処理箇所
            overlaySimpleUI.Begin();
            foreach (var squareReference in squareReferenceList)
            {
                overlaySimpleUI.RenderBar(squareReference.Size, squareReference.Position, squareReference.Color);
            }
            overlaySimpleUI.End();

            // テキスト描画で必要な最低限の処理箇所
            needString = needString ?? $"1234567890., {stringBuffer}";
            overlayText.Begin(needString);
            foreach (var textReference in textReferenceList)
            {
                overlayText.Render(textReference.Text, textReference.Size, textReference.Position, textReference.Color);
            }
            foreach (var numberReference in numberReferenceList)
            {
                overlayText.Render(numberReference.Number, numberReference.Size, numberReference.Position, numberReference.Color);
            }
            overlayText.End();


            GL.PopMatrix();
        }
    }
}