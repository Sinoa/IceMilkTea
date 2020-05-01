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
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.UI;

namespace IceMilkTea.UI
{
    #region コンポーネントクラス
    /// <summary>
    /// ルビ表示が可能なテキストUIコンポーネントクラスです
    /// </summary>
    [AddComponentMenu("IceMilkTea/UI/RubyableText")]
    [RequireComponent(typeof(CanvasRenderer))]
    public class RubyableText : MaskableGraphic, ILayoutElement
    {
        // インスペクタ公開メンバ変数定義
        [Header("FontSetting")]
        [SerializeField]
        private Font font;
        [SerializeField]
        private FontStyle style;
        [SerializeField]
        [Range(1, 300)]
        private int requestMainFontSize;
        [SerializeField]
        [Range(1, 300)]
        private int requestRubyFontSize;
        [SerializeField]
        [Range(1, 300)]
        private int renderMainFontSize;
        [SerializeField]
        [Range(1, 300)]
        private int renderRubyFontSize;
        [SerializeField]
        private Color bottomColor;
        [Header("LayoutSetting")]
        [SerializeField]
        [Range(0, 10)]
        private float lineSpace;
        [SerializeField]
        [Range(0, 10)]
        private float letterSpace;
        [SerializeField]
        private TextAlignment textAlignment;
        [SerializeField]
        private bool forceProportional;
        [Header("AnimationSetting")]
        [SerializeField]
        private ImtTextAnimator textAnimator;
        [Header("GameText")]
        [SerializeField]
        [TextArea(5, 10)]
        private string text;

        // メンバ変数定義
        [NonSerialized]
        private bool initialized;
        private GameText gameText;
        private RubyableTextFontData fontData;
        private RubyableTextVerticesCache verticesCache;
        private RubyableTextVerticesCache.BuildSetting buildSetting;
        private Action<UIVertex[]> vertexUploader;
        private VertexHelper currentVertexHelper;
        private Rect cachedBoundingRect;
        private bool needRebuild;



        #region プロパティ
        /// <summary>
        /// 表示に必要な横幅を返します。
        /// </summary>
        public float preferredWidth => cachedBoundingRect.width;


        /// <summary>
        /// 表示に必要な縦幅を返します。
        /// </summary>
        public float preferredHeight => cachedBoundingRect.height;


        /// <summary>
        /// 最小の横幅を返します。常に 0.0 を返します。
        /// </summary>
        public float minWidth => 0.0f;


        /// <summary>
        /// 最小の縦幅を返します。常に 0.0 を返します
        /// </summary>
        public float minHeight => 0.0f;


        /// <summary>
        /// 残りのサイズに対して割り当てる割合。常に -1.0(disable) を返します
        /// </summary>
        public float flexibleWidth => -1.0f;


        /// <summary>
        /// 残りのサイズに対して割り当てる割合。常に -1.0(disable) を返します
        /// </summary>
        public float flexibleHeight => -1.0f;


        /// <summary>
        /// レイアウト計算の優先順位。常に 0 を返します
        /// </summary>
        public int layoutPriority => 0;


        /// <summary>
        /// レンダリングに用いるメインテクスチャを返します
        /// </summary>
        public override Texture mainTexture => GetFontTexture();


        /// <summary>
        /// テキストを取得設定します
        /// </summary>
        public string Text { get { return text; } set { text = value; needRebuild = true; gameText.Parse(text); SetAllDirty(); textAnimator?.OnTextChanged(); } }


        /// <summary>
        /// 本文テキストを取得します
        /// </summary>
        public string MainText => gameText.MainText;


        /// <summary>
        /// ルビテキストを取得します
        /// </summary>
        public string RubyText => gameText.RubyText;


        /// <summary>
        /// 装飾エントリ情報カウントを取得します
        /// </summary>
        public int DecorationEntryCount => gameText.DecorationEntryCount;
        #endregion



        #region コンストラクタ
        /// <summary>
        /// RubyableText クラスのインスタンスを初期化します
        /// </summary>
        public RubyableText()
        {
            // レガシーなメッシュ生成方式を無効化する
            useLegacyMeshGeneration = false;
        }
        #endregion


        #region Unityイベントハンドラ
        /// <summary>
        /// コンポーネントの初期化を行います
        /// </summary>
        protected override void Awake()
        {
            // ベースのAwakeを呼んでから初期化を行う
            base.Awake();
            Initialize();
            textAnimator?.Initialize(this);
        }


        /// <summary>
        /// コンポーネントの更新を行います
        /// </summary>
        protected virtual void Update()
        {
            // 初期化処理は呼び続けながらフォントのリフレッシュを行う
            Initialize();
            RefreshFont();
        }
        #endregion


        #region 汎用ロジック関数群
        /// <summary>
        /// 指定されたインデックスの装飾エントリを取得します
        /// </summary>
        /// <param name="index">取り出すエントリのインデックス</param>
        /// <param name="decorationEntry">取り出したエントリを格納する参照</param>
        public bool TryGetDecorationEntry(int index, out GameText.DecorationEntry decorationEntry)
        {
            // 実際の操作しているゲームテキストから取り出す
            return gameText.TryGetDecorationEntry(index, out decorationEntry);
        }


        /// <summary>
        /// 未初期化の場合に初期化の処理を実行します
        /// </summary>
        private void Initialize()
        {
            // 初期化済みなら
            if (initialized)
            {
                // 何もしない
                return;
            }


            // テキストが未初期化なら空文字列で初期化
            text = text ?? string.Empty;


            // ゲームテキストの初期化
            gameText = new GameText();
            if (!gameText.Parse(text))
            {
                // パースエラーを起こしているなら空文字列で回避
                gameText.Parse(string.Empty);
            }


            // フォントと頂点キャッシュの生成
            fontData = new RubyableTextFontData(font ?? Resources.GetBuiltinResource<Font>("Arial.ttf"), requestMainFontSize, requestRubyFontSize, style, style, gameText);
            verticesCache = new RubyableTextVerticesCache(1024, textAnimator);


            // ビルド設定を設定する
            SetupBuildSetting();


            // 初回構築をしてしまう
            verticesCache.BuildVertices(gameText, fontData, ref buildSetting);
            cachedBoundingRect = verticesCache.BoundingRect;


            // 頂点アップローダの参照を持つ
            vertexUploader = UploadVertex;


            // リビルドは必要なし
            needRebuild = false;


            // 初期化済みマーク
            initialized = true;
        }


        /// <summary>
        /// インスペクタ用パラメータに設定された値をビルド設定にセットアップします
        /// </summary>
        private void SetupBuildSetting()
        {
            // ビルド設定にインスペクタの値を入れる
            buildSetting.TopColor = color;
            buildSetting.BottomColor = bottomColor;
            buildSetting.MainTextSize = renderMainFontSize;
            buildSetting.RubyTextSize = renderRubyFontSize;
            buildSetting.LineSpace = lineSpace;
            buildSetting.LetterSpace = letterSpace;
            buildSetting.TextAlignment = textAlignment;
            buildSetting.Proportional = forceProportional;
        }


        /// <summary>
        /// フォント状態をリフレッシュします
        /// </summary>
        private void RefreshFont()
        {
            // もしフォントパラメータのいづれかが異なる場合は
            if (fontData.Font != font || fontData.MainFontSize != requestMainFontSize || fontData.RubyFontSize != requestRubyFontSize || fontData.MainFontStyle != style || fontData.RubyFontStyle != style)
            {
                // フォントのリセットを行って再構築をマーク
                fontData.Reset(font, requestMainFontSize, requestRubyFontSize, style, style, gameText);
                needRebuild = true;
                return;
            }


            // 同じ値ならリフレッシュだけする
            fontData.Refresh();
        }


        /// <summary>
        /// フォントテクスチャを取得します。
        /// </summary>
        /// <returns>有効なフォントテクスチャを返しますが、フォントが設定されていない場合は基底のメインテクスチャを返します</returns>
        private Texture GetFontTexture()
        {
            // フォントが設定されているならフォントテクスチャを返す
            return fontData.Font != null ? fontData.Font.material.mainTexture : base.mainTexture;
        }
        #endregion


        #region UIイベントハンドラ
        /// <summary>
        /// メッシュの再生成の要求に対応します
        /// </summary>
        /// <param name="vertexHelper">生成したメッシュを送るためのヘルパオブジェクト</param>
        protected override void OnPopulateMesh(VertexHelper vertexHelper)
        {
            // 初期化を呼び続ける
            Initialize();


            // 文字列のパースエラーが起きていたら
            if (gameText.LastParseError)
            {
                // 何もせず終了
                return;
            }


            // 再構築が必要なら
            if (needRebuild)
            {
                // 再構築を行う
                SetupBuildSetting();
                verticesCache.BuildVertices(gameText, fontData, ref buildSetting);
                cachedBoundingRect = verticesCache.BoundingRect;


                // 再構築フラグを下ろす
                needRebuild = false;
            }


            // 頂点バッファを転送する
            currentVertexHelper = vertexHelper;
            currentVertexHelper.Clear();
            verticesCache.UploadMainTextVertices(vertices => vertexUploader(vertices));
            verticesCache.UploadRubyTextVertices(vertices => vertexUploader(vertices));
        }


        /// <summary>
        /// 頂点キャッシュロジックから渡された1文字分の頂点データをアップロードします
        /// </summary>
        /// <param name="vertices">アップロードするべき頂点データ</param>
        protected virtual void UploadVertex(UIVertex[] vertices)
        {
            // 描画するべき先の矩形を取得
            var drawTargetRect = rectTransform.rect;


            // 全頂点回る
            for (int i = 0; i < vertices.Length; ++i)
            {
                // キャンバス座標を作る
                Vector3 canvasPos;
                canvasPos.x = drawTargetRect.xMin + vertices[i].position.x;
                canvasPos.y = (drawTargetRect.yMax - cachedBoundingRect.yMax) + vertices[i].position.y;
                canvasPos.z = vertices[i].position.z;


                // 作った座標を自身に戻す
                vertices[i].position = canvasPos;
            }


            // キャンバス座標に変換済みの頂点を渡す
            currentVertexHelper.AddUIVertexQuad(vertices);
        }
        #endregion


        #region レイアウト計算ロジック（ILayoutElementの空実装）
        /// <summary>
        /// 横幅のレイアウト計算を行いますが、この関数は何もしません。
        /// </summary>
        public void CalculateLayoutInputHorizontal()
        {
        }


        /// <summary>
        /// 縦幅のレイアウト計算を行いますが、この関数は何もしません。
        /// </summary>
        public void CalculateLayoutInputVertical()
        {
        }
        #endregion


