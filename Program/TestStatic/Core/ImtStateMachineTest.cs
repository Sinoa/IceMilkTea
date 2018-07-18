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
using IceMilkTea.Core;
using NUnit.Framework;

namespace IceMilkTeaTestStatic.Core
{
    /// <summary>
    /// IceMilkTea の ImtStateMachine クラスのテストをするクラスです
    /// </summary>
    public class ImtStateMachineTest
    {
        /// <summary>
        /// ステートマシンのコンストラクタのテストをします
        /// </summary>
        [Test]
        public void ConstructorTest()
        {
            // null渡しによるインスタンスの生成は許されていないのでテスト
            Assert.Throws<ArgumentNullException>(() => new ImtStateMachine<ImtStateMachineTest>(null));
            Assert.DoesNotThrow(() => new ImtStateMachine<ImtStateMachineTest>(this));
        }


        /// <summary>
        /// ステートマシンのステート遷移テーブルの構築テストをします
        /// </summary>
        [Test]
        public void StateTransitionTableBuildTest()
        {
        }


        /// <summary>
        /// ステートマシンのイベント送信テストをします
        /// </summary>
        [Test]
        public void SendEventTest()
        {
        }


        /// <summary>
        /// ステートの遷移テストをします
        /// </summary>
        [Test]
        public void StateTransitionTest()
        {
        }


        /// <summary>
        /// ステートによるイベントガードのテストをします
        /// </summary>
        [Test]
        public void StateGuardEventTest()
        {
        }


        /// <summary>
        /// ステートの更新テストをします
        /// </summary>
        [Test]
        public void StateUpdateTest()
        {
        }


        /// <summary>
        /// ステートマシンの現在処理中ステートのチェックテストをします
        /// </summary>
        [Test]
        public void CurrentStateCheckTest()
        {
        }
    }
}