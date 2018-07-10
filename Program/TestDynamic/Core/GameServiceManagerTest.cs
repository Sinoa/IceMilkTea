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

using IceMilkTea.Core;
using NUnit.Framework;

namespace IceMilkTeaTestDynamic.Core
{
    /// <summary>
    /// テストで用いるサービスAクラスです
    /// </summary>
    public class TestServiceA : GameService
    {
    }



    /// <summary>
    /// テストで用いるサービスBクラスです
    /// </summary>
    public class TestServiceB : GameService
    {
    }



    /// <summary>
    /// テストで用いるサービスAから派生した派生サービスAクラスです
    /// </summary>
    public class DerivedServiceA : TestServiceA
    {
    }



    /// <summary>
    /// テストで用いるサービスBから派生した派生サービスBクラスです
    /// </summary>
    public class DerivedServiceB : TestServiceB
    {
    }



    /// <summary>
    /// IceMilkTeaの InternalGameServiceManager をテストするクラスです
    /// </summary>
    public class ServiceManagerTest
    {
        /// <summary>
        /// サービスの追加をテストします
        /// </summary>
        [Test]
        public void AddServiceTest()
        {
            // サービスマネージャのインスタンスを生成する
            var manager = new InternalGameServiceManager();


            // 各種サービスのインスタンスを生成
            var serviceA = new TestServiceA();
            var serviceB = new TestServiceB();
            var derivedA = new DerivedServiceA();
            var derivedB = new DerivedServiceB();


            // 全サービスの正常な追加をする（ここで例外が出るようではダメ）（Aは基本 -> 派生、Bは派生 -> 基本 の順で追加）
            Assert.DoesNotThrow(() => manager.AddService(serviceA));
            Assert.DoesNotThrow(() => manager.AddService(derivedA));
            Assert.DoesNotThrow(() => manager.AddService(derivedB));
            Assert.DoesNotThrow(() => manager.AddService(serviceB));


            // 同じインスタンスの追加をしようとしてちゃんと例外を投げてくれるかを調べる
            Assert.Throws<GameServiceAlreadyExistsException>(() => manager.AddService(serviceA));
            Assert.Throws<GameServiceAlreadyExistsException>(() => manager.AddService(derivedA));
            Assert.Throws<GameServiceAlreadyExistsException>(() => manager.AddService(derivedB));
            Assert.Throws<GameServiceAlreadyExistsException>(() => manager.AddService(serviceB));


            // サービスの新しいインスタンスの生成し直し（参照が変わる）
            serviceA = new TestServiceA();
            serviceB = new TestServiceB();
            derivedA = new DerivedServiceA();
            derivedB = new DerivedServiceB();


            // インスタンスが変わっても同じ型のサービスの追加をしようとしたら、ちゃんと例外を投げてくれるかを調べる
            Assert.Throws<GameServiceAlreadyExistsException>(() => manager.AddService(serviceA));
            Assert.Throws<GameServiceAlreadyExistsException>(() => manager.AddService(derivedA));
            Assert.Throws<GameServiceAlreadyExistsException>(() => manager.AddService(derivedB));
            Assert.Throws<GameServiceAlreadyExistsException>(() => manager.AddService(serviceB));
        }


        /// <summary>
        /// サービスの追加（TryAddService）をテストします
        /// </summary>
        [Test]
        public void TryAddServiceTest()
        {
            // サービスマネージャのインスタンスを生成する
            var manager = new InternalGameServiceManager();


            // 各種サービスのインスタンスを生成
            var serviceA = new TestServiceA();
            var serviceB = new TestServiceB();
            var derivedA = new DerivedServiceA();
            var derivedB = new DerivedServiceB();


            // 全サービスの正常な追加をする（Aは基本 -> 派生、Bは派生 -> 基本 の順で追加）
            Assert.AreEqual(manager.TryAddService(serviceA), true);
            Assert.AreEqual(manager.TryAddService(derivedA), true);
            Assert.AreEqual(manager.TryAddService(derivedB), true);
            Assert.AreEqual(manager.TryAddService(serviceB), true);


            // 同じインスタンスのTry追加をしようとしてちゃんと失敗を返してくれるかを調べる
            Assert.AreEqual(manager.TryAddService(serviceA), false);
            Assert.AreEqual(manager.TryAddService(derivedA), false);
            Assert.AreEqual(manager.TryAddService(derivedB), false);
            Assert.AreEqual(manager.TryAddService(serviceB), false);


            // サービスの新しいインスタンスの生成し直し（参照が変わる）
            serviceA = new TestServiceA();
            serviceB = new TestServiceB();
            derivedA = new DerivedServiceA();
            derivedB = new DerivedServiceB();


            // インスタンスが変わっても同じ型のサービスの追加をしようとしたら、失敗を返してくれるかを調べる
            Assert.AreEqual(manager.TryAddService(serviceA), false);
            Assert.AreEqual(manager.TryAddService(derivedA), false);
            Assert.AreEqual(manager.TryAddService(derivedB), false);
            Assert.AreEqual(manager.TryAddService(serviceB), false);
        }


