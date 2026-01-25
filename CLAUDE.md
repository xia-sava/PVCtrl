# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## プロジェクト概要

PVCtrl は OBS を制御する Windows デスクトップアプリケーションです。
録画開始・停止、録画停止予約、OBS プロジェクター表示、音声ミュート等を UI から操作できます。

## アーキテクチャ

### 主要コンポーネント

- **MainWindow.xaml / MainWindow.xaml.cs**: メイン UI（WPF）
  - 録画ファイル名入力
  - 録画開始・停止
  - 録画停止予約と残り時間表示
  - OBS プロジェクター表示/終了と音声ミュート

- **MainViewModel.cs**: MVVM ロジック
  - UI 状態管理、コマンド、メッセージ出力
  - 録画停止予約・残り時間更新

- **ObsService.cs**: OBS 制御
  - OBS WebSocket 接続
  - 録画開始・停止
  - プロジェクター表示/終了
  - OBS 起動・終了

- **RecTimerService (PvCtrlUtil.cs)**: 録画停止予約タイマー
  - 1 秒間隔で残り時間更新
  - アラーム時刻でサウンド再生

- **AudioMuteService.cs**: プロセス別音声ミュート切替（NAudio）

- **AwakeOnBatchService.cs**: バッチ中のスリープ抑止
  - TMPGEnc バッチ処理の UI Automation 検知

### 外部依存

- **WPF + WinForms**: UI フレームワーク
- **.NET 8 (net8.0-windows)**: ターゲットフレームワーク
- **CommunityToolkit.Mvvm**: MVVM
- **obs-websocket-dotnet**: OBS WebSocket 制御
- **NAudio**: 音声ミュート切替
- **UI Automation**: バッチ処理検知

## ビルド方法

### Visual Studio を使用する場合
```bash
# ソリューションファイルを開いてビルド
PVCtrl.sln
```

### dotnet CLI を使用する場合
```bash
dotnet build PVCtrl.sln
```

### ビルド構成
- Debug: デバッグ用ビルド
- Release: リリース用ビルド
- x64/AnyCPU: プラットフォーム設定

## 開発時の注意事項

### OBS 連携
- OBS WebSocket の接続先は `ws://localhost:4455`
- OBS 実行ファイルの想定パス:
  - `C:\Program Files\obs-studio\bin\64bit\obs64.exe`

### UI Automation 使用時の考慮
- TMPGEnc バッチの UI 要素検知でスリープ抑止判定

### ファイル名の正規化
- Windows で使用できない文字を全角文字に自動置換
- 改行文字や余分なスペースを除去

### タイマー機能
- 録画の自動停止機能を実装
- 録画終了後の OBS プロジェクター終了オプション付き
