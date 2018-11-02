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

namespace IceMilkTea.Core
{
    /// <summary>
    /// 特定の型のインスタンスをプールすることが出来るプール抽象クラスです。
    /// </summary>
    /// <remarks>
    /// このプールクラスのインスタンスによって生成されたオブジェクトは、プールから提供されたままでも
    /// プール自体が解放される時に、オブジェクトの解放を行います。
    /// </remarks>
    /// <typeparam name="T">プールするインスタンスの型</typeparam>
    public abstract class ObjectPool<T> : IDisposable
    {
        // メンバ変数定義
        private List<T> createdObjectPool;
        private Queue<T> nextProvideObjectQueue;
        private bool allowCreateObject;
        private bool disposed;



        /// <summary>
        /// プールから直ちに取り出せる数
        /// </summary>
        /// <remarks>
        /// このプロパティが 0 の場合でも allowCreateObject が true ならばオブジェクトの取得は可能です
        /// </remarks>
        public int AvailableCount => nextProvideObjectQueue.Count;



        #region コンストラクタ＆解放
        /// <summary>
        /// ObjectPool のインスタンスを初期化します
        /// </summary>
        public ObjectPool()
        {
            // リストやキューの初期化のみ行う
            createdObjectPool = new List<T>();
            nextProvideObjectQueue = new Queue<T>();
        }


        /// <summary>
        /// ObjectPool のインスタンスを指定された初期生成数でプールを初期化します
        /// </summary>
        /// <param name="initializeCount">初期生成を行うオブジェクト数</param>
        /// <exception cref="ArgumentOutOfRangeException">initializeCount に負の範囲が指定されました</exception>
        public ObjectPool(int initializeCount) : this(initializeCount, true)
        {
        }


        /// <summary>
        /// ObjectPool のインスタンスを指定された初期生成数でプールを初期化します
        /// </summary>
        /// <param name="initializeCount">初期生成を行うオブジェクト数</param>
        /// <param name="allowCreateObject">プールが空の時新しくオブジェクトを生成することを許可する場合は true を、許可しない場合は false</param>
        public ObjectPool(int initializeCount, bool allowCreateObject)
        {
            // 初期個数が負の値なら
            if (initializeCount < 0)
            {
                // 負の個数ってどうやって作るのだ
                throw new ArgumentOutOfRangeException(nameof(initializeCount), $"{nameof(initializeCount)} に負の範囲が指定されました");
            }


            // リストとキューの長さを初期の数分生成する
            createdObjectPool = new List<T>(initializeCount);
            nextProvideObjectQueue = new Queue<T>(initializeCount);


            // 初期生成の数分回る
            for (int i = 0; i < initializeCount; ++i)
            {
                // オブジェクトを生成してリストとキューに追加
                var newObject = Create();
                createdObjectPool.Add(newObject);
                nextProvideObjectQueue.Enqueue(newObject);
            }


            // オブジェクト生成許可情報を受取る
            this.allowCreateObject = allowCreateObject;
        }


        /// <summary>
        /// ObjectPool の解放をします
        /// </summary>
        ~ObjectPool()
        {
            // ファイナライザからのDispose呼び出し
            Dispose(false);
        }


