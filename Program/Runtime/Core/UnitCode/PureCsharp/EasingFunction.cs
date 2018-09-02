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

using System.Collections.Generic;

namespace NineTail.Tween
{
    //! @brief		補間関数のデリゲータです.
    //! @details	NtEasingFunction クラスにある各補間関数と同じシグネチャになっています.
    //! @param[in]	current_time	補間時間の現在時間（秒）.
    //! @param[in]	duration_time	補間する時間（秒）.
    //! @param[in]	start			補間値のスタート値.
    //! @param[in]	end				補間値の目標値.
    //! @retval		float	補間された結果の値を返します.
    public delegate float NtEasing(float current_time, float duration_time, float start, float end);



    //! @brief	補間関数タイプを列挙しています.
    public enum NtEasingType
    {
        Linear,
        QuadOut, QuadIn, QuadInOut, QuadOutIn,
        ExpoOut, ExpoIn, ExpoInOut, ExpoOutIn,
        CubicOut, CubicIn, CubicInOut, CubicOutIn,
        QuartOut, QuartIn, QuartInOut, QuartOutIn,
        QuintOut, QuintIn, QuintInOut, QuintOutIn,
        CircOut, CircIn, CircInOut, CircOutIn,
        SineOut, SineIn, SineInOut, SineOutIn,
        ElasticOut, ElasticIn, ElasticInOut, ElasticOutIn,
        BounceOut, BounceIn, BounceInOut, BounceOutIn,
        BackOut, BackIn, BackInOut, BackOutIn,
    }



    //! @brief	補間関数を収録したクラスです.
    public static class NtEasingFunction
    {
        // 以下静的メンバ変数定義.
        private static readonly Dictionary<NtEasingType, NtEasing> easing_table_;   //!< 補間関数テーブル.



        #region Constructor
        //! @brief	静的コンストラクタです.
        static NtEasingFunction()
        {
            // 補間関数テーブルを初期化する.
            easing_table_ = new Dictionary<NtEasingType, NtEasing>()
        {
            {NtEasingType.Linear, Linear},
            {NtEasingType.QuadOut, QuadOut},{NtEasingType.QuadIn, QuadIn},{NtEasingType.QuadInOut, QuadInOut},{NtEasingType.QuadOutIn, QuadOutIn},
            {NtEasingType.ExpoOut, ExpoOut},{NtEasingType.ExpoIn, ExpoIn},{NtEasingType.ExpoInOut, ExpoInOut},{NtEasingType.ExpoOutIn, ExpoOutIn},
            {NtEasingType.CubicOut, CubicOut},{NtEasingType.CubicIn, CubicIn},{NtEasingType.CubicInOut, CubicInOut},{NtEasingType.CubicOutIn, CubicOutIn},
            {NtEasingType.QuartOut, QuartOut},{NtEasingType.QuartIn, QuartIn},{NtEasingType.QuartInOut, QuartInOut},{NtEasingType.QuartOutIn, QuartOutIn},
            {NtEasingType.QuintOut, QuintOut},{NtEasingType.QuintIn, QuintIn},{NtEasingType.QuintInOut, QuintInOut},{NtEasingType.QuintOutIn, QuintOutIn},
            {NtEasingType.CircOut, CircOut},{NtEasingType.CircIn, CircIn},{NtEasingType.CircInOut, CircInOut},{NtEasingType.CircOutIn, CircOutIn},
            {NtEasingType.SineOut, SineOut},{NtEasingType.SineIn, SineIn},{NtEasingType.SineInOut, SineInOut},{NtEasingType.SineOutIn, SineOutIn},
            {NtEasingType.ElasticOut, ElasticOut},{NtEasingType.ElasticIn, ElasticIn},{NtEasingType.ElasticInOut, ElasticInOut},{NtEasingType.ElasticOutIn, ElasticOutIn},
            {NtEasingType.BounceOut, BounceOut},{NtEasingType.BounceIn, BounceIn},{NtEasingType.BounceInOut, BounceInOut},{NtEasingType.BounceOutIn, BounceOutIn},
            {NtEasingType.BackOut, BackOut},{NtEasingType.BackIn, BackIn},{NtEasingType.BackInOut, BackInOut},{NtEasingType.BackOutIn, BackOutIn},
        };
        }
        #endregion


