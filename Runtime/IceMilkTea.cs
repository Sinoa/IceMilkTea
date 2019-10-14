// zlib/libpng License
//
// Copyright (c) 2018 - 2019 Sinoa
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

#region using
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using UnityObject = UnityEngine.Object;
#endregion

#region AssemblyInfo
[assembly: AssemblyTitle("IceMilkTea")]
[assembly: AssemblyProduct("IceMilkTea")]
[assembly: AssemblyDescription("Unity Game Framework")]
[assembly: AssemblyCompany("Sinoa")]
[assembly: AssemblyTrademark("Sinoa")]
[assembly: AssemblyCopyright("Copyright © 2018 - 2019 Sinoa")]
[assembly: ComVisible(false)]
[assembly: Guid("6B94121C-5255-4DA7-94B6-34FC3C377178")]
[assembly: AssemblyVersion("0.0.2.*")]
[assembly: AssemblyFileVersion("0.0.2.0")]
#if DEBUG
[assembly: InternalsVisibleTo("IceMilkTeaEditor")]
[assembly: InternalsVisibleTo("IceMilkTeaTestDynamic")]
[assembly: InternalsVisibleTo("IceMilkTeaTestStatic")]
#endif
#endregion

namespace IceMilkTea.Core
{
    #region GameMain
    /// <summary>
    /// GameMain クラスのアセット生成ツールメニューの非表示を示す属性クラスです
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class HideCreateGameMainAssetMenuAttribute : Attribute
    {
    }



    /// <summary>
    /// ゲームメインクラスの実装をするための抽象クラスです。
    /// IceMilkTeaによるゲームのスタートアップからメインループを構築する場合は必ず継承し実装をして下さい。
    /// </summary>
    [HideCreateGameMainAssetMenu]
    public abstract class GameMain : ScriptableObject
    {
        #region PlayerLoopSystem用型定義
        /// <summary>
        /// ゲームサービスマネージャのサービス起動ルーチンを実行する型です
        /// </summary>
        public struct GameServiceManagerStartup { }


        /// <summary>
        /// ゲームサービスマネージャのサービス終了ルーチンを実行する型です
        /// </summary>
        public struct GameServiceManagerCleanup { }
        #endregion



        #region プロパティ
        /// <summary>
        /// 現在のゲームメインコンテキストを取得します
        /// </summary>
        public static GameMain Current { get; private set; }


        /// <summary>
        /// 現在のゲームメインが保持しているサービスマネージャを取得します
        /// </summary>
        public GameServiceManager ServiceManager { get; private set; }
        #endregion



        #region エントリポイントとロジック関数
        /// <summary>
        /// Unity起動時に実行されるゲームのエントリポイントです
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Main()
        {
            // ゲームメインをロードする
            Current = LoadGameMain();


            // IceMilkTeaはこのまま起動を継続してはいけないのなら
            if (!Current.Continue())
            {
                // ロードしたばかりのGameMainを解放して起動を中止する
                Current = null;
                Resources.UnloadUnusedAssets();
                return;
            }


            // サービスマネージャのインスタンスを生成するが、nullが返却されるようなことがあれば
            Current.ServiceManager = Current.CreateGameServiceManager();
            if (Current.ServiceManager == null)
            {
                // ゲームシステムは破壊的な死亡をした
                throw new InvalidOperationException("GameServiceManager の正しいインスタンスが生成されませんでした。");
            }


            // ハンドラの登録をする
            RegisterHandler();


            // サービスマネージャを起動する
            Current.ServiceManager.Startup();


            // ゲームの起動を開始する
            Current.Startup();
        }


        /// <summary>
        /// 指定されたゲームメインによって動作を上書きします。
        /// この関数は、テストの為に用意された関数であり、通常のゲームロジック上で使用される想定はありません。
        /// 動作の切り替えや、想定のゲームメイン動作を設定する場合は GameMain.RedirectGameMain 関数をオーバーライドして下さい。
        /// </summary>
        /// <param name="gameMain">上書きするゲームメインの参照</param>
        internal static void OverrideGameMain(GameMain gameMain)
        {
            // 起動中のゲームメインがあるのなら
            if (Current != null)
            {
                // 何があろうとシャットダウンして、ImtPlayerLoopSystemから既定Unityループシステムを読み込んで上書きすることですべての登録を破壊できる
                InternalShutdown();
                ImtPlayerLoopSystem.GetUnityDefaultPlayerLoop().BuildAndSetUnityPlayerLoop();
            }


            // 渡されたゲームメインを設定して初期化を実行する
            Current = gameMain;
            Current.ServiceManager = Current.CreateGameServiceManager();
            RegisterHandler();
            Current.ServiceManager.Startup();
            Current.Startup();
        }


        /// <summary>
        /// Unityのアプリケーション終了時に処理するべき後処理を行います
        /// </summary>
        private static void InternalShutdown()
        {
            // ハンドラの解除をする
            UnregisterHandler();


            // サービスマネージャを停止する
            Current.ServiceManager.Shutdown();


            // ゲームのシャットダウンをする
            Current.Shutdown();
        }


        /// <summary>
        /// ゲームメインをロードします
        /// </summary>
        /// <returns>ロードされたゲームメインを返します</returns>
        private static GameMain LoadGameMain()
        {
            // 内部で保存されたGameMainのGameMainをロードする
            var gameMain = Resources.Load<GameMain>("GameMain");


            // ロードが出来なかったのなら
            if (gameMain == null)
            {
                // セーフ起動用のゲームメインで立ち上げる
                return CreateInstance<SafeGameMain>();
            }


            // リダイレクトするGameMainがあるか聞いて、存在するなら
            var redirectGameMain = gameMain.RedirectGameMain();
            if (redirectGameMain != null)
            {
                // リダイレクトされたGameMainを設定して、ロードされたGameMainを解放
                gameMain = redirectGameMain;
                Resources.UnloadUnusedAssets();
            }


            // ロードしたゲームメインを返す
            return gameMain;
        }


        /// <summary>
        /// GameMainの動作に必要なハンドラの登録処理を行います
        /// </summary>
        private static void RegisterHandler()
        {
            // アプリケーションの終了イベントを引っ掛けておく
            Application.quitting += InternalShutdown;


            // サービスマネージャの開始と終了のループシステムを生成
            var startupGameServiceLoopSystem = new ImtPlayerLoopSystem(typeof(GameServiceManagerStartup), Current.ServiceManager.StartupServices);
            var cleanupGameServiceLoopSystem = new ImtPlayerLoopSystem(typeof(GameServiceManagerCleanup), Current.ServiceManager.CleanupServices);


            // ゲームループの開始と終了のタイミングあたりにサービスマネージャのスタートアップとクリーンアップの処理を引っ掛ける
            var loopSystem = ImtPlayerLoopSystem.GetLastBuildLoopSystem();
            loopSystem.InsertLoopSystem<Initialization.PlayerUpdateTime>(InsertTiming.AfterInsert, startupGameServiceLoopSystem);
            loopSystem.InsertLoopSystem<PostLateUpdate.ExecuteGameCenterCallbacks>(InsertTiming.AfterInsert, cleanupGameServiceLoopSystem);
            loopSystem.BuildAndSetUnityPlayerLoop();
        }


        /// <summary>
        /// GameMainの動作に必要なハンドラの解除処理を行います
        /// </summary>
        private static void UnregisterHandler()
        {
            // アプリケーション終了イベントを外す
            // （PlayerLoopSystemはPlayerLoopSystem自身が登録解除まで担保してくれているのでそのまま）
            Application.quitting -= InternalShutdown;
        }
        #endregion


        #region オーバーライド可能なGameMainのハンドラ関数
        /// <summary>
        /// IceMilkTeaのシステムがこのまま継続して起動するかどうかを判断します
        /// </summary>
        /// <returns>起動を継続する場合は true を、継続しない場合は false を返します</returns>
        protected virtual bool Continue()
        {
            // 通常は起動を継続する
            return true;
        }


        /// <summary>
        /// ゲームの起動処理を行います。
        /// 主に、ゲームサービスの初期登録や必要な追加モジュールの初期化などを行います。
        /// </summary>
        protected virtual void Startup()
        {
        }


        /// <summary>
        /// ゲームの終了処理を行います。
        /// ゲームサービスそのものの終了処理は、サービス側で処理されるべきで、
        /// この関数では主に、追加モジュールなどの解放やサービス管轄外の解放などを行うべきです。
        /// </summary>
        protected virtual void Shutdown()
        {
        }


        /// <summary>
        /// 起動するGameMainをリダイレクトします。
        /// IceMilkTeaによって起動されたGameMainから他のGameMainへリダイレクトする場合は、
        /// この関数をオーバーライドして起動するGameMainのインスタンスを返します。
        /// </summary>
        /// <returns>リダイレクトするGameMainがある場合はインスタンスを返しますが、ない場合はnullを返します</returns>
        protected virtual GameMain RedirectGameMain()
        {
            // リダイレクト先GameMainはなし
            return null;
        }


        /// <summary>
        /// ゲームサービスを管理する、サービスマネージャを生成します。
        /// ゲームサービスの管理をカスタマイズする場合は、
        /// この関数をオーバーライドしてGameServiceManagerを継承したクラスのインスタンスを返します。
        /// </summary>
        /// <returns>GameServiceManager のインスタンスを返します</returns>
        protected virtual GameServiceManager CreateGameServiceManager()
        {
            // 通常は、素のゲームサービスマネージャを生成して返す
            return new GameServiceManager();
        }
        #endregion



        #region SafeGameMain実装
        /// <summary>
        /// 起動するべきGameMainが見つからなかった場合や、起動できない場合において
        /// 代わりに起動するための GameMain クラスです。
        /// </summary>
        [HideCreateGameMainAssetMenu]
        private class SafeGameMain : GameMain
        {
            /// <summary>
            /// セーフ起動時のIceMilkTeaは、起動を継続しないようにします。
            /// </summary>
            /// <returns>この関数は常にfalseを返します</returns>
            protected override bool Continue()
            {
                // 起動を止めるようにする
                return false;
            }
        }
        #endregion
    }
    #endregion



