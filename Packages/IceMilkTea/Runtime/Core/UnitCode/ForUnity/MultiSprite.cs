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
using UnityEngine;

namespace IceMilkTea.Core
{
    /// <summary>
    /// Unityのマルチスプライトを扱うアセット型クラスです
    /// </summary>
    public class MultiSprite : ScriptableObject
    {
        // メンバ変数定義
        private Dictionary<string, Sprite> spriteTable;



        /// <summary>
        /// スプライト名からスプライトへアクセスします
        /// </summary>
        /// <param name="name">スプライト名</param>
        /// <returns>指定されたスプライト名のスプライトを返しますが、存在しない場合は null を返すことがあります</returns>
        public Sprite this[string name]
        {
            get
            {
                // 名前からスプライトを取り出してそのまま返す
                Sprite result;
                spriteTable.TryGetValue(name, out result);
                return result;
            }
        }


        /// <summary>
        /// このマルチスプライトに含まれるスプライト名のコレクションを取得します
        /// </summary>
        public Dictionary<string, Sprite>.KeyCollection SpriteNames => spriteTable.Keys;


        /// <summary>
        /// このマルチスプライトに含まれるスプライトのコレクションを取得します
        /// </summary>
        public Dictionary<string, Sprite>.ValueCollection Sprites => spriteTable.Values;



        /// <summary>
        /// このマルチスプライトに含まれる全てのスプライトに対して列挙操作を行います
        /// </summary>
        /// <param name="handler">スプライトの列挙操作を行うハンドラ関数（引数2:スプライト名 引数2:スプライト）</param>
        /// <exception cref="ArgumentNullException">action が null です</exception>
        public void ForEach(Action<string, Sprite> handler)
        {
            // null を渡されたら
            if (handler == null)
            {
                // どうやって処理をすれば良いのだ
                throw new ArgumentNullException(nameof(handler));
            }


            // レコードの数分回る
            foreach (var pair in spriteTable)
            {
                // ハンドラに値を渡す
                handler(pair.Key, pair.Value);
            }
        }


        /// <summary>
        /// MultiSprite のインスタンスを初期化します
        /// </summary>
        /// <param name="sprites">マルチスプライトとして扱うスプライトの配列</param>
        internal void SetSprites(Sprite[] sprites)
        {
            // nullを渡されたら
            if (sprites == null)
            {
                // 何を管理すればよいのですか
                throw new ArgumentNullException(nameof(sprites));
            }


            // テーブルとして全て構築する
            spriteTable = new Dictionary<string, Sprite>(sprites.Length);
            foreach (var sprite in sprites)
            {
                // キーは名前として登録する
                spriteTable[sprite.name] = sprite;
            }
        }
    }
}