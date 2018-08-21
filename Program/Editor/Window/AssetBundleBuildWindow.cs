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

using UnityEditor;
using UnityEngine;

namespace IceMilkTeaEditor.Window
{
    /// <summary>
    /// アセットバンドルのビルドを簡易的に操作するためのウィンドウクラスです
    /// </summary>
    public class AssetBundleBuildWindow : EditorWindow
    {
        // 定数定義
        private const string PlaceHolderPlatformNamePattern = "@PlatformName";
        private const string DefaultOutputDirectoryPath = "AssetBundles/" + PlaceHolderPlatformNamePattern;



        // メンバ変数定義
        private string outputDirectoryPath;
        private bool buildWin64;
        private bool buildAndroid;
        private bool buildIos;
        private Vector2 scrollPosition;



        /// <summary>
        /// AssetBundleWindow のウィンドウを開きます
        /// </summary>
        public static void OpenWindow()
        {
            // ウィンドウインスタンスの取得関数はOpenもやってくれる
            GetWindow<AssetBundleBuildWindow>();
        }


        /// <summary>
        /// ウィンドウの起動時の初期化処理を実行します
        /// </summary>
        private void Awake()
        {
            // ウィンドウ設定をする
            titleContent = new GUIContent("AssetBundle");
            minSize = new Vector2(800.0f, 600.0f);


            // 初期化する
            outputDirectoryPath = DefaultOutputDirectoryPath;
            buildWin64 = true;
            buildAndroid = true;
            buildIos = true;
        }


        /// <summary>
        /// ウィンドウの描画を行います
        /// </summary>
        private void OnGUI()
        {
            // 少し隙間を設ける
            EditorGUILayout.Space();


            // ビルドターゲット選択を横に並べる
            using (new EditorGUILayout.HorizontalScope(GUILayout.Width(400.0f)))
            {
                // ラベルの表示と各種ビルドターゲットのチェックボックスの表示
                EditorGUILayout.LabelField("ビルドターゲットの選択");
                buildWin64 = EditorGUILayout.ToggleLeft(new GUIContent("Win64"), buildWin64);
                buildAndroid = EditorGUILayout.ToggleLeft(new GUIContent("Android"), buildAndroid);
                buildIos = EditorGUILayout.ToggleLeft(new GUIContent("iOS"), buildIos);
            }


            // 少し隙間を設ける
            EditorGUILayout.Space();


            // 出力先ディレクトリの表示と選択を横に並べる
            using (new EditorGUILayout.HorizontalScope())
            {
                // 出力先ディレクトリのテキストフィールドと、保存先選択ボタンが押されたのなら
                outputDirectoryPath = EditorGUILayout.TextField(new GUIContent("出力先ディレクトリ"), outputDirectoryPath);
                if (GUILayout.Button("...", GUILayout.Width(25.0f)))
                {
                }
            }


            // 少し隙間を設ける
            EditorGUILayout.Space();


            // ビルドボタンが押されたのなら
            if (GUILayout.Button("ビルド"))
            {
            }


            // 少し隙間を設ける
            EditorGUILayout.Space();


            // ビルドされるアセットバンドルを表示するためのスクロールビューを開始
            EditorGUILayout.LabelField("ビルドされるアセットバンドルリスト");
            using (var scrollView = new EditorGUILayout.ScrollViewScope(scrollPosition))
            {
                // スクロール座標の更新
                scrollPosition = scrollView.scrollPosition;


                // ビルドするアセットバンドルリストが存在する分だけ回る
                foreach (var name in AssetDatabase.GetAllAssetBundleNames())
                {
                    // ひたすらアセットバンドルリストをラベルで表示
                    EditorGUILayout.LabelField(name);
                }
            }
        }
    }
}