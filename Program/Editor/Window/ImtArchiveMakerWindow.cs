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
    /// IceMilkTeaアーカイブをGUI上で作るためのウィンドウを提供するクラスです
    /// </summary>
    public class ImtArchiveMakerWindow : EditorWindow
    {
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
            // タイトルの変更
            titleContent = new GUIContent("ImtArchiveMaker");
        }


        /// <summary>
        /// エディタウィンドウのGUIのハンドリングと描画を行います
        /// </summary>
        private void OnGUI()
        {
        }
    }
}