        #region Generic
        //! @brief		指定された補間関数タイプから補間関数を取得します.
        //! @details	この関数は一度生成された補間関数デリゲータを返すので、デリゲータのインスタンス生成負荷を抑えることが出来ます.
        //! @param[in]	type		取得する補間関数のタイプ.
        //! @retval		NtEasing	取得された補間関数のデリゲータ.
        public static NtEasing GetEasingFunction(NtEasingType type)
        {
            // 対応した補間関数を返す.
            return easing_table_[type];
        }
        #endregion


        #region Easing Functions
        #region Linear
        //! @brief		補間処理を行います.
        //! @details	渡された情報を元に値の補間を行います.
        //! @param[in]	current_time	補間時間の現在時間（秒）.
        //! @param[in]	duration_time	補間する時間（秒）.
        //! @param[in]	start			補間値のスタート値.
        //! @param[in]	end				補間値の目標値.
        //! @retval		float	補間された結果の値を返します.
        public static float Linear(float current_time, float duration_time, float start, float end)
        {
            // 補間した結果を返す.
            return end * current_time / duration_time + start;
        }
        #endregion


        #region Quad
        //! @brief		補間処理を行います.
        //! @details	渡された情報を元に値の補間を行います.
        //! @param[in]	current_time	補間時間の現在時間（秒）.
        //! @param[in]	duration_time	補間する時間（秒）.
        //! @param[in]	start			補間値のスタート値.
        //! @param[in]	end				補間値の目標値.
        //! @retval		float	補間された結果の値を返します.
        public static float QuadOut(float current_time, float duration_time, float start, float end)
        {
            // 補間した結果を返す.
            return -end * (current_time /= duration_time) * (current_time - 2.0f) + start;
        }


        //! @brief		補間処理を行います.
        //! @details	渡された情報を元に値の補間を行います.
        //! @param[in]	current_time	補間時間の現在時間（秒）.
        //! @param[in]	duration_time	補間する時間（秒）.
        //! @param[in]	start			補間値のスタート値.
        //! @param[in]	end				補間値の目標値.
        //! @retval		float	補間された結果の値を返します.
        public static float QuadIn(float current_time, float duration_time, float start, float end)
        {
            // 補間した結果を返す.
            return end * (current_time /= duration_time) * current_time + start;
        }


        //! @brief		補間処理を行います.
        //! @details	渡された情報を元に値の補間を行います.
        //! @param[in]	current_time	補間時間の現在時間（秒）.
        //! @param[in]	duration_time	補間する時間（秒）.
        //! @param[in]	start			補間値のスタート値.
        //! @param[in]	end				補間値の目標値.
        //! @retval		float	補間された結果の値を返します.
        public static float QuadInOut(float current_time, float duration_time, float start, float end)
        {
            // 補間した結果を返す.
            if ((current_time /= duration_time / 2.0f) < 1.0f) return end / 2.0f * current_time * current_time + start;
            return -end / 2.0f * ((--current_time) * (current_time - 2.0f) - 1.0f) + start;
        }


        //! @brief		補間処理を行います.
        //! @details	渡された情報を元に値の補間を行います.
        //! @param[in]	current_time	補間時間の現在時間（秒）.
        //! @param[in]	duration_time	補間する時間（秒）.
        //! @param[in]	start			補間値のスタート値.
        //! @param[in]	end				補間値の目標値.
        //! @retval		float	補間された結果の値を返します.
        public static float QuadOutIn(float current_time, float duration_time, float start, float end)
        {
            // 補間した結果を返す.
            if (current_time < duration_time / 2.0f) return QuadOut(current_time * 2.0f, duration_time, start, end / 2.0f);
            return QuadIn((current_time * 2.0f) - duration_time, duration_time, start + end / 2.0f, end / 2.0f);
        }
        #endregion


        #region Expo
        //! @brief		補間処理を行います.
        //! @details	渡された情報を元に値の補間を行います.
        //! @param[in]	current_time	補間時間の現在時間（秒）.
        //! @param[in]	duration_time	補間する時間（秒）.
        //! @param[in]	start			補間値のスタート値.
        //! @param[in]	end				補間値の目標値.
        //! @retval		float	補間された結果の値を返します.
        public static float ExpoOut(float current_time, float duration_time, float start, float end)
        {
            // 補間した結果を返す.
            return (current_time == duration_time) ? start + end : end * (-UnityEngine.Mathf.Pow(2.0f, -10.0f * current_time / duration_time) + 1.0f) + start;
        }