    #region SynchronizationContext
    /// <summary>
    /// IceMilkTea 自身が提供する同期コンテキストクラスです。
    /// 独立したスレッドの同期コンテキストとして利用したり、特定コード範囲の同期コンテキストとして利用出来ます。
    /// </summary>
    public class ImtSynchronizationContext : SynchronizationContext, IDisposable
    {
        /// <summary>
        /// 同期コンテキストに送られてきたコールバックを、メッセージとして保持する構造体です。
        /// </summary>
        private struct Message
        {
            // メンバ変数定義
            private SendOrPostCallback callback;
            private ManualResetEvent waitHandle;
            private object state;



            /// <summary>
            /// Message のインスタンスを初期化します。
            /// </summary>
            /// <param name="callback">呼び出すべきコールバック関数</param>
            /// <param name="state">コールバックに渡すオブジェクト</param>
            /// <param name="waitHandle">コールバックの呼び出しを待機するために、利用する待機ハンドル</param>
            public Message(SendOrPostCallback callback, object state, ManualResetEvent waitHandle)
            {
                // メンバの初期化
                this.callback = callback;
                this.waitHandle = waitHandle;
                this.state = state;
            }


            /// <summary>
            /// メッセージに設定されたコールバックを呼び出します。
            /// また、待機ハンドルが設定されている場合は、待機ハンドルのシグナルを設定します。
            /// </summary>
            public void Invoke()
            {
                try
                {
                    // コールバックを叩く
                    callback(state);
                }
                finally
                {
                    // もし待機ハンドルがあるなら
                    if (waitHandle != null)
                    {
                        // シグナルを設定する
                        waitHandle.Set();
                    }
                }
            }


            /// <summary>
            /// このメッセージを管理していた同期コンテキストが、何かの理由で管理できなくなった場合
            /// このメッセージを指定された同期コンテキストに、再ポストします。
            /// また、送信メッセージの場合は、直ちに処理され待機ハンドルのシグナルが設定されます。
            /// </summary>
            /// <param name="rePostTargetContext">再ポスト先の同期コンテキスト</param>
            public void Failover(SynchronizationContext rePostTargetContext)
            {
                // 待機ハンドルが存在するなら
                if (waitHandle != null)
                {
                    // コールバックを叩いてシグナルを設定する
                    callback(state);
                    waitHandle.Set();
                    return;
                }


                // 再ポスト先同期コンテキストにポストする
                rePostTargetContext.Post(callback, state);
            }
        }



        // 定数定義
        public const int DefaultMessageQueueCapacity = 32;

        // メンバ変数定義
        private SynchronizationContext previousContext;
        private Queue<Message> messageQueue;
        private List<Exception> errorList;
        private int myStartupThreadId;
        private bool disposed;



        /// <summary>
        /// ImtSynchronizationContext のインスタンスを初期化します。
        /// </summary>
        /// <remarks>
        /// この同期コンテキストは messagePumpHandler が呼び出されない限りメッセージを蓄え続けます。
        /// メッセージを処理するためには、必ず messagePumpHandler を定期的に呼び出してください。
        /// </remarks>
        /// <param name="messagePumpHandler">この同期コンテキストに送られてきたメッセージを処理するための、メッセージポンプハンドラを受け取ります</param>
        public ImtSynchronizationContext(out Action messagePumpHandler)
        {
            // メンバの初期化と、メッセージ処理関数を伝える
            previousContext = AsyncOperationManager.SynchronizationContext;
            messageQueue = new Queue<Message>(DefaultMessageQueueCapacity);
            errorList = new List<Exception>(DefaultMessageQueueCapacity);
            myStartupThreadId = Thread.CurrentThread.ManagedThreadId;
            messagePumpHandler = DoProcessMessage;
        }


        /// <summary>
        /// ImtSynchronizationContext のファイナライザです。
        /// </summary>
        ~ImtSynchronizationContext()
        {
            // ファイナライザからのDispose呼び出し
            Dispose(false);
        }


        /// <summary>
        /// リソースを解放します。また、解放する際にメッセージが残っていた場合は
        /// この同期コンテキストが生成される前に存在していた、同期コンテキストに再ポストされ、同期コンテキストが再設定されます。
        /// </summary>
        public void Dispose()
        {
            // DisposeからのDispose呼び出し
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        /// <summary>
        /// 実際のリソース解放を行います。
        /// </summary>
        /// <param name="disposing">マネージ解放の場合は true を、アンマネージ解放なら false を指定</param>
        protected virtual void Dispose(bool disposing)
        {
            // 既に解放済みなら
            if (disposed)
            {
                // 終了
                return;
            }


            // もし現在の同期コンテキストが自身なら
            if (AsyncOperationManager.SynchronizationContext == this)
            {
                // 同期コンテキストを、インスタンス生成時に覚えたコンテキストに戻す
                AsyncOperationManager.SynchronizationContext = previousContext;
            }


            // メッセージキューをロック
            lock (messageQueue)
            {
                // 全てのメッセージを処理するまでループ
                while (messageQueue.Count > 0)
                {
                    // 一つ前の同期コンテキストにフェイルオーバーする
                    messageQueue.Dequeue().Failover(previousContext);
                }
            }


            // 解放済みマーク
            disposed = true;
        }


        /// <summary>
        /// ImtSynchronizationContext のインスタンスを生成と同時に、同期コンテキストの設定も行います。
        /// </summary>
        /// <param name="messagePumpHandler">コンストラクタの messagePumpHandler に渡す参照</param>
        /// <returns>インスタンスの生成と設定が終わった、同期コンテキストを返します。</returns>
        public static ImtSynchronizationContext Install(out Action messagePumpHandler)
        {
            // 新しい同期コンテキストのインスタンスを生成して、設定した後に返す
            var context = new ImtSynchronizationContext(out messagePumpHandler);
            AsyncOperationManager.SynchronizationContext = context;
            return context;
        }


        /// <summary>
        /// 同期メッセージを送信します。
        /// </summary>
        /// <param name="callback">呼び出すべきメッセージのコールバック</param>
        /// <param name="state">コールバックに渡してほしいオブジェクト</param>
        /// <exception cref="ObjectDisposedException">既にオブジェクトが解放済みです</exception>
        public override void Send(SendOrPostCallback callback, object state)
        {
            // 解放済み例外送出関数を叩く
            ThrowIfDisposed();


            // 同じスレッドからの送信なら
            if (Thread.CurrentThread.ManagedThreadId == myStartupThreadId)
            {
                // 直ちにコールバックを叩いて終了
                callback(state);
                return;
            }


            // メッセージ処理待ち用同期プリミティブを用意
            using (var waitHandle = new ManualResetEvent(false))
            {
                // メッセージキューをロック
                lock (messageQueue)
                {
                    // 処理して欲しいコールバックを登録
                    messageQueue.Enqueue(new Message(callback, state, waitHandle));
                }


                // 登録したコールバックが処理されるまで待機
                waitHandle.WaitOne();
            }
        }


        /// <summary>
        /// 非同期メッセージをポストします。
        /// </summary>
        /// <param name="callback">呼び出すべきメッセージのコールバック</param>
        /// <param name="state">コールバックに渡してほしいオブジェクト</param>
        /// <exception cref="ObjectDisposedException">既にオブジェクトが解放済みです</exception>
        public override void Post(SendOrPostCallback callback, object state)
        {
            // 解放済み例外送出関数を叩く
            ThrowIfDisposed();


            // メッセージキューをロック
            lock (messageQueue)
            {
                // 処理して欲しいコールバックを登録
                messageQueue.Enqueue(new Message(callback, state, null));
            }
        }


        /// <summary>
        /// 同期コンテキストに、送られてきたメッセージを処理します。
        /// </summary>
        /// <exception cref="ObjectDisposedException">既にオブジェクトが解放済みです</exception>
        private void DoProcessMessage()
        {
            // 解放済み例外送出関数を叩く
            ThrowIfDisposed();


            // エラーリストをクリアする
            errorList.Clear();


            // メッセージキューをロック
            lock (messageQueue)
            {
                // メッセージ処理中にポストされても次回になるよう、今回処理するべきメッセージ件数の取得
                var processCount = messageQueue.Count;


                // 今回処理するべきメッセージの件数分だけループ
                for (int i = 0; i < processCount; ++i)
                {
                    try
                    {
                        // メッセージを呼ぶ
                        messageQueue.Dequeue().Invoke();
                    }
                    catch (Exception exception)
                    {
                        // エラーが発生したらエラーリストに詰める
                        errorList.Add(exception);
                    }
                }
            }


            // エラーリストに要素が1つでも存在したら
            if (errorList.Count > 0)
            {
                // エラーリストの内容全てを包んでまとめて例外を投げる
                throw new AggregateException($"メッセージ処理中に {errorList.Count} 件のエラーが発生しました", errorList.ToArray());
            }
        }


        /// <summary>
        /// 解放済みの場合に、例外を送出します。
        /// </summary>
        /// <exception cref="ObjectDisposedException">既にオブジェクトが解放済みです</exception>
        private void ThrowIfDisposed()
        {
            // 解放済みなら
            if (disposed)
            {
                // 解放済み例外を投げる
                throw new ObjectDisposedException(null);
            }
        }
    }
    #endregion



    #region PlayerLoop
    /// <summary>
    /// ループシステムの挿入をする時、対象の型に対して挿入するタイミングを指示します
    /// </summary>
    public enum InsertTiming
    {
        /// <summary>
        /// 対象の前に挿入を指示します
        /// </summary>
        BeforeInsert,

        /// <summary>
        /// 対象の後に挿入を指示します
        /// </summary>
        AfterInsert,
    }



    /// <summary>
    /// ループシステムに挿入するユーザーカスタムの更新基本クラスです
    /// </summary>
    public abstract class GameUpdater
    {
        /// <summary>
        /// ループシステムによって実行される更新関数です
        /// </summary>
        protected internal abstract void Update();
    }



    /// <summary>
    /// PlayerLoopSystem構造体の内容をクラスとして表現され、更に調整するための機構を保持したクラスです
    /// </summary>
    public class ImtPlayerLoopSystem
    {
        /// <summary>
        /// ループシステムの検索で、対象のループシステムを見つけられなかったときに返す値です
        /// </summary>
        public const int LoopSystemNotFoundValue = -1;



        // クラス変数宣言
        private static ImtPlayerLoopSystem lastBuildLoopSystem;

