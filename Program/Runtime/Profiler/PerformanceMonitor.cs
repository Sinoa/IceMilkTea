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
using IceMilkTea.Core;
using UnityEngine.Experimental.PlayerLoop;

namespace IceMilkTea.Profiler
{
    /// <summary>
    /// ゲームのパフォーマンスを監視し、パフォーマンス情報を保持するクラスです
    /// </summary>
    public sealed class PerformanceMonitor
    {
        /// <summary>
        /// パフォーマンスモニタのプロファイラ開始関数の型
        /// </summary>
        private struct StartProfile { }


        /// <summary>
        /// パフォーマンスモニタのプロファイラ終了関数の型
        /// </summary>
        private struct EndProfile { }


        /// <summary>
        /// パフォーマンスモニタの計測結果を描画する関数の型
        /// </summary>
        private struct DrawProfile { }



        // シングルトン実装
        public static PerformanceMonitor Instance { get; } = new PerformanceMonitor();



        // 静的クラス変数宣言
        private static bool initialized;

        // メンバ変数宣言
        private List<PerformanceProbe> performanceProbeList;
        private List<PerformanceRenderer> performanceRendererList;
        private ProfileFetchResult[] profileFetchResultsCache;



        /// <summary>
        /// クラスの初期化をします
        /// </summary>
        static PerformanceMonitor()
        {
            // まだ未初期化
            initialized = false;
        }


        /// <summary>
        /// インスタンスの初期化をします
        /// </summary>
        private PerformanceMonitor()
        {
            // メンバ変数のインスタンスを生成
            performanceProbeList = new List<PerformanceProbe>();
            performanceRendererList = new List<PerformanceRenderer>();
            profileFetchResultsCache = new ProfileFetchResult[0];
        }


        /// <summary>
        /// パフォーマンスモニタの初期化を行います
        /// </summary>
        public void Initialize()
        {
            // 初期化済みなら
            if (initialized)
            {
                // 何もしないで終了
                return;
            }


            // Unityの実行ループにパフォーマンスモニタが動くべき場所に更新関数を差し込む
            var rootLoopSystem = ImtPlayerLoopSystem.GetUnityDefaultPlayerLoop();
            rootLoopSystem.InsertLoopSystem<Initialization.SynchronizeState, StartProfile>(InsertTiming.AfterInsert, StartProfiler);
            rootLoopSystem.InsertLoopSystem<PostLateUpdate.PresentAfterDraw, EndProfile>(InsertTiming.BeforeInsert, EndProfiler);
            rootLoopSystem.InsertLoopSystem<PostLateUpdate.PresentAfterDraw, DrawProfile>(InsertTiming.BeforeInsert, DrawProfiler);
            rootLoopSystem.BuildAndSetUnityDefaultPlayerLoop();


            // 初期化は完了
            initialized = true;
        }


        /// <summary>
        /// パフォーマンスモニタにプローブを追加します
        /// </summary>
        /// <param name="probe">追加するプローブ</param>
        /// <exception cref="NullReferenceException">probeがnullです</exception>
        public void AddProbe(PerformanceProbe probe)
        {
            // 追加しようとしているプローブがnullなら
            if (probe == null)
            {
                // そんな追加は許されない！
                throw new NullReferenceException($"{nameof(probe)}がnullです");
            }


            // プローブを追加する
            performanceProbeList.Add(probe);
        }


        /// <summary>
        /// パフォーマンスモニタにレンダラを追加します
        /// </summary>
        /// <param name="renderer">追加するレンダラ</param>
        /// <exception cref="NullReferenceException">rendererがnullです</exception>
        public void AddRenderer(PerformanceRenderer renderer)
        {
            // 追加しようとしているレンダラがnullなら
            if (renderer == null)
            {
                // そんな追加は許されない！
                throw new NullReferenceException($"{nameof(renderer)}がnullです");
            }
        }


        /// <summary>
        /// プロファイラの動作を開始させます
        /// </summary>
        private void StartProfiler()
        {
            // もしプローブの数がプロファイル結果の数と一致しないなら
            if (performanceProbeList.Count != profileFetchResultsCache.Length)
            {
                // プロファイル結果キャッシュ配列を作り直す
                profileFetchResultsCache = new ProfileFetchResult[performanceProbeList.Count];
            }


            // プローブの数分ループ
            foreach (var probe in performanceProbeList)
            {
                // プローブに計測開始の指示をする
                probe.Start();
            }
        }


        /// <summary>
        /// プロファイラの動作を停止させます
        /// </summary>
        private void EndProfiler()
        {
            // プローブの数分ループ
            foreach (var probe in performanceProbeList)
            {
                // プローブに計測終了の指示をする
                // ただしプローブの結果受け取りのオーバーヘッドやその他の兼ね合いで結果受け取りはもう一度後でやる
                probe.Stop();
            }


            // プローブの数分ループ
            for (int i = 0; i < performanceProbeList.Count; ++i)
            {
                // 今度こそ結果をもらう
                profileFetchResultsCache[i] = performanceProbeList[i].Result;
            }
        }


        /// <summary>
        /// パフォーマンスレンダラにパフォーマンス結果の描画をさせます
        /// </summary>
        private void DrawProfiler()
        {
            // レンダラの数分ループ
            foreach (var renderer in performanceRendererList)
            {
                // レンダラに描画の開始から描画、終了まで指示する
                renderer.Begin(profileFetchResultsCache);
                renderer.Render();
                renderer.End();
            }
        }
    }
}