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
using IceMilkTea.Core;

namespace IceMilkTea.Service
{
    public abstract class ImtPerformanceSensor : IDisposable
    {
        private bool disposed;



        public abstract string Name { get; }



        public ImtPerformanceSensor()
        {
            GetService().AddSensor(this);
        }


        ~ImtPerformanceSensor()
        {
            Dispose(false);
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }


            if (disposing)
            {
                GetService().RemoveSensor(this);
            }


            disposed = true;
        }


        protected static ImtPerformanceMonitorService GetService()
        {
            return GameMain.Current.ServiceManager.GetService<ImtPerformanceMonitorService>();
        }


        protected static PerformanceGraphics GetGraphics()
        {
            return GetService().Graphics;
        }


        public virtual void Update()
        {
        }
    }
}