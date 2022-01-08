﻿// zlib/libpng License
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

namespace IceMilkTea.Core
{
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
        /// サービスをシャットダウンします
        /// </summary>
        protected internal virtual void Shutdown()
        {
        }
    }
}