; ImageCropper Inno Setup Script
; Inno Setup 6.x 以降が必要です

#define MyAppName "ImageCropper"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "ImageCropper"
#define MyAppExeName "ImageCropper.exe"
#define MyAppURL ""

#define BuildOutput "..\ImageCropper\bin\Release\net10.0-windows7.0"

[Setup]
AppId={{B3F8A2C1-7D5E-4A9B-8C6F-1E2D3A4B5C6D}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
LicenseFile=..\ImageCropper\LICENSE
InfoAfterFile=ThirdPartyNotice.txt
OutputDir=output
OutputBaseFilename=ImageCropperSetup_{#MyAppVersion}
Compression=lzma2
SolidCompression=yes
SetupIconFile=..\ImageCropper\Assets\app.ico
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
WizardStyle=modern
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog
MinVersion=6.1sp1
UninstallDisplayName={#MyAppName}

[Languages]
Name: "japanese"; MessagesFile: "compiler:Languages\Japanese.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; メイン実行ファイル・設定ファイル
Source: "{#BuildOutput}\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#BuildOutput}\ImageCropper.deps.json"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#BuildOutput}\ImageCropper.runtimeconfig.json"; DestDir: "{app}"; Flags: ignoreversion

; マネージドアセンブリ (DLL) をすべて含める
Source: "{#BuildOutput}\*.dll"; DestDir: "{app}"; Flags: ignoreversion

; ライセンス・サードパーティ通知
Source: "..\ImageCropper\LICENSE"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#BuildOutput}\ThirdPartyNotices.md"; DestDir: "{app}"; Flags: ignoreversion

; Assets フォルダ
Source: "{#BuildOutput}\Assets\*"; DestDir: "{app}\Assets"; Flags: ignoreversion recursesubdirs createallsubdirs

; OpenCV ネイティブ ランタイム (x64)
Source: "{#BuildOutput}\runtimes\win-x64\native\*"; DestDir: "{app}\runtimes\win-x64\native"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Code]
// .NET 10 ランタイムがインストールされているかチェック
function IsDotNetInstalled(): Boolean;
var
  ResultCode: Integer;
begin
  Result := Exec('dotnet', '--list-runtimes', '', SW_HIDE, ewWaitUntilTerminated, ResultCode) and (ResultCode = 0);
end;

function InitializeSetup(): Boolean;
begin
  Result := True;
  if not IsDotNetInstalled() then
  begin
    if MsgBox(
      '.NET ランタイムが検出されませんでした。' + #13#10 +
      'このアプリケーションの実行には .NET 10 Desktop Runtime (x64) が必要です。' + #13#10 + #13#10 +
      'インストールを続行しますか？' + #13#10 +
      '（実行前に https://dotnet.microsoft.com/download/dotnet/10.0/runtime の「デスクトップ アプリを実行する」からランタイムをインストールしてください）',
      mbConfirmation, MB_YESNO) = IDNO then
    begin
      Result := False;
    end;
  end;
end;