        //! @brief		補間処理を行います.
        //! @details	渡された情報を元に値の補間を行います.
        //! @param[in]	current_time	補間時間の現在時間（秒）.
        //! @param[in]	duration_time	補間する時間（秒）.
        //! @param[in]	start			補間値のスタート値.
        //! @param[in]	end				補間値の目標値.
        //! @retval		float	補間された結果の値を返します.
        public static float ExpoIn(float current_time, float duration_time, float start, float end)
        {
            // 補間した結果を返す.
            return (current_time == 0.0f) ? start : end * UnityEngine.Mathf.Pow(2.0f, 10.0f * (current_time / duration_time - 1.0f)) + start;
        }


        //! @brief		補間処理を行います.
        //! @details	渡された情報を元に値の補間を行います.
        //! @param[in]	current_time	補間時間の現在時間（秒）.
        //! @param[in]	duration_time	補間する時間（秒）.
        //! @param[in]	start			補間値のスタート値.
        //! @param[in]	end				補間値の目標値.
        //! @retval		float	補間された結果の値を返します.
        public static float ExpoInOut(float current_time, float duration_time, float start, float end)
        {
            // 補間した結果を返す.
            if (current_time == 0.0f) return start;
            if (current_time == duration_time) return start + end;
            if ((current_time /= duration_time / 2.0f) < 1.0f) return end / 2.0f * UnityEngine.Mathf.Pow(2.0f, 10.0f * (current_time - 1.0f)) + start;
            return end / 2.0f * (-UnityEngine.Mathf.Pow(2.0f, -10.0f * --current_time) + 2.0f) + start;
        }


        //! @brief		補間処理を行います.
        //! @details	渡された情報を元に値の補間を行います.
        //! @param[in]	current_time	補間時間の現在時間（秒）.
        //! @param[in]	duration_time	補間する時間（秒）.
        //! @param[in]	start			補間値のスタート値.
        //! @param[in]	end				補間値の目標値.
        //! @retval		float	補間された結果の値を返します.
        public static float ExpoOutIn(float current_time, float duration_time, float start, float end)
        {
            // 補間した結果を返す.
            if (current_time < duration_time / 2.0f) return ExpoOut(current_time * 2.0f, duration_time, start, end / 2.0f);
            return ExpoIn((current_time * 2.0f) - duration_time, duration_time, start + end / 2.0f, end / 2.0f);
        }
        #endregion


        #region Cubic
        //! @brief		補間処理を行います.
        //! @details	渡された情報を元に値の補間を行います.
        //! @param[in]	current_time	補間時間の現在時間（秒）.
        //! @param[in]	duration_time	補間する時間（秒）.
        //! @param[in]	start			補間値のスタート値.
        //! @param[in]	end				補間値の目標値.
        //! @retval		float	補間された結果の値を返します.
        public static float CubicOut(float current_time, float duration_time, float start, float end)
        {
            // 補間した結果を返す.
            return end * ((current_time = current_time / duration_time - 1.0f) * current_time * current_time + 1.0f) + start;
        }


        //! @brief		補間処理を行います.
        //! @details	渡された情報を元に値の補間を行います.
        //! @param[in]	current_time	補間時間の現在時間（秒）.
        //! @param[in]	duration_time	補間する時間（秒）.
        //! @param[in]	start			補間値のスタート値.
        //! @param[in]	end				補間値の目標値.
        //! @retval		float	補間された結果の値を返します.
        public static float CubicIn(float current_time, float duration_time, float start, float end)
        {
            // 補間した結果を返す.
            return end * (current_time /= duration_time) * current_time * current_time + start;
        }


        //! @brief		補間処理を行います.
        //! @details	渡された情報を元に値の補間を行います.
        //! @param[in]	current_time	補間時間の現在時間（秒）.
        //! @param[in]	duration_time	補間する時間（秒）.
        //! @param[in]	start			補間値のスタート値.
        //! @param[in]	end				補間値の目標値.
        //! @retval		float	補間された結果の値を返します.
        public static float CubicInOut(float current_time, float duration_time, float start, float end)
        {
            // 補間した結果を返す.
            if ((current_time /= duration_time / 2.0f) < 1.0f) return end / 2.0f * current_time * current_time * current_time + start;
            return end / 2.0f * ((current_time -= 2.0f) * current_time * current_time + 2.0f) + start;
        }


