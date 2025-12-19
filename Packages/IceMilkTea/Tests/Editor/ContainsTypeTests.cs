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
    /// PlayerLoopSystemBuilder.ContainsType メソッドのテストクラスです
    /// </summary>
    [TestFixture]
    public class ContainsTypeTests
    {
        /// <summary>
        /// 空の PlayerLoopSystem で型を検索した場合に false を返すことを検証します
        /// </summary>
        [Test]
        public void ContainsType_EmptyLoop_ReturnsFalse()
        {
            // Arrange
            var mockSystem = PlayerLoopSystemMockFactory.CreateEmpty();
            var builder = new PlayerLoopSystemBuilder(mockSystem);

            // Act
            var result = builder.ContainsType(typeof(DummySystemA));

            // Assert
            Assert.IsFalse(result);
        }

        /// <summary>
        /// ルートに型が存在する場合に true を返すことを検証します
        /// </summary>
        [Test]
        public void ContainsType_TypeExistsInRoot_ReturnsTrue()
        {
            // Arrange
            var mockSystem = PlayerLoopSystemMockFactory.CreateSingleLevel<DummySystemA>();
            var builder = new PlayerLoopSystemBuilder(mockSystem);

            // Act
            var result = builder.ContainsType(typeof(DummySystemA));

            // Assert
            Assert.IsTrue(result);
        }

        /// <summary>
        /// 子要素に型が存在する場合に true を返すことを検証します
        /// </summary>
        [Test]
        public void ContainsType_TypeExistsInChild_ReturnsTrue()
        {
            // Arrange
            var mockSystem = PlayerLoopSystemMockFactory.CreateTwoLevel<DummySystemA, DummySystemB>();
            var builder = new PlayerLoopSystemBuilder(mockSystem);

            // Act
            var result = builder.ContainsType(typeof(DummySystemB));

            // Assert
            Assert.IsTrue(result);
        }

        /// <summary>
        /// 深いネスト構造の孫要素に型が存在する場合に true を返すことを検証します
        /// </summary>
        [Test]
        public void ContainsType_TypeExistsInGrandChild_ReturnsTrue()
        {
            // Arrange
            var mockSystem = PlayerLoopSystemMockFactory.CreateNestedThreeLevel<DummySystemA, DummySystemB, DummySystemC>();
            var builder = new PlayerLoopSystemBuilder(mockSystem);

            // Act
            var result = builder.ContainsType(typeof(DummySystemC));

            // Assert
            Assert.IsTrue(result);
        }

        /// <summary>
        /// 存在しない型を検索した場合に false を返すことを検証します
        /// </summary>
        [Test]
        public void ContainsType_TypeNotExists_ReturnsFalse()
        {
            // Arrange
            var mockSystem = PlayerLoopSystemMockFactory.CreateTwoLevel<DummySystemA, DummySystemB>();
            var builder = new PlayerLoopSystemBuilder(mockSystem);

            // Act
            var result = builder.ContainsType(typeof(DummyNonExistent));

            // Assert
            Assert.IsFalse(result);
        }

        /// <summary>
        /// 複数の子要素から特定の型を検索した場合に true を返すことを検証します
        /// </summary>
        [Test]
        public void ContainsType_TypeExistsInMultipleChildren_ReturnsTrue()
        {
            // Arrange
            var mockSystem = PlayerLoopSystemMockFactory.CreateMultipleChildren<DummySystemA, DummySystemB, DummySystemC, DummySystemD>();
            var builder = new PlayerLoopSystemBuilder(mockSystem);

            // Act
            var result = builder.ContainsType(typeof(DummySystemC));

            // Assert
            Assert.IsTrue(result);
        }

        /// <summary>
        /// null の型を渡した場合に ArgumentNullException がスローされることを検証します
        /// </summary>
        [Test]
        public void ContainsType_NullType_ThrowsArgumentNullException()
        {
            // Arrange
            var mockSystem = PlayerLoopSystemMockFactory.CreateEmpty();
            var builder = new PlayerLoopSystemBuilder(mockSystem);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => builder.ContainsType(null));
        }

        /// <summary>
        /// ルートの type が null でも子要素を検索できることを検証します
        /// </summary>
        [Test]
        public void ContainsType_NullTypeRootWithChildren_FindsChildType()
        {
            // Arrange
            var mockSystem = PlayerLoopSystemMockFactory.CreateNullTypeRootWithChildren<DummySystemA>();
            var builder = new PlayerLoopSystemBuilder(mockSystem);

            // Act
            var result = builder.ContainsType(typeof(DummySystemA));

            // Assert
            Assert.IsTrue(result);
        }
    }
}