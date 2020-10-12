// zlib/libpng License
//
// Copyright (c) 2020 Sinoa
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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using IceMilkTea.Core;

namespace IceMilkTea.Service
{
    /// <summary>
    /// アセットバンドル自体またはアセットバンドルの構成に問題が発生した場合にスローされる例外です
    /// </summary>
    public class InvalidAssetBundleException : ImtException
    {
        /// <summary>
        /// 例外を発生させたアセットバンドル
        /// </summary>
        public string AssetBundleName { get; }


        /// <summary>
        /// 例外を発生するまでに辿ったアセットバンドル名の依存コレクション。辿る処理以外の例外の場合は空の可能性があります。
        /// </summary>
        public IReadOnlyCollection<string> DependenciesRoute { get; }



        public InvalidAssetBundleException() : this(string.Empty, string.Empty, null, null)
        {
        }


        public InvalidAssetBundleException(string message) : this(message, string.Empty, null, null)
        {
        }


        public InvalidAssetBundleException(string message, Exception innerException) : this(message, string.Empty, null, innerException)
        {
        }


        protected InvalidAssetBundleException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }


        public InvalidAssetBundleException(string message, string assetBundleName, IEnumerable<string> dependenciesRoute) : this(message, assetBundleName, dependenciesRoute, null)
        {
        }


        public InvalidAssetBundleException(string message, string assetBundleName, IEnumerable<string> dependenciesRoute, Exception innerException) : base(message, innerException)
        {
            AssetBundleName = assetBundleName ?? string.Empty;
            DependenciesRoute = (dependenciesRoute ?? Array.Empty<string>()).ToList().AsReadOnly();
        }
    }
}