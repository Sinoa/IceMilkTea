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
    /// 様々な補間関数を収録したクラスです
    /// </summary>
    public static class EasingFunctions
    {
        public static double Linear(double currentTime, double durationTime, double from, double to)
        {
            return to * currentTime / durationTime + from;
        }


        public static double QuadOut(double currentTime, double durationTime, double from, double to)
        {
            return -to * (currentTime /= durationTime) * (currentTime - 2.0) + from;
        }


        public static double QuadIn(double currentTime, double durationTime, double from, double to)
        {
            return to * (currentTime /= durationTime) * currentTime + from;
        }


        public static double QuadInOut(double currentTime, double durationTime, double from, double to)
        {
            if ((currentTime /= durationTime / 2.0) < 1.0) return to / 2.0 * currentTime * currentTime + from;
            return -to / 2.0 * ((--currentTime) * (currentTime - 2.0) - 1.0) + from;
        }


        public static double QuadOutIn(double currentTime, double durationTime, double from, double to)
        {
            if (currentTime < durationTime / 2.0) return QuadOut(currentTime * 2.0, durationTime, from, to / 2.0);
            return QuadIn((currentTime * 2.0) - durationTime, durationTime, from + to / 2.0, to / 2.0);
        }


        public static double ExpoOut(double currentTime, double durationTime, double from, double to)
        {
            return (currentTime == durationTime) ? from + to : to * (-System.Math.Pow(2.0, -10.0 * currentTime / durationTime) + 1.0) + from;
        }


        public static double ExpoIn(double currentTime, double durationTime, double from, double to)
        {
            return (currentTime == 0.0) ? from : to * System.Math.Pow(2.0, 10.0 * (currentTime / durationTime - 1.0)) + from;
        }


        public static double ExpoInOut(double currentTime, double durationTime, double from, double to)
        {
            if (currentTime == 0.0) return from;
            if (currentTime == durationTime) return from + to;
            if ((currentTime /= durationTime / 2.0) < 1.0) return to / 2.0 * System.Math.Pow(2.0, 10.0 * (currentTime - 1.0)) + from;
            return to / 2.0 * (-System.Math.Pow(2.0, -10.0 * --currentTime) + 2.0) + from;
        }


        public static double ExpoOutIn(double currentTime, double durationTime, double from, double to)
        {
            if (currentTime < durationTime / 2.0) return ExpoOut(currentTime * 2.0, durationTime, from, to / 2.0);
            return ExpoIn((currentTime * 2.0) - durationTime, durationTime, from + to / 2.0, to / 2.0);
        }


        public static double CubicOut(double currentTime, double durationTime, double from, double to)
        {
            return to * ((currentTime = currentTime / durationTime - 1.0) * currentTime * currentTime + 1.0) + from;
        }


        public static double CubicIn(double currentTime, double durationTime, double from, double to)
        {
            return to * (currentTime /= durationTime) * currentTime * currentTime + from;
        }


        public static double CubicInOut(double currentTime, double durationTime, double from, double to)
        {
            if ((currentTime /= durationTime / 2.0) < 1.0) return to / 2.0 * currentTime * currentTime * currentTime + from;
            return to / 2.0 * ((currentTime -= 2.0) * currentTime * currentTime + 2.0) + from;
        }


        public static double CubicOutIn(double currentTime, double durationTime, double from, double to)
        {
            if (currentTime < durationTime / 2.0) return CubicOut(currentTime * 2.0, durationTime, from, to / 2.0);
            return CubicIn((currentTime * 2.0) - durationTime, durationTime, from + to / 2.0, to / 2.0);
        }


        public static double QuartOut(double currentTime, double durationTime, double from, double to)
        {
            return -to * ((currentTime = currentTime / durationTime - 1.0) * currentTime * currentTime * currentTime - 1.0) + from;
        }


        public static double QuartIn(double currentTime, double durationTime, double from, double to)
        {
            return to * (currentTime /= durationTime) * currentTime * currentTime * currentTime + from;
        }


        public static double QuartInOut(double currentTime, double durationTime, double from, double to)
        {
            if ((currentTime /= durationTime / 2.0) < 1.0) return to / 2.0 * currentTime * currentTime * currentTime * currentTime + from;
            return -to / 2.0 * ((currentTime -= 2.0) * currentTime * currentTime * currentTime - 2.0) + from;
        }


        public static double QuartOutIn(double currentTime, double durationTime, double from, double to)
        {
            if (currentTime < durationTime / 2.0) return QuartOut(currentTime * 2.0, from, to / 2.0, durationTime);
            return QuartIn((currentTime * 2.0) - from, from + to / 2.0, to / 2.0, durationTime);
        }


        public static double QuintOut(double currentTime, double durationTime, double from, double to)
        {
            return to * ((currentTime = currentTime / durationTime - 1.0) * currentTime * currentTime * currentTime * currentTime + 1.0) + from;
        }


        public static double QuintIn(double currentTime, double durationTime, double from, double to)
        {
            return to * (currentTime /= durationTime) * currentTime * currentTime * currentTime * currentTime + from;
        }


        public static double QuintInOut(double currentTime, double durationTime, double from, double to)
        {
            if ((currentTime /= durationTime / 2.0) < 1.0) return to / 2.0 * currentTime * currentTime * currentTime * currentTime * currentTime + from;
            return to / 2.0 * ((currentTime -= 2.0) * currentTime * currentTime * currentTime * currentTime + 2.0) + from;
        }


        public static double QuintOutIn(double currentTime, double durationTime, double from, double to)
        {
            if (currentTime < durationTime / 2.0) return QuintOut(currentTime * 2.0, from, to / 2.0, durationTime);
            return QuintIn((currentTime * 2.0) - durationTime, from + to / 2.0, to / 2.0, durationTime);
        }


        public static double CircOut(double currentTime, double durationTime, double from, double to)
        {
            return to * System.Math.Sqrt(1.0 - (currentTime = currentTime / durationTime - 1.0) * currentTime) + from;
        }


        public static double CircIn(double currentTime, double durationTime, double from, double to)
        {
            return -to * (System.Math.Sqrt(1.0 - (currentTime /= durationTime) * currentTime) - 1.0) + from;
        }


        public static double CircInOut(double currentTime, double durationTime, double from, double to)
        {
            if ((currentTime /= durationTime / 2.0) < 1.0) return -to / 2.0 * (System.Math.Sqrt(1.0 - currentTime * currentTime) - 1.0) + from;
            return to / 2.0 * (System.Math.Sqrt(1.0 - (currentTime -= 2.0) * currentTime) + 1.0) + from;
        }


        public static double CircOutIn(double currentTime, double durationTime, double from, double to)
        {
            if (currentTime < durationTime / 2.0) return CircOut(currentTime * 2.0, durationTime, from, to / 2.0);
            return CircIn((currentTime * 2.0) - durationTime, durationTime, from + to / 2.0, to / 2.0);
        }


        public static double SineOut(double currentTime, double durationTime, double from, double to)
        {
            return to * System.Math.Sin(currentTime / durationTime * (System.Math.PI / 2.0)) + from;
        }


        public static double SineIn(double currentTime, double durationTime, double from, double to)
        {
            return -to * System.Math.Cos(currentTime / durationTime * (System.Math.PI / 2.0)) + to + from;
        }


        public static double SineInOut(double currentTime, double durationTime, double from, double to)
        {
            if ((currentTime /= durationTime / 2.0) < 1.0) return to / 2.0 * (System.Math.Sin(System.Math.PI * currentTime / 2.0)) + from;
            return -to / 2.0 * (System.Math.Cos(System.Math.PI * --currentTime / 2.0) - 2.0) + from;
        }


        public static double SineOutIn(double currentTime, double durationTime, double from, double to)
        {
            if (currentTime < durationTime / 2.0) return SineOut(currentTime * 2.0, durationTime, from, to / 2.0);
            return SineIn((currentTime * 2.0) - durationTime, durationTime, from + to / 2.0, to / 2.0);
        }


        public static double ElasticOut(double currentTime, double durationTime, double from, double to)
        {
            if ((currentTime /= durationTime) == 1.0) return from + to;
            double p = durationTime * 0.3;
            double s = p / 4.0;
            return (to * System.Math.Pow(2.0, -10.0 * currentTime) * System.Math.Sin((currentTime * durationTime - s) * (2.0 * System.Math.PI) / p) + to + from);
        }


        public static double ElasticIn(double currentTime, double durationTime, double from, double to)
        {
            if ((currentTime /= durationTime) == 1.0) return from + to;
            double p = durationTime * 0.3;
            double s = p / 4.0;
            return -(to * System.Math.Pow(2.0, 10.0 * (currentTime -= 1.0)) * System.Math.Sin((currentTime * durationTime - s) * (2.0 * System.Math.PI) / p)) + from;
        }


        public static double ElasticInOut(double currentTime, double durationTime, double from, double to)
        {
            if ((currentTime /= durationTime / 2.0) == 2.0) return from + to;
            double p = durationTime * (0.3 * 1.5);
            double s = p / 4.0;
            if (currentTime < 1.0) return -0.5 * (to * System.Math.Pow(2.0, 10.0 * (currentTime -= 1.0)) * System.Math.Sin((currentTime * durationTime - s) * (2.0 * System.Math.PI) / p)) + from;
            return to * System.Math.Pow(2.0, -10.0 * (currentTime -= 1.0)) * System.Math.Sin((currentTime * durationTime - s) * (2.0 * System.Math.PI) / p) * 0.5 + to + from;
        }


        public static double ElasticOutIn(double currentTime, double durationTime, double from, double to)
        {
            if (currentTime < durationTime / 2.0) return ElasticOut(currentTime * 2.0, durationTime, from, to / 2.0);
            return ElasticIn((currentTime * 2.0) - durationTime, durationTime, from + to / 2.0, to / 2.0);
        }


        public static double BounceOut(double currentTime, double durationTime, double from, double to)
        {
            if ((currentTime /= durationTime) < (1.0 / 2.75)) return to * (7.5625 * currentTime * currentTime) + from;
            else if (currentTime < (2.0 / 2.75)) return to * (7.5625 * (currentTime -= (1.5 / 2.75)) * currentTime + 0.75) + from;
            else if (currentTime < (2.5 / 2.75)) return to * (7.5625 * (currentTime -= (2.25 / 2.75)) * currentTime + 0.9375) + from;
            else return to * (7.5625 * (currentTime -= (2.625 / 2.75)) * currentTime + 0.984375) + from;
        }


        public static double BounceIn(double currentTime, double durationTime, double from, double to)
        {
            return to - BounceOut(durationTime - currentTime, durationTime, 0.0, to) + from;
        }


        public static double BounceInOut(double currentTime, double durationTime, double from, double to)
        {
            if (currentTime < durationTime / 2.0) return BounceIn(currentTime * 2.0, durationTime, 0.0, to) * 0.5 + from;
            else return BounceOut(currentTime * 2.0 - durationTime, durationTime, 0.0, to) * 0.5 + to * 0.5 + from;
        }


        public static double BounceOutIn(double currentTime, double durationTime, double from, double to)
        {
            if (currentTime < durationTime / 2.0) return BounceOut(currentTime * 2.0, durationTime, from, to / 2.0);
            return BounceIn((currentTime * 2.0) - durationTime, durationTime, from + to / 2.0, to / 2.0);
        }


        public static double BackOut(double currentTime, double durationTime, double from, double to)
        {
            return to * ((currentTime = currentTime / durationTime - 1.0) * currentTime * ((1.70158 + 1.0) * currentTime + 1.70158) + 1.0) + from;
        }


        public static double BackIn(double currentTime, double durationTime, double from, double to)
        {
            return to * (currentTime /= durationTime) * currentTime * ((1.70158 + 1.0) * currentTime - 1.70158) + from;
        }


        public static double BackInOut(double currentTime, double durationTime, double from, double to)
        {
            double s = 1.70158;
            if ((currentTime /= durationTime / 2.0) < 1.0) return to / 2.0 * (currentTime * currentTime * (((s *= (1.525)) + 1.0) * currentTime - s)) + from;
            return to / 2.0 * ((currentTime -= 2.0) * currentTime * (((s *= (1.525)) + 1.0) * currentTime + s) + 2.0) + from;
        }


        public static double BackOutIn(double currentTime, double durationTime, double from, double to)
        {
            if (currentTime < durationTime / 2.0) return BackOut(currentTime * 2.0, from, to / 2.0, durationTime);
            return BackIn((currentTime * 2.0) - durationTime, from + to / 2.0, to / 2.0, durationTime);
        }
    }
}