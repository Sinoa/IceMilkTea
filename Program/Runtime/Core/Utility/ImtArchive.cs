// zlib/libpng License
//
// Copyright (c) 2018 Sinoa
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
using System.Text;

namespace IceMilkTea.Core
{
    public abstract class CrcBase<T>
    {
        public abstract T Compute(byte[] buffer);


        public abstract T Compute(byte[] buffer, int index, int count);


        public abstract T Compute(Stream stream);


        public abstract T Compute(string message);


        public abstract T Compute(string message, Encoding encoding);
    }



    public abstract class Crc32Base : CrcBase<uint>
    {
        protected uint[] CreateTable(uint polynomial)
        {
            var table = new uint[256];
            for (uint i = 0U; i < (uint)table.Length; ++i)
            {
                uint num = ((i & 1) * polynomial) ^ (i >> 1);
                num = ((num & 1) * polynomial) ^ (num >> 1);
                num = ((num & 1) * polynomial) ^ (num >> 1);
                num = ((num & 1) * polynomial) ^ (num >> 1);
                num = ((num & 1) * polynomial) ^ (num >> 1);
                num = ((num & 1) * polynomial) ^ (num >> 1);
                num = ((num & 1) * polynomial) ^ (num >> 1);
                num = ((num & 1) * polynomial) ^ (num >> 1);
                table[i] = num;
            }
            return table;
        }
    }



    public abstract class Crc64Base : CrcBase<ulong>
    {
        protected ulong[] CreateTable(ulong polynomial)
        {
            var table = new ulong[256];
            for (ulong i = 0UL; i < (ulong)table.Length; ++i)
            {
                ulong num = ((i & 1) * polynomial) ^ (i >> 1);
                num = ((num & 1) * polynomial) ^ (num >> 1);
                num = ((num & 1) * polynomial) ^ (num >> 1);
                num = ((num & 1) * polynomial) ^ (num >> 1);
                num = ((num & 1) * polynomial) ^ (num >> 1);
                num = ((num & 1) * polynomial) ^ (num >> 1);
                num = ((num & 1) * polynomial) ^ (num >> 1);
                num = ((num & 1) * polynomial) ^ (num >> 1);
                table[i] = num;
            }
            return table;
        }
    }
}