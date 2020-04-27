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
using System.Diagnostics;
using IceMilkTea.Core;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace IceMilkTea.Service
{
    public class ImtBuiltinPerformanceSensor : ImtPerformanceSensor
    {
        private const int EarlyUpdateTimeIndex = 0;
        private const int FixedTimeIndex = 1;
        private const int PreUpdateTimeIndex = 2;
        private const int UpdateTimeIndex = 3;
        private const int LateTimeIndex = 4;
        private const int RenderTextureTimeIndex = 5;
        private const int RenderTimeIndex = 6;
        private const int TimesCapacity = RenderTimeIndex + 1;

        private bool disposed;
        private Stopwatch stopwatch;
        private ImtPerformanceMonitorService service;
        private double[] times;
        private int[] frames;



        public override string Name => "ImtBuiltin";


        public double ServiceProcessTime { get; private set; }
        public double EarlyUpdateProcessTime { get; private set; }
        public double FixedUpdateProcessTime { get; private set; }
        public double PreUpdateProcessTime { get; private set; }
        public double UpdateProcessTime { get; private set; }
        public double LateUpdateProcessTime { get; private set; }
        public double RenderTextureProcessTime { get; private set; }
        public double RenderProcessTime { get; private set; }
        public double VMMemoryUsage { get; private set; }



        public ImtBuiltinPerformanceSensor()
        {
            stopwatch = new Stopwatch();
            times = new double[TimesCapacity];
            frames = new int[TimesCapacity];
            InitializePlayerLoop();
        }


        protected override void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }


            if (disposing)
            {
                Camera.onPreCull -= OnPreCull;
                Camera.onPostRender -= OnPostRender;
            }


            disposed = true;
            base.Dispose(disposing);
        }


        private void InitializePlayerLoop()
        {
            var earlyBeginUpdate = new ImtPlayerLoopSystem(GetType(), () => BeginCheckpoint(EarlyUpdateTimeIndex));
            var earlyEndUpdate = new ImtPlayerLoopSystem(GetType(), () => EndCheckpoint(EarlyUpdateTimeIndex));
            var fixedBeginUpdate = new ImtPlayerLoopSystem(GetType(), () => BeginCheckpoint(FixedTimeIndex));
            var fixedEndUpdate = new ImtPlayerLoopSystem(GetType(), () => EndCheckpoint(FixedTimeIndex));
            var preUpdateBeginUpdate = new ImtPlayerLoopSystem(GetType(), () => BeginCheckpoint(PreUpdateTimeIndex));
            var preUpdateEndUpdate = new ImtPlayerLoopSystem(GetType(), () => EndCheckpoint(PreUpdateTimeIndex));
            var updateBeginUpdate = new ImtPlayerLoopSystem(GetType(), () => BeginCheckpoint(UpdateTimeIndex));
            var updateEndUpdate = new ImtPlayerLoopSystem(GetType(), () => EndCheckpoint(UpdateTimeIndex));
            var lateBeginUpdate = new ImtPlayerLoopSystem(GetType(), () => BeginCheckpoint(LateTimeIndex));
            var lateEndUpdate = new ImtPlayerLoopSystem(GetType(), () => EndCheckpoint(LateTimeIndex));
            //var renderTextureBeginUpdate = new ImtPlayerLoopSystem(GetType(), () => BeginCheckpoint(RenderTextureTimeIndex));
            //var renderTextureEndUpdate = new ImtPlayerLoopSystem(GetType(), () => EndCheckpoint(RenderTextureTimeIndex));
            //var renderBeginUpdate = new ImtPlayerLoopSystem(GetType(), () => BeginCheckpoint(RenderTimeIndex));
            //var renderEndUpdate = new ImtPlayerLoopSystem(GetType(), () => EndCheckpoint(RenderTimeIndex));


            var current = ImtPlayerLoopSystem.GetCurrentPlayerLoop();
            current.Insert<EarlyUpdate.UnityWebRequestUpdate>(InsertTiming.BeforeInsert, earlyBeginUpdate);
            current.Insert<EarlyUpdate.SpriteAtlasManagerUpdate>(InsertTiming.AfterInsert, earlyEndUpdate);
            current.Insert<FixedUpdate.ClearLines>(InsertTiming.BeforeInsert, fixedBeginUpdate);
            current.Insert<FixedUpdate.ScriptRunDelayedFixedFrameRate>(InsertTiming.AfterInsert, fixedEndUpdate);
            current.Insert<PreUpdate.PhysicsUpdate>(InsertTiming.BeforeInsert, preUpdateBeginUpdate);
            current.Insert<PreUpdate.UpdateVideo>(InsertTiming.AfterInsert, preUpdateEndUpdate);
            current.Insert<Update.ScriptRunBehaviourUpdate>(InsertTiming.BeforeInsert, updateBeginUpdate);
            current.Insert<Update.DirectorUpdate>(InsertTiming.AfterInsert, updateEndUpdate);
            current.Insert<PreLateUpdate.AIUpdatePostScript>(InsertTiming.BeforeInsert, lateBeginUpdate);
            current.Insert<PostLateUpdate.ScriptRunDelayedDynamicFrameRate>(InsertTiming.AfterInsert, lateEndUpdate);
            current.BuildAndSetUnityPlayerLoop();


            Camera.onPreCull += OnPreCull;
            Camera.onPostRender += OnPostRender;
        }


        public override void Update()
        {
            ServiceProcessTime = GameMain.Current.ServiceManager.ServiceProcessTime;
            VMMemoryUsage = GC.GetTotalMemory(false);
        }


        private void OnPreCull(Camera camera)
        {
            if (camera.targetTexture == null)
            {
                BeginCheckpoint(RenderTimeIndex);
                return;
            }


            BeginCheckpoint(RenderTextureTimeIndex);
        }


        private void OnPostRender(Camera camera)
        {
            if (camera.targetTexture == null)
            {
                EndCheckpoint(RenderTimeIndex);
                return;
            }


            EndCheckpoint(RenderTextureTimeIndex);
        }


        private void BeginCheckpoint(int index)
        {
            if (frames[index] < Time.frameCount)
            {
                frames[index] = Time.frameCount;
                stopwatch.Restart();
            }
            else
            {
                stopwatch.Start();
            }
        }


        private void EndCheckpoint(int index)
        {
            stopwatch.Stop();
            times[index] = stopwatch.ElapsedTicks / (double)Stopwatch.Frequency * 1000.0 * 1000.0;
        }
    }
}
