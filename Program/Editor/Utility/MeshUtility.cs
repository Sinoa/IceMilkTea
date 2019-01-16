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
    /// メッシュの結合結果を保持する構造体です
    /// </summary>
    public struct MeshCombineResult
    {
        /// <summary>
        /// 結合された結果のメッシュです
        /// </summary>
        public Mesh CombinedMesh;


        /// <summary>
        /// 結合時にまとめられたマテリアルの配列です。
        /// </summary>
        /// <remarks>
        /// 配列の順番は、結合されたメッシュのサブメッシュインデックスに対応しているため、絶対に並び替えてはいけません。
        /// </remarks>
        public Material[] Materials;
    }



    /// <summary>
    /// メッシュ操作を容易に可能とするためのユーティリティクラスです
    /// </summary>
    public static class MeshUtility
    {
        /// <summary>
        /// 指定されたゲームオブジェクト配列に含まれる、メッシュフィルタ及びレンダラを用いてメッシュ結合を行います。
        /// </summary>
        /// <param name="gameObjects">結合したいメッシュフィルタとレンダラを持ったゲームオブジェクトの配列</param>
        /// <param name="result">結合結果を出力します</param>
        /// <exception cref="ArgumentNullException">gameObjects が null です</exception>
        /// <exception cref="ArgumentException">gameObjects 配列の要素に null が含まれています</exception>
        public static void CombineMeshFromGameObjects(GameObject[] gameObjects, out MeshCombineResult result)
        {
            // 指定されたゲームオブジェクト配列からすべてのレンダラとフィルタを取り出して、サブメッシュデータとしてマテリアルによるメッシュのグループを行う
            var meshComponents = GetAllMeshRendererAndFilter(gameObjects);
            var subMeshData = GroupMeshByMaterial(meshComponents);


            // WIP
            result = default;
        }


        /// <summary>
        /// 指定されたゲームオブジェクト配列から全てのメッシュレンダラとフィルタのコンポーネントを取得します。
        /// </summary>
        /// <param name="gameObjects">メッシュレンダラ及びフィルタコンポーネントを持っているゲームオブジェクトの配列</param>
        /// <returns>取得されたメッシュレンダラとフィルタの配列を返します</returns>
        /// <exception cref="ArgumentNullException">gameObjects が null です</exception>
        /// <exception cref="ArgumentException">gameObjects 配列の要素に null が含まれています</exception>
        private static (MeshRenderer renderer, MeshFilter filter)[] GetAllMeshRendererAndFilter(GameObject[] gameObjects)
        {
            // そもそも null を渡されたら
            if (gameObjects == null)
            {
                // なにをすればよいのか
                throw new ArgumentNullException(nameof(gameObjects));
            }


            // メッシュレンダラとフィルタのコンポーネントを保持するリストを宣言
            var result = new List<(MeshRenderer renderer, MeshFilter filter)>();


            // 全ゲームオブジェクト回る
            foreach (var gameObject in gameObjects)
            {
                // もし null が含まれていたら
                if (gameObject == null)
                {
                    // null が含まれることは許されない
                    throw new ArgumentException($"{nameof(gameObjects)} 配列の要素に null が含まれています", nameof(gameObjects));
                }


                // ゲームオブジェクトからメッシュレンダラとフィルタを取得して、どちらかの参照がない場合は取り扱わない
                var meshRenderer = gameObject.GetComponent<MeshRenderer>();
                var meshFilter = gameObject.GetComponent<MeshFilter>();
                if (meshRenderer == null || meshFilter == null)
                {
                    // どちらとも持っていないと取り扱わない
                    continue;
                }


                // リストに追加する
                result.Add((meshRenderer, meshFilter));
            }


            // 取り出した結果を返す
            return result.ToArray();
        }


        /// <summary>
        /// メッシュコンポーネントのタプル配列から、マテリアルをキーとしたメッシュのグループを行います。
        /// </summary>
        /// <param name="meshComponents">メッシュレンダラ及びフィルタのコンポーネントの配列</param>
        /// <returns>マテリアルをキーとしたメッシュのグループ分け結果を返します</returns>
        private static (Material material, Mesh[] meshes)[] GroupMeshByMaterial((MeshRenderer renderer, MeshFilter filter)[] meshComponents)
        {
            // マテリアルをキーとしたメッシュリストのテーブルを初期化する
            var materialTable = new Dictionary<Material, List<Mesh>>();


            // コンポーネントの数分回る
            foreach (var meshComponent in meshComponents)
            {
                // レンダラが持っているマテリアルの数分ループ
                foreach (var material in meshComponent.renderer.sharedMaterials)
                {
                    // 該当のマテリアルキーがまだ無いなら
                    if (!materialTable.ContainsKey(material))
                    {
                        // キーを作りつつメッシュリストの初期化を行う
                        materialTable[material] = new List<Mesh>();
                    }


                    // 該当のメッシュの追加を行う
                    materialTable[material].Add(meshComponent.filter.sharedMesh);
                }
            }


            // 返すための結果変数の宣言を行いマテリアルの数分ループする
            var index = 0;
            var result = new (Material material, Mesh[] meshes)[materialTable.Count];
            foreach (var materialRecord in materialTable)
            {
                // 結果を入れていく
                result[index++] = (materialRecord.Key, materialRecord.Value.ToArray());
            }


            // 結果を返す
            return result;
        }
    }
}