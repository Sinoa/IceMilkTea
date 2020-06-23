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
        #region テスト用クラスの定義
        /// <summary>
        /// 各種テスト確認用ステートの基本ステートクラスです
        /// </summary>
        private abstract class SampleBaseState : ImtStateMachine<ImtStateMachineTest, int>.State
        {
            /// <summary>
            /// ステート開始時のSampleValue値
            /// </summary>
            public abstract int SampleValueEnter { get; }


            /// <summary>
            /// ステート更新時のSampleValue値
            /// </summary>
            public abstract int SampleValueUpdate { get; }



            /// <summary>
            /// ステートの開始処理を行います
            /// </summary>
            protected internal override void Enter()
            {
                // コンテキストの値を設定する
                Context.sampleValue = SampleValueEnter;
            }


            /// <summary>
            /// ステートの更新処理を行います
            /// </summary>
            protected internal override void Update()
            {
                // コンテキストの値を設定する
                Context.sampleValue = SampleValueUpdate;
            }
        }



        /// <summary>
        /// テスト確認用ステートAクラスです
        /// </summary>
        private class SampleAState : SampleBaseState
        {
            // 定数定義とプロパティ実装
            public const int EnterValue = 100;
            public const int UpdateValue = 110;
            public override int SampleValueEnter => EnterValue;
            public override int SampleValueUpdate => UpdateValue;


            /// <summary>
            /// ステートの終了処理を行います
            /// </summary>
            protected internal override void Exit()
            {
                // ステートAが終了した証明
                Context.stateAExited = true;
            }
        }



        /// <summary>
        /// テスト確認用ステートBクラスです
        /// </summary>
        private class SampleBState : SampleBaseState
        {
            // 定数定義とプロパティ実装
            public const int EnterValue = 200;
            public const int UpdateValue = 210;
            public override int SampleValueEnter => EnterValue;
            public override int SampleValueUpdate => UpdateValue;


            /// <summary>
            /// ステートの終了処理を行います
            /// </summary>
            protected internal override void Exit()
            {
                // ステートBが終了した証明
                Context.stateBExited = true;
            }
        }



        /// <summary>
        /// テスト確認用ステートCクラスです
        /// </summary>
        private class SampleCState : SampleBaseState
        {
            // 定数定義とプロパティ実装
            public const int EnterValue = 300;
            public const int UpdateValue = 310;
            public override int SampleValueEnter => EnterValue;
            public override int SampleValueUpdate => UpdateValue;


            /// <summary>
            /// ステートの終了処理を行います
            /// </summary>
            protected internal override void Exit()
            {
                // ステートCが終了した証明
                Context.stateCExited = true;
            }
        }



        /// <summary>
        /// Exit 時に SendEvent する許されざるステートクラスです
        /// </summary>
        private class ExitSendEventState : ImtStateMachine<ImtStateMachineTest, int>.State
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



        /// <summary>
        /// このステートに遷移したら直ちに SendEvent を行うステートクラスです
        /// </summary>
        private class ImmediateTransitionState : ImtStateMachine<ImtStateMachineTest, int>.State
        {
            // 定数定義
            public const int EventId = 12345;



            /// <summary>
            /// ステートの開始処理を行います
            /// </summary>
            protected internal override void Enter()
            {
                // 直ちにSendEventして遷移準備に入る（遷移テーブルが構築されていれば）
                StateMachine.SendEvent(EventId);
            }


            /// <summary>
            /// ステートの更新処理を行います
            /// </summary>
            protected internal override void Update()
            {
                // 触れてないフラグに思いっきり触れているように見えるが Enter でSendEventしているため
                // 遷移に成功していれば、まず触れることはないので安心
                Context.dontTouch = true;
            }


            /// <summary>
            /// ステートの終了処理を行います
            /// </summary>
            protected internal override void Exit()
            {
                // 直ちに遷移した証明
                Context.immediateExited = true;
            }
        }


        /// <summary>
        /// このステートに遷移して更新処理された時に SendEvent を行うステートクラスです
        /// </summary>
        private class UpdateTransitionState : ImtStateMachine<ImtStateMachineTest, int>.State
        {
            // 定数定義
            public const int EvnetId = 123456;
            public const int EnterValue = 300;



            /// <summary>
            /// ステートの開始処理を行います
            /// </summary>
            protected internal override void Enter()
            {
                // サンプル値を設定する
                Context.sampleValue = EnterValue;
            }


            /// <summary>
            /// ステートの更新処理を行います
            /// </summary>
            protected internal override void Update()
            {
                // SendEventして遷移準備に入る（遷移テーブルが構築されていれば）
                StateMachine.SendEvent(EvnetId);
            }


            /// <summary>
            /// ステートの終了処理を行います
            /// </summary>
            protected internal override void Exit()
            {
                // 遷移した証明
                Context.updateExited = true;
            }
        }


        /// <summary>
        /// コンテキストのガード有効値に基づいて、ガード制御を行うステートクラスです
        /// </summary>
        private class ProGuardState : ImtStateMachine<ImtStateMachineTest, int>.State
        {
            /// <summary>
            /// ステートマシンのイベントをガードします
            /// </summary>
            /// <param name="eventId">送られたイベントID</param>
            /// <returns>ガードする場合は true を、ガードしない場合は false を返します</returns>
            protected internal override bool GuardEvent(int eventId)
            {
                // 送られてきたイベントIDをサンプル値に設定して、コンテキストのガード有効値をそのまま返す
                Context.sampleValue = eventId;
                return Context.enableGuardEvent;
            }


            /// <summary>
            /// ステートマシンのステートスタックポップをガードします
            /// </summary>
            /// <returns>ガードする場合は true を、ガードしない場合は false を返します</returns>
            protected internal override bool GuardPop()
            {
                // コンテキストのガード有効値をそのまま返す
                return Context.enableGuardPop;
            }
        }



        /// <summary>
        /// ステート開始時にアップデートプロパティをキャプチャするステートクラスです
        /// </summary>
        private class EnterUpdateCaptureState : ImtStateMachine<ImtStateMachineTest, int>.State
        {
            /// <summary>
            /// ステートの開始処理を行います
            /// </summary>
            protected internal override void Enter()
            {
                // Updateプロパティを設定する
                Context.updateCapture = StateMachine.Updating;
            }
        }



        /// <summary>
        /// ステート更新時にアップデートプロパティをキャプチャするステートクラスです
        /// </summary>
        private class UpdateUpdateCaptureState : ImtStateMachine<ImtStateMachineTest, int>.State
        {
            /// <summary>
            /// ステートの更新処理を行います
            /// </summary>
            protected internal override void Update()
            {
                // Updateプロパティを設定する
                Context.updateCapture = StateMachine.Updating;
            }
        }



        /// <summary>
        /// ステート終了時にアップデートプロパティをキャプチャするステートクラスです
        /// </summary>
        private class ExitUpdateCaptureState : ImtStateMachine<ImtStateMachineTest, int>.State
        {
            /// <summary>
            /// ステートの終了処理を行います
            /// </summary>
            protected internal override void Exit()
            {
                // Updateプロパティを設定する
                Context.updateCapture = StateMachine.Updating;
            }
        }


        /// <summary>
        /// Enter時に sampleValue が 0 の場合は、例外を発生させるステートクラスです。
        /// </summary>
        private class ForceEnterExceptionState : ImtStateMachine<ImtStateMachineTest, int>.State
        {
            /// <summary>
            /// ステートの開始処理を行います
            /// </summary>
            protected internal override void Enter()
            {
                // sampleValue が 0 なら
                if (Context.sampleValue == 0)
                {
                    // 例外を即座に発生
                    throw new InvalidOperationException("この Enter エラーを拾ってください");
                }
            }
        }


        /// <summary>
        /// Update時に sampleValue が 0 の場合は、例外を発生させるステートクラスです
        /// また例外モードが CatchStateException の場合は MyEventId の遷移イベントを送出します
        /// </summary>
        private class ForceUpdateExceptionState : ImtStateMachine<ImtStateMachineTest, int>.State
        {
            /// <summary>
            /// CatchStateException 時に送出するイベントID
            /// </summary>
            public const int MyEventId = -1;



            /// <summary>
            /// ステートの更新処理を行います
            /// </summary>
            protected internal override void Update()
            {
                // sampleValue が 0 なら
                if (Context.sampleValue == 0)
                {
                    // 例外を即座に発生
                    throw new InvalidOperationException("この Update エラーを拾ってください");
                }
            }


            /// <summary>
            /// ステートのエラーハンドリングを行います
            /// </summary>
            /// <param name="exception">発生した例外</param>
            /// <returns>例外をハンドリングした場合は true を、ハンドリング出来なかった場合は false を返します</returns>
            protected internal override bool Error(Exception exception)
            {
                // 自分の遷移IDを送出してハンドリング済みを返す
                stateMachine.SendEvent(MyEventId);
                return true;
            }
        }


        /// <summary>
        /// Exit時に sampleValue が 0 の場合は、例外を発生させるステートクラスです
        /// </summary>
        private class ForceExitExceptionState : ImtStateMachine<ImtStateMachineTest, int>.State
        {
            /// <summary>
            /// ステートの終了処理を行います
            /// </summary>
            protected internal override void Exit()
            {
                // sampleValue が 0 なら
                if (Context.sampleValue == 0)
                {
                    // 例外を即座に発生
                    throw new InvalidOperationException("この Exit エラーを拾ってください");
                }
            }
        }



        /// <summary>
        /// ステートクラスのインスタンス生成に失敗するテスト用クラスです
        /// </summary>
        private class CreateStateInstanceMissStateMachine : ImtStateMachine<ImtStateMachineTest>
        {
            /// <summary>
            /// CreateStateInstanceMissStateMachine クラスのインスタンスを初期化します
            /// </summary>
            /// <param name="context">このステートマシンが持つべきコンテキスト</param>
            public CreateStateInstanceMissStateMachine(ImtStateMachineTest context) : base(context)
            {
            }


            /// <summary>
            /// 必ずインスタンスの生成に失敗します
            /// </summary>
            /// <typeparam name="TState">生成するべきステートの型</typeparam>
            /// <returns>必ず null を返します</returns>
            protected override TState CreateStateInstance<TState>()
            {
                // 無条件で失敗する
                return null;
            }
        }
        #endregion



        // メンバ変数定義
        private int sampleValue;
        private bool stateAExited;
        private bool stateBExited;
        private bool stateCExited;
        private bool dontTouch;
        private bool immediateExited;
        private bool updateExited;
        private bool enableGuardEvent;
        private bool enableGuardPop;
        private bool updateCapture;



        /// <summary>
        /// ステートマシンのコンストラクタのテストをします
        /// </summary>
        [Test]
        public void ConstructorTest()
        {
            // null渡しによるインスタンスの生成は許されていないのでテスト
            Assert.Throws<ArgumentNullException>(() => new ImtStateMachine<ImtStateMachineTest, int>(null));
            Assert.DoesNotThrow(() => new ImtStateMachine<ImtStateMachineTest, int>(this));
        }


        /// <summary>
        /// ステートマシンのステート遷移テーブルの構築テストをします
        /// </summary>
        [Test]
        public void StateTransitionTableBuildTest()
        {
            // ステートマシンのインスタンスを生成する
            var stateMachine = new ImtStateMachine<ImtStateMachineTest, int>(this);


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
            var stateMachine = new ImtStateMachine<ImtStateMachineTest, int>(this);
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
            stateMachine.AllowRetransition = true; // 再遷移を許可する
            Assert.IsTrue(stateMachine.SendEvent(2)); // 本来なら遷移済みなので false だが、再遷移を許可しているので true になる
            stateMachine.AllowRetransition = false; // 再遷移を禁止する
            stateMachine.Update(); // B -> ExitSendEvent


            // SendEventする前にスタックに積んで下ろした時SendEventが失敗することを確認する（PopStateが遷移状態にさせるため）
            stateMachine.PushState();
            stateMachine.PopState();
            Assert.IsFalse(stateMachine.SendEvent(1));


            // このタイミングの Update にて ExitSendEvent -> ExitSendEvent になる予定だが
            // ExitSendEvent.Exit で 許されざる SendEvent をしているため例外が吐かれる
            Assert.Throws<InvalidOperationException>(() => stateMachine.Update());
        }


        /// <summary>
        /// ステートの遷移テストをします
        /// </summary>
        [Test]
        public void StateTransitionTest()
        {
            // 念の為検査用メンバ変数の初期化を行う
            sampleValue = 0;
            stateAExited = false;
            stateBExited = false;
            stateCExited = false;
            immediateExited = false;
            updateExited = false;
            dontTouch = false;


            // ステートマシンのインスタンスを生成してサクッと遷移テーブルを構築する
            var stateMachine = new ImtStateMachine<ImtStateMachineTest, int>(this);
            stateMachine.AddTransition<SampleAState, SampleBState>(1);
            stateMachine.AddTransition<SampleBState, SampleAState>(1);
            stateMachine.AddTransition<SampleCState, ImmediateTransitionState>(1);
            stateMachine.AddTransition<SampleCState, UpdateTransitionState>(2);
            stateMachine.AddTransition<ImmediateTransitionState, SampleAState>(ImmediateTransitionState.EventId);
            stateMachine.AddTransition<UpdateTransitionState, SampleBState>(UpdateTransitionState.EvnetId);
            stateMachine.AddAnyTransition<SampleCState>(10);
            stateMachine.SetStartState<SampleAState>();


            // ステートマシンを起動する
            stateMachine.Update();


            // サンプル値が、起動値と更新値の想定値になっていることを確認する（念の為IsCurrentStateも見る）
            Assert.IsTrue(stateMachine.IsCurrentState<SampleAState>());
            Assert.AreEqual(SampleAState.EnterValue, sampleValue);
            stateMachine.Update();
            Assert.IsTrue(stateMachine.IsCurrentState<SampleAState>());
            Assert.AreEqual(SampleAState.UpdateValue, sampleValue);


            // ステート遷移を行って一つ前のステートが終了し、サンプル値が、起動地と更新値の想定地になっていることを確認する
            stateMachine.SendEvent(1);
            stateMachine.Update();
            Assert.IsTrue(stateAExited);
            Assert.AreEqual(SampleBState.EnterValue, sampleValue);
            stateMachine.Update();
            Assert.AreEqual(SampleBState.UpdateValue, sampleValue);


            // 任意ステートからの遷移（イベントID10で、StateBステートからなく、任意遷移にはあるので）を行い同上の想定判断を確認する
            stateMachine.SendEvent(10);
            stateMachine.Update();
            Assert.IsTrue(stateBExited);
            Assert.AreEqual(SampleCState.EnterValue, sampleValue);
            stateMachine.Update();
            Assert.AreEqual(SampleCState.UpdateValue, sampleValue);


            // 直ちに遷移するステートに遷移を行い、直ちに遷移が行われたかの形跡を確認をする
            Assert.IsFalse(dontTouch);
            Assert.IsFalse(immediateExited);
            stateMachine.SendEvent(1);
            stateMachine.Update();
            Assert.IsTrue(stateCExited);
            Assert.AreEqual(SampleAState.EnterValue, sampleValue);
            Assert.IsFalse(dontTouch);
            Assert.IsTrue(immediateExited);


            // ひとまずステートCへ遷移する
            stateMachine.SendEvent(10);
            stateMachine.Update();


            // 更新時に遷移するステートに遷移を行い、想定されたサンプル値になるか確認後、更新時に遷移されたかも確認する
            stateMachine.SendEvent(2);
            stateMachine.Update();
            Assert.AreEqual(UpdateTransitionState.EnterValue, sampleValue);
            Assert.IsFalse(updateExited);
            stateMachine.Update();
            Assert.AreEqual(SampleBState.EnterValue, sampleValue);
            Assert.IsTrue(updateExited);
        }


        /// <summary>
        /// ステートによる遷移ガードのテストをします
        /// </summary>
        [Test]
        public void StateTransitionGuardTest()
        {
            // 念の為検査用メンバ変数の初期化を行う
            sampleValue = 0;


            // ステートマシンのインスタンスを生成してサクッと遷移テーブルを構築する
            var stateMachine = new ImtStateMachine<ImtStateMachineTest, int>(this);
            stateMachine.AddTransition<SampleAState, ProGuardState>(1);
            stateMachine.AddTransition<SampleBState, ProGuardState>(1);
            stateMachine.AddTransition<ProGuardState, SampleAState>(1);
            stateMachine.AddTransition<ProGuardState, SampleBState>(2);
            stateMachine.SetStartState<SampleAState>();


            // 起動して早速遷移する
            stateMachine.Update();
            stateMachine.SendEvent(1);
            stateMachine.Update();
            Assert.IsTrue(stateMachine.IsCurrentState<ProGuardState>());


            // イベント、ポップともに遷移をガードする状態にする
            enableGuardEvent = true;
            enableGuardPop = true;


            // StateAへ遷移したいがイベントがガードされて、StateBにも行こうとしてもガードされることを確認する
            Assert.IsFalse(stateMachine.SendEvent(1));
            Assert.AreEqual(1, sampleValue);
            Assert.IsFalse(stateMachine.SendEvent(2));
            Assert.AreEqual(2, sampleValue);


            // ガードステートをプッシュしてポップすらもガードされることを確認する
            stateMachine.PushState();
            Assert.IsFalse(stateMachine.PopState());


            // イベントのみガードを無効化してStateAへ遷移する準備をして成功したことを確認する
            enableGuardEvent = false;
            Assert.IsTrue(stateMachine.SendEvent(1));
            Assert.AreEqual(1, sampleValue);


            // 遷移をして希望のステートになっていることを確認
            stateMachine.Update();
            Assert.IsTrue(stateMachine.IsCurrentState<SampleAState>());


            // 本来はSendEventしてProGuardへ遷移するが、ステートスタックにはProGuardが積まれているので戻れる
            enableGuardPop = false;
            Assert.IsTrue(stateMachine.PopState());
            stateMachine.Update();


            // 戻ってこれた事を確認
            Assert.IsTrue(stateMachine.IsCurrentState<ProGuardState>());
        }


        /// <summary>
        /// ステートマシンの現在処理中ステートのチェックテストをします
        /// </summary>
        [Test]
        public void CurrentStateCheckTest()
        {
            // ステートマシンのインスタンスを生成してサクッと遷移テーブルを構築する
            var stateMachine = new ImtStateMachine<ImtStateMachineTest, int>(this);
            stateMachine.AddTransition<SampleAState, SampleBState>(1);
            stateMachine.AddTransition<SampleBState, SampleAState>(1);
            stateMachine.SetStartState<SampleAState>();


            // まだ、ステートマシンが起動していないので、現在のステート確認を叩くと例外が吐かれることを確認する
            Assert.Throws<InvalidOperationException>(() => stateMachine.IsCurrentState<SampleAState>());


            // ステートマシンを起動して、遷移をしながら想定の現在ステート確認結果が返ってくることを確認する
            stateMachine.Update();
            Assert.IsTrue(stateMachine.IsCurrentState<SampleAState>());
            Assert.IsFalse(stateMachine.IsCurrentState<SampleBState>());
            stateMachine.SendEvent(1);
            stateMachine.Update();
            Assert.IsFalse(stateMachine.IsCurrentState<SampleAState>());
            Assert.IsTrue(stateMachine.IsCurrentState<SampleBState>());
            stateMachine.SendEvent(1);
            stateMachine.Update();
            Assert.IsTrue(stateMachine.IsCurrentState<SampleAState>());
            Assert.IsFalse(stateMachine.IsCurrentState<SampleBState>());
        }


        /// <summary>
        /// ステートマシンのステートスタック操作のテストをします
        /// </summary>
        [Test]
        public void StateStackTest()
        {
            // ステートマシンのインスタンスを生成してサクッと遷移テーブルを構築する
            var stateMachine = new ImtStateMachine<ImtStateMachineTest, int>(this);
            stateMachine.AddTransition<SampleAState, SampleBState>(1);
            stateMachine.AddTransition<SampleBState, SampleCState>(1);
            stateMachine.SetStartState<SampleAState>();


            // ステートマシンが起動していない状態で、プッシュやポップをしようとすると例外が吐かれることを確認する
            Assert.Throws<InvalidOperationException>(() => stateMachine.PushState());
            Assert.Throws<InvalidOperationException>(() => stateMachine.PopState());
            Assert.Throws<InvalidOperationException>(() => stateMachine.PopAndDirectSetState());


            // ステートマシンを起動すれば例外が吐かれない事を確認する
            stateMachine.Update();
            Assert.DoesNotThrow(() => stateMachine.PushState());
            Assert.DoesNotThrow(() => stateMachine.PopAndDirectSetState());
            Assert.DoesNotThrow(() => stateMachine.PushState());
            Assert.DoesNotThrow(() => stateMachine.PopState());


            // ポップしたことで遷移準備状態になっているのでひとまず更新
            stateMachine.Update();


            // ステートA、B、Cで順にプッシュしながら遷移する
            Assert.IsTrue(stateMachine.IsCurrentState<SampleAState>());
            stateMachine.PushState(); // [A]
            stateMachine.SendEvent(1);
            stateMachine.Update();
            Assert.IsTrue(stateMachine.IsCurrentState<SampleBState>());
            stateMachine.PushState(); // [A,B]
            stateMachine.SendEvent(1);
            stateMachine.Update();
            Assert.IsTrue(stateMachine.IsCurrentState<SampleCState>());
            stateMachine.PushState(); // [A,B,C]


            // この段階でスタックには3段階のステートが積まれている状態のはず
            Assert.AreEqual(3, stateMachine.StackCount);


            // 本来 C->B->A の遷移テーブルは構築されていないので
            // このような遷移は不可能だが、今回はプッシュしながら遷移したので
            // ポップしながら状態を更新すれば、実現されることを確認する
            Assert.IsTrue(stateMachine.PopState()); // [A,B] -> C
            stateMachine.Update();
            Assert.IsTrue(stateMachine.IsCurrentState<SampleCState>());
            Assert.IsTrue(stateMachine.PopState()); // [A] -> B
            stateMachine.Update();
            Assert.IsTrue(stateMachine.IsCurrentState<SampleBState>());
            Assert.IsTrue(stateMachine.PopState()); // [] -> A
            stateMachine.Update();
            Assert.IsTrue(stateMachine.IsCurrentState<SampleAState>());
            Assert.AreEqual(0, stateMachine.StackCount);


            // もう一度プッシュして遷移をする
            stateMachine.PushState();
            stateMachine.SendEvent(1);
            stateMachine.Update();


            // 遷移を準備してから、ポップをしたいが、遷移準備が完了した状態ではポップできないことを確認する
            Assert.IsTrue(stateMachine.SendEvent(1));
            Assert.IsFalse(stateMachine.PopState());


            // ステートマシンを更新するとAではなくCへ遷移していることを確認し、ポップ出来て戻ってこれることを確認
            stateMachine.Update();
            Assert.IsTrue(stateMachine.IsCurrentState<SampleCState>());
            Assert.IsTrue(stateMachine.PopState());
            stateMachine.Update();
            Assert.IsTrue(stateMachine.IsCurrentState<SampleAState>());


            // ステートA、B、Cで順にプッシュしながら遷移する
            stateMachine.PushState(); // [A]
            stateMachine.SendEvent(1);
            stateMachine.Update();
            Assert.IsTrue(stateMachine.IsCurrentState<SampleBState>());
            stateMachine.PushState(); // [A,B]
            stateMachine.SendEvent(1);
            stateMachine.Update();
            Assert.IsTrue(stateMachine.IsCurrentState<SampleCState>());
            stateMachine.PushState(); // [A,B,C]


            // ステートスタックのトップをただただ捨てるだけして数が減っていることを確認する
            Assert.AreEqual(3, stateMachine.StackCount);
            stateMachine.PopAndDropState(); // [A,B] -> C
            Assert.AreEqual(2, stateMachine.StackCount);


            // この段階でポップ遷移をするとCは捨てられたのでBに戻ることを期待する
            // （直前のポップは、ただ捨てるだけで遷移はしないので、今回のポップは成功するはず）
            Assert.IsTrue(stateMachine.PopState()); // [A] -> B
            stateMachine.Update();
            Assert.IsTrue(stateMachine.IsCurrentState<SampleBState>());


            // ここでもう一度プッシュして、スタック段数が2段になることを確認
            stateMachine.PushState();
            Assert.AreEqual(2, stateMachine.StackCount);


            // 今度はスタックを空にして、空になったことを確認しポップも出来ないことを確認
            stateMachine.ClearStack();
            Assert.AreEqual(0, stateMachine.StackCount);
            Assert.IsTrue(stateMachine.IsCurrentState<SampleBState>());
            Assert.IsFalse(stateMachine.PopState());


            // ステート更新をしてもスタックから復帰できるわけでも無いので、ステータスが現状維持になっていることを確認
            stateMachine.Update();
            Assert.IsTrue(stateMachine.IsCurrentState<SampleBState>());
        }


        /// <summary>
        /// ステートマシンの提供するプロパティのテストをします
        /// </summary>
        [Test]
        public void StateMachinePropertyTest()
        {
            // 念の為検査用メンバ変数の初期化を行う
            updateCapture = false;


            // ステートマシンのインスタンスを生成してサクッと遷移テーブルを構築する
            var stateMachine = new ImtStateMachine<ImtStateMachineTest, int>(this);
            stateMachine.AddTransition<EnterUpdateCaptureState, UpdateUpdateCaptureState>(1);
            stateMachine.AddTransition<UpdateUpdateCaptureState, ExitUpdateCaptureState>(1);
            stateMachine.AddTransition<ExitUpdateCaptureState, UpdateUpdateCaptureState>(1);
            stateMachine.AddAnyTransition<ExitSendEventState>(10); // 自爆ステート
            stateMachine.SetStartState<EnterUpdateCaptureState>();


            // 未起動状態時のプロパティ値を検証する
            Assert.IsInstanceOf<ImtStateMachineTest>(stateMachine.Context);
            Assert.AreEqual(this, stateMachine.Context);
            Assert.IsFalse(stateMachine.Running);
            Assert.IsFalse(stateMachine.Updating);
            Assert.IsFalse(updateCapture);
            Assert.AreEqual(0, stateMachine.StackCount);


            // 起動してプロパティ値を検証する（Runningはステートマシンが起動しているかどうか、Updatingはステートマシンが Update 処理中かどうか）
            stateMachine.Update();
            Assert.IsInstanceOf<ImtStateMachineTest>(stateMachine.Context);
            Assert.AreEqual(this, stateMachine.Context);
            Assert.IsTrue(stateMachine.Running);
            Assert.IsFalse(stateMachine.Updating);
            Assert.IsTrue(updateCapture); // Updatingの代わりにステートで拾ってもらった
            Assert.AreEqual(0, stateMachine.StackCount);


            // 更新キャプチャをクリアして遷移
            updateCapture = false;
            stateMachine.SendEvent(1);
            stateMachine.Update();


            // プロパティ値を検証
            Assert.IsInstanceOf<ImtStateMachineTest>(stateMachine.Context);
            Assert.AreEqual(this, stateMachine.Context);
            Assert.IsTrue(stateMachine.Running);
            Assert.IsFalse(stateMachine.Updating);
            Assert.IsFalse(updateCapture); // Updateのタイミングで拾うのでまだfalse
            Assert.AreEqual(0, stateMachine.StackCount);


            // 遷移せず更新してプロパティ値を検証する
            stateMachine.Update();
            Assert.IsInstanceOf<ImtStateMachineTest>(stateMachine.Context);
            Assert.AreEqual(this, stateMachine.Context);
            Assert.IsTrue(stateMachine.Running);
            Assert.IsFalse(stateMachine.Updating);
            Assert.IsTrue(updateCapture); // Updatingの代わりにステートで拾ってもらった
            Assert.AreEqual(0, stateMachine.StackCount);


            // 更新キャプチャをクリアして遷移
            updateCapture = false;
            stateMachine.SendEvent(1);
            stateMachine.Update();


            // プロパティ値を検証
            Assert.IsInstanceOf<ImtStateMachineTest>(stateMachine.Context);
            Assert.AreEqual(this, stateMachine.Context);
            Assert.IsTrue(stateMachine.Running);
            Assert.IsFalse(stateMachine.Updating);
            Assert.IsFalse(updateCapture); // Exitのタイミングで拾うのでまだfalse
            Assert.AreEqual(0, stateMachine.StackCount);


            // 自爆ステートへ遷移
            stateMachine.SendEvent(10);
            stateMachine.Update();


            // プロパティ値を検証
            Assert.IsInstanceOf<ImtStateMachineTest>(stateMachine.Context);
            Assert.AreEqual(this, stateMachine.Context);
            Assert.IsTrue(stateMachine.Running);
            Assert.IsFalse(stateMachine.Updating);
            Assert.IsTrue(updateCapture); // Updatingの代わりにステートで拾ってもらった
            Assert.AreEqual(0, stateMachine.StackCount);


            // スタック操作をしまくってスタックカウント値が想定値になるか確認
            stateMachine.PushState();
            Assert.AreEqual(1, stateMachine.StackCount);
            stateMachine.PushState();
            stateMachine.PushState();
            stateMachine.PushState();
            Assert.AreEqual(4, stateMachine.StackCount);
            stateMachine.PopAndDropState();
            stateMachine.PopAndDropState();
            Assert.AreEqual(2, stateMachine.StackCount);
            stateMachine.ClearStack();
            Assert.AreEqual(0, stateMachine.StackCount);


            // キャプチャをクリアしてステートマシンを更新してまだ死なない事を確認
            updateCapture = false;
            Assert.DoesNotThrow(() => stateMachine.Update());


            // ステートのスタック操作を行って、想定スタック値になっていることを確認
            stateMachine.PushState();
            Assert.AreEqual(1, stateMachine.StackCount);
            stateMachine.PopState();
            Assert.IsInstanceOf<ImtStateMachineTest>(stateMachine.Context);
            Assert.AreEqual(this, stateMachine.Context);
            Assert.IsTrue(stateMachine.Running);
            Assert.IsFalse(stateMachine.Updating);
            Assert.IsFalse(updateCapture);
            Assert.AreEqual(0, stateMachine.StackCount);


            // このステート更新は自爆ステートによって例外が吐かれるので例外チェックを行う
            // また Update 中に例外により死んだステートマシンは、もうどうにもならないのでインスタンスの生成し直しが必要
            // このチェックは（UnhandledExceptionModeがThrowException（既定）である場合の挙動）
            Assert.Throws<InvalidOperationException>(() => stateMachine.Update());


            // MENO : UnhandledExceptionモード周りが実装された事により、死ぬことはなくなりました。
            // ステートマシンが死んだ状態プロパティ検証
            // もちろんステートマシンが死んでいるのでこのタイミングでも Updating は true を示す
            //Assert.IsInstanceOf<ImtStateMachineTest>(stateMachine.Context);
            //Assert.AreEqual(this, stateMachine.Context);
            //Assert.IsTrue(stateMachine.Running);
            //Assert.IsTrue(stateMachine.Updating);
            //Assert.IsFalse(updateCapture);
            //Assert.AreEqual(0, stateMachine.StackCount);
        }


        /// <summary>
        /// ステートマシンのUpdateで発生した例外のハンドリングを行うテスト
        /// </summary>
        [Test]
        public void UnhandledExceptionTest()
        {
            // 念の為検査用メンバ変数の初期化を行う
            sampleValue = 0;


            // ステートマシンのインスタンスを生成して、この救いようのない例外発生しまくり遷移テーブルを構築する
            var stateMachine = new ImtStateMachine<ImtStateMachineTest, int>(this);
            stateMachine.AddTransition<ForceEnterExceptionState, ForceUpdateExceptionState>(1);
            stateMachine.AddTransition<ForceUpdateExceptionState, ForceExitExceptionState>(1);
            stateMachine.AddTransition<ForceUpdateExceptionState, ForceExitExceptionState>(ForceUpdateExceptionState.MyEventId);
            stateMachine.AddTransition<ForceExitExceptionState, ForceEnterExceptionState>(1);
            stateMachine.SetStartState<ForceEnterExceptionState>();


            // まずは起動するが例外が発生するのと起動が未起動かチェック（例外が発生した場合は起動は失敗する）
            Assert.Throws<InvalidOperationException>(() => stateMachine.Update());
            Assert.IsFalse(stateMachine.Running);


            // 起動可能状態にして起動して確認
            sampleValue = 1;
            Assert.DoesNotThrow(() => stateMachine.Update());
            Assert.IsTrue(stateMachine.Running);
            sampleValue = 0;


            // 遷移してUpdateへ
            stateMachine.SendEvent(1);
            stateMachine.Update();


            // Updateで例外が発生するかどうか（既定は ThrowException）
            Assert.Throws<InvalidOperationException>(() => stateMachine.Update());


            // CatchExceptionモードに変えても例外が発生するかどうか（イベント未設定時は ThrowException と同等の動作）
            stateMachine.UnhandledExceptionMode = ImtStateMachineUnhandledExceptionMode.CatchException;
            Assert.Throws<InvalidOperationException>(() => stateMachine.Update());


            // イベントを設定しても false を返したら例外が発生するかどうか
            var returnValue = false;
            stateMachine.UnhandledException += error => returnValue;
            Assert.Throws<InvalidOperationException>(() => stateMachine.Update());


            // trueを返せば例外は発生しないはず
            returnValue = true;
            Assert.DoesNotThrow(() => stateMachine.Update());


            // 例外を発生させない様にして遷移する
            sampleValue = 1;
            stateMachine.SendEvent(1);
            stateMachine.Update();


            // Exit時に例外が発生するのを確認する
            stateMachine.UnhandledExceptionMode = ImtStateMachineUnhandledExceptionMode.ThrowException;
            sampleValue = 0;
            stateMachine.SendEvent(1);
            Assert.Throws<InvalidOperationException>(() => stateMachine.Update());


            // 例外が出ないようにしてUpdateまで遷移する
            sampleValue = 1;
            stateMachine.SendEvent(1);
            stateMachine.Update();
            stateMachine.SendEvent(1);
            stateMachine.Update();


            // ステート側エラーハンドリングモードにして例外が発生してもステート内でエラーハンドリングして遷移されることを確認
            stateMachine.UnhandledExceptionMode = ImtStateMachineUnhandledExceptionMode.CatchStateException;
            sampleValue = 0;
            stateMachine.SendEvent(1);
            Assert.DoesNotThrow(() => stateMachine.Update()); // ステート側でエラーハンドリングしているためココでは例外は発生しない
            stateMachine.Update();
            Assert.IsTrue(stateMachine.IsCurrentState<ForceExitExceptionState>());
        }


        /// <summary>
        /// ステートのインスタンス生成ロジックが正しく動作するかのテスト
        /// </summary>
        [Test]
        public void CreateStateInstanceTest()
        {
            // ステートマシンを生成して簡単にステートの生成に失敗するか試みる
            var stateMachine = new CreateStateInstanceMissStateMachine(this);
            Assert.Throws<InvalidOperationException>(() => stateMachine.AddAnyTransition<SampleAState>(0));


            // レジスタとアンレジスタ関数の失敗を確認する
            Assert.Throws<ArgumentNullException>(() => stateMachine.RegisterStateFactory(null));
            Assert.Throws<ArgumentNullException>(() => stateMachine.UnregisterStateFactory(null));


            // ステート生成関数を登録して今度は問題なく生成されるか確認をする
            stateMachine.RegisterStateFactory(type =>
            {
                if (typeof(CreateStateInstanceMissStateMachine.AnyState) == type)
                {
                    return new CreateStateInstanceMissStateMachine.AnyState();
                }
                else if (typeof(SampleAState) == type)
                {
                    return new SampleAState();
                }


                return null;
            });
            Assert.DoesNotThrow(() => stateMachine.AddAnyTransition<SampleAState>(0));
        }
    }
}