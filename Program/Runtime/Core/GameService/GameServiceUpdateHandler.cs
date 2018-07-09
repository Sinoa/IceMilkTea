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

using System.Collections.ObjectModel;

namespace IceMilkTea.Core
{
    /// <summary>
    /// 担当するサービスリストの更新関数をハンドリングするクラスです
    /// </summary>
    internal class GameServiceUpdateHandler
    {
        // メンバ変数定義
        private ReadOnlyCollection<GameService> serviceCollection;



        /// <summary>
        /// GameServiceUpdateHandler のインスタンスを初期化します
        /// </summary>
        /// <param name="serviceList">更新処理をする対象となる読み取り専用サービスリスト</param>
        public GameServiceUpdateHandler(ReadOnlyCollection<GameService> serviceList)
        {
            // 受け取る
            serviceCollection = serviceList;
        }


        #region 各種更新関数のエントリポイント
        private void MainLoopHead()
        {
        }


        private void PreFixedUpdate()
        {
        }


        private void PostFixedUpdate()
        {
        }


        private void PostPhysicsSimulation()
        {
        }


        private void PostWaitForFixedUpdate()
        {
        }


        private void PreProcessSynchronizationContext()
        {
        }


        private void PostProcessSynchronizationContext()
        {
        }


        private void PreUpdate()
        {
        }


        private void PostUpdate()
        {
        }


        private void PreAnimation()
        {
        }


        private void PostAnimation()
        {
        }


        private void PreLateUpdate()
        {
        }


        private void PostLateUpdate()
        {
        }


        private void PreRendering()
        {
        }


        private void PostRendering()
        {
        }


        private void MainLoopTail()
        {
        }


        private void OnApplicationFocusIn()
        {
        }


        private void OnApplicationFocusOut()
        {
        }


        private void OnApplicationSuspend()
        {
        }


        private void OnApplicationResume()
        {
        }


        private void CameraPreCulling()
        {
        }


        private void CameraPreRendering()
        {
        }


        private void CameraPostRendering()
        {
        }
        #endregion
    }
}