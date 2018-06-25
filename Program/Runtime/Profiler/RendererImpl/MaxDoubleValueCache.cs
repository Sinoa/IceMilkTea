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

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace IceMilkTea.Profiler
{

    /// <summary>
    /// 最大値を更新するか、指定した時間が経過するまで値をキャッシュします。
    /// </summary>
    internal class MaxDoubleValueCache
    {
        private readonly int cacheMilliSeconds;
        long lastUpdateTick;
        double _value;
        public double Value { get { return this._value; } }

        /// <summary>
        /// MaxDoubleValueCacheのコンストラクタです。
        /// </summary>
        /// <param name="cacheMilliSeconds">キャッシュする時間(ミリ秒)</param>
        /// <param name="initialValue">初期値</param>
        public MaxDoubleValueCache(int cacheMilliSeconds, double initialValue)
        {
            this.cacheMilliSeconds = cacheMilliSeconds;
            this._value = initialValue;
            this.lastUpdateTick = DateTime.Now.Ticks;
        }

        /// <summary>
        /// 値の更新を試みます。
        /// 前回の値より大きいか、キャッシュ時間を超えたら更新されます。
        /// </summary>
        /// <param name="value">更新する値</param>
        public void Update(double value)
        {
            var tick = DateTime.Now.Ticks;

            if (this._value < value
                 || TimeSpan.FromTicks(tick - this.lastUpdateTick).TotalMilliseconds > this.cacheMilliSeconds)
            {
                this.lastUpdateTick = tick;
                this._value = value;
                return;
            }
        }
    }
}