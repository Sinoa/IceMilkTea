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
using System.Threading;
using System.Threading.Tasks;

namespace IceMilkTea.Core
{
    /// <summary>
    /// コンテキストを持つことが出来るタスク駆動型ステートマシンです
    /// </summary>
    /// <typeparam name="TContext">持つコンテキストの型</typeparam>
    public class ImtTaskStateMachine<TContext>
    {
        // メンバ変数定義
        private readonly List<State> stateList;
        private State currentState;
        private State nextState;



        /// <summary>
        /// このステートマシンが保持しているコンテキスト
        /// </summary>
        public TContext Context { get; }



        /// <summary>
        /// TaskStateMachine クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="context">このステートマシンがもつコンテキスト</param>
        /// <exception cref="ArgumentNullException">context が null です</exception>
        public ImtTaskStateMachine(TContext context)
        {
            // 諸々の初期化
            Context = context ?? throw new ArgumentNullException(nameof(context));
            stateList = new List<State>();
            currentState = null;
            nextState = null;
        }


        #region ステートマシン制御系
        /// <summary>
        /// 現在実行中のステートが、指定されたステートかどうかを調べます。
        /// </summary>
        /// <typeparam name="TState">確認するステートの型</typeparam>
        /// <returns>指定されたステートの状態であれば true を、異なる場合は false を返します</returns>
        /// <exception cref="InvalidOperationException">ステートマシンは、まだ起動していません</exception>
        public bool IsCurrentState<TState>() where TState : State
        {
            // そもそもまだ現在実行中のステートが存在していないなら例外を投げる
            IfNotRunningThrowException();


            // 現在のステートと型が一致するかの条件式の結果をそのまま返す
            return currentState.GetType() == typeof(TState);
        }


        /// <summary>
        /// ステートマシンにイベントを送信して、ステート遷移の準備を行います。
        /// </summary>
        /// <remarks>
        /// ステートの遷移は直ちに行われず、次の Update が実行された時に遷移処理が行われます。
        /// また、この関数によるイベント受付優先順位は、一番最初に遷移を受け入れたイベントのみであり Update によって遷移されるまで、後続のイベントはすべて失敗します。
        /// ただし AllowRetransition プロパティに true が設定されている場合は、再遷移が許されます。
        /// さらに、イベントはステートの Enter または Update 処理中でも受け付けることが可能で、ステートマシンの Update 中に
        /// 何度も遷移をすることが可能ですが Exit 中でイベントを送ると、遷移中になるため例外が送出されます。
        /// </remarks>
        /// <param name="eventId">ステートマシンに送信するイベントID</param>
        /// <returns>ステートマシンが送信されたイベントを受け付けた場合は true を、イベントを拒否または、イベントの受付ができない場合は false を返します</returns>
        /// <exception cref="InvalidOperationException">ステートマシンは、まだ起動していません</exception>
        /// <exception cref="InvalidOperationException">ステートが Exit 処理中のためイベントを受け付けることが出来ません</exception>
        public virtual bool SendEvent(int eventId)
        {
            // そもそもまだ現在実行中のステートが存在していないなら例外を投げる
            IfNotRunningThrowException();


            // もし Exit 処理中なら
            if (updateState == UpdateState.Exit)
            {
                // Exit 中の SendEvent は許されない
                throw new InvalidOperationException("ステートが Exit 処理中のためイベントを受け付けることが出来ません");
            }


            // 既に遷移準備をしていて かつ 再遷移が許可されていないなら
            if (nextState != null && !AllowRetransition)
            {
                // イベントの受付が出来なかったことを返す
                return false;
            }


            // 現在のステートにイベントガードを呼び出して、ガードされたら
            if (currentState.GuardEvent(eventId))
            {
                // ガードされて失敗したことを返す
                return false;
            }


            // 次に遷移するステートを現在のステートから取り出すが見つけられなかったら
            if (!currentState.transitionTable.TryGetValue(eventId, out nextState))
            {
                // 任意ステートからすらも遷移が出来なかったのなら
                if (!GetOrCreateState<AnyState>().transitionTable.TryGetValue(eventId, out nextState))
                {
                    // イベントの受付が出来なかった
                    return false;
                }
            }


            // 最後に受け付けたイベントIDを覚えてイベントの受付をした事を返す
            LastAcceptedEventID = eventId;
            return true;
        }


