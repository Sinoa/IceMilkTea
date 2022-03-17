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
}