        // メンバ変数定義
        private Type type;
        private List<ImtPlayerLoopSystem> subLoopSystemList;
        private PlayerLoopSystem.UpdateFunction updateDelegate;
        private IntPtr updateFunction;
        private IntPtr loopConditionFunction;



        #region コンストラクタ
        /// <summary>
        /// クラスの初期化を行います
        /// </summary>
        static ImtPlayerLoopSystem()
        {
            // アプリケーション終了イベントを登録する
            Application.quitting += OnApplicationQuit;
        }


        /// <summary>
        /// 指定されたPlayerLoopSystem構造体オブジェクトから値をコピーしてインスタンスの初期化を行います。
        /// また、指定されたPlayerLoopSystem構造体オブジェクトにサブループシステムが存在する場合は再帰的にインスタンスの初期化が行われます。
        /// </summary>
        /// <param name="originalPlayerLoopSystem">コピー元になるPlayerLoopSystem構造体オブジェクトへの参照</param>
        public ImtPlayerLoopSystem(ref PlayerLoopSystem originalPlayerLoopSystem)
        {
            // 参照元から値を引っ張って初期化する
            type = originalPlayerLoopSystem.type;
            updateDelegate = originalPlayerLoopSystem.updateDelegate;
            updateFunction = originalPlayerLoopSystem.updateFunction;
            loopConditionFunction = originalPlayerLoopSystem.loopConditionFunction;


            // もしサブシステムが有効な数で存在するなら
            if (originalPlayerLoopSystem.subSystemList != null && originalPlayerLoopSystem.subSystemList.Length > 0)
            {
                // 再帰的にコピーを生成する
                var enumerable = originalPlayerLoopSystem.subSystemList.Select(original => new ImtPlayerLoopSystem(ref original));
                subLoopSystemList = new List<ImtPlayerLoopSystem>(enumerable);
            }
            else
            {
                // 存在しないならインスタンスの生成だけする
                subLoopSystemList = new List<ImtPlayerLoopSystem>();
            }
        }


        /// <summary>
        /// 指定された型でインスタンスの初期化を行います
        /// </summary>
        /// <param name="type">生成するPlayerLoopSystemの型</param>
        public ImtPlayerLoopSystem(Type type) : this(type, null)
        {
        }


        /// <summary>
        /// 指定された型と更新関数でインスタンスの初期化を行います
        /// </summary>
        /// <param name="type">生成するPlayerLoopSystemの型</param>
        /// <param name="updateDelegate">生成するPlayerLoopSystemの更新関数。更新関数が不要な場合はnullの指定が可能です</param>
        /// <exception cref="ArgumentNullException">typeがnullです</exception>
        public ImtPlayerLoopSystem(Type type, PlayerLoopSystem.UpdateFunction updateDelegate)
        {
            // 更新の型がnullなら
            if (type == null)
            {
                // 関数は死ぬ
                throw new ArgumentNullException(nameof(type));
            }


            // シンプルに初期化をする
            this.type = type;
            this.updateDelegate = updateDelegate;
            subLoopSystemList = new List<ImtPlayerLoopSystem>();
        }
        #endregion


        #region Unityイベントハンドラ
        /// <summary>
        /// Unityがアプリケーションの終了をする時に呼び出されます
        /// </summary>
        private static void OnApplicationQuit()
        {
            //イベントの登録を解除する
            Application.quitting -= OnApplicationQuit;


            // Unityの弄り倒したループ構成をもとに戻してあげる
            PlayerLoop.SetPlayerLoop(PlayerLoop.GetDefaultPlayerLoop());
        }
        #endregion


        #region Unity変換関数群
        /// <summary>
        /// Unityの標準プレイヤーループを ImtPlayerLoopSystem として取得します
        /// </summary>
        /// <returns>Unityの標準プレイヤーループをImtPlayerLoopSystemにキャストされた結果を返します</returns>
        public static ImtPlayerLoopSystem GetUnityDefaultPlayerLoop()
        {
            // キャストして返すだけ
            return (ImtPlayerLoopSystem)PlayerLoop.GetDefaultPlayerLoop();
        }


        /// <summary>
        /// このインスタンスを本来の構造へ構築し、Unityのプレイヤーループへ設定します
        /// </summary>
        public void BuildAndSetUnityPlayerLoop()
        {
            // 最後に構築した経験のあるループシステムとして覚えて、自身をキャストして設定するだけ
            lastBuildLoopSystem = this;
            PlayerLoop.SetPlayerLoop((PlayerLoopSystem)this);
        }
        #endregion


        #region コントロール関数群
        /// <summary>
        /// BuildAndSetUnityDefaultPlayerLoop関数によって最後に構築されたループシステムを取得します。
        /// まだ一度も構築した経験がない場合は、GetUnityDefaultPlayerLoop関数の値を採用します。
        /// </summary>
        /// <returns>最後に構築されたループシステムを返します</returns>
        public static ImtPlayerLoopSystem GetLastBuildLoopSystem()
        {
            // 過去に構築経験があれば返して、まだなければGetUnityDefaultPlayerLoopの結果を返す
            return lastBuildLoopSystem ?? GetUnityDefaultPlayerLoop();
        }


        /// <summary>
        /// 指定されたインデックスの位置に更新関数を挿入します。
        /// また、nullの更新関数を指定すると何もしないループシステムが生成されます。
        /// </summary>
        /// <typeparam name="T">更新関数を表す型</typeparam>
        /// <param name="index">挿入するインデックスの位置</param>
        /// <param name="function">挿入する更新関数</param>
        public void InsertLoopSystem<T>(int index, PlayerLoopSystem.UpdateFunction function)
        {
            // 新しいループシステムを作って本来の挿入関数を叩く
            var loopSystem = new ImtPlayerLoopSystem(typeof(T), function);
            InsertLoopSystem(index, loopSystem);
        }


        /// <summary>
        /// 指定されたインデックスの位置にループシステムを挿入します
        /// </summary>
        /// <param name="index">挿入するインデックスの位置</param>
        /// <param name="loopSystem">挿入するループシステム</param>
        /// <exception cref="ArgumentNullException">loopSystemがnullです</exception>
        public void InsertLoopSystem(int index, ImtPlayerLoopSystem loopSystem)
        {
            // ループシステムがnullなら（境界チェックはあえてここでやらず、List<T>コンテナに任せる）
            if (loopSystem == null)
            {
                // nullの挿入は許されない！
                throw new ArgumentNullException(nameof(loopSystem));
            }


            // 指定されたインデックスにループシステムを挿入する
            subLoopSystemList.Insert(index, loopSystem);
        }


        /// <summary>
        /// 指定された型の更新ループに対して、ループシステムをタイミングの位置に挿入します
        /// </summary>
        /// <typeparam name="T">これから挿入するループシステムの挿入起点となる更新型</typeparam>
        /// <typeparam name="U">挿入する予定の更新関数を表す型</typeparam>
        /// <param name="timing">T で指定された更新ループを起点にどのタイミングで挿入するか</param>
        /// <param name="function">挿入する更新関数</param>
        /// <returns>対象のループシステムが挿入された場合はtrueを、挿入されなかった場合はfalseを返します</returns>
        public bool InsertLoopSystem<T, U>(InsertTiming timing, PlayerLoopSystem.UpdateFunction function)
        {
            // 再帰検索を有効にして挿入関数を叩く
            return InsertLoopSystem<T, U>(timing, function, true);
        }


        /// <summary>
        /// 指定された型の更新ループに対して、ループシステムをタイミングの位置に挿入します
        /// </summary>
        /// <typeparam name="T">これから挿入するループシステムの挿入起点となる更新型</typeparam>
        /// <typeparam name="U">挿入する予定の更新関数を表す型</typeparam>
        /// <param name="timing">T で指定された更新ループを起点にどのタイミングで挿入するか</param>
        /// <param name="function">挿入する更新関数</param>
        /// <param name="recursiveSearch">対象の型の検索を再帰的に行うかどうか</param>
        /// <returns>対象のループシステムが挿入された場合はtrueを、挿入されなかった場合はfalseを返します</returns>
        public bool InsertLoopSystem<T, U>(InsertTiming timing, PlayerLoopSystem.UpdateFunction function, bool recursiveSearch)
        {
            // 新しいループシステムを作って本来の挿入関数を叩く
            var loopSystem = new ImtPlayerLoopSystem(typeof(U), function);
            return InsertLoopSystem<T>(timing, loopSystem, recursiveSearch);
        }


        /// <summary>
        /// 指定された型の更新ループに対して、ループシステムをタイミングの位置に挿入します
        /// </summary>
        /// <typeparam name="T">これから挿入するループシステムの挿入起点となる更新型</typeparam>
        /// <param name="timing">T で指定された更新ループを起点にどのタイミングで挿入するか</param>
        /// <param name="loopSystem">挿入するループシステム</param>
        /// <exception cref="ArgumentNullException">loopSystemがnullです</exception>
        /// <returns>対象のループシステムが挿入された場合はtrueを、挿入されなかった場合はfalseを返します</returns>
        public bool InsertLoopSystem<T>(InsertTiming timing, ImtPlayerLoopSystem loopSystem)
        {
            // 再帰検索を有効にして本来の挿入関数を叩く
            return InsertLoopSystem<T>(timing, loopSystem, true);
        }


        /// <summary>
        /// 指定された型の更新ループに対して、ループシステムをタイミングの位置に挿入します
        /// </summary>
        /// <typeparam name="T">これから挿入するループシステムの挿入起点となる更新型</typeparam>
        /// <param name="timing">T で指定された更新ループを起点にどのタイミングで挿入するか</param>
        /// <param name="loopSystem">挿入するループシステム</param>
        /// <param name="recursiveSearch">対象の型の検索を再帰的に行うかどうか</param>
        /// <exception cref="ArgumentNullException">loopSystemがnullです</exception>
        /// <returns>対象のループシステムが挿入された場合はtrueを、挿入されなかった場合はfalseを返します</returns>
        public bool InsertLoopSystem<T>(InsertTiming timing, ImtPlayerLoopSystem loopSystem, bool recursiveSearch)
        {
            // ループシステムがnullなら
            if (loopSystem == null)
            {
                // nullの挿入は許されない！
                throw new ArgumentNullException(nameof(loopSystem));
            }


            // 挿入するインデックス値を探すが見つけられなかったら
            var insertIndex = IndexOf<T>();
            if (insertIndex == LoopSystemNotFoundValue)
            {
                // もし再帰的に調べるのなら
                if (recursiveSearch)
                {
                    // 自身のサブループシステム分回る
                    foreach (var subLoopSystem in subLoopSystemList)
                    {
                        // サブループシステムに対して挿入を依頼して成功したのなら
                        if (subLoopSystem.InsertLoopSystem<T>(timing, loopSystem, recursiveSearch))
                        {
                            // 成功を返す
                            return true;
                        }
                    }
                }


                // やっぱり駄目だったよ
                return false;
            }


            // 検索結果を見つけたのなら、挿入タイミングによってインデックス値を調整して挿入後、成功を返す
            insertIndex = timing == InsertTiming.BeforeInsert ? insertIndex : insertIndex + 1;
            subLoopSystemList.Insert(insertIndex, loopSystem);
            return true;
        }


