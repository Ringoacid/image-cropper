# ImageCropper リリース手順 (Release Skill)

ImageCropper アプリケーションの新バージョンをビルド・パッケージングし、GitHub リリースを作成するための手順です。

## 前提条件

このスキルを実行する前に、以下の環境が整っていることを確認してください：
- **.NET 10.0 SDK**: アプリケーションのビルドに必要です。
- **Inno Setup 6.x**: インストーラ（`ISCC.exe`）のコンパイルに必要です。標準インストールパス：`C:\Program Files (x86)\Inno Setup 6\ISCC.exe`
- **GitHub CLI (`gh`)**: リリースの作成およびアセットのアップロードに必要です。認証済みである必要があります。
- **Git**: 変更のコミット、プッシュ、およびタグ付けに必要です。

## リリースプロセス

### 1. バージョン情報の更新

新しいバージョン（例: `1.3.0.0`）に合わせて、以下のファイルを書き換えます。

- **`ImageCropper/ImageCropper.csproj`**:
  `<AssemblyVersion>` および `<FileVersion>` を新しいバージョン番号に書き換えます。
  ```xml
  <AssemblyVersion>1.3.0.0</AssemblyVersion>
  <FileVersion>1.3.0.0</FileVersion>
  ```

- **`installer/ImageCropperSetup.iss`**:
  `#define MyAppVersion` の値を新しいバージョンに書き換えます。
  ```pascal
  #define MyAppVersion "1.3.0.0"
  ```

- **`CLAUDE.md`**:
  `Current Version:` を更新し、末尾のバージョン履歴テーブルに変更内容を追記します。

- **`VersionHistory.md`**:
  先頭に新バージョンの変更履歴（日本語）を追加します。

### 2. リリースビルドの生成

dotnet コマンドを用いて、Release 構成でアプリケーションをビルドします。

```powershell
dotnet build -c Release ImageCropper/ImageCropper.csproj
```

### 3. インストーラ（セットアップファイル）のコンパイル

Inno Setupのコマンドラインコンパイラ (`ISCC.exe`) を使用して、ビルド成果物をパッケージングします。

```powershell
& "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" installer/ImageCropperSetup.iss
```
出力先：`installer/output/ImageCropperSetup_<バージョン番号>.exe`

### 4. 変更のコミットとプッシュ

更新したソースコードとドキュメントをコミットし、Gitリモートリポジトリにプッシュします。

```powershell
git add .
git commit -m "bump: バージョンを <バージョン番号> に更新し、リリースノートを追加"
git push origin main
```

### 5. GitHub リリースの作成

GitHub CLI (`gh`) を使用してリリースを作成し、ビルドしたインストーラを添付します。
リリースノート用のテキスト（日本語）を `release_notes.txt` に一時的に出力し、それを指定してリリースを作成します。

```powershell
# リリースノートの作成（例）
Get-Content VersionHistory.md | Select-Object -First 30 | Out-File -FilePath release_notes.txt -Encoding utf8

# リリースの作成とアセット添付
gh release create v<バージョン番号> installer/output/ImageCropperSetup_<バージョン番号>.exe --title "v<バージョン番号>" --notes-file release_notes.txt

# 一時ファイルの削除
Remove-Item release_notes.txt
```

---

## 自動化スクリプトによる一括実行

上記のプロセス（2〜5）を自動化するための PowerShell スクリプト `release.ps1` がルートディレクトリに配置されています。
以下のコマンドを実行することで、ビルドからGitHubリリースまでを自動的に実行できます。

```powershell
.\release.ps1 -Version "<バージョン番号>"
```

※実行前に、手順1の「バージョン情報の更新」が完了し、Gitワークツリーに変更がある状態にしてください。
