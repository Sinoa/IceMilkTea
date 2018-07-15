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
using IceMilkTea.Core;
using NUnit.Framework;
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
                // マネージャを生成して初期化を呼ぶ
                manager = new GameServiceManager();
                manager.Startup();
            });
        }


        /// <summary>
        /// テストの終了処理を行います
        /// </summary>
        [OneTimeTearDown]
        public void Celanup()
        {
            // マネージャの停止では例外さえ出なければ良いとする
            Assert.DoesNotThrow(() =>
            {
                // マネージャを停止する
                manager.Shutdown();
            });
        }


        /// <summary>
        /// サービスの追加をテストします
        /// </summary>
        /// <returns>Unityのフレーム待機をするための列挙子を返します</returns>
        [UnityTest, Order(10)]
        public IEnumerator AddServiceTest()
        {
            // サービスAクラスは末端の2系サービスを追加し、別のクラスが登録出来ないことを確認
            // サービスBクラスは中間の1系サービスを追加し、基本クラス及び末端クラスの登録が出来ない事を確認


            // サービスA2_0、サービスB1_0を登録できる事を確認する
            Assert.DoesNotThrow(() => manager.AddService(new ServiceA_2_0()));
            Assert.DoesNotThrow(() => manager.AddService(new ServiceB_1_0()));


            // サービスAのあらゆるサービスが登録出来ないことを確認する（ついでに重複登録出来ないことも確認する）
            Assert.Throws<GameServiceAlreadyExistsException>(() => manager.AddService(new ServiceBaseA()));
            Assert.Throws<GameServiceAlreadyExistsException>(() => manager.AddService(new ServiceA_1_0()));
            Assert.Throws<GameServiceAlreadyExistsException>(() => manager.AddService(new ServiceA_2_1()));
            Assert.Throws<GameServiceAlreadyExistsException>(() => manager.AddService(new ServiceA_2_0())); // 重複確認


            // サービスBのあらゆるサービスが登録出来ないことを確認する（ついでに重複登録出来ないことも確認する）
            Assert.Throws<GameServiceAlreadyExistsException>(() => manager.AddService(new ServiceBaseB()));
            Assert.Throws<GameServiceAlreadyExistsException>(() => manager.AddService(new ServiceB_2_0()));
            Assert.Throws<GameServiceAlreadyExistsException>(() => manager.AddService(new ServiceB_2_1()));
            Assert.Throws<GameServiceAlreadyExistsException>(() => manager.AddService(new ServiceB_1_0())); // 重複確認


            // フレームを進める
            yield return null;


            // 後続テストのためにサービスを削除する
            manager.RemoveService<ServiceA_2_0>();
            manager.RemoveService<ServiceB_1_0>();


            // フレームを進める
            yield return null;
        }


        /// <summary>
        /// サービスの追加（TryAddService）をテストします
        /// </summary>
        /// <returns>Unityのフレーム待機をするための列挙子を返します</returns>
        [UnityTest, Order(20)]
        public IEnumerator TryAddServiceTest()
        {
            // 今は失敗するように振る舞う
            Assert.Fail();
            yield break;
        }


        /// <summary>
        /// サービスの取得をテストします
        /// </summary>
        /// <returns>Unityのフレーム待機をするための列挙子を返します</returns>
        [UnityTest, Order(30)]
        public IEnumerator GetServiceTest()
        {
            // 今は失敗するように振る舞う
            Assert.Fail();
            yield break;
        }


        /// <summary>
        /// サービスの取得（TryGetService）をテストします
        /// </summary>
        /// <returns>Unityのフレーム待機をするための列挙子を返します</returns>
        [UnityTest, Order(40)]
        public IEnumerator TryGetServiceTest()
        {
            // 今は失敗するように振る舞う
            Assert.Fail();
            yield break;
        }


        /// <summary>
        /// サービスの削除をテストします
        /// </summary>
        /// <returns>Unityのフレーム待機をするための列挙子を返します</returns>
        [UnityTest, Order(50)]
        public IEnumerator RemoveServiceTest()
        {
            // 今は失敗するように振る舞う
            Assert.Fail();
            yield break;
        }


        /// <summary>
        /// サービスのアクティブ設定をのテストをします
        /// </summary>
        /// <returns>Unityのフレーム待機をするための列挙子を返します</returns>
        [UnityTest, Order(60)]
        public IEnumerator ActiveDeactiveTest()
        {
            // 今は失敗するように振る舞う
            Assert.Fail();
            yield break;
        }


        /// <summary>
        /// サービスの起動停止の処理が想定タイミングで行われるかをテストします
        /// </summary>
        /// <returns>Unityのフレーム待機をするための列挙子を返します</returns>
        public IEnumerator StartupAndShutdownTest()
        {
            // 今は失敗するように振る舞う
            Assert.Fail();
            yield break;
        }
    }
}