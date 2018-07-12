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

namespace IceMilkTea.Core
{
    /// <summary>
    /// ゲームが終了要求に対する答えを表します
    /// </summary>
    public enum GameShutdownAnswer
    {
        /// <summary>
        /// ゲームが終了することを許可します
        /// </summary>
        Approve,

        /// <summary>
        /// ゲームが終了することを拒否します
        /// </summary>
        Reject,
    }



    /// <summary>
    /// ゲームサービスが動作を開始するための情報を保持する構造体です
    /// </summary>
    public struct GameServiceStartupInfo
    {
        /// <summary>
        /// サービスが更新処理として必要としている更新関数テーブル
        /// </summary>
        public Dictionary<GameServiceUpdateTiming, Action> UpdateFunctionTable { get; set; }
    }



    /// <summary>
    /// ゲームのサブシステムをサービスとして提供するための基本クラスです。
    /// ゲームのサブシステムを実装する場合は、このクラスを継承し適切な振る舞いを実装してください。
    /// </summary>
    public abstract class GameService
    {
        /// <summary>
        /// サービスを起動します。
        /// </summary>
        /// <param name="info">サービスが起動する時に必要とする情報を設定します</param>
        protected internal virtual void Startup(out GameServiceStartupInfo info)
        {
            // 特に何もしない起動情報を設定して修了
            info = new GameServiceStartupInfo()
            {
                UpdateFunctionTable = null,
            };
        }


        /// <summary>
        /// ゲームアプリケーションが終了することを許可するかどうかを判断します。
        /// </summary>
        /// <returns>アプリケーションが終了することを許可する場合は GameShutdownAnswer.Approve を、許可しない場合は GameShutdownAnswer.Reject を返します</returns>
        protected internal virtual GameShutdownAnswer JudgeGameShutdown()
        {
            // 通常は許可をする
            return GameShutdownAnswer.Approve;
        }


        /// <summary>
        /// サービスをシャットダウンします
        /// </summary>
        protected internal virtual void Shutdown()
        {
        }
    }
}