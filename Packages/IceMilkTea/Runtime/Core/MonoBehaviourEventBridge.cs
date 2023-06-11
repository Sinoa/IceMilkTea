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
using System.Collections;
using UnityEngine;

namespace IceMilkTea.Core
{
    /// <summary>
    /// UnityのMonoBehaviourでしか得られないイベントを引き込むためのコンポーネントクラスです
    /// </summary>
    internal class MonoBehaviourEventBridge : MonoBehaviour
    {
        // 以下メンバ変数定義
        private WaitForEndOfFrame waitForEndOfFrame;
        private Action<bool> onApplicationFocusFunction;
        private Action<bool> onApplicationPauseFunction;
        private Action onEndOfFrame;



        /// <summary>
        /// 対象のゲームオブジェクトに MonoBehaviourEventBridge コンポーネントを新規アタッチを行い初期化を行います
        /// </summary>
        /// <param name="targetGameObject">アタッチする対象のゲームオブジェクト</param>
        /// <returns>新規でアタッチした MonoBehaviourEventBridge のインスタンスを返します</returns>
        /// <exception cref="ArgumentNullException">targetGameObject が null です</exception>
        public static MonoBehaviourEventBridge Attach(GameObject targetGameObject)
        {
            // nullなゲームオブジェクトを渡されたら
            if (targetGameObject == null)
            {
                // そんなことは許さない
                throw new ArgumentNullException(nameof(targetGameObject));
            }


            // 自身をアタッチして初期化をする
            var component = targetGameObject.AddComponent<MonoBehaviourEventBridge>();
            component.waitForEndOfFrame = new WaitForEndOfFrame();
            component.onApplicationFocusFunction = new Action<bool>(_ => { });
            component.onApplicationPauseFunction = new Action<bool>(_ => { });
            component.onEndOfFrame = new Action(() => { });


            // コルーチンを開始する
            component.StartCoroutine(component.DoEndOfFrameLoop());


            // インスペクタから姿を消して返す
            component.hideFlags = HideFlags.HideInInspector;
            return component;
        }


        /// <summary>
        /// OnApplicationFocusを実行する関数を設定します
        /// </summary>
        /// <param name="focusFunction">OnApplicationFocusを実行する関数</param>
        /// <exception cref="ArgumentNullException">focusFunction が null です</exception>
        public void SetApplicationFocusFunction(Action<bool> focusFunction)
        {
            // null が渡されたら
            if (focusFunction == null)
            {
                // 許さない
                throw new ArgumentNullException(nameof(focusFunction));
            }


            // 新しい関数を受け取る
            onApplicationFocusFunction = focusFunction;
        }


        /// <summary>
        /// OnApplicationPauseを実行する関数を設定します
        /// </summary>
        /// <param name="pauseFunction">OnApplicationPauseを実行する関数</param>
        /// <exception cref="ArgumentNullException">pauseFunction が null です</exception>
        public void SetApplicationPauseFunction(Action<bool> pauseFunction)
        {
            // null が渡されたら
            if (pauseFunction == null)
            {
                // 許さない
                throw new ArgumentNullException(nameof(pauseFunction));
            }


            // 新しい関数を受け取る
            onApplicationPauseFunction = pauseFunction;
        }


        /// <summary>
        /// WaitForEndOfFrameの継続関数を設定します
        /// </summary>
        /// <param name="endOfFrameFunction">WaitForEndOfFrameの継続を行う関数</param>
        /// <exception cref="ArgumentNullException">endOfFrameFunction が null です</exception>
        public void SetEndOfFrameFunction(Action endOfFrameFunction)
        {
            // null が渡されたら
            if (endOfFrameFunction == null)
            {
                // 許さない
                throw new ArgumentNullException(nameof(endOfFrameFunction));
            }


            // 新しい関数を受け取る
            onEndOfFrame = endOfFrameFunction;
        }


        /// <summary>
        /// コンポーネントが破棄される時の処理を実行します
        /// </summary>
        private void OnDestroy()
        {
            // 関数の参照を殺す
            onApplicationFocusFunction = null;
            onApplicationPauseFunction = null;
            onEndOfFrame = null;
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


        /// <summary>
        /// UnityのWaitForEndOfFrameの処理を永遠に実行し続けます
        /// </summary>
        /// <returns>WaitForEndOfFrame のインスタンスを常に返し続けます</returns>
        private IEnumerator DoEndOfFrameLoop()
        {
            // 無限ループ
            while (true)
            {
                // フレームの終端まで待機（描画の終端でゲームループの終端ではない）して関数を叩く
                yield return waitForEndOfFrame;
                onEndOfFrame();
            }
        }
    }
}