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
using IceMilkTea.Core;

namespace IceMilkTea.Service
{
    /// <summary>
    /// ゲームのシーンとして制御する基本クラスです
    /// </summary>
    public abstract class GameScene
    {
        #region シーンイベントハンドラ
        /// <summary>
        /// シーンの初期化を行います
        /// </summary>
        protected internal virtual void Initialize()
        {
        }


        /// <summary>
        /// シーンの停止処理を行います
        /// </summary>
        protected internal virtual void Terminate()
        {
        }


        /// <summary>
        /// シーンの更新処理を行います
        /// </summary>
        protected internal virtual void Update()
        {
        }


        /// <summary>
        /// アプリケーションの要因でシーンの一時停止処理を行います
        /// </summary>
        protected internal virtual void OnApplicationSleep()
        {
        }


        /// <summary>
        /// アプリケーションの要因でシーンの再開処理を行います
        /// </summary>
        protected internal virtual void OnApplicationResume()
        {
        }
        #endregion
    }



    /// <summary>
    /// ゲーム進行を行うサービスクラスです
    /// </summary>
    public class GameFacilitatorService : GameService
    {
        // メンバ変数定義
        private Stack<GameScene> sceneStack;
        private Queue<GameScene> initializeQueue;
        private Queue<GameScene> destroyQueue;



        /// <summary>
        /// 現在積まれているシーンの数
        /// </summary>
        public int SceneStackCount => sceneStack.Count;



        #region コンストラクタと起動停止処理部
        /// <summary>
        /// GameFacilitatorService のインスタンスを初期化します
        /// </summary>
        public GameFacilitatorService()
        {
            // シーンスタックと初期化待ちキュー、解放待ちキューの生成
            sceneStack = new Stack<GameScene>();
            initializeQueue = new Queue<GameScene>();
            destroyQueue = new Queue<GameScene>();
        }


        /// <summary>
        /// サービスの起動処理を行います
        /// </summary>
        /// <param name="info">起動情報を設定します</param>
        protected internal override void Startup(out GameServiceStartupInfo info)
        {
            // 起動情報の設定をする
            info = new GameServiceStartupInfo()
            {
                // サービスの更新関数テーブルを構築する
                UpdateFunctionTable = new Dictionary<GameServiceUpdateTiming, Action>()
                {
                    { GameServiceUpdateTiming.MainLoopHead, UpdateService },
                    { GameServiceUpdateTiming.MainLoopTail, FinalFrameUpdate },
                    { GameServiceUpdateTiming.OnApplicationSuspend, OnApplicationSuspendAndFocusOut },
                    { GameServiceUpdateTiming.OnApplicationFocusOut, OnApplicationSuspendAndFocusOut },
                    { GameServiceUpdateTiming.OnApplicationResume, OnApplicationResumeAndFocusIn },
                    { GameServiceUpdateTiming.OnApplicationFocusIn, OnApplicationResumeAndFocusIn },
                }
            };
        }


        /// <summary>
        /// サービスの停止処理を行います
        /// </summary>
        protected internal override void Shutdown()
        {
        }
        #endregion


        #region サービスの更新
        /// <summary>
        /// サービスの更新を行います
        /// </summary>
        private void UpdateService()
        {
        }


        /// <summary>
        /// フレーム最後のサービス更新を行います
        /// </summary>
        private void FinalFrameUpdate()
        {
        }


        /// <summary>
        /// アプリケーションが一時停止またはフォーカスを失った時の処理を行います
        /// </summary>
        private void OnApplicationSuspendAndFocusOut()
        {
        }


        /// <summary>
        /// アプリケーションが再開またはフォーカスを取得した時の処理を行います
        /// </summary>
        private void OnApplicationResumeAndFocusIn()
        {
        }
        #endregion


        #region シーンリスト操作系
        /// <summary>
        /// 指定された新しいシーンを、シーンスタックに積んで次のフレームから動作するようにします。
        /// </summary>
        /// <remarks>
        /// 同一フレーム内で複数のシーンをスタックに積むことは可能ですが、その場合は次のフレームで
        /// スタックに積んだ順から初期化処理が呼び出され Update が呼び出されるのは最後に積んだシーンとなります。
        /// </remarks>
        /// <param name="newScene">スタックに積む新しいシーン</param>
        /// <exception cref="ArgumentNullException">newScene が null です</exception>
        public void PushScene(GameScene newScene)
        {
        }


        /// <summary>
        /// シーンスタックから処理中のシーンを下ろします。
        /// </summary>
        /// <remarks>
        /// シーンスタックから下ろされた後、下ろされたシーンの Terminate が呼び出されるタイミングは
        /// 同一フレームの最後のタイミングになります。
        /// </remarks>
        /// <exception cref="InvalidOperationException">シーンスタックは空です</exception>
        public void PopScene()
        {
        }


