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
using System.Collections;
using System.Collections.Generic;
using IceMilkTea.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace IceMilkTeaTestDynamic.Core
{
    #region テスト確認用クラス
    /*
     * テストで用いるクラスは以下の継承構造になります。
     * 
     * GameService（IceMilkTeaにおけるサービスの抽象クラス）
     * ├ ServiceBaseA（ユーザー定義クラスの最基底サービスクラスでありキーとなるクラス）
     * │  └ServiceA_1_0
     * │   ├ ServiceA_2_0
     * │   └ ServiceA_2_1
     * │
     * └ ServiceBaseB（ユーザー定義クラスの最基底サービスクラスでありキーとなるクラス）
     *   └ ServiceB_1_0
     *      ├ ServiceB_2_0
     *      └ ServiceB_2_1
     * 
     * IceMilkTeaでは "GameServiceクラスを直接継承したクラス" がキーとなるクラスとなり
     * そのクラスを継承したすべての派生クラスは、キーとなるクラスが重複して登録されない制約が付きます。
     */

    /// <summary>
    /// サービスAの基本テストクラスです
    /// </summary>
    public class ServiceBaseA : GameService
    {
        /// <summary>
        /// サービスが起動済みかどうか
        /// </summary>
        public bool Startuped { get; set; }


        /// <summary>
        /// サービスが停止済みかどうか
        /// </summary>
        public bool Stopped { get; set; }


        /// <summary>
        /// PreUpdate毎にカウントし続けるカウンタ
        /// </summary>
        public int StepCount { get; set; }



        /// <summary>
        /// サービスの起動処理を行います
        /// </summary>
        /// <param name="info">サービスの起動情報を設定します</param>
        protected internal override void Startup(out GameServiceStartupInfo info)
        {
            // サービス起動情報の初期化
            info = new GameServiceStartupInfo()
            {
                // 更新関数テーブルの初期化
                UpdateFunctionTable = new Dictionary<GameServiceUpdateTiming, Action>()
                {
                    {GameServiceUpdateTiming.PreUpdate, PreUpdate}
                }
            };


            // サービスは起動済みである
            Startuped = true;
        }


        /// <summary>
        /// サービスの停止処理を行います
        /// </summary>
        protected internal override void Shutdown()
        {
            // サービスは停止済みである
            Stopped = true;
        }


        /// <summary>
        /// Update前の処理を実行します
        /// </summary>
        private void PreUpdate()
        {
            // カウントし続ける
            StepCount += 1;
        }
    }



    /// <summary>
    /// サービスAの基本クラスから派生したサービスA1クラスです
    /// </summary>
    public class ServiceA_1_0 : ServiceBaseA
    {
    }



    /// <summary>
    /// サービスAクラスから更に派生したサービスA2クラスです
    /// </summary>
    public class ServiceA_2_0 : ServiceA_1_0
    {
    }



    /// <summary>
    /// サービスAクラスからもう一つ派生したサービスA2の二つ目のクラスです
    /// </summary>
    public class ServiceA_2_1 : ServiceA_1_0
    {
    }



    /// <summary>
    /// サービスBの基本テストクラスです
    /// </summary>
    public class ServiceBaseB : GameService
    {
    }



    /// <summary>
    /// サービスBの基本クラスから派生したサービスB1クラスです
    /// </summary>
    public class ServiceB_1_0 : ServiceBaseB
    {
    }



    /// <summary>
    /// サービスBクラスから更に派生したサービスB2クラスです
    /// </summary>
    public class ServiceB_2_0 : ServiceB_1_0
    {
    }



    /// <summary>
    /// サービスBクラスからもう一つ派生したサービスB2の二つ目のクラスです
    /// </summary>
    public class ServiceB_2_1 : ServiceB_1_0
    {
    }


    /// <summary>
    /// ServiceBaseAサービスを削除するサービスです
    /// </summary>
    public class ServiceBaseARemoveService : GameService
    {
        /// <summary>
        /// サービスの起動処理を実行します
        /// </summary>
        /// <param name="info">サービスの起動情報を設定します</param>
        protected internal override void Startup(out GameServiceStartupInfo info)
        {
            // サービス起動情報の初期化
            info = new GameServiceStartupInfo()
            {
                // 更新関数テーブルの初期化
                UpdateFunctionTable = new Dictionary<GameServiceUpdateTiming, Action>()
                {
                    {GameServiceUpdateTiming.PreUpdate, PreUpdate}
                }
            };
        }


        /// <summary>
        /// Update前の処理を行います
        /// </summary>
        private void PreUpdate()
        {
            // マネージャを取得
            var manager = GameMain.Current.ServiceManager;


            // サービスが有るなら
            if (manager.Exists<ServiceBaseA>())
            {
                // アクティブを戻して削除する
                manager.SetActiveService<ServiceBaseA>(true);
                manager.RemoveService<ServiceBaseA>();
            }
        }
    }
    #endregion



    /// <summary>
    /// IceMilkTeaの GameServiceManager をテストするクラスです
    /// </summary>
    public class ServiceManagerTest
    {
        // メンバ変数定義
        private GameServiceManager manager;



        /// <summary>
        /// テストの開始準備を行います
        /// </summary>
        [OneTimeSetUp]
        public void Setup()
        {
            // マネージャの起動では例外さえ出なければ良いとする
            Assert.DoesNotThrow(() =>
            {
                // 空ゲームメインで動作を上書きしてサービスマネージャを取得する
                GameMain.OverrideGameMain(ScriptableObject.CreateInstance<EmptyGameMain>());
                manager = GameMain.Current.ServiceManager;
            });
        }


        /// <summary>
        /// テストの終了処理を行います
        /// </summary>
        [OneTimeTearDown]
        public void Celanup()
        {
            // ここでは何もしない
        }


        #region Add系テスト
        /// <summary>
        /// サービスの追加をテストします。
        /// ついでに削除系のテストも行います。
        /// </summary>
        /// <returns>Unityのフレーム待機をするための列挙子を返します</returns>
        [UnityTest, Order(10)]
        public IEnumerator AddServiceTest()
        {
            // サービス追加に成功する関数
            var addSuccess = new Action(() =>
            {
                // そもそもnull渡しは死ぬ
                Assert.Throws<ArgumentNullException>(() => manager.AddService(null));


                // サービスA2_0、サービスB1_0を登録できる事を確認する
                Assert.DoesNotThrow(() => manager.AddService(new ServiceA_2_0()));
                Assert.DoesNotThrow(() => manager.AddService(new ServiceB_1_0()));
            });


            // サービスの追加に失敗する関数
            var addFailed = new Action(() =>
            {
                // サービスA、サービスBのあらゆるサービスが登録出来ないことを確認する（ついでに重複登録出来ないことも確認する）
                Assert.Throws<GameServiceAlreadyExistsException>(() => manager.AddService(new ServiceBaseA()));
                Assert.Throws<GameServiceAlreadyExistsException>(() => manager.AddService(new ServiceA_1_0()));
                Assert.Throws<GameServiceAlreadyExistsException>(() => manager.AddService(new ServiceA_2_0())); // 重複確認
                Assert.Throws<GameServiceAlreadyExistsException>(() => manager.AddService(new ServiceA_2_1()));
                Assert.Throws<GameServiceAlreadyExistsException>(() => manager.AddService(new ServiceBaseB()));
                Assert.Throws<GameServiceAlreadyExistsException>(() => manager.AddService(new ServiceB_1_0())); // 重複確認
                Assert.Throws<GameServiceAlreadyExistsException>(() => manager.AddService(new ServiceB_2_0()));
                Assert.Throws<GameServiceAlreadyExistsException>(() => manager.AddService(new ServiceB_2_1()));
            });


            // サービスのシャットダウン中のため追加に失敗する関数
            var addInvalidOperation = new Action(() =>
            {
                // サービスの削除中の場合Get可能なサービスはInvalidOperationが発生し、関係ないサービスは引き続き追加済み例外が発生することを確認する
                Assert.Throws<InvalidOperationException>(() => manager.AddService(new ServiceBaseA()));
                Assert.Throws<InvalidOperationException>(() => manager.AddService(new ServiceA_1_0()));
                Assert.Throws<InvalidOperationException>(() => manager.AddService(new ServiceA_2_0())); // 重複確認
                Assert.Throws<GameServiceAlreadyExistsException>(() => manager.AddService(new ServiceA_2_1()));
                Assert.Throws<InvalidOperationException>(() => manager.AddService(new ServiceBaseB()));
                Assert.Throws<InvalidOperationException>(() => manager.AddService(new ServiceB_1_0())); // 重複確認
                Assert.Throws<GameServiceAlreadyExistsException>(() => manager.AddService(new ServiceB_2_0()));
                Assert.Throws<GameServiceAlreadyExistsException>(() => manager.AddService(new ServiceB_2_1()));
            });


            // 共通の追加テスト関数を呼ぶ
            return AddTestCommon(addSuccess, addFailed, addInvalidOperation);
        }


        /// <summary>
        /// サービスの追加（TryAddService）をテストします
        /// ついでに削除系のテストも行います。
        /// </summary>
        /// <returns>Unityのフレーム待機をするための列挙子を返します</returns>
        [UnityTest, Order(20)]
        public IEnumerator TryAddServiceTest()
        {
            // サービス追加に成功する関数
            var addSuccess = new Action(() =>
            {
                // そもそもnull渡しは死ぬ
                Assert.Throws<ArgumentNullException>(() => manager.TryAddService(null));


                // サービスA2_0、サービスB1_0を登録できる事を確認する
                Assert.IsTrue(manager.TryAddService(new ServiceA_2_0()));
                Assert.IsTrue(manager.TryAddService(new ServiceB_1_0()));
            });


            // サービスの追加に失敗する関数
            var addFailed = new Action(() =>
            {
                // サービスA、サービスBのあらゆるサービスが登録出来ないことを確認する（ついでに重複登録出来ないことも確認する）
                Assert.IsFalse(manager.TryAddService(new ServiceBaseA()));
                Assert.IsFalse(manager.TryAddService(new ServiceA_1_0()));
                Assert.IsFalse(manager.TryAddService(new ServiceA_2_1()));
                Assert.IsFalse(manager.TryAddService(new ServiceA_2_0())); // 重複確認
                Assert.IsFalse(manager.TryAddService(new ServiceBaseB()));
                Assert.IsFalse(manager.TryAddService(new ServiceB_2_0()));
                Assert.IsFalse(manager.TryAddService(new ServiceB_2_1()));
                Assert.IsFalse(manager.TryAddService(new ServiceB_1_0())); // 重複確認
            });


            // 共通の追加テスト関数を呼ぶ（TryAddは例外を気にしない関数のため、この関数は通常のAddFailedの処理とみなす）
            return AddTestCommon(addSuccess, addFailed, addFailed);
        }



        /// <summary>
        /// AddService系の共通テスト関数です。
        /// </summary>
        /// <param name="addSuccess">AddServiceが成功する関数</param>
        /// <param name="addFailed">AddService全般失敗する関数</param>
        /// <param name="addInvalidOperation">削除中のためGet可能なサービスがInvalidOperationする関数</param>
        /// <returns>Unityのフレーム待機をするための列挙子を返します</returns>
        private IEnumerator AddTestCommon(Action addSuccess, Action addFailed, Action addInvalidOperation)
        {

            // サービスの追加をして、直後に追加に失敗するかを確認する
            addSuccess();
            addFailed();
            yield return null;


            // サービス削除直後は、まだサービスが存在していることを保証しなければならないため
            // 追加関数を叩くと失敗することになるので、その確認をする
            manager.RemoveService<ServiceA_2_0>();
            manager.RemoveService<ServiceB_1_0>();
            addInvalidOperation();
            yield return null;


            // 次のフレームには削除は完了しているので、正常に追加が出来ることを確認する
            addSuccess();
            addFailed();


            // 削除して次のフレームへすすめる
            manager.RemoveService<ServiceA_2_0>();
            manager.RemoveService<ServiceB_1_0>();
            yield return null;
        }
        #endregion


        #region Get系テスト
        /// <summary>
        /// サービスの取得をテストします。
        /// ついでに削除系のテストも行います。
        /// </summary>
        /// <returns>Unityのフレーム待機をするための列挙子を返します</returns>
        [UnityTest, Order(30)]
        public IEnumerator GetServiceTest()
        {
            // サービスAクラスは末端の2系サービスを追加し、GameServiceとサービスA2_1以外でサービスの取得が出来ることを確認
            // サービスBクラスは中間の1系サービスを追加し、GameServiceとサービスB2系以外でサービスの取得が出来ることを確認


            // 全サービス取得が出来ないときの関数
            var allServiceNotFoundTest = new Action(() =>
            {
                // すべての関数にて例外が発生すれば全サービス取得出来ないということになる
                Assert.Throws<GameServiceNotFoundException>(() => manager.GetService<GameService>());
                Assert.Throws<GameServiceNotFoundException>(() => manager.GetService<ServiceBaseA>());
                Assert.Throws<GameServiceNotFoundException>(() => manager.GetService<ServiceA_1_0>());
                Assert.Throws<GameServiceNotFoundException>(() => manager.GetService<ServiceA_2_0>());
                Assert.Throws<GameServiceNotFoundException>(() => manager.GetService<ServiceA_2_1>());
                Assert.Throws<GameServiceNotFoundException>(() => manager.GetService<ServiceBaseB>());
                Assert.Throws<GameServiceNotFoundException>(() => manager.GetService<ServiceB_1_0>());
                Assert.Throws<GameServiceNotFoundException>(() => manager.GetService<ServiceB_2_0>());
                Assert.Throws<GameServiceNotFoundException>(() => manager.GetService<ServiceB_2_1>());
            });


            // サービスの部分取得に成功する場合の関数
            var getServiceTest = new Action(() =>
            {
                // 取得出来ないサービスは例外が発生し、取得できるサービスは例外が発生せず値が取れることでテストが通ることになる
                Assert.Throws<GameServiceNotFoundException>(() => manager.GetService<GameService>()); // 最基底クラスでは取得は出来ない
                Assert.DoesNotThrow(() => manager.GetService<ServiceBaseA>()); // A2_0の継承元
                Assert.IsNotNull(manager.GetService<ServiceBaseA>()); // A2_0の継承元
                Assert.DoesNotThrow(() => manager.GetService<ServiceA_1_0>()); // A2_0の継承元
                Assert.IsNotNull(manager.GetService<ServiceA_1_0>()); // A2_0の継承元
                Assert.DoesNotThrow(() => manager.GetService<ServiceA_2_0>()); // A2_0ご本人様
                Assert.IsNotNull(manager.GetService<ServiceA_2_0>()); // A2_0ご本人様
                Assert.Throws<GameServiceNotFoundException>(() => manager.GetService<ServiceA_2_1>()); // A2_0とは関係のないサービス
                Assert.DoesNotThrow(() => manager.GetService<ServiceBaseB>()); // B1_0の継承元
                Assert.IsNotNull(manager.GetService<ServiceBaseB>()); // B1_0の継承元
                Assert.DoesNotThrow(() => manager.GetService<ServiceB_1_0>()); // B1_0ご本人様
                Assert.IsNotNull(manager.GetService<ServiceB_1_0>()); // B1_0ご本人様
                Assert.Throws<GameServiceNotFoundException>(() => manager.GetService<ServiceB_2_0>()); // B1_0を継承しているがB1_0から見たら関係ないサービス
                Assert.Throws<GameServiceNotFoundException>(() => manager.GetService<ServiceB_2_1>()); // B1_0を継承しているがB1_0から見たら関係ないサービス
            });


            // Get系共通テストの関数を呼ぶ
            return GetTestCommon(allServiceNotFoundTest, getServiceTest);
        }


        /// <summary>
        /// サービスの取得（TryGetService）をテストします。
        /// ついでに削除系のテストも行います。
        /// </summary>
        /// <returns>Unityのフレーム待機をするための列挙子を返します</returns>
        [UnityTest, Order(40)]
        public IEnumerator TryGetServiceTest()
        {
            // TryGetServiceの基本の振る舞いはGetServiceと同じで
            // リザルトが例外ではなく戻り値及び設定される値の確認となる


            // 格納用変数宣言
            var gameService = default(GameService);
            var serviceBaseA = default(ServiceBaseA);
            var serviceA_1_0 = default(ServiceA_1_0);
            var serviceA_2_0 = default(ServiceA_2_0);
            var serviceA_2_1 = default(ServiceA_2_1);
            var serviceBaseB = default(ServiceBaseB);
            var serviceB_1_0 = default(ServiceB_1_0);
            var serviceB_2_0 = default(ServiceB_2_0);
            var serviceB_2_1 = default(ServiceB_2_1);


            // 全サービス取得が出来ないときの関数
            var allServiceNotFoundTest = new Action(() =>
            {
                // すべて false が返されて、値が null に設定されればサービスは全部取得出来ないということになる
                Assert.IsFalse(manager.TryGetService(out gameService)); Assert.IsNull(gameService);
                Assert.IsFalse(manager.TryGetService(out serviceBaseA)); Assert.IsNull(serviceBaseA);
                Assert.IsFalse(manager.TryGetService(out serviceA_1_0)); Assert.IsNull(serviceA_1_0);
                Assert.IsFalse(manager.TryGetService(out serviceA_2_0)); Assert.IsNull(serviceA_2_0);
                Assert.IsFalse(manager.TryGetService(out serviceA_2_1)); Assert.IsNull(serviceA_2_1);
                Assert.IsFalse(manager.TryGetService(out serviceBaseB)); Assert.IsNull(serviceBaseB);
                Assert.IsFalse(manager.TryGetService(out serviceB_1_0)); Assert.IsNull(serviceB_1_0);
                Assert.IsFalse(manager.TryGetService(out serviceB_2_0)); Assert.IsNull(serviceB_2_0);
                Assert.IsFalse(manager.TryGetService(out serviceB_2_1)); Assert.IsNull(serviceB_2_1);
            });


            // サービスの部分取得に成功する場合の関数
            var getServiceTest = new Action(() =>
            {
                // 取得できる場合は true が設定されて、値がセットされていればOK
                Assert.IsFalse(manager.TryGetService(out gameService)); Assert.IsNull(gameService); // 最基底クラスでは取得は出来ない
                Assert.IsTrue(manager.TryGetService(out serviceBaseA)); Assert.IsNotNull(serviceBaseA); // A2_0の継承元
                Assert.IsTrue(manager.TryGetService(out serviceA_1_0)); Assert.IsNotNull(serviceA_1_0); // A2_0の継承元
                Assert.IsTrue(manager.TryGetService(out serviceA_2_0)); Assert.IsNotNull(serviceA_2_0); // A2_0ご本人様
                Assert.IsFalse(manager.TryGetService(out serviceA_2_1)); Assert.IsNull(serviceA_2_1); // A2_0とは関係のないサービス
                Assert.IsTrue(manager.TryGetService(out serviceBaseB)); Assert.IsNotNull(serviceBaseB); // B1_0の継承元
                Assert.IsTrue(manager.TryGetService(out serviceB_1_0)); Assert.IsNotNull(serviceB_1_0); // B1_0ご本人様
                Assert.IsFalse(manager.TryGetService(out serviceB_2_0)); Assert.IsNull(serviceB_2_0); // B1_0を継承しているがB1_0から見たら関係ないサービス
                Assert.IsFalse(manager.TryGetService(out serviceB_2_1)); Assert.IsNull(serviceB_2_1); // B1_0を継承しているがB1_0から見たら関係ないサービス
            });


            // Get系共通テストの関数を呼ぶ
            return GetTestCommon(allServiceNotFoundTest, getServiceTest);
        }


        /// <summary>
        /// GetService系の共通テスト関数です。
        /// </summary>
        /// <param name="allServiceNotFoundTest">すべてのサービスの取得が出来ないときの関数</param>
        /// <param name="getServiceTest">部分的に取得に成功する関数</param>
        /// <returns>Unityのフレーム待機をするための列挙子を返します</returns>
        private IEnumerator GetTestCommon(Action allServiceNotFoundTest, Action getServiceTest)
        {
            // この時点ではまだ全サービスの取得が出来ないことを確認する
            allServiceNotFoundTest();


            // 通常は追加された直後のサービスは起動していないが、取得は可能であることは保証しているべきなので
            // サービスA2_0、サービスB1_0の取得系がちゃんと動作するか確認して、次のフレームでも同じ動作をしていることを期待する
            manager.AddService(new ServiceA_2_0());
            manager.AddService(new ServiceB_1_0());
            getServiceTest();
            yield return null;
            getServiceTest();


            // サービスのアクティブを切ってもサービスの取得ができることを確認する
            manager.SetActiveService<ServiceA_2_0>(false);
            manager.SetActiveService<ServiceB_1_0>(false);
            getServiceTest();


            // 通常は削除された直後のサービスはまだ停止していないので、まだ取得できる状態を保証しているべきで
            // まだサービスが取得出来ることを確認して、次のフレームで取得ができなくなっていることを確認する
            manager.RemoveService<ServiceA_2_0>();
            manager.RemoveService<ServiceB_1_0>();
            getServiceTest();
            yield return null;
            allServiceNotFoundTest();
        }
        #endregion


        #region サービスのコントロール系テスト
        /// <summary>
        /// サービスのアクティブ設定をのテストをします
        /// </summary>
        /// <returns>Unityのフレーム待機をするための列挙子を返します</returns>
        [UnityTest, Order(50)]
        public IEnumerator ActiveDeactiveTest()
        {
            // Active操作はフレーム進行関係なくアクティブ状態は直ちに反映されるかを確認する


            // 確認用サービスを生成して登録
            var service = new ServiceBaseA();
            manager.AddService(service);


            // この時点ではカウンタは0であることと、まだ起動していないこと、アクティブ状態であることを確認してフレームを進める
            Assert.AreEqual(0, service.StepCount);
            Assert.IsFalse(service.Startuped);
            Assert.IsTrue(manager.IsActiveService<ServiceBaseA>());
            yield return null;


            // フレームが進んだのでカウンタが進んだことを確認して、もう起動済みで、アクティブの切り替わりの確認をして、フレームを進める
            Assert.AreEqual(1, service.StepCount);
            Assert.IsTrue(service.Startuped);
            Assert.IsTrue(manager.IsActiveService<ServiceBaseA>());
            manager.SetActiveService<ServiceBaseA>(false);
            Assert.IsFalse(manager.IsActiveService<ServiceBaseA>());
            yield return null;


            // 非アクティブ状態なのでカウンタが進んでいないことを確認して、アクティブ状態にして、アクティブ変化を確認後
            // 削除後に非アクティブになることを確認してフレームを進める
            Assert.AreEqual(1, service.StepCount);
            manager.SetActiveService<ServiceBaseA>(true);
            Assert.IsTrue(manager.IsActiveService<ServiceBaseA>());
            manager.RemoveService<ServiceBaseA>();
            Assert.IsFalse(manager.IsActiveService<ServiceBaseA>());
            yield return null;


            // サービスは生成直後にアクティブを切ると起動処理すら呼ばれないことを確認する
            service = new ServiceBaseA();
            manager.AddService(service);
            Assert.AreEqual(0, service.StepCount);
            Assert.IsFalse(service.Startuped);
            Assert.IsTrue(manager.IsActiveService<ServiceBaseA>());
            manager.SetActiveService<ServiceBaseA>(false);
            yield return null;
            Assert.AreEqual(0, service.StepCount);
            Assert.IsFalse(service.Startuped);
            Assert.IsFalse(manager.IsActiveService<ServiceBaseA>());


            // サービスを削除してフレームを進める
            manager.RemoveService<ServiceBaseA>();
            yield return null;
        }


        /// <summary>
        /// サービスの起動停止の処理が想定タイミングで行われるかをテストします
        /// </summary>
        /// <returns>Unityのフレーム待機をするための列挙子を返します</returns>
        [UnityTest, Order(60)]
        public IEnumerator StartupAndShutdownTest()
        {
            // サービスの起動と停止はフレームの開始や終了のタイミングのため
            // 生成直後や削除要求直後はまだステータス変化が起きていないことを確認する


            // サービスを生成、登録後にまだ起動処理が呼び出されていないことを確認して、フレームを進める
            var service = new ServiceBaseA();
            manager.AddService(service);
            Assert.IsFalse(service.Startuped);
            yield return null;


            // サービスが起動したことを確認して、削除要求後にまだシャットダウンされていない事を確認して、フレームを進める
            Assert.IsTrue(service.Startuped);
            manager.RemoveService<ServiceBaseA>();
            Assert.IsFalse(service.Stopped);
            yield return null;


            // サービスが停止したことを確認する
            Assert.IsTrue(service.Stopped);


            // 新たにサービスを生成して追加後にアクティブを切ったら、フレームが進んでも起動しないことを確認する
            service = new ServiceBaseA();
            manager.AddService(service);
            manager.SetActiveService<ServiceBaseA>(false);
            Assert.IsFalse(service.Startuped);
            yield return null;
            Assert.IsFalse(service.Startuped);


            // 寝たままの状態で削除されてもシャットダウンされないことを確認する
            manager.RemoveService<ServiceBaseA>();
            yield return null;
            Assert.IsFalse(service.Stopped);


            // 新たにサービスを生成してアクティブを切った後、フレーム更新中にアクティブを戻して削除してもシャットダウンが呼ばれないことを確認する
            service = new ServiceBaseA();
            manager.AddService(service);
            manager.SetActiveService<ServiceBaseA>(false);
            yield return null;
            manager.AddService(new ServiceBaseARemoveService());
            yield return null;
            Assert.IsFalse(service.Stopped);
            manager.RemoveService<ServiceBaseARemoveService>();
            yield return null;
        }


        /// <summary>
        /// サービスの存在チェックが正しく行われるかをテストします。
        /// </summary>
        /// <returns>Unityのフレーム待機をするための列挙子を返します</returns>
        [UnityTest, Order(70)]
        public IEnumerator ExistsTest()
        {
            // サービス全確認ができない関数
            var allNotExists = new Action(() =>
            {
                // すべてのサービスの存在を確認できない場合の処理
                Assert.IsFalse(manager.Exists<GameService>()); // 最基底クラスはそもそも検索や存在確認で利用できない
                Assert.IsFalse(manager.Exists<ServiceBaseA>());
                Assert.IsFalse(manager.Exists<ServiceA_1_0>());
                Assert.IsFalse(manager.Exists<ServiceA_2_0>());
                Assert.IsFalse(manager.Exists<ServiceA_2_1>());
                Assert.IsFalse(manager.Exists<ServiceBaseB>());
                Assert.IsFalse(manager.Exists<ServiceB_1_0>());
                Assert.IsFalse(manager.Exists<ServiceB_2_0>());
                Assert.IsFalse(manager.Exists<ServiceB_2_1>());

            });


            // サービスの部分確認が出来る関数
            var existsPass = new Action(() =>
            {
                // 部分的なサービス存在が可能な場合の処理
                Assert.IsFalse(manager.Exists<GameService>()); // 最基底クラスはそもそも検索や存在確認で利用できない
                Assert.IsTrue(manager.Exists<ServiceBaseA>()); // A2_0の継承元
                Assert.IsTrue(manager.Exists<ServiceA_1_0>()); // A2_0の継承元
                Assert.IsTrue(manager.Exists<ServiceA_2_0>()); // A2_0ご本人様
                Assert.IsFalse(manager.Exists<ServiceA_2_1>()); // A2_0とは関係のないサービス
                Assert.IsTrue(manager.Exists<ServiceBaseB>()); // B1_0の継承元
                Assert.IsTrue(manager.Exists<ServiceB_1_0>()); // B1_0ご本人様
                Assert.IsFalse(manager.Exists<ServiceB_2_0>()); // B1_0を継承しているがB1_0から見たら関係ないサービス
                Assert.IsFalse(manager.Exists<ServiceB_2_1>()); // B1_0を継承しているがB1_0から見たら関係ないサービス
            });


            // 現時点では全サービスは存在していないはず
            allNotExists();


            // サービスを追加すると直ちに存在確認が出来る事を確認して、フレームを進める
            manager.AddService(new ServiceA_2_0());
            manager.AddService(new ServiceB_1_0());
            existsPass();
            yield return null;


            // アクティブを切っても存在チェックは通る事を確認する
            manager.SetActiveService<ServiceA_2_0>(false);
            manager.SetActiveService<ServiceB_1_0>(false);
            existsPass();


            // 削除対象になってもそのフレームの間は存在チェックが通ることを確認して、フレームを進める
            manager.RemoveService<ServiceB_2_0>();
            manager.RemoveService<ServiceB_1_0>();
            existsPass();
            yield return null;


            // すべて削除されたはずなら、全サービスは存在していないはず
            allNotExists();
        }
        #endregion
    }
}