        /// <summary>
        /// サービスの取得をテストします
        /// </summary>
        [Test]
        public void GetServiceTest()
        {
            // サービスマネージャのインスタンスを生成する
            var manager = new InternalGameServiceManager();


            // 今の段階でサービスの取得を試みて、ちゃんと例外を吐いてくれるかを調べる
            Assert.Throws<GameServiceNotFoundException>(() => manager.GetService<GameService>());
            Assert.Throws<GameServiceNotFoundException>(() => manager.GetService<TestServiceA>());
            Assert.Throws<GameServiceNotFoundException>(() => manager.GetService<TestServiceB>());
            Assert.Throws<GameServiceNotFoundException>(() => manager.GetService<DerivedServiceA>());
            Assert.Throws<GameServiceNotFoundException>(() => manager.GetService<DerivedServiceB>());


            // 各種サービスのインスタンスを生成
            var serviceA = new TestServiceA();
            var serviceB = new TestServiceB();
            var derivedA = new DerivedServiceA();
            var derivedB = new DerivedServiceB();


            // 派生サービスA、基本サービスBを追加して、それ以外はちゃんと例外を吐いてくれるかを調べる
            manager.AddService(derivedA);
            manager.AddService(serviceB);
            Assert.Throws<GameServiceNotFoundException>(() => manager.GetService<GameService>());
            Assert.Throws<GameServiceNotFoundException>(() => manager.GetService<TestServiceA>());
            Assert.DoesNotThrow(() => manager.GetService<TestServiceB>());
            Assert.DoesNotThrow(() => manager.GetService<DerivedServiceA>());
            Assert.Throws<GameServiceNotFoundException>(() => manager.GetService<DerivedServiceB>());


            // 残りのサービスも追加してServiceProviderの基本型のみだけ例外を吐いてくれるかを調べる
            manager.AddService(serviceA);
            manager.AddService(derivedB);
            Assert.Throws<GameServiceNotFoundException>(() => manager.GetService<GameService>());
            Assert.DoesNotThrow(() => manager.GetService<TestServiceA>());
            Assert.DoesNotThrow(() => manager.GetService<TestServiceA>());
            Assert.DoesNotThrow(() => manager.GetService<DerivedServiceA>());
            Assert.DoesNotThrow(() => manager.GetService<DerivedServiceB>());
        }


        /// <summary>
        /// サービスの取得（TryGetService）をテストします
        /// </summary>
        [Test]
        public void TryGetServiceTest()
        {
            // サービスマネージャのインスタンスを生成する
            var manager = new InternalGameServiceManager();


            // 各種サービスの結果受け取り変数の用意
            var baseService = default(GameService);
            var serviceA = default(TestServiceA);
            var serviceB = default(TestServiceB);
            var derivedA = default(DerivedServiceA);
            var derivedB = default(DerivedServiceB);


            // 今の段階でサービスの取得を試みて、ちゃんと失敗してくれるか調べる（戻り値の失敗と設定される変数にnullが入るのかの2パターン）
            Assert.AreEqual(manager.TryGetService(out baseService), false);
            Assert.AreEqual(manager.TryGetService(out serviceA), false);
            Assert.AreEqual(manager.TryGetService(out serviceB), false);
            Assert.AreEqual(manager.TryGetService(out derivedA), false);
            Assert.AreEqual(manager.TryGetService(out derivedB), false);
            Assert.IsNull(baseService);
            Assert.IsNull(serviceA);
            Assert.IsNull(serviceB);
            Assert.IsNull(derivedA);
            Assert.IsNull(derivedB);


            // 派生サービスA、基本サービスBを追加して、それ以外はちゃんと失敗してくれるかを調べる
            manager.AddService(new DerivedServiceA());
            manager.AddService(new TestServiceB());
            Assert.AreEqual(manager.TryGetService(out baseService), false);
            Assert.AreEqual(manager.TryGetService(out serviceA), false);
            Assert.AreEqual(manager.TryGetService(out serviceB), true);
            Assert.AreEqual(manager.TryGetService(out derivedA), true);
            Assert.AreEqual(manager.TryGetService(out derivedB), false);
            Assert.IsNull(baseService);
            Assert.IsNull(serviceA);
            Assert.IsNotNull(serviceB);
            Assert.IsNotNull(derivedA);
            Assert.IsNull(derivedB);


            // 残りのサービスも追加してServiceProviderの基本型のみだけ失敗してくれるかを調べる
            manager.AddService(new TestServiceA());
            manager.AddService(new DerivedServiceB());
            Assert.AreEqual(manager.TryGetService(out baseService), false);
            Assert.AreEqual(manager.TryGetService(out serviceA), true);
            Assert.AreEqual(manager.TryGetService(out serviceB), true);
            Assert.AreEqual(manager.TryGetService(out derivedA), true);
            Assert.AreEqual(manager.TryGetService(out derivedB), true);
            Assert.IsNull(baseService);
            Assert.IsNotNull(serviceA);
            Assert.IsNotNull(serviceB);
            Assert.IsNotNull(derivedA);
            Assert.IsNotNull(derivedB);
        }


