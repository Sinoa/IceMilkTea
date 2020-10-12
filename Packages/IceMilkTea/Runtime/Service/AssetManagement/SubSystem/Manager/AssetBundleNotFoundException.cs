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
using System.Runtime.Serialization;
using IceMilkTea.Core;

namespace IceMilkTea.Service
{
    /// <summary>
    /// 存在しないアセットバンドルへアクセスしようとして失敗した場合にスローされる例外です
    /// </summary>
    public class AssetBundleNotFoundException : ImtException
    {
        /// <summary>
        /// 見つからなかったアセットバンドルへのパス
        /// </summary>
        public string AssetBundlePath { get; }



        public AssetBundleNotFoundException()
        {
        }


        public AssetBundleNotFoundException(string message) : this(message, null, null)
        {
        }


        public AssetBundleNotFoundException(string message, Exception innerException) : this(message, null, innerException)
        {
        }


        protected AssetBundleNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }


        public AssetBundleNotFoundException(string message, string assetBundlePath) : this(message, assetBundlePath, null)
        {
        }


        public AssetBundleNotFoundException(string message, string assetBundlePath, Exception innerException) : base(message, innerException)
        {
            AssetBundlePath = assetBundlePath ?? string.Empty;
        }
    }
}