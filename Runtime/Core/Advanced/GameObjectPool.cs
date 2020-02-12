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

namespace IceMilkTea.Core
{
    /// <summary>
    /// UnityのGameObjectをプールするクラスです
    /// </summary>
    public class GameObjectPool : ObjectPool<GameObject>
    {
        // メンバ変数定義
        private GameObject original;



        /// <summary>
        /// GameObjectPool のインスタンスを初期化します
        /// </summary>
        /// <param name="original">プールするオリジナルオブジェクトの参照</param>
        /// <exception cref="System.ArgumentNullException">original が null です</exception>
        public GameObjectPool(GameObject original) : base()
        {
            // nullを渡されたら
            if (original == null)
            {
                // nullはプール出来ない
                throw new System.ArgumentNullException(nameof(original));
            }


            // 生成するオリジナルオブジェクトを覚える
            this.original = original;
        }


        /// <summary>
        /// 新しいオブジェクトのインスタンスを生成します
        /// </summary>
        /// <returns>生成したGameObjectのインスタンスを返します</returns>
        protected override GameObject Create()
        {
            // 素直にインスタンスを生成して返す
            return Object.Instantiate(original);
        }


        /// <summary>
        /// オブジェクトの解放をします
        /// </summary>
        /// <param name="obj">解放するオブジェクト</param>
        protected override void Destroy(GameObject obj)
        {
            // そのままDestroyを呼ぶ
            Object.Destroy(obj);
        }


        /// <summary>
        /// オブジェクトの初期化をします
        /// </summary>
        /// <param name="obj">初期化するオブジェクト</param>
        protected override void InitializeObject(GameObject obj)
        {
            // アクティブ状態にする
            obj.SetActive(true);
        }


        /// <summary>
        /// オブジェクトの終了をします
        /// </summary>
        /// <param name="obj">終了するオブジェクト</param>
        protected override void FinalizeObject(GameObject obj)
        {
            // 非アクティブ状態にする
            obj.SetActive(false);
        }
    }
}