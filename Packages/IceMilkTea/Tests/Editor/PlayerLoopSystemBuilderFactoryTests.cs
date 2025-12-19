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

using Foxtamp.IceMilkTea.Utilities;
using NUnit.Framework;

namespace Foxtamp.IceMilkTea.Tests
{
    /// <summary>
    /// PlayerLoopSystemBuilder のファクトリメソッドと Build メソッドのテストクラスです
    /// </summary>
    [TestFixture]
    public class PlayerLoopSystemBuilderFactoryTests
    {
        /// <summary>
        /// CreateEmpty が null でないインスタンスを返すことを検証します
        /// </summary>
        [Test]
        public void CreateEmpty_ReturnsNonNullBuilder()
        {
            // Act
            var builder = PlayerLoopSystemBuilder.CreateEmpty();

            // Assert
            Assert.IsNotNull(builder);
        }

        /// <summary>
        /// CreateEmpty で生成したビルダーの Build 結果が空の構造であることを検証します
        /// </summary>
        [Test]
        public void CreateEmpty_BuildReturnsEmptyStructure()
        {
            // Arrange
            var builder = PlayerLoopSystemBuilder.CreateEmpty();

            // Act
            var result = builder.Build();

            // Assert
            Assert.IsNull(result.type);
            Assert.IsNull(result.subSystemList);
        }

        /// <summary>
        /// コンストラクタで渡した PlayerLoopSystem が Build で返されることを検証します
        /// </summary>
        [Test]
        public void Constructor_BuildReturnsSameStructure()
        {
            // Arrange
            var mockSystem = PlayerLoopSystemMockFactory.CreateSingleLevel<DummySystemA>();
            var builder = new PlayerLoopSystemBuilder(mockSystem);

            // Act
            var result = builder.Build();

            // Assert
            Assert.AreEqual(typeof(DummySystemA), result.type);
        }

        /// <summary>
        /// Build を複数回呼んでも同じ構造が返されることを検証します
        /// </summary>
        [Test]
        public void Build_MultipleCalls_ReturnsSameStructure()
        {
            // Arrange
            var mockSystem = PlayerLoopSystemMockFactory.CreateTwoLevel<DummySystemA, DummySystemB>();
            var builder = new PlayerLoopSystemBuilder(mockSystem);

            // Act
            var result1 = builder.Build();
            var result2 = builder.Build();

            // Assert
            Assert.AreEqual(result1.type, result2.type);
            Assert.AreEqual(result1.subSystemList.Length, result2.subSystemList.Length);
        }

        /// <summary>
        /// 注入後の Build が更新された構造を返すことを検証します
        /// </summary>
        [Test]
        public void Build_AfterInjection_ReturnsUpdatedStructure()
        {
            // Arrange
            var mockSystem = PlayerLoopSystemMockFactory.CreateTwoLevel<DummySystemA, DummySystemB>();
            var builder = new PlayerLoopSystemBuilder(mockSystem);
            var originalLength = builder.Build().subSystemList.Length;

            // Act
            builder.InjectUpdateFunction(DummyInjectionTarget.UpdateFunction, typeof(DummySystemB), before: true);
            var newLength = builder.Build().subSystemList.Length;

            // Assert
            Assert.AreEqual(originalLength + 1, newLength);
        }
    }
}