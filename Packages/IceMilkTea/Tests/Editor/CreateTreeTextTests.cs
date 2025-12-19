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
    /// PlayerLoopSystemBuilder.CreateTreeText メソッドのテストクラスです
    /// </summary>
    [TestFixture]
    public class CreateTreeTextTests
    {
        /// <summary>
        /// 空の PlayerLoopSystem で NULL が出力されることを検証します
        /// </summary>
        [Test]
        public void CreateTreeText_EmptyLoop_ReturnsNullTypeName()
        {
            // Arrange
            var mockSystem = PlayerLoopSystemMockFactory.CreateEmpty();
            var builder = new PlayerLoopSystemBuilder(mockSystem);

            // Act
            var result = builder.CreateTreeText();

            // Assert
            Assert.IsTrue(result.Contains("[NULL]"));
        }

        /// <summary>
        /// 単一レベルの構造で正しい型名が出力されることを検証します
        /// </summary>
        [Test]
        public void CreateTreeText_SingleLevel_ReturnsCorrectTypeName()
        {
            // Arrange
            var mockSystem = PlayerLoopSystemMockFactory.CreateSingleLevel<DummySystemA>();
            var builder = new PlayerLoopSystemBuilder(mockSystem);

            // Act
            var result = builder.CreateTreeText();

            // Assert
            Assert.IsTrue(result.Contains("[DummySystemA]"));
        }

        /// <summary>
        /// 2階層の構造で両方の型名が出力されることを検証します
        /// </summary>
        [Test]
        public void CreateTreeText_TwoLevel_ContainsBothTypeNames()
        {
            // Arrange
            var mockSystem = PlayerLoopSystemMockFactory.CreateTwoLevel<DummySystemA, DummySystemB>();
            var builder = new PlayerLoopSystemBuilder(mockSystem);

            // Act
            var result = builder.CreateTreeText();

            // Assert
            Assert.IsTrue(result.Contains("[DummySystemA]"));
            Assert.IsTrue(result.Contains("[DummySystemB]"));
        }

        /// <summary>
        /// 子要素にインデントが適用されていることを検証します
        /// </summary>
        [Test]
        public void CreateTreeText_TwoLevel_ChildHasIndentation()
        {
            // Arrange
            var mockSystem = PlayerLoopSystemMockFactory.CreateTwoLevel<DummySystemA, DummySystemB>();
            var builder = new PlayerLoopSystemBuilder(mockSystem);

            // Act
            var result = builder.CreateTreeText();

            // Assert
            // 子要素は2スペースのインデントを持つはず
            Assert.IsTrue(result.Contains("  [DummySystemB]"));
        }

        /// <summary>
        /// 3階層のネスト構造で正しいインデントが適用されることを検証します
        /// </summary>
        [Test]
        public void CreateTreeText_ThreeLevel_CorrectIndentation()
        {
            // Arrange
            var mockSystem = PlayerLoopSystemMockFactory.CreateNestedThreeLevel<DummySystemA, DummySystemB, DummySystemC>();
            var builder = new PlayerLoopSystemBuilder(mockSystem);

            // Act
            var result = builder.CreateTreeText();

            // Assert
            // ルートはインデントなし
            Assert.IsTrue(result.Contains("[DummySystemA]"));
            // 1階層目は2スペース
            Assert.IsTrue(result.Contains("  [DummySystemB]"));
            // 2階層目は4スペース
            Assert.IsTrue(result.Contains("    [DummySystemC]"));
        }

        /// <summary>
        /// 複数の子要素が全て出力されることを検証します
        /// </summary>
        [Test]
        public void CreateTreeText_MultipleChildren_AllChildrenIncluded()
        {
            // Arrange
            var mockSystem = PlayerLoopSystemMockFactory.CreateMultipleChildren<DummySystemA, DummySystemB, DummySystemC, DummySystemD>();
            var builder = new PlayerLoopSystemBuilder(mockSystem);

            // Act
            var result = builder.CreateTreeText();

            // Assert
            Assert.IsTrue(result.Contains("[DummySystemA]"));
            Assert.IsTrue(result.Contains("[DummySystemB]"));
            Assert.IsTrue(result.Contains("[DummySystemC]"));
            Assert.IsTrue(result.Contains("[DummySystemD]"));
        }

        /// <summary>
        /// 複合構造が正しく出力されることを検証します
        /// </summary>
        [Test]
        public void CreateTreeText_ComplexStructure_CorrectOutput()
        {
            // Arrange
            var mockSystem = PlayerLoopSystemMockFactory.CreateComplexStructure();
            var builder = new PlayerLoopSystemBuilder(mockSystem);

            // Act
            var result = builder.CreateTreeText();

            // Assert
            Assert.IsTrue(result.Contains("[DummySystemA]"));
            Assert.IsTrue(result.Contains("  [DummySystemB]"));
            Assert.IsTrue(result.Contains("    [DummySystemE]"));
            Assert.IsTrue(result.Contains("  [DummySystemC]"));
            Assert.IsTrue(result.Contains("  [DummySystemD]"));
        }

        /// <summary>
        /// 各行が改行で終わっていることを検証します
        /// </summary>
        [Test]
        public void CreateTreeText_MultipleElements_EndsWithNewLine()
        {
            // Arrange
            var mockSystem = PlayerLoopSystemMockFactory.CreateTwoLevel<DummySystemA, DummySystemB>();
            var builder = new PlayerLoopSystemBuilder(mockSystem);

            // Act
            var result = builder.CreateTreeText();
            var lines = result.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            // Assert
            // 最後の行は空文字列（末尾の改行により）
            Assert.AreEqual(string.Empty, lines[^1]);
        }

        /// <summary>
        /// ルートの type が null でも子要素が正しく出力されることを検証します
        /// </summary>
        [Test]
        public void CreateTreeText_NullTypeRoot_OutputsNullAndChildren()
        {
            // Arrange
            var mockSystem = PlayerLoopSystemMockFactory.CreateNullTypeRootWithChildren<DummySystemA>();
            var builder = new PlayerLoopSystemBuilder(mockSystem);

            // Act
            var result = builder.CreateTreeText();

            // Assert
            Assert.IsTrue(result.Contains("[NULL]"));
            Assert.IsTrue(result.Contains("  [DummySystemA]"));
        }
    }
}