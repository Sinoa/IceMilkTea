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
    /// コンテキストを持つことのできるステートマシンクラスです
    /// </summary>
    /// <typeparam name="TContext">このステートマシンが持つコンテキストのクラス型</typeparam>
    public class ImtStateMachine<TContext> where TContext : class
    {
        /// <summary>
        /// ステートマシンが処理する状態を表現するステートクラスです。
        /// </summary>
        public abstract class State
        {
            // メンバ変数定義
            internal Dictionary<int, State> transitionTable;
            internal ImtStateMachine<TContext> stateMachine;



            /// <summary>
            /// このステートが所属するステートマシン
            /// </summary>
            protected ImtStateMachine<TContext> StateMachine => stateMachine;


            /// <summary>
            /// このステートが所属するステートマシンが持っているコンテキスト
            /// </summary>
            protected TContext Context => stateMachine.Context;



            /// <summary>
            /// ステートに突入したときの処理を行います
            /// </summary>
            protected internal virtual void Enter()
            {
            }


            /// <summary>
            /// ステートを更新するときの処理を行います
            /// </summary>
            protected internal virtual void Update()
            {
            }


            /// <summary>
            /// ステートから脱出したときの処理を行います
            /// </summary>
            protected internal virtual void Exit()
            {
            }


            /// <summary>
            /// ステートマシンがイベントを受ける時に、このステートがそのイベントをガードします。
            /// </summary>
            /// <param name="eventId">受けるイベントID</param>
            /// <param name="eventArg">受けるイベントの引数オブジェクト</param>
            /// <returns>イベントの受付をガードする場合は true を、ガードせずイベントを受け付ける場合は false を返します</returns>
            protected internal virtual bool GuardEvent(int eventId, object eventArg)
            {
                // 通常はガードしない
                return false;
            }


            /// <summary>
            /// ステートマシンが遷移をするとき、このステートがその遷移をガードします。
            /// </summary>
            /// <param name="eventId">遷移する理由になったイベントID</param>
            /// <param name="eventArg">イベントの引数オブジェクト</param>
            /// <returns>遷移をガードする場合は true を、ガードせず遷移を許す場合は false を返します</returns>
            protected internal virtual bool GuardTransition(int eventId, object eventArg)
            {
                // 通常はガードしない
                return false;
            }
        }



        /// <summary>
        /// ステートマシンのUpdate状態を表現します
        /// </summary>
        private enum UpdateState
        {
            /// <summary>
            /// アイドリング中です。つまり何もしていません
            /// </summary>
            Idle,

            /// <summary>
            /// ステートの突入処理中です
            /// </summary>
            Enter,

            /// <summary>
            /// ステートの更新処理中です
            /// </summary>
            Update,

            /// <summary>
            /// ステートの脱出処理中です
            /// </summary>
            Exit,
        }



        /// <summary>
        /// ステートマシンで "任意" を表現する特別なステートクラスです
        /// </summary>
        private sealed class AnyState : State { }



        // メンバ変数定義
        private UpdateState updateState;
        private List<State> stateList;
        private State currentState;
        private State nextState;



        #region プロパティ定義
        /// <summary>
        /// ステートマシンが保持しているコンテキスト
        /// </summary>
        public TContext Context { get; private set; }


        /// <summary>
        /// ステートマシンが起動しているかどうか
        /// </summary>
        public bool Running => currentState != null;
        #endregion



        /// <summary>
        /// ImtStateMachine のインスタンスを初期化します
        /// </summary>
        /// <param name="context">このステートマシンが持つコンテキスト</param>
        /// <exception cref="ArgumentNullException">context が null です</exception>
        public ImtStateMachine(TContext context)
        {
            // 渡されたコンテキストがnullなら
            if (context == null)
            {
                // nullは許されない
                throw new ArgumentNullException(nameof(context));
            }


            // メンバの初期化をする
            Context = context;
            stateList = new List<State>();
            updateState = UpdateState.Idle;


            // この時点で任意ステートのインスタンスを作ってしまう
            GetOrCreateState<AnyState>();
        }


        #region ステート遷移テーブル操作系
        /// <summary>
        /// ステートの任意遷移構造を追加します。
        /// この関数は、遷移元が任意の状態からの遷移を希望する場合に利用してください。
        /// 任意の遷移は、通常の遷移（Any以外の遷移元）より優先度が低いことにも、注意をしてください。
        /// また、ステートの遷移テーブル設定はステートマシンが起動する前に完了しなければなりません。
        /// </summary>
        /// <typeparam name="TNextState">任意状態から遷移する先になるステートの型</typeparam>
        /// <param name="eventId">遷移する条件となるイベントID</param>
        /// <exception cref="ArgumentException">既に同じ eventId が設定された遷移先ステートが存在します</exception>
        /// <exception cref="InvalidOperationException">ステートマシンが既に起動中です</exception>
        public void AddAnyTransition<TNextState>(int eventId) where TNextState : State, new()
        {
            // 単純に遷移元がAnyStateなだけの単純な遷移追加関数を呼ぶ
            AddTransition<AnyState, TNextState>(eventId);
        }


        /// <summary>
        /// ステートの遷移構造を追加します。
        /// また、ステートの遷移テーブル設定はステートマシンが起動する前に完了しなければなりません。
        /// </summary>
        /// <typeparam name="TPrevState">遷移する元になるステートの型</typeparam>
        /// <typeparam name="TNextState">遷移する先になるステートの型</typeparam>
        /// <param name="eventId">遷移する条件となるイベントID</param>
        /// <exception cref="ArgumentException">既に同じ eventId が設定された遷移先ステートが存在します</exception>
        /// <exception cref="InvalidOperationException">ステートマシンが既に起動中です</exception>
        public void AddTransition<TPrevState, TNextState>(int eventId) where TPrevState : State, new() where TNextState : State, new()
        {
            // ステートマシンが起動してしまっている場合は
            if (Running)
            {
                // もう設定できないので例外を吐く
                throw new InvalidOperationException("ステートマシンが既に起動中です");
            }


            // 遷移元と遷移先のステートインスタンスを取得
            var prevState = GetOrCreateState<TPrevState>();
            var nextState = GetOrCreateState<TNextState>();


            // 遷移元ステートの遷移テーブルに既に同じイベントIDが存在していたら
            if (prevState.transitionTable.ContainsKey(eventId))
            {
                // 上書き登録を許さないので例外を吐く
                throw new ArgumentException($"ステート'{prevState.GetType().Name}'には、既にイベントID'{eventId}'の遷移が設定済みです");
            }


            // 遷移テーブルに遷移を設定する
            prevState.transitionTable[eventId] = nextState;
        }
        #endregion


        #region ステートマシン制御系
        /// <summary>
        /// 現在実行中のステートが、指定されたステートかどうかを調べます。
        /// </summary>
        /// <typeparam name="TState">確認するステートの型</typeparam>
        /// <returns>指定されたステートの状態であれば true を、異なる場合は false を返します</returns>
        /// <exception cref="InvalidOperationException">ステートマシンは、まだ起動していません</exception>
        public bool IsCurrentState<TState>() where TState : State
        {
            // そもそもまだ現在実行中のステートが存在していないなら
            if (!Running)
            {
                // まだ起動すらしていないので例外を吐く
                throw new InvalidOperationException("ステートマシンは、まだ起動していません");
            }


            // 現在のステートと型が一致するかの条件式の結果をそのまま返す
            return currentState.GetType() == typeof(TState);
        }


        /// <summary>
        /// ステートマシンが起動する時に、最初に開始するステートを設定します。
        /// </summary>
        /// <typeparam name="TStartState">ステートマシンが起動時に開始するステートの型</typeparam>
        /// <exception cref="InvalidOperationException">ステートマシンは、既に起動済みです</exception>
        public void SetStartState<TStartState>() where TStartState : State, new()
        {
            // 既にステートマシンが起動してしまっている場合は
            if (Running)
            {
                // 起動してしまったらこの関数の操作は許されない
                throw new InvalidOperationException("ステートマシンは、既に起動済みです");
            }


            // 次に処理するステートの設定をする
            nextState = GetOrCreateState<TStartState>();
        }


        public void SendEvent(int eventId)
        {
        }


        public void SendEvent(int eventId, object eventArg)
        {
        }


        /// <summary>
        /// ステートマシンの状態を更新します。
        /// ステートマシンの現在処理しているステートの更新を行いますが、まだ未起動の場合は SetStartState 関数によって設定されたステートが起動します。
        /// また、ステートマシンが初回起動時の場合、ステートのUpdateは呼び出されず、次の更新処理が実行される時になります。
        /// </summary>
        /// <exception cref="InvalidOperationException">現在のステートマシンは、既に更新処理を実行しています</exception>
        /// <exception cref="InvalidOperationException">開始ステートが設定されていないため、ステートマシンの起動が出来ません</exception>
        public void Update()
        {
            // もしステートマシンの更新状態がアイドリング以外だったら
            if (updateState != UpdateState.Idle)
            {
                // 多重でUpdateが呼び出せない例外を吐く
                throw new InvalidOperationException("現在のステートマシンは、既に更新処理を実行しています");
            }


            // まだ未起動なら
            if (!Running)
            {
                // 次に処理するべきステート（つまり起動開始ステート）が未設定なら
                if (nextState == null)
                {
                    // 起動が出来ない例外を吐く
                    throw new InvalidOperationException("開始ステートが設定されていないため、ステートマシンの起動が出来ません");
                }


                // 現在処理中ステートとして設定する
                currentState = nextState;
                nextState = null;


                // Enter処理中であることを設定してEnterを呼ぶ
                updateState = UpdateState.Enter;
                currentState.Enter();


                // 次に遷移するステートが無いなら
                if (nextState == null)
                {
                    // 起動処理は終わったので一旦終わる
                    updateState = UpdateState.Idle;
                    return;
                }
            }


            // 次に遷移するステートが存在していないなら
            if (nextState == null)
            {
                // Update処理中であることを設定してUpdateを呼ぶ
                updateState = UpdateState.Update;
                currentState.Update();
            }


            // 次に遷移するステートが存在している間ループ
            while (nextState != null)
            {
                // Exit処理中であることを設定してExit処理を呼ぶ
                updateState = UpdateState.Exit;
                currentState.Exit();


                // 次のステートに切り替える
                currentState = nextState;
                nextState = null;


                // Enter処理中であることを設定してEnterを呼ぶ
                updateState = UpdateState.Enter;
                currentState.Enter();
            }


            // 更新処理が終わったらアイドリングに戻る
            updateState = UpdateState.Idle;
        }
        #endregion


        #region 内部ロジック系
        /// <summary>
        /// 指定されたステートの型のインスタンスを取得しますが、存在しない場合は生成してから取得します。
        /// 生成されたインスタンスは、次回から取得されるようになります。
        /// </summary>
        /// <typeparam name="TState">取得、または生成するステートの型</typeparam>
        /// <returns>取得、または生成されたステートのインスタンスを返します</returns>
        private TState GetOrCreateState<TState>() where TState : State, new()
        {
            // 要求ステートの型を取得
            var requestStateType = typeof(TState);


            // ステートの数分回る
            foreach (var state in stateList)
            {
                // もし該当のステートの型と一致するインスタンスなら
                if (state.GetType() == requestStateType)
                {
                    // そのインスタンスを返す
                    return (TState)state;
                }
            }


            // ループから抜けたのなら、型一致するインスタンスが無いという事なのでインスタンスを生成してキャッシュする
            var newState = new TState();
            stateList.Add(newState);


            // 新しいステートに、自身の参照と遷移テーブルのインスタンスの初期化も行って返す
            newState.stateMachine = this;
            newState.transitionTable = new Dictionary<int, State>();
            return newState;
        }
        #endregion
    }
}