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
    /// Collider クラスの拡張関数実装用クラスです
    /// </summary>
    public static class ColliderExtension
    {
        /// <summary>
        /// 自身のゴースト状態を設定して、全てのコライダーとの衝突を無いことにするかどうかを設定します
        /// </summary>
        /// <param name="collider">ゴーストになるコライダー</param>
        /// <param name="ghost">ゴーストになる場合は true を、ゴースト状態を解除する場合は false</param>
        public static void SetGhost(this Collider collider, bool ghost)
        {
            // 全てのコライダーを取得してゴースト状態を設定する
            SetGhost(collider, ghost, UnityEngine.Object.FindObjectsOfType<Collider>());
        }


        /// <summary>
        /// 自身のゴースト状態を設定して、指定された他のコライダーとの衝突を無いことにするかどうかを設定します
        /// </summary>
        /// <param name="collider">ゴーストになるコライダー</param>
        /// <param name="ghost">ゴーストになる場合は true を、ゴースト状態を解除する場合は false</param>
        /// <param name="otherCollider">自身のゴースト状態として認識する他のコライダー</param>
        /// <exception cref="ArgumentNullException">otherCollider が null です</exception>
        public static void SetGhost(this Collider collider, bool ghost, Collider[] otherCollider)
        {
            // null を渡されたら
            if (otherCollider == null)
            {
                // 無視設定が出来ない
                throw new ArgumentNullException(nameof(otherCollider));
            }


            // 指定されたコライダー分回る
            foreach (var other in otherCollider)
            {
                // 自身と他との衝突判定を無視するかどうかを設定
                Physics.IgnoreCollision(collider, other, ghost);
            }
        }
    }
}