        /// <summary>
        /// サービスの削除をテストします
        /// </summary>
        [Test]
        public void RemoveServiceTest()
        {
            // サービスマネージャのインスタンスを生成する
            var manager = new InternalGameServiceManager();


            // 各種サービスのインスタンスを生成して追加する
            manager.AddService(new TestServiceA());
            manager.AddService(new TestServiceB());
            manager.AddService(new DerivedServiceA());
            manager.AddService(new DerivedServiceB());


            // 最基底クラスであるServiceProviderで削除を要求してどのサービスも死んでいないことを確認する
            manager.RemoveService<GameService>();
            Assert.DoesNotThrow(() => manager.GetService<TestServiceA>());
            Assert.DoesNotThrow(() => manager.GetService<TestServiceB>());
            Assert.DoesNotThrow(() => manager.GetService<DerivedServiceA>());
            Assert.DoesNotThrow(() => manager.GetService<DerivedServiceB>());


            // 派生サービスA、基本サービスB、を削除して基本サービスA、派生サービスBが生きていることを確認する
            manager.RemoveService<DerivedServiceA>();
            manager.RemoveService<TestServiceB>();
            Assert.DoesNotThrow(() => manager.GetService<TestServiceA>());
            Assert.Throws<GameServiceNotFoundException>(() => manager.GetService<TestServiceB>());
            Assert.Throws<GameServiceNotFoundException>(() => manager.GetService<DerivedServiceA>());
            Assert.DoesNotThrow(() => manager.GetService<DerivedServiceB>());


            // 残りのサービスも全員削除して、全て死んでいることを確認する
            manager.RemoveService<TestServiceA>();
            manager.RemoveService<DerivedServiceB>();
            Assert.Throws<GameServiceNotFoundException>(() => manager.GetService<TestServiceA>());
            Assert.Throws<GameServiceNotFoundException>(() => manager.GetService<TestServiceB>());
            Assert.Throws<GameServiceNotFoundException>(() => manager.GetService<DerivedServiceA>());
            Assert.Throws<GameServiceNotFoundException>(() => manager.GetService<DerivedServiceB>());
        }


        /// <summary>
        /// サービスの全削除をテストします
        /// </summary>
        [Test]
        public void RemoveAllServiceTest()
        {
            // サービスマネージャのインスタンスを生成する
            var manager = new InternalGameServiceManager();


            // 各種サービスのインスタンスを生成して追加する
            manager.AddService(new TestServiceA());
            manager.AddService(new TestServiceB());
            manager.AddService(new DerivedServiceA());
            manager.AddService(new DerivedServiceB());


            // とりあえずどのサービスも死んでいないことを確認する
            Assert.DoesNotThrow(() => manager.GetService<TestServiceA>());
            Assert.DoesNotThrow(() => manager.GetService<TestServiceB>());
            Assert.DoesNotThrow(() => manager.GetService<DerivedServiceA>());
            Assert.DoesNotThrow(() => manager.GetService<DerivedServiceB>());


            // 全サービス削除をして、全て死んでいることを確認する
            manager.RemoveAllService();
            Assert.Throws<GameServiceNotFoundException>(() => manager.GetService<TestServiceA>());
            Assert.Throws<GameServiceNotFoundException>(() => manager.GetService<TestServiceB>());
            Assert.Throws<GameServiceNotFoundException>(() => manager.GetService<DerivedServiceA>());
            Assert.Throws<GameServiceNotFoundException>(() => manager.GetService<DerivedServiceB>());
        }
    }
}