        /// <summary>
        /// 指定された型の更新ループをサブループシステムから削除します
        /// </summary>
        /// <typeparam name="T">削除する更新ループの型</typeparam>
        /// <param name="recursiveSearch">対象の型を再帰的に検索し削除するかどうか</param>
        /// <returns>対象のループシステムが削除された場合はtrueを、削除されなかった場合はfalseを返します</returns>
        public bool RemoveLoopSystem<T>(bool recursiveSearch)
        {
            // 削除するインデックス値を探すが見つけられなかったら
            var removeIndex = IndexOf<T>();
            if (removeIndex == LoopSystemNotFoundValue)
            {
                // もし再帰的に調べるのなら
                if (recursiveSearch)
                {
                    // 自身のサブループシステム分回る
                    foreach (var subLoopSystem in subLoopSystemList)
                    {
                        // サブループシステムに対して削除依頼して成功したのなら
                        if (subLoopSystem.RemoveLoopSystem<T>(recursiveSearch))
                        {
                            // 成功を返す
                            return true;
                        }
                    }
                }


                // だめでした
                return false;
            }


            // 対象インデックスの要素を殺す
            subLoopSystemList.RemoveAt(removeIndex);
            return true;
        }


        /// <summary>
        /// 指定された型の更新ループを指定された数だけ移動します。
        /// また、移動量が境界を超えないように内部で調整されます。
        /// </summary>
        /// <typeparam name="T">移動する更新ループの型</typeparam>
        /// <param name="count">移動する量、負の値なら前方へ、正の値なら後方へ移動します</param>
        /// <param name="recursiveSearch">移動する型が見つからない場合、再帰的に検索をするかどうか</param>
        /// <returns>移動に成功した場合はtrueを、移動に失敗した場合はfalseを返します</returns>
        public bool MoveLoopSystem<T>(int count, bool recursiveSearch)
        {
            // 移動する更新ループの位置を特定するが、見つけられなかったら
            var currentIndex = IndexOf<T>();
            if (currentIndex == LoopSystemNotFoundValue)
            {
                // もし再帰的に調べるのなら
                if (recursiveSearch)
                {
                    // 自身のサブループシステム分回る
                    foreach (var childLoopSystem in subLoopSystemList)
                    {
                        // サブループシステムに対して削除を依頼して成功したのなら
                        if (childLoopSystem.MoveLoopSystem<T>(count, recursiveSearch))
                        {
                            // 成功を返す
                            return true;
                        }
                    }
                }


                // だめだったらだめ
                return false;
            }


            // 新しいインデックス値を求める
            // 更にインデックス値が後方へ移動する場合は削除分ズレるので-1する
            var newIndex = currentIndex + count + (count > 0 ? -1 : 0);
            if (newIndex < 0) newIndex = 0;
            if (newIndex > subLoopSystemList.Count) newIndex = subLoopSystemList.Count;


            // 古いインデックスから値を取り出して削除した後新しいインデックスに挿入
            var subLoopSystem = subLoopSystemList[currentIndex];
            subLoopSystemList.RemoveAt(currentIndex);
            subLoopSystemList.Insert(newIndex, subLoopSystem);
            return true;
        }


        /// <summary>
        /// 指定された更新型でループシステムを探し出します。
        /// </summary>
        /// <typeparam name="T">検索するループシステムの型</typeparam>
        /// <param name="recursiveSearch">対象の型の検索を再帰的に行うかどうか</param>
        /// <returns>最初に見つけたループシステムを返しますが、見つけられなかった場合はnullを返します</returns>
        public ImtPlayerLoopSystem FindLoopSystem<T>(bool recursiveSearch)
        {
            // 自身のサブループシステムに該当の型があるか調べて、見つけたら
            var result = subLoopSystemList.Find(loopSystem => loopSystem.type == typeof(T));
            if (result != null)
            {
                // 結果を返す
                return result;
            }


            // 見つけられなく、かつ再帰検索でないのなら
            if (result == null && !recursiveSearch)
            {
                // 諦めてnullを返す
                return null;
            }


            // 自分のサブループシステムにも検索を問いかける
            return subLoopSystemList.Find(loopSystem => loopSystem.FindLoopSystem<T>(recursiveSearch) != null);
        }


        /// <summary>
        /// 指定された更新型で存在インデックス位置を取得します
        /// </summary>
        /// <typeparam name="T">検索するループシステムの型</typeparam>
        /// <returns>最初に見つけたループシステムのインデックスを返しますが、見つけられなかった場合は ImtPlayerLoopSystem.LoopSystemNotFoundValue をかえします</returns>
        public int IndexOf<T>()
        {
            // 自身のサブループシステムに該当の型があるか調べるが、見つけられなかったら
            var result = subLoopSystemList.FindIndex(loopSystem => loopSystem.type == typeof(T));
            if (result == -1)
            {
                // 見つけられなかったことを返す
                return LoopSystemNotFoundValue;
            }


            // 見つけた位置を返す
            return result;
        }


        /// <summary>
        /// 内部で保持しているUnityネイティブ関数の参照をリセットします
        /// </summary>
        public void ResetUnityNativeFunctions()
        {
            // Unityのネイティブ関数系全てリセットする
            updateFunction = default(IntPtr);
            loopConditionFunction = default(IntPtr);
        }


        /// <summary>
        /// 指定された型を設定します
        /// </summary>
        /// <param name="type">変更する新しい型</param>
        /// <exception cref="ArgumentNullException">typeがnullです</exception>
        public void SetType(Type type)
        {
            // もしnullが渡されていたら
            if (type == null)
            {
                // 関数は死ぬ
                throw new ArgumentNullException(nameof(type));
            }


            // 指示された型を設定する
            this.type = type;
        }


        /// <summary>
        /// 指定された更新関数を設定します
        /// </summary>
        /// <param name="updateFunction">設定する新しい更新関数。nullを設定することができます</param>
        public void SetUpdateFunction(PlayerLoopSystem.UpdateFunction updateFunction)
        {
            // 更新関数を素直に設定する
            updateDelegate = updateFunction;
        }


        /// <summary>
        /// クラス化されているPlayerLoopSystemを構造体のPlayerLoopSystemへ変換します。
        /// また、サブループシステムを保持している場合はサブループシステムも構造体のインスタンスが新たに生成され、初期化されます。
        /// </summary>
        /// <returns>内部コンテキストのコピーを行ったPlayerLoopSystemを返します</returns>
        public PlayerLoopSystem ToPlayerLoopSystem()
        {
            // 新しいPlayerLoopSystem構造体のインスタンスを生成して初期化を行った後返す
            return new PlayerLoopSystem()
            {
                // 各パラメータのコピー（サブループシステムも再帰的に構造体へインスタンス化）
                type = type,
                updateDelegate = updateDelegate,
                updateFunction = updateFunction,
                loopConditionFunction = loopConditionFunction,
                subSystemList = subLoopSystemList.Select(source => source.ToPlayerLoopSystem()).ToArray(),
            };
        }
        #endregion


        #region オペレータ＆ToStringオーバーライド
        /// <summary>
        /// PlayerLoopSystemからImtPlayerLoopSystemへキャストします
        /// </summary>
        /// <param name="original">キャストする元になるPlayerLoopSystem</param>
        public static explicit operator ImtPlayerLoopSystem(PlayerLoopSystem original)
        {
            // 渡されたPlayerLoopSystemからImtPlayerLoopSystemのインスタンスを生成して返す
            return new ImtPlayerLoopSystem(ref original);
        }


        /// <summary>
        /// ImtPlayerLoopSystemからPlayerLoopSystemへキャストします
        /// </summary>
        /// <param name="klass">キャストする元になるImtPlayerLoopSystem</param>
        public static explicit operator PlayerLoopSystem(ImtPlayerLoopSystem klass)
        {
            // 渡されたImtPlayerLoopSystemからPlayerLoopSystemへ変換する関数を叩いて返す
            return klass.ToPlayerLoopSystem();
        }


        /// <summary>
        /// ImpPlayerLoopSystem内のLoopSystem階層表示を文字列へ変換します
        /// </summary>
        /// <returns>このインスタンスのLoopSystem階層状況を文字列化したものを返します</returns>
        public override string ToString()
        {
            // バッファ用意
            var buffer = new StringBuilder();


            // バッファにループシステムツリーの内容をダンプする
            DumpLoopSystemTree(buffer, string.Empty);


            // バッファの内容を返す
            return buffer.ToString();
        }


        /// <summary>
        /// ImpPlayerLoopSystem内のLoopSystem階層を再帰的にバッファへ文字列を追記します
        /// </summary>
        /// <param name="buffer">追記対象のバッファ</param>
        /// <param name="indentSpace">現在のインデントスペース</param>
        private void DumpLoopSystemTree(StringBuilder buffer, string indentSpace)
        {
            // 自分の名前からぶら下げツリー表記
            buffer.Append($"{indentSpace}[{(type == null ? "NULL" : type.Name)}]\n");
            foreach (var subSystem in subLoopSystemList)
            {
                // 新しいインデントスペース文字列を用意して自分の子にダンプさせる
                subSystem.DumpLoopSystemTree(buffer, indentSpace + "  ");
            }
        }
        #endregion
    }
    #endregion



    #region GameService
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



