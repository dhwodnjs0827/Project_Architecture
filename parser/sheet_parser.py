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

def parse_sheet(client, spreadsheet_id, sheet_name):
    """시트 데이터를 딕셔너리 리스트로 변환"""
    try:
        spreadsheet = client.open_by_key(spreadsheet_id)
        worksheet = spreadsheet.worksheet(sheet_name)
        records = worksheet.get_all_records()
        print(f'✓ Parsed: {sheet_name} ({len(records)} rows)')
        return records
    except gspread.exceptions.WorksheetNotFound:
        print(f'✗ Worksheet not found: {sheet_name}')
        return None
    except Exception as e:
        print(f'✗ Error parsing {sheet_name}: {e}')
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

    if not spreadsheet_id:
        print('Error: SPREADSHEET_ID environment variable is not set')
        sys.exit(1)

    # 파싱할 시트 목록 (시트이름, 출력경로)
    sheets_config = [
        ('ItemData', 'Assets/_Project/Resources/Data/ItemData.json'),
        #('MonsterData', 'Assets/_Project/Resources/Data/MonsterData.json'),
        #('StageData', 'Assets/_Project/Resources/Data/StageData.json'),
    ]

    print('=== Google Sheets Parser ===')
    print(f'Spreadsheet ID: {spreadsheet_id[:10]}...')
    print()

    # 클라이언트 생성
    try:
        client = get_sheet_client(credentials_path)
    except Exception as e:
        print(f'Error: Failed to authenticate: {e}')
        sys.exit(1)

    # 각 시트 파싱
    success_count = 0
    for sheet_name, output_path in sheets_config:
        data = parse_sheet(client, spreadsheet_id, sheet_name)
        if data is not None:
            save_to_json(data, output_path)
            success_count += 1

    print()
    print(f'=== Complete: {success_count}/{len(sheets_config)} sheets parsed ===')

    if success_count == 0:
        sys.exit(1)

if __name__ == '__main__':
    main()
