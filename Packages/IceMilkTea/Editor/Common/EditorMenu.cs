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

using IceMilkTeaEditor.Utility;
using IceMilkTeaEditor.Window;
using UnityEditor;

namespace IceMilkTeaEditor.Common
{
    /// <summary>
    /// UnityエディタのメニューにIceMilkTeaのメニューを取りまとめるクラスです
    /// </summary>
    internal static class EditorMenu
    {
        // 定数定義
        private const string RootMenuName = "IceMilkTea";
        private const string RightClickRootMenuName = "Assets/IceMilkTea";
        private const string DebugMenuName = RootMenuName + "/Debug";
        private const string WindowMenuName = RootMenuName + "/Window";
        private const string HierarchyMenuName = RootMenuName + "/Hierarchy";



        /// <summary>
        /// GameMainAssetGenerateウィンドウを開きます
        /// </summary>
        [MenuItem(WindowMenuName + "/GameMainAssetCreate")]
        public static void OpenGameMainAssetGenerateWindow()
        {
            // GameMainGenerateWindowを開く
            GameMainAssetGenerateWindow.OpenWindow();
        }


        /// <summary>
        /// ImtArchiveMakerウィンドウを開きます
        /// </summary>
        [MenuItem(WindowMenuName + "/ArchiveMaker")]
        public static void OpenImtArchiveMakerWindow()
        {
            // ImtArchiveMakerWindowを開く
            ImtArchiveMakerWindow.OpenWindow();
        }


        /// <summary>
        /// AssetBundleBuildウィンドウを開きます
        /// </summary>
        [MenuItem(WindowMenuName + "/AssetBundleBuild")]
        public static void OpenAssetBundleBuildWindow()
        {
            // AssetBundleWindowを開く
            AssetBundleBuildWindow.OpenWindow();
        }


        /// <summary>
        /// StateCollectorウィンドウを開きます
        /// </summary>
        [MenuItem(WindowMenuName + "/ImtStateGenerator")]
        public static void OpenStateCollectorWindow()
        {
            // StateCollectorWindowsを開く
            StateCollectorWindow.OpenWindow();
        }


        [MenuItem(DebugMenuName + "/DumpPlayerLoopSystem")]
        public static void OpenPlayerLoopSystemDumpWindow()
        {
            PlayerLoopSystemDumpWindow.OpenWindow();
        }


        [MenuItem(DebugMenuName + "/DumpSceneStack")]
        public static void OpenSceneStackDumpWindow()
        {
            SceneStackDumpWindow.OpenWindow();
        }


        /// <summary>
        /// エディタシーン上に存在する、空のゲームオブジェクトを再帰的に削除します
        /// </summary>
        [MenuItem(HierarchyMenuName + "/RemoveEmptyGameObject")]
        public static void DoRemoveEmptyGameObjectOnEditorScene()
        {
            // 空のゲームオブジェクトを削除する
            ImtGameObjectUtility.DestoryEmptyGameObjectOnEditorScene(null);
        }


        /// <summary>
        /// 選択されたアセットを再シリアライズします
        /// </summary>
        [MenuItem(RightClickRootMenuName + "/Reserialize/Selected")]
        public static void DoReserializeAsset()
        {
            // 本当に再シリアライズしてよいか確認
            if (EditorUtility.DisplayDialog("確認", "本当に再シリアライズして良いですか？", "OK", "Cancel"))
            {
                // 選択中のアセットを再シリアライズして完了を通知
                AssetReserializer.Reserialize(Selection.activeObject);
                EditorUtility.DisplayDialog("報告", "再シリアライズが完了しました", "OK");
            }
        }


        /// <summary>
        /// Unityプロジェクトの再シリアライズをします
        /// </summary>
        [MenuItem(RightClickRootMenuName + "/Reserialize/All")]
        public static void DoReserializeAssetAll()
        {
            // 本当に再シリアライズしてよいか確認
            if (EditorUtility.DisplayDialog("確認", "本当にプロジェクトの再シリアライズして良いですか？", "OK", "Cancel"))
            {
                // プロジェクトのアセットを再シリアライズして完了を通知
                AssetReserializer.ReserializeAll();
                EditorUtility.DisplayDialog("報告", "再シリアライズが完了しました", "OK");
            }
        }
    }
}