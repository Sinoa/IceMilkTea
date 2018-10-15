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

namespace IceMilkTea.Core
{
    /// <summary>
    /// 様々な補間関数を収録したクラスです。
    /// また、各種補間関数及び実行関数は、変数として宣言されているため、値として利用しても再インスタンス化されることは無いため、取り回しやすい用になっています。
    /// </summary>
    public static class EasingFunctions
    {
        // 定数定義
        public const double BackAmplitude = 0.4;
        public const double Exponent = 3.0;
        public const double Magnitude = 10.0;
        public const double ElasticCycles = 5.0;

        // クラス変数宣言
        public static readonly System.Func<double, double> Linear = t => t;
        public static readonly System.Func<double, double> Quadratic = t => t * t;
        public static readonly System.Func<double, double> Cubic = t => t * t * t;
        public static readonly System.Func<double, double> Quartic = t => t * t * t * t;
        public static readonly System.Func<double, double> Quintic = t => t * t * t * t * t;
        public static readonly System.Func<double, double> Power = t => System.Math.Pow(t, Magnitude);
        public static readonly System.Func<double, double> Circle = t => 1.0 - System.Math.Sqrt(1.0 - t * t);
        public static readonly System.Func<double, double> Sine = t => 1.0 - System.Math.Sin(System.Math.PI * 0.5 * (1.0 - t));
        public static readonly System.Func<double, double> Back = t => t * t * t - t * BackAmplitude * System.Math.Sin(System.Math.PI * t);
        public static readonly System.Func<double, double> Exponential = t => (System.Math.Exp(Exponent * t) - 1.0) / (System.Math.Exp(Exponent) - 1.0);
        public static readonly System.Func<double, double> Elastic = t => Exponential(t) * System.Math.Sin((System.Math.PI * 2.0 * ElasticCycles + System.Math.PI * 0.5) * t);
        public static readonly System.Func<double, double> Bounce = t => t; // Bounce = t => throw new System.NotImplementedException();
        public static readonly System.Func<double, double, double, System.Func<double, double>, double> EaseIn = (from, to, t, ease) => from + (to - from) * ease(t);
        public static readonly System.Func<double, double, double, System.Func<double, double>, double> EaseOut = (from, to, t, ease) => from + (to - from) * (1.0 - ease(1.0 - t));
        public static readonly System.Func<double, double, double, System.Func<double, double>, double> EaseInOut = (from, to, t, ease) => from + (to - from) * (t < 0.5 ? ease(t * 2.0) * 0.5 : (1.0 - ease((1.0 - t) * 2.0)) * 0.5 + 0.5);
        public static readonly System.Func<double, double, double, System.Func<double, double>, double> EaseOutIn = (from, to, t, ease) => from + (to - from) * (t < 0.5 ? (1.0 - ease((1.0 - t - 0.5) * 2.0)) * 0.5 : ease((t - 0.5) * 2.0) * 0.5 + 0.5);
    }
}