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
using UnityEngine;

namespace IceMilkTea.Core
{
    /// <summary>
    /// UnityのMonoBehaviourでしか得られないイベントを引き込むためのコンポーネントクラスです
    /// </summary>
    internal class MonoBehaviourEventBridge : MonoBehaviour
    {
        // 以下メンバ変数定義
        private Action<bool> onApplicationFocusFunction;
        private Action<bool> onApplicationPauseFunction;



        /// <summary>
        /// 対象のゲームオブジェクトに MonoBehaviourEventBridge コンポーネントを新規アタッチを行い初期化を行います
        /// </summary>
        /// <param name="targetGameObject">アタッチする対象のゲームオブジェクト</param>
        /// <param name="focusFunction">OnApplicationFocusを実行する関数</param>
        /// <param name="pauseFunction">OnApplicationPauseを実行する関数</param>
        /// <returns>新規でアタッチした MonoBehaviourEventBridge のインスタンスを返します</returns>
        public static MonoBehaviourEventBridge Attach(GameObject targetGameObject, Action<bool> focusFunction, Action<bool> pauseFunction)
        {
            // 自身をアタッチして初期化をする
            var component = targetGameObject.AddComponent<MonoBehaviourEventBridge>();
            component.onApplicationFocusFunction = focusFunction;
            component.onApplicationPauseFunction = pauseFunction;


            // インスペクタから姿を消す
            component.hideFlags = HideFlags.HideInInspector;


            // 結果を返す
            return component;
        }


        /// <summary>
        /// コンポーネントが破棄される時の処理を実行します
        /// </summary>
        private void OnDestroy()
        {
            // 関数の参照を殺す
            onApplicationFocusFunction = null;
            onApplicationPauseFunction = null;
        }


        /// <summary>
        /// ゲームアプリケーションがウィンドウなどプレイヤーのフォーカスの状態が変化したときの処理を行います
        /// </summary>
        /// <param name="focus">フォーカスを得られたときはtrueを、失ったときはfalse</param>
        private void OnApplicationFocus(bool focus)
        {
            // 本来実行したい関数を叩く
            onApplicationFocusFunction(focus);
        }


        /// <summary>
        /// ゲームアプリケーションの再生状態が変化したときの処理を行います
        /// </summary>
        /// <param name="pause">一時停止になったときはtrueを、再生状態になったときはfalse</param>
        private void OnApplicationPause(bool pause)
        {
            // 本来実行したい関数を叩く
            onApplicationPauseFunction(pause);
        }
    }
}