        #region Unityエディタイベントハンドラとエディタ専用実装
#if UNITY_EDITOR
        /// <summary>
        /// コンポーネントのインスペクタリセットを行います
        /// </summary>
        protected override void Reset()
        {
            // ベースのリセットを呼ぶ
            base.Reset();


            // このコンポーネントが持つインスペクタを、標準的なパラメータでリセットをする
            font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            style = FontStyle.Normal;
            requestMainFontSize = 20;
            requestRubyFontSize = 10;
            renderMainFontSize = 20;
            renderRubyFontSize = 10;
            color = Color.grey;
            bottomColor = color;
            lineSpace = 1.3f;
            letterSpace = 1.0f;
            textAlignment = TextAlignment.Left;
            forceProportional = false;
            text = $"[{GetType().Name}]<{GetType().Name}>";
        }


        /// <summary>
        /// インスペクタの入力に対して検証をします
        /// </summary>
        protected override void OnValidate()
        {
            // ベースのValidateを呼ぶ
            base.OnValidate();


            // 初期化をここでも呼ぶ
            Initialize();


            // テキストの入力が変化したのなら
            if (gameText.OriginalText != text)
            {
                // テキストのパースをするがパースエラーなら
                if (!gameText.Parse(text))
                {
                    // 空文字列で回避
                    gameText.Parse(string.Empty);
                }
            }


            // フォントのリフレッシュもする
            fontData.Refresh();


            // 再構築が必要であることをマーク
            needRebuild = true;
        }
#endif
        #endregion
    }
    #endregion



    #region ゲームテキストクラス
    /// <summary>
    /// ゲーム上で装飾付きテキストを扱いやすくしたゲームテキストクラスです
    /// </summary>
    public class GameText
    {
        // クラス変数宣言
        private static readonly Dictionary<char, byte> HexToDigitTable;

        // メンバ変数定義
        private string mainText;
        private string rubyText;
        private List<char> mainTextCharaList;
        private List<char> rubyTextCharaList;
        private List<DecorationEntry> decorationEntryList;



        /// <summary>
        /// 前回パースしたオリジナル文字列
        /// </summary>
        public string OriginalText { get; private set; }


        /// <summary>
        /// 最後にパースエラーを起こした時のメッセージ
        /// </summary>
        public string LastParseErrorMessage { get; private set; }


        /// <summary>
        /// 最後にパースエラーが発生したかどうか
        /// </summary>
        public bool LastParseError { get; private set; }


        /// <summary>
        /// 本文の文字コレクションを取得します
        /// </summary>
        public ReadOnlyCollection<char> MainTextCharacters { get; private set; }


        /// <summary>
        /// ルビの文字コレクションを取得します
        /// </summary>
        /// <remarks>
        /// 全ルビ文字列が結合されています。各本文の単語ごとのルビを取り出したい場合は、RubyEntry情報から取得して下さい
        /// </remarks>
        public ReadOnlyCollection<char> RubyTextCharacters { get; private set; }


        /// <summary>
        /// 本文の文字列を取得します
        /// </summary>
        public string MainText => mainText ?? (mainText = new string(mainTextCharaList.ToArray()));


        /// <summary>
        /// ルビの文字列を取得します。ただし、全ルビの結合された状態になっている事に注意して下さい。
        /// </summary>
        public string RubyText => rubyText ?? (rubyText = new string(rubyTextCharaList.ToArray()));


        /// <summary>
        /// 本文に対する装飾エントリの数を取得します
        /// </summary>
        public int DecorationEntryCount => decorationEntryList.Count;



        #region コンストラクタ
        /// <summary>
        /// GameText クラスを初期化します
        /// </summary>
        static GameText()
        {
            // 16進数文字から数値へ変換するテーブルを初期化する
            HexToDigitTable = new Dictionary<char, byte>()
            {
                {'0',0},{'1',1},{'2',2},{'3',3},{'4',4},{'5',5},{'6',6},{'7',7},{'8',8},
                {'9',9},{'a',10},{'b',11},{'c',12},{'d',13},{'e',14},{'f',15}
            };
        }


        /// <summary>
        /// GameText のインスタンスを初期化します
        /// </summary>
        public GameText()
        {
            // それぞれのインスタンスを生成する
            OriginalText = string.Empty;
            mainText = null;
            rubyText = null;
            LastParseErrorMessage = string.Empty;
            mainTextCharaList = new List<char>();
            rubyTextCharaList = new List<char>();
            MainTextCharacters = new ReadOnlyCollection<char>(mainTextCharaList);
            RubyTextCharacters = new ReadOnlyCollection<char>(rubyTextCharaList);
            decorationEntryList = new List<DecorationEntry>();
        }


        /// <summary>
        /// GameText のインスタンスを初期化します
        /// </summary>
        /// <param name="initialCharaBufferSize">予め想定している本文及びルビの文字バッファサイズ</param>
        /// <param name="initialDecorationBufferSize">予め想定している装飾バッファサイズ</param>
        public GameText(int initialCharaBufferSize, int initialDecorationBufferSize)
        {
            // それぞれのインスタンスを生成する
            OriginalText = string.Empty;
            mainText = null;
            rubyText = null;
            LastParseErrorMessage = string.Empty;
            mainTextCharaList = new List<char>(initialCharaBufferSize);
            rubyTextCharaList = new List<char>(initialCharaBufferSize);
            MainTextCharacters = new ReadOnlyCollection<char>(mainTextCharaList);
            RubyTextCharacters = new ReadOnlyCollection<char>(rubyTextCharaList);
            decorationEntryList = new List<DecorationEntry>(initialDecorationBufferSize);
        }
        #endregion


        #region 汎用ロジック関数
        /// <summary>
        /// 指定されたインデックスの装飾エントリを取得します
        /// </summary>
        /// <param name="index">取り出すエントリのインデックス</param>
        /// <param name="decorationEntry">取り出したエントリを格納する参照</param>
        public bool TryGetDecorationEntry(int index, out DecorationEntry decorationEntry)
        {
            // インデックスが参照外になる位置なら
            if (index < 0 || decorationEntryList.Count <= index)
            {
                // 取得に失敗を返す
                decorationEntry = default(DecorationEntry);
                return false;
            }


            // そもそも装飾エントリが空なら
            if (decorationEntryList.Count == 0)
            {
                // 取得は確実に失敗する
                decorationEntry = default(DecorationEntry);
                return false;
            }


            // 装飾エントリのコピーして成功を返す
            decorationEntry = decorationEntryList[index];
            return true;
        }
        #endregion


        #region パーサーのメインループ
        /// <summary>
        /// 指定されたゲームテキストをパースします。
        /// また、パースエラーが発生した場合は、現在の文字列状態が空になります。
        /// </summary>
        /// <param name="gameText">パースするゲームテキスト</param>
        /// <returns>パースに成功した場合は true を、失敗した場合は false を返します</returns>
        /// <exception cref="ArgumentNullException">gameText が null です</exception>
        public bool Parse(string gameText)
        {
            // null を渡されたら
            if (gameText == null)
            {
                // 流石に何も出来ない
                throw new ArgumentNullException(nameof(gameText));
            }


            // 同じ文字列を渡されたのなら
            if (gameText == OriginalText)
            {
                // 直ちに終了する
                return true;
            }


            // パースコンテキストの初期化
            var context = new ParseContext();
            context.CurrentState = ParseState.TakeMainCharacter;
            context.OriginalText = gameText;
            context.CurrentIndex = 0;
            context.SkipCount = 0;
            context.BackCount = 0;
            context.CurrentRowNumber = 1;
            context.CurrentColumnNumber = 0;
            context.CurrentCharacter = default(char);
            context.MainTextCharaList = mainTextCharaList;
            context.MainTextCharaList.Clear();
            context.RubyTextCharaList = rubyTextCharaList;
            context.RubyTextCharaList.Clear();
            context.DecorationEntryList = decorationEntryList;
            context.DecorationEntryList.Clear();
            context.TemporaryCharaList = new List<char>();


            // パースエラー状態をリセットする
            LastParseError = false;
            LastParseErrorMessage = string.Empty;


            // 文字数分ループする
            for (int i = 0; i < gameText.Length; ++i)
            {
                // コンテキスト状態の更新
                context.CurrentIndex = i;
                context.CurrentCharacter = gameText[i];
                context.CurrentColumnNumber += 1;


                // 文字の読み取りスキップ数が有効数なら
                if (context.SkipCount > 0)
                {
                    // デクリメントして次へ
                    --context.SkipCount;
                    continue;
                }


                // パーサーの状態に応じて呼び出す関数を切り替える
                switch (context.CurrentState)
                {
                    // 本文の文字を取り出す状態なら、本文の文字を取り出すパース処理を行う
                    case ParseState.TakeMainCharacter:
                        ParseTakeMainCharacter(ref context);
                        break;


                    // 装飾される本文の文字を取り出す状態なら、装飾される本文の文字を取り出すパース処理を行う
                    case ParseState.TakeDecoratedMainCharacter:
                        ParseTakeDecoratedMainCharacter(ref context);
                        break;


                    // ルビの文字を取り出す状態なら、ルビの文字を取り出すパース処理を行う
                    case ParseState.TakeRubyCharacter:
                        ParseTakeRubyCharacter(ref context);
                        break;


                    // カラーコード文字を取り出す状態なら、空コードの文字を取り出すパース処理を行う
                    case ParseState.TakeColorCodeCharacter:
                        ParseTakeColorCodeCharacter(ref context);
                        break;


                    // 文字サイズ文字を取り出す状態なら、文字サイズ文字を取り出すパース処理を行う
                    case ParseState.TakeSizeCharacter:
                        ParseTakeTextSizeCharacter(ref context);
                        break;
                }


                // もしパースエラーが発生しているのなら
                if (context.CurrentState == ParseState.Error)
                {
                    // ループを脱出する
                    break;
                }


                // バックカウントの値分カウンタを戻す
                i -= context.BackCount;
                context.CurrentColumnNumber -= context.BackCount;
                context.BackCount = 0;
            }


            // もしTakeMainCharacterとError以外のステータスで終了したのなら
            if (context.CurrentState != ParseState.TakeMainCharacter && context.CurrentState != ParseState.Error)
            {
                // 正しい文の終了の仕方をしていないのでパースエラー
                SetParseError(ref context, "文末が正しい終了の仕方をしていません");
            }


            // もしパースエラーによる脱出なら
            if (context.CurrentState == ParseState.Error)
            {
                // 状態をリセットしてパースに失敗したことを返す
                OriginalText = string.Empty;
                mainText = null;
                rubyText = null;
                mainTextCharaList.Clear();
                rubyTextCharaList.Clear();
                decorationEntryList.Clear();
                return false;
            }


            // オリジナルのテキストを覚えてパースに成功したことを返す
            OriginalText = gameText;
            mainText = null;
            rubyText = null;
            return true;
        }
        #endregion