        //! @brief		補間処理を行います.
        //! @details	渡された情報を元に値の補間を行います.
        //! @param[in]	current_time	補間時間の現在時間（秒）.
        //! @param[in]	duration_time	補間する時間（秒）.
        //! @param[in]	start			補間値のスタート値.
        //! @param[in]	end				補間値の目標値.
        //! @retval		float	補間された結果の値を返します.
        public static float CubicOutIn(float current_time, float duration_time, float start, float end)
        {
            // 補間した結果を返す.
            if (current_time < duration_time / 2.0f) return CubicOut(current_time * 2.0f, duration_time, start, end / 2.0f);
            return CubicIn((current_time * 2.0f) - duration_time, duration_time, start + end / 2.0f, end / 2.0f);
        }
        #endregion


        #region Quart
        //! @brief		補間処理を行います.
        //! @details	渡された情報を元に値の補間を行います.
        //! @param[in]	current_time	補間時間の現在時間（秒）.
        //! @param[in]	duration_time	補間する時間（秒）.
        //! @param[in]	start			補間値のスタート値.
        //! @param[in]	end				補間値の目標値.
        //! @retval		float	補間された結果の値を返します.
        public static float QuartOut(float current_time, float duration_time, float start, float end)
        {
            // 補間した結果を返す.
            return -end * ((current_time = current_time / duration_time - 1.0f) * current_time * current_time * current_time - 1.0f) + start;
        }


        //! @brief		補間処理を行います.
        //! @details	渡された情報を元に値の補間を行います.
        //! @param[in]	current_time	補間時間の現在時間（秒）.
        //! @param[in]	duration_time	補間する時間（秒）.
        //! @param[in]	start			補間値のスタート値.
        //! @param[in]	end				補間値の目標値.
        //! @retval		float	補間された結果の値を返します.
        public static float QuartIn(float current_time, float duration_time, float start, float end)
        {
            // 補間した結果を返す.
            return end * (current_time /= duration_time) * current_time * current_time * current_time + start;
        }


        //! @brief		補間処理を行います.
        //! @details	渡された情報を元に値の補間を行います.
        //! @param[in]	current_time	補間時間の現在時間（秒）.
        //! @param[in]	duration_time	補間する時間（秒）.
        //! @param[in]	start			補間値のスタート値.
        //! @param[in]	end				補間値の目標値.
        //! @retval		float	補間された結果の値を返します.
        public static float QuartInOut(float current_time, float duration_time, float start, float end)
        {
            // 補間した結果を返す.
            if ((current_time /= duration_time / 2.0f) < 1.0f) return end / 2.0f * current_time * current_time * current_time * current_time + start;
            return -end / 2.0f * ((current_time -= 2.0f) * current_time * current_time * current_time - 2.0f) + start;
        }


        //! @brief		補間処理を行います.
        //! @details	渡された情報を元に値の補間を行います.
        //! @param[in]	current_time	補間時間の現在時間（秒）.
        //! @param[in]	duration_time	補間する時間（秒）.
        //! @param[in]	start			補間値のスタート値.
        //! @param[in]	end				補間値の目標値.
        //! @retval		float	補間された結果の値を返します.
        public static float QuartOutIn(float current_time, float duration_time, float start, float end)
        {
            // 補間した結果を返す.
            if (current_time < duration_time / 2.0f) return QuartOut(current_time * 2.0f, start, end / 2.0f, duration_time);
            return QuartIn((current_time * 2.0f) - start, start + end / 2.0f, end / 2.0f, duration_time);
        }
        #endregion


        #region Quint
        //! @brief		補間処理を行います.
        //! @details	渡された情報を元に値の補間を行います.
        //! @param[in]	current_time	補間時間の現在時間（秒）.
        //! @param[in]	duration_time	補間する時間（秒）.
        //! @param[in]	start			補間値のスタート値.
        //! @param[in]	end				補間値の目標値.
        //! @retval		float	補間された結果の値を返します.
        public static float QuintOut(float current_time, float duration_time, float start, float end)
        {
            // 補間した結果を返す.
            return end * ((current_time = current_time / duration_time - 1.0f) * current_time * current_time * current_time * current_time + 1.0f) + start;
        }


        //! @brief		補間処理を行います.
        //! @details	渡された情報を元に値の補間を行います.
        //! @param[in]	current_time	補間時間の現在時間（秒）.
        //! @param[in]	duration_time	補間する時間（秒）.
        //! @param[in]	start			補間値のスタート値.
        //! @param[in]	end				補間値の目標値.
        //! @retval		float	補間された結果の値を返します.
        public static float QuintIn(float current_time, float duration_time, float start, float end)
        {
            // 補間した結果を返す.
            return end * (current_time /= duration_time) * current_time * current_time * current_time * current_time + start;
        }


