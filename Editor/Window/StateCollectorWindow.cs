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

using IceMilkTeaEditor.Utility;
using UnityEditor;
using UnityEngine;

namespace IceMilkTeaEditor.Window
{
    /// <summary>
    /// ImtStateMachine クラスの状態クラスを収集するエディタウィンドウクラスです
    /// </summary>
    public class StateCollectorWindow : EditorWindow
    {
        /// <summary>
        /// ウィンドウを開きます
        /// </summary>
        public static void OpenWindow()
        {
            // インスタンスを取得すると開かれる
            GetWindow<StateCollectorWindow>();
        }


        /// <summary>
        /// ウィンドウの起動時の初期化処理を実行します
        /// </summary>
        private void Awake()
        {
            // ウィンドウ設定をする
            titleContent = new GUIContent("ステート生成ツール");
            minSize = new Vector2(800.0f, 600.0f);
        }


        private void OnGUI()
        {
            if (GUILayout.Button("Test"))
            {
                var filtered = StateMachineUtility.CollectStateTypes()
                    .FilterAssembly(new string[] { "Assembly-CSharp" })
                    .SelectTypeFullPathName();


                foreach (var state in filtered)
                {
                    Debug.Log(state);
                }
            }
        }
    }
}