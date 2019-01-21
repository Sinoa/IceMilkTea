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
using System.Collections.Generic;
using UnityEngine;

namespace IceMilkTeaEditor.Utility
{
    /// <summary>
    /// チャンクに所属するモデルオブジェクトを保持した情報構造体です
    /// </summary>
    public struct ModelObjectChunkInfo
    {
        /// <summary>
        /// このチャンクを表す、ワールド空間上の境界ボックス
        /// </summary>
        public Bounds Bounds;


        /// <summary>
        /// このチャンクに所属するゲームオブジェクトの配列
        /// </summary>
        public GameObject[] GameObjects;
    }



    /// <summary>
    /// ゲームオブジェクト操作を容易に行うためのユーティリティクラスです
    /// </summary>
    public static class ImtGameObjectUtility
    {
        /// <summary>
        /// シーンに含まれる、すべてのモデルゲームオブジェクトをチャンク形式として取得します
        /// </summary>
        /// <param name="includeSkinnedMeshRenderer">スキンメッシュレンダラのゲームオブジェクトも含む場合は true を、含まない場合は false を指定</param>
        /// <param name="excludeInactiveObject">ヒエラルキ上における非アクティブ状態または、メッシュレンダラが無効の状態のゲームオブジェクトを除外する場合は true を、含む場合は false を指定</param>
        /// <param name="materialFilter">レンダラが持っているマテリアルから取得対象とするかを判断する関数。true を返すと取得対象 false を返すと無視対象として扱われます。何もしない場合は null の指定が可能です</param>
        /// <param name="chunkInfo">シーンに含まれる、すべてのモデルゲームオブジェクトの配列と、それの全体を含む境界ボックス情報を持った構造体を出力します</param>
        public static void GetAllModelObjects(bool includeSkinnedMeshRenderer, bool excludeInactiveObject, Func<Material[], bool> materialFilter, out ModelObjectChunkInfo chunkInfo)
        {
            // まずはゲームオブジェクトを取得してから境界ボックスを計算する
            chunkInfo.GameObjects = GetAllModelObjects(includeSkinnedMeshRenderer, excludeInactiveObject, materialFilter);
            chunkInfo.Bounds = CalculateBoundingBoxEnclosedModel(chunkInfo.GameObjects);
        }


        /// <summary>
        /// シーンに含まれる、すべてのモデルゲームオブジェクトを取得します
        /// </summary>
        /// <param name="includeSkinnedMeshRenderer">スキンメッシュレンダラのゲームオブジェクトも含む場合は true を、含まない場合は false を指定</param>
        /// <param name="excludeInactiveObject">ヒエラルキ上における非アクティブ状態または、メッシュレンダラが無効の状態のゲームオブジェクトを除外する場合は true を、含む場合は false を指定</param>
        /// <param name="materialFilter">レンダラが持っているマテリアルから取得対象とするかを判断する関数。true を返すと取得対象 false を返すと無視対象として扱われます。何もしない場合は null の指定が可能です</param>
        /// <returns>シーンに含まれる、すべてのモデルゲームオブジェクトの配列を返します</returns>
        public static GameObject[] GetAllModelObjects(bool includeSkinnedMeshRenderer, bool excludeInactiveObject, Func<Material[], bool> materialFilter)
        {
            // 収集したゲームオブジェクトを収めるリストを生成して、materialFilterがnullなら無条件通過関数で初期化する
            var gameObjectList = new List<GameObject>();
            materialFilter = materialFilter ?? new Func<Material[], bool>(x => true);


            // メッシュフィルタコンポーネントをすべて取得してループする
            var meshFilters = UnityEngine.Object.FindObjectsOfType<MeshFilter>();
            foreach (var meshFilter in meshFilters)
            {
                // メッシュレンダラの取得に失敗または、表示がされないオブジェクトを除外する場合は
                var gameObject = meshFilter.gameObject;
                var meshRenderer = gameObject.GetComponent<MeshRenderer>();
                if (meshRenderer == null || (excludeInactiveObject && (!meshRenderer.enabled || !gameObject.activeInHierarchy)))
                {
                    // 収集リストには含めず次へ
                    continue;
                }


                // マテリアルフィルタを通して拒否されたのなら
                if (!materialFilter(meshRenderer.sharedMaterials))
                {
                    // 収集リストには含めず次へ
                    continue;
                }


                // ゲームオブジェクトリストに追加
                gameObjectList.Add(gameObject);
            }


            // スキンメッシュレンダラも対象にするなら
            if (includeSkinnedMeshRenderer)
            {
                // スキンメッシュレンダラコンポーネントをすべて取得してループする
                var skinnedMeshRenderers = UnityEngine.Object.FindObjectsOfType<SkinnedMeshRenderer>();
                foreach (var skinnedMeshRenderer in skinnedMeshRenderers)
                {
                    // 表示されない可能性のあるゲームオブジェクトを除外する場合は
                    var gameObject = skinnedMeshRenderer.gameObject;
                    if (excludeInactiveObject && (!skinnedMeshRenderer.enabled || !gameObject.activeInHierarchy))
                    {
                        // 収集リストには含めず次へ
                        continue;
                    }


                    // マテリアルフィルタを通して拒否されたのなら
                    if (!materialFilter(skinnedMeshRenderer.sharedMaterials))
                    {
                        // 収集リストには含めず次へ
                        continue;
                    }


                    // ゲームオブジェクトリストに追加
                    gameObjectList.Add(gameObject);
                }
            }


            // 収集したゲームオブジェクトリストを配列にして返す
            return gameObjectList.ToArray();
        }