    /// <summary>
    /// サービスが動作するための更新タイミングを表します
    /// </summary>
    [Flags]
    public enum GameServiceUpdateTiming : UInt32
    {
        /// <summary>
        /// メインループ最初のタイミング。
        /// ただし、Time.frameCountや入力情報の更新直後となります。
        /// </summary>
        MainLoopHead = (1 << 0),

        /// <summary>
        /// MonoBehaviour.FixedUpdate直前のタイミング
        /// </summary>
        PreFixedUpdate = (1 << 1),

        /// <summary>
        /// MonoBehaviour.FixedUpdate直後のタイミング
        /// </summary>
        PostFixedUpdate = (1 << 2),

        /// <summary>
        /// 物理シミュレーション直後のタイミング。
        /// ただし、シミュレーションによる物理イベントキューが全て処理された直後となります。
        /// </summary>
        PostPhysicsSimulation = (1 << 3),

        /// <summary>
        /// WaitForFixedUpdate直後のタイミング。
        /// </summary>
        PostWaitForFixedUpdate = (1 << 4),

        /// <summary>
        /// UnitySynchronizationContextにPostされた関数キューが処理される直前のタイミング
        /// </summary>
        PreProcessSynchronizationContext = (1 << 5),

        /// <summary>
        /// UnitySynchronizationContextにPostされた関数キューが処理された直後のタイミング
        /// </summary>
        PostProcessSynchronizationContext = (1 << 6),

        /// <summary>
        /// MonoBehaviour.Update直前のタイミング
        /// </summary>
        PreUpdate = (1 << 7),

        /// <summary>
        /// MonoBehaviour.Update直後のタイミング
        /// </summary>
        PostUpdate = (1 << 8),

        /// <summary>
        /// UnityのAnimator(UpdateMode=Normal)によるポージング処理される直前のタイミング
        /// </summary>
        PreAnimation = (1 << 9),

        /// <summary>
        /// UnityのAnimator(UpdateMode=Normal)によるポージング処理された直後のタイミング
        /// </summary>
        PostAnimation = (1 << 10),

        /// <summary>
        /// MonoBehaviour.LateUpdate直前のタイミング
        /// </summary>
        PreLateUpdate = (1 << 11),

        /// <summary>
        /// MonoBehaviour.LateUpdate直後のタイミング
        /// </summary>
        PostLateUpdate = (1 << 12),

        /// <summary>
        /// メインスレッドにおける描画デバイスのPresentする直前のタイミング
        /// </summary>
        PreDrawPresent = (1 << 13),

        /// <summary>
        /// メインスレッドにおける描画デバイスのPresentされた直後のタイミング
        /// </summary>
        PostDrawPresent = (1 << 14),

        /// <summary>
        /// メインループの最後のタイミング。
        /// </summary>
        MainLoopTail = (1 << 15),

        /// <summary>
        /// Unityプレイヤーのフォーカスが得られたときのタイミング。
        /// OnApplicationFocus(true)。
        /// </summary>
        OnApplicationFocusIn = (1 << 16),

        /// <summary>
        /// Unityプレイヤーのフォーカスが失われたときのタイミング。
        /// OnApplicationFocus(false)。
        /// </summary>
        OnApplicationFocusOut = (1 << 17),

        /// <summary>
        /// Unityプレイヤーのメインループが一時停止したときのタイミング。
        /// OnApplicationPause(true)。
        /// </summary>
        OnApplicationSuspend = (1 << 18),

        /// <summary>
        /// Unityプレイヤーのメインループが再開したときのタイミング。
        /// OnApplicationPause(false)。
        /// </summary>
        OnApplicationResume = (1 << 19),

        /// <summary>
        /// あらゆるカメラのカリングが行われる直前のタイミング。
        /// ただし、カメラが存在する数分１フレームで複数回呼び出される可能性があります。
        /// さらに、スレッドはメインスレッド上におけるタイミングとなります。
        /// </summary>
        CameraPreCulling = (1 << 20),

        /// <summary>
        /// あらゆるカメラのレンダリングが行われる直前のタイミング。
        /// ただし、カメラが存在する数分１フレームで複数回呼び出される可能性があります。
        /// さらに、スレッドはメインスレッド上におけるタイミングとなります。
        /// </summary>
        CameraPreRendering = (1 << 21),

        /// <summary>
        /// あらゆるカメラのレンダリングが行われた直後のタイミング。
        /// ただし、カメラが存在する数分１フレームで複数回呼び出される可能性があります。
        /// さらに、スレッドはメインスレッド上におけるタイミングとなります。
        /// </summary>
        CameraPostRendering = (1 << 22),

        /// <summary>
        /// UnityプレイヤーのWaitForEndOfFrameの継続するタイミング。
        /// {yield return endOfFrame; OnEndOfFrame;}
        /// </summary>
        OnEndOfFrame = (1 << 23),
    }



    /// <summary>
    /// ゲームが終了要求に対する答えを表します
    /// </summary>
    public enum GameShutdownAnswer
    {
        /// <summary>
        /// ゲームが終了することを許可します
        /// </summary>
        Approve,

        /// <summary>
        /// ゲームが終了することを拒否します
        /// </summary>
        Reject,
    }



    /// <summary>
    /// ゲームサービスが動作を開始するための情報を保持する構造体です
    /// </summary>
    public struct GameServiceStartupInfo
    {
        /// <summary>
        /// サービスが更新処理として必要としている更新関数テーブル
        /// </summary>
        public Dictionary<GameServiceUpdateTiming, Action> UpdateFunctionTable { get; set; }
    }



    /// <summary>
    /// ゲームのサブシステムをサービスとして提供するための基本クラスです。
    /// ゲームのサブシステムを実装する場合は、このクラスを継承し適切な振る舞いを実装してください。
    /// </summary>
    public abstract class GameService
    {
        /// <summary>
        /// サービスを起動します。
        /// </summary>
        /// <param name="info">サービスが起動する時に必要とする情報を設定します</param>
        protected internal virtual void Startup(out GameServiceStartupInfo info)
        {
            // 特に何もしない起動情報を設定して修了
            info = new GameServiceStartupInfo()
            {
                UpdateFunctionTable = null,
            };
        }


        /// <summary>
        /// ゲームアプリケーションが終了することを許可するかどうかを判断します。
        /// </summary>
        /// <returns>アプリケーションが終了することを許可する場合は GameShutdownAnswer.Approve を、許可しない場合は GameShutdownAnswer.Reject を返します</returns>
        protected internal virtual GameShutdownAnswer JudgeGameShutdown()
        {
            // 通常は許可をする
            return GameShutdownAnswer.Approve;
        }


        /// <summary>
        /// サービスをシャットダウンします
        /// </summary>
        protected internal virtual void Shutdown()
        {
        }
    }



    /// <summary>
    /// IceMilkTeaのゲームサービスを管理及び制御を行うクラスです。
    /// </summary>
    public class GameServiceManager
    {
        /// <summary>
        /// サービスの状態を表します
        /// </summary>
        protected enum ServiceStatus
        {
            /// <summary>
            /// サービスが生成され、動作を開始する準備が出来ました。
            /// </summary>
            Ready,

            /// <summary>
            /// サービスが生成され、動作を開始する準備が出来ていますが、休止中です。
            /// </summary>
            ReadyButSleeping,

            /// <summary>
            /// サービスは動作中です。
            /// </summary>
            Running,

            /// <summary>
            /// サービスは休止中です。
            /// </summary>
            Sleeping,

            /// <summary>
            /// サービスは破棄対象としてマークされ、シャットダウン状態になりました。
            /// </summary>
            Shutdown,

            /// <summary>
            /// サービスは破棄対象としてマークされましたが、シャットダウン処理は実行されずそのまま破棄される状態になりました。
            /// </summary>
            SilentShutdown,
        }



        /// <summary>
        /// サービスマネージャが管理するサービスの管理情報を保持するデータクラスです
        /// </summary>
        protected class ServiceManagementInfo
        {
            /// <summary>
            /// サービス本体への参照
            /// </summary>
            public GameService Service { get; set; }


            /// <summary>
            /// サービスの状態
            /// </summary>
            public ServiceStatus Status { get; set; }


            /// <summary>
            /// 管理しているサービス本体のクラスが継承している型で、GameService型を直接継承している基本となるサービスの型
            /// </summary>
            public Type BaseGameServiceType { get; set; }


            /// <summary>
            /// このサービスが利用している更新関数テーブル
            /// </summary>
            public Dictionary<GameServiceUpdateTiming, Action> UpdateFunctionTable { get; set; }
        }



        // メンバ変数定義
        protected List<ServiceManagementInfo> serviceManageList;



        /// <summary>
        /// GameServiceManager の初期化を行います
        /// </summary>
        public GameServiceManager()
        {
            // サービス管理用リストのインスタンスを生成
            serviceManageList = new List<ServiceManagementInfo>();
        }


