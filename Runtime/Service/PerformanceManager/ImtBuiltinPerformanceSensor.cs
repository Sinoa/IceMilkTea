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
using System.Diagnostics;
using System.Reflection;
using IceMilkTea.Core;
using UnityEngine.LowLevel;

namespace IceMilkTea.Service
{
    public class ImtBuiltinPerformanceSensor : ImtPerformanceSensor
    {
        private const int FirstlyTimeIndex = 0;
        private const int FixedTimeIndex = 1;
        private const int UpdateTimeIndex = 2;
        private const int LateTimeIndex = 3;
        private const int RenderTextureTimeIndex = 4;
        private const int RenderTimeIndex = 5;
        private const int TimesCapacity = RenderTimeIndex + 1;

        private Stopwatch stopwatch;
        private ImtPerformanceMonitorService service;
        private double[] times;



        public override string Name => "ImtBuiltin";


        public double ServiceProcessTime => GameMain.Current.ServiceManager.ServiceProcessTime;
        public double FirstlyUpdateProcessTime { get; private set; }
        public double FixedUpdateProcessTime { get; private set; }
        public double UpdateProcessTime { get; private set; }
        public double LateUpdateProcessTime { get; private set; }
        public double RenderTextureProcessTime { get; private set; }
        public double RenderProcessTime { get; private set; }
        public double VMMemoryUsage => GC.GetTotalMemory(false);



        public ImtBuiltinPerformanceSensor()
        {
            stopwatch = new Stopwatch();
            times = new double[TimesCapacity];
            InitializePlayerLoop();
        }


        private void InitializePlayerLoop()
        {
            var firstlyBeginUpdate = new ImtPlayerLoopSystem(GetType(), BeginCheckpoint);
            var firstlyEndUpdate = new ImtPlayerLoopSystem(GetType(), () => EndCheckpoint(FirstlyTimeIndex));
            var fixedBeginUpdate = new ImtPlayerLoopSystem(GetType(), BeginCheckpoint);
            var fixedEndUpdate = new ImtPlayerLoopSystem(GetType(), () => EndCheckpoint(FixedTimeIndex));
            var UpdateBeginUpdate = new ImtPlayerLoopSystem(GetType(), BeginCheckpoint);
            var UpdateEndUpdate = new ImtPlayerLoopSystem(GetType(), () => EndCheckpoint(UpdateTimeIndex));
            var LateBeginUpdate = new ImtPlayerLoopSystem(GetType(), BeginCheckpoint);
            var LateEndUpdate = new ImtPlayerLoopSystem(GetType(), () => EndCheckpoint(LateTimeIndex));
        }


        private void BeginCheckpoint()
        {
            stopwatch.Restart();
        }


        private void EndCheckpoint(int index)
        {
            stopwatch.Stop();
            times[index] = stopwatch.ElapsedTicks / (double)Stopwatch.Frequency * 1000.0 * 1000.0;
        }
    }
}
