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

using IceMilkTea.Core;
using IceMilkTeaEditor.LayoutSystem;
using UnityEditor;
using UnityEngine;

namespace IceMilkTeaEditor.Window
{
    /// <summary>
    /// PlayerLoopSystem の内部ツリーをダンプして確認するエディタウィンドウです
    /// </summary>
    public class PlayerLoopSystemDumpWindow : ImtEditorWindow
    {
        private ImtPlayerLoopSystemNodeItem rootPlayerLoopSystemNodeItem;
        private Vector2 scrollPosition;



        public static void OpenWindow()
        {
            GetWindow<PlayerLoopSystemDumpWindow>();
        }


        protected override void Initialize()
        {
            InitializeWindowProperty();
            InitializeUI();
        }


        private void InitializeWindowProperty()
        {
            titleContent = new GUIContent("DumpPlayerLoopSystem");
            //minSize = new Vector2(800, 600);
        }


        private void InitializeUI()
        {
            var dumpButton = new ImtEditorButton(this, "現在のPlayerLoopSystemの構造をダンプする");
            dumpButton.Click += OnDumpButtonClick;


            var logDumpButton = new ImtEditorButton(this, "現在のPlayerLoopSystemの構造をログとしてダンプする");
            logDumpButton.Click += OnLogDumpButtonClick;


            var playerLoopTree = new ImtEditorFreeRenderer(this, RenderPlayerLoopTree);


            RootUi.AddUi(dumpButton);
            RootUi.AddUi(logDumpButton);
            RootUi.AddUi(playerLoopTree);
        }


        private void OnDumpButtonClick(ImtEditorButton button)
        {
            var currentRootPlayerLoop = ImtPlayerLoopSystem.GetCurrentPlayerLoop();
            rootPlayerLoopSystemNodeItem = new ImtPlayerLoopSystemNodeItem(currentRootPlayerLoop);
        }


        private void OnLogDumpButtonClick(ImtEditorButton button)
        {
            var currentRootPlayerLoop = ImtPlayerLoopSystem.GetCurrentPlayerLoop();
            Debug.Log(currentRootPlayerLoop.ToString());
        }


        private void RenderPlayerLoopTree()
        {
            if (rootPlayerLoopSystemNodeItem == null)
            {
                GUILayout.Box(new GUIContent("構造がダンプされると結果が描画されます"));
                return;
            }


            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            RenderPlayerLoopNode(rootPlayerLoopSystemNodeItem);
            EditorGUILayout.EndScrollView();
        }


        private void RenderPlayerLoopNode(ImtPlayerLoopSystemNodeItem node)
        {
            if (node.Childlen.Length > 0)
            {
                node.Foldouted = EditorGUILayout.Foldout(node.Foldouted, node.FoldoutHeaderContent);
            }
            else
            {
                EditorGUILayout.LabelField(node.FoldoutHeaderContent);
            }


            if (node.Foldouted)
            {
                ++EditorGUI.indentLevel;
                foreach (var child in node.Childlen)
                {
                    RenderPlayerLoopNode(child);
                }
                --EditorGUI.indentLevel;
            }
        }



        private class ImtPlayerLoopSystemNodeItem
        {
            public bool Foldouted;
            public GUIContent FoldoutHeaderContent;
            public ImtPlayerLoopSystem PlayerLoopSystem;
            public ImtPlayerLoopSystemNodeItem[] Childlen;



            public ImtPlayerLoopSystemNodeItem(ImtPlayerLoopSystem loopSystem)
            {
                Foldouted = false;
                FoldoutHeaderContent = new GUIContent(loopSystem.Type == null ? "NULL" : loopSystem.Type.Name);
                PlayerLoopSystem = loopSystem;
                Childlen = new ImtPlayerLoopSystemNodeItem[loopSystem.SubLoopSystemList.Count];
                for (int i = 0; i < Childlen.Length; ++i)
                {
                    Childlen[i] = new ImtPlayerLoopSystemNodeItem(loopSystem.SubLoopSystemList[i]);
                }
            }
        }
    }
}