## プロジェクト概要

IceMilkTea は Unity 6 (6000.3.x) 向けのカーネルフレームワーク。ゲーム基板を構築するための基盤ライブラリで、Service Locator パターンと PlayerLoopSystem によるカスタムアップデートを提供する。UPM パッケージ (`jp.foxtamp.icemilktea`) として配布される。

## 技術スタック

- **Unity**: 6000.3.8f1
- **言語**: C# 9.0 (厳密準拠)
- **外部依存**: なし（`UnityEngine.*`, `Unity.*`, `System.*` のみ使用可。UniTask, UniRx, VContainer 等は禁止）

## テスト

- **フレームワーク**: NUnit 3 (Unity Test Framework 1.6.0)
- **テスト場所**: `Packages/IceMilkTea/Tests/Editor/`
- **実行方法**: Unity Editor の Test Runner ウィンドウから実行（Editor テストのみ）
- **名前空間**: `Foxtamp.IceMilkTea.Tests`

## アーキテクチャ

パッケージのルートは `Packages/IceMilkTea/`

### アセンブリ構成
- **Runtime**: `Foxtamp.IceMilkTea` — `Packages/IceMilkTea/Runtime/`
- **Tests**: `Foxtamp.IceMilkTea.Tests.Editor` — `Packages/IceMilkTea/Tests/Editor/`

### コア設計

**サービス駆動アーキテクチャ**: ゲームロジックは `MonoBehaviour` を継承しない純粋な C# クラス（サービス）として定義する。サービスは `GameMain` が管理する中央レジストリに登録され、`IGameServiceProvider` 経由で取得される。

**PlayerLoop 注入**: サービスの定期実行は `MonoBehaviour.Update` を使わず、`PlayerLoopSystemBuilder` を通じて Unity の `PlayerLoopSystem` ツリーに直接注入する。`GameServiceUpdateTiming` enum で24種類のタイミングポイント（MainLoopHead, PreFixedUpdate, PostUpdate 等）を定義。

**主要クラスの関係**:
- `GameMain` — アプリケーションのエントリポイント。`GameMain.Current` でシングルトンアクセス。`IGameServiceProvider` を保持
- `GameService` — サービスの基底クラス。virtual な Startup/Shutdown を持つ
- `PlayerLoopSystemBuilder` — PlayerLoop ツリーの操作（注入・検索・可視化）を行うビルダー
- `GameMainEntryPointAttribute` — RuntimeInitializeOnLoadMethod 相当の自動起動用属性

## コーディング規約

- **名前空間**: ブロック形式 (`namespace X { }`) 必須。ファイルスコープ名前空間は禁止
- **Nullable**: 全ファイルで `#nullable enable` 有効（`csc.rsp` で設定済み）
- **レコード**: `record class` のみ可。`record struct` (C# 10) は禁止
- **非同期**: Unity 6 標準の `Awaitable` を使用。UniTask 禁止
- **XML ドキュメント**: 全 public/protected メンバーに必須。private メソッドは分岐処理を含むか5行以上の場合に必須
- **パフォーマンス**: PlayerLoop に注入されるメソッドでの GC Alloc は厳禁。データ保持は構造体優先
- **ファイルヘッダ**: Zlib ライセンスヘッダを全ソースファイルに付与
- **回答言語**: 日本語で回答する
