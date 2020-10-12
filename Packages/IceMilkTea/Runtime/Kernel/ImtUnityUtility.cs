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

using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace IceMilkTea.Core
{
    /// <summary>
    /// Unity関連実装でユーティリティな関数として使えるような、関数が実装されているクラスです
    /// </summary>
    public static class ImtUnityUtility
    {
        /// <summary>
        /// 永続的に存在し続けるゲームオブジェクトを生成します。
        /// この関数で生成されるゲームオブジェクトはヒエラルキに表示されません。
        /// また、名前はNewGameObjectとして作られます。
        /// </summary>
        /// <returns>生成された永続ゲームオブジェクトを返します</returns>
        public static GameObject CreatePersistentGameObject()
        {
            // "NewGameObject" な見えないゲームオブジェクトを生成して返す
            return CreatePersistentGameObject("NewGameObject", HideFlags.HideInHierarchy);
        }


        /// <summary>
        /// 永続的に存在し続けるゲームオブジェクトを生成します。
        /// この関数で生成されるゲームオブジェクトはヒエラルキに表示されません。
        /// </summary>
        /// <param name="name">生成する永続ゲームオブジェクトの名前</param>
        /// <returns>生成された永続ゲームオブジェクトを返します</returns>
        public static GameObject CreatePersistentGameObject(string name)
        {
            // 見えないゲームオブジェクトを生成して返す
            return CreatePersistentGameObject(name, HideFlags.HideInHierarchy);
        }


        /// <summary>
        /// 永続的に存在し続けるゲームオブジェクトを生成します。
        /// </summary>
        /// <param name="name">生成する永続ゲームオブジェクトの名前</param>
        /// <param name="hideFlags">生成する永続ゲームオブジェクトの隠しフラグ</param>
        /// <returns>生成された永続ゲームオブジェクトを返します</returns>
        public static GameObject CreatePersistentGameObject(string name, HideFlags hideFlags)
        {
            // ゲームオブジェクトを生成する
            var gameObject = new GameObject(name);


            // ヒエラルキから姿を消して永続化
            gameObject.hideFlags = hideFlags;
            UnityObject.DontDestroyOnLoad(gameObject);


            // トランスフォームを取得して念の為初期値を入れる
            var transform = gameObject.GetComponent<Transform>();
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;


            // 作ったゲームオブジェクトを返す
            return gameObject;
        }
    }
}