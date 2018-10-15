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
        public static float Linear(float currentTime, float durationTime, float from, float to)
        {
            return to * currentTime / durationTime + from;
        }


        public static float QuadOut(float currentTime, float durationTime, float from, float to)
        {
            return -to * (currentTime /= durationTime) * (currentTime - 2.0f) + from;
        }


        public static float QuadIn(float currentTime, float durationTime, float from, float to)
        {
            return to * (currentTime /= durationTime) * currentTime + from;
        }


        public static float QuadInOut(float currentTime, float durationTime, float from, float to)
        {
            if ((currentTime /= durationTime / 2.0f) < 1.0f) return to / 2.0f * currentTime * currentTime + from;
            return -to / 2.0f * ((--currentTime) * (currentTime - 2.0f) - 1.0f) + from;
        }


        public static float QuadOutIn(float currentTime, float durationTime, float from, float to)
        {
            if (currentTime < durationTime / 2.0f) return QuadOut(currentTime * 2.0f, durationTime, from, to / 2.0f);
            return QuadIn((currentTime * 2.0f) - durationTime, durationTime, from + to / 2.0f, to / 2.0f);
        }


        public static float ExpoOut(float currentTime, float durationTime, float from, float to)
        {
            return (currentTime == durationTime) ? from + to : to * (-UnityEngine.Mathf.Pow(2.0f, -10.0f * currentTime / durationTime) + 1.0f) + from;
        }


        public static float ExpoIn(float currentTime, float durationTime, float from, float to)
        {
            return (currentTime == 0.0f) ? from : to * UnityEngine.Mathf.Pow(2.0f, 10.0f * (currentTime / durationTime - 1.0f)) + from;
        }


        public static float ExpoInOut(float currentTime, float durationTime, float from, float to)
        {
            if (currentTime == 0.0f) return from;
            if (currentTime == durationTime) return from + to;
            if ((currentTime /= durationTime / 2.0f) < 1.0f) return to / 2.0f * UnityEngine.Mathf.Pow(2.0f, 10.0f * (currentTime - 1.0f)) + from;
            return to / 2.0f * (-UnityEngine.Mathf.Pow(2.0f, -10.0f * --currentTime) + 2.0f) + from;
        }


        public static float ExpoOutIn(float currentTime, float durationTime, float from, float to)
        {
            if (currentTime < durationTime / 2.0f) return ExpoOut(currentTime * 2.0f, durationTime, from, to / 2.0f);
            return ExpoIn((currentTime * 2.0f) - durationTime, durationTime, from + to / 2.0f, to / 2.0f);
        }


        public static float CubicOut(float currentTime, float durationTime, float from, float to)
        {
            return to * ((currentTime = currentTime / durationTime - 1.0f) * currentTime * currentTime + 1.0f) + from;
        }


        public static float CubicIn(float currentTime, float durationTime, float from, float to)
        {
            return to * (currentTime /= durationTime) * currentTime * currentTime + from;
        }


        public static float CubicInOut(float currentTime, float durationTime, float from, float to)
        {
            if ((currentTime /= durationTime / 2.0f) < 1.0f) return to / 2.0f * currentTime * currentTime * currentTime + from;
            return to / 2.0f * ((currentTime -= 2.0f) * currentTime * currentTime + 2.0f) + from;
        }


        public static float CubicOutIn(float currentTime, float durationTime, float from, float to)
        {
            if (currentTime < durationTime / 2.0f) return CubicOut(currentTime * 2.0f, durationTime, from, to / 2.0f);
            return CubicIn((currentTime * 2.0f) - durationTime, durationTime, from + to / 2.0f, to / 2.0f);
        }


        public static float QuartOut(float currentTime, float durationTime, float from, float to)
        {
            return -to * ((currentTime = currentTime / durationTime - 1.0f) * currentTime * currentTime * currentTime - 1.0f) + from;
        }


        public static float QuartIn(float currentTime, float durationTime, float from, float to)
        {
            return to * (currentTime /= durationTime) * currentTime * currentTime * currentTime + from;
        }


        public static float QuartInOut(float currentTime, float durationTime, float from, float to)
        {
            if ((currentTime /= durationTime / 2.0f) < 1.0f) return to / 2.0f * currentTime * currentTime * currentTime * currentTime + from;
            return -to / 2.0f * ((currentTime -= 2.0f) * currentTime * currentTime * currentTime - 2.0f) + from;
        }


        public static float QuartOutIn(float currentTime, float durationTime, float from, float to)
        {
            if (currentTime < durationTime / 2.0f) return QuartOut(currentTime * 2.0f, from, to / 2.0f, durationTime);
            return QuartIn((currentTime * 2.0f) - from, from + to / 2.0f, to / 2.0f, durationTime);
        }


        public static float QuintOut(float currentTime, float durationTime, float from, float to)
        {
            return to * ((currentTime = currentTime / durationTime - 1.0f) * currentTime * currentTime * currentTime * currentTime + 1.0f) + from;
        }


        public static float QuintIn(float currentTime, float durationTime, float from, float to)
        {
            return to * (currentTime /= durationTime) * currentTime * currentTime * currentTime * currentTime + from;
        }


        public static float QuintInOut(float currentTime, float durationTime, float from, float to)
        {
            if ((currentTime /= durationTime / 2.0f) < 1.0f) return to / 2.0f * currentTime * currentTime * currentTime * currentTime * currentTime + from;
            return to / 2.0f * ((currentTime -= 2.0f) * currentTime * currentTime * currentTime * currentTime + 2.0f) + from;
        }


        public static float QuintOutIn(float currentTime, float durationTime, float from, float to)
        {
            if (currentTime < durationTime / 2.0f) return QuintOut(currentTime * 2.0f, from, to / 2.0f, durationTime);
            return QuintIn((currentTime * 2.0f) - durationTime, from + to / 2.0f, to / 2.0f, durationTime);
        }


        public static float CircOut(float currentTime, float durationTime, float from, float to)
        {
            return to * UnityEngine.Mathf.Sqrt(1.0f - (currentTime = currentTime / durationTime - 1.0f) * currentTime) + from;
        }


        public static float CircIn(float currentTime, float durationTime, float from, float to)
        {
            return -to * (UnityEngine.Mathf.Sqrt(1.0f - (currentTime /= durationTime) * currentTime) - 1.0f) + from;
        }


        public static float CircInOut(float currentTime, float durationTime, float from, float to)
        {
            if ((currentTime /= durationTime / 2.0f) < 1.0f) return -to / 2.0f * (UnityEngine.Mathf.Sqrt(1.0f - currentTime * currentTime) - 1.0f) + from;
            return to / 2.0f * (UnityEngine.Mathf.Sqrt(1.0f - (currentTime -= 2.0f) * currentTime) + 1.0f) + from;
        }


        public static float CircOutIn(float currentTime, float durationTime, float from, float to)
        {
            if (currentTime < durationTime / 2.0f) return CircOut(currentTime * 2.0f, durationTime, from, to / 2.0f);
            return CircIn((currentTime * 2.0f) - durationTime, durationTime, from + to / 2.0f, to / 2.0f);
        }


        public static float SineOut(float currentTime, float durationTime, float from, float to)
        {
            return to * UnityEngine.Mathf.Sin(currentTime / durationTime * (UnityEngine.Mathf.PI / 2.0f)) + from;
        }


        public static float SineIn(float currentTime, float durationTime, float from, float to)
        {
            return -to * UnityEngine.Mathf.Cos(currentTime / durationTime * (UnityEngine.Mathf.PI / 2.0f)) + to + from;
        }


        public static float SineInOut(float currentTime, float durationTime, float from, float to)
        {
            if ((currentTime /= durationTime / 2.0f) < 1.0f) return to / 2.0f * (UnityEngine.Mathf.Sin(UnityEngine.Mathf.PI * currentTime / 2.0f)) + from;
            return -to / 2.0f * (UnityEngine.Mathf.Cos(UnityEngine.Mathf.PI * --currentTime / 2.0f) - 2.0f) + from;
        }


        public static float SineOutIn(float currentTime, float durationTime, float from, float to)
        {
            if (currentTime < durationTime / 2.0f) return SineOut(currentTime * 2.0f, durationTime, from, to / 2.0f);
            return SineIn((currentTime * 2.0f) - durationTime, durationTime, from + to / 2.0f, to / 2.0f);
        }


        public static float ElasticOut(float currentTime, float durationTime, float from, float to)
        {
            if ((currentTime /= durationTime) == 1.0f) return from + to;
            float p = durationTime * 0.3f;
            float s = p / 4.0f;
            return (to * UnityEngine.Mathf.Pow(2.0f, -10.0f * currentTime) * UnityEngine.Mathf.Sin((currentTime * durationTime - s) * (2.0f * UnityEngine.Mathf.PI) / p) + to + from);
        }


        public static float ElasticIn(float currentTime, float durationTime, float from, float to)
        {
            if ((currentTime /= durationTime) == 1.0f) return from + to;
            float p = durationTime * 0.3f;
            float s = p / 4.0f;
            return -(to * UnityEngine.Mathf.Pow(2.0f, 10.0f * (currentTime -= 1.0f)) * UnityEngine.Mathf.Sin((currentTime * durationTime - s) * (2.0f * UnityEngine.Mathf.PI) / p)) + from;
        }


        public static float ElasticInOut(float currentTime, float durationTime, float from, float to)
        {
            if ((currentTime /= durationTime / 2.0f) == 2.0f) return from + to;
            float p = durationTime * (0.3f * 1.5f);
            float s = p / 4.0f;
            if (currentTime < 1.0f) return -0.5f * (to * UnityEngine.Mathf.Pow(2.0f, 10.0f * (currentTime -= 1.0f)) * UnityEngine.Mathf.Sin((currentTime * durationTime - s) * (2.0f * UnityEngine.Mathf.PI) / p)) + from;
            return to * UnityEngine.Mathf.Pow(2.0f, -10.0f * (currentTime -= 1.0f)) * UnityEngine.Mathf.Sin((currentTime * durationTime - s) * (2.0f * UnityEngine.Mathf.PI) / p) * 0.5f + to + from;
        }


        public static float ElasticOutIn(float currentTime, float durationTime, float from, float to)
        {
            if (currentTime < durationTime / 2.0f) return ElasticOut(currentTime * 2.0f, durationTime, from, to / 2.0f);
            return ElasticIn((currentTime * 2.0f) - durationTime, durationTime, from + to / 2.0f, to / 2.0f);
        }


        public static float BounceOut(float currentTime, float durationTime, float from, float to)
        {
            if ((currentTime /= durationTime) < (1.0f / 2.75f)) return to * (7.5625f * currentTime * currentTime) + from;
            else if (currentTime < (2.0f / 2.75f)) return to * (7.5625f * (currentTime -= (1.5f / 2.75f)) * currentTime + 0.75f) + from;
            else if (currentTime < (2.5f / 2.75f)) return to * (7.5625f * (currentTime -= (2.25f / 2.75f)) * currentTime + 0.9375f) + from;
            else return to * (7.5625f * (currentTime -= (2.625f / 2.75f)) * currentTime + 0.984375f) + from;
        }


        public static float BounceIn(float currentTime, float durationTime, float from, float to)
        {
            return to - BounceOut(durationTime - currentTime, durationTime, 0.0f, to) + from;
        }


        public static float BounceInOut(float currentTime, float durationTime, float from, float to)
        {
            if (currentTime < durationTime / 2.0f) return BounceIn(currentTime * 2.0f, durationTime, 0.0f, to) * 0.5f + from;
            else return BounceOut(currentTime * 2.0f - durationTime, durationTime, 0.0f, to) * 0.5f + to * 0.5f + from;
        }


        public static float BounceOutIn(float currentTime, float durationTime, float from, float to)
        {
            if (currentTime < durationTime / 2.0f) return BounceOut(currentTime * 2.0f, durationTime, from, to / 2.0f);
            return BounceIn((currentTime * 2.0f) - durationTime, durationTime, from + to / 2.0f, to / 2.0f);
        }


        public static float BackOut(float currentTime, float durationTime, float from, float to)
        {
            return to * ((currentTime = currentTime / durationTime - 1.0f) * currentTime * ((1.70158f + 1.0f) * currentTime + 1.70158f) + 1.0f) + from;
        }


        public static float BackIn(float currentTime, float durationTime, float from, float to)
        {
            return to * (currentTime /= durationTime) * currentTime * ((1.70158f + 1.0f) * currentTime - 1.70158f) + from;
        }


        public static float BackInOut(float currentTime, float durationTime, float from, float to)
        {
            float s = 1.70158f;
            if ((currentTime /= durationTime / 2.0f) < 1.0f) return to / 2.0f * (currentTime * currentTime * (((s *= (1.525f)) + 1.0f) * currentTime - s)) + from;
            return to / 2.0f * ((currentTime -= 2.0f) * currentTime * (((s *= (1.525f)) + 1.0f) * currentTime + s) + 2.0f) + from;
        }


        public static float BackOutIn(float currentTime, float durationTime, float from, float to)
        {
            if (currentTime < durationTime / 2.0f) return BackOut(currentTime * 2.0f, from, to / 2.0f, durationTime);
            return BackIn((currentTime * 2.0f) - durationTime, from + to / 2.0f, to / 2.0f, durationTime);
        }
    }
}