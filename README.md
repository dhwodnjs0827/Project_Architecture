# Unity Project Architecture

Unity 게임 개발을 위한 기본 프로젝트 아키텍처 템플릿입니다.

## 개요

- **Unity 버전**: 2022.3+ LTS 권장
- **주요 패턴**: Singleton, Strategy, Observer
- **비동기 처리**: UniTask

## 프로젝트 구조

```
Project_Architecture/
├── Jenkinsfile                  # Jenkins CI/CD 파이프라인
├── parser/                      # Google Sheets 파서
│   ├── sheet_parser.py          # 파싱 스크립트
│   └── requirements.txt         # Python 의존성
│
└── Assets/_Project/
    ├── 0_Scenes/                # 게임 씬
    │   ├── TitleScene.unity
    │   ├── LobbyScene.unity
    │   ├── GameScene.unity
    │   └── SampleScene.unity
    │
    ├── 1_Scripts/
    │   ├── Core/                # 핵심 게임 로직
    │   ├── Data/Generated/      # 자동 생성 데이터 클래스
    │   ├── Editor/              # 에디터 유틸리티
    │   ├── Enums/               # 열거형 정의
    │   ├── Interfaces/          # 인터페이스
    │   ├── Managers/            # 싱글톤 매니저
    │   ├── Scene/               # 씬 관리 클래스
    │   ├── UI/                  # UI 시스템
    │   └── Utils/               # 유틸리티 및 Extensions
    │
    ├── 3_Prefabs/               # 프리팹
    ├── 4_Animations/            # 애니메이션
    ├── 5_Sprites/               # 스프라이트
    │
    └── Resources/
        ├── Audio/               # 오디오 리소스
        ├── Data/                # JSON 및 ScriptableObject
        ├── UI/                  # UI Canvas 프리팹
        └── Prefabs/             # 런타임 로드 프리팹
```

## 핵심 시스템

### Manager 시스템

| Manager | 설명 |
|---------|------|
| `EventManager` | 전역 이벤트 발행/구독 시스템 |
| `ResourceManager` | 리소스 로딩 및 캐싱 (Resources/Addressable) |
| `AudioManager` | BGM/SFX 재생, 볼륨 관리, Fade 효과 |
| `ObjectPoolManager` | 오브젝트 풀링 시스템 |
| `UIManager` | UI 열기/닫기, Canvas 계층 관리 |
| `SceneLoadManager` | 비동기 씬 전환 |

### 유틸리티

| 클래스 | 설명 |
|--------|------|
| `Singleton<T>` | 순수 C# 싱글톤 |
| `MonoSingleton<T>` | MonoBehaviour 싱글톤 |
| `CDebug` | 조건부 디버그 로깅 (CUSTOM_DEBUG 심볼) |
| `TimeFormatUtil` | 시간 포맷팅 유틸리티 |

### Extension 메서드

| 클래스 | 주요 메서드 |
|--------|------------|
| `EnumExtensions` | `ToInt()`, `Next()`, `Previous()`, `GetRandom<T>()` |
| `StringExtensions` | `ToInt()`, `ToFloat()`, `ToBool()`, `ToEnum<T>()` |
| `ListExtensions` | `GetRandom<T>()`, `Shuffle<T>()` |
| `RectTransformExtensions` | `GetWorldRect()`, `GetScreenRect()`, `ContainsScreenPoint()` |

## 사용 방법

### EventManager

```csharp
// 이벤트 구독
EventManager.Subscribe(EventType.GameStart, OnGameStart);
EventManager.Subscribe<int>(EventType.GameOver, OnGameOver);

// 이벤트 발행
EventManager.Dispatch(EventType.GameStart);
EventManager.Dispatch(EventType.GameOver, score);

// 구독 해제
EventManager.Unsubscribe(EventType.GameStart, OnGameStart);
```

### ResourceManager

```csharp
// 리소스 로드
var clip = await ResourceManager.Instance.LoadAsync<AudioClip>("Audio/BGM/Title");

// 리소스 해제
ResourceManager.Instance.Release(clip);
```

### AudioManager

