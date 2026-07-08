## プロジェクト概要

IceMilkTea は Unity 6 (6000.3.x) 向けのカーネルフレームワーク。ゲーム基板を構築するための基盤ライブラリで、Service Locator パターンと PlayerLoopSystem によるカスタムアップデートを提供する。UPM パッケージ (`jp.sinoa.icemilktea`) として配布される。

## 技術スタック

- **Unity**: 6000.3.16f1（`ProjectSettings/ProjectVersion.txt`。package.json の最小対応バージョンは `6000.0`）
- **言語**: C# 9.0 (厳密準拠)
- **外部依存**: なし（`UnityEngine.*`, `Unity.*`, `System.*` のみ使用可。UniTask, UniRx, VContainer 等は禁止）
- **unsafe コード**: 許可（asmdef で `allowUnsafeCode: true`）
- **Enter Play Mode**: Domain Reload / Scene Reload を両方無効化（`ProjectSettings/EditorSettings.asset` の `m_EnterPlayModeOptions: 3`）。再生開始の高速化のため

## テスト

- **フレームワーク**: NUnit 3 (Unity Test Framework)
- **テスト場所**: `Packages/IceMilkTea/Tests/Editor/` を予定（現在 `Tests/` ディレクトリ・テスト用 asmdef ともに未作成）
- **実行方法**: Unity Editor の Test Runner ウィンドウから実行（Editor テストのみ）
- **InternalsVisibleTo**: DEBUG ビルド時に `IceMilkTeaEditor`, `IceMilkTeaTestDynamic`, `IceMilkTeaTestStatic` へ内部公開（3アセンブリとも現在は未作成の将来向け宣言）

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

パッケージ外の `Assets/Scripts/Sample/`（アセンブリ `IceMilkTea.Sample`）に起動フローの実例がある: `SampleEntryPoint.cs` が `[GameMain]` 属性付き静的メソッドから `new SampleGameMain().Run()` を呼び出し、`SampleGameMain` / `SampleService` が `GameMain` / `GameService` の最小実装例になっている。

### アセンブリ構成
- **Runtime**: `IceMilkTea` — `Packages/IceMilkTea/Runtime/`
- **名前空間**: `IceMilkTea.Core`（全ソースファイル共通）

### コア設計

**サービス駆動アーキテクチャ**: ゲームロジックは `MonoBehaviour` を継承しない純粋な C# クラス（サービス）として定義する。サービスは `GameMain` が保持する `GameServiceManager` に登録され、`GameServiceManager` 経由で取得・制御される。

**PlayerLoop 注入**: サービスの定期実行は `MonoBehaviour.Update` を使わず、`ImtPlayerLoopSystem` を通じて Unity の `PlayerLoopSystem` ツリーに直接注入する。`GameServiceUpdateTiming` enum（`[Flags] UInt32`）で24種類のタイミングポイントを定義。`GameServiceManager.Startup()` は登録済みサービスが実際に使用するタイミングのみを PlayerLoop に注入する（未使用タイミングは注入しない）。

**Domain Reload 無効対応**: 本プロジェクトは Enter Play Mode Options で Domain Reload / Scene Reload を無効化しているため、static フィールドは Play Mode をまたいで残存する。`GameMain`・`ImtPlayerLoopSystem`・`ImtAwaitableUpdateBehaviourScheduler` は `[RuntimeInitializeOnLoadMethod(SubsystemRegistration)]` を用いて Play Mode 開始時に静的状態をリセット（`GameMain.Current` クリア、`Awaiter` のスケジューラ初期化）またはイベント再購読（`Application.quitting`）する。`GameMain.InternalShutdown()` は Play Mode 終了時に `Current` を `try/finally` でクリアする（`GameMain` の SubsystemRegistration リセットはこれが失敗した場合に備えた `#if UNITY_EDITOR` 限定の保険。`ImtAwaitableUpdateBehaviourScheduler` のリセットも `#if UNITY_EDITOR` 限定）。新たに static フィールドを追加する場合は Play Mode 間の残存で問題が出ないか確認し、必要なら同様のリセット処理を設けること。

**主要クラスの関係**:
- `GameMain` — 純粋な抽象 C# クラス。アプリケーションのエントリポイント。利用者が `[GameMain]` 属性を付与した静的メソッドから `new MyGameMain().Run()` を呼び出して明示的に起動する。`GameMain.Current` でシングルトンアクセス。`ServiceManager` プロパティで `GameServiceManager` を保持。`Config` プロパティで `IGameConfig` を保持（`Run()` 以降は常に非 null、NullObject パターン）。起動順序: `Run()` → `CreateConfig()` → `new GameServiceManager()` → `RegisterHandler()` → `Startup()`（サービス登録）→ `ServiceManager.Startup()`（PlayerLoop 注入）。`Restart()` で `ServiceManager` のみ再起動可能（`ServiceManager.Shutdown()` → `OnRestart()` → `ServiceManager.Startup()` の順で実行）。virtual フック: `CreateConfig()`, `Startup()`, `OnRestart()`（既定では `Startup()` を再呼び出し）, `Shutdown()`, `Update()`
- `IGameConfig` — ゲームコンフィグのマーカーインターフェイス（空）。アプリケーション固有の設定はアプリ側でこのインターフェイスを実装して定義する
- `NullGameConfig` — `IGameConfig` の NullObject 実装（`internal sealed`）。`CreateConfig()` 未オーバーライド時のデフォルト値
- `GameService` — サービスの抽象基底クラス。`Startup(out GameServiceStartupInfo info)` で更新関数テーブルを登録、`Shutdown()` で終了処理
- `GameServiceManager` — サービスのライフサイクル管理。サービスは `GameMain.Startup()` 内で `AddService()` / `TryAddService()` により登録する必要がある（`ServiceManager.Startup()` 後に追加されたサービスの更新タイミングは PlayerLoop に反映されない）。API: `AddService()`, `TryAddService()`, `GetService<T>()`, `TryGetService<T>()`, `RemoveService<T>()`, `RemoveAllServices()`, `Exists<T>()`, `SetActiveService<T>()`, `IsActiveService<T>()`, `ServiceForEach()`, `ServiceProcessTime`（サービス処理時間の計測値プロパティ）
- `GameServiceStartupInfo` — サービス起動時に `UpdateFunctionTable`（`Dictionary<GameServiceUpdateTiming, Action>`）を設定する構造体
- `ImtPlayerLoopSystem` — `PlayerLoopSystem` 構造体をクラスとしてラップ。`Insert<T>()`, `Remove<T>()`, `Find<T>()`, `IndexOf<T>()`, `BuildAndSetUnityPlayerLoop()` で PlayerLoop ツリーを操作。`PlayerLoopSystem` との相互明示キャスト対応
- `PlayerLoopUpdater` — PlayerLoop で動作するアップデータの抽象基底クラス
- `MonoBehaviourEventBridge` — MonoBehaviour ライフサイクルイベント（Focus, Pause, EndOfFrame）をコールバックへ中継（`internal`、所在は `Runtime/Core/`）。いずれかのタイミングを使用するサービスが存在する場合のみ生成される
- `ImtGameServiceReferenceCache<T>` — サービス参照の遅延キャッシュ構造体
- `InsertTiming` — `BeforeInsert`, `AfterInsert` を持つ enum

