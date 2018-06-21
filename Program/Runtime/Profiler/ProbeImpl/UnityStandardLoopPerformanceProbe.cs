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

using System.Diagnostics;
using IceMilkTea.Core;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;

namespace IceMilkTea.Profiler
{
    /// <summary>
    /// Unityの標準ループのパフォーマンスを計測するプローブクラスです。
    /// </summary>
    public class UnityStandardLoopPerformanceProbe : PerformanceProbe
    {
        /// <summary>
        /// FixedUpdateの計測を開始する更新ループの型定義です
        /// </summary>
        private struct FixedUpdateProbeStart { }


        /// <summary>
        /// FixedUpdateの計測を終了する更新ループの型定義です
        /// </summary>
        private struct FixedUpdateProbeEnd { }


        /// <summary>
        /// Updateの計測を開始する更新ループの型定義です
        /// </summary>
        private struct UpdateProbeStart { }


        /// <summary>
        /// Updateの計測を終了する更新ループの型定義です
        /// </summary>
        private struct UpdateProbeEnd { }


        /// <summary>
        /// LateUpdateの計測を開始する更新ループの型定義です
        /// </summary>
        private struct LateUpdateProbeStart { }


        /// <summary>
        /// LateUpdateの計測を終了する更新ループの型定義です
        /// </summary>
        private struct LateUpdateProbeEnd { }



        // シングルトン実装
        public static UnityStandardLoopPerformanceProbe Instance { get; } = new UnityStandardLoopPerformanceProbe();



        // メンバ変数宣言
        private Stopwatch stopwatch;
        private long fixedUpdateStartCount;
        private long fixedUpdateEndCount;
        private long updateStartCount;
        private long updateEndCount;
        private long lateUpdateStartCount;
        private long lateUpdateEndCount;
        private long renderingStartCount;
        private long renderingEndCount;
        private long textureRenderingStartCount;
        private long textureRenderingEndCount;



        /// <summary>
        /// パフォーマンス計測結果を取得します
        /// </summary>
        public override ProfileFetchResult Result { get; protected set; } = new UnityStandardLoopProfileResult();



        /// <summary>
        /// インスタンスの初期化を行います
        /// </summary>
        private UnityStandardLoopPerformanceProbe()
        {
            // 計測用ストップウォッチを生成
            stopwatch = Stopwatch.StartNew();


            // 各種アップデートの更新関数を登録する
            var rootLoopSystem = ImtPlayerLoopSystem.GetLastBuildLoopSystem();
            rootLoopSystem.InsertLoopSystem<FixedUpdate.ClearLines, FixedUpdateProbeStart>(InsertTiming.BeforeInsert, () => fixedUpdateStartCount = stopwatch.ElapsedTicks);
            rootLoopSystem.InsertLoopSystem<PreUpdate.UpdateVideo, FixedUpdateProbeEnd>(InsertTiming.AfterInsert, () => fixedUpdateEndCount = stopwatch.ElapsedTicks);
            rootLoopSystem.InsertLoopSystem<Update.ScriptRunBehaviourUpdate, UpdateProbeStart>(InsertTiming.BeforeInsert, () => updateStartCount = stopwatch.ElapsedTicks);
            rootLoopSystem.InsertLoopSystem<Update.DirectorUpdate, UpdateProbeEnd>(InsertTiming.AfterInsert, () => updateEndCount = stopwatch.ElapsedTicks);
            rootLoopSystem.InsertLoopSystem<PreLateUpdate.AIUpdatePostScript, LateUpdateProbeStart>(InsertTiming.BeforeInsert, () => lateUpdateStartCount = stopwatch.ElapsedTicks);
            rootLoopSystem.InsertLoopSystem<PreLateUpdate.ConstraintManagerUpdate, LateUpdateProbeEnd>(InsertTiming.AfterInsert, () => lateUpdateEndCount = stopwatch.ElapsedTicks);
            rootLoopSystem.BuildAndSetUnityDefaultPlayerLoop();


            // レンダリング系の計測はカメラクラスにいるハンドラを使う
            // まずは描画前（本当はカリングの部分も取りたいけどファーストのカメラのDrawを呼ぶ前にドライバのPresent待ちまで計上されるので一旦なし）
            Camera.onPreRender += (Camera camera) =>
            {
                // 現在のチックカウントを取り出す
                var tick = stopwatch.ElapsedTicks;


                // レンダーテクスチャではなくバックバッファへの書き込みなら
                if (camera.targetTexture == null)
                {
                    // 通常のレンダリングの開始時間として計測する
                    renderingStartCount = tick < renderingStartCount ? tick : renderingStartCount;
                    return;
                }


                // レンダーテクスチャの書き込みならテクスチャ書き込みとして計測する
                textureRenderingStartCount = tick < textureRenderingStartCount ? tick : textureRenderingStartCount;
            };


            // 描画後のハンドラ
            Camera.onPostRender += (Camera camera) =>
            {
                // 現在のチックカウントを取り出す
                var tick = stopwatch.ElapsedTicks;


                // レンダーテクスチャではなくバックバッファへの書き込みなら
                if (camera.targetTexture == null)
                {
                    // 通常のレンダリングの終了時間として計測する
                    renderingEndCount = tick > renderingEndCount ? tick : renderingEndCount;
                    return;
                }


                // レンダーテクスチャの書き込みならテクスチャ書き込みとして計測する
                textureRenderingEndCount = tick > textureRenderingEndCount ? tick : textureRenderingEndCount;
            };
        }


        /// <summary>
        /// 計測を開始します
        /// </summary>
        public override void Start()
        {
            // ここでは何もしない
        }


        /// <summary>
        /// 計測を終了します
        /// </summary>
        public override void Stop()
        {
            // 各チックカウントの経過数を求める
            var fixedCount = fixedUpdateEndCount - fixedUpdateStartCount;
            var updateCount = updateEndCount - updateStartCount;
            var lateCount = lateUpdateEndCount - lateUpdateStartCount;
            var renderingCount = renderingEndCount - renderingStartCount;
            var renderTextureRenderingCount = textureRenderingEndCount - textureRenderingStartCount;


            // レンダリング系はカメラがあったりなかったり、レンダーテクスチャが設定されていたりされていなかったりで
            // 計測が正しく出来ないので (0 - long.MaxValue) と一致するようであれば0カウントとする
            if (renderingCount == (0 - long.MaxValue))
            {
                // 未計測とする
                renderingCount = 0;
            }


            // レンダーテクスチャ版も調べる
            if (renderTextureRenderingCount == (0 - long.MaxValue))
            {
                // 未計測とする
                renderTextureRenderingCount = 0;
            }


            // レンダリングの計測カウンタの初期化をする
            renderingStartCount = long.MaxValue;
            renderingEndCount = 0;
            textureRenderingStartCount = long.MaxValue;
            textureRenderingEndCount = 0;


            // 結果を入れる
            var result = (UnityStandardLoopProfileResult)Result;
            result.UpdateResult(fixedCount, updateCount, lateCount, renderingCount, renderTextureRenderingCount);
        }
    }
}