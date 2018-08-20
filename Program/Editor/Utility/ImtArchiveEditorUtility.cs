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
using System.Threading.Tasks;
using IceMilkTea.Module;

namespace IceMilkTeaEditor.Utility
{
    /// <summary>
    /// IceMilkTeaArchiveを容易に制御するためのユーティリティクラスです
    /// </summary>
    public static class ImtArchiveEditorUtility
    {
        /// <summary>
        /// 指定されたアーカイブに、インクルードするファイルを、非同期追加インストールします
        /// </summary>
        /// <param name="archivePath">操作する対象のアーカイブファイルパス</param>
        /// <param name="includeFilePaths">追加インストールするファイルパス</param>
        /// <param name="progress">インストール状況を確認するためのプログレスコールバック関数（arg1:タイトル arg2:メッセージ arg3:進行割合）</param>
        /// <returns>インストール結果を返す、待機オブジェクトを返します</returns>
        /// <exception cref="ArgumentException">archivePath に 無効な値が渡されました</exception>
        /// <exception cref="ArgumentException">includeFilePath が null か、長さが 0 です</exception>
        public static async Task<ImtArchiveEntryInstallResult> BuildArchiveAsync(string archivePath, string[] includeFilePaths, Action<string, string, float> progress)
        {
            // アーカイブパスが無効なら
            if (string.IsNullOrWhiteSpace(archivePath))
            {
                // アーカイブ操作が出来ません
                throw new ArgumentException(nameof(archivePath));
            }


            // ファイルパス配列が扱える範囲でないなら
            if (includeFilePaths == null || includeFilePaths.Length == 0)
            {
                // なにをインストールすれば良いのか
                throw new ArgumentException(nameof(includeFilePaths));
            }


            // 進行通知コールバックを呼び出せるようにインスタンスの調整
            progress = progress ?? new Action<string, string, float>((title, message, prog) => { });


            // アーカイブを開いて、管理データをフェッチ出来るなら
            var archive = new ImtArchive(archivePath);
            if (archive.CanFetchManageData())
            {
                // 管理データをフェッチする
                archive.FetchManageData();
            }


            // ファイルパスの数分回る
            for (int i = 0; i < includeFilePaths.Length; ++i)
            {
                // 現在の進行状況を確認する
                var currentProgress = i / (float)includeFilePaths.Length;
                var includeFilePath = includeFilePaths[i];


                // 進行通知をしながらインストーラを準備する
                progress("インストーラを準備しています", $"[{i + 1}/{includeFilePaths.Length}] {includeFilePath}", currentProgress);
                archive.EnqueueInstaller(new ImtArchiveEntryFileInstaller(includeFilePath));
            }


            // インストール通知をして、インストール結果を返す
            progress("インストールしています", "インストールが完了するまでお待ち下さい", 1.0f);
            return await archive.InstallEntryAsync();
        }
    }
}