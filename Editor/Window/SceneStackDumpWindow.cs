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
using System.Linq;
using IceMilkTea.Core;
using IceMilkTea.Service;
using IceMilkTeaEditor.LayoutSystem;
using UnityEditor;
using UnityEngine;

namespace IceMilkTeaEditor.Window
{
    /// <summary>
    /// GameFacilitatorService で管理しているサービススタックのダンプを確認するウィンドウクラスです
    /// </summary>
    public class SceneStackDumpWindow : ImtEditorWindow
    {
        private Action<object> internalUpdate;
        private List<SceneInfo> sceneInfoList;


        protected IGameFacilitatorService GameFacilitatorService { get; private set; }


        protected IReadOnlyCollection<SceneInfo> SceneInfos { get; private set; }



        public static void OpenWindow()
        {
            GetWindow<SceneStackDumpWindow>();
        }


        protected override void Initialize()
        {
            InitializeWindowProperty();
            InitializeUI();
            InitializeGameFacilitatorService();
            Internal_Update(null);
        }


        private void InitializeWindowProperty()
        {
            titleContent = new GUIContent("SceneStack");
        }


        protected virtual void InitializeUI()
        {
            var playerLoopTree = new ImtEditorFreeRenderer(this, RenderSceneStack);
            RootUi.AddUi(playerLoopTree);
        }


        private void Internal_Update(object state)
        {
            InitializeGameFacilitatorService();
            UpdateSceneStack();
            PostMessage(internalUpdate ?? (internalUpdate = Internal_Update), state);
        }


        private void InitializeGameFacilitatorService()
        {
            if (GameFacilitatorService != null)
            {
                return;
            }


            var serviceManager = GameMain.Current == null ? null : GameMain.Current.ServiceManager;
            if (serviceManager == null)
            {
                return;
            }


            serviceManager.ServiceForEach(service =>
            {
                if (service is IGameFacilitatorService gameFacilitatorService)
                {
                    GameFacilitatorService = gameFacilitatorService;
                }
            });
        }


        private void UpdateSceneStack()
        {
            if (sceneInfoList == null)
            {
                sceneInfoList = new List<SceneInfo>();
                SceneInfos = sceneInfoList.AsReadOnly();
            }


            if (GameFacilitatorService == null)
            {
                return;
            }


            sceneInfoList.Clear();
            GameFacilitatorService.SceneContextForEach((scene, state) =>
            {
                sceneInfoList.Add(new SceneInfo()
                {
                    GameScene = scene,
                    SceneState = state,
                });
            });
        }


        protected virtual void RenderSceneStack()
        {
            foreach (var sceneInfo in SceneInfos.Reverse())
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.TextField(sceneInfo.GameScene.GetType().Name);
                EditorGUILayout.TextField(sceneInfo.SceneState.ToString());
                EditorGUILayout.EndHorizontal();
            }
        }



        protected struct SceneInfo
        {
            public GameScene GameScene;
            public GameFacilitatorService<GameScene>.SceneState SceneState;
        }
    }
}
