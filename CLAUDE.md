## プロジェクト概要

IceMilkTea は Unity 6 (6000.3.x) 向けのカーネルフレームワーク。ゲーム基板を構築するための基盤ライブラリで、Service Locator パターンと PlayerLoopSystem によるカスタムアップデートを提供する。UPM パッケージ (`jp.sinoa.icemilktea`) として配布される。

## 技術スタック

- **Unity**: 6000.3.8f1
- **言語**: C# 9.0 (厳密準拠)
- **外部依存**: なし（`UnityEngine.*`, `Unity.*`, `System.*` のみ使用可。UniTask, UniRx, VContainer 等は禁止）
- **unsafe コード**: 許可（asmdef で `allowUnsafeCode: true`）

## テスト

- **フレームワーク**: NUnit 3 (Unity Test Framework)
- **テスト場所**: `Packages/IceMilkTea/Tests/Editor/`（現在テストファイルは未作成）
- **実行方法**: Unity Editor の Test Runner ウィンドウから実行（Editor テストのみ）
- **InternalsVisibleTo**: DEBUG ビルド時に `IceMilkTeaEditor`, `IceMilkTeaTestDynamic`, `IceMilkTeaTestStatic` へ内部公開

## アーキテクチャ

パッケージのルートは `Packages/IceMilkTea/`

### ディレクトリ構成
```
Packages/IceMilkTea/
├── Runtime/
│   ├── Core/           # ユーティリティ・汎用機能
│   ├── Kernel/         # ゲームフレームワーク中核
│   ├── AssemblyInfo.cs
│   └── Unity.IceMilkTea.asmdef
├── package.json
└── LICENSE.md
```

### アセンブリ構成
- **Runtime**: `IceMilkTea` — `Packages/IceMilkTea/Runtime/`
- **名前空間**: `IceMilkTea.Core`（全ソースファイル共通）

### コア設計

**サービス駆動アーキテクチャ**: ゲームロジックは `MonoBehaviour` を継承しない純粋な C# クラス（サービス）として定義する。サービスは `GameMain` が保持する `GameServiceManager` に登録され、`GameServiceManager` 経由で取得・制御される。

**PlayerLoop 注入**: サービスの定期実行は `MonoBehaviour.Update` を使わず、`ImtPlayerLoopSystem` を通じて Unity の `PlayerLoopSystem` ツリーに直接注入する。`GameServiceUpdateTiming` enum（`[Flags] UInt32`）で24種類のタイミングポイントを定義。`GameServiceManager.Startup()` は登録済みサービスが実際に使用するタイミングのみを PlayerLoop に注入する（未使用タイミングは注入しない）。

