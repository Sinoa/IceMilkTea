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

using System.Collections.Generic;

namespace IceMilkTea.Core
{
    /// <summary>
    /// コンテキストを持つことのできるステートマシンクラスです
    /// </summary>
    /// <typeparam name="ContextT">このステートマシンが持つコンテキストのクラス型</typeparam>
    public class ImtStateMachine<ContextT> where ContextT : class
    {
        /// <summary>
        /// ステートマシンが処理する状態を表現するステートクラスです。
        /// </summary>
        public abstract class State
        {
            // メンバ変数定義
            internal Dictionary<int, State> stateTable;
            internal ImtStateMachine<ContextT> stateMachine;



            /// <summary>
            /// このステートが所属するステートマシン
            /// </summary>
            protected ImtStateMachine<ContextT> StateMachine => stateMachine;


            /// <summary>
            /// このステートが所属するステートマシンが持っているコンテキスト
            /// </summary>
            protected ContextT OwnerContext => stateMachine.context;



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
            /// ステートマシンが遷移をするとき、このステートがその遷移をガードします。
            /// </summary>
            /// <param name="eventId">遷移する理由になったイベントID</param>
            /// <param name="eventArg">イベントIDに付随する引数オブジェクト</param>
            /// <returns>遷移をガードする場合は true を、ガードせず遷移を許す場合は false を返します</returns>
            protected internal virtual bool GuardTransition(int eventId, object eventArg)
            {
                // 通常はガードしない
                return false;
            }
        }



        /// <summary>
        /// ステートマシンで "任意" を表現する特別なステートクラスです
        /// </summary>
        private sealed class AnyState : State { }



        // メンバ変数定義
        private ContextT context;
        private List<State> stateList;
        private State currentState;
        private State nextState;
        private State prevState;
        private int lastEventId;
        private object lastEventArg;




        public ImtStateMachine(ContextT context)
        {
            this.context = context;
        }


        public void Update()
        {
            var state = new AnyState();
            state.stateMachine = this;
        }
    }
}