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
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using UnityEditor;

namespace IceMilkTeaEditor.LayoutSystem
{
    /// <summary>
    /// IceMilkTeaが提供するエディタウィンドウの基本クラスです。
    /// このウィンドウ内での、メッセージループなどの機能を提供します。
    /// </summary>
    public abstract class ImtEditorWindow : EditorWindow
    {
        // メンバ変数定義
        [NonSerialized]
        private bool initialized;
        private Queue<Message> messageQueue;



        /// <summary>
        /// このウィンドウが保持するルートUIを取得します
        /// </summary>
        protected ImtEditorUiRoot RootUi { get; private set; }



        #region Unityイベントハンドラとロジック関数
        /// <summary>
        /// ウィンドウの初期化を行います
        /// </summary>
        private void Awake()
        {
            // 内部初期化関数を呼ぶ
            InternalInitialize();
        }


        /// <summary>
        /// ウィンドウのレンダリングを行います
        /// </summary>
        private void OnGUI()
        {
            try
            {
                // レンダリングループ中でも初期化を呼び出し続ける
                InternalInitialize();


                // メッセージを処理して描画する
                ProcessMessage();
                Render();


                // メッセージがまだ存在する場合は継続する
                ContinueIfMessageAvailable();
            }
            catch (Exception exception)
            {
                // エラーが発生したイベントを起こす
                bool canClose;
                OnError(exception, out canClose);


                // 閉じることが可能なら
                if (canClose)
                {
                    // 自身を閉じる
                    Close();
                }


                // 例外をキャプチャして再スロー
                ExceptionDispatchInfo.Capture(exception).Throw();
            }
        }


        /// <summary>
        /// エディタウィンドウの内部初期化を行います
        /// </summary>
        private void InternalInitialize()
        {
            // 初期化済みなら
            if (initialized)
            {
                // 何もせず終了
                return;
            }


            // メッセージキューとルートUIを生成する
            messageQueue = new Queue<Message>();
            RootUi = new ImtEditorUiRoot(this);


            // 初期化イベントを起こす
            Initialize();


            // 初期化済みマーク
            initialized = true;
        }


        /// <summary>
        /// メッセージキューに溜まったメッセージを処理します。
        /// 処理されるメッセージは、この関数が呼び出された段階までのメッセージです。
        /// </summary>
        private void ProcessMessage()
        {
            // 現在溜まっているメッセージ数を取得
            var currentMessageCount = messageQueue.Count;


            // 溜まっているメッセージ数分だけループ
            for (int i = 0; i < currentMessageCount; ++i)
            {
                // メッセージを取り出して呼ぶ
                messageQueue.Dequeue().Invoke();
            }
        }


        /// <summary>
        /// ウィンドウの具体的なレンダリングを行います
        /// </summary>
        private void Render()
        {
            // ルートUIのレンダリングを行う
            RootUi.Render();
        }


        /// <summary>
        /// 処理するべきウィンドウメッセージがあるなら、レンダリングループを継続します
        /// </summary>
        private void ContinueIfMessageAvailable()
        {
            // メッセージの数が１つでもあるなら
            if (messageQueue.Count > 0)
            {
                // 再描画を発行してレンダリングをもう一度呼び出されるようにする
                Repaint();
            }
        }
        #endregion


        #region IceMilkTeaエディタウィンドウのイベントハンドラ
        /// <summary>
        /// エディタウィンドウの初期化を行います
        /// </summary>
        protected virtual void Initialize()
        {
        }


        /// <summary>
        /// エディタウィンドウで発生した未処理の例外を処理します
        /// </summary>
        /// <param name="exception">発生した未処理の例外</param>
        /// <param name="canClose">ウィンドウはこのまま閉じられるかどうか</param>
        protected virtual void OnError(Exception exception, out bool canClose)
        {
            // 通常は閉じる
            canClose = true;
        }
        #endregion


        #region アセンブリ内公開関数
        /// <summary>
        /// このウィンドウが処理するべきメッセージをポストします
        /// </summary>
        /// <param name="callback">処理するべきメッセージのコールバック</param>
        /// <param name="state">コールバックに渡される任意のオブジェクト</param>
        /// <exception cref="ArgumentNullException">callback が null です</exception>
        internal void PostMessage(Action<object> callback, object state)
        {
            // もし callback が null なら
            if (callback == null)
            {
                // 何を処理するというのだ
                throw new ArgumentNullException(nameof(callback));
            }


            // メッセージキューにメッセージとして追加
            messageQueue.Enqueue(new Message(callback, state));
        }
        #endregion



        #region メッセージ構造体定義
        /// <summary>
        /// 処理するべきメッセージの構造体です
        /// </summary>
        private struct Message
        {
            // メンバ変数定義
            private Action<object> callback;
            private object state;



            /// <summary>
            /// EventMessage 構造体のインスタンスを初期化します
            /// </summary>
            /// <param name="callback">呼び出されるべきコールバック</param>
            /// <param name="state">コールバックに渡される任意のオブジェクト</param>
            public Message(Action<object> callback, object state)
            {
                // 初期化をする
                this.callback = callback;
                this.state = state;
            }


            /// <summary>
            /// メッセージの呼び出しを行います
            /// </summary>
            public void Invoke()
            {
                // コールバックを呼ぶ
                callback(state);
            }
        }
        #endregion
    }
}