        #region 起動と停止
        /// <summary>
        /// サービスマネージャの起動をします。
        /// </summary>
        protected internal virtual void Startup()
        {
            // 各種更新関数のLoopSystemを生成する
            var mainLoopHead = new ImtPlayerLoopSystem(typeof(GameServiceUpdate.GameServiceMainLoopHead), () => DoUpdateService(GameServiceUpdateTiming.MainLoopHead));
            var preFixedUpdate = new ImtPlayerLoopSystem(typeof(GameServiceUpdate.GameServicePreFixedUpdate), () => DoUpdateService(GameServiceUpdateTiming.PreFixedUpdate));
            var postFixedUpdate = new ImtPlayerLoopSystem(typeof(GameServiceUpdate.GameServicePostFixedUpdate), () => DoUpdateService(GameServiceUpdateTiming.PostFixedUpdate));
            var postPhysicsSimulation = new ImtPlayerLoopSystem(typeof(GameServiceUpdate.GameServicePostPhysicsSimulation), () => DoUpdateService(GameServiceUpdateTiming.PostPhysicsSimulation));
            var postWaitForFixedUpdate = new ImtPlayerLoopSystem(typeof(GameServiceUpdate.GameServicePostWaitForFixedUpdate), () => DoUpdateService(GameServiceUpdateTiming.PostWaitForFixedUpdate));
            var preUpdate = new ImtPlayerLoopSystem(typeof(GameServiceUpdate.GameServicePreUpdate), () => DoUpdateService(GameServiceUpdateTiming.PreUpdate));
            var postUpdate = new ImtPlayerLoopSystem(typeof(GameServiceUpdate.GameServicePostUpdate), () => DoUpdateService(GameServiceUpdateTiming.PostUpdate));
            var preProcessSynchronizationContext = new ImtPlayerLoopSystem(typeof(GameServiceUpdate.GameServicePreProcessSynchronizationContext), () => DoUpdateService(GameServiceUpdateTiming.PreProcessSynchronizationContext));
            var postProcessSynchronizationContext = new ImtPlayerLoopSystem(typeof(GameServiceUpdate.GameServicePostProcessSynchronizationContext), () => DoUpdateService(GameServiceUpdateTiming.PostProcessSynchronizationContext));
            var preAnimation = new ImtPlayerLoopSystem(typeof(GameServiceUpdate.GameServicePreAnimation), () => DoUpdateService(GameServiceUpdateTiming.PreAnimation));
            var postAnimation = new ImtPlayerLoopSystem(typeof(GameServiceUpdate.GameServicePostAnimation), () => DoUpdateService(GameServiceUpdateTiming.PostAnimation));
            var preLateUpdate = new ImtPlayerLoopSystem(typeof(GameServiceUpdate.GameServicePreLateUpdate), () => DoUpdateService(GameServiceUpdateTiming.PreLateUpdate));
            var postLateUpdate = new ImtPlayerLoopSystem(typeof(GameServiceUpdate.GameServicePostLateUpdate), () => DoUpdateService(GameServiceUpdateTiming.PostLateUpdate));
            var preDrawPresent = new ImtPlayerLoopSystem(typeof(GameServiceUpdate.GameServicePreDrawPresent), () => DoUpdateService(GameServiceUpdateTiming.PreDrawPresent));
            var postDrawPresent = new ImtPlayerLoopSystem(typeof(GameServiceUpdate.GameServicePostDrawPresent), () => DoUpdateService(GameServiceUpdateTiming.PostDrawPresent));
            var mainLoopTail = new ImtPlayerLoopSystem(typeof(GameServiceUpdate.GameServiceMainLoopTail), () => DoUpdateService(GameServiceUpdateTiming.MainLoopTail));


            // 処理を差し込むためのPlayerLoopSystemを取得して、処理を差し込んで構築する
            var loopSystem = ImtPlayerLoopSystem.GetLastBuildLoopSystem();
            loopSystem.InsertLoopSystem<GameMain.GameServiceManagerStartup>(InsertTiming.AfterInsert, mainLoopHead);
            loopSystem.InsertLoopSystem<FixedUpdate.ScriptRunBehaviourFixedUpdate>(InsertTiming.BeforeInsert, preFixedUpdate);
            loopSystem.InsertLoopSystem<FixedUpdate.ScriptRunBehaviourFixedUpdate>(InsertTiming.AfterInsert, postFixedUpdate);
            loopSystem.InsertLoopSystem<FixedUpdate.DirectorFixedUpdatePostPhysics>(InsertTiming.AfterInsert, postPhysicsSimulation);
            loopSystem.InsertLoopSystem<FixedUpdate.ScriptRunDelayedFixedFrameRate>(InsertTiming.AfterInsert, postWaitForFixedUpdate);
            loopSystem.InsertLoopSystem<Update.ScriptRunBehaviourUpdate>(InsertTiming.BeforeInsert, preUpdate);
            loopSystem.InsertLoopSystem<Update.ScriptRunBehaviourUpdate>(InsertTiming.AfterInsert, postUpdate);
            loopSystem.InsertLoopSystem<Update.ScriptRunDelayedTasks>(InsertTiming.BeforeInsert, preProcessSynchronizationContext);
            loopSystem.InsertLoopSystem<Update.ScriptRunDelayedTasks>(InsertTiming.AfterInsert, postProcessSynchronizationContext);
            loopSystem.InsertLoopSystem<Update.DirectorUpdate>(InsertTiming.BeforeInsert, preAnimation);
            loopSystem.InsertLoopSystem<Update.DirectorUpdate>(InsertTiming.AfterInsert, postAnimation);
            loopSystem.InsertLoopSystem<PreLateUpdate.ScriptRunBehaviourLateUpdate>(InsertTiming.BeforeInsert, preLateUpdate);
            loopSystem.InsertLoopSystem<PreLateUpdate.ScriptRunBehaviourLateUpdate>(InsertTiming.AfterInsert, postLateUpdate);
            loopSystem.InsertLoopSystem<PostLateUpdate.PresentAfterDraw>(InsertTiming.BeforeInsert, preDrawPresent);
            loopSystem.InsertLoopSystem<PostLateUpdate.PresentAfterDraw>(InsertTiming.AfterInsert, postDrawPresent);
            loopSystem.InsertLoopSystem<GameMain.GameServiceManagerCleanup>(InsertTiming.BeforeInsert, mainLoopTail);
            loopSystem.BuildAndSetUnityPlayerLoop();


            // 永続ゲームオブジェクトを生成してアプリケーションのフォーカス、ポーズのハンドラを登録する
            var persistentGameObject = ImtUnityUtility.CreatePersistentGameObject();
            var eventBridge = MonoBehaviourEventBridge.Attach(persistentGameObject);
            eventBridge.SetApplicationFocusFunction(OnApplicationFocus);
            eventBridge.SetApplicationPauseFunction(OnApplicationPause);
            eventBridge.SetEndOfFrameFunction(OnEndOfFrame);


            // カメラのハンドラを登録する
            Camera.onPreCull += OnCameraPreCulling;
            Camera.onPreRender += OnCameraPreRendering;
            Camera.onPostRender += OnCameraPostRendering;


            // アプリケーション終了要求ハンドラを登録する
            Application.wantsToQuit += OnApplicationWantsToQuit;
        }


        /// <summary>
        /// サービスマネージャの停止をします。
        /// </summary>
        protected internal virtual void Shutdown()
        {
            // 終了要求ハンドラを解除する
            Application.wantsToQuit -= OnApplicationWantsToQuit;


            // カメラのハンドラを解除する
            Camera.onPreCull -= OnCameraPreCulling;
            Camera.onPreRender -= OnCameraPreRendering;
            Camera.onPostRender -= OnCameraPostRendering;


            // サービスの数分ループ
            for (int i = 0; i < serviceManageList.Count; ++i)
            {
                // サービスの状態が Running, Shutdown, Sleeping 以外なら
                var serviceInfo = serviceManageList[i];
                if (!(serviceInfo.Status == ServiceStatus.Running || serviceInfo.Status == ServiceStatus.Shutdown || serviceInfo.Status == ServiceStatus.Sleeping))
                {
                    // 次へ
                    continue;
                }


                // サービスのシャットダウンを呼ぶ
                serviceInfo.Service.Shutdown();
            }


            // 管理リストをクリアする
            serviceManageList.Clear();
        }
        #endregion


        #region 更新系
        /// <summary>
        /// Addされたサービスの起動処理を行います。
        /// </summary>
        protected internal virtual void StartupServices()
        {
            // サービスの起動情報を受け取る変数を用意
            var serviceStartupInfo = default(GameServiceStartupInfo);


            // サービスの数分ループ
            for (int i = 0; i < serviceManageList.Count; ++i)
            {
                // サービスの状態がReady以外なら
                if (serviceManageList[i].Status != ServiceStatus.Ready)
                {
                    // 次の項目へ
                    continue;
                }


                // サービスを起動状態に設定、サービスの起動処理を実行して更新関数テーブルのキャッシュをする
                serviceManageList[i].Status = ServiceStatus.Running;
                serviceManageList[i].Service.Startup(out serviceStartupInfo);
                serviceManageList[i].UpdateFunctionTable = serviceStartupInfo.UpdateFunctionTable ?? new Dictionary<GameServiceUpdateTiming, Action>();
            }
        }


        /// <summary>
        /// Removeされたサービスの停止処理を行います。
        /// </summary>
        protected internal virtual void CleanupServices()
        {
            // 実際の破棄そのもののステップ必要かどうかを検知するための変数を用意
            var needDeleteStep = false;


            // サービスの数分ループ
            for (int i = 0; i < serviceManageList.Count; ++i)
            {
                // サービスの状態がShutdownでないなら
                if (serviceManageList[i].Status != ServiceStatus.Shutdown)
                {
                    // サービスの状態がサイレントシャットダウンなら
                    if (serviceManageList[i].Status == ServiceStatus.SilentShutdown)
                    {
                        // シャットダウン関数を呼びはしないが破棄ステップでは破棄されるようにマーク
                        needDeleteStep = true;
                    }


                    // 次の項目へ
                    continue;
                }


                // サービスの停止処理を実行する（が、このタイミングでは破棄しない、破棄のタイミングは次のステップで行う）
                serviceManageList[i].Service.Shutdown();


                // 破棄処理を行うようにマーク
                needDeleteStep = true;
            }


            // もし破棄処理をしないなら
            if (!needDeleteStep)
            {
                // ここで終了
                return;
            }


            // サービスの数分ケツからループ
            for (int i = serviceManageList.Count - 1; i >= 0; --i)
            {
                // サービスの状態がShutdown系なら
                var status = serviceManageList[i].Status;
                var isShutdown = status == ServiceStatus.Shutdown || status == ServiceStatus.SilentShutdown;
                if (isShutdown)
                {
                    // リストからサービスをパージする
                    serviceManageList.RemoveAt(i);
                }
            }
        }
        #endregion