```csharp
// BGM 재생 (Fade In)
await AudioManager.Instance.PlayBgmAsync("Audio/BGM/Title", fade: true);

// SFX 재생
await AudioManager.Instance.PlaySfxAsync("Audio/SFX/Click");

// 볼륨 조절 (0~1)
AudioManager.Instance.SetMasterVolume(0.8f);
AudioManager.Instance.SetBgmVolume(0.5f);
```

### ObjectPoolManager

```csharp
// 프리팹에서 오브젝트 가져오기
var bullet = ObjectPoolManager.Instance.Get<Bullet>(bulletPrefab);

// 풀에 반환
ObjectPoolManager.Instance.Release(bullet);

// 미리 생성
ObjectPoolManager.Instance.Preload(bulletPrefab, 20);
```

### UIManager

```csharp
// UI 열기
var popup = await UIManager.Instance.OpenAsync<SettingsPopup>();

// UI 닫기
UIManager.Instance.Close<SettingsPopup>();

// 열린 UI 가져오기
var hud = UIManager.Instance.GetUI<GameHUD>();
```

### SceneLoadManager

```csharp
// 씬 전환
await SceneLoadManager.Instance.LoadSceneAsync(SceneType.GameScene);
```

## 데이터 파이프라인

데이터 파이프라인은 두 단계로 구성됩니다:

```
[Google Sheets] → (sheet_parser.py) → [JSON + C# 클래스] → (JsonToSOParser) → [ScriptableObject]
```

### 1단계: Google Sheets → JSON + C# 클래스

`parser/sheet_parser.py` 스크립트가 Google Sheets에서 데이터를 가져와 JSON 파일과 C# 클래스를 자동 생성합니다.

#### 설치

```bash
cd parser
pip install -r requirements.txt
```

#### 환경 변수 설정

| 변수 | 설명 |
|------|------|
| `GOOGLE_CREDENTIALS_PATH` | Google 서비스 계정 JSON 키 파일 경로 |
| `SPREADSHEET_ID` | Google Sheets 문서 ID |

#### 실행

```bash
export GOOGLE_CREDENTIALS_PATH="credentials.json"
export SPREADSHEET_ID="your-spreadsheet-id"
python sheet_parser.py
```

#### Google Sheets 형식

| 행 | 내용 | 예시 |
|----|------|------|
| 1행 | 주석 (필드 설명) | `아이디`, `이름`, `가격` |
| 2행 | 타입 | `int`, `string`, `int` |
| 3행 | 키 (필드명) | `id`, `name`, `price` |
| 4행~ | 데이터 | `1`, `Sword`, `100` |

**지원 타입**:
- 기본: `int`, `float`, `string`, `bool`
- 배열: `int[]`, `float[]`, `string[]`
- 리스트: `List<int>`, `List<float>`, `List<string>`
- Enum: Enum 타입명 직접 사용 (예: `EventType`)

**시트 필터링**:
- `!`, `@`, `#`로 시작하는 시트는 무시됨
- 데이터 행에서 첫 열이 `#`로 시작하면 해당 행 무시 (주석 처리)

#### 출력

```
parser/
└── sheet_parser.py

↓ 실행 후 생성

Assets/_Project/
├── Resources/Data/JSON/
│   ├── ItemData.json
│   └── TestData.json
└── 1_Scripts/Data/Generated/
    ├── ItemData.cs
    ├── ItemDataSO.cs
    ├── TestData.cs
    └── TestDataSO.cs
```

### 2단계: JSON → ScriptableObject 변환

Unity 에디터에서 JSON을 ScriptableObject로 변환합니다.

1. `Tools > Data > Parse All` 메뉴 실행
2. `Resources/Data/SO/` 폴더에 ScriptableObject 자동 생성

**JSON 예시** (`ItemData.json`):
```json
[
  { "id": 1, "name": "Sword", "price": 100, "description": "A sharp sword" },
  { "id": 2, "name": "Shield", "price": 80, "description": "A sturdy shield" }
]
```

**생성되는 클래스** (`Data/Generated/ItemData.cs`):
```csharp
namespace Generated
{
    [Serializable]
    public class ItemData
    {
        public int id;
        public string name;
        public int price;
        public string description;
    }
}
```

### Jenkins CI/CD 파이프라인

