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
using System.Linq;
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
        /// ゲームオブジェクトから取り出したメッシュ関連のコンポーネントを保持する構造体です
        /// </summary>
        private struct MeshComponentInfo
        {
            /// <summary>
            /// メッシュ情報を持つメッシュフィルタコンポーネント
            /// </summary>
            public MeshFilter Filter;


            /// <summary>
            /// メッシュ描画を行うメッシュレンダラコンポーネント
            /// </summary>
            public MeshRenderer Renderer;


            /// <summary>
            /// メッシュ描画をする際に利用する姿勢コンポーネント
            /// </summary>
            public Transform Transform;
        }



        /// <summary>
        /// メッシュに含まれるサブメッシュに対応する情報を保持する構造体です
        /// </summary>
        private struct SubMeshInfo
        {
            /// <summary>
            /// サブメッシュのインデックス値
            /// </summary>
            public int SubMeshIndex;


            /// <summary>
            /// サブメッシュを保持するメッシュ
            /// </summary>
            public Mesh SubMesh;


            /// <summary>
            /// サブメッシュに割り当てられたマテリアル
            /// </summary>
            public Material Material;


            /// <summary>
            /// サブメッシュを保持するメッシュがアタッチされたゲームオブジェクトのワールド変換行列
            /// </summary>
            public Matrix4x4 LocalToWorldMatrix;
        }



        /// <summary>
        /// 指定されたゲームオブジェクト配列に含まれる、メッシュフィルタ及びレンダラを用いてメッシュ結合を行います。
        /// </summary>
        /// <param name="gameObjects">結合したいメッシュフィルタとレンダラを持ったゲームオブジェクトの配列</param>
        /// <param name="result">結合結果を出力します</param>
        /// <exception cref="ArgumentNullException">gameObjects が null です</exception>
        /// <exception cref="ArgumentException">gameObjects 配列の要素に null が含まれています</exception>
        public static void CombineMeshFromGameObjects(GameObject[] gameObjects, out MeshCombineResult result)
        {
            // 無視するマテリアル配列をnull指定した関数呼び出しをする
            CombineMeshFromGameObjects(gameObjects, out result, null);
        }


        /// <summary>
        /// 指定されたゲームオブジェクト配列に含まれる、メッシュフィルタ及びレンダラを用いてメッシュ結合を行います。
        /// </summary>
        /// <param name="gameObjects">結合したいメッシュフィルタとレンダラを持ったゲームオブジェクトの配列</param>
        /// <param name="result">結合結果を出力します</param>
        /// <param name="ignoreMaterial">結合時に結合を無視するマテリアルの配列。不要であれば null の指定が可能です</param>
        /// <exception cref="ArgumentNullException">gameObjects が null です</exception>
        /// <exception cref="ArgumentException">gameObjects 配列の要素に null が含まれています</exception>
        public static void CombineMeshFromGameObjects(GameObject[] gameObjects, out MeshCombineResult result, Material[] ignoreMaterial)
        {
            // ゲームオブジェクトからメッシュコンポーネントをすべて取り出して、サブメッシュ情報を取り出す
            var meshComponents = GetMeshComponents(gameObjects);
            var subMeshInfos = GetSubMeshInfos(meshComponents);


            // サブメッシュ情報配列に含まれるユニークなマテリアル配列を作って、結果に入れておく
            result.Materials = subMeshInfos
                .Select(x => x.Material)
                .Distinct()
                .Where(x => ignoreMaterial == null ? true : !ignoreMaterial.Contains(x))
                .ToArray();


            // コンバインした最終結果を格納するメッシュも生成しておく
            result.CombinedMesh = new Mesh();


            // コンバインするための情報を持ったインスタンス配列を用意して、長さ分ループする
            var masterCombine = new CombineInstance[result.Materials.Length];
            for (int i = 0; i < masterCombine.Length; ++i)
            {
                // コンバイン情報にメッシュを詰めていく
                // （各結合済みサブメッシュは、マテリアルインデックスと一致させるため、このループで追加順によるサブメッシュインデックス化する）
                masterCombine[i].mesh = CombineSubMesh(result.Materials[i], subMeshInfos);
                masterCombine[i].subMeshIndex = 0;
                masterCombine[i].transform = Matrix4x4.identity;
            }


            // 生成した各メッシュをサブメッシュとして結合させて完了
            result.CombinedMesh.CombineMeshes(masterCombine, false);
        }


        /// <summary>
        /// 指定されたゲームオブジェクト配列から全てのメッシュレンダラとフィルタのコンポーネントを取得します。
        /// </summary>
        /// <param name="gameObjects">メッシュレンダラ及びフィルタコンポーネントを持っているゲームオブジェクトの配列</param>
        /// <returns>取得されたメッシュレンダラとフィルタの配列を返します</returns>
        /// <exception cref="ArgumentNullException">gameObjects が null です</exception>
        /// <exception cref="ArgumentException">gameObjects 配列の要素に null が含まれています</exception>
        private static MeshComponentInfo[] GetMeshComponents(GameObject[] gameObjects)
        {
            // そもそも null を渡されたら
            if (gameObjects == null)
            {
                // なにをすればよいのか
                throw new ArgumentNullException(nameof(gameObjects));
            }


            // メッシュレンダラとフィルタのコンポーネントを保持するリストを宣言
            var result = new List<MeshComponentInfo>();


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
                MeshComponentInfo info;
                info.Renderer = gameObject.GetComponent<MeshRenderer>();
                info.Filter = gameObject.GetComponent<MeshFilter>();
                info.Transform = gameObject.GetComponent<Transform>();
                if (info.Renderer == null || info.Filter == null)
                {
                    // どちらとも持っていないと取り扱わない
                    continue;
                }


                // リストに追加する
                result.Add(info);
            }


            // 取り出した結果を返す
            return result.ToArray();
        }


        /// <summary>
        /// メッシュコンポーネント配列から、すべてのサブメッシュ情報を取得します。
        /// </summary>
        /// <param name="meshComponents">メッシュレンダラ及びフィルタのコンポーネントの配列</param>
        /// <returns>取得されたすべてのサブメッシュ情報配列を返します</returns>
        private static SubMeshInfo[] GetSubMeshInfos(MeshComponentInfo[] meshComponents)
        {
            // サブメッシュ情報リストを初期化する
            var subMeshList = new List<SubMeshInfo>();


            // コンポーネントの数分回る
            foreach (var meshComponent in meshComponents)
            {
                // レンダラが持っているマテリアルの数分ループ
                for (int i = 0; i < meshComponent.Renderer.sharedMaterials.Length; ++i)
                {
                    // マテリアルの取得
                    var material = meshComponent.Renderer.sharedMaterials[i];


                    // サブメッシュ情報を生成してリストに追加
                    SubMeshInfo info;
                    info.SubMeshIndex = i;
                    info.SubMesh = meshComponent.Filter.sharedMesh;
                    info.Material = material;
                    info.LocalToWorldMatrix = meshComponent.Transform.localToWorldMatrix;
                    subMeshList.Add(info);
                }
            }


            // 全サブメッシュ情報を配列として返す
            return subMeshList.ToArray();
        }


        /// <summary>
        /// 該当マテリアルに一致するサブメッシュ情報からサブメッシュのメッシュを結合します。
        /// </summary>
        /// <param name="material">キーとなる、結合するサブメッシュに割り当てられたマテリアル</param>
        /// <param name="subMeshInfos">結合するサブメッシュ情報の配列</param>
        /// <returns>該当マテリアルが割り当てられたサブメッシュの結合結果のメッシュを返します</returns>
        private static Mesh CombineSubMesh(Material material, SubMeshInfo[] subMeshInfos)
        {
            // コンバイン情報を持つインスタンスリストを生成
            var combineList = new List<CombineInstance>();


            // サブメッシュ情報の数分ループ
            foreach (var subMeshInfo in subMeshInfos)
            {
                // サブメッシュ情報に結合するべきマテリアルが割り当てられていないなら
                if (subMeshInfo.Material != material)
                {
                    // 次のサブメッシュへ
                    continue;
                }


                // サブメッシュの結合インスタンスを生成してリストに追加
                CombineInstance combine = default;
                combine.mesh = subMeshInfo.SubMesh;
                combine.subMeshIndex = subMeshInfo.SubMeshIndex;
                combine.transform = subMeshInfo.LocalToWorldMatrix;
                combineList.Add(combine);
            }


            // メッシュを生成してサブメッシュを結合した結果を返す
            var mesh = new Mesh();
            mesh.CombineMeshes(combineList.ToArray(), true, true);
            return mesh;
        }
    }
}