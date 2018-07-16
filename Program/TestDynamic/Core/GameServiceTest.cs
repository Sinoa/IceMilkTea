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

using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace IceMilkTeaTestDynamic.Core
{
    /// <summary>
    /// GameServiceの挙動をテストをするクラスです
    /// </summary>
    public class GameServiceTest
    {
        [UnityTest]
        public IEnumerator RunGameServiceTestMonoBehaviourTest()
        {
            // コンポーネント式テストクラスのインスタンスを生成してテストする
            yield return new MonoBehaviourTest<GameServiceTestMonoBehaviourTest>();
        }
    }



    /// <summary>
    /// GameServiceをテストするためのサービスクラスです
    /// </summary>
    public class GameServiceTestService
    {
    }



    /// <summary>
    /// GameServiceの動作を調べるためのテストコンポーネントクラスです
    /// </summary>
    public class GameServiceTestMonoBehaviourTest : MonoBehaviour, IMonoBehaviourTest
    {
        /// <summary>
        /// テストが完了したかどうか
        /// </summary>
        public bool IsTestFinished => true;
    }
}