        /// <summary>
        /// ステートマシンの状態を更新します。
        /// </summary>
        /// <remarks>
        /// ステートマシンの現在処理しているステートの更新を行いますが、まだ未起動の場合は SetStartState 関数によって設定されたステートが起動します。
        /// また、ステートマシンが初回起動時の場合、ステートのUpdateは呼び出されず、次の更新処理が実行される時になります。
        /// </remarks>
        /// <exception cref="InvalidOperationException">現在のステートマシンは、別のスレッドによって更新処理を実行しています。[UpdaterThread={LastUpdateThreadId}, CurrentThread={currentThreadId}]</exception>
        /// <exception cref="InvalidOperationException">現在のステートマシンは、既に更新処理を実行しています</exception>
        /// <exception cref="InvalidOperationException">開始ステートが設定されていないため、ステートマシンの起動が出来ません</exception>
        public virtual void Update()
        {
            // もしステートマシンの更新状態がアイドリング以外だったら
            if (updateState != UpdateState.Idle)
            {
                // もし別スレッドからのUpdateによる多重Updateなら
                int currentThreadId = Thread.CurrentThread.ManagedThreadId;
                if (LastUpdateThreadId != currentThreadId)
                {
                    // 別スレッドからの多重Updateであることを例外で吐く
                    throw new InvalidOperationException($"現在のステートマシンは、別のスレッドによって更新処理を実行しています。[UpdaterThread={LastUpdateThreadId}, CurrentThread={currentThreadId}]");
                }


                // 多重でUpdateが呼び出せない例外を吐く
                throw new InvalidOperationException("現在のステートマシンは、既に更新処理を実行しています");
            }


            // Updateの起動スレッドIDを覚える
            LastUpdateThreadId = Thread.CurrentThread.ManagedThreadId;


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


                try
                {
                    // Enter処理中であることを設定してEnterを呼ぶ
                    updateState = UpdateState.Enter;
                    currentState.Enter();
                }
                catch (Exception exception)
                {
                    // 起動時の復帰は現在のステートにnullが入っていないとまずいので遷移前の状態に戻す
                    nextState = currentState;
                    currentState = null;


                    // 更新状態をアイドリングにして、例外発生時のエラーハンドリングを行い終了する
                    updateState = UpdateState.Idle;
                    DoHandleException(exception);
                    return;
                }


                // 次に遷移するステートが無いなら
                if (nextState == null)
                {
                    // 起動処理は終わったので一旦終わる
                    updateState = UpdateState.Idle;
                    return;
                }
            }


            try
            {
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
            catch (Exception exception)
            {
                // 更新状態をアイドリングにして、例外発生時のエラーハンドリングを行い終了する
                updateState = UpdateState.Idle;
                DoHandleException(exception);
                return;
            }
        }
        #endregion