        #region パーサーの各ステート処理
        /// <summary>
        /// 本文の文字を取り出す状態のパースを行います
        /// </summary>
        /// <param name="context">パーサーのコンテキスト</param>
        private void ParseTakeMainCharacter(ref ParseContext context)
        {
            // もしオープンブラケットが来たら
            if (context.CurrentCharacter == '[')
            {
                // 次に続く文字が無いなら
                if (context.OriginalText.Length == context.CurrentIndex + 1)
                {
                    // 次に続く文字が無いのは流石に文法に誤りが有るのでパースエラーになる
                    SetParseError(ref context, "次に読み取るための文字が見つかりませんでした");
                    return;
                }


                // 次の文字もオープンブラケットなら
                if (context.OriginalText[context.CurrentIndex + 1] == '[')
                {
                    // ただのオープンブラケットとして文字を入れて1文字分スキップ
                    context.MainTextCharaList.Add('[');
                    context.SkipCount = 1;
                    return;
                }


                // 装飾エントリデータを作って追加する
                var decorationEntry = new DecorationEntry();
                decorationEntry.MainTextStartIndex = context.MainTextCharaList.Count;
                decorationEntry.MainTextCharacterCount = 0;
                decorationEntry.RubyTextStartIndex = -1;
                decorationEntry.RubyTextCharacterCount = 0;
                decorationEntry.TextSize = -1;
                decorationEntry.UseColor = false;
                decorationEntry.TopTextColor = 0xFFFFFFFF;
                decorationEntry.BottomTextColor = 0xFFFFFFFF;
                context.DecorationEntryList.Add(decorationEntry);


                // 装飾される本文の文字読み取り開始として状態を変える
                context.CurrentState = ParseState.TakeDecoratedMainCharacter;
                return;
            }


            // もしキャリッジリターンが来たら
            else if (context.CurrentCharacter == '\r')
            {
                // 何もせず無視
                return;
            }


            // もしラインフィールドが来たら
            else if (context.CurrentCharacter == '\n')
            {
                // 行番号をインクリメントして列番号をリセット
                ++context.CurrentRowNumber;
                context.CurrentColumnNumber = 0;
            }


            // 基本はそのまま受け取る（他の記号はただの文字として受け入れる）
            context.MainTextCharaList.Add(context.CurrentCharacter);
        }


        /// <summary>
        /// 装飾される本文の文字を取り出す状態のパースを行います
        /// </summary>
        /// <param name="context">パーサーのコンテキスト</param>
        private void ParseTakeDecoratedMainCharacter(ref ParseContext context)
        {
            // もしクローズブラケットが来たら
            if (context.CurrentCharacter == ']')
            {
                // 次に続く文字が無いなら
                if (context.OriginalText.Length == context.CurrentIndex + 1)
                {
                    // 次に続く文字が無いのは流石に文法に誤りが有るのでパースエラーになる
                    SetParseError(ref context, "次に読み取るための文字が見つかりませんでした");
                    return;
                }


                // 次の文字もクローズブラケットなら
                if (context.OriginalText[context.CurrentIndex + 1] == ']')
                {
                    // ただのクローズブラケットとして文字を入れて1文字分スキップ
                    context.MainTextCharaList.Add(']');
                    context.SkipCount = 1;
                    return;
                }


                // 次の文字がオープンアングルでないなら
                if (context.OriginalText[context.CurrentIndex + 1] != '<')
                {
                    // パースエラーとして状態を切り替えて終了
                    SetParseError(ref context, "本文の装飾範囲閉じ記号の次は、必ず装飾開始記号で始まらなければなりません");
                    return;
                }


                // 最新の装飾エントリデータを取り出して更新する
                var decorationEntry = context.DecorationEntryList[context.DecorationEntryList.Count - 1];
                decorationEntry.MainTextCharacterCount = context.MainTextCharaList.Count - decorationEntry.MainTextStartIndex;
                decorationEntry.RubyTextStartIndex = context.RubyTextCharaList.Count;
                context.DecorationEntryList[context.DecorationEntryList.Count - 1] = decorationEntry;


                // 1文字分スキップ
                context.SkipCount = 1;


                // ルビ文字取り出し状態にする
                context.CurrentState = ParseState.TakeRubyCharacter;
                return;
            }


            // もしオープンブラケットがきたら
            else if (context.CurrentCharacter == '[')
            {
                // 次に続く文字が無いなら
                if (context.OriginalText.Length == context.CurrentIndex + 1)
                {
                    // 次に続く文字が無いのは流石に文法に誤りが有るのでパースエラーになる
                    SetParseError(ref context, "次に読み取るための文字が見つかりませんでした");
                    return;
                }


                // 次の文字もオープンブラケットでないなら
                if (context.OriginalText[context.CurrentIndex + 1] != '[')
                {
                    // 装飾された本文の中に装飾開始文字を含めるのはパースエラー
                    SetParseError(ref context, "装飾される本文に装飾開始記号を含めることは出来ません、オープンブラケットを入れる場合は '[[' を記述して下さい");
                    return;
                }


                // 1文字分スキップ
                context.SkipCount = 1;
            }


            // キャリッジリターンが来たら
            else if (context.CurrentCharacter == '\r')
            {
                // 何もせず終了（もはや改行コードとしても見ない）
                return;
            }


            // ラインフィールドが来たら
            else if (context.CurrentCharacter == '\n')
            {
                // 装飾される文字は途中改行を許可しないためパースエラー
                SetParseError(ref context, "装飾される本文は改行をすることは出来ません");
                return;
            }


            // 文字として受け入れる
            context.MainTextCharaList.Add(context.CurrentCharacter);
        }


        /// <summary>
        /// ルビの文字を取り出す状態のパースを行います
        /// </summary>
        /// <param name="context">パーサーのコンテキスト</param>
        private void ParseTakeRubyCharacter(ref ParseContext context)
        {
            // もしクローズアングルが来たら
            if (context.CurrentCharacter == '>')
            {
                // 次に続く文字が無いか、次の文字がクローズアングルでないなら
                if (context.OriginalText.Length == context.CurrentIndex + 1 || context.OriginalText[context.CurrentIndex + 1] != '>')
                {
                    // 最新の装飾エントリデータを取り出して更新する
                    var decorationEntry = context.DecorationEntryList[context.DecorationEntryList.Count - 1];
                    decorationEntry.RubyTextCharacterCount = context.RubyTextCharaList.Count - decorationEntry.RubyTextStartIndex;
                    decorationEntry.RubyTextStartIndex = decorationEntry.RubyTextCharacterCount == 0 ? -1 : decorationEntry.RubyTextStartIndex;
                    context.DecorationEntryList[context.DecorationEntryList.Count - 1] = decorationEntry;


                    // 本文の文字を取り出すパース状態にする
                    context.CurrentState = ParseState.TakeMainCharacter;
                    return;
                }


                // 1文字分スキップ
                context.SkipCount = 1;
            }


            // もしオープンアングルが来たら
            else if (context.CurrentCharacter == '<')
            {
                // 次に続く文字が無いなら
                if (context.OriginalText.Length == context.CurrentIndex + 1)
                {
                    // 次に続く文字が無いのは流石に文法に誤りが有るのでパースエラーになる
                    SetParseError(ref context, "次に読み取るための文字が見つかりませんでした");
                    return;
                }


                // もし次の文字もオープンアングルで無いなら
                if (context.OriginalText[context.CurrentIndex + 1] != '<')
                {
                    // ルビ文字にルビ開始記号を含める事は出来ないのでパースエラー
                    SetParseError(ref context, "装飾文字列に装飾開始記号を含めることは出来ません、オープンアングルを入れる場合は '<<' を記述して下さい");
                    return;
                }


                // 1文字分スキップ
                context.SkipCount = 1;
            }


            // もしカンマが来たら
            else if (context.CurrentCharacter == ',')
            {
                // 空白の読み飛ばしをするがパースエラーを起こしていたら
                SkipWhiteSpaceCharacter(ref context);
                if (context.CurrentState == ParseState.Error)
                {
                    // このまま終了する
                    return;
                }


                // 次に読み取りを開始するインデックスを計算
                var nextIndex = context.CurrentIndex + context.SkipCount + 1;


                // もしハッシュ文字が次に来る文字なら
                if (context.OriginalText[nextIndex] == '#')
                {
                    // スキップカウントをインクリメントする
                    context.SkipCount += 1;


                    // カラーコード取り出し状態にする
                    context.CurrentState = ParseState.TakeColorCodeCharacter;
                    context.TemporaryCharaList.Clear();
                    return;
                }


                // もし10進数字が次に来る文字なら
                if (char.IsDigit(context.OriginalText[nextIndex]))
                {
                    // サイズ取り出し状態にする
                    context.CurrentState = ParseState.TakeSizeCharacter;
                    context.TemporaryCharaList.Clear();
                    return;
                }


                // ハッシュ文字でも数字でもない場合は、構文的に問題があるためパースエラー
                SetParseError(ref context, $"装飾文字列の引数は、カラーコードまたは、文字サイズ指定のみ可能です NotSupportedCharacter='{context.CurrentCharacter}'");
                return;
            }


            // もしキャリッジリターンが来たら
            else if (context.CurrentCharacter == '\r')
            {
                // 何もせず終了（もはや以下同文）
                return;
            }


            // ラインフィールドが来たら
            else if (context.CurrentCharacter == '\n')
            {
                // ルビ文字に改行を許可しないためパースエラー
                SetParseError(ref context, "装飾文字列は改行をすることは出来ません");
                return;
            }


            // ルビ文字として受け入れる
            context.RubyTextCharaList.Add(context.CurrentCharacter);
        }


        /// <summary>
        /// カラーコード文字を取り出す状態のパースを行います
        /// </summary>
        /// <param name="context">パーサーコンテキスト</param>
        private void ParseTakeColorCodeCharacter(ref ParseContext context)
        {
            // 現在の文字を変数に置く
            var currentChara = context.CurrentCharacter;


            // もしクローズアングルまたは、カンマがきたら
            if (currentChara == '>' || currentChara == ',')
            {
                // もし一時格納文字リストが8桁未満（RGBAの各16進数文字数未満）なら
                if (context.TemporaryCharaList.Count < 8)
                {
                    // カラーコードとしての表現が出来ていないためパースエラー
                    SetParseError(ref context, $"カラーコード文字列が8桁未満です Current={new string(context.TemporaryCharaList.ToArray())}");
                    return;
                }


                // 最新の装飾エントリデータを取り出す
                var decorationEntry = context.DecorationEntryList[context.DecorationEntryList.Count - 1];


                // まだカラー変更が有効でないなら
                if (!decorationEntry.UseColor)
                {
                    // カラー変更は有効にして上部と下部のテキストカラー値として設定する
                    decorationEntry.UseColor = true;
                    decorationEntry.TopTextColor = ConvertTemporaryCharaToUint(ref context);
                    decorationEntry.BottomTextColor = decorationEntry.TopTextColor;
                }
                else
                {
                    // カラーが有効済みなら以降は無条件に下部テキストカラー値として設定する
                    decorationEntry.BottomTextColor = ConvertTemporaryCharaToUint(ref context);
                }


                // 最新の装飾エントリーとして更新する
                context.DecorationEntryList[context.DecorationEntryList.Count - 1] = decorationEntry;


                // 1文字分読み取りも巻き戻す
                context.BackCount = 1;


                // ルビ文字取り出し状態にする
                context.CurrentState = ParseState.TakeRubyCharacter;
                return;
            }


            // もし16進数以外の文字なら
            else if (!char.IsDigit(currentChara) && !((currentChara >= 'a' && currentChara <= 'f') || (currentChara >= 'A' && currentChara <= 'F')))
            {
                // まだ、一時格納文字リストが8桁未満なら
                if (context.TemporaryCharaList.Count < 8)
                {
                    // カラーコードは16進数表記以外認めないためパースエラー
                    SetParseError(ref context, $"カラーコードは16進数で定義されている必要があります NotSupportedCharacter='{currentChara}'");
                    return;
                }
            }


            // もし、既に一時格納文字リストが8桁になっていた場合は
            if (context.TemporaryCharaList.Count == 8)
            {
                // もし空白文字なら
                if (char.IsWhiteSpace(currentChara))
                {
                    // 何事も無かったかのように読み飛ばす
                    return;
                }


                // これ以上の入力は符号なし32bit整数の境界外になるのでパースエラー
                SetParseError(ref context, "カラーコードが、符号なし32bit整数の境界を超えました");
                return;
            }


            // 一時格納文字リストに追加
            context.TemporaryCharaList.Add(char.ToLower(currentChara));
        }