        //! @brief		補間処理を行います.
        //! @details	渡された情報を元に値の補間を行います.
        //! @param[in]	current_time	補間時間の現在時間（秒）.
        //! @param[in]	duration_time	補間する時間（秒）.
        //! @param[in]	start			補間値のスタート値.
        //! @param[in]	end				補間値の目標値.
        //! @retval		float	補間された結果の値を返します.
        public static float QuintInOut(float current_time, float duration_time, float start, float end)
        {
            // 補間した結果を返す.
            if ((current_time /= duration_time / 2.0f) < 1.0f) return end / 2.0f * current_time * current_time * current_time * current_time * current_time + start;
            return end / 2.0f * ((current_time -= 2.0f) * current_time * current_time * current_time * current_time + 2.0f) + start;
        }


        //! @brief		補間処理を行います.
        //! @details	渡された情報を元に値の補間を行います.
        //! @param[in]	current_time	補間時間の現在時間（秒）.
        //! @param[in]	duration_time	補間する時間（秒）.
        //! @param[in]	start			補間値のスタート値.
        //! @param[in]	end				補間値の目標値.
        //! @retval		float	補間された結果の値を返します.
        public static float QuintOutIn(float current_time, float duration_time, float start, float end)
        {
            // 補間した結果を返す.
            if (current_time < duration_time / 2.0f) return QuintOut(current_time * 2.0f, start, end / 2.0f, duration_time);
            return QuintIn((current_time * 2.0f) - duration_time, start + end / 2.0f, end / 2.0f, duration_time);
        }
        #endregion


        #region Circ
        //! @brief		補間処理を行います.
        //! @details	渡された情報を元に値の補間を行います.
        //! @param[in]	current_time	補間時間の現在時間（秒）.
        //! @param[in]	duration_time	補間する時間（秒）.
        //! @param[in]	start			補間値のスタート値.
        //! @param[in]	end				補間値の目標値.
        //! @retval		float	補間された結果の値を返します.
        public static float CircOut(float current_time, float duration_time, float start, float end)
        {
            // 補間した結果を返す.
            return end * UnityEngine.Mathf.Sqrt(1.0f - (current_time = current_time / duration_time - 1.0f) * current_time) + start;
        }


        //! @brief		補間処理を行います.
        //! @details	渡された情報を元に値の補間を行います.
        //! @param[in]	current_time	補間時間の現在時間（秒）.
        //! @param[in]	duration_time	補間する時間（秒）.
        //! @param[in]	start			補間値のスタート値.
        //! @param[in]	end				補間値の目標値.
        //! @retval		float	補間された結果の値を返します.
        public static float CircIn(float current_time, float duration_time, float start, float end)
        {
            // 補間した結果を返す.
            return -end * (UnityEngine.Mathf.Sqrt(1.0f - (current_time /= duration_time) * current_time) - 1.0f) + start;
        }


        //! @brief		補間処理を行います.
        //! @details	渡された情報を元に値の補間を行います.
        //! @param[in]	current_time	補間時間の現在時間（秒）.
        //! @param[in]	duration_time	補間する時間（秒）.
        //! @param[in]	start			補間値のスタート値.
        //! @param[in]	end				補間値の目標値.
        //! @retval		float	補間された結果の値を返します.
        public static float CircInOut(float current_time, float duration_time, float start, float end)
        {
            // 補間した結果を返す.
            if ((current_time /= duration_time / 2.0f) < 1.0f) return -end / 2.0f * (UnityEngine.Mathf.Sqrt(1.0f - current_time * current_time) - 1.0f) + start;
            return end / 2.0f * (UnityEngine.Mathf.Sqrt(1.0f - (current_time -= 2.0f) * current_time) + 1.0f) + start;
        }


        //! @brief		補間処理を行います.
        //! @details	渡された情報を元に値の補間を行います.
        //! @param[in]	current_time	補間時間の現在時間（秒）.
        //! @param[in]	duration_time	補間する時間（秒）.
        //! @param[in]	start			補間値のスタート値.
        //! @param[in]	end				補間値の目標値.
        //! @retval		float	補間された結果の値を返します.
        public static float CircOutIn(float current_time, float duration_time, float start, float end)
        {
            // 補間した結果を返す.
            if (current_time < duration_time / 2.0f) return CircOut(current_time * 2.0f, duration_time, start, end / 2.0f);
            return CircIn((current_time * 2.0f) - duration_time, duration_time, start + end / 2.0f, end / 2.0f);
        }
        #endregion


