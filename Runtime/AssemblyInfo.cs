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

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("IceMilkTea")]
[assembly: AssemblyProduct("IceMilkTea")]
[assembly: AssemblyDescription("Unity Game Framework")]
[assembly: AssemblyCompany("Sinoa")]
[assembly: AssemblyTrademark("Sinoa")]
[assembly: AssemblyCopyright("Copyright © 2018 - 2019 Sinoa")]
[assembly: ComVisible(false)]
[assembly: Guid("6B94121C-5255-4DA7-94B6-34FC3C377178")]
[assembly: AssemblyVersion("0.0.2.*")]
[assembly: AssemblyFileVersion("0.0.2.0")]

#if DEBUG
[assembly: InternalsVisibleTo("IceMilkTeaEditor")]
[assembly: InternalsVisibleTo("IceMilkTeaTestDynamic")]
[assembly: InternalsVisibleTo("IceMilkTeaTestStatic")]
#endif