        /// <summary>
        /// 文字サイズ文字を取り出す状態のパースを行います
        /// </summary>
        /// <param name="context">パーサーコンテキスト</param>
        private void ParseTakeTextSizeCharacter(ref ParseContext context)
        {
            // 現在の文字を変数に置く
            var currentChara = context.CurrentCharacter;


            // もしクローズアングルまたは、カンマがきたら
            if (currentChara == '>' || currentChara == ',')
            {
                // 最新の装飾エントリデータを取り出して更新する
                var decorationEntry = context.DecorationEntryList[context.DecorationEntryList.Count - 1];
                decorationEntry.TextSize = ConvertTemporaryCharaToInt(ref context);
                context.DecorationEntryList[context.DecorationEntryList.Count - 1] = decorationEntry;


                // 1文字分読み取りも巻き戻す
                context.BackCount = 1;


                // ルビ文字取り出し状態にする
                context.CurrentState = ParseState.TakeRubyCharacter;
                return;
            }


            // もし数字以外なら
            else if (!char.IsDigit(currentChara))
            {
                // 空白の読み飛ばしをするがパースエラーを起こしていたら
                SkipWhiteSpaceCharacter(ref context);
                if (context.CurrentState == ParseState.Error)
                {
                    // このまま終了する
                    return;
                }


                // 次に読み取りを開始するインデックスを計算
                var nextIndex = context.CurrentIndex + context.SkipCount + 1;


                // 次に読み取る文字がクローズアングルまたは、カンマなら
                if (context.OriginalText[nextIndex] == '>' || context.OriginalText[nextIndex] == ',')
                {
                    // 何事もなかったかのように読み飛ばす
                    return;
                }


                // 数字以外は正の整数としての書式ではないとしてパースエラー
                SetParseError(ref context, $"文字サイズは正の整数でなければなりません NotSupportedCharacter='{currentChara}'");
                return;
            }


            // 一時格納文字リストに追加
            context.TemporaryCharaList.Add(char.ToLower(currentChara));
        }
        #endregion


        #region パーサーユーティリティ関数
        /// <summary>
        /// パースエラーの設定を行います。また、この関数は行と列の番号をメッセージの末尾に自動的に付与します。
        /// </summary>
        /// <param name="context">パースエラーを起こしたコンテキスト</param>
        /// <param name="message">設定するパースエラーメッセージ</param>
        private void SetParseError(ref ParseContext context, string message)
        {
            // コンテキストをパースエラー状態にしてメッセージを設定する
            context.CurrentState = ParseState.Error;
            LastParseError = true;
            LastParseErrorMessage = $"{(message ?? string.Empty)} line:{context.CurrentRowNumber} col:{context.CurrentColumnNumber}";
        }


        /// <summary>
        /// 空白文字が続く間、読み飛ばすためコンテキスト上でスキップ処理をします
        /// </summary>
        /// <param name="context">パーサーのコンテキスト</param>
        private void SkipWhiteSpaceCharacter(ref ParseContext context)
        {
            // 次に続く文字が無いなら
            if (context.OriginalText.Length == context.CurrentIndex + 1)
            {
                // 次に続く文字が無いのは想定されていない可能性があるのでパースエラーになる
                SetParseError(ref context, "次に読み取るための文字が見つかりませんでした");
                return;
            }


            // 空白文字がある間はひたすら読み飛ばす
            context.SkipCount = 1;
            while (char.IsWhiteSpace(context.OriginalText[context.CurrentIndex + context.SkipCount]))
            {
                // スキップカウントをインクリメント
                ++context.SkipCount;


                // もし次に続く文字が無いなら
                if (context.OriginalText.Length == context.CurrentIndex + context.SkipCount)
                {
                    // 次に続く文字が無いのは流石に文法に誤りが有るのでパースエラーになる
                    SetParseError(ref context, "次に読み取るための文字が見つかりませんでした");
                    return;
                }
            }


            // 最後に読み取った文字は空白ではないのでカウントしたスキップをデクリメントする
            --context.SkipCount;
        }


        /// <summary>
        /// パーサーのコンテキストに含まれる一時文字リストから符号無し32bit整数変数として変換します。
        /// </summary>
        /// <param name="context">符号なし32bit整数に変換するための一時文字リストに文字が含まれるパーサーコンテキスト</param>
        /// <returns>変換した結果の値を返します</returns>
        private uint ConvertTemporaryCharaToUint(ref ParseContext context)
        {
            // 一時文字リストが空または8桁以外なら
            if (context.TemporaryCharaList.Count == 0 || context.TemporaryCharaList.Count != 8)
            {
                // パースエラーとして処理して0を返す
                SetParseError(ref context, "16進数文字列は必ず8桁でなければなりません");
                return 0;
            }


            // シフト用変数と結果変数を用意して8ループする
            var shiftValue = 28;
            var result = 0u;
            for (int i = 0; i < 8; ++i)
            {
                // 桁ごとにシフトしながら結果変数に入れていく
                result |= (uint)HexToDigitTable[context.TemporaryCharaList[i]] << shiftValue;
                shiftValue -= 4;
            }


            // 結果を返す
            return result;
        }


        /// <summary>
        /// パーサーのコンテキストに含まれる一時文字リストから符号付き32bit整数変数として変換します
        /// </summary>
        /// <param name="context">符号付き32bit整数に変換するための一時文字リストに文字が含まれるパーサーコンテキスト</param>
        /// <returns>変換した結果の値を返します</returns>
        private int ConvertTemporaryCharaToInt(ref ParseContext context)
        {
            // 結果変数を用意してループする
            var result = 0;
            foreach (var digit in context.TemporaryCharaList)
            {
                // 結果変数の桁を繰り上げてから、数字を数値として加算する
                result *= 10;
                result += digit - 48;
            }


            // 結果を返す
            return result;
        }
        #endregion



        #region ゲームテキスト向け型定義
        /// <summary>
        /// パースの状態を表現します
        /// </summary>
        private enum ParseState
        {
            /// <summary>
            /// 本文の文字を取り出しています
            /// </summary>
            TakeMainCharacter,

            /// <summary>
            /// 装飾された本文の文字を取り出しています
            /// </summary>
            TakeDecoratedMainCharacter,

            /// <summary>
            /// ルビの文字を取り出しています
            /// </summary>
            TakeRubyCharacter,

            /// <summary>
            /// カラーコード文字を取り出しています
            /// </summary>
            TakeColorCodeCharacter,

            /// <summary>
            /// サイズ文字を取り出しています
            /// </summary>
            TakeSizeCharacter,

            /// <summary>
            /// パースエラーが発生しました
            /// </summary>
            Error,
        }



        /// <summary>
        /// パーサーのパースコンテキストを持つ構造体です
        /// </summary>
        private struct ParseContext
        {
            /// <summary>
            /// 現在のパースステート
            /// </summary>
            public ParseState CurrentState;


            /// <summary>
            /// パース対象のオリジナル文字列
            /// </summary>
            public string OriginalText;


            /// <summary>
            /// 現在処理しているオリジナル文字列のインデックス
            /// </summary>
            public int CurrentIndex;


            /// <summary>
            /// 文字の読み取りスキップカウント数
            /// </summary>
            public int SkipCount;


            /// <summary>
            /// 文字の読み取り巻き戻りカウント数
            /// </summary>
            public int BackCount;


            /// <summary>
            /// 現在処理している文字
            /// </summary>
            public char CurrentCharacter;


            /// <summary>
            /// 現在処理している列番号
            /// </summary>
            public int CurrentColumnNumber;


            /// <summary>
            /// 現在処理している行番号
            /// </summary>
            public int CurrentRowNumber;


            /// <summary>
            /// パースした本文の文字データを入れるリスト
            /// </summary>
            public List<char> MainTextCharaList;


            /// <summary>
            /// パースしたルビの文字データを入れるリスト
            /// </summary>
            public List<char> RubyTextCharaList;


            /// <summary>
            /// パース中の一時的な文字リスト
            /// </summary>
            public List<char> TemporaryCharaList;


            /// <summary>
            /// パースした装飾エントリを入れるリスト
            /// </summary>
            public List<DecorationEntry> DecorationEntryList;
        }



        /// <summary>
        /// 装飾データを持つ構造体です
        /// </summary>
        public struct DecorationEntry
        {
            /// <summary>
            /// 装飾される本文の開始インデックス
            /// </summary>
            public int MainTextStartIndex;


            /// <summary>
            /// 装飾される本文のインデックスからの文字数
            /// </summary>
            public int MainTextCharacterCount;


            /// <summary>
            /// 装飾するルビの文字開始インデックス。（-1はルビを含みません）
            /// </summary>
            public int RubyTextStartIndex;


            /// <summary>
            /// 装飾するルビの文字数。（0はルビを含みません）
            /// </summary>
            public int RubyTextCharacterCount;


            /// <summary>
            /// 装飾する本文のサイズ（-1は変化しません）
            /// </summary>
            public int TextSize;


            /// <summary>
            /// 文字色変更を利用するかどうか
            /// </summary>
            public bool UseColor;


            /// <summary>
            /// 装飾する本文の上側文字色
            /// </summary>
            public uint TopTextColor;


            /// <summary>
            /// 装飾する本文の下側文字色
            /// </summary>
            public uint BottomTextColor;
        }
        #endregion
    }
    #endregion



    #region フォントデータクラス
    /// <summary>
    /// ルビ表示可能なテキストUIのフォントデータクラスです
    /// </summary>
    public class RubyableTextFontData
    {
        // メンバ変数定義
        private string prevMainText;
        private string prevRubyText;
        private GameText gameText;