**主要クラスの関係**:
- `GameMain` — 純粋な抽象 C# クラス。アプリケーションのエントリポイント。利用者が `[GameMain]` 属性を付与した静的メソッドから `new MyGameMain().Run()` を呼び出して明示的に起動する。`GameMain.Current` でシングルトンアクセス。`ServiceManager` プロパティで `GameServiceManager` を保持。`Config` プロパティで `IGameConfig` を保持（常に非 null、NullObject パターン）。起動順序: `Run()` → `CreateConfig()` → `new GameServiceManager()` → `RegisterHandler()` → `Startup()`（サービス登録）→ `ServiceManager.Startup()`（PlayerLoop 注入）。virtual フック: `CreateConfig()`, `Startup()`, `Shutdown()`, `Update()`
- `IGameConfig` — ゲームコンフィグのマーカーインターフェイス（空）。アプリケーション固有の設定はアプリ側でこのインターフェイスを実装して定義する
- `NullGameConfig` — `IGameConfig` の NullObject 実装（`internal sealed`）。`CreateConfig()` 未オーバーライド時のデフォルト値
- `GameService` — サービスの抽象基底クラス。`Startup(out GameServiceStartupInfo info)` で更新関数テーブルを登録、`Shutdown()` で終了処理
- `GameServiceManager` — サービスのライフサイクル管理。サービスは `GameMain.Startup()` 内で `AddService()` / `TryAddService()` により登録する必要がある（`ServiceManager.Startup()` 後に追加されたサービスの更新タイミングは PlayerLoop に反映されない）。API: `AddService()`, `TryAddService()`, `GetService<T>()`, `TryGetService<T>()`, `RemoveService<T>()`, `RemoveAllServices()`, `Exists<T>()`, `SetActiveService<T>()`, `IsActiveService<T>()`, `ServiceForEach()`
- `GameServiceStartupInfo` — サービス起動時に `UpdateFunctionTable`（`Dictionary<GameServiceUpdateTiming, Action>`）を設定する構造体
- `ImtPlayerLoopSystem` — `PlayerLoopSystem` 構造体をクラスとしてラップ。`Insert<T>()`, `Remove<T>()`, `Find<T>()`, `IndexOf<T>()`, `BuildAndSetUnityPlayerLoop()` で PlayerLoop ツリーを操作。`PlayerLoopSystem` との相互明示キャスト対応
- `PlayerLoopUpdater` — PlayerLoop で動作するアップデータの抽象基底クラス
- `MonoBehaviourEventBridge` — MonoBehaviour ライフサイクルイベント（Focus, Pause, EndOfFrame）をコールバックへ中継。いずれかのタイミングを使用するサービスが存在する場合のみ生成される
- `ImtGameServiceReferenceCache<T>` — サービス参照の遅延キャッシュ構造体
- `InsertTiming` — `BeforeInsert`, `AfterInsert` を持つ enum

**ユーティリティ (Core/)**:
- `ImtStateMachine<TContext, TEvent>` — ジェネリックステートマシン
- `ImtAwaiter` — カスタム awaiter（`INotifyCompletion` 実装）
- `ObjectPool<T>` — オブジェクトプール
- `Crc` — CRC チェックサム計算
- `DataFetcher` — データ取得ユーティリティ
- `EasingFunction` — イージング関数ライブラリ
- `RetryableWorker` — リトライロジック
- `WebDownloader` — Web コンテンツダウンロード
- `Progress<T>` — 進捗追跡
- `TaskStateMachine` — タスクベースステートマシン

**例外クラス**:
- `ImtException` — 基底例外クラス
- `GameServiceAlreadyExistsException` — サービス重複登録時
- `GameServiceNotFoundException` — サービス未発見時

**その他**:
- `GameServiceUpdate` — PlayerLoop タイミング用の16個のネストマーカー構造体を持つ
- `GameServiceManagerStartup` — `[Obsolete]`。サービス起動は `GameServiceManager.Startup()` 内で同期的に行われるため不使用
- `GameServiceManagerCleanup` — `RemoveService` によるサービス破棄を処理するため常に PlayerLoop に注入される
- `GameShutdownAnswer` — `Approve`, `Reject`
- `GameMainAttribute` — `RuntimeInitializeOnLoadMethodAttribute` を継承した属性。エントリポイントメソッドに付与すると `BeforeSceneLoad` タイミングで自動呼び出しされる
- `ImtUnityUtility` — `CreatePersistentGameObject()` 等の静的ユーティリティ

## コーディング規約

- **名前空間**: ブロック形式 (`namespace X { }`) 必須。ファイルスコープ名前空間は禁止
- **Nullable**: 現在未有効（`csc.rsp` なし、`#nullable enable` 未使用）
- **レコード**: `record class` のみ可。`record struct` (C# 10) は禁止
- **非同期**: Unity 6 標準の `Awaitable` を使用。UniTask 禁止
- **XML ドキュメント**: 全 public/protected メンバーに必須。private メソッドは分岐処理を含むか5行以上の場合に必須
- **パフォーマンス**: PlayerLoop に注入されるメソッドでの GC Alloc は厳禁。データ保持は構造体優先
- **ファイルヘッダ**: Zlib ライセンスヘッダを全ソースファイルに付与
- **回答言語**: 日本語で回答する