        #region Sine
        //! @brief		補間処理を行います.
        //! @details	渡された情報を元に値の補間を行います.
        //! @param[in]	current_time	補間時間の現在時間（秒）.
        //! @param[in]	duration_time	補間する時間（秒）.
        //! @param[in]	start			補間値のスタート値.
        //! @param[in]	end				補間値の目標値.
        //! @retval		float	補間された結果の値を返します.
        public static float SineOut(float current_time, float duration_time, float start, float end)
        {
            // 補間した結果を返す.
            return end * UnityEngine.Mathf.Sin(current_time / duration_time * (UnityEngine.Mathf.PI / 2.0f)) + start;
        }


        //! @brief		補間処理を行います.
        //! @details	渡された情報を元に値の補間を行います.
        //! @param[in]	current_time	補間時間の現在時間（秒）.
        //! @param[in]	duration_time	補間する時間（秒）.
        //! @param[in]	start			補間値のスタート値.
        //! @param[in]	end				補間値の目標値.
        //! @retval		float	補間された結果の値を返します.
        public static float SineIn(float current_time, float duration_time, float start, float end)
        {
            // 補間した結果を返す.
            return -end * UnityEngine.Mathf.Cos(current_time / duration_time * (UnityEngine.Mathf.PI / 2.0f)) + end + start;
        }


        //! @brief		補間処理を行います.
        //! @details	渡された情報を元に値の補間を行います.
        //! @param[in]	current_time	補間時間の現在時間（秒）.
        //! @param[in]	duration_time	補間する時間（秒）.
        //! @param[in]	start			補間値のスタート値.
        //! @param[in]	end				補間値の目標値.
        //! @retval		float	補間された結果の値を返します.
        public static float SineInOut(float current_time, float duration_time, float start, float end)
        {
            // 補間した結果を返す.
            if ((current_time /= duration_time / 2.0f) < 1.0f) return end / 2.0f * (UnityEngine.Mathf.Sin(UnityEngine.Mathf.PI * current_time / 2.0f)) + start;
            return -end / 2.0f * (UnityEngine.Mathf.Cos(UnityEngine.Mathf.PI * --current_time / 2.0f) - 2.0f) + start;
        }


        //! @brief		補間処理を行います.
        //! @details	渡された情報を元に値の補間を行います.
        //! @param[in]	current_time	補間時間の現在時間（秒）.
        //! @param[in]	duration_time	補間する時間（秒）.
        //! @param[in]	start			補間値のスタート値.
        //! @param[in]	end				補間値の目標値.
        //! @retval		float	補間された結果の値を返します.
        public static float SineOutIn(float current_time, float duration_time, float start, float end)
        {
            // 補間した結果を返す.
            if (current_time < duration_time / 2.0f) return SineOut(current_time * 2.0f, duration_time, start, end / 2.0f);
            return SineIn((current_time * 2.0f) - duration_time, duration_time, start + end / 2.0f, end / 2.0f);
        }
        #endregion


        #region Elastic
        //! @brief		補間処理を行います.
        //! @details	渡された情報を元に値の補間を行います.
        //! @param[in]	current_time	補間時間の現在時間（秒）.
        //! @param[in]	duration_time	補間する時間（秒）.
        //! @param[in]	start			補間値のスタート値.
        //! @param[in]	end				補間値の目標値.
        //! @retval		float	補間された結果の値を返します.
        public static float ElasticOut(float current_time, float duration_time, float start, float end)
        {
            // 補間した結果を返す.
            if ((current_time /= duration_time) == 1.0f) return start + end;
            float p = duration_time * 0.3f;
            float s = p / 4.0f;
            return (end * UnityEngine.Mathf.Pow(2.0f, -10.0f * current_time) * UnityEngine.Mathf.Sin((current_time * duration_time - s) * (2.0f * UnityEngine.Mathf.PI) / p) + end + start);
        }


