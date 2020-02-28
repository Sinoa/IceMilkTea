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
using System.Collections.Generic;
using IceMilkTea.Core;
using UnityEngine;

namespace IceMilkTea.Graphics
{
    /// <summary>
    /// Unityのフォントデータを制御するサービスクラスです
    /// </summary>
    public class ImtFontService : GameService
    {
        // 以下メンバ変数定義
        private readonly Dictionary<Font, List<IImtTextRendererMetaData>> rendererTable;
        private readonly Action<string, int, FontStyle> requestFunction;
        private Font currentControlFont;



        /// <summary>
        /// ImtFontService クラスのインスタンスを初期化します
        /// </summary>
        public ImtFontService()
        {
            // テーブルと関数参照の用意
            rendererTable = new Dictionary<Font, List<IImtTextRendererMetaData>>();
            requestFunction = RequestCharacters;
        }


        /// <summary>
        /// サービスの起動をします
        /// </summary>
        /// <param name="info">起動時情報を設定するオブジェクトへの参照</param>
        protected internal override void Startup(out GameServiceStartupInfo info)
        {
            // 起動情報の生成
            info = new GameServiceStartupInfo()
            {
                // 更新関数テーブルの初期化
                UpdateFunctionTable = new Dictionary<GameServiceUpdateTiming, System.Action>()
                {
                    { GameServiceUpdateTiming.PostLateUpdate, PostLateUpdate },
                }
            };
        }


        /// <summary>
        /// UnityのLateUpdate更新処理の後処理を行います
        /// </summary>
        private void PostLateUpdate()
        {
            // レコードの数分回る
            foreach (var record in rendererTable)
            {
                // 現在制御するフォントを設定してテキストレンダラメタデータの数分回る
                currentControlFont = record.Key;
                foreach (var metaData in record.Value)
                {
                    // メタデータにリクエストするべきテキスト情報を収集する関数を叩く
                    metaData.CollectDrawCharacterInfo(requestFunction);
                }
            }


            // フォントの参照を忘れる
            currentControlFont = null;
        }


        /// <summary>
        /// Unityのフォントに対して使用するテキストの情報元にテクスチャの生成をリクエストします
        /// </summary>
        /// <param name="text">使用するテキストの文字列</param>
        /// <param name="size">使用するテキストのサイズ</param>
        /// <param name="style">使用するテキストのスタイル</param>
        private void RequestCharacters(string text, int size, FontStyle style)
        {
            // 現在の制御下に置かれているフォントに対してテキストのテクスチャ生成をリクエストする
            currentControlFont.RequestCharactersInTexture(text, size, style);
        }


        /// <summary>
        /// サービスにテキスト描画メタデータを登録します
        /// </summary>
        /// <param name="metaData">登録するメタデータ</param>
        public void RegisterRendererMetaData(Font font, IImtTextRendererMetaData metaData)
        {
        }


        /// <summary>
        /// サービスからテキスト描画メタデータを解除します
        /// </summary>
        /// <param name="font"></param>
        /// <param name="metaData"></param>
        public void UnregisterRendererMetaData(Font font, IImtTextRendererMetaData metaData)
        {
        }
    }
}