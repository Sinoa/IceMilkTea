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

namespace IceMilkTea.Core
{
    /// <summary>
    /// PlayerLoopSystemに登録する際に必要になる型情報を定義したGameService用構造体です
    /// </summary>
    public struct GameServiceUpdate
    {
        /// <summary>
        /// MainLoopHead 用型定義
        /// </summary>
        public struct GameServiceMainLoopHead { }

        /// <summary>
        /// PreFixedUpdate 用型定義
        /// </summary>
        public struct GameServicePreFixedUpdate { }

        /// <summary>
        /// PostFixedUpdate 用型定義
        /// </summary>
        public struct GameServicePostFixedUpdate { }

        /// <summary>
        /// PostPhysicsSimulation 用型定義
        /// </summary>
        public struct GameServicePostPhysicsSimulation { }

        /// <summary>
        /// PostWaitForFixedUpdate 用型定義
        /// </summary>
        public struct GameServicePostWaitForFixedUpdate { }

        /// <summary>
        /// PreProcessSynchronizationContext 用型定義
        /// </summary>
        public struct GameServicePreProcessSynchronizationContext { }

        /// <summary>
        /// PostProcessSynchronizationContext 用型定義
        /// </summary>
        public struct GameServicePostProcessSynchronizationContext { }

        /// <summary>
        /// PreUpdate 用型定義
        /// </summary>
        public struct GameServicePreUpdate { }

        /// <summary>
        /// PostUpdate 用型定義
        /// </summary>
        public struct GameServicePostUpdate { }

        /// <summary>
        /// PreAnimation 用型定義
        /// </summary>
        public struct GameServicePreAnimation { }

        /// <summary>
        /// PostAnimation 用型定義
        /// </summary>
        public struct GameServicePostAnimation { }

        /// <summary>
        /// PreLateUpdate 用型定義
        /// </summary>
        public struct GameServicePreLateUpdate { }

        /// <summary>
        /// PostLateUpdate 用型定義
        /// </summary>
        public struct GameServicePostLateUpdate { }

        /// <summary>
        /// PreDrawPresent 用型定義
        /// </summary>
        public struct GameServicePreDrawPresent { }

        /// <summary>
        /// PostDrawPresent 用型定義
        /// </summary>
        public struct GameServicePostDrawPresent { }

        /// <summary>
        /// MainLoopTail 用型定義
        /// </summary>
        public struct GameServiceMainLoopTail { }
    }



    /// <summary>
    /// サービスが動作するための更新タイミングを表します
    /// </summary>
    [Flags]
    public enum GameServiceUpdateTiming : UInt32
    {
        /// <summary>
        /// メインループ最初のタイミング。
        /// ただし、Time.frameCountや入力情報の更新直後となります。
        /// </summary>
        MainLoopHead = (1 << 0),

        /// <summary>
        /// MonoBehaviour.FixedUpdate直前のタイミング
        /// </summary>
        PreFixedUpdate = (1 << 1),

        /// <summary>
        /// MonoBehaviour.FixedUpdate直後のタイミング
        /// </summary>
        PostFixedUpdate = (1 << 2),

        /// <summary>
        /// 物理シミュレーション直後のタイミング。
        /// ただし、シミュレーションによる物理イベントキューが全て処理された直後となります。
        /// </summary>
        PostPhysicsSimulation = (1 << 3),

        /// <summary>
        /// WaitForFixedUpdate直後のタイミング。
        /// </summary>
        PostWaitForFixedUpdate = (1 << 4),

        /// <summary>
        /// UnitySynchronizationContextにPostされた関数キューが処理される直前のタイミング
        /// </summary>
        PreProcessSynchronizationContext = (1 << 5),

        /// <summary>
        /// UnitySynchronizationContextにPostされた関数キューが処理された直後のタイミング
        /// </summary>
        PostProcessSynchronizationContext = (1 << 6),

        /// <summary>
        /// MonoBehaviour.Update直前のタイミング
        /// </summary>
        PreUpdate = (1 << 7),

        /// <summary>
        /// MonoBehaviour.Update直後のタイミング
        /// </summary>
        PostUpdate = (1 << 8),

        /// <summary>
        /// UnityのAnimator(UpdateMode=Normal)によるポージング処理される直前のタイミング
        /// </summary>
        PreAnimation = (1 << 9),

        /// <summary>
        /// UnityのAnimator(UpdateMode=Normal)によるポージング処理された直後のタイミング
        /// </summary>
        PostAnimation = (1 << 10),

        /// <summary>
        /// MonoBehaviour.LateUpdate直前のタイミング
        /// </summary>
        PreLateUpdate = (1 << 11),

        /// <summary>
        /// MonoBehaviour.LateUpdate直後のタイミング
        /// </summary>
        PostLateUpdate = (1 << 12),

        /// <summary>
        /// メインスレッドにおける描画デバイスのPresentする直前のタイミング
        /// </summary>
        PreDrawPresent = (1 << 13),

        /// <summary>
        /// メインスレッドにおける描画デバイスのPresentされた直後のタイミング
        /// </summary>
        PostDrawPresent = (1 << 14),

        /// <summary>
        /// メインループの最後のタイミング。
        /// </summary>
        MainLoopTail = (1 << 15),

        /// <summary>
        /// Unityプレイヤーのフォーカスが得られたときのタイミング。
        /// OnApplicationFocus(true)。
        /// </summary>
        OnApplicationFocusIn = (1 << 16),

        /// <summary>
        /// Unityプレイヤーのフォーカスが失われたときのタイミング。
        /// OnApplicationFocus(false)。
        /// </summary>
        OnApplicationFocusOut = (1 << 17),

        /// <summary>
        /// Unityプレイヤーのメインループが一時停止したときのタイミング。
        /// OnApplicationPause(true)。
        /// </summary>
        OnApplicationSuspend = (1 << 18),

        /// <summary>
        /// Unityプレイヤーのメインループが再開したときのタイミング。
        /// OnApplicationPause(false)。
        /// </summary>
        OnApplicationResume = (1 << 19),

        /// <summary>
        /// あらゆるカメラのカリングが行われる直前のタイミング。
        /// ただし、カメラが存在する数分１フレームで複数回呼び出される可能性があります。
        /// さらに、スレッドはメインスレッド上におけるタイミングとなります。
        /// </summary>
        CameraPreCulling = (1 << 20),

        /// <summary>
        /// あらゆるカメラのレンダリングが行われる直前のタイミング。
        /// ただし、カメラが存在する数分１フレームで複数回呼び出される可能性があります。
        /// さらに、スレッドはメインスレッド上におけるタイミングとなります。
        /// </summary>
        CameraPreRendering = (1 << 21),

        /// <summary>
        /// あらゆるカメラのレンダリングが行われた直後のタイミング。
        /// ただし、カメラが存在する数分１フレームで複数回呼び出される可能性があります。
        /// さらに、スレッドはメインスレッド上におけるタイミングとなります。
        /// </summary>
        CameraPostRendering = (1 << 22),
    }
}