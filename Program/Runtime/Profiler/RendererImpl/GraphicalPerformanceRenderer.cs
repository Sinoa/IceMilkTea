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
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace IceMilkTea.Profiler
{
    /// <summary>
    /// パフォーマンス計測結果をユーザーが視認出来るディスプレイへ、グラフィカルにレンダリングするクラスです
    /// </summary>
    public class GraphicalPerformanceRenderer : PerformanceRenderer
    {
        private const int FontSize = 20; //フォントサイズ
        private const float RowHeight = 20; //1行のサイズ。テキストとゲージはこの高さで描画される。
        private const float BarMaxWidthPercentage = 80; //バーの最大横幅(画面サイズに対する%)
        private const float FontMarginLeftPercentage = 2; //テキストの左側の余白
        private const float BarMarginLeftPercentage = 8;//バーの左側の余白

        private const float MaxMillisecondPerFrame = 33; //バーで計測できる1フレーム毎の実行時間(ミリ秒)
        private const int MaxValueCacheMilliseconds = 1000; //直近の最大値をどれだけの時間キャッシュするか(ミリ秒)

        // メンバ変数宣言
        private UnityStandardLoopProfileResult result;
        private Font builtinFont;
        private Material barMaterial;
        private Material fontMaterial;
        private Vector2 screenSize;
        private float fontMarginLeft;
        private float barMarginLeft;
        private float barMaxWidth;
        private float textScale;

        private MaxDoubleValueCache updateResultCache;
        private MaxDoubleValueCache lateUpdateResultCache;
        private MaxDoubleValueCache renderingResultCache;

        public GraphicalPerformanceRenderer()
        {
            this.builtinFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
            this.builtinFont.RequestCharactersInTexture("0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ-&_=:;%()[]?{}|/.,", FontSize);

            this.barMaterial = new Material(Shader.Find("GUI/Text Shader"));
            this.fontMaterial = new Material(Shader.Find("UI/Default"));

            this.screenSize = new Vector2(Screen.width, Screen.height);

            this.fontMarginLeft = FontMarginLeftPercentage / 100 * screenSize.x;
            this.barMarginLeft = BarMarginLeftPercentage / 100 * screenSize.x;
            this.barMaxWidth = BarMaxWidthPercentage / 100 * screenSize.x;
            this.textScale = RowHeight / FontSize;

            this.updateResultCache = new MaxDoubleValueCache(MaxValueCacheMilliseconds, float.MinValue, DateTime.Now.Ticks);
            this.lateUpdateResultCache = new MaxDoubleValueCache(MaxValueCacheMilliseconds, float.MinValue, DateTime.Now.Ticks);
            this.renderingResultCache = new MaxDoubleValueCache(MaxValueCacheMilliseconds, float.MinValue, DateTime.Now.Ticks);
        }

        /// <summary>
        /// 出力の準備を行います
        /// </summary>
        /// <param name="profileFetchResults">パフォーマンスモニタから渡されるすべての計測結果の配列</param>
        public override void Begin(ProfileFetchResult[] profileFetchResults)
        {
            if (this.result == null)
            {
                // プロファイル結果を覚える
                this.result = profileFetchResults.First(x => x is UnityStandardLoopProfileResult) as UnityStandardLoopProfileResult;
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
            this.updateResultCache.Update(this.result.UpdateTime, DateTime.Now.Ticks);
            this.lateUpdateResultCache.Update(this.result.LateUpdateTime, DateTime.Now.Ticks);
            this.renderingResultCache.Update(this.result.RenderingTime, DateTime.Now.Ticks);

            GL.PushMatrix();
            GL.LoadOrtho();

            var characterHelper = GLHelper.CreateCharacterHelper(builtinFont, Color.black, FontSize, screenSize);

            //1段目:Update
            var row1Top = GetMarginTop(1);

            //2段目:LateUpdate
            var row2Top = GetMarginTop(2);

            //3段目:Rendering
            var row3Top = GetMarginTop(3);

            //バー
            this.barMaterial.SetPass(0);
            GL.Begin(GL.QUADS);
            GLHelper.DrawBar(new Vector3(this.barMarginLeft, screenSize.y - row1Top), Color.yellow, (float)(this.updateResultCache.Value / MaxMillisecondPerFrame * this.barMaxWidth), RowHeight, this.screenSize);
            GLHelper.DrawBar(new Vector3(this.barMarginLeft, screenSize.y - row2Top), Color.blue, (float)(this.lateUpdateResultCache.Value / MaxMillisecondPerFrame * this.barMaxWidth), RowHeight, this.screenSize);
            GLHelper.DrawBar(new Vector3(this.barMarginLeft, screenSize.y - row3Top), Color.green, (float)(this.renderingResultCache.Value / MaxMillisecondPerFrame * this.barMaxWidth), RowHeight, this.screenSize);
            GL.End();

            //テキスト
            this.builtinFont.material.SetPass(0);
            GL.Begin(GL.QUADS);
            characterHelper.DrawCharacters($"Update:{this.updateResultCache.Value}ms", new Vector3(this.fontMarginLeft, screenSize.y - row1Top), this.textScale);
            characterHelper.DrawCharacters($"LateUpdate:{this.lateUpdateResultCache.Value}ms", new Vector3(this.fontMarginLeft, screenSize.y - row2Top), this.textScale);
            characterHelper.DrawCharacters($"RenderingTime:{this.renderingResultCache.Value}ms", new Vector3(this.fontMarginLeft, screenSize.y - row3Top), this.textScale);
            GL.End();

            GL.PopMatrix();
        }


        /// <summary>
        /// 指定したrowのy座標を返す
        /// </summary>
        /// <param name="row"></param>
        private float GetMarginTop(int row)
        {
            return row * (RowHeight + 10);//行の高さ+α
        }

        /// <summary>
        /// 最大値を更新するか、指定した時間が経過するまで値をキャッシュする
        /// </summary>
        private class MaxDoubleValueCache
        {
            private readonly int cacheMilliSeconds;
            long lastUpdateTick;
            double _value;
            public double Value { get { return this._value; } }

            public MaxDoubleValueCache(int cacheMilliSeconds, double initialValue, long currentTick)
            {
                this.cacheMilliSeconds = cacheMilliSeconds;
                this._value = initialValue;
                this.lastUpdateTick = currentTick;
            }

            public void Update(double value, long tick)
            {
                //前回の値より大きいか、Cache時間を超えたら更新
                if (this._value < value
                     || TimeSpan.FromTicks(tick - this.lastUpdateTick).TotalMilliseconds > this.cacheMilliSeconds)
                {
                    this.lastUpdateTick = tick;
                    this._value = value;
                    return;
                }
            }
        }

        private class GLHelper
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

            public static CharacterHelper CreateCharacterHelper(Font font, Color color, int fontSize, Vector2 screenSize)
            {
                return new CharacterHelper(font, color, fontSize, screenSize);
            }


            private static void SetVertex(Color color, Vector3 texCoord, Vector3 vertex)
            {
                GL.Color(color);
                GL.TexCoord(texCoord);
                GL.Vertex(vertex);
            }

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