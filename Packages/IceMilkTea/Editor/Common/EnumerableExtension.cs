// zlib/libpng License
//
// Copyright (c) 2019 Sinoa
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
using System.Collections;
using System.Collections.Generic;
using UnityEditor;


namespace IceMilkTeaEditor.Common
{
    /// <summary>
    /// IEnumerable の拡張関数実装用クラスです
    /// </summary>
    public static class ImtEnumerableExtensions
    {
        /// <summary>
        /// DisplayProgress を実行した際にダイアログに表示するパラメータを持つクラスです
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        public class ProgressWindowParameter<TSource>
        {
            /// <summary>
            /// 要素の最大値
            /// </summary>
            public int Max { get; set; }


            /// <summary>
            /// 処理した数
            /// </summary>
            public int Count { get; set; }


            /// <summary>
            /// 列挙されたオブジェクト
            /// </summary>
            public TSource Item { get; set; }


            /// <summary>
            /// プログレスダイアログに表示するタイトル
            /// </summary>
            public string Title { get; set; }


            /// <summary>
            /// プログレスダイアログに表示するテキスト
            /// </summary>
            public string Text { get; set; }
        }



        /// <summary>
        /// sources から要素数を取得できる場合は要素数を取得します
        /// </summary>
        /// <typeparam name="TSource">要素の型</typeparam>
        /// <param name="sources">要素数を取得する元になる Enumerable</param>
        /// <returns>要素数を取得できた場合は要素数を返しますが、取得できなかった場合は -1 を返します</returns>
        private static int GetCount<TSource>(IEnumerable<TSource> sources)
        {
            // 配列なら
            if (sources is Array)
            {
                // 長さを返す
                return ((Array)sources).Length;
            }
            else if (sources is IList)
            {
                // リストなら数を返す
                return ((IList)sources).Count;
            }
            else if (sources is IList<TSource>)
            {
                // リストなら数を返す
                return ((IList)sources).Count;
            }


            // どれもすぐに長さを取得できないなら-1を返す
            return -1;
        }



        /// <summary>
        /// 列挙しながらプログレスダイアログを表示します
        /// </summary>
        /// <typeparam name="TSource">列挙する要素の型</typeparam>
        /// <param name="sources">列挙できるオブジェクトを持っている Enumerable</param>
        /// <param name="callback">プログレスダイアログに表示するべき内容を設定するためのコールバック</param>
        /// <returns>列挙中の列挙可能オブジェクトを返します</returns>
        /// <exception cref="ArgumentNullException">sources が null です</exception>
        /// <exception cref="ArgumentNullException">callback が null です</exception>
        public static IEnumerable<TSource> DisplayProgress<TSource>(this IEnumerable<TSource> sources, Action<ProgressWindowParameter<TSource>> callback)
        {
            // 必要な変数を宣言
            var parameter = new ProgressWindowParameter<TSource>();
            var max = GetCount(sources);
            var count = 0;


            // コールバックが null なら
            if (callback == null)
            {
                // 何をするのか
                throw new ArgumentNullException(nameof(callback));
            }


            // 要素をすべて回るための
            using (var enumerator = (sources ?? throw new ArgumentNullException(nameof(sources))).GetEnumerator())
            {
                // すべて回る
                while (enumerator.MoveNext())
                {
                    // パラメータを設定してコールバックを呼ぶ
                    parameter.Max = max;
                    parameter.Count = ++count;
                    parameter.Title = string.Empty;
                    parameter.Text = string.Empty;
                    parameter.Item = enumerator.Current;
                    callback(parameter);


                    // ダイアログを表示
                    var title = parameter.Title ?? string.Empty;
                    var text = parameter.Text ?? string.Empty;
                    var progress = parameter.Max < 0 ? 1.0f : parameter.Count / (float)parameter.Max;
                    var cancel = EditorUtility.DisplayCancelableProgressBar(title, text, progress);


                    // キャンセルされたらループから抜ける
                    if (cancel) break;


                    // 要素を一時返却
                    yield return enumerator.Current;
                }
            }


            // プログレスダイアログを非表示
            EditorUtility.ClearProgressBar();
        }
    }
}