# python utf8_crlf_converter.py .. cs xaml

import argparse
from pathlib import Path
from tqdm import tqdm
import sys

def convert_encoding_and_newline(file_path):
    """
    ファイルを読み込み、UTF-8 (BOMなし) & CRLF に変換して上書き保存する。
    元のエンコーディングは一般的なものから推定する。
    """
    encodings_to_try = ['utf-8', 'cp932', 'shift_jis', 'euc-jp', 'utf-16']
    
    content = None
    detected_enc = None
    
    # 1. バイナリとして読み込む
    try:
        raw_data = file_path.read_bytes()
    except Exception as e:
        return False, f"読み込みエラー: {e}"

    # 2. エンコーディングを推定してデコード
    for enc in encodings_to_try:
        try:
            content = raw_data.decode(enc)
            detected_enc = enc
            break
        except UnicodeDecodeError:
            continue
    
    if content is None:
        return False, "エンコーディング判定不能"

    # 3. 【修正ポイント】改行コードを一度 '\n' に正規化する
    # これを行わないと、元々 \r\n だったものが書き込み時に重複して変換される場合がある
    content = content.replace('\r\n', '\n').replace('\r', '\n')

    # 4. 書き込み処理 (UTF-8, CRLF)
    try:
        # newline='\r\n' を指定すると、メモリ内の '\n' が '\r\n' に変換されて書き込まれる
        with file_path.open('w', encoding='utf-8', newline='\r\n') as f:
            f.write(content)
        return True, detected_enc
    except Exception as e:
        return False, f"書き込みエラー: {e}"

def main():
    parser = argparse.ArgumentParser(description='指定した拡張子のファイルをUTF-8(CRLF)に変換します。')
    parser.add_argument('target_dir', type=str, help='検索対象のフォルダパス')
    parser.add_argument('extensions', type=str, nargs='+', help='対象の拡張子 (例: txt py c cpp)')
    
    args = parser.parse_args()

    target_path = Path(args.target_dir)
    target_exts = {
        ext.lower() if ext.startswith('.') else f'.{ext.lower()}' 
        for ext in args.extensions
    }

    if not target_path.exists():
        print(f"エラー: 指定されたフォルダが存在しません -> {target_path}")
        sys.exit(1)

    print(f"検索対象: {target_path}")
    print(f"対象拡張子: {', '.join(target_exts)}")
    print("ファイル検索中...")

    files = [
        p for p in target_path.rglob('*') 
        if p.is_file() and p.suffix.lower() in target_exts
    ]

    total_files = len(files)
    if total_files == 0:
        print("対象のファイルが見つかりませんでした。")
        sys.exit(0)

    print(f"{total_files} 個のファイルが見つかりました。変換を開始します。")

    success_count = 0
    error_count = 0
    errors = []

    for file_path in tqdm(files, unit="file"):
        success, msg = convert_encoding_and_newline(file_path)
        if success:
            success_count += 1
        else:
            error_count += 1
            errors.append(f"{file_path.name}: {msg}")

    print("\n" + "="*30)
    print("処理完了")
    print(f"成功: {success_count} ファイル")
    print(f"失敗: {error_count} ファイル")
    
    if error_count > 0:
        print("\n[エラー詳細]")
        for err in errors:
            print(err)

if __name__ == "__main__":
    main()