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

namespace IceMilkTea.Profiler
{
    /// <summary>
    /// パフォーマンスプローブから結果をフェッチした情報を保持する抽象クラスです。
    /// パフォーマンスプローブを実装した場合、合わせて計測結果を保持するためにこのクラスを継承し実現してください。
    /// </summary>
    public abstract class ProfileFetchResult
    {
        /// <summary>
        /// このプロファイル結果のデータ名
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// このプロファイル結果を作り出したプローブの型
        /// </summary>
        public abstract Type ProbeType { get; }

        /// <summary>
        /// プロファイル結果の生の数値。
        /// ただし、サポートされない場合は 0 を返すことがあります。
        /// </summary>
        public virtual long RawValue { get; } = 0L;

        /// <summary>
        /// プロファイル結果の生のテキスト。
        /// ただし、サポートされない場合は 空文字列 を返すことがあります。
        /// </summary>
        public virtual string RawText { get; } = string.Empty;

        /// <summary>
        /// フォーマットされた数値。
        /// 生の値に対して、このプロファイル結果がわかりやすい表現のスケールでフォーマットします。
        /// ただし、サポートされない場合は 0.0 を返すことがあります。
        /// </summary>
        public virtual double FormatValue { get; } = 0.0;

        /// <summary>
        /// フォーマットされたテキスト。
        /// 生のテキストに対して、このプロファイル結果がわかりやすい表現のフォーマットをします。
        /// ただし、サポートされない場合は 空文字列 を返すことがあります。
        /// </summary>
        public virtual string FormatText { get; } = string.Empty;
    }
}