프로젝트 루트의 `Jenkinsfile`을 사용하여 Google Sheets 데이터 동기화를 자동화합니다.

#### 파이프라인 단계

```
Checkout → Setup Python → Parse Google Sheets → Commit & Push
```

| 단계 | 설명 |
|------|------|
| **Checkout** | `data-sync` 브랜치 체크아웃 |
| **Setup Python** | 가상환경 생성 및 의존성 설치 |
| **Parse Google Sheets** | `sheet_parser.py` 실행 |
| **Commit & Push** | 변경사항 자동 커밋 및 푸시 |

#### Jenkins Credentials 설정

Jenkins에 다음 Credentials를 등록해야 합니다:

| Credential ID | 타입 | 설명 |
|---------------|------|------|
| `spreadsheet-id` | Secret text | Google Sheets 문서 ID |
| `google-sheets-credentials` | Secret file | Google 서비스 계정 JSON 키 |
| `github-credentials` | Username with password | GitHub 인증 정보 |

#### Jenkins Job 설정

1. Jenkins에서 새 Pipeline Job 생성
2. Pipeline script from SCM 선택
3. Repository URL 및 Credentials 설정
4. Script Path: `Jenkinsfile`

#### Unity 에디터에서 트리거

1. `Tools > Google Sheets Sync` 메뉴 열기
2. Jenkins URL, Job Name, 인증 정보 입력
3. `Trigger Sheets Build` 버튼 클릭

#### 워크플로우

```
[Unity Editor]
     │
     ▼ Trigger Build (HTTP POST)
[Jenkins]
     │
     ├─ 1. git checkout data-sync
     ├─ 2. pip install requirements
     ├─ 3. python sheet_parser.py
     │      ├─ Google Sheets API 호출
     │      ├─ JSON 파일 생성
     │      └─ C# 클래스 생성
     └─ 4. git commit & push
     │
     ▼ Pull 또는 Merge
[Unity Project]
     │
     ▼ Tools > Data > Parse All
[ScriptableObject 생성]
```

## UI 시스템

### Canvas 계층 구조

| UIType | Sort Order | 용도 |
|--------|-----------|------|
| HUD | 0 | 게임 HUD |
| UI | 100 | 일반 UI |
| Popup | 200 | 팝업 창 |
| Tooltip | 300 | 툴팁 |
| Loading | 400 | 로딩 화면 |
| System | 500 | 시스템 메시지 |

### UIBase 상속

```csharp
public class SettingsPopup : UIBase
{
    protected override void Opened(params object[] args)
    {
        // UI 열릴 때 로직
    }

    protected override void Closed(params object[] args)
    {
        // UI 닫힐 때 로직
    }
}
```

## 씬 시스템

### BaseScene 상속

```csharp
public class GameScene : BaseScene
{
    public override SceneType SceneType => SceneType.GameScene;

    public override async UniTask InitializeAsync()
    {
        // 씬 초기화 로직
        await UniTask.CompletedTask;
    }

    public override async UniTask CleanupAsync()
    {
        // 씬 정리 로직
        await UniTask.CompletedTask;
    }
}
```

## 오브젝트 풀링

### IPoolable 인터페이스

```csharp
public class Bullet : MonoBehaviour, IPoolable
{
    public void OnGet()
    {
        // 풀에서 가져올 때 초기화
    }

    public void OnRelease()
    {
        // 풀에 반환할 때 정리
    }
}
```

## 스크립팅 심볼

| 심볼 | 설명 |
|------|------|
| `CUSTOM_DEBUG` | CDebug 로그 출력 활성화 |
| `ADDRESSABLE` | Addressable Asset System 사용 |

**설정 방법**: `Project Settings > Player > Scripting Define Symbols`

## 의존성

- [UniTask](https://github.com/Cysharp/UniTask) - 비동기 처리
- [Newtonsoft.Json](https://www.newtonsoft.com/json) - JSON 파싱
- [Addressables](https://docs.unity3d.com/Packages/com.unity.addressables@latest) (선택)
- [DOTween](http://dotween.demigiant.com/) - 트윈 애니메이션
- [Odin Inspector](https://odininspector.com/) - 인스펙터 확장

## 라이선스

MIT License