        /// <summary>
        /// 指定されたゲームオブジェクトから、ワールド空間上におけるモデル全体を含む境界ボックスを計算します。
        /// この関数は、対象ゲームオブジェクトが非アクティブまたは、レンダラが無効状態でも計算対象になることに注意して下さい。
        /// </summary>
        /// <param name="gameObjects">境界ボックスを計算する対象になるモデルゲームオブジェクトの配列</param>
        /// <returns>ワールド空間上におけるモデル全体を含む境界ボックスの計算結果を返します</returns>
        public static Bounds CalculateBoundingBoxEnclosedModel(GameObject[] gameObjects)
        {
            // 計算結果を受ける境界ボックスを宣言
            var bounds = new Bounds(Vector3.zero, Vector3.zero);


            // 渡されたゲームオブジェクト分回る
            foreach (GameObject gameObject in gameObjects)
            {
                // ゲームオブジェクトから境界ボックスを取得出来たのなら
                if (TryGetBoundingBox(gameObject, true, out var result))
                {
                    // センター、サイズ共に値がゼロなら
                    if (bounds.center == Vector3.zero && bounds.size == Vector3.zero)
                    {
                        // 取得した境界ボックスで上書きする
                        bounds = result;
                    }
                    else
                    {
                        // 取得した境界ボックスが収まるように拡張する
                        bounds.Encapsulate(result);
                    }


                    // 次のゲームオブジェクトへ
                    continue;
                }
            }


            // 計算した全体の境界ボックスを返す
            return bounds;
        }


        /// <summary>
        /// シーンに含まれる、すべてのモデルゲームオブジェクトを一定の大きさのチャンクごとにまとめた、チャンクの配列を取得します。
        /// </summary>
        /// <param name="gameObjects">チャンクに取り込むゲームオブジェクトの配列</param>
        /// <param name="chunkSize">チャンクのサイズ。このサイズはアラインメントのサイズとしても使われます。</param>
        /// <param name="worldSpaceBaseChunk">チャンクの区分けをする原点が、ワールド空間の場合は true を、モデルゲームオブジェクト全体の境界ボックスが原点の場合は false を指定</param>
        /// <param name="chunkBias">チャンクの領域を判定する場合に、浮動小数点誤差やモデルの頂点精度などによる判定漏れを防ぐためのバイアス</param>
        /// <returns>ゲームオブジェクトが含まれたチャンク情報の配列を返します</returns>
        public static ModelObjectChunkInfo[] GetModelObjectChunkInfos(GameObject[] gameObjects, Vector3 chunkSize, bool worldSpaceBaseChunk, Vector3 chunkBias)
        {
            // チャンクID毎に所属するゲームオブジェクトのリストテーブルを生成する
            var chunkTable = new Dictionary<ulong, List<GameObject>>();


            // ワールド空間座標ではなく、モデル全体オブジェクトの境界ボックスを中心とするなら座標計算のオフセットを求める
            var boundingBoxCenterOffset = worldSpaceBaseChunk ? Vector3.zero : -CalculateBoundingBoxEnclosedModel(gameObjects).center;


            // 渡されたゲームオブジェクト分ループする
            foreach (var gameObject in gameObjects)
            {
                // ゲームオブジェクトからワールド境界ボックスを取得するが失敗したら
                if (!TryGetBoundingBox(gameObject, true, out var boundingBox))
                {
                    // 直ちに次へ
                    continue;
                }


                // 境界ボックスの中心座標からチャンクIDを作る（64bit整数を16bit x4（x, y, z, w）として利用するがwは0）
                var center = boundingBox.center + boundingBoxCenterOffset + chunkBias;
                var chunkID = 0UL;
                chunkID |= (ulong)((long)((short)Mathf.FloorToInt(center.x / chunkSize.x) & 0xFFFF) << 48);
                chunkID |= (ulong)((long)((short)Mathf.FloorToInt(center.y / chunkSize.y) & 0xFFFF) << 32);
                chunkID |= (ulong)((long)((short)Mathf.FloorToInt(center.z / chunkSize.z) & 0xFFFF) << 16);


                // もしチャンクIDのデータがまだ未初期化なら
                if (!chunkTable.TryGetValue(chunkID, out var gameObjectList))
                {
                    // 新しくリストを作ってIDを設定する
                    gameObjectList = new List<GameObject>();
                    chunkTable[chunkID] = gameObjectList;
                }


                // ゲームオブジェクトに該当ゲームオブジェクトを追加する
                gameObjectList.Add(gameObject);
            }


            // テーブルのレコード数がチャンク数になるのでチャンク情報配列を作ってループする
            var insertIndex = 0;
            var modelObjectChunkInfos = new ModelObjectChunkInfo[chunkTable.Count];
            foreach (var chunkRecord in chunkTable)
            {
                // チャンクIDから境界ボックスの座標を作る
                var boundingBoxCenter = new Vector3();
                boundingBoxCenter.x = (short)((chunkRecord.Key >> 48) & 0xFFFF) * chunkSize.x - boundingBoxCenterOffset.x + chunkSize.x * 0.5f;
                boundingBoxCenter.y = (short)((chunkRecord.Key >> 32) & 0xFFFF) * chunkSize.y - boundingBoxCenterOffset.y + chunkSize.y * 0.5f;
                boundingBoxCenter.z = (short)((chunkRecord.Key >> 16) & 0xFFFF) * chunkSize.z - boundingBoxCenterOffset.z + chunkSize.z * 0.5f;


                // 境界ボックスを作る
                var chunkInfo = new ModelObjectChunkInfo();
                chunkInfo.Bounds = new Bounds(boundingBoxCenter, chunkSize);


                // 所属ゲームオブジェクトのリストを配列として設定する
                chunkInfo.GameObjects = chunkRecord.Value.ToArray();


                // チャンク情報に設定する
                modelObjectChunkInfos[insertIndex++] = chunkInfo;
            }


            // 結果を返す
            return modelObjectChunkInfos;
        }


