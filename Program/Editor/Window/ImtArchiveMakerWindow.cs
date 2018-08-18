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

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace IceMilkTeaEditor.Window
{
    /// <summary>
    /// IceMilkTeaアーカイブをGUI上で作るためのウィンドウを提供するクラスです
    /// </summary>
    public class ImtArchiveMakerWindow : EditorWindow
    {
        // メンバ変数定義
        private List<string> filePathList;
        private Vector2 scrollPosition;



        /// <summary>
        /// ウィンドウを開きます
        /// </summary>
        public static void OpenWindow()
        {
            // ウィンドウインスタンスを取得する関数でウィンドウを開く
            GetWindow<ImtArchiveMakerWindow>();
        }


        /// <summary>
        /// エディタウィンドウのインスタンスを初期化します
        /// </summary>
        private void Awake()
        {
            // ウィンドウ設定をする
            titleContent = new GUIContent("ArchiveMaker");
            minSize = new Vector2(640.0f, 480.0f);


            // メンバ変数の初期化
            filePathList = new List<string>();
        }


        /// <summary>
        /// エディタウィンドウのGUIのハンドリングと描画を行います
        /// </summary>
        private void OnGUI()
        {
            // 水平レイアウトにする
            using (new GUILayout.HorizontalScope())
            {
                // インクルードするファイル選択ボタンを押されたら
                if (GUILayout.Button("インクルードするファイルを選択"))
                {
                    // 該当ハンドラ関数を呼ぶ
                    OnIncludeFileSelectButtonClick();
                }


                // フォルダごと選択するボタンを押されたら
                if (GUILayout.Button("フォルダから選択"))
                {
                    // 該当ハンドラ関数を呼ぶ
                    OnIncludeDirectorySelectButtonClick();
                }


                // ビルドボタンを押されたら
                if (GUILayout.Button("ビルド"))
                {
                    // 該当ハンドラ関数を呼ぶ
                    OnBuildButtonClick();
                }
            }


            // スクロールレイアウトにする
            using (var scrollView = new EditorGUILayout.ScrollViewScope(scrollPosition))
            {
                // スクロール量を覚えておく
                scrollPosition = scrollView.scrollPosition;


                // 現在の選択済みファイルパス分回る
                foreach (var filePath in filePathList)
                {
                    // ラベルとして表示する
                    GUILayout.Label(filePath);
                }
            }
        }


        /// <summary>
        /// ファイルをひとつだけ選択するボタンのクリックイベントを処理します
        /// </summary>
        private void OnIncludeFileSelectButtonClick()
        {
            // ファイル選択ダイアログを表示して、選択されなかったら
            var filePath = EditorUtility.OpenFilePanel("アーカイブに入れるファイルを選択して下さい", string.Empty, string.Empty);
            if (string.IsNullOrWhiteSpace(filePath))
            {
                // なにもせず終了
                return;
            }


            // パスを正規化して、既に登録済みなら
            filePath = filePath.Replace("\\", "/");
            if (filePathList.Contains(filePath))
            {
                // 終了
                return;
            }


            // ファイルリストに登録する
            filePathList.Add(filePath);
        }


        /// <summary>
        /// フォルダごと選択するボタンのクリックイベントを処理します
        /// </summary>
        private void OnIncludeDirectorySelectButtonClick()
        {
            // ディレクトリ選択ダイアログを表示して、選択されなかったら
            var dirPath = EditorUtility.OpenFolderPanel("インクルードするファイルを含んだフォルダを選択して下さい", string.Empty, string.Empty);
            if (string.IsNullOrWhiteSpace(dirPath))
            {
                // 終了
                return;
            }


            // ディレクトリ内のファイルを列挙する
            foreach (var tempFilePath in Directory.EnumerateFiles(dirPath, "*.*", SearchOption.TopDirectoryOnly))
            {
                // パスを正規化して、既に登録済みなら
                var filePath = tempFilePath.Replace("\\", "/");
                if (filePathList.Contains(filePath))
                {
                    // 次のパスへ
                    continue;
                }


                // ファイルリストに登録する
                filePathList.Add(filePath);
            }
        }


        /// <summary>
        /// ビルドボタンのクリックイベントを処理します
        /// </summary>
        private void OnBuildButtonClick()
        {
        }
    }
}