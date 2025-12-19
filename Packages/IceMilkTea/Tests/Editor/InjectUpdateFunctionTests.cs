// Zlib License
//
// Copyright (c) 2025 Sinoa
//
// This software is provided 'as-is', without any express or implied
// warranty. In no event will the authors be held liable for any damages
// arising from the use of this software.
//
// Permission is granted to anyone to use this software for any purpose,
// including commercial applications, and to alter it and redistribute it
// freely, subject to the following restrictions:
//
// 1. The origin of this software must not be misrepresented; you must not
// claim that you wrote the original software. If you use this software
// in a product, an acknowledgment in the product documentation would be
// appreciated but is not required.
//
// 2. Altered source versions must be plainly marked as such, and must not be
// misrepresented as being the original software.
//
// 3. This notice may not be removed or altered from any source
// distribution.

using System;
using Foxtamp.IceMilkTea.Utilities;
using NUnit.Framework;

namespace Foxtamp.IceMilkTea.Tests
{
    /// <summary>
    /// PlayerLoopSystemBuilder.InjectUpdateFunction メソッドのテストクラスです
    /// </summary>
    [TestFixture]
    public class InjectUpdateFunctionTests
    {
        /// <summary>
        /// 基準型の前に更新関数を注入できることを検証します
        /// </summary>
        [Test]
        public void InjectUpdateFunction_BeforePivot_InsertsAtCorrectIndex()
        {
            // Arrange
            var mockSystem = PlayerLoopSystemMockFactory.CreateMultipleChildren<DummySystemA, DummySystemB, DummySystemC, DummySystemD>();
            var builder = new PlayerLoopSystemBuilder(mockSystem);

            // Act
            var result = builder.InjectUpdateFunction(DummyInjectionTarget.UpdateFunction, typeof(DummySystemC), before: true);
            var builtSystem = builder.Build();

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(4, builtSystem.subSystemList.Length);
            Assert.AreEqual(typeof(DummySystemB), builtSystem.subSystemList[0].type);
            Assert.AreEqual(typeof(DummyInjectionTarget), builtSystem.subSystemList[1].type);
            Assert.AreEqual(typeof(DummySystemC), builtSystem.subSystemList[2].type);
            Assert.AreEqual(typeof(DummySystemD), builtSystem.subSystemList[3].type);
        }

        /// <summary>
        /// 基準型の後ろに更新関数を注入できることを検証します
        /// </summary>
        [Test]
        public void InjectUpdateFunction_AfterPivot_InsertsAtCorrectIndex()
        {
            // Arrange
            var mockSystem = PlayerLoopSystemMockFactory.CreateMultipleChildren<DummySystemA, DummySystemB, DummySystemC, DummySystemD>();
            var builder = new PlayerLoopSystemBuilder(mockSystem);

            // Act
            var result = builder.InjectUpdateFunction(DummyInjectionTarget.UpdateFunction, typeof(DummySystemC), before: false);
            var builtSystem = builder.Build();

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(4, builtSystem.subSystemList.Length);
            Assert.AreEqual(typeof(DummySystemB), builtSystem.subSystemList[0].type);
            Assert.AreEqual(typeof(DummySystemC), builtSystem.subSystemList[1].type);
            Assert.AreEqual(typeof(DummyInjectionTarget), builtSystem.subSystemList[2].type);
            Assert.AreEqual(typeof(DummySystemD), builtSystem.subSystemList[3].type);
        }

        /// <summary>
        /// 配列の先頭に注入できることを検証します（最初の要素の前）
        /// </summary>
        [Test]
        public void InjectUpdateFunction_BeforeFirstElement_InsertsAtIndex0()
        {
            // Arrange
            var mockSystem = PlayerLoopSystemMockFactory.CreateMultipleChildren<DummySystemA, DummySystemB, DummySystemC, DummySystemD>();
            var builder = new PlayerLoopSystemBuilder(mockSystem);

            // Act
            var result = builder.InjectUpdateFunction(DummyInjectionTarget.UpdateFunction, typeof(DummySystemB), before: true);
            var builtSystem = builder.Build();

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(4, builtSystem.subSystemList.Length);
            Assert.AreEqual(typeof(DummyInjectionTarget), builtSystem.subSystemList[0].type);
            Assert.AreEqual(typeof(DummySystemB), builtSystem.subSystemList[1].type);
        }

        /// <summary>
        /// 配列の末尾に注入できることを検証します（最後の要素の後ろ）
        /// </summary>
        [Test]
        public void InjectUpdateFunction_AfterLastElement_InsertsAtEnd()
        {
            // Arrange
            var mockSystem = PlayerLoopSystemMockFactory.CreateMultipleChildren<DummySystemA, DummySystemB, DummySystemC, DummySystemD>();
            var builder = new PlayerLoopSystemBuilder(mockSystem);

            // Act
            var result = builder.InjectUpdateFunction(DummyInjectionTarget.UpdateFunction, typeof(DummySystemD), before: false);
            var builtSystem = builder.Build();

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(4, builtSystem.subSystemList.Length);
            Assert.AreEqual(typeof(DummySystemD), builtSystem.subSystemList[2].type);
            Assert.AreEqual(typeof(DummyInjectionTarget), builtSystem.subSystemList[3].type);
        }

