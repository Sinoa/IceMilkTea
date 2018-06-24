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
using static IceMilkTea.Profiler.GLHelper;

namespace IceMilkTea.Profiler
{
    /// <summary>
    /// パフォーマンス計測結果をユーザーが視認出来るディスプレイへ、グラフィカルにレンダリングするクラスです
    /// </summary>
    public class GraphicalPerformanceRenderer : PerformanceRenderer
    {
        private const int FontSize = 20; //フォントサイズ
        private const float RowHeight = 20; //テキストやバーを表示する行の高さ
        private const float BarMaxWidthPercentage = 80; //バーの最大横幅(画面サイズに対する%)
        private const float FontMarginLeftPercentage = 2; //テキストの左側の余白
        private const float BarMarginLeftPercentage = 2;//バーの左側の余白

        private const float MaxMillisecondPerFrame = 33; //バーで計測できる1フレーム毎の実行時間(ミリ秒)
        private const int MaxValueCacheMilliseconds = 1000; //直近の最大値をどれだけの時間キャッシュするか(ミリ秒)

        // メンバ変数宣言
        private UnityStandardLoopProfileResult result;
        private Font builtinFont;
        private Material barMaterial;
        private Vector2 screenSize;
        private float fontMarginLeft;
        private float barMarginLeft;
        private float barMaxWidth;
        private float textScale;

        private MaxDoubleValueCache updateResultCache;
        private MaxDoubleValueCache lateUpdateResultCache;
        private MaxDoubleValueCache renderingResultCache;
        private CharacterHelper characterHelper;

        public GraphicalPerformanceRenderer()
        {
            this.builtinFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
            this.builtinFont.RequestCharactersInTexture("0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ-&_=:;%()[]?{}|/.,", FontSize);
            this.barMaterial = new Material(Shader.Find("GUI/Text Shader"));
            this.screenSize = new Vector2(Screen.width, Screen.height);

            this.fontMarginLeft = FontMarginLeftPercentage / 100 * screenSize.x;
            this.barMarginLeft = BarMarginLeftPercentage / 100 * screenSize.x;
            this.barMaxWidth = BarMaxWidthPercentage / 100 * screenSize.x;
            this.textScale = RowHeight / FontSize;

            this.updateResultCache = new MaxDoubleValueCache(MaxValueCacheMilliseconds, float.MinValue);
            this.lateUpdateResultCache = new MaxDoubleValueCache(MaxValueCacheMilliseconds, float.MinValue);
            this.renderingResultCache = new MaxDoubleValueCache(MaxValueCacheMilliseconds, float.MinValue);
            this.characterHelper = GLHelper.CreateCharacterHelper(this.builtinFont, FontSize, this.screenSize);
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
            this.updateResultCache.Update(this.result.UpdateTime);
            this.lateUpdateResultCache.Update(this.result.LateUpdateTime);
            this.renderingResultCache.Update(this.result.RenderingTime);

            GL.PushMatrix();
            GL.LoadOrtho();


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
            //1段目
            var lastXposition = this.characterHelper.DrawString("Update:", new Vector3(this.fontMarginLeft, screenSize.y - row1Top), Color.black, this.textScale);
            lastXposition = this.characterHelper.DrawDouble(this.updateResultCache.Value, new Vector3(lastXposition, screenSize.y - row1Top), Color.black, this.textScale);
            lastXposition = this.characterHelper.DrawString("ms", new Vector3(lastXposition, screenSize.y - row1Top), Color.black, this.textScale);

            //2段目
            lastXposition = this.characterHelper.DrawString("LateUpdate:", new Vector3(this.fontMarginLeft, screenSize.y - row2Top), Color.black, this.textScale);
            lastXposition = this.characterHelper.DrawDouble(this.lateUpdateResultCache.Value, new Vector3(lastXposition, screenSize.y - row2Top), Color.black, this.textScale);
            lastXposition = this.characterHelper.DrawString("ms", new Vector3(lastXposition, screenSize.y - row2Top), Color.black, this.textScale);

            //3段目
            lastXposition = this.characterHelper.DrawString("RenderingTime:", new Vector3(this.fontMarginLeft, screenSize.y - row3Top), Color.black, this.textScale);
            lastXposition = this.characterHelper.DrawDouble(this.renderingResultCache.Value, new Vector3(lastXposition, screenSize.y - row3Top), Color.black, this.textScale);
            lastXposition = this.characterHelper.DrawString("ms", new Vector3(lastXposition, screenSize.y - row3Top), Color.black, this.textScale);
            GL.End();

            GL.PopMatrix();
        }


        /// <summary>
        /// 指定した行のy座標を返します。
        /// </summary>
        /// <param name="row">行番号</param>
        private float GetMarginTop(int row)
        {
            return row * (RowHeight + 10);//行の高さ+α
        }
    }
}