        /// <summary>
        /// 使用するフォント
        /// </summary>
        public Font Font { get; private set; }


        /// <summary>
        /// Unityに要求するメインテキストフォントのサイズ
        /// </summary>
        public int MainFontSize { get; private set; }


        /// <summary>
        /// Unityに要求するルビテキストフォントのサイズ
        /// </summary>
        public int RubyFontSize { get; private set; }


        /// <summary>
        /// 使用するメインテキストフォントのスタイル
        /// </summary>
        public FontStyle MainFontStyle { get; private set; }


        /// <summary>
        /// 使用するルビテキストフォントのスタイル
        /// </summary>
        public FontStyle RubyFontStyle { get; private set; }


        /// <summary>
        /// フォントから表示しようとしている、メインテキストの最大グリフサイズ
        /// </summary>
        public Vector2 MaxMainTextGlyphSize { get; private set; }


        /// <summary>
        /// フォントから表示しようとしている、ルビテキストの最大グリフサイズ
        /// </summary>
        public Vector2 MaxRubyTextGlyphSize { get; private set; }



        /// <summary>
        /// RubyableTextFontData のインスタンスを初期化します
        /// </summary>
        /// <param name="font">使用するフォント</param>
        /// <param name="mainSize">メインテキストで使用するフォントサイズ</param>
        /// <param name="rubySize">ルビテキストで使用するフォントサイズ</param>
        /// <param name="mainStyle">メインテキストのフォントスタイル</param>
        /// <param name="rubyStyle">ルビテキストのフォントスタイル</param>
        /// <param name="gameText">メインテキストとルビテキストを持っているゲームテキスト</param>
        /// <exception cref="ArgumentNullException">font が null です</exception>
        /// <exception cref="ArgumentNullException">gameText が null です</exception>
        /// <exception cref="ArgumentException">mainSize または rubySize の値が 0 以下です</exception>
        public RubyableTextFontData(Font font, int mainSize, int rubySize, FontStyle mainStyle, FontStyle rubyStyle, GameText gameText)
        {
            // リセット関数を呼ぶ
            Reset(font, mainSize, rubySize, mainStyle, rubyStyle, gameText);
        }


        /// <summary>
        /// 内部のフォントパラメータをリセットします。
        /// </summary>
        /// <param name="font">使用するフォント</param>
        /// <param name="mainSize">メインテキストで使用するフォントサイズ</param>
        /// <param name="rubySize">ルビテキストで使用するフォントサイズ</param>
        /// <param name="mainStyle">メインテキストのフォントスタイル</param>
        /// <param name="rubyStyle">ルビテキストのフォントスタイル</param>
        /// <param name="gameText">メインテキストとルビテキストを持っているゲームテキスト</param>
        /// <exception cref="ArgumentNullException">font が null です</exception>
        /// <exception cref="ArgumentNullException">gameText が null です</exception>
        /// <exception cref="ArgumentException">mainSize または rubySize の値が 0 以下です</exception>
        public void Reset(Font font, int mainSize, int rubySize, FontStyle mainStyle, FontStyle rubyStyle, GameText gameText)
        {
            // フォントの参照が無いなら
            if (font == null)
            {
                // どのフォントのテクスチャを作れば良いんだろうか
                throw new ArgumentNullException(nameof(font));
            }


            // ゲームテキストがnullなら
            if (gameText == null)
            {
                // 何を元に文字列の要求をすればよいのか
                throw new ArgumentNullException(nameof(gameText));
            }


            // 要求フォントサイズが0以下なら
            if (mainSize <= 0 || rubySize <= 0)
            {
                // 流石に0以下のフォントサイズは無理
                throw new ArgumentException($"{nameof(mainSize)} または {nameof(rubySize)} の値が 0 以下です");
            }


            // フォントにメインテキスト用とルビテキスト用の文字列を要求する
            font.RequestCharactersInTexture(gameText.MainText, mainSize, mainStyle);
            font.RequestCharactersInTexture(gameText.RubyText, rubySize, rubyStyle);


            // 参照の更新をする
            Font = font;
            MainFontStyle = mainStyle;
            RubyFontStyle = rubyStyle;
            MainFontSize = mainSize;
            RubyFontSize = rubySize;
            prevMainText = gameText.MainText;
            prevRubyText = gameText.RubyText;
            this.gameText = gameText;


            // グリフサイズを覚える
            MaxMainTextGlyphSize = CalcuMaxGlyphSize(gameText.MainTextCharacters, mainSize, mainStyle);
            MaxRubyTextGlyphSize = CalcuMaxGlyphSize(gameText.RubyTextCharacters, rubySize, rubyStyle);
        }


        /// <summary>
        /// 現在の状態でフォントの状態をリフレッシュします。
        /// リフレッシュしたフォントには、このフォントデータインスタンスが必要とする文字の要求などが行われます。
        /// </summary>
        public void Refresh()
        {
            // 現在の設定でフォントテクスチャの準備を要求する
            Font.RequestCharactersInTexture(gameText.MainText, MainFontSize, MainFontStyle);
            Font.RequestCharactersInTexture(gameText.RubyText, RubyFontSize, RubyFontStyle);


            // メインテキストの内容が変わっていたら
            if (!ReferenceEquals(prevMainText, gameText.MainText))
            {
                // メインテキストのグリフサイズを再計算して最新を覚える
                MaxMainTextGlyphSize = CalcuMaxGlyphSize(gameText.MainTextCharacters, MainFontSize, MainFontStyle);
                prevMainText = gameText.MainText;
            }


            // ルビテキストの内容が変わっていたら
            if (!ReferenceEquals(prevRubyText, gameText.RubyText))
            {
                // ルビテキストのグリフサイズを再計算する
                MaxRubyTextGlyphSize = CalcuMaxGlyphSize(gameText.RubyTextCharacters, RubyFontSize, RubyFontStyle);
                prevRubyText = gameText.RubyText;
            }
        }


        /// <summary>
        /// 指定されたサイズとスタイルから現在のテキストで最大グリフサイズを取得します
        /// </summary>
        /// <param name="text">グリフ計算する対象になる文字を持ったテキスト</param>
        /// <param name="size">利用するフォントのサイズ</param>
        /// <param name="style">利用するフォントのスタイル</param>
        /// <returns>現在に保持しているゲームテキストを元に</returns>
        private Vector2 CalcuMaxGlyphSize(ReadOnlyCollection<char> text, int size, FontStyle style)
        {
            // 渡されたテキストから最大グリフサイズを覚える変数を宣言してループする
            var maxGlyphSize = Vector2.zero;
            foreach (var character in text)
            {
                // 文字情報の取得をするが出来なかったら（本来は事前にリクエストしているので取得できるはず）
                CharacterInfo info;
                if (!Font.GetCharacterInfo(character, out info, size, style))
                {
                    // そのまま次へ
                    continue;
                }


                // グリフの最大値を更新する
                maxGlyphSize.x = Mathf.Max(maxGlyphSize.x, info.glyphWidth);
                maxGlyphSize.y = Mathf.Max(maxGlyphSize.y, info.glyphHeight);
            }


            // 計算結果を返す
            return maxGlyphSize;
        }


        /// <summary>
        /// メインテキストで利用する文字情報を取得します
        /// </summary>
        /// <param name="character">取得したい情報の文字</param>
        /// <param name="info">取得した文字情報を格納する CharacterInfo の参照</param>
        /// <returns>情報を正しく取得できた場合は true を、取得できなかった場合は false を返します</returns>
        public bool GetMainCharacterInfo(char character, out CharacterInfo info)
        {
            // メインテキスト情報で文字情報の取得を試みる
            return Font.GetCharacterInfo(character, out info, MainFontSize, MainFontStyle);
        }


        /// <summary>
        /// ルビテキストで利用する文字情報を取得します
        /// </summary>
        /// <param name="character">取得したい情報の文字</param>
        /// <param name="info">取得した文字情報を格納する CharacterInfo の参照</param>
        /// <returns>情報を正しく取得できた場合は true を、取得できなかった場合は false を返します</returns>
        public bool GetRubyCharacterInfo(char character, out CharacterInfo info)
        {
            // ルビテキスト情報で文字情報の取得を試みる
            return Font.GetCharacterInfo(character, out info, RubyFontSize, RubyFontStyle);
        }
    }
    #endregion



    #region テキスト頂点キャッシュクラス
    /// <summary>
    /// ルビ表示可能なテキストUIの頂点キャッシュクラスです
    /// </summary>
    public class RubyableTextVerticesCache
    {
        // メンバ変数定義
        private UIVertex[] verticesCache;
        private UIVertex[] translateBuffer;
        private int mainCharacterCount;
        private int rubyCharacterCount;
        private ImtTextAnimator textAnimator;



        /// <summary>
        /// 構築されたルビ表示可能なテキストの境界矩形
        /// </summary>
        public Rect BoundingRect { get; private set; }



        #region コンストラクタ
        /// <summary>
        /// RubyableTextVerticesCache のインスタンスを初期化します
        /// </summary>
        public RubyableTextVerticesCache() : this(256)
        {
        }


        /// <summary>
        /// RubyableTextVerticesCache のインスタンスを初期化します
        /// </summary>
        /// <param name="initialCharacterCapacity">初期確保する本文とルビを含む文字容量</param>
        public RubyableTextVerticesCache(int initialCharacterCapacity)
        {
        }


        /// <summary>
        /// RubyableTextVerticesCache のインスタンスを初期化します
        /// </summary>
        /// <param name="initialCharacterCapacity">初期確保する本文とルビを含む文字容量</param>
        /// <param name="textAnimator">生成された頂点に対してアニメーションする場合のテキストアニメータ</param>
        public RubyableTextVerticesCache(int initialCharacterCapacity, ImtTextAnimator textAnimator)
        {
            // メンバの初期化をして、頂点キャッシュを確保する（1文字4頂点分なので4倍）
            BoundingRect = Rect.zero;
            verticesCache = new UIVertex[initialCharacterCapacity * 4];


            // 転送バッファを用意する
            translateBuffer = new UIVertex[4];


            // テキストアニメータも覚える
            this.textAnimator = textAnimator;
        }
        #endregion


