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
        /// テスト確認用ステートAクラスです
        /// </summary>
        private class SampleAState : ImtStateMachine<ImtStateMachineTest>.State
        {
            // 定数定義
            public const int SampleValue = 100;



            /// <summary>
            /// ステートの開始処理を行います
            /// </summary>
            protected internal override void Enter()
            {
                // コンテキストの値を設定する
                Context.sampleValue = SampleValue;
            }
        }



        /// <summary>
        /// テスト確認用ステートBクラスです
        /// </summary>
        private class SampleBState : ImtStateMachine<ImtStateMachineTest>.State
        {
            // 定数定義
            public const int SampleValue = 200;



            /// <summary>
            /// ステートの開始処理を行います
            /// </summary>
            protected internal override void Enter()
            {
                // コンテキストの値を設定する
                Context.sampleValue = SampleValue;
            }
        }



        /// <summary>
        /// Exit 時に SendEvent する許されざるステートクラスです
        /// </summary>
        private class ExitSendEventState : ImtStateMachine<ImtStateMachineTest>.State
        {
            /// <summary>
            /// ステートの終了処理を行います
            /// </summary>
            protected internal override void Exit()
            {
                // 許されざる SendEvent の呼び出しを行う（Assertは、テスト関数側のUpdateで引っ掛ける）
                StateMachine.SendEvent(Context.sampleValue);
            }
        }



        // メンバ変数定義
        private int sampleValue;



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
            // ステートマシンのインスタンスを生成する
            var stateMachine = new ImtStateMachine<ImtStateMachineTest>(this);


            // イベントIDの重複しないステート遷移なら問題ない事を確認する
            Assert.DoesNotThrow(() => stateMachine.AddTransition<SampleAState, SampleBState>(1)); // A[1] -> B
            Assert.DoesNotThrow(() => stateMachine.AddTransition<SampleAState, SampleBState>(2)); // A[2] -> B
            Assert.DoesNotThrow(() => stateMachine.AddTransition<SampleAState, SampleAState>(3)); // A[3] -> A
            Assert.DoesNotThrow(() => stateMachine.AddTransition<SampleBState, SampleAState>(1)); // B[1] -> A
            Assert.DoesNotThrow(() => stateMachine.AddTransition<SampleBState, SampleAState>(2)); // B[2] -> A
            Assert.DoesNotThrow(() => stateMachine.AddTransition<SampleBState, SampleBState>(3)); // B[3] -> B
            Assert.DoesNotThrow(() => stateMachine.AddAnyTransition<SampleAState>(1)); // Any[1] -> A
            Assert.DoesNotThrow(() => stateMachine.AddAnyTransition<SampleBState>(2)); // Any[2] -> B


            // 既に同じイベントIDの遷移が存在した場合、例外が吐かれる事を確認する
            Assert.Throws<ArgumentException>(() => stateMachine.AddTransition<SampleAState, SampleBState>(1)); // A[1] -> B
            Assert.Throws<ArgumentException>(() => stateMachine.AddTransition<SampleBState, SampleAState>(2)); // B[2] -> A
            Assert.Throws<ArgumentException>(() => stateMachine.AddAnyTransition<SampleAState>(1)); // Any[1] -> A


            // 開始ステートの設定はまだ可能であることを確認する
            Assert.DoesNotThrow(() => stateMachine.SetStartState<SampleBState>());
            Assert.DoesNotThrow(() => stateMachine.SetStartState<SampleAState>());


            // ステートマシンを起動する
            stateMachine.Update();


            // 起動してしまったステートマシンは、二度と遷移テーブルを編集出来ないことを確認する（また、既に登録済み遷移でも ArgumentException ではなく起動中例外が吐かれることを確認する）
            Assert.Throws<InvalidOperationException>(() => stateMachine.AddTransition<SampleAState, SampleBState>(1)); // A[1] -> B
            Assert.Throws<InvalidOperationException>(() => stateMachine.AddTransition<SampleAState, SampleBState>(4)); // A[4] -> B
            Assert.Throws<InvalidOperationException>(() => stateMachine.AddTransition<SampleBState, SampleAState>(1)); // B[1] -> A
            Assert.Throws<InvalidOperationException>(() => stateMachine.AddTransition<SampleBState, SampleAState>(4)); // B[4] -> A
            Assert.Throws<InvalidOperationException>(() => stateMachine.AddAnyTransition<SampleAState>(1)); // Any[1] -> A
            Assert.Throws<InvalidOperationException>(() => stateMachine.AddAnyTransition<SampleAState>(3)); // Any[3] -> A


            // 本来登録されていないはずの遷移イベント（A[4] -> B）を送って、ステートマシンを更新後、遷移されていないことを確認する
            Assert.IsTrue(stateMachine.IsCurrentState<SampleAState>());
            stateMachine.SendEvent(4);
            stateMachine.Update();
            Assert.IsTrue(stateMachine.IsCurrentState<SampleAState>());
        }


        /// <summary>
        /// ステートマシンのイベント送信テストをします
        /// </summary>
        [Test]
        public void SendEventTest()
        {
            // ステートマシンのインスタンスを生成してサクッと遷移テーブルを構築する
            var stateMachine = new ImtStateMachine<ImtStateMachineTest>(this);
            stateMachine.AddTransition<SampleAState, SampleBState>(1);
            stateMachine.AddTransition<SampleBState, SampleAState>(1);
            stateMachine.AddTransition<SampleBState, ExitSendEventState>(2);
            stateMachine.AddTransition<ExitSendEventState, SampleAState>(1);
            stateMachine.SetStartState<SampleAState>();


            // ステートマシンが未起動状態でSendEventしたら例外が吐かれることを確認する
            Assert.Throws<InvalidOperationException>(() => stateMachine.SendEvent(1));


            // ステートマシンを起動して ExitSendEventState へ遷移するイベントを送って正しく終了することを確認する（ついでに遷移準備済みの確認もする）
            stateMachine.Update(); // Wakeup
            Assert.DoesNotThrow(() => stateMachine.SendEvent(1));
            Assert.IsFalse(stateMachine.SendEvent(1)); // 遷移準備済みなので false が返ってくるはず
            stateMachine.Update(); // A -> B
            Assert.IsTrue(stateMachine.SendEvent(2)); // 初遷移なので true が返ってくるはず
            stateMachine.Update(); // B -> ExitSendEvent
            Assert.DoesNotThrow(() => stateMachine.SendEvent(1));


            // このタイミングの Update にて ExitSendEvent -> A になる予定だが
            // ExitSendEvent.Exit で 許されざる SendEvent をしているため例外が吐かれる
            Assert.Throws<InvalidOperationException>(() => stateMachine.Update());
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


        /// <summary>
        /// ステートマシンのステートスタック操作のテストをします
        /// </summary>
        [Test]
        public void StateStackTest()
        {
        }
    }
}