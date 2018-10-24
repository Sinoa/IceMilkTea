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

using UnityEngine.Events;
using UnityEngine.UI;

namespace IceMilkTea.Core
{
    /// <summary>
    /// UGUIで提供されているButtonの拡張関数実装用クラスです
    /// </summary>
    public static class UguiButtonExtensions
    {
        /// <summary>
        /// ボタンのクリックイベントの待機が出来るオブジェクトを取得します
        /// </summary>
        /// <typeparam name="T">ボタンクリック時にボタンに付随させる追加のデータの型</typeparam>
        /// <param name="button">クリックイベントの待機をしたいボタン</param>
        /// <returns>クリックイベントを待機するオブジェクトを返します</returns>
        public static ImtAwaitableFromEvent<UnityAction, AwaitButtonClickResult<T>> ToAwaitableClick<T>(this Button button)
        {
            // ユーザーデータを規定値かつ自動リセットとして、イベントの待機可能インスタンスを返す
            return ToAwaitableClick(button, default(T), true);
        }


        /// <summary>
        /// ボタンのクリックイベントの待機が出来るオブジェクトを取得します
        /// </summary>
        /// <typeparam name="T">ボタンクリック時にボタンに付随させる追加のデータの型</typeparam>
        /// <param name="button">クリックイベントの待機をしたいボタン</param>
        /// <param name="userData">ボタンクリック結果に付随させる自由なユーザーデータ</param>
        /// <returns>クリックイベントを待機するオブジェクトを返します</returns>
        public static ImtAwaitableFromEvent<UnityAction, AwaitButtonClickResult<T>> ToAwaitableClick<T>(this Button button, T userData)
        {
            // 自動リセットとして、イベントの待機可能インスタンスを返す
            return ToAwaitableClick(button, userData, true);
        }


        /// <summary>
        /// ボタンのクリックイベントの待機が出来るオブジェクトを取得します
        /// </summary>
        /// <typeparam name="T">ボタンクリック時にボタンに付随させる追加のデータの型</typeparam>
        /// <param name="button">クリックイベントの待機をしたいボタン</param>
        /// <param name="userData">ボタンクリック結果に付随させる自由なユーザーデータ</param>
        /// <param name="autoReset">ボタンイベントが発生した後、待機可能オブジェクトが完了状態をリセットする場合は true を、そのままにする場合は false</param>
        /// <returns>クリックイベントを待機するオブジェクトを返します</returns>
        public static ImtAwaitableFromEvent<UnityAction, AwaitButtonClickResult<T>> ToAwaitableClick<T>(this Button button, T userData, bool autoReset)
        {
            // 結果返却用データのインスタンスを用意
            var result = new AwaitButtonClickResult<T>(button, userData);


            // イベント機構から待機可能クラスのインスタンスとして生成して返す
            return new ImtAwaitableFromEvent<UnityAction, AwaitButtonClickResult<T>>(
                null, autoReset,
                x => () => x(result),
                x => button.onClick.AddListener(x),
                x => button.onClick.RemoveListener(x));
        }
    }



    /// <summary>
    /// 待機可能なボタンクリックイベントの結果を扱うためのクラスです
    /// </summary>
    /// <typeparam name="T">ボタンクリックイベントの結果として扱う、ユーザーの任意のデータ型</typeparam>
    public class AwaitButtonClickResult<T>
    {
        /// <summary>
        /// イベントを発生させたボタン
        /// </summary>
        public Button Button { get; private set; }


        /// <summary>
        /// イベントを発生させたボタンに付随するユーザーデータ
        /// </summary>
        public T UserData { get; private set; }



        /// <summary>
        /// AwaitButtonClickResult のインスタンスを初期化します
        /// </summary>
        /// <param name="button">イベントを発生させるボタン</param>
        /// <param name="userData">ユーザーデータ</param>
        public AwaitButtonClickResult(Button button, T userData)
        {
            // 受け取る
            Button = button;
            UserData = userData;
        }
    }
}