        #region 頂点構築ルート関数
        /// <summary>
        /// ゲームテキストとフォントデータを用いて頂点データを構築します
        /// </summary>
        /// <param name="gameText">メッシュの構築元になるゲームテキスト</param>
        /// <param name="fontData">メッシュの構築元になるフォントデータ</param>
        /// <param name="setting">メッシュの構築設定オブジェクトの参照</param>
        /// <exception cref="ArgumentNullException">gameText が null です</exception>
        /// <exception cref="ArgumentNullException">fontData が null です</exception>
        public void BuildVertices(GameText gameText, RubyableTextFontData fontData, ref BuildSetting setting)
        {
            // gameTextがnullなら
            if (gameText == null)
            {
                // どのテキストのメッシュを作ればよいのか
                throw new ArgumentNullException(nameof(gameText));
            }


            // fontDataがnullなら
            if (fontData == null)
            {
                // どのフォントを使ってメッシュを作ればよいのか
                throw new ArgumentNullException(nameof(fontData));
            }


            // パースエラーが起きているゲームテキストなら
            if (gameText.LastParseError)
            {
                // 例外ではなく何もせず終了
                return;
            }


            // 頂点キャッシュの確保をしてクリアする
            EnsureVerticesCache(gameText);
            ClearVertices();


            // 計算に必要なデータを求める
            var mainTextScale = setting.MainTextSize / (float)fontData.MainFontSize;
            var rubyTextScale = setting.RubyTextSize / (float)fontData.RubyFontSize;
            var mainLineHeight = fontData.MaxMainTextGlyphSize.y * mainTextScale;
            var rubyLineHeight = fontData.MaxRubyTextGlyphSize.y * rubyTextScale;
            var totalLineHeight = (mainLineHeight + rubyLineHeight) * setting.LineSpace;


            // 初回装飾データも求める
            var decorationEntry = new GameText.DecorationEntry();
            var decorationAvailable = gameText.DecorationEntryCount > 0;
            if (decorationAvailable)
            {
                // 初回装飾データが取得できそうなら取得する
                gameText.TryGetDecorationEntry(0, out decorationEntry);
            }


            // 構築コンテキストを初期化する
            var context = new BuilderContext();
            context.GameText = gameText;
            context.FontData = fontData;
            context.BuildSetting = setting;
            context.VertexBufferContext.Vertices = verticesCache;
            context.VertexBufferContext.RubyVerticesOffset = gameText.MainTextCharacters.Count * 4;
            context.TypographyContext.CurrentMainCharacterIndex = 0;
            context.TypographyContext.CurrentRubyCharacterIndex = 0;
            context.DimensionContext.Offset = Vector2.zero;
            context.DimensionContext.MainTextScale = mainTextScale;
            context.DimensionContext.RubyTextScale = rubyTextScale;
            context.DimensionContext.MainLineHeight = mainLineHeight;
            context.DimensionContext.RubyLineHeight = rubyLineHeight;
            context.DimensionContext.LineHeight = totalLineHeight;
            context.DimensionContext.LetterSpace = setting.LetterSpace;
            context.DimensionContext.Proportional = setting.Proportional;
            context.DecorationContext.CurrentTopColor = setting.TopColor;
            context.DecorationContext.CurrentBottomColor = setting.BottomColor;
            context.DecorationContext.DecorationAvailable = decorationAvailable;
            context.DecorationContext.CurrentSelectEntryIndex = 0;
            if (decorationAvailable)
            {
                // 装飾コンテキストの初期化が必要なら装飾コンテキストも初期化する
                context.DecorationContext.DecorationEntry = decorationEntry;
                context.DecorationContext.DecorationStartIndex = decorationEntry.MainTextStartIndex;
                context.DecorationContext.DecorationEndIndex = decorationEntry.MainTextStartIndex + decorationEntry.MainTextCharacterCount;
            }


            // 整列方法によって構築関数を変える
            switch (setting.TextAlignment)
            {
                // 左寄せなら左整列頂点の構築をする
                case TextAlignment.Left:
                    BuildLeftAlignmentTextVertices(ref context);
                    break;


                // 右寄せなら右整列頂点の構築をする
                case TextAlignment.Right:
                    BuildRightAlignmentTextVertices(ref context);
                    break;


                // 中央寄せなら中央整列頂点の構築をする
                case TextAlignment.Center:
                    BuildCenterAlignmentTextVertices(ref context);
                    break;
            }


            // メインテキストとルビテキストの文字数を覚える
            mainCharacterCount = gameText.MainTextCharacters.Count;
            rubyCharacterCount = gameText.RubyTextCharacters.Count;


            // 境界矩形を計算する
            CalcuBoundingRect();
        }
        #endregion


        #region 整列方法毎の頂点計算関数群
        /// <summary>
        /// 左寄せ整列のテキストメッシュを構築します
        /// </summary>
        /// <param name="context">メッシュ構築コンテキストへの参照</param>
        private void BuildLeftAlignmentTextVertices(ref BuilderContext context)
        {
            // メインテキストの構築インデックス位置がメインテキストの長さに到達するまでループ
            while (context.TypographyContext.CurrentMainCharacterIndex < context.GameText.MainTextCharacters.Count)
            {
                // もし改行コードなら
                if (context.GameText.MainTextCharacters[context.TypographyContext.CurrentMainCharacterIndex] == '\n')
                {
                    // 次の文字へ
                    context.TypographyContext.CurrentMainCharacterIndex += 1;
                    context.DimensionContext.Offset.y -= context.DimensionContext.LineHeight;
                    context.DimensionContext.Offset.x = 0.0f;
                    continue;
                }


                // もし装飾対象の開始位置と同じなら
                var decorationAvailable = context.DecorationContext.DecorationAvailable;
                var decorationStartIndex = context.DecorationContext.DecorationStartIndex;
                if (decorationAvailable && context.TypographyContext.CurrentMainCharacterIndex == decorationStartIndex)
                {
                    // 装飾文字を描く
                    BuildDecorationCharacters(ref context);
                    continue;
                }


                // 特別な操作が無いならメイン文字を描く
                BuildMainCharacter(ref context);
            }
        }


        /// <summary>
        /// 右寄せ整列のテキストメッシュを構築します
        /// </summary>
        /// <param name="context">メッシュ構築コンテキストへの参照</param>
        private void BuildRightAlignmentTextVertices(ref BuilderContext context)
        {
        }


        /// <summary>
        /// 中央寄せ整列のテキストメッシュを構築します
        /// </summary>
        /// <param name="context">メッシュ構築コンテキストへの参照</param>
        private void BuildCenterAlignmentTextVertices(ref BuilderContext context)
        {
        }
        #endregion


        #region 頂点構築関数群
        /// <summary>
        /// 現在の状態で一文字だけメインテキストの頂点を構築します
        /// </summary>
        /// <param name="context">現在処理中のビルダーコンテキスト</param>
        private void BuildMainCharacter(ref BuilderContext context)
        {
            // 構築するべき文字情報を取得する
            var character = context.GameText.MainTextCharacters[context.TypographyContext.CurrentMainCharacterIndex];
            var info = default(CharacterInfo);
            if (!context.FontData.GetMainCharacterInfo(character, out info))
            {
                // 取得に失敗したら何事もなく終了
                return;
            }


            // オフセットとスケールを取得する
            var offset = context.DimensionContext.Offset;
            var scale = context.DimensionContext.MainTextScale;


            // オフセット込みのX,Yの最小と最大を求める
            var xMin = offset.x + info.minX * scale;
            var xMax = offset.x + info.maxX * scale;
            var yMin = offset.y + info.minY * scale;
            var yMax = offset.y + info.maxY * scale;


            // 現在のフォントカラーを取り出す
            var topColor = context.DecorationContext.CurrentTopColor;
            var bottomColor = context.DecorationContext.CurrentBottomColor;


            // 頂点を書き込む位置を求めて頂点データを書き込む
            var buffer = context.VertexBufferContext.Vertices;
            var index = context.TypographyContext.CurrentMainCharacterIndex * 4;
            buffer[index + 0] = new UIVertex() { position = new Vector3(xMin, yMin, 0.0f), color = bottomColor, uv0 = info.uvBottomLeft };
            buffer[index + 1] = new UIVertex() { position = new Vector3(xMin, yMax, 0.0f), color = topColor, uv0 = info.uvTopLeft };
            buffer[index + 2] = new UIVertex() { position = new Vector3(xMax, yMax, 0.0f), color = topColor, uv0 = info.uvTopRight };
            buffer[index + 3] = new UIVertex() { position = new Vector3(xMax, yMin, 0.0f), color = bottomColor, uv0 = info.uvBottomRight };


            // オフセットと文字読み取りインデックスの更新
            var advance = context.DimensionContext.Proportional ? info.glyphWidth : info.advance;
            context.DimensionContext.Offset.x += advance * context.DimensionContext.MainTextScale * context.DimensionContext.LetterSpace;
            context.TypographyContext.CurrentMainCharacterIndex += 1;
        }


        /// <summary>
        /// 現在の状態で一文字だけルビテキストの頂点を構築します
        /// </summary>
        /// <param name="context">現在処理中のビルダーコンテキスト</param>
        private void BuildRubyCharacter(ref BuilderContext context)
        {
            // 構築するべき文字情報を取得する
            var character = context.GameText.RubyTextCharacters[context.TypographyContext.CurrentRubyCharacterIndex];
            var info = default(CharacterInfo);
            if (!context.FontData.GetRubyCharacterInfo(character, out info))
            {
                // 取得に失敗したら何事もなく終了
                return;
            }


            // オフセットとスケールを取得する
            var offset = context.DimensionContext.Offset;
            var scale = context.DimensionContext.RubyTextScale;


            // オフセット込みのX,Yの最小と最大を求める
            var xMin = offset.x + info.minX * scale;
            var xMax = offset.x + info.maxX * scale;
            var yMin = offset.y + context.DimensionContext.MainLineHeight + info.minY * scale;
            var yMax = offset.y + context.DimensionContext.MainLineHeight + info.maxY * scale;


            // 現在のフォントカラーを取り出す
            var topColor = context.DecorationContext.CurrentTopColor;
            var bottomColor = context.DecorationContext.CurrentBottomColor;


            // 頂点を書き込む位置を求めて頂点データを書き込む
            var buffer = context.VertexBufferContext.Vertices;
            var index = (context.GameText.MainTextCharacters.Count + context.TypographyContext.CurrentRubyCharacterIndex) * 4;
            buffer[index + 0] = new UIVertex() { position = new Vector3(xMin, yMin, 0.0f), color = bottomColor, uv0 = info.uvBottomLeft };
            buffer[index + 1] = new UIVertex() { position = new Vector3(xMin, yMax, 0.0f), color = topColor, uv0 = info.uvTopLeft };
            buffer[index + 2] = new UIVertex() { position = new Vector3(xMax, yMax, 0.0f), color = topColor, uv0 = info.uvTopRight };
            buffer[index + 3] = new UIVertex() { position = new Vector3(xMax, yMin, 0.0f), color = bottomColor, uv0 = info.uvBottomRight };


            // オフセットと文字読み取りインデックスの更新
            var advance = context.DimensionContext.Proportional ? info.glyphWidth : info.advance;
            context.DimensionContext.Offset.x += advance * context.DimensionContext.RubyTextScale * context.DimensionContext.LetterSpace;
            context.TypographyContext.CurrentRubyCharacterIndex += 1;
        }


