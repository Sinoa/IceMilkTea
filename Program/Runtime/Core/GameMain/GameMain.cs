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

using UnityEngine;

namespace IceMilkTea.Core
{
    /// <summary>
    /// ゲームメインクラスの実装をするための抽象クラスです。
    /// IceMilkTeaによるゲームのスタートアップからメインループを構築する場合は必ず継承し実装をして下さい。
    /// </summary>
    public abstract class GameMain : ScriptableObject
    {
        /// <summary>
        /// 現在のゲームメインコンテキストを取得します
        /// </summary>
        public static GameMain CurrentContext { get; private set; }



        /// <summary>
        /// Unity起動時に実行されるゲームのエントリポイントです
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Main()
        {
            // アプリケーションのイベントハンドラを登録
            Application.wantsToQuit += Internal_RequestShutdown;
            Application.quitting += Internal_Shutdown;


            // 内部で保存されたGameMainのGameMainをロードする
            CurrentContext = Resources.Load<GameMain>("GameMain");


            // ロードが出来なかったのなら
            if (CurrentContext == null)
            {
                // セーフ起動用のゲームメインを立ち上げる
                CurrentContext = CreateInstance<SafeGameMain>();
            }


            // GameMainを起動する
            Internal_Startup();
        }


        /// <summary>
        /// このGameMainクラスのための Startup 関数です。
        /// </summary>
        private static void Internal_Startup()
        {
            // 起動を叩く
            CurrentContext.Startup();
        }


        /// <summary>
        /// このGameMainクラスのための RequestShutdown 関数です。
        /// </summary>>
        /// <returns>修了を許可する場合はtrueを、禁止する場合はfalseを返します</returns>
        private static bool Internal_RequestShutdown()
        {
            // 終了処理の要求をして結果をそのまま返す
            return CurrentContext.RequestShutdown();
        }


        /// <summary>
        /// このGameMainクラスのための Shutdown 関数です。
        /// </summary>
        private static void Internal_Shutdown()
        {
            // 終了を叩く
            CurrentContext.Shutdown();
        }


        /// <summary>
        /// ゲームの起動処理を行います。
        /// サービスの初期化や他のサブシステムの初期化などを主に行います。
        /// </summary>
        protected virtual void Startup()
        {
        }


        /// <summary>
        /// ゲームの終了処理の要求を処理します。
        /// ゲームが終了してよいのかどうかを判断し修了のコントロールを行います。
        /// </summary>
        /// <returns>修了を許可する場合はtrueを、禁止する場合はfalseを返します</returns>
        protected virtual bool RequestShutdown()
        {
            // 通常は終了を許容する
            return true;
        }


        /// <summary>
        /// ゲームの終了処理を行います。
        /// サービスの終了処理や他のサブシステムの終了処理などを主に行います。
        /// </summary>
        protected virtual void Shutdown()
        {
        }
    }
}