        /// <summary>
        /// 最後に積まれたシーンを下ろして、直ちに指定された新しいシーンを積みます。
        /// まるで Pop してから Push したかのような動作をします。
        /// </summary>
        /// <remarks>
        /// この関数は、実際に Pop してから Push を行いますが、通常の Pop - Push と異なる点は
        /// シーンスタックが空の状態でも、例外は送出されないという点になります。
        /// </remarks>
        /// <param name="newScene">新しく切り替えるシーン</param>
        /// <exception cref="ArgumentNullException">newScene が null です</exception>
        public void ChangeScene(GameScene newScene)
        {
        }


        /// <summary>
        /// シーンスタックを空にします。
        /// </summary>
        public void ClearSceneStack()
        {
        }


        /// <summary>
        /// シーンスタックの最上位に最も近い、指定されたシーン型のインスタンスを検索します
        /// </summary>
        /// <typeparam name="TScene">検索するシーン型</typeparam>
        /// <returns>シーンスタックの最上位に最も近い、シーンインスタンスが見つかった場合は、そのインスタンスを返します。しかし、見つけられなかった場合は null を返します。</returns>
        public GameScene FindTopMostScene<TScene>() where TScene : GameScene
        {
            return null;
        }


        /// <summary>
        /// シーンスタックの最下位に最も近い、指定されたシーン型のインスタンスを検索します
        /// </summary>
        /// <typeparam name="TScene">検索するシーン型</typeparam>
        /// <returns>シーンスタックの最下位に最も近い、シーンインスタンスが見つかった場合は、そのインスタンスを返します。しかし、見つけられなかった場合は null を返します。</returns>
        public GameScene FindBottomMostScene<TScene>() where TScene : GameScene
        {
            return null;
        }


        /// <summary>
        /// シーンスタックに積まれた内の、指定されたシーン型のインスタンスをすべて検索します
        /// </summary>
        /// <typeparam name="TScene">検索するシーン型</typeparam>
        /// <returns>シーンインスタンスを見つけた場合は、見つけたインスタンスの配列を返します。しかし、見つけられなかった場合は、長さ 0 の配列を返します。</returns>
        public GameScene[] FindAllScene<TScene>() where TScene : GameScene
        {
            return new GameScene[0];
        }


        /// <summary>
        /// シーンスタックに積まれた内の、指定されたシーン型のインスタンスをすべて検索します
        /// </summary>
        /// <remarks>
        /// FindAllScene() と この関数の違いは、検索結果を格納するバッファを呼び出し元から受け取る点にあります。
        /// 非常に、高い頻度でシーン全体検索を行う場合において FindAllScene() 関数は内部で結果バッファを生成することにより
        /// ヒープを沢山消費してしまいパフォーマンスに悪影響を与えてしまうため、事前にバッファを用意することで、この問題を回避します。
        /// また、バッファサイズが実際に検索結果の数を下回ったとしても、バッファを超える検索は行わずバッファが満たされた状態で関数は終了します。
        /// </remarks>
        /// <typeparam name="TScene">検索するシーン型</typeparam>
        /// <param name="result">検索結果を格納する GameScene バッファ</param>
        /// <returns>result 引数に与えられたバッファに格納した総数を返します。</returns>
        /// <exception cref="ArgumentNullException">result が null です</exception>
        public int FindAllSceneNonAlloc<TScene>(GameScene[] result) where TScene : GameScene
        {
            return 0;
        }


        /// <summary>
        /// 指定されたシーンがシーンスタックに含まれているか確認をします
        /// </summary>
        /// <param name="scene">確認をするシーン</param>
        /// <returns>該当のシーンが、シーンスタックに含まれている場合は true を、含まれていない場合は false を返します</returns>
        public bool ContainsScene(GameScene scene)
        {
            return false;
        }


        /// <summary>
        /// 指定されたシーンのひとつ下に存在するシーンを取得します
        /// </summary>
        /// <param name="scene">取り出すシーンの一つ上に存在するシーン</param>
        /// <returns>指定されたシーンのひとつ下のシーンを返します</returns>
        /// <exception cref="ArgumentNullException">scene が null です</exception>
        /// <exception cref="InvalidOperationException">指定された scene は、シーンスタックに含まれていません</exception>
        /// <exception cref="InvalidOperationException">指定された scene は、シーンスタックの最下位に存在しています</exception>
        public GameScene GetPreviousScene(GameScene scene)
        {
            return null;
        }
        #endregion
    }
}