        #region 内部ロジック
        /// <summary>
        /// 発生した未処理の例外をハンドリングします
        /// </summary>
        /// <param name="exception">発生した未処理の例外</param>
        /// <exception cref="ArgumentNullException">exception が null です</exception>
        private void DoHandleException(Exception exception)
        {
            // nullを渡されたら
            if (exception == null)
            {
                // 何をハンドリングすればよいのか
                throw new ArgumentNullException(nameof(exception));
            }


            // もし、例外を拾うモード かつ ハンドラが設定されているなら
            if (UnhandledExceptionMode == ImtStateMachineUnhandledExceptionMode.CatchException && UnhandledException != null)
            {
                // イベントを呼び出して、正しくハンドリングされたのなら
                if (UnhandledException(exception))
                {
                    // そのまま終了
                    return;
                }
            }


            // もし、例外を拾ってステートに任せるモード かつ 現在の実行ステートが設定されているのなら
            if (UnhandledExceptionMode == ImtStateMachineUnhandledExceptionMode.CatchStateException && currentState != null)
            {
                // ステートに例外を投げて、正しくハンドリングされたのなら
                if (currentState.Error(exception))
                {
                    // そのまま終了
                    return;
                }
            }


            // 上記のモード以外（つまり ThrowException）か、例外がハンドリングされなかった（false を返された）のなら例外をキャプチャして発生させる
            ExceptionDispatchInfo.Capture(exception).Throw();
        }


        /// <summary>
        /// ステートマシンが未起動の場合に例外を送出します
        /// </summary>
        /// <exception cref="InvalidOperationException">ステートマシンは、まだ起動していません</exception>
        protected void IfNotRunningThrowException()
        {
            // そもそもまだ現在実行中のステートが存在していないなら
            if (!Running)
            {
                // まだ起動すらしていないので例外を吐く
                throw new InvalidOperationException("ステートマシンは、まだ起動していません");
            }
        }


        /// <summary>
        /// 指定されたステートの型のインスタンスを取得しますが、存在しない場合は生成してから取得します。
        /// 生成されたインスタンスは、次回から取得されるようになります。
        /// </summary>
        /// <typeparam name="TState">取得、または生成するステートの型</typeparam>
        /// <returns>取得、または生成されたステートのインスタンスを返します</returns>
        private TState GetOrCreateState<TState>() where TState : State
        {
            // 型の引数を取る関数を叩いて返す
            return (TState)GetOrCreateState(typeof(TState));
        }


        /// <summary>
        /// 指定されたステートの型のインスタンスを取得しますが、存在しない場合は生成してから取得します。
        /// 生成されたインスタンスは、次回から取得されるようになります。
        /// </summary>
        /// <param name="stateType">取得、または生成するステートの型</param>
        /// <returns>取得、または生成されたステートのインスタンスを返します</returns>
        /// <exception cref="ArgumentNullException">stateType が null です</exception>
        /// <exception cref="ArgumentException">'{stateType}'は、ステートの型ではありません</exception>
        private State GetOrCreateState(Type stateType)
        {
            // ステート型かどうかの値を取得してステート型ではないなら
            if (!typeof(State).IsAssignableFrom(stateType ?? throw new ArgumentNullException(nameof(stateType))))
            {
                // ステート型ではないことを例外で吐く
                throw new ArgumentException($"'{stateType}'は、ステートの型ではありません");
            }


            // ステートリストを回る
            foreach (var state in stateList)
            {
                // ステートと同じ型があれば返す
                if (state.GetType() == stateType) return state;
            }


            // 新しいステートを生成する
            var newState = (State)Activator.CreateInstance(stateType);
            newState.stateMachine = this;
            newState.transitionTable = new Dictionary<int, State>();
            stateList.Add(newState);
            return newState;
        }
        #endregion



        #region 内部型定義
        /// <summary>
        /// ステートマシンが処理する状態クラスです
        /// </summary>
        public class State
        {
            // メンバ変数定義
            internal Dictionary<int, State> transitionTable;
            internal ImtTaskStateMachine<TContext> stateMachine;



            /// <summary>
            /// この状態を持っているステートマシン
            /// </summary>
            public ImtTaskStateMachine<TContext> StateMachine => stateMachine;



            /// <summary>
            /// 状態の処理を非同期で実行します
            /// </summary>
            /// <param name="token"></param>
            /// <returns></returns>
            internal protected virtual Task DoProcessAsync(CancellationToken token)
            {
                return Task.CompletedTask;
            }
        }
        #endregion
    }
}