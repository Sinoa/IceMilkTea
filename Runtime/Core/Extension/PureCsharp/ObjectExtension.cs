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

namespace IceMilkTea.Core
{
    /// <summary>
    /// object 型の拡張関数実装用クラスです
    /// </summary>
    public static class ObjectExtensions
    {
        /// <summary>
        /// 自身のハッシュコードと他のハッシュコードをマージして別のハッシュコードを生成します
        /// </summary>
        /// <param name="obj">マージ先の自身のオブジェクト</param>
        /// <param name="other">マージ元のオブジェクト</param>
        /// <returns>2つのマージされたハッシュコードを返します</returns>
        public static int MergeHashCode(this object obj, object other)
        {
            // 両オブジェクトからハッシュコードを取得してマージ結果を返す
            return MergeHashCode(obj.GetHashCode(), other.GetHashCode());
        }


        /// <summary>
        /// 自身のハッシュコードと他のハッシュコードをマージして別のハッシュコードを生成します
        /// </summary>
        /// <param name="obj">マージ先の自身のオブジェクト</param>
        /// <param name="otherHashCode">マージ元のハッシュコード</param>
        /// <returns>2つのマージされたハッシュコードを返します</returns>
        public static int MergeHashCode(this object obj, int otherHashCode)
        {
            // マージ先のオブジェクトからハッシュコードを取得してマージ結果を返す
            return MergeHashCode(obj.GetHashCode(), otherHashCode);
        }


        /// <summary>
        /// 自身のハッシュコードと他のハッシュコードをマージして別のハッシュコードを生成します
        /// </summary>
        /// <param name="myHashCode">マージ先のオブジェクトのハッシュコード</param>
        /// <param name="other">マージ元のオブジェクト</param>
        /// <returns>2つのマージされたハッシュコードを返します</returns>
        public static int MergeHashCode(this int myHashCode, object other)
        {
            // マージ元のオブジェクトからハッシュコードを取得してマージ結果を返す
            return MergeHashCode(myHashCode, other.GetHashCode());
        }


        /// <summary>
        /// 自身のハッシュコードと他のハッシュコードをマージして別のハッシュコードを生成します
        /// </summary>
        /// <param name="myHashCode">マージ先のオブジェクトのハッシュコード</param>
        /// <param name="otherHashCode">マージ元のハッシュコード</param>
        /// <returns>2つのマージされたハッシュコードを返します</returns>
        public static int MergeHashCode(this int myHashCode, int otherHashCode)
        {
            // 自身と相手の符号付き32bit整数を符号なし32bit整数として受け取り直す
            var myHash = (uint)myHashCode;
            var otherHash = (uint)otherHashCode;


            // マージ結果を返す
            return (int)(((myHash >> 28) | (myHash << 4)) ^ otherHash);
        }
    }
}
