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
using static System.Math;

namespace IceMilkTea.Core
{
    /// <summary>
    /// 様々な補間関数を収録したクラスです。
    /// また、各種補間関数及び実行関数は、変数として宣言されているため、値として利用しても再インスタンス化されることは無いため、取り回しやすい用になっています。
    /// </summary>
    public static class EasingFunction
    {
        // 定数定義
        /// <summary>
        /// Back 補間関数が使用する後退の振幅値です
        /// </summary>
        public const double BackAmplitude = 0.4;

        /// <summary>
        /// Exponential 補間関数が使用する指数値です
        /// </summary>
        public const double Exponent = 3.0;

        /// <summary>
        /// Power 補間関数が使用する累乗の指数値です
        /// </summary>
        public const double Magnitude = 10.0;

        /// <summary>
        /// Elastic 補間関数が使用する振動回数です
        /// </summary>
        public const double ElasticCycles = 5.0;

        // クラス変数宣言
        /// <summary>
        /// 線形補間関数です
        /// </summary>
        public static readonly Func<double, double> Linear = t => t;

        /// <summary>
        /// 2次補間関数です
        /// </summary>
        public static readonly Func<double, double> Quadratic = t => t * t;

        /// <summary>
        /// 3次補間関数です
        /// </summary>
        public static readonly Func<double, double> Cubic = t => t * t * t;

        /// <summary>
        /// 4次補間関数です
        /// </summary>
        public static readonly Func<double, double> Quartic = t => t * t * t * t;

        /// <summary>
        /// 5次補間関数です
        /// </summary>
        public static readonly Func<double, double> Quintic = t => t * t * t * t * t;

        /// <summary>
        /// Magnitude を指数とした累乗補間関数です
        /// </summary>
        public static readonly Func<double, double> Power = t => Pow(t, Magnitude);

        /// <summary>
        /// 円弧補間関数です
        /// </summary>
        public static readonly Func<double, double> Circle = t => 1.0 - Sqrt(1.0 - t * t);

        /// <summary>
        /// 正弦波補間関数です
        /// </summary>
        public static readonly Func<double, double> Sine = t => 1.0 - Sin(PI * 0.5 * (1.0 - t));

        /// <summary>
        /// BackAmplitude の振幅で一度後退してから進行する補間関数です
        /// </summary>
        public static readonly Func<double, double> Back = t => t * t * t - t * BackAmplitude * Sin(PI * t);

        /// <summary>
        /// Exponent を指数とした指数補間関数です
        /// </summary>
        public static readonly Func<double, double> Exponential = t => (Exp(Exponent * t) - 1.0) / (Exp(Exponent) - 1.0);

        /// <summary>
        /// ElasticCycles の回数で振動する弾性補間関数です
        /// </summary>
        public static readonly Func<double, double> Elastic = t => Exponential(t) * Sin((PI * 2.0 * ElasticCycles + PI * 0.5) * t);

        /// <summary>
        /// バウンス補間関数です（未実装のため、現在は Linear と同一の動作をします）
        /// </summary>
        public static readonly Func<double, double> Bounce = t => t; // Bounce = t => throw new NotImplementedException();

        /// <summary>
        /// 指定された補間関数を用いて from から to へイーズイン補間した値を返します
        /// </summary>
        public static readonly Func<double, double, double, Func<double, double>, double> EaseIn = (from, to, t, ease) => from + (to - from) * ease(t);

        /// <summary>
        /// 指定された補間関数を用いて from から to へイーズアウト補間した値を返します
        /// </summary>
        public static readonly Func<double, double, double, Func<double, double>, double> EaseOut = (from, to, t, ease) => from + (to - from) * (1.0 - ease(1.0 - t));

        /// <summary>
        /// 指定された補間関数を用いて from から to へイーズインアウト補間した値を返します
        /// </summary>
        public static readonly Func<double, double, double, Func<double, double>, double> EaseInOut = (from, to, t, ease) => from + (to - from) * (t < 0.5 ? ease(t * 2.0) * 0.5 : (1.0 - ease((1.0 - t) * 2.0)) * 0.5 + 0.5);

        /// <summary>
        /// 指定された補間関数を用いて from から to へイーズアウトイン補間した値を返します
        /// </summary>
        public static readonly Func<double, double, double, Func<double, double>, double> EaseOutIn = (from, to, t, ease) => from + (to - from) * (t < 0.5 ? (1.0 - ease((1.0 - t - 0.5) * 2.0)) * 0.5 : ease((t - 0.5) * 2.0) * 0.5 + 0.5);

        /// <summary>
        /// 指定された補間関数を用いて from から to へ往復（ピンポン）するイーズイン補間した値を返します
        /// </summary>
        public static readonly Func<double, double, double, Func<double, double>, double> EaseInPingPong = (from, to, t, ease) => from + (to - from) * (t < 0.5 ? ease(t * 2.0) : ease((1.0 - t) * 2.0));

        /// <summary>
        /// 指定された補間関数を用いて from から to へ往復（ピンポン）するイーズアウト補間した値を返します
        /// </summary>
        public static readonly Func<double, double, double, Func<double, double>, double> EaseOutPingPong = (from, to, t, ease) => from + (to - from) * (t < 0.5 ? 1.0 - ease((0.5 - t) * 2.0) : 1.0 - ease(1.0 - (2.0 - t * 2.0)));
    }
}