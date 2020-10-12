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
using System.IO;
using System.Linq;
using System.Text;
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
        // 定数定義
        private const string NamespacePlaceHolder = "%NAMESPACE%";
        private const string ClassNamePlaceHolder = "%CLASSNAME%";
        private const string CodeBlockPlaceHolder = "%CODEBLOCK%";
        private const string OriginalTextData = "dXNpbmcgU3lzdGVtOw0KDQpuYW1lc3BhY2UgJU5BTUVTUEFDRSUNCnsNCiAgICBwdWJsaWMgc3RhdGljIGNsYXNzICVDTEFTU05BTUUlDQogICAgew0KICAgICAgICBwdWJsaWMgc3RhdGljIG9iamVjdCBDcmVhdGVTdGF0ZUluc3RhbmNlKFR5cGUgdHlwZSkNCiAgICAgICAgew0KJUNPREVCTE9DSyUNCg0KDQogICAgICAgICAgICByZXR1cm4gbnVsbDsNCiAgICAgICAgfQ0KICAgIH0NCn0=";

        // メンバ変数定義
        private string namespacePath;
        private string className;
        private string assemblies;



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


            // 既定値
            namespacePath = "Utility";
            className = "StateMachineUtility";
            assemblies = "Assembly-CSharp";
        }


        private void OnGUI()
        {
            namespacePath = EditorGUILayout.TextField(new GUIContent("名前空間"), namespacePath);
            className = EditorGUILayout.TextField(new GUIContent("クラス名"), className);
            assemblies = EditorGUILayout.TextField(new GUIContent("参照するアセンブリ名"), assemblies);
            if (GUILayout.Button("生成する"))
            {
                SaveCode(assemblies.Split(','), namespacePath, className);
            }
        }


        private void SaveCode(string[] assemblies, string namespacePath, string className)
        {
            var savePath = EditorUtility.SaveFilePanel("保存先の選択", null, "State.Generate", "cs");
            if (string.IsNullOrWhiteSpace(savePath))
            {
                return;
            }


            var code = GenerateCode(assemblies, namespacePath, className);
            File.WriteAllText(savePath, code);
            AssetDatabase.Refresh();
        }


        private string GenerateCode(string[] assemblies, string namespacePath, string className)
        {
            var rawCodeText = new UTF8Encoding(false).GetString(Convert.FromBase64String(OriginalTextData));
            var codeBlock = GenerateCodeBlock(assemblies);
            return rawCodeText
                .Replace(NamespacePlaceHolder, namespacePath)
                .Replace(ClassNamePlaceHolder, className)
                .Replace(CodeBlockPlaceHolder, codeBlock);
        }


        private string GenerateCodeBlock(string[] assemblies)
        {
            var stateTypeTexts = StateMachineUtility.CollectStateTypes()
                .FilterAssembly(assemblies)
                .SelectTypeFullPathName()
                .ToArray();


            var buffer = new StringBuilder();
            for (int i = 0; i < stateTypeTexts.Length; ++i)
            {
                var stateClassName = stateTypeTexts[i];
                if (i == 0)
                {
                    buffer.Append($"if (typeof({stateClassName}) == type)\n    return new {stateClassName}();\n");
                }
                else
                {
                    buffer.Append($"else if (typeof({stateClassName}) == type)\n    return new {stateClassName}();\n");
                }
            }


            return buffer.ToString();
        }
    }
}