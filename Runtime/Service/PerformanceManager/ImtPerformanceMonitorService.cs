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
    public class ImtPerformanceMonitorService : GameService
    {
        private List<ImtPerformanceSensor> sensorList;
        private List<ImtPerformanceRenderer> rendererList;

        public PerformanceGraphics Graphics { get; private set; }



        protected internal override void Startup(out GameServiceStartupInfo info)
        {
            sensorList = new List<ImtPerformanceSensor>();
            rendererList = new List<ImtPerformanceRenderer>();
            Graphics = new PerformanceGraphics();


            info = new GameServiceStartupInfo()
            {
                UpdateFunctionTable = new Dictionary<GameServiceUpdateTiming, Action>()
                {
                    { GameServiceUpdateTiming.OnEndOfFrame, OnEndOfFrame }
                }
            };
        }


        private void OnEndOfFrame()
        {
            UpdateSensor();
            UpdateRenderer();
            Graphics.Render();
        }


        private void UpdateSensor()
        {
            foreach (var sensor in sensorList)
            {
                sensor.Update();
            }
        }


        private void UpdateRenderer()
        {
            foreach (var renderer in rendererList)
            {
                renderer.Render();
            }
        }


        public void AddSensor(ImtPerformanceSensor sensor)
        {
            sensorList.Add(sensor);
        }


        public void RemoveSensor(ImtPerformanceSensor sensor)
        {
            sensorList.Remove(sensor);
        }


        public T GetSensor<T>() where T : ImtPerformanceSensor
        {
            foreach (var sensor in sensorList)
            {
                if (sensor is T)
                {
                    return (T)sensor;
                }
            }


            return null;
        }


        public ImtPerformanceSensor GetSensor(string name)
        {
            foreach (var sensor in sensorList)
            {
                if (sensor.Name == name)
                {
                    return sensor;
                }
            }


            return null;
        }


        public void AddRenderer(ImtPerformanceRenderer renderer)
        {
            rendererList.Add(renderer);
        }


        public void RemoveRenderer(ImtPerformanceRenderer renderer)
        {
            rendererList.Remove(renderer);
        }
    }
}