        #region コントロール系
        /// <summary>
        /// 指定されたサービスのアクティブ状態を設定します。
        /// </summary>
        /// <typeparam name="T">アクティブ状態を設定する対象のサービスの型</typeparam>
        /// <param name="active">設定する状態（true=アクティブ false=非アクティブ）</param>
        /// <exception cref="GameServiceNotFoundException">指定された型のサービスが見つかりませんでした</exception>
        /// <exception cref="InvalidOperationException">指定された型のサービスは見つかりましたが、シャットダウン状態になっています</exception>
        public virtual void SetActiveService<T>(bool active) where T : GameService
        {
            // 指定された型から管理情報を取得するが、取得に失敗または取得したがキャスト不可の型なら
            var serviceInfo = GetServiceInfo(typeof(T));
            if (serviceInfo == null || !(serviceInfo.Service is T))
            {
                // サービスを見つけられなかったとして例外を吐く
                throw new GameServiceNotFoundException(typeof(T));
            }


            // もしサービスがシャットダウン予定になっているのなら
            var shutdownStatus = (serviceInfo.Status == ServiceStatus.Shutdown || serviceInfo.Status == ServiceStatus.SilentShutdown);
            if (shutdownStatus)
            {
                // 無効な操作として例外を吐く
                throw new InvalidOperationException($"サービス'{typeof(T).Name}'は、シャットダウン状態です。");
            }


            // アクティブにするなら
            if (active)
            {
                // 現在の状態によって遷移先状態を変える
                serviceInfo.Status = serviceInfo.Status == ServiceStatus.ReadyButSleeping ? ServiceStatus.Ready : ServiceStatus.Running;
            }
            else
            {
                // 現在の状態によって遷移先状態を変える
                serviceInfo.Status = serviceInfo.Status == ServiceStatus.Ready ? ServiceStatus.ReadyButSleeping : ServiceStatus.Sleeping;
            }
        }


        /// <summary>
        /// 指定されたサービスがアクティブかどうかを確認します。
        /// </summary>
        /// <typeparam name="T">アクティブ状態を確認するサービスの型</typeparam>
        /// <returns>アクティブの場合は true を、非アクティブの場合は false を返します</returns>
        /// <exception cref="GameServiceNotFoundException">指定された型のサービスが見つかりませんでした</exception>
        public virtual bool IsActiveService<T>() where T : GameService
        {
            // 指定された型から管理情報を取得するが、取得に失敗または取得したがキャスト不可の型なら
            var serviceInfo = GetServiceInfo(typeof(T));
            if (serviceInfo == null || !(serviceInfo.Service is T))
            {
                // サービスを見つけられなかったとして例外を吐く
                throw new GameServiceNotFoundException(typeof(T));
            }


            // Running, Ready以外は全部非アクティブ
            return serviceInfo.Status == ServiceStatus.Ready || serviceInfo.Status == ServiceStatus.Running;
        }
        #endregion


        #region リスト操作系
        /// <summary>
        /// 指定されたサービスの追加をします。
        /// また、サービスの型が同じインスタンスまたは同一継承元インスタンスが存在する場合は例外がスローされます。
        /// ただし、サービスは直ちには起動せずフレーム開始のタイミングで起動することに注意してください。
        /// さらに、シャットダウン対象となっているサービスの場合は無効な操作として例外がスローされます。
        /// </summary>
        /// <param name="service">追加するサービスのインスタンス</param>
        /// <exception cref="ArgumentNullException">service が null です</exception>
        /// <exception cref="GameServiceAlreadyExistsException">既に同じ型のサービスが追加されています</exception>
        /// <exception cref="InvalidOperationException">追加しようとしたサービスが既にシャットダウン状態です</exception>
        public virtual void AddService(GameService service)
        {
            // null が渡されちゃったら
            if (service == null)
            {
                // そんな処理は許されない
                throw new ArgumentNullException(nameof(service));
            }


            // まずは管理リストから情報が取り出せるなら
            var serviceInfo = GetServiceInfo(service.GetType());
            if (serviceInfo != null)
            {
                // 既に存在しているサービスの型を取り出す
                var existsServiceType = serviceInfo.Service.GetType();


                // もし既にシャットダウン状態でかつ、追加しようとしたサービスの型がキャスト可能な型なら
                var shutdownState = (serviceInfo.Status == ServiceStatus.Shutdown || serviceInfo.Status == ServiceStatus.SilentShutdown);
                if (shutdownState && service.GetType().IsAssignableFrom(existsServiceType))
                {
                    // サービスはシャットダウン状態である例外を吐く
                    throw new InvalidOperationException($"サービス'{service.GetType().Name}'は、既にシャットダウン状態です。");
                }


                // 既に同じサービスがあるとして例外を吐く
                throw new GameServiceAlreadyExistsException(existsServiceType, serviceInfo.BaseGameServiceType);
            }


            // 管理リストから情報を取り出せないのなら追加が可能なサービスとして追加する
            serviceManageList.Add(new ServiceManagementInfo()
            {
                Status = ServiceStatus.Ready,
                Service = service,
                BaseGameServiceType = GetBaseGameServiceType(service),
            });
        }


        /// <summary>
        /// 指定されたサービスの追加をします。
        /// この関数は AddService() 関数と違い、同じ型のサービスまたは同一継承元インスタンスの追加は出来ませんが、例外をスローしません。
        /// ただし、サービスは直ちには起動せずフレーム開始のタイミングで起動することに注意してください。
        /// </summary>
        /// <param name="service">追加するサービスのインスタンス</param>
        /// <returns>サービスの追加が出来た場合は true を、出来なかった場合は false を返します</returns>
        /// <exception cref="ArgumentNullException">service が null です</exception>
        public virtual bool TryAddService(GameService service)
        {
            // null が渡されちゃったら
            if (service == null)
            {
                // いくらTry系関数とは言えど、そんな処理は許されない
                throw new ArgumentNullException(nameof(service));
            }


            // 管理リストから情報が取り出せるなら
            var serviceInfo = GetServiceInfo(service.GetType());
            if (serviceInfo != null)
            {
                // 既に登録済みの何かのサービスがいるとしてfalseを返す
                return false;
            }


            // 管理リストから情報を取り出せないのなら追加が可能なサービスとして追加する
            serviceManageList.Add(new ServiceManagementInfo()
            {
                Status = ServiceStatus.Ready,
                Service = service,
                BaseGameServiceType = GetBaseGameServiceType(service),
            });


            // 追加が出来たということを返す
            return true;
        }


        /// <summary>
        /// 指定された型のサービスを取得します。
        /// また、サービスが見つけられなかった場合は例外がスローされます。
        /// </summary>
        /// <typeparam name="T">取得するサービスの型</typeparam>
        /// <returns>見つけられたサービスのインスタンスを返します</returns>
        /// <exception cref="GameServiceNotFoundException">指定された型のサービスが見つかりませんでした</exception>
        public virtual T GetService<T>() where T : GameService
        {
            // 指定された型から管理情報を取得するが、取得に失敗または取得したがキャスト不可の型なら
            var serviceInfo = GetServiceInfo(typeof(T));
            if (serviceInfo == null || !(serviceInfo.Service is T))
            {
                // サービスを見つけられなかったとして例外を吐く
                throw new GameServiceNotFoundException(typeof(T));
            }


            // 見つけたサービスを返す
            return (T)serviceInfo.Service;
        }


        /// <summary>
        /// 指定された型のサービスを取得します
        /// </summary>
        /// <typeparam name="T">取得するサービスの型</typeparam>
        /// <param name="service">見つけられたサービスのインスタンスを設定しますが、見つけられなかった場合はnullが設定されます</param>
        public virtual bool TryGetService<T>(out T service) where T : GameService
        {
            // 指定された型から管理情報を取得するが、取得に失敗または取得したがキャスト不可の型なら
            var serviceInfo = GetServiceInfo(typeof(T));
            if (serviceInfo == null || !(serviceInfo.Service is T))
            {
                // サービスにnullを設定して取得できなかったことを返す
                service = null;
                return false;
            }


            // 見つけたサービスを設定して見つけられた事を返す
            service = (T)serviceInfo.Service;
            return true;
        }


        /// <summary>
        /// 指定された型のサービスを削除します。
        /// しかし、サービスは直ちには削除されずフレーム終了のタイミングで削除されることに注意してください。
        /// </summary>
        /// <typeparam name="T">削除するサービスの型</typeparam>
        public virtual void RemoveService<T>() where T : GameService
        {
            // 指定された型から管理情報を取得するが、取得に失敗または取得したがキャスト不可の型なら
            var serviceInfo = GetServiceInfo(typeof(T));
            if (serviceInfo == null || !(serviceInfo.Service is T))
            {
                // 何事もなかったかのように終了する
                return;
            }


            // サービスの状態がまだReady系なら静かに死ぬようにマークする
            var readyStatus = (serviceInfo.Status == ServiceStatus.Ready || serviceInfo.Status == ServiceStatus.ReadyButSleeping);
            if (readyStatus)
            {
                // 静かに消えるがよい
                serviceInfo.Status = ServiceStatus.SilentShutdown;
                return;
            }


            // 通常は普通に死ぬようにマークする
            serviceInfo.Status = ServiceStatus.Shutdown;
        }


        /// <summary>
        /// 指定された型のサービスが、単純に存在するか確認します。
        /// この関数は、シャットダウンされうかどうかの状態を考慮しないことに気をつけて下さい。
        /// </summary>
        /// <typeparam name="T">存在を確認するサービスの型</typeparam>
        /// <returns>サービスが存在している場合は true を、存在しない場合は false を返します</returns>
        public virtual bool Exists<T>() where T : GameService
        {
            // 指定された型から管理情報を取得するが、取得に失敗または取得したがキャスト不可の型なら
            var serviceInfo = GetServiceInfo(typeof(T));
            if (serviceInfo == null || !(serviceInfo.Service is T))
            {
                // サービスが無いことを返す
                return false;
            }


            // 見つけたサービスを見つけられたことを返す
            return true;
        }
        #endregion


        #region Unityイベントハンドラ
        /// <summary>
        /// アプリケーションが終了を要求してきた時の処理を行います
        /// </summary>
        /// <returns>サービスが終了を許可した場合は true を、拒否された場合は false を返します</returns>
        private bool OnApplicationWantsToQuit()
        {
            // サービスの数分回る
            for (int i = 0; i < serviceManageList.Count; ++i)
            {
                // サービスの状態が Running 以外なら
                var service = serviceManageList[i];
                if (service.Status != ServiceStatus.Running)
                {
                    // 次へ
                    continue;
                }


                // サービスに終了判断をしてもらい、拒否されたら
                if (service.Service.JudgeGameShutdown() == GameShutdownAnswer.Reject)
                {
                    // この段階でfalseを返す
                    return false;
                }
            }


            // 最後まで回りきったら全員が許可したとしてtrueを返す
            return true;
        }