        /// <summary>
        /// 指定されたゲームオブジェクトから描画上の境界ボックスの取得を試みます
        /// </summary>
        /// <param name="gameObject">取得する元のゲームオブジェクト</param>
        /// <param name="fromWorldSpace">ワールド空間上としての境界ボックスを取得する場合は true を、メッシュのローカル空間の境界ボックスを取得する場合は false を指定</param>
        /// <param name="boundingBox">取得された境界ボックスを設定します。取得に失敗した場合はセンター、サイズ共にゼロとして初期化されます。</param>
        /// <returns>取得に成功した場合は true を、失敗した場合は false を返します</returns>
        public static bool TryGetBoundingBox(GameObject gameObject, bool fromWorldSpace, out Bounds boundingBox)
        {
            // gameObject が null なら
            if (gameObject == null)
            {
                // 境界ボックスに初期値を入れて失敗を返す
                boundingBox = new Bounds(Vector3.zero, Vector3.zero);
                return false;
            }


            // もしワールド空間の境界ボックスを取得するなら
            if (fromWorldSpace)
            {
                // メッシュレンダラコンポーネントの境界ボックスの取得に成功したのなら
                var bounds = gameObject.GetComponent<MeshRenderer>()?.bounds;
                if (bounds.HasValue)
                {
                    // メッシュレンダラの境界ボックスを設定して成功を返す
                    boundingBox = bounds.Value;
                    return true;
                }


                // スキンメッシュレンダラコンポーネントの境界ボックスの取得に成功したのなら
                bounds = gameObject.GetComponent<SkinnedMeshRenderer>()?.bounds;
                if (bounds.HasValue)
                {
                    // スキンメッシュレンダラの境界ボックスを設定して成功を返す
                    boundingBox = bounds.Value;
                    return true;
                }
            }
            else
            {
                // メッシュフィルタコンポーネントの境界ボックスの取得に成功したのなら
                var bounds = gameObject.GetComponent<MeshFilter>()?.sharedMesh.bounds;
                if (bounds.HasValue)
                {
                    // メッシュフィルタの境界ボックスを設定して成功を返す
                    boundingBox = bounds.Value;
                    return true;
                }


                // スキンメッシュレンダラコンポーネントの境界ボックスの取得に成功したのなら
                bounds = gameObject.GetComponent<SkinnedMeshRenderer>()?.sharedMesh.bounds;
                if (bounds.HasValue)
                {
                    // スキンメッシュレンダラの参照しているメッシュの境界ボックスを設定して成功を返す
                    boundingBox = bounds.Value;
                    return true;
                }
            }


            // ここまで到達してしまったのなら取得に失敗したということなので、初期値を入れて失敗を返す
            boundingBox = new Bounds(Vector3.zero, Vector3.zero);
            return false;
        }
    }
}