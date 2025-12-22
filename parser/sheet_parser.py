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
    """모든 워크시트 목록 가져오기 (!, @, # 접두사 제외)"""
    spreadsheet = client.open_by_key(spreadsheet_id)
    worksheets = spreadsheet.worksheets()

    INVALID_PREFIXES = ('!', '@', '#')
    valid_sheets = [
        ws for ws in worksheets
        if not ws.title.startswith(INVALID_PREFIXES)
    ]

    return valid_sheets


def parse_sheet(worksheet):
    """시트 데이터를 파싱"""
    try:
        rows = worksheet.get_all_values()

        if len(rows) < 4:
            print(f'⚠ Skip {worksheet.title}: not enough rows')
            return None, None, None, None

        # 주석
        type_desc = row[0]
        # 타입
        type_row = rows[1]
        # 키 값
        key_row = rows[2]
        # 데이터 값
        data_rows = rows[3:]

        parsed = []

        for row in data_rows:
            if not row:
                continue

            if row[0].startswith('#'):
                continue

            item = {}
            for i in range(len(key_row)):
                key = key_row[i]
                value_type = type_row[i]
                value = row[i] if i < len(row) else ''

                item[key] = parse_value(value, value_type)

            parsed.append(item)

        print(f'✓ Parsed: {worksheet.title} ({len(parsed)} rows)')
        return parsed, type_row, key_row, type_desc

    except Exception as e:
        print(f'✗ Error parsing {worksheet.title}: {e}')
        return None, None, None, None


def parse_value(value, value_type):
    """값을 타입에 맞게 변환"""
    if value == '' or value == 'None':
        return None

    if value_type == 'int':
        return int(value)

    if value_type == 'float':
        return float(value)

    if value_type == 'string':
        return value

    if value_type == 'bool':
        return value.lower() in ('true', '1', 'yes')

    if value_type in ('List<int>', 'int[]'):
        if not value:
            return []
        return [int(v.strip()) for v in value.split(',') if v.strip()]

    if value_type in ('List<float>', 'float[]'):
        if not value:
            return []
        return [float(v.strip()) for v in value.split(',') if v.strip()]

    if value_type in ('List<string>', 'string[]'):
        if not value:
            return []
        return [v.strip() for v in value.split(',') if v.strip()]

    return value


def save_to_json(data, output_path):
    """JSON 파일로 저장"""
    os.makedirs(os.path.dirname(output_path), exist_ok=True)
    with open(output_path, 'w', encoding='utf-8') as f:
        json.dump(data, f, ensure_ascii=False, indent=2)
    print(f'  → Saved: {output_path}')


def generate_csharp_classes(sheet_name, type_row, key_row, type_desc, output_dir):
    """C# 데이터 클래스 및 SO 클래스 생성"""

    # 타입 매핑
    type_mapping = {
        'int': 'int',
        'float': 'float',
        'string': 'string',
        'bool': 'bool',
        'List<int>': 'List<int>',
        'List<float>': 'List<float>',
        'List<string>': 'List<string>',
        'int[]': 'int[]',
        'float[]': 'float[]',
        'string[]': 'string[]',
    }

    # 필드 생성
    fields = []
    for i in range(len(key_row)):
        key = key_row[i]
        if not key:
            continue
        value_type = type_row[i] if i < len(type_row) else 'string'
        cs_type = type_mapping.get(value_type, 'string')
        value_desc = type_desc[i]
        fields.append(f'    public {cs_type} {key} // {value_desc};')

    fields_str = '\n'.join(fields)

    # 데이터 클래스
    data_class = f'''using System;
using System.Collections.Generic;

namespace Generated
{{
    [Serializable]
    public class {sheet_name}
    {{
    {fields_str}
    }}
}}
  '''

    # SO 클래스
    so_class = f'''using System.Collections.Generic;
using UnityEngine;

namespace Generated
{{
    [CreateAssetMenu(fileName = "{sheet_name}SO", menuName = "Data/{sheet_name}SO")]
    public class {sheet_name}SO : ScriptableObject
    {{
    {fields_str}
    }}
}}
  '''

    # 폴더 생성
    os.makedirs(output_dir, exist_ok=True)

    # 파일 저장
    data_path = f'{output_dir}/{sheet_name}.cs'
    so_path = f'{output_dir}/{sheet_name}SO.cs'

    with open(data_path, 'w', encoding='utf-8') as f:
        f.write(data_class)
    print(f'  → Generated: {data_path}')

    with open(so_path, 'w', encoding='utf-8') as f:
        f.write(so_class)
    print(f'  → Generated: {so_path}')


def main():
    # 환경 변수에서 설정 읽기
    credentials_path = os.environ.get('GOOGLE_CREDENTIALS_PATH', 'credentials.json')
    spreadsheet_id = os.environ.get('SPREADSHEET_ID')
    json_output_dir = 'Assets/_Project/Resources/Data/JSON'
    cs_output_dir = 'Assets/_Project/1_Scripts/Data/Generated'

    if not spreadsheet_id:
        print('Error: SPREADSHEET_ID environment variable is not set')
        sys.exit(1)

    print('=== Google Sheets Parser ===')
    print(f'Spreadsheet ID: {spreadsheet_id[:10]}...')
    print(f'JSON Output: {json_output_dir}')
    print(f'C# Output: {cs_output_dir}')
    print()

    # 클라이언트 생성
    try:
        client = get_sheet_client(credentials_path)
    except Exception as e:
        print(f'Error: Failed to authenticate: {e}')
        sys.exit(1)

    # 모든 워크시트 가져오기
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
        json_path = f'{json_output_dir}/{sheet_name}.json'

        data, type_row, key_row = parse_sheet(worksheet)

        if data is not None:
            save_to_json(data, json_path)
            generate_csharp_classes(sheet_name, type_row, key_row, type_desc, cs_output_dir)
            success_count += 1

    print()
    print(f'=== Complete: {success_count}/{len(worksheets)} sheets parsed ===')

    if success_count == 0:
        sys.exit(1)


if __name__ == '__main__':
    main()
