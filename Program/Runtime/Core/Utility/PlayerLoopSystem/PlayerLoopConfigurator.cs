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
using UnityEngine.Experimental.LowLevel;
using UnityEngine.Experimental.PlayerLoop;

namespace IceMilkTea.Core
{
    /// <summary>
    /// Unity標準のFixedUpdateの代わりに差し替えられるIceMilkTea用のFixedUpdate型構造体です
    /// </summary>
    public struct IceMilkTeaFixedUpdate
    {
    }



    /// <summary>
    /// Unityのメインループを制御するPlayerLoopの設定を操作するクラスです
    /// </summary>
    public class PlayerLoopConfigurator
    {
        // メンバ変数宣言
        private ImtPlayerLoopSystem rootPlayerLoopSystem;



        /// <summary>
        /// Unity標準のFixedUpdateではなくカスタムされたFixedUpdateを利用するかどうか
        /// カスタムFixedUpdateを利用すると物理挙動の整合性を取るためのサブループが行われなくなります
        /// </summary>
        public bool UseCustomFixedUpdate { get; set; }



        /// <summary>
        /// インスタンスの初期化を行います
        /// </summary>
        public PlayerLoopConfigurator()
        {
            // Unityから現状のPlayerLoop情報を引っ張り出してクラス化
            rootPlayerLoopSystem = (ImtPlayerLoopSystem)PlayerLoop.GetDefaultPlayerLoop();
        }


        /// <summary>
        /// 設定したメインループ内容をUnityのPlayerLoopシステムに反映します
        /// </summary>
        public void Apply()
        {
            // もしカスタムなFixedUpdateが要求されていたら
            if (UseCustomFixedUpdate)
            {
                // UnityのFixedUpdateを探し出して見つかれば
                var fixedUpdateSubLoopSystem = rootPlayerLoopSystem.SubLoopSystemList.Find(system => system.Type == typeof(FixedUpdate));
                if (fixedUpdateSubLoopSystem != null)
                {
                    // カスタムのFixedUpdateへ変更する
                    fixedUpdateSubLoopSystem.ResetUnityNativeFunctions();
                    fixedUpdateSubLoopSystem.UpdateDelegate = null;
                    fixedUpdateSubLoopSystem.ChangeType(typeof(IceMilkTeaFixedUpdate));
                }
            }


            // 現在のループシステムの内容をUnityに設定する
            PlayerLoop.SetPlayerLoop((PlayerLoopSystem)rootPlayerLoopSystem);
        }


        /// <summary>
        /// 指定されたサブループシステムの型の後に、指示された挿入サブループシステムを挿入します
        /// </summary>
        /// <param name="previousSubLoopSystemType">挿入する位置を決定するサブループシステムの型。もし、先頭へ挿入する場合はnullの指定が可能です</param>
        /// <param name="insertSubLoopSystemType">挿入するサブループシステムの型</param>
        /// <param name="updateFunction">サブループシステムの更新関数。不要な場合はnullを指定することが出来ます</param>
        public void InsertSubPlayerLoopSystem(Type previousSubLoopSystemType, Type insertSubLoopSystemType, PlayerLoopSystem.UpdateFunction updateFunction)
        {
            // 挿入するサブループシステムのインスタンスを生成する
            var insertSubLoopSystem = new ImtPlayerLoopSystem(insertSubLoopSystemType, updateFunction);


            // もし先頭へ挿入することを指示されているのなら
            if (previousSubLoopSystemType == null)
            {
                // 先頭へ挿入して終了
                rootPlayerLoopSystem.SubLoopSystemList.Insert(0, insertSubLoopSystem);
                return;
            }


            // 挿入する位置を見つけるが見つけられなかったら
            var insertIndex = rootPlayerLoopSystem.SubLoopSystemList.FindIndex(system => system.Type == previousSubLoopSystemType);
            if (insertIndex == -1)
            {
                // 見つけられなかったので死ぬ
                throw new InvalidOperationException($"指示されたサブシステムループタイプ {previousSubLoopSystemType} が見つけられませんでした");
            }


            // 挿入位置を見つけたのなら、見つけた位置の次へ挿入する
            insertIndex = insertIndex + 1;
            rootPlayerLoopSystem.SubLoopSystemList.Insert(insertIndex, insertSubLoopSystem);
        }


        /// <summary>
        /// 指定された副ループシステム型の後に、指示された挿入副ループシステムを挿入します
        /// </summary>
        /// <param name="previousType">挿入する位置を決定するアップデートタイプ。先頭に挿入する場合はnullの指定が可能です</param>
        /// <param name="insertType">挿入するアップデートタイプ</param>
        /// <param name="updateFunction">挿入する更新関数。更新しない仕様ならnullの指定が可能です</param>
        public void InsertCoPlayerLoopSystem(Type previousType, Type insertType, PlayerLoopSystem.UpdateFunction updateFunction)
        {
            // 挿入するImtPlayerLoopSystemのインスタンスを生成する
            var loopSystem = new ImtPlayerLoopSystem(insertType, updateFunction);


            // もし挿入するアップデートタイプの位置がnullなら
            if (previousType == null)
            {
                // ルートループシステムの先頭にぶちこんで終了
                rootPlayerLoopSystem.SubLoopSystemList.Insert(0, loopSystem);
                return;
            }


            // previouseTypeの型はなにかの内部クラスなら
            if (previousType.DeclaringType != null)
            {
                // 内包している側の方を取得する
                var subLoopSystemType = previousType.DeclaringType;


                // サブループシステムの型が存在するか探して存在しているなら
                var subLoopSystem = rootPlayerLoopSystem.SubLoopSystemList.Find(system => system.Type == subLoopSystemType);
                if (subLoopSystem != null)
                {
                    // サブループシステム内に該当の副ループシステムがあるか探して存在しているなら
                    var coLoopSystem = subLoopSystem.SubLoopSystemList.Find(system => system.Type == previousType);
                    if (coLoopSystem != null)
                    {
                    }
                }
            }
        }
    }
}