        //! @brief		補間処理を行います.
        //! @details	渡された情報を元に値の補間を行います.
        //! @param[in]	current_time	補間時間の現在時間（秒）.
        //! @param[in]	duration_time	補間する時間（秒）.
        //! @param[in]	start			補間値のスタート値.
        //! @param[in]	end				補間値の目標値.
        //! @retval		float	補間された結果の値を返します.
        public static float ElasticIn(float current_time, float duration_time, float start, float end)
        {
            // 補間した結果を返す.
            if ((current_time /= duration_time) == 1.0f) return start + end;
            float p = duration_time * 0.3f;
            float s = p / 4.0f;
            return -(end * UnityEngine.Mathf.Pow(2.0f, 10.0f * (current_time -= 1.0f)) * UnityEngine.Mathf.Sin((current_time * duration_time - s) * (2.0f * UnityEngine.Mathf.PI) / p)) + start;
        }


        //! @brief		補間処理を行います.
        //! @details	渡された情報を元に値の補間を行います.
        //! @param[in]	current_time	補間時間の現在時間（秒）.
        //! @param[in]	duration_time	補間する時間（秒）.
        //! @param[in]	start			補間値のスタート値.
        //! @param[in]	end				補間値の目標値.
        //! @retval		float	補間された結果の値を返します.
        public static float ElasticInOut(float current_time, float duration_time, float start, float end)
        {
            // 補間した結果を返す.
            if ((current_time /= duration_time / 2.0f) == 2.0f) return start + end;
            float p = duration_time * (0.3f * 1.5f);
            float s = p / 4.0f;
            if (current_time < 1.0f) return -0.5f * (end * UnityEngine.Mathf.Pow(2.0f, 10.0f * (current_time -= 1.0f)) * UnityEngine.Mathf.Sin((current_time * duration_time - s) * (2.0f * UnityEngine.Mathf.PI) / p)) + start;
            return end * UnityEngine.Mathf.Pow(2.0f, -10.0f * (current_time -= 1.0f)) * UnityEngine.Mathf.Sin((current_time * duration_time - s) * (2.0f * UnityEngine.Mathf.PI) / p) * 0.5f + end + start;
        }


        //! @brief		補間処理を行います.
        //! @details	渡された情報を元に値の補間を行います.
        //! @param[in]	current_time	補間時間の現在時間（秒）.
        //! @param[in]	duration_time	補間する時間（秒）.
        //! @param[in]	start			補間値のスタート値.
        //! @param[in]	end				補間値の目標値.
        //! @retval		float	補間された結果の値を返します.
        public static float ElasticOutIn(float current_time, float duration_time, float start, float end)
        {
            // 補間した結果を返す.
            if (current_time < duration_time / 2.0f) return ElasticOut(current_time * 2.0f, duration_time, start, end / 2.0f);
            return ElasticIn((current_time * 2.0f) - duration_time, duration_time, start + end / 2.0f, end / 2.0f);
        }
        #endregion


        #region Bounce
        //! @brief		補間処理を行います.
        //! @details	渡された情報を元に値の補間を行います.
        //! @param[in]	current_time	補間時間の現在時間（秒）.
        //! @param[in]	duration_time	補間する時間（秒）.
        //! @param[in]	start			補間値のスタート値.
        //! @param[in]	end				補間値の目標値.
        //! @retval		float	補間された結果の値を返します.
        public static float BounceOut(float current_time, float duration_time, float start, float end)
        {
            // 補間した結果を返す.
            if ((current_time /= duration_time) < (1.0f / 2.75f)) return end * (7.5625f * current_time * current_time) + start;
            else if (current_time < (2.0f / 2.75f)) return end * (7.5625f * (current_time -= (1.5f / 2.75f)) * current_time + 0.75f) + start;
            else if (current_time < (2.5f / 2.75f)) return end * (7.5625f * (current_time -= (2.25f / 2.75f)) * current_time + 0.9375f) + start;
            else return end * (7.5625f * (current_time -= (2.625f / 2.75f)) * current_time + 0.984375f) + start;
        }


        //! @brief		補間処理を行います.
        //! @details	渡された情報を元に値の補間を行います.
        //! @param[in]	current_time	補間時間の現在時間（秒）.
        //! @param[in]	duration_time	補間する時間（秒）.
        //! @param[in]	start			補間値のスタート値.
        //! @param[in]	end				補間値の目標値.
        //! @retval		float	補間された結果の値を返します.
        public static float BounceIn(float current_time, float duration_time, float start, float end)
        {
            // 補間した結果を返す.
            return end - BounceOut(duration_time - current_time, duration_time, 0.0f, end) + start;
        }


