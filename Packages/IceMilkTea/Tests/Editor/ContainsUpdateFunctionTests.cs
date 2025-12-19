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
    /// PlayerLoopSystemBuilder.ContainsUpdateFunction メソッドのテストクラスです
    /// </summary>
    [TestFixture]
    public class ContainsUpdateFunctionTests
    {
        /// <summary>
        /// 空の PlayerLoopSystem で更新関数を検索した場合に false を返すことを検証します
        /// </summary>
        [Test]
        public void ContainsUpdateFunction_EmptyLoop_ReturnsFalse()
        {
            // Arrange
            var mockSystem = PlayerLoopSystemMockFactory.CreateEmpty();
            var builder = new PlayerLoopSystemBuilder(mockSystem);

            // Act
            var result = builder.ContainsUpdateFunction(DummyInjectionTarget.UpdateFunction);

            // Assert
            Assert.IsFalse(result);
        }

        /// <summary>
        /// 更新関数の宣言型が存在する場合に true を返すことを検証します
        /// </summary>
        [Test]
        public void ContainsUpdateFunction_DeclaringTypeExists_ReturnsTrue()
        {
            // Arrange
            var mockSystem = PlayerLoopSystemMockFactory.CreateSingleLevel<DummyInjectionTarget>();
            var builder = new PlayerLoopSystemBuilder(mockSystem);

            // Act
            var result = builder.ContainsUpdateFunction(DummyInjectionTarget.UpdateFunction);

            // Assert
            Assert.IsTrue(result);
        }

        /// <summary>
        /// 更新関数の宣言型が存在しない場合に false を返すことを検証します
        /// </summary>
        [Test]
        public void ContainsUpdateFunction_DeclaringTypeNotExists_ReturnsFalse()
        {
            // Arrange
            var mockSystem = PlayerLoopSystemMockFactory.CreateSingleLevel<DummySystemA>();
            var builder = new PlayerLoopSystemBuilder(mockSystem);

            // Act
            var result = builder.ContainsUpdateFunction(DummyInjectionTarget.UpdateFunction);

            // Assert
            Assert.IsFalse(result);
        }

        /// <summary>
        /// null の更新関数を渡した場合に ArgumentNullException がスローされることを検証します
        /// </summary>
        [Test]
        public void ContainsUpdateFunction_NullFunction_ThrowsArgumentNullException()
        {
            // Arrange
            var mockSystem = PlayerLoopSystemMockFactory.CreateEmpty();
            var builder = new PlayerLoopSystemBuilder(mockSystem);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => builder.ContainsUpdateFunction(null));
        }

        /// <summary>
        /// ネスト構造の深い位置に宣言型が存在する場合に true を返すことを検証します
        /// </summary>
        [Test]
        public void ContainsUpdateFunction_DeclaringTypeExistsInNestedStructure_ReturnsTrue()
        {
            // Arrange
            var mockSystem = PlayerLoopSystemMockFactory.CreateNestedThreeLevel<DummySystemA, DummySystemB, DummyInjectionTarget>();
            var builder = new PlayerLoopSystemBuilder(mockSystem);

            // Act
            var result = builder.ContainsUpdateFunction(DummyInjectionTarget.UpdateFunction);

            // Assert
            Assert.IsTrue(result);
        }
    }
}