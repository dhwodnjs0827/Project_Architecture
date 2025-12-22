import gspread
import json
import os
import sys
from oauth2client.service_account import ServiceAccountCredentials


def get_sheet_client(credentials_path):
    """Google Sheets 클라이언트 생성"""
    scope = [
        'https://spreadsheets.google.com/feeds',
        'https://www.googleapis.com/auth/drive'
    ]
    creds = ServiceAccountCredentials.from_json_keyfile_name(credentials_path, scope)
    return gspread.authorize(creds)


def get_all_worksheets(client, spreadsheet_id):
    """모든 워크시트 목록 가져오기 (! 접두사 제외)"""
    spreadsheet = client.open_by_key(spreadsheet_id)
    worksheets = spreadsheet.worksheets()

    # !, @, # 접두사가 붙은 시트 제외
    valid_sheets = [ws for ws in worksheets if not ws.title.startswith('!', '@', '#')]

    return valid_sheets


def parse_sheet(worksheet):
    """시트 데이터를 딕셔너리 리스트로 변환"""
    try:
        records = worksheet.get_all_records()
        print(f'✓ Parsed: {worksheet.title} ({len(records)} rows)')
        return records
    except Exception as e:
        print(f'✗ Error parsing {worksheet.title}: {e}')
        return None


def save_to_json(data, output_path):
    """JSON 파일로 저장"""
    os.makedirs(os.path.dirname(output_path), exist_ok=True)
    with open(output_path, 'w', encoding='utf-8') as f:
        json.dump(data, f, ensure_ascii=False, indent=2)
    print(f'  → Saved: {output_path}')


def main():
    # 환경 변수에서 설정 읽기
    credentials_path = os.environ.get('GOOGLE_CREDENTIALS_PATH', 'credentials.json')
    spreadsheet_id = os.environ.get('SPREADSHEET_ID')
    output_dir = 'Assets/_Project/Resources/Data/JSON'

    if not spreadsheet_id:
        print('Error: SPREADSHEET_ID environment variable is not set')
        sys.exit(1)

    print('=== Google Sheets Parser ===')
    print(f'Spreadsheet ID: {spreadsheet_id[:10]}...')
    print(f'Output Directory: {output_dir}')
    print()

    # 클라이언트 생성
    try:
        client = get_sheet_client(credentials_path)
    except Exception as e:
        print(f'Error: Failed to authenticate: {e}')
        sys.exit(1)

    # 모든 워크시트 가져오기 (! ,@, # 제외)
    try:
        worksheets = get_all_worksheets(client, spreadsheet_id)
        print(f'Found {len(worksheets)} sheets to parse')
        print()
    except Exception as e:
        print(f'Error: Failed to get worksheets: {e}')
        sys.exit(1)

    # 각 시트 파싱
    success_count = 0
    for worksheet in worksheets:
        sheet_name = worksheet.title
        output_path = f'{output_dir}/{sheet_name}.json'

        data = parse_sheet(worksheet)
        if data is not None:
            save_to_json(data, output_path)
            success_count += 1

    print()
    print(f'=== Complete: {success_count}/{len(worksheets)} sheets parsed ===')

    if success_count == 0:
        sys.exit(1)


if __name__ == '__main__':
    main()
