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
    /// 2番目の注入テスト用のダミー型です
    /// </summary>
    public struct DummyInjectionTarget2
    {
        /// <summary>
        /// テスト用の更新関数です
        /// </summary>
        public static void UpdateFunction()
        {
        }
    }

    /// <summary>
    /// PlayerLoopSystemBuilder.TryInjectUpdateFunction メソッドのテストクラスです
    /// </summary>
    [TestFixture]
    public class TryInjectUpdateFunctionTests
    {
        /// <summary>
        /// 初回の注入が成功することを検証します
        /// </summary>
        [Test]
        public void TryInjectUpdateFunction_FirstInjection_ReturnsTrue()
        {
            // Arrange
            var mockSystem = PlayerLoopSystemMockFactory.CreateTwoLevel<DummySystemA, DummySystemB>();
            var builder = new PlayerLoopSystemBuilder(mockSystem);

            // Act
            var result = builder.TryInjectUpdateFunction(DummyInjectionTarget.UpdateFunction, typeof(DummySystemB), before: true);

            // Assert
            Assert.IsTrue(result);
        }

        /// <summary>
        /// 同じ更新関数を2回注入しようとした場合に2回目が false を返すことを検証します
        /// </summary>
        [Test]
        public void TryInjectUpdateFunction_DuplicateInjection_ReturnsFalse()
        {
            // Arrange
            var mockSystem = PlayerLoopSystemMockFactory.CreateMultipleChildren<DummySystemA, DummySystemB, DummySystemC, DummySystemD>();
            var builder = new PlayerLoopSystemBuilder(mockSystem);

            // Act
            var firstResult = builder.TryInjectUpdateFunction(DummyInjectionTarget.UpdateFunction, typeof(DummySystemB), before: false);
            var secondResult = builder.TryInjectUpdateFunction(DummyInjectionTarget.UpdateFunction, typeof(DummySystemC), before: true);

            // Assert
            Assert.IsTrue(firstResult);
            Assert.IsFalse(secondResult);
        }

        /// <summary>
        /// 重複時に構造が変更されないことを検証します
        /// </summary>
        [Test]
        public void TryInjectUpdateFunction_DuplicateInjection_StructureUnchanged()
        {
            // Arrange
            var mockSystem = PlayerLoopSystemMockFactory.CreateTwoLevel<DummySystemA, DummySystemB>();
            var builder = new PlayerLoopSystemBuilder(mockSystem);

            // Act
            builder.TryInjectUpdateFunction(DummyInjectionTarget.UpdateFunction, typeof(DummySystemB), before: true);
            var structureAfterFirst = builder.Build().subSystemList.Length;
            builder.TryInjectUpdateFunction(DummyInjectionTarget.UpdateFunction, typeof(DummySystemB), before: false);
            var structureAfterSecond = builder.Build().subSystemList.Length;

            // Assert
            Assert.AreEqual(structureAfterFirst, structureAfterSecond);
        }

        /// <summary>
        /// 異なる更新関数は両方とも注入できることを検証します
        /// </summary>
        [Test]
        public void TryInjectUpdateFunction_DifferentFunctions_BothSucceed()
        {
            // Arrange
            var mockSystem = PlayerLoopSystemMockFactory.CreateMultipleChildren<DummySystemA, DummySystemB, DummySystemC, DummySystemD>();
            var builder = new PlayerLoopSystemBuilder(mockSystem);

            // Act
            var firstResult = builder.TryInjectUpdateFunction(DummyInjectionTarget.UpdateFunction, typeof(DummySystemB), before: false);
            var secondResult = builder.TryInjectUpdateFunction(DummyInjectionTarget2.UpdateFunction, typeof(DummySystemC), before: true);
            var builtSystem = builder.Build();

            // Assert
            Assert.IsTrue(firstResult);
            Assert.IsTrue(secondResult);
            Assert.AreEqual(5, builtSystem.subSystemList.Length);
        }

        /// <summary>
        /// 基準型が見つからない場合に false を返すことを検証します
        /// </summary>
        [Test]
        public void TryInjectUpdateFunction_PivotNotFound_ReturnsFalse()
        {
            // Arrange
            var mockSystem = PlayerLoopSystemMockFactory.CreateSingleLevel<DummySystemA>();
            var builder = new PlayerLoopSystemBuilder(mockSystem);

            // Act
            var result = builder.TryInjectUpdateFunction(DummyInjectionTarget.UpdateFunction, typeof(DummyNonExistent), before: true);

            // Assert
            Assert.IsFalse(result);
        }

        /// <summary>
        /// null の更新関数を渡した場合に ArgumentNullException がスローされることを検証します
        /// </summary>
        [Test]
        public void TryInjectUpdateFunction_NullFunction_ThrowsArgumentNullException()
        {
            // Arrange
            var mockSystem = PlayerLoopSystemMockFactory.CreateSingleLevel<DummySystemA>();
            var builder = new PlayerLoopSystemBuilder(mockSystem);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => builder.TryInjectUpdateFunction(null, typeof(DummySystemA), true));
        }

        /// <summary>
        /// 既に型が登録済みの構造に TryInject した場合に false を返すことを検証します
        /// </summary>
        [Test]
        public void TryInjectUpdateFunction_TypeAlreadyExists_ReturnsFalse()
        {
            // Arrange
            var mockSystem = PlayerLoopSystemMockFactory.CreateTwoLevel<DummySystemA, DummyInjectionTarget>();
            var builder = new PlayerLoopSystemBuilder(mockSystem);

            // Act
            var result = builder.TryInjectUpdateFunction(DummyInjectionTarget.UpdateFunction, typeof(DummySystemA), before: false);

            // Assert
            Assert.IsFalse(result);
        }
    }
}