        /// <summary>
        /// 存在しない基準型を指定した場合に false を返すことを検証します
        /// </summary>
        [Test]
        public void InjectUpdateFunction_PivotNotFound_ReturnsFalse()
        {
            // Arrange
            var mockSystem = PlayerLoopSystemMockFactory.CreateSingleLevel<DummySystemA>();
            var builder = new PlayerLoopSystemBuilder(mockSystem);

            // Act
            var result = builder.InjectUpdateFunction(DummyInjectionTarget.UpdateFunction, typeof(DummyNonExistent), before: true);

            // Assert
            Assert.IsFalse(result);
        }

        /// <summary>
        /// 深いネスト構造に注入できることを検証します
        /// </summary>
        [Test]
        public void InjectUpdateFunction_DeepNestedStructure_InsertsCorrectly()
        {
            // Arrange
            var mockSystem = PlayerLoopSystemMockFactory.CreateNestedThreeLevel<DummySystemA, DummySystemB, DummySystemC>();
            var builder = new PlayerLoopSystemBuilder(mockSystem);

            // Act
            var result = builder.InjectUpdateFunction(DummyInjectionTarget.UpdateFunction, typeof(DummySystemC), before: true);
            var builtSystem = builder.Build();

            // Assert
            Assert.IsTrue(result);
            var childSystem = builtSystem.subSystemList[0];
            Assert.AreEqual(2, childSystem.subSystemList.Length);
            Assert.AreEqual(typeof(DummyInjectionTarget), childSystem.subSystemList[0].type);
            Assert.AreEqual(typeof(DummySystemC), childSystem.subSystemList[1].type);
        }

        /// <summary>
        /// null の更新関数を渡した場合に ArgumentNullException がスローされることを検証します
        /// </summary>
        [Test]
        public void InjectUpdateFunction_NullFunction_ThrowsArgumentNullException()
        {
            // Arrange
            var mockSystem = PlayerLoopSystemMockFactory.CreateSingleLevel<DummySystemA>();
            var builder = new PlayerLoopSystemBuilder(mockSystem);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => builder.InjectUpdateFunction(null, typeof(DummySystemA), true));
        }

        /// <summary>
        /// null の基準型を渡した場合に ArgumentNullException がスローされることを検証します
        /// </summary>
        [Test]
        public void InjectUpdateFunction_NullPivotType_ThrowsArgumentNullException()
        {
            // Arrange
            var mockSystem = PlayerLoopSystemMockFactory.CreateSingleLevel<DummySystemA>();
            var builder = new PlayerLoopSystemBuilder(mockSystem);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => builder.InjectUpdateFunction(DummyInjectionTarget.UpdateFunction, null, true));
        }

        /// <summary>
        /// 注入された更新関数の updateDelegate が正しく設定されていることを検証します
        /// </summary>
        [Test]
        public void InjectUpdateFunction_UpdateDelegateIsSet_ReturnsCorrectDelegate()
        {
            // Arrange
            var mockSystem = PlayerLoopSystemMockFactory.CreateTwoLevel<DummySystemA, DummySystemB>();
            var builder = new PlayerLoopSystemBuilder(mockSystem);

            // Act
            builder.InjectUpdateFunction(DummyInjectionTarget.UpdateFunction, typeof(DummySystemB), before: true);
            var builtSystem = builder.Build();

            // Assert
            var injectedSystem = builtSystem.subSystemList[0];
            Assert.AreEqual(typeof(DummyInjectionTarget), injectedSystem.type);
            Assert.IsNotNull(injectedSystem.updateDelegate);
        }

        /// <summary>
        /// 複合構造で特定の子要素に注入できることを検証します
        /// </summary>
        [Test]
        public void InjectUpdateFunction_ComplexStructure_InsertsCorrectly()
        {
            // Arrange
            var mockSystem = PlayerLoopSystemMockFactory.CreateComplexStructure();
            var builder = new PlayerLoopSystemBuilder(mockSystem);

            // Act
            var result = builder.InjectUpdateFunction(DummyInjectionTarget.UpdateFunction, typeof(DummySystemC), before: false);
            var builtSystem = builder.Build();

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(4, builtSystem.subSystemList.Length);
            Assert.AreEqual(typeof(DummySystemC), builtSystem.subSystemList[1].type);
            Assert.AreEqual(typeof(DummyInjectionTarget), builtSystem.subSystemList[2].type);
            Assert.AreEqual(typeof(DummySystemD), builtSystem.subSystemList[3].type);
        }
    }
}