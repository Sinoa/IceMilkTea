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

using UnityEngine.LowLevel;

namespace Foxtamp.IceMilkTea.Tests
{
    #region ダミー型定義

    /// <summary>
    /// テスト用のダミーPlayerLoopSystem型Aです
    /// </summary>
    public struct DummySystemA
    {
    }

    /// <summary>
    /// テスト用のダミーPlayerLoopSystem型Bです
    /// </summary>
    public struct DummySystemB
    {
    }

    /// <summary>
    /// テスト用のダミーPlayerLoopSystem型Cです
    /// </summary>
    public struct DummySystemC
    {
    }

    /// <summary>
    /// テスト用のダミーPlayerLoopSystem型Dです
    /// </summary>
    public struct DummySystemD
    {
    }

    /// <summary>
    /// テスト用のダミーPlayerLoopSystem型Eです
    /// </summary>
    public struct DummySystemE
    {
    }

    /// <summary>
    /// 注入対象として使用するダミー型です
    /// </summary>
    public struct DummyInjectionTarget
    {
        /// <summary>
        /// テスト用の更新関数です
        /// </summary>
        public static void UpdateFunction()
        {
        }
    }

    /// <summary>
    /// 存在しない型として使用するダミー型です
    /// </summary>
    public struct DummyNonExistent
    {
    }

    #endregion

    /// <summary>
    /// テスト用の PlayerLoopSystem モック構造を生成するファクトリクラスです
    /// </summary>
    public static class PlayerLoopSystemMockFactory
    {
        /// <summary>
        /// 空の PlayerLoopSystem を生成します
        /// </summary>
        /// <returns>type と subSystemList が null の空の PlayerLoopSystem</returns>
        public static PlayerLoopSystem CreateEmpty()
        {
            return new PlayerLoopSystem();
        }

        /// <summary>
        /// ルートのみに型を持つ単一レベルの PlayerLoopSystem を生成します
        /// </summary>
        /// <typeparam name="T">ルートに設定する型</typeparam>
        /// <returns>ルートのみの PlayerLoopSystem</returns>
        public static PlayerLoopSystem CreateSingleLevel<T>()
        {
            return new PlayerLoopSystem
            {
                type = typeof(T),
                subSystemList = null
            };
        }

        /// <summary>
        /// ルートとその直下に1つの子要素を持つ2階層の PlayerLoopSystem を生成します
        /// </summary>
        /// <typeparam name="TRoot">ルートに設定する型</typeparam>
        /// <typeparam name="TChild">子要素に設定する型</typeparam>
        /// <returns>2階層の PlayerLoopSystem</returns>
        public static PlayerLoopSystem CreateTwoLevel<TRoot, TChild>()
        {
            return new PlayerLoopSystem
            {
                type = typeof(TRoot),
                subSystemList = new PlayerLoopSystem[]
                {
                    new()
                    {
                        type = typeof(TChild),
                        subSystemList = null
                    }
                }
            };
        }

        /// <summary>
        /// ルート配下に複数の子要素を持つ PlayerLoopSystem を生成します
        /// </summary>
        /// <typeparam name="TRoot">ルートに設定する型</typeparam>
        /// <typeparam name="TChild1">1番目の子要素の型</typeparam>
        /// <typeparam name="TChild2">2番目の子要素の型</typeparam>
        /// <typeparam name="TChild3">3番目の子要素の型</typeparam>
        /// <returns>複数の子要素を持つ PlayerLoopSystem</returns>
        public static PlayerLoopSystem CreateMultipleChildren<TRoot, TChild1, TChild2, TChild3>()
        {
            return new PlayerLoopSystem
            {
                type = typeof(TRoot),
                subSystemList = new PlayerLoopSystem[]
                {
                    new()
                    {
                        type = typeof(TChild1),
                        subSystemList = null
                    },
                    new()
                    {
                        type = typeof(TChild2),
                        subSystemList = null
                    },
                    new()
                    {
                        type = typeof(TChild3),
                        subSystemList = null
                    }
                }
            };
        }

        /// <summary>
        /// 3階層のネスト構造を持つ PlayerLoopSystem を生成します
        /// Root -> Child -> GrandChild の構造になります
        /// </summary>
        /// <typeparam name="TRoot">ルートに設定する型</typeparam>
        /// <typeparam name="TChild">子要素に設定する型</typeparam>
        /// <typeparam name="TGrandChild">孫要素に設定する型</typeparam>
        /// <returns>3階層のネスト構造を持つ PlayerLoopSystem</returns>
        public static PlayerLoopSystem CreateNestedThreeLevel<TRoot, TChild, TGrandChild>()
        {
            return new PlayerLoopSystem
            {
                type = typeof(TRoot),
                subSystemList = new PlayerLoopSystem[]
                {
                    new()
                    {
                        type = typeof(TChild),
                        subSystemList = new PlayerLoopSystem[]
                        {
                            new()
                            {
                                type = typeof(TGrandChild),
                                subSystemList = null
                            }
                        }
                    }
                }
            };
        }

        /// <summary>
        /// 複合的な構造（複数の子と深いネスト）を持つ PlayerLoopSystem を生成します
        /// Root
        /// ├── ChildA
        /// │   └── GrandChildA
        /// ├── ChildB
        /// └── ChildC
        /// </summary>
        /// <returns>複合的な構造を持つ PlayerLoopSystem</returns>
        public static PlayerLoopSystem CreateComplexStructure()
        {
            return new PlayerLoopSystem
            {
                type = typeof(DummySystemA),
                subSystemList = new PlayerLoopSystem[]
                {
                    new()
                    {
                        type = typeof(DummySystemB),
                        subSystemList = new PlayerLoopSystem[]
                        {
                            new()
                            {
                                type = typeof(DummySystemE),
                                subSystemList = null
                            }
                        }
                    },
                    new()
                    {
                        type = typeof(DummySystemC),
                        subSystemList = null
                    },
                    new()
                    {
                        type = typeof(DummySystemD),
                        subSystemList = null
                    }
                }
            };
        }

        /// <summary>
        /// type が null でサブシステムのみを持つ PlayerLoopSystem を生成します
        /// </summary>
        /// <typeparam name="TChild">子要素に設定する型</typeparam>
        /// <returns>type が null のルートを持つ PlayerLoopSystem</returns>
        public static PlayerLoopSystem CreateNullTypeRootWithChildren<TChild>()
        {
            return new PlayerLoopSystem
            {
                type = null,
                subSystemList = new PlayerLoopSystem[]
                {
                    new()
                    {
                        type = typeof(TChild),
                        subSystemList = null
                    }
                }
            };
        }
    }
}