        /// <summary>
        /// 現在の状態と指定された幅でメインテキストの頂点を構築します
        /// </summary>
        /// <param name="context">現在処理中のビルダーコンテキスト</param>
        /// <param name="textWidth">構築するための用意されたテキスト幅</param>
        private void BuildFixedWidthMainText(ref BuilderContext context, float textWidth)
        {
            // オフセットとスケールを取得する
            var offset = context.DimensionContext.Offset;
            var scale = context.DimensionContext.MainTextScale;


            // 現在のフォントカラーを取り出す
            var topColor = context.DecorationContext.CurrentTopColor;
            var bottomColor = context.DecorationContext.CurrentBottomColor;


            // 一文字分の幅を求める
            var characterCount = context.DecorationContext.DecorationEntry.MainTextCharacterCount;
            var characterWidth = textWidth / characterCount;


            // 処理する文字数分回る
            for (int i = 0; i < characterCount; ++i)
            {
                // 構築するべき文字情報を取得する
                var character = context.GameText.MainTextCharacters[context.TypographyContext.CurrentMainCharacterIndex];
                var info = default(CharacterInfo);
                if (!context.FontData.GetMainCharacterInfo(character, out info))
                {
                    // 取得に失敗したら次へ
                    continue;
                }


                // オフセット込みのX,Yの最小と最大を求める
                var xMin = offset.x + (characterWidth / 2.0f - (info.maxX - info.minX) * scale / 2.0f) + info.minX * scale;
                var xMax = offset.x + (characterWidth / 2.0f - (info.maxX - info.minX) * scale / 2.0f) + info.maxX * scale;
                var yMin = offset.y + info.minY * scale;
                var yMax = offset.y + info.maxY * scale;


                // 頂点を書き込む位置を求めて頂点データを書き込む
                var buffer = context.VertexBufferContext.Vertices;
                var index = context.TypographyContext.CurrentMainCharacterIndex * 4;
                buffer[index + 0] = new UIVertex() { position = new Vector3(xMin, yMin, 0.0f), color = bottomColor, uv0 = info.uvBottomLeft };
                buffer[index + 1] = new UIVertex() { position = new Vector3(xMin, yMax, 0.0f), color = topColor, uv0 = info.uvTopLeft };
                buffer[index + 2] = new UIVertex() { position = new Vector3(xMax, yMax, 0.0f), color = topColor, uv0 = info.uvTopRight };
                buffer[index + 3] = new UIVertex() { position = new Vector3(xMax, yMin, 0.0f), color = bottomColor, uv0 = info.uvBottomRight };


                // オフセットと文字の読み取りインデックスを進める
                offset.x += characterWidth;
                context.TypographyContext.CurrentMainCharacterIndex += 1;
            }


            // オフセットの更新
            context.DimensionContext.Offset.x += textWidth;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="context">現在処理中のビルダーコンテキスト</param>
        /// <param name="textWidth">構築するための用意されたテキスト幅</param>
        private void BuildFixedWidthRubyText(ref BuilderContext context, float textWidth)
        {
            // オフセットとスケールを取得する
            var offset = context.DimensionContext.Offset;
            var scale = context.DimensionContext.RubyTextScale;


            // 現在のフォントカラーを取り出す
            var topColor = context.DecorationContext.CurrentTopColor;
            var bottomColor = context.DecorationContext.CurrentBottomColor;


            // 一文字分の幅を求める
            var characterCount = context.DecorationContext.DecorationEntry.RubyTextCharacterCount;
            var characterWidth = textWidth / characterCount;


            // 処理する文字数分回る
            for (int i = 0; i < characterCount; ++i)
            {
                // 構築するべき文字情報を取得する
                var character = context.GameText.RubyTextCharacters[context.TypographyContext.CurrentRubyCharacterIndex];
                var info = default(CharacterInfo);
                if (!context.FontData.GetRubyCharacterInfo(character, out info))
                {
                    // 取得に失敗したら次へ
                    continue;
                }


                // オフセット込みのX,Yの最小と最大を求める
                var xMin = offset.x + (characterWidth / 2.0f - (info.maxX - info.minX) * scale / 2.0f) + info.minX * scale;
                var xMax = offset.x + (characterWidth / 2.0f - (info.maxX - info.minX) * scale / 2.0f) + info.maxX * scale;
                var yMin = offset.y + context.DimensionContext.MainLineHeight + info.minY * scale;
                var yMax = offset.y + context.DimensionContext.MainLineHeight + info.maxY * scale;


                // 頂点を書き込む位置を求めて頂点データを書き込む
                var buffer = context.VertexBufferContext.Vertices;
                var index = (context.GameText.MainTextCharacters.Count + context.TypographyContext.CurrentRubyCharacterIndex) * 4;
                buffer[index + 0] = new UIVertex() { position = new Vector3(xMin, yMin, 0.0f), color = bottomColor, uv0 = info.uvBottomLeft };
                buffer[index + 1] = new UIVertex() { position = new Vector3(xMin, yMax, 0.0f), color = topColor, uv0 = info.uvTopLeft };
                buffer[index + 2] = new UIVertex() { position = new Vector3(xMax, yMax, 0.0f), color = topColor, uv0 = info.uvTopRight };
                buffer[index + 3] = new UIVertex() { position = new Vector3(xMax, yMin, 0.0f), color = bottomColor, uv0 = info.uvBottomRight };


                // オフセットと文字の読み取りインデックスを進める
                offset.x += characterWidth;
                context.TypographyContext.CurrentRubyCharacterIndex += 1;
            }


            // オフセットの更新
            context.DimensionContext.Offset.x += textWidth;
        }


        /// <summary>
        /// 装飾付き本文の頂点を構築します
        /// </summary>
        /// <param name="context">現在処理中のビルダーコンテキスト</param>
        private void BuildDecorationCharacters(ref BuilderContext context)
        {
            // もし色変更が有効なら
            if (context.DecorationContext.DecorationEntry.UseColor)
            {
                // 現在の色を変更する
                context.DecorationContext.CurrentTopColor = context.DecorationContext.DecorationEntry.TopTextColor.ToColor();
                context.DecorationContext.CurrentBottomColor = context.DecorationContext.DecorationEntry.BottomTextColor.ToColor();
            }


            // 装飾情報を取得する
            var mainTextStartIndex = context.DecorationContext.DecorationEntry.MainTextStartIndex;
            var mainCharacterCount = context.DecorationContext.DecorationEntry.MainTextCharacterCount;
            var mainFontSize = context.FontData.MainFontSize;
            var mainFontStyle = context.FontData.MainFontStyle;
            var rubyTextStartIndex = context.DecorationContext.DecorationEntry.RubyTextStartIndex;
            var rubyCharacterCount = context.DecorationContext.DecorationEntry.RubyTextCharacterCount;
            var rubyFontSize = context.FontData.RubyFontSize;
            var rubyFontStyle = context.FontData.RubyFontStyle;


            // もしルビの存在しない装飾なら
            if (rubyTextStartIndex == -1 && rubyCharacterCount == 0)
            {
                // 装飾本文文字数分ループ
                var mainTextCharacterCount = context.DecorationContext.DecorationEntry.MainTextCharacterCount;
                for (int i = 0; i < mainTextCharacterCount; ++i)
                {
                    // メインテキスト文字の描画
                    BuildMainCharacter(ref context);
                }


                // 現在の色を戻して終了
                context.DecorationContext.CurrentTopColor = context.BuildSetting.TopColor;
                context.DecorationContext.CurrentBottomColor = context.BuildSetting.BottomColor;
                return;
            }


            // オフセットとスケールを取得する
            var offset = context.DimensionContext.Offset;
            var mainScale = context.DimensionContext.MainTextScale;
            var rubyScale = context.DimensionContext.RubyTextScale;


            // 本文とルビの構築される想定横幅を計算する
            var mainTextWidth = CalcuRenderTextWidth(context.GameText.MainTextCharacters, mainTextStartIndex, mainCharacterCount, context.FontData.Font, mainFontSize, mainFontStyle, mainScale);
            var rubyTextWidth = CalcuRenderTextWidth(context.GameText.RubyTextCharacters, rubyTextStartIndex, rubyCharacterCount, context.FontData.Font, rubyFontSize, rubyFontStyle, rubyScale);


            // もし本文側が広いなら
            if (rubyTextWidth <= mainTextWidth)
            {
                // 本文文字数分ループ
                var mainTextCharacterCount = context.DecorationContext.DecorationEntry.MainTextCharacterCount;
                for (int i = 0; i < mainTextCharacterCount; ++i)
                {
                    // メインテキスト文字の描画
                    BuildMainCharacter(ref context);
                }


                // オフセットを戻す
                context.DimensionContext.Offset = offset;


                // 固定幅ルビテキストの構築をする
                BuildFixedWidthRubyText(ref context, mainTextWidth);
            }
            else
            {
                // ルビ側が広いならルビ文字数分ループ
                var rubyTextCharacterCount = context.DecorationContext.DecorationEntry.RubyTextCharacterCount;
                for (int i = 0; i < rubyTextCharacterCount; ++i)
                {
                    // ルビの描画をする
                    BuildRubyCharacter(ref context);
                }


                // オフセットを戻す
                context.DimensionContext.Offset = offset;


                // 固定幅本文テキストの構築をする
                BuildFixedWidthMainText(ref context, rubyTextWidth);
            }


            // 現在の色を戻す
            context.DecorationContext.CurrentTopColor = context.BuildSetting.TopColor;
            context.DecorationContext.CurrentBottomColor = context.BuildSetting.BottomColor;


            // もしこの装飾が最後なら
            if (context.GameText.DecorationEntryCount == context.DecorationContext.CurrentSelectEntryIndex + 1)
            {
                // 装飾はもうないとしてマークして終了
                context.DecorationContext.DecorationAvailable = false;
                return;
            }


            // 次の装飾を選択して終了
            context.GameText.TryGetDecorationEntry(++context.DecorationContext.CurrentSelectEntryIndex, out context.DecorationContext.DecorationEntry);
            context.DecorationContext.DecorationStartIndex = context.DecorationContext.DecorationEntry.MainTextStartIndex;
            context.DecorationContext.DecorationEndIndex = context.DecorationContext.DecorationEntry.MainTextStartIndex + context.DecorationContext.DecorationEntry.MainTextCharacterCount;
        }
        #endregion


        #region 頂点計算関数群
        /// <summary>
        /// 指定された文字情報を元に描画されるテキストの横幅を計算します
        /// </summary>
        /// <param name="charaList">描画する文字のリスト</param>
        /// <param name="index">文字のリストを読み取る開始インデックス</param>
        /// <param name="count">文字の読み取る数</param>
        /// <param name="font">描画に利用するフォント</param>
        /// <param name="size">フォントの利用するサイズ</param>
        /// <param name="style">フォントの利用するスタイル</param>
        /// <param name="scale">フォントから取り出された文字に対して掛けるスケール</param>
        /// <returns>計算された描画する横幅を返します</returns>
        private float CalcuRenderTextWidth(ReadOnlyCollection<char> charaList, int index, int count, Font font, int size, FontStyle style, float scale)
        {
            // 結果格納変数と末尾インデックスを宣言
            float result = 0.0f;
            int endIndex = index + count;


            // 指定された範囲をループ
            for (int i = index; i < endIndex; ++i)
            {
                // 文字情報を取得出来たのなら
                CharacterInfo info;
                if (font.GetCharacterInfo(charaList[i], out info, size, style))
                {
                    // 描画する横幅を加算する
                    result += info.advance * scale;
                }
            }


            // 計算結果を返す
            return result;
        }


        /// <summary>
        /// 現在キャッシュされている頂点バッファから、描画するべき文字数分の描画をした際の境界矩形を計算します
        /// </summary>
        private void CalcuBoundingRect()
        {
            // トータル頂点数と境界矩形の宣言
            var totalVertexCount = (mainCharacterCount + rubyCharacterCount) * 4;
            var boundingRect = Rect.zero;


            // 頂点数分回る
            for (int i = 0; i < totalVertexCount; ++i)
            {
                // 頂点座標を取得して境界を調整する
                var position = verticesCache[i].position;
                boundingRect.xMin = Mathf.Min(boundingRect.xMin, position.x);
                boundingRect.xMax = Mathf.Max(boundingRect.xMax, position.x);
                boundingRect.yMin = Mathf.Min(boundingRect.yMin, position.y);
                boundingRect.yMax = Mathf.Max(boundingRect.yMax, position.y);
            }


            // 結果を覚える
            BoundingRect = boundingRect;
        }
        #endregion


        #region リスト制御関数群
        /// <summary>
        /// 指定されたゲームテキストの描画が出来る、十分な頂点キャッシュを確保します
        /// </summary>
        /// <param name="gameText">描画をするゲームテキスト</param>
        private void EnsureVerticesCache(GameText gameText)
        {
            // 本文とルビの合計文字数を求める
            var totalCharacterCount = gameText.MainTextCharacters.Count + gameText.RubyTextCharacters.Count;


            // もし合計文字数分収まるキャッシュがあるなら
            if (verticesCache.Length >= totalCharacterCount * 4)
            {
                // そのまま終了
                return;
            }


            // 必要なキャッシュを生成して新しいキャッシュにすべてコピーする
            var newerCache = new UIVertex[totalCharacterCount * 4];
            for (int i = 0; i < verticesCache.Length; ++i)
            {
                // コピーする
                newerCache[i] = verticesCache[i];
            }


            // 参照の更新をする
            verticesCache = newerCache;
        }


        /// <summary>
        /// 現在の頂点キャッシュの全てをクリアします
        /// </summary>
        private void ClearVertices()
        {
            // 全キャッシュ回る
            for (int i = 0; i < verticesCache.Length; ++i)
            {
                // 単純に既定値で代入
                verticesCache[i] = default;
            }
        }


        /// <summary>
        /// 指定されたアップロードハンドラにメインテキスト分の頂点データをアップロードします
        /// </summary>
        /// <param name="uploadHandler">一文字ごとに呼び出されるアップロードハンドラ（引数には必ず4頂点分のバッファを送ります）</param>
        /// <exception cref="ArgumentNullException">uploadHandler が null です</exception>
        public void UploadMainTextVertices(Action<UIVertex[]> uploadHandler)
        {
            // null を 渡されたら
            if (uploadHandler == null)
            {
                // どこにアップロードすればよいのじゃ
                throw new ArgumentNullException(nameof(uploadHandler));
            }


            // メインテキストの長さ分ループ
            for (int i = 0; i < mainCharacterCount; ++i)
            {
                // 転送用バッファにコピーしてアップロードハンドラに渡す
                translateBuffer[0] = verticesCache[i * 4 + 0];
                translateBuffer[1] = verticesCache[i * 4 + 1];
                translateBuffer[2] = verticesCache[i * 4 + 2];
                translateBuffer[3] = verticesCache[i * 4 + 3];
                textAnimator?.AnimateMainCharaVertex(translateBuffer, i);
                uploadHandler(translateBuffer);
            }
        }


        /// <summary>
        /// 指定されたアップロードハンドラにルビテキスト分の頂点データをアップロードします
        /// </summary>
        /// <param name="uploadHandler">一文字ごとに呼び出されるアップロードハンドラ（引数には必ず4頂点分のバッファを送ります）</param>
        /// <exception cref="ArgumentNullException">uploadHandler が null です</exception>
        public void UploadRubyTextVertices(Action<UIVertex[]> uploadHandler)
        {
            // null を 渡されたら
            if (uploadHandler == null)
            {
                // どこにアップロードすればよいのじゃ
                throw new ArgumentNullException(nameof(uploadHandler));
            }


            // ルビテキストの長さ分ループ
            for (int i = 0; i < rubyCharacterCount; ++i)
            {
                // オフセットインデックスを求める
                var index = mainCharacterCount + i;


                // 転送用バッファにコピーしてアップロードハンドラに渡す
                translateBuffer[0] = verticesCache[index * 4 + 0];
                translateBuffer[1] = verticesCache[index * 4 + 1];
                translateBuffer[2] = verticesCache[index * 4 + 2];
                translateBuffer[3] = verticesCache[index * 4 + 3];
                textAnimator?.AnimateRubyCharaVertex(translateBuffer, i);
                uploadHandler(translateBuffer);
            }
        }
        #endregion



        #region 内部の型定義
        /// <summary>
        /// 頂点構築器のコンテキストを持った構造体です
        /// </summary>
        private struct BuilderContext
        {
            /// <summary>
            /// 描画するゲームテキスト
            /// </summary>
            public GameText GameText;


            /// <summary>
            /// 描画に用いるフォントデータ
            /// </summary>
            public RubyableTextFontData FontData;


            /// <summary>
            /// 頂点構築の設定値
            /// </summary>
            public BuildSetting BuildSetting;


            /// <summary>
            /// 頂点バッファ
            /// </summary>
            public VertexBufferContext VertexBufferContext;


            /// <summary>
            /// 文字列の描画状態コンテキスト
            /// </summary>
            public TypographyContext TypographyContext;


            /// <summary>
            /// 頂点計算用
            /// </summary>
            public DimensionContext DimensionContext;


            /// <summary>
            /// 装飾制御のコンテキスト
            /// </summary>
            public DecorationContext DecorationContext;
        }



        /// <summary>
        /// 頂点構築対象となる頂点バッファを持つコンテキスト構造体です
        /// </summary>
        private struct VertexBufferContext
        {
            /// <summary>
            /// UI頂点バッファへの参照
            /// </summary>
            public UIVertex[] Vertices;


            /// <summary>
            /// ルビテキスト用UI頂点バッファの書き込み開始オフセット
            /// </summary>
            public int RubyVerticesOffset;
        }



        /// <summary>
        /// テキストとして印字する状態を持つコンテキスト構造体です
        /// </summary>
        private struct TypographyContext
        {
            /// <summary>
            /// 描画するべきメイン文字のインデックス
            /// </summary>
            public int CurrentMainCharacterIndex;


            /// <summary>
            /// 描画するべきルビ文字のインデックス
            /// </summary>
            public int CurrentRubyCharacterIndex;
        }



        /// <summary>
        /// 頂点構築時に使用する各種座標調整用の値を持つコンテキスト構造体です
        /// </summary>
        private struct DimensionContext
        {
            /// <summary>
            /// 頂点を構築する時のオフセット
            /// </summary>
            public Vector2 Offset;


            /// <summary>
            /// メインテキスト描画時に用いるスケール
            /// </summary>
            public float MainTextScale;


            /// <summary>
            /// ルビテキスト描画時に用いるスケール
            /// </summary>
            public float RubyTextScale;


            /// <summary>
            /// メインテキストの高さ
            /// </summary>
            public float MainLineHeight;


            /// <summary>
            /// ルビテキストの高さ
            /// </summary>
            public float RubyLineHeight;


            /// <summary>
            /// 行間の距離
            /// </summary>
            public float LineHeight;


            /// <summary>
            /// 文字間隔
            /// </summary>
            public float LetterSpace;


            /// <summary>
            /// 文字のアドバンス値を無視してグリフの幅を見るかどうか
            /// </summary>
            public bool Proportional;
        }



        /// <summary>
        /// 装飾制御の状態を持つコンテキスト構造体です
        /// </summary>
        private struct DecorationContext
        {
            /// <summary>
            /// 装飾が利用可能かどうか
            /// </summary>
            public bool DecorationAvailable;


            /// <summary>
            /// 装飾開始メインテキストのインデックス
            /// </summary>
            public int DecorationStartIndex;


            /// <summary>
            /// 装飾終了メインテキストのインデックス
            /// </summary>
            public int DecorationEndIndex;


            /// <summary>
            /// メインテキスト描画時に利用する上側頂点カラー
            /// </summary>
            public Color CurrentTopColor;


            /// <summary>
            /// メインテキスト描画時に利用する下側頂点カラー
            /// </summary>
            public Color CurrentBottomColor;


            /// <summary>
            /// 現在の選択中装飾エントリインデックス
            /// </summary>
            public int CurrentSelectEntryIndex;


            /// <summary>
            /// 装飾エントリが有効時に利用する装飾情報
            /// </summary>
            public GameText.DecorationEntry DecorationEntry;
        }



        /// <summary>
        /// 頂点データの構築設定を持った構造体です
        /// </summary>
        public struct BuildSetting
        {
            /// <summary>
            /// テキストの整列方法
            /// </summary>
            public TextAlignment TextAlignment;


            /// <summary>
            /// 行間の値（1.0が標準）
            /// </summary>
            public float LineSpace;


            /// <summary>
            /// 字送りの値（1.0が標準）
            /// </summary>
            public float LetterSpace;


            /// <summary>
            /// 文字のアドバンス値を無視してグリフの幅を見るかどうか
            /// </summary>
            public bool Proportional;


            /// <summary>
            /// メインテキストの描画テキストサイズ
            /// </summary>
            public int MainTextSize;


            /// <summary>
            /// ルビテキストの描画テキストサイズ
            /// </summary>
            public int RubyTextSize;


            /// <summary>
            /// メインテキストの描画時に利用する上側テキストカラー
            /// </summary>
            public Color TopColor;


            /// <summary>
            /// メインテキストの描画時に利用する下側テキストカラー
            /// </summary>
            public Color BottomColor;
        }
        #endregion
    }
    #endregion



    #region 拡張関数実装クラス
    /// <summary>
    /// 変換拡張関数実装用クラスです
    /// </summary>
    internal static class ConvertExtensions
    {
        /// <summary>
        /// 符号なし32bit整数からカラーへ変換します（RGBA32bit）
        /// </summary>
        /// <param name="value">RGBA32形式の符号なし32bit整数</param>
        /// <returns>変換されたカラーオブジェクトを返します</returns>
        public static Color ToColor(this uint value)
        {
            // ビット演算を活用してRGBA変換して返す
            return new Color32((byte)((value >> 24) & 0xFF), (byte)((value >> 16) & 0xFF), (byte)((value >> 8) & 0xFF), (byte)(value & 0xFF));
        }
    }
    #endregion
}