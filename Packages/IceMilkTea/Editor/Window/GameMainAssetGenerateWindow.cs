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
using System.Linq;
using IceMilkTeaEditor.Utility;
using UnityEditor;
using UnityEngine;

namespace IceMilkTeaEditor.Window
{
    /// <summary>
    /// GameMainのアセットをお手軽に生成するためのウィンドウを提供するクラスです
    /// </summary>
    public class GameMainAssetGenerateWindow : EditorWindow
    {
        /// <summary>
        /// 生成GameMainメニュー一覧に表示するための内容を保持するクラスです
        /// </summary>
        private class GameMainMenuItem
        {
            /// <summary>
            /// GameMainの名前
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// GameMainの型
            /// </summary>
            public Type Type { get; set; }
        }



        // メンバ変数定義
        private GameMainMenuItem[] menuItems;



        /// <summary>
        /// ウィンドウを開きます
        /// </summary>
        public static void OpenWindow()
        {
            // インスタンスを取得すると開かれる
            GetWindow<GameMainAssetGenerateWindow>();
        }


        /// <summary>
        /// エディタウィンドウのインスタンス初期化を行います
        /// </summary>
        private void Awake()
        {
            // タイトル設定
            titleContent = new GUIContent("GameMainアセット作成");


            // アセット生成可能なGameMain配列を貰ってメニューアイテム配列を作る
            menuItems = GameMainAssetUtility.GetCreatableGameMainTypes()
                .Select(x => new GameMainMenuItem()
                {
                    Type = x,
                    Name = x.Name,
                })
                .ToArray();
        }


        /// <summary>
        /// エディタウィンドウのGUIのハンドリングと再描画を行います
        /// </summary>
        private void OnGUI()
        {
            // メニューの数分回る
            foreach (var menuItem in menuItems)
            {
                // ボタンを出して押されたのなら
                if (GUILayout.Button($"Create {menuItem.Name}"))
                {
                    // アセットの生成をする
                    CreateGameMainAsset(menuItem);
                }
            }
        }


        /// <summary>
        /// 指定されたメニューアイテムが保持するGameMainの型からアセットを生成します
        /// </summary>
        /// <param name="menuItem">生成するべきGameMainの型を持つメニューアイテムへの参照</param>
        private void CreateGameMainAsset(GameMainMenuItem menuItem)
        {
            // 保存するアセットの場所を教えてもらうために保存ダイアログを出すが、キャンセルされたのなら
            var savePath = EditorUtility.SaveFilePanelInProject("保存するアセットの場所を指定", "GameMain", "asset", "GameMainアセットを保存する場所を指定して下さい");
            if (string.IsNullOrWhiteSpace(savePath))
            {
                // 何事もなかったかのように修了
                return;
            }


            // アセットの保存をする
            GameMainAssetUtility.CreateGameMainAsset(menuItem.Type, savePath);


            // 保存したことを通知する
            EditorUtility.DisplayDialog("完了", "GameMainアセットの生成をしました", "OK");
        }
    }
}