        /// <summary>
        /// Unityプレイヤーのフォーカス状態に変化があった時の処理を行います
        /// </summary>
        /// <param name="focus">フォーカスを得られたときは true を、得られなかったときは false が渡されます</param>
        private void OnApplicationFocus(bool focus)
        {
            // もしフォーカスを得られたのなら
            if (focus)
            {
                // FocusInのサービス呼び出しをする
                DoUpdateService(GameServiceUpdateTiming.OnApplicationFocusIn);
                return;
            }


            // FocusOutのサービス呼び出しをする
            DoUpdateService(GameServiceUpdateTiming.OnApplicationFocusOut);
        }


        /// <summary>
        /// Unityプレイヤーの再生状態に変化があった時の処理を行います
        /// </summary>
        /// <param name="pause">一時停止した場合は true を、再開した場合は false が渡されます</param>
        private void OnApplicationPause(bool pause)
        {
            // もし一時停止なら
            if (pause)
            {
                // Suspendのサービス呼び出しをする
                DoUpdateService(GameServiceUpdateTiming.OnApplicationSuspend);
                return;
            }


            // Resumeのサービス呼び出しをする
            DoUpdateService(GameServiceUpdateTiming.OnApplicationResume);
        }


        /// <summary>
        /// UnityプレイヤーのWaitForEndOfFrameの継続処理を行います
        /// </summary>
        private void OnEndOfFrame()
        {
            // EndOfFrameのサービス呼び出しをする
            DoUpdateService(GameServiceUpdateTiming.OnEndOfFrame);
        }


        /// <summary>
        /// カメラのカリング処理を始める直前の処理を行います
        /// </summary>
        /// <param name="targetCamera">処理するカメラ</param>
        private void OnCameraPreCulling(Camera targetCamera)
        {
            // CameraPreCullingのサービス呼び出しをする
            DoUpdateService(GameServiceUpdateTiming.CameraPreCulling);
        }


        /// <summary>
        /// カメラのレンダリング処理を始める直前の処理を行います
        /// </summary>
        /// <param name="targetCamera">処理するカメラ</param>
        private void OnCameraPreRendering(Camera targetCamera)
        {
            // CameraPreRenderingのサービス呼び出しをする
            DoUpdateService(GameServiceUpdateTiming.CameraPreRendering);
        }


        /// <summary>
        /// カメラのレンダリング処理が終わった直後の処理を行います
        /// </summary>
        /// <param name="targetCamera"></param>
        private void OnCameraPostRendering(Camera targetCamera)
        {
            // CameraPostRenderingのサービス呼び出しをする
            DoUpdateService(GameServiceUpdateTiming.CameraPostRendering);
        }
        #endregion


        #region 共通ロジック系
        /// <summary>
        /// 指定されたタイミングのサービス更新関数を実行します。
        /// また、実行する対象はステータスに応じた呼び出しを行います。
        /// </summary>
        /// <param name="timing">実行するべきサービスの更新関数のタイミング</param>
        private void DoUpdateService(GameServiceUpdateTiming timing)
        {
            // サービスの数分回る
            for (int i = 0; i < serviceManageList.Count; ++i)
            {
                // サービス情報を取得する
                var serviceInfo = serviceManageList[i];


                // サービスの状態がRunning以外なら
                if (serviceInfo.Status != ServiceStatus.Running)
                {
                    // 次のサービスへ
                    continue;
                }


                // 該当のタイミングの更新関数を持っていないなら
                Action updateFunction;
                if (!serviceInfo.UpdateFunctionTable.TryGetValue(timing, out updateFunction))
                {
                    // 次のサービスへ
                    continue;
                }


                // 該当タイミングの更新関数を持っているのなら更新関数を叩く
                updateFunction();
            }
        }


        /// <summary>
        /// 指定された型からサービス管理情報を取得します。
        /// また、型一致条件は基本クラス一致となります。
        /// </summary>
        /// <param name="serviceType">取得したいサービスの型。ただし、内部ではGameServiceを直接継承している型が採用されます。</param>
        /// <returns>指定された型から取り出せるサービス管理情報を返しますが、取得できなかった場合は null を返します。</returns>
        private ServiceManagementInfo GetServiceInfo(Type serviceType)
        {
            // まずは基本型を取り出すがこの時点で取り出せなかったら
            var baseGameServiceType = GetBaseGameServiceType(serviceType);
            if (baseGameServiceType == null)
            {
                // ダメだったよ
                return null;
            }


            // サービスの数分回る
            for (int i = 0; i < serviceManageList.Count; ++i)
            {
                // 基本型が一致するサービスなら
                var serviceInfo = serviceManageList[i];
                if (serviceInfo.BaseGameServiceType == baseGameServiceType)
                {
                    // この情報を返す
                    return serviceInfo;
                }
            }


            // ループから抜けてきたという事は見つからなかったということ
            return null;
        }


        /// <summary>
        /// 指定されたゲームサービスの、GameService型を直接継承している基本クラスの型を取得します
        /// </summary>
        /// <param name="service">GameService型を直接継承しているクラスの型を取り出したいサービス</param>
        /// <returns>GameService型を直接継承している基本クラスの型を返します</returns>
        private Type GetBaseGameServiceType(GameService service)
        {
            // 渡されたサービスの型を受け取って型取得結果をそのまま返す（GameServiceで受け取っているのでnullになることはない）
            return GetBaseGameServiceType(service.GetType());
        }


        /// <summary>
        /// 指定された型から、GameService型を直接継承している基本クラスの型を取得します
        /// </summary>
        /// <param name="serviceType">GameService型を直接継承しているクラスの型を取り出したい型</param>
        /// <returns>GameService型を直接継承している基本クラスの型が見つけられた場合はその型を返しますが、見つけられなかった場合は null を返します</returns>
        private Type GetBaseGameServiceType(Type serviceType)
        {
            // GameServiceの型を取得
            var gameServiceType = typeof(GameService);


            // 直接GameService型を継承している型にたどり着くまでループ
            while (serviceType.BaseType != gameServiceType)
            {
                // 現在のサービス型の継承元の型を現在のサービスの型にするがnullなら
                serviceType = serviceType.BaseType;
                if (serviceType == null)
                {
                    // そもそも見つけられなかった
                    return null;
                }
            }


            // たどり着いた型を返す
            return serviceType;
        }
        #endregion
    }



    /// <summary>
    /// サービスが既に存在している場合にスローされる例外クラスです
    /// </summary>
    public class GameServiceAlreadyExistsException : Exception
    {
        /// <summary>
        /// GameServiceAlreadyExistsException インスタンスの初期化をします
        /// </summary>
        /// <param name="serviceType">既に存在しているサービスのタイプ</param>
        /// <param name="baseType">存在しているサービスの基本となるタイプ</param>
        public GameServiceAlreadyExistsException(Type serviceType, Type baseType) : base($"'{serviceType.Name}'のサービスは既に、'{baseType.Name}'として存在しています")
        {
        }


        /// <summary>
        /// GameServiceAlreadyExistsException インスタンスの初期化をします
        /// </summary>
        /// <param name="serviceType">既に存在しているサービスのタイプ</param>
        /// <param name="baseType">存在しているサービスの基本となるタイプ</param>
        /// <param name="inner">この例外がスローされる原因となったら例外</param>
        public GameServiceAlreadyExistsException(Type serviceType, Type baseType, Exception inner) : base($"'{serviceType.Name}'のサービスは既に、'{baseType.Name}'として存在しています", inner)
        {
        }
    }



    /// <summary>
    /// サービスが見つからなかった場合にスローされる例外クラスです
    /// </summary>
    public class GameServiceNotFoundException : Exception
    {
        /// <summary>
        /// GameServiceNotFoundException インスタンスの初期化をします
        /// </summary>
        /// <param name="serviceType">見つけられなかったサービスのタイプ</param>
        public GameServiceNotFoundException(Type serviceType) : base($"'{serviceType.Name}'のサービスを見つけられませんでした")
        {
        }


        /// <summary>
        /// GameServiceNotFoundException インスタンスの初期化をします
        /// </summary>
        /// <param name="serviceType">見つけられなかったサービスのタイプ</param>
        /// <param name="inner">この例外がスローされる原因となった例外</param>
        public GameServiceNotFoundException(Type serviceType, Exception inner) : base($"'{serviceType.Name}'のサービスを見つけられませんでした", inner)
        {
        }
    }
    #endregion



    #region Utility
    /// <summary>
    /// Unity関連実装でユーティリティな関数として使えるような、関数が実装されているクラスです
    /// </summary>
    public static class ImtUnityUtility
    {
        /// <summary>
        /// 永続的に存在し続けるゲームオブジェクトを生成します。
        /// この関数で生成されるゲームオブジェクトはヒエラルキに表示されません。
        /// また、名前はNewGameObjectとして作られます。
        /// </summary>
        /// <returns>生成された永続ゲームオブジェクトを返します</returns>
        public static GameObject CreatePersistentGameObject()
        {
            // "NewGameObject" な見えないゲームオブジェクトを生成して返す
            return CreatePersistentGameObject("NewGameObject", HideFlags.HideInHierarchy);
        }


        /// <summary>
        /// 永続的に存在し続けるゲームオブジェクトを生成します。
        /// この関数で生成されるゲームオブジェクトはヒエラルキに表示されません。
        /// </summary>
        /// <param name="name">生成する永続ゲームオブジェクトの名前</param>
        /// <returns>生成された永続ゲームオブジェクトを返します</returns>
        public static GameObject CreatePersistentGameObject(string name)
        {
            // 見えないゲームオブジェクトを生成して返す
            return CreatePersistentGameObject(name, HideFlags.HideInHierarchy);
        }


        /// <summary>
        /// 永続的に存在し続けるゲームオブジェクトを生成します。
        /// </summary>
        /// <param name="name">生成する永続ゲームオブジェクトの名前</param>
        /// <param name="hideFlags">生成する永続ゲームオブジェクトの隠しフラグ</param>
        /// <returns>生成された永続ゲームオブジェクトを返します</returns>
        public static GameObject CreatePersistentGameObject(string name, HideFlags hideFlags)
        {
            // ゲームオブジェクトを生成する
            var gameObject = new GameObject(name);


            // ヒエラルキから姿を消して永続化
            gameObject.hideFlags = hideFlags;
            UnityObject.DontDestroyOnLoad(gameObject);


            // トランスフォームを取得して念の為初期値を入れる
            var transform = gameObject.GetComponent<Transform>();
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;


            // 作ったゲームオブジェクトを返す
            return gameObject;
        }
    }
    #endregion
}