        /// <summary>
        /// ObjectPool のリソースを解放します
        /// </summary>
        public void Dispose()
        {
            // DisposeからのDispose呼び出しをして、GCのファイナライザは呼ばないようにしてもらう
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        /// <summary>
        /// ObjectPool のリソースを解放します
        /// </summary>
        /// <param name="disposing">マネージド解放なら turu を、アンマネージ解放なら false を指定</param>
        protected virtual void Dispose(bool disposing)
        {
            // 既に解放済みなら
            if (disposed)
            {
                // 何もせず終了
                return;
            }


            // マネージの解放なら
            if (disposing)
            {
                // 自身によって生成または、Releaseから渡されたオブジェクト分回る
                for (int i = 0; i < createdObjectPool.Count; ++i)
                {
                    // 解放を呼ぶ
                    Destroy(createdObjectPool[i]);
                }
            }


            // 解放済みマーク
            disposed = true;
        }
        #endregion


        #region プール操作
        /// <summary>
        /// 指定された数がプールから直ちに取り出せるように、追加のオブジェクトを生成しプールします。
        /// ただし、指定された数のオブジェクトが既にプールから取り出せる場合は、この関数は何もしません。
        /// </summary>
        /// <param name="poolCount">保証するべきプールの数</param>
        /// <exception cref="InvalidOperationException">新しくオブジェクトを作ることが許可されていないため、数の確保が出来ません</exception>
        public void Ensure(int poolCount)
        {
            // 要求された数が既にプールにあるのなら
            if (poolCount <= AvailableCount)
            {
                // 要求には答えられるサイズはあるので直ちに終了
                return;
            }


            // 新しくオブジェクトを作ることが許可されていないのなら
            if (!allowCreateObject)
            {
                // 新しく作ることが出来ないので例外を吐く
                throw new InvalidOperationException("新しくオブジェクトを作ることが許可されていないため、数の確保が出来ません");
            }


            // 足りない数分回る
            int requiredCount = poolCount - AvailableCount;
            for (int i = 0; i < requiredCount; ++i)
            {
                // オブジェクトを生成してリストとキューに追加
                var newObject = Create();
                createdObjectPool.Add(newObject);
                nextProvideObjectQueue.Enqueue(newObject);
            }
        }


        /// <summary>
        /// プールからオブジェクトを取り出します
        /// </summary>
        /// <remarks>
        /// プールが空になった場合 allowCreateObject が true の時新しくオブジェクトを生成しますが
        /// allowCreateObject が false の場合、この関数は例外をスローします。
        /// </remarks>
        /// <returns>取り出されたオブジェクトを返します</returns>
        /// <exception cref="InvalidOperationException">プールが空のためオブジェクトを取り出すことが出来ません</exception>
        public T Take()
        {
            // 例外判定を入れる
            ThrowIfDisposed();


            // もしキューが空なら
            if (nextProvideObjectQueue.Count == 0)
            {
                // もし新しくオブジェクトを生成する事が許されていないなら
                if (!allowCreateObject)
                {
                    // プールが空でオブジェクトを提供することが出来ない例外を吐く
                    throw new InvalidOperationException("プールが空のためオブジェクトを取り出すことが出来ません");
                }


                // 新しくオブジェクトを生成してリストに追加後返す
                var newerObject = Create();
                createdObjectPool.Add(newerObject);
                return newerObject;
            }


            // キューから取り出して初期化の振る舞いを実行して返す
            var providableObject = nextProvideObjectQueue.Dequeue();
            InitializeObject(providableObject);
            return providableObject;
        }


        /// <summary>
        /// プールからオブジェクトの取り出しを試みます。
        /// プールが空の場合は通常新しくオブジェクトが生成されますが allowCreateObject が false の時は T の既定値を返します
        /// </summary>
        /// <returns>取り出されたオブジェクトを返します</returns>
        public T TryTake()
        {
            // 例外判定を入れる
            ThrowIfDisposed();


            // もしキューが空なら
            if (nextProvideObjectQueue.Count == 0)
            {
                // もし新しくオブジェクトを生成する事が許されていないなら
                if (!allowCreateObject)
                {
                    // 既定値を返す
                    return default(T);
                }


                // 新しくオブジェクトを生成してリストに追加後返す
                var newerObject = Create();
                createdObjectPool.Add(newerObject);
                return newerObject;
            }


            // キューから取り出して初期化の振る舞いを実行して返す
            var providableObject = nextProvideObjectQueue.Dequeue();
            InitializeObject(providableObject);
            return providableObject;
        }


        /// <summary>
        /// プールから取り出したオブジェクトまたは外部のオブジェクトをプールに返します
        /// </summary>
        /// <param name="obj">返すオブジェクト</param>
        /// <exception cref="ArgumentNullException">obj が null です</exception>
        public void Release(T obj)
        {
            // 例外判定を入れる
            ThrowIfDisposed();


            // nullを渡されたら
            if (obj == null)
            {
                // nullのプールは許可されない
                throw new ArgumentNullException(nameof(obj));
            }


            // オブジェクトの解放の振る舞いを実行して提供キューに追加する
            FinalizeObject(obj);
            nextProvideObjectQueue.Enqueue(obj);
        }
        #endregion


        #region インスタンス操作
        /// <summary>
        /// プールする新しいオブジェクトを生成します
        /// </summary>
        /// <returns>新しく生成したオブジェクトを返します</returns>
        protected abstract T Create();


        /// <summary>
        /// プールが生成したオブジェクトを解放します
        /// </summary>
        /// <param name="obj">解放するオブジェクト</param>
        protected virtual void Destroy(T obj)
        {
            // もしIDisposableを実装しているのなら
            if (obj is IDisposable)
            {
                // Disposeを呼ぶ
                ((IDisposable)obj).Dispose();
            }
        }


        /// <summary>
        /// プールからオブジェクトが取り出され、そのオブジェクトを初期化する振る舞いを実行します
        /// </summary>
        /// <param name="obj">初期化を行うオブジェクト</param>
        protected virtual void InitializeObject(T obj)
        {
        }


        /// <summary>
        /// オブジェクトがプールに返される時に、そのオブジェクトを解放する振る舞いを実行します
        /// </summary>
        /// <param name="obj">解放を行うオブジェクト</param>
        protected virtual void FinalizeObject(T obj)
        {
        }
        #endregion


        #region 内部ロジック
        /// <summary>
        /// オブジェクトが解放済みの時に例外をスローします
        /// </summary>
        /// <exception cref="ObjectDisposedException">このインスタンスは既に解放済みです</exception>
        private void ThrowIfDisposed()
        {
            // 解放済みなら
            if (disposed)
            {
                // オブジェクト解放済み例外を投げる
                throw new ObjectDisposedException(GetType().FullName);
            }
        }
        #endregion
    }
}