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
using IceMilkTea.Core;

namespace IceMilkTea.Service
{
    public class ImtPerformanceRenderer : IDisposable
    {
        private bool disposed;
        private readonly PerformanceGraphics graphics;
        private readonly List<ImtTextReference> textReferenceList;
        private readonly List<ImtNumberReference> numberReferenceList;
        private readonly List<ImtSquareReference> squareReferenceList;



        public ImtPerformanceRenderer()
        {
            graphics = GetGraphics();
            textReferenceList = new List<ImtTextReference>();
            numberReferenceList = new List<ImtNumberReference>();
            squareReferenceList = new List<ImtSquareReference>();
            GetService().AddRenderer(this);
        }


        ~ImtPerformanceRenderer()
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
                foreach (var textReference in textReferenceList)
                {
                    graphics.RemoveTextReference(textReference);
                }


                foreach (var numberReference in numberReferenceList)
                {
                    graphics.RemoveNumberReference(numberReference);
                }


                foreach (var squareReference in squareReferenceList)
                {
                    graphics.RemoveSquareReference(squareReference);
                }


                textReferenceList.Clear();
                numberReferenceList.Clear();
                squareReferenceList.Clear();


                GetService().RemoveRenderer(this);
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


        protected ImtTextReference CreateTextReference()
        {
            var reference = graphics.CreateTextReference();
            textReferenceList.Add(reference);
            return reference;
        }


        protected ImtNumberReference CreateNumberReference()
        {
            var reference = graphics.CreateNumberReference();
            numberReferenceList.Add(reference);
            return reference;
        }


        protected ImtSquareReference CreateSquareReference()
        {
            var reference = graphics.CreateSquareReference();
            squareReferenceList.Add(reference);
            return reference;
        }


        protected void RemoveTextReference(ImtTextReference reference)
        {
            graphics.RemoveTextReference(reference);
            textReferenceList.Remove(reference);
        }


        protected void RemoveNumberReference(ImtNumberReference reference)
        {
            graphics.RemoveNumberReference(reference);
            numberReferenceList.Remove(reference);
        }


        protected void RemoveSquareReference(ImtSquareReference reference)
        {
            graphics.RemoveSquareReference(reference);
            squareReferenceList.Remove(reference);
        }


        public virtual void Render()
        {
        }
    }
}