        //! @brief		補間処理を行います.
        //! @details	渡された情報を元に値の補間を行います.
        //! @param[in]	current_time	補間時間の現在時間（秒）.
        //! @param[in]	duration_time	補間する時間（秒）.
        //! @param[in]	start			補間値のスタート値.
        //! @param[in]	end				補間値の目標値.
        //! @retval		float	補間された結果の値を返します.
        public static float BounceInOut(float current_time, float duration_time, float start, float end)
        {
            // 補間した結果を返す.
            if (current_time < duration_time / 2.0f) return BounceIn(current_time * 2.0f, duration_time, 0.0f, end) * 0.5f + start;
            else return BounceOut(current_time * 2.0f - duration_time, duration_time, 0.0f, end) * 0.5f + end * 0.5f + start;
        }


        //! @brief		補間処理を行います.
        //! @details	渡された情報を元に値の補間を行います.
        //! @param[in]	current_time	補間時間の現在時間（秒）.
        //! @param[in]	duration_time	補間する時間（秒）.
        //! @param[in]	start			補間値のスタート値.
        //! @param[in]	end				補間値の目標値.
        //! @retval		float	補間された結果の値を返します.
        public static float BounceOutIn(float current_time, float duration_time, float start, float end)
        {
            // 補間した結果を返す.
            if (current_time < duration_time / 2.0f) return BounceOut(current_time * 2.0f, duration_time, start, end / 2.0f);
            return BounceIn((current_time * 2.0f) - duration_time, duration_time, start + end / 2.0f, end / 2.0f);
        }
        #endregion


        #region Back
        //! @brief		補間処理を行います.
        //! @details	渡された情報を元に値の補間を行います.
        //! @param[in]	current_time	補間時間の現在時間（秒）.
        //! @param[in]	duration_time	補間する時間（秒）.
        //! @param[in]	start			補間値のスタート値.
        //! @param[in]	end				補間値の目標値.
        //! @retval		float	補間された結果の値を返します.
        public static float BackOut(float current_time, float duration_time, float start, float end)
        {
            // 補間した結果を返す.
            return end * ((current_time = current_time / duration_time - 1.0f) * current_time * ((1.70158f + 1.0f) * current_time + 1.70158f) + 1.0f) + start;
        }


        //! @brief		補間処理を行います.
        //! @details	渡された情報を元に値の補間を行います.
        //! @param[in]	current_time	補間時間の現在時間（秒）.
        //! @param[in]	duration_time	補間する時間（秒）.
        //! @param[in]	start			補間値のスタート値.
        //! @param[in]	end				補間値の目標値.
        //! @retval		float	補間された結果の値を返します.
        public static float BackIn(float current_time, float duration_time, float start, float end)
        {
            // 補間した結果を返す.
            return end * (current_time /= duration_time) * current_time * ((1.70158f + 1.0f) * current_time - 1.70158f) + start;
        }


        //! @brief		補間処理を行います.
        //! @details	渡された情報を元に値の補間を行います.
        //! @param[in]	current_time	補間時間の現在時間（秒）.
        //! @param[in]	duration_time	補間する時間（秒）.
        //! @param[in]	start			補間値のスタート値.
        //! @param[in]	end				補間値の目標値.
        //! @retval		float	補間された結果の値を返します.
        public static float BackInOut(float current_time, float duration_time, float start, float end)
        {
            // 補間した結果を返す.
            float s = 1.70158f;
            if ((current_time /= duration_time / 2.0f) < 1.0f) return end / 2.0f * (current_time * current_time * (((s *= (1.525f)) + 1.0f) * current_time - s)) + start;
            return end / 2.0f * ((current_time -= 2.0f) * current_time * (((s *= (1.525f)) + 1.0f) * current_time + s) + 2.0f) + start;
        }


        //! @brief		補間処理を行います.
        //! @details	渡された情報を元に値の補間を行います.
        //! @param[in]	current_time	補間時間の現在時間（秒）.
        //! @param[in]	duration_time	補間する時間（秒）.
        //! @param[in]	start			補間値のスタート値.
        //! @param[in]	end				補間値の目標値.
        //! @retval		float	補間された結果の値を返します.
        public static float BackOutIn(float current_time, float duration_time, float start, float end)
        {
            // 補間した結果を返す.
            if (current_time < duration_time / 2.0f) return BackOut(current_time * 2.0f, start, end / 2.0f, duration_time);
            return BackIn((current_time * 2.0f) - duration_time, start + end / 2.0f, end / 2.0f, duration_time);
        }
        #endregion
        #endregion
    }
}