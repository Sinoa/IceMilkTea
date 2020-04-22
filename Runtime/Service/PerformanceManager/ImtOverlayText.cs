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
using UnityEngine;

namespace IceMilkTea.Service
{
    public class ImtOverlayText
    {
        private Font font;
        private int fontSize;
        private char[] integerCharaBuffer;


        public ImtOverlayText(Font font, int fontSize)
        {
            this.font = font;
            this.fontSize = fontSize;
            integerCharaBuffer = new char[20];
        }


        public void Begin(string requestText)
        {
            font.RequestCharactersInTexture(requestText, fontSize);
            font.material.SetPass(0);
            GL.Begin(GL.QUADS);
        }


        public void End()
        {
            GL.End();
        }


        public void Render(string text, float size, Vector2 position, Color color)
        {
            var nextXPos = 0.0f;
            GL.Color(color);
            for (int i = 0; i < text.Length; ++i)
            {
                var chara = text[i];
                Render(chara, ref nextXPos, size, position);
            }
        }


        public void Render(double number, float size, Vector2 position, Color color)
        {
            var length = CreateNumberCharacters(number);
            var nextXPos = 0.0f;
            GL.Color(color);
            for (int i = 0; i < length; ++i)
            {
                Render(integerCharaBuffer[i], ref nextXPos, size, position);
            }
        }


        private void Render(char chara, ref float nextXPos, float size, Vector2 position)
        {
            font.GetCharacterInfo(chara, out var info, fontSize);


            var scale = size / fontSize;
            var minX = (info.minX + nextXPos) * scale + position.x;
            var maxX = (info.maxX + nextXPos) * scale + position.x;
            var minY = info.minY * scale + position.y;
            var maxY = info.maxY * scale + position.y;


            GL.TexCoord(info.uvBottomLeft);
            GL.Vertex(new Vector3(minX, minY, 0.0f));


            GL.TexCoord(info.uvBottomRight);
            GL.Vertex(new Vector3(maxX, minY, 0.0f));


            GL.TexCoord(info.uvTopRight);
            GL.Vertex(new Vector3(maxX, maxY, 0.0f));


            GL.TexCoord(info.uvTopLeft);
            GL.Vertex(new Vector3(minX, maxY, 0.0f));


            nextXPos += info.advance;
        }


        private int CreateNumberCharacters(double number)
        {
            var length = CreateNumberCharacters((long)(number * 1000));
            Array.Reverse(integerCharaBuffer, 0, length);
            return length;
        }


        private int CreateNumberCharacters(long number)
        {
            if (number == 0)
            {
                integerCharaBuffer[0] = '0';
                integerCharaBuffer[1] = '0';
                integerCharaBuffer[2] = '0';
                integerCharaBuffer[3] = '.';
                integerCharaBuffer[4] = '0';
                return 5;
            }


            var belowThousand = number < 1000;


            var count = 0;
            while (number > 0)
            {
                var digitChar = (char)((number % 10) + 48);
                number /= 10;


                if (count > 0 && (count + 1) % 4 == 0)
                {
                    if (count == 3)
                    {
                        integerCharaBuffer[count++] = '.';
                    }
                    else
                    {
                        integerCharaBuffer[count++] = ',';
                    }
                }


                integerCharaBuffer[count++] = digitChar;
            }


            if (belowThousand)
            {
                integerCharaBuffer[3] = '.';
                integerCharaBuffer[4] = '0';
                return 5;
            }


            return count;
        }
    }
}