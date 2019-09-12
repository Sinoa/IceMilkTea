// zlib/libpng License
//
// Copyright (c) 2019 Sinoa
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

using System.IO;

namespace IceMilkTea.SubSystem
{
    /// <summary>
    /// アセットカタログのデータを相互変換するシリアライザインターフェイスです
    /// </summary>
    public interface IAssetCatalogSerializer
    {
        /// <summary>
        /// 指定したストリームにカタログの内容をシリアライズして出力します
        /// </summary>
        /// <param name="stream">出力先ストリーム</param>
        /// <param name="catalog">シリアライズするカタログ</param>
        void Serialize(Stream stream, IAssetCatalog catalog);


        /// <summary>
        /// 指定したストリームからカタログのデシリアライズをします
        /// </summary>
        /// <param name="stream">入力元ストリーム</param>
        /// <param name="catalog">デシリアライズされたカタログの設定先参照</param>
        void Deserialize(Stream stream, out IAssetCatalog catalog);
    }
}