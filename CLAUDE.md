# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## プロジェクト概要

PVCtrl は PV4（EARTH SOFT のビデオプレイヤー）を制御する Windows Forms アプリケーションです。
録画機能の自動化と UI からの簡単な操作を提供します。

## アーキテクチャ

### 主要コンポーネント

- **Form1.cs / PvCtrl**: メイン UI フォーム
  - 録画開始・停止
  - タイマー設定
  - ファイル名管理

- **PvCtrlUtil.cs**: PV4 プロセス制御のユーティリティ
  - UI Automation による PV4 メニュー操作
  - プロセス管理（起動・終了・ウィンドウ状態制御）
  - タイマー機能での自動録画停止
  - SendKeys による自動ダイアログ操作

- **Settings.cs**: 設定管理
  - JSON 形式での設定の永続化
  - ユーザー設定の保存・読み込み

- **WheelableNumericUpDown.cs**: カスタム NumericUpDown コントロール
  - マウスホイール対応の数値入力コントロール

### 外部依存

- **UI Automation**: PV4 のメニュー操作に使用
- **System.Windows.Forms**: UI フレームワーク
- **.NET Framework 4.8**: ターゲットフレームワーク

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
- Remote: リモート環境用ビルド
- x64/AnyCPU: プラットフォーム設定

## 開発時の注意事項

### PV4 連携
- PV4 プロセスの検出は `ProcessName == "PV"` で行う
- インストールパスは以下を検索:
  - `C:\Program Files\EARTH SOFT\PV\PV.exe`
  - `C:\Program Files (x86)\EARTH SOFT\PV\PV.exe`

### UI Automation 使用時の考慮
- メニュー操作は階層的にアイテムを指定（例: `new[] {"ファイル", "録画..."}`）
- ダイアログへの入力は SendKeys を使用
- プロセスの優先度を High に設定して安定性を確保

### ファイル名の正規化
- Windows で使用できない文字を全角文字に自動置換
- 改行文字や余分なスペースを除去

### タイマー機能
- 録画の自動停止機能を実装
- 録画終了後の PV 自動クローズオプション付き