**ユーティリティ (Core/)**:
- `ImtStateMachine<TContext, TEvent>` — ジェネリックステートマシン。イベント型を `int` に固定した派生 `ImtStateMachine<TContext>` もある
- `Awaiter.cs` — 独自 awaitable フレームワーク一式（約20 public 型）。`ImtAwaiter` / `ImtAwaiter<TResult>`（`INotifyCompletion` 実装のカスタム awaiter 構造体）、`IAwaitable` / `IAwaitable<TResult>`、`ImtAwaitable` 系待機クラス（`ImtAwaitableManualReset`, `ImtAwaitableAutoReset`, `ImtAwaitableFromEvent` 等）、`ImtTask` / `ImtTask<TResult>`、`ImtAwaitableUpdateBehaviourScheduler` など
- `ObjectPool<T>` — オブジェクトプール（抽象クラス）
- `CrcBase<T>` / `Crc32Base` / `Crc64Base` / `Crc32` / `Crc64Iso` / `Crc64Ecma` — CRC チェックサム計算（`Crc.cs`）
- `HashStream` / `Crc64Stream<TCrc>` / `MonitorableStream` — ハッシュ計算・監視機能付きストリームラッパー（`Stream.cs`）
- `IDataFetcher` / `HttpDataFetcher` / `FileDataFetcher` — データ取得ユーティリティ（`DataFetcher.cs`、進捗通知は `FetcherReport` 構造体）
- `EasingFunction` — イージング関数ライブラリ（`Bounce` のみ未実装で、現状は `Linear` と同一の動作）
- `RetryableWorker` — リトライロジックの抽象基底。派生に `CountdownRetryableWorker` / `TimeBasedCountdownRetryableWorker`
- `WebDownloader` — Web コンテンツダウンロード（進捗通知は `WebDownloadProgress` 構造体）
- `ThrottleableProgress<T>` — 通知間引き機能付き進捗追跡（`Progress.cs`）
- `TaskStateMachine<TContext>` — タスクベースステートマシン（現状は実質スタブ: コンテキスト未保持・遷移 API 未実装）

**例外クラス**（所在はいずれも `Runtime/Kernel/`）:
- `ImtException` — 基底例外クラス
- `GameServiceAlreadyExistsException` — サービス重複登録時
- `GameServiceNotFoundException` — サービス未発見時

**その他**:
- `GameServiceUpdate` — PlayerLoop タイミング用の16個のネストマーカー構造体を持つ
- `GameServiceManagerStartup` — `[Obsolete]`。サービス起動は `GameServiceManager.Startup()` 内で同期的に行われるため不使用
- `GameServiceManagerCleanup` — `RemoveService` によるサービス破棄を処理するため常に PlayerLoop に注入される
- `GameShutdownAnswer` — `Approve`, `Reject`（現状どこからも参照されていない未使用型）
- `GameMainAttribute` — `RuntimeInitializeOnLoadMethodAttribute` を継承した属性。エントリポイントメソッドに付与すると `BeforeSceneLoad` タイミングで自動呼び出しされる
- `ImtUnityUtility` — `CreatePersistentGameObject()` 等の静的ユーティリティ

## コーディング規約

- **名前空間**: ブロック形式 (`namespace X { }`) 必須。ファイルスコープ名前空間は禁止
- **Nullable**: 現在未有効（`csc.rsp` なし、`#nullable enable` 未使用）
- **レコード**: `record class` のみ可。`record struct` (C# 10) は禁止
- **非同期**: 独自 awaitable フレームワーク（`Core/Awaiter.cs` の `ImtAwaitable` 系）と `System.Threading.Tasks.Task` を使用（レガシー実装）。UniTask 禁止。`UnityEngine.Awaitable` への移行は将来検討事項（現状使用箇所なし）
- **XML ドキュメント**: 全 public/protected メンバーに必須。private メソッドは分岐処理を含むか5行以上の場合に必須
- **パフォーマンス**: PlayerLoop に注入されるメソッドでの GC Alloc は厳禁。データ保持は構造体優先
- **ファイルヘッダ**: Zlib ライセンスヘッダを全ソースファイルに付与
- **回答言語**: 日本語で回答する
