# Project C — 하이브리드 캐주얼 액션 RPG

## 프로젝트 개요
Archero 스타일의 하이브리드 캐주얼 액션 RPG 프로토타입.
캐주얼한 조작(이동만)으로 즐기면서, RPG 성장 시스템으로 장기 잔존율을 확보하는 구조.

## 기획 의도
- **코어**: 단순 조작 (이동) + 자동 공격 → 즉각적인 재미
- **메타**: 장비 강화, 스킬 빌드, 재화 경제 → 장기 리텐션
- **수익 모델**: 광고(리워드) + IAP 혼합 (하이브리드)

## 레퍼런스
- Archero (코어 루프, 스킬 선택 구조)
- Survivor.io (웨이브 구조, 장비 시스템)

---

## 기획

### 컨셉
- **테마**: 다크 던전 (어두운 던전을 탐험하는 닌자)
- **세계관**: 저주받은 던전에 잠든 보물을 찾아 잠입하는 닌자
- **그래픽**: 3D 탑다운 뷰
- **시점**: Archero와 동일한 고정 탑다운 카메라 (약간 기울어진 쿼터뷰)
- **타겟 플랫폼**: Mobile (Android / iOS) — Galaxy S22 기준 (1080x2340)

### 플레이어
- **컨셉**: 닌자
- **기본 무기**: 표창 (자동 발사)
- **조작**: 다이나믹 조이스틱 (화면 터치 위치에 생성), 멈추면 자동 공격
- **차별점**: Archero의 활 → 표창으로 변경, 닌자 테마에 맞는 스킬 체계

### 스킬 (레벨업 시 3개 중 택 1, 최대 Lv5)
| 스킬명 | 설명 | Lv5 유니크 |
|--------|------|-----------|
| 다중 표창 | 표창 발사 수 +1 (Lv1=2발 ~ Lv4=5발) | 6발 + 관통 |
| 독 묻히기 | 표창에 지속 데미지 부여 | 독이 주변 적에게 전염 |
| 분신술 | 분신이 함께 공격 (Lv1~2: 1개, Lv3~4: 2개) | 분신 3개 + 관통 |
| 질풍 | 이동속도 증가 + 이동 중 공격 가능 | 이동 중 공격속도 2배 |
| 연막탄 | 피격 시 일정 확률로 회피 | 회피 시 순간이동 + 무적 |
| 수리검 회전 | 플레이어 주위를 도는 수리검 (최대 3개) | 관통 + 크기 2배 |
| 자석 | 경험치 구슬 흡수 범위 확장 | 맵 전체 자동 흡수 |

### 적 / 던전
- **일반 몹**: 해골 병사, 박쥐, 슬라임 등 던전 테마 몬스터
- **보스**: 각 스테이지 마지막 방에 등장
- **웨이브 구조**: 무한 서바이벌, 전부 처치 시 다음 웨이브 (3초 쿨타임)
- **웨이브 난이도 곡선**:
  - 적 수: 3 + (웨이브-1) × 2
  - HP: 기본값 × (1 + (웨이브-1) × 0.25)
  - 데미지: 기본값 × (1 + (웨이브-1) × 0.15)
  - 이동속도: 기본값 × (1 + (웨이브-1) × 0.05)
  - 스케일: 1 + (웨이브-1) × 0.01
- **기본 스탯 (웨이브 1 기준)**:
  - 적: HP 100 / 데미지 10 / 이동속도 2
  - 플레이어: HP 50 / 표창 데미지 50

---

## 개발 현황

### ✅ 완료
- 플레이어 이동 (다이나믹 조이스틱)
- 자동 공격 (가장 가까운 적에게 표창 발사, 멈출 때만)
- 적 기본 AI (NavMeshAgent 기반 플레이어 추적, 에이전트 자동 분리)
- 적 충돌 데미지 + 넉백 (재접촉 시에도 항상 넉백 적용)
- 적 HP / 플레이어 HP 관리 (인스펙터 실시간 확인)
- HP 바 UI (World Space, 빌보드 효과)
- 웨이브 스폰 시스템 (웨이브마다 적 수 증가, NavMesh 기반 맵 안 스폰 보정)
- 플레이어 무적 시간 (피격 후 1.5초, 깜빡임 시각 피드백)
- 웨이브 UI (상단 웨이브 번호 + 시작/클리어 중앙 알림)
- 플레이어 사망 처리 (게임 오버 UI, 씬 재시작)
- 경험치 구슬 드롭 + 레벨업 시스템
- 스킬 선택 UI (레벨업 시 3개 중 택 1, 최대 Lv5, 유니크 강화)
- 스킬 7종 코드 구현 (다중 표창, 독 묻히기, 분신술, 질풍, 연막탄, 수리검 회전, 자석)
- 플레이어/적/표창 3D 모델 + 애니메이션 연동
- 적 사망 애니메이션 + 페이드 아웃
- 던전 맵 제작 (용암 던전 테마, NavMesh 베이크 완료)
- 골드 시스템 (적 사망 시 UI로 코인 날아가는 연출 + 카운트업 애니메이션)
- 스킬 카드 등장 애니메이션 (회전 연출 + 선택 잠금)
- 일시정지 버튼 (BGM/SFX 토글, 이어하기, 재시작, 로비 이동)
- 카메라 경계 제한 (BoxCollider 기반 클램프 + 고정 회전)
- 스킬 디버그 키 (에디터 전용, 숫자 1~7키로 스킬 즉시 레벨업)
- 수리검 회전 지속 데미지 (OnTriggerStay, 0.5초 쿨다운)
- 분신술 그림자 리워크 (플레이어 모델 복제, 검은 반투명, 딜레이 추적)
- 이동 중 공격 시 표창/분신 조준 보정
- 전투 피드백 시스템 (데미지 팝업, 몬스터 피격 빨간 깜빡임, MISS 표시, 레벨업 이펙트)
- 웨이브 난이도 곡선 (HP/데미지/속도/스케일 점진적 강화)
- 보스 시스템 (5웨이브마다, 돌진/충격파/바닥폭격 패턴, 상단 HP바)
- 보스 HP바 UI (캔버스 직접 배치, Inspector 연결)
- 웨이브 알림 텍스트 Inspector 커스터마이징

### 🔧 에디터 작업 (Inspector에서 직접)

#### 즉시 해야 할 것
1. **WaveManager** → `Debug Boss First Wave` 체크 해제
2. **WaveManager** → `Boss Name`을 `던전 수호자`로 확인
3. **Enemy 프리팹** → `Scale Per Wave`가 `0.01`인지 확인
4. **SpinningStarSkill** → `Orbit Radius`가 `2.0`인지 확인
5. **GoldManager** → `Gold Icon Target`을 코인 **Image** 오브젝트로 변경 (현재 Text로 연결됨)
6. **SK_MultiShuriken** SO → Level Descriptions 수정:
   - Lv1: 표창 2발 발사 / Lv2: 표창 3발 발사 / Lv3: 표창 4발 발사
   - Lv4: 표창 5발 발사 / Lv5: [유니크] 표창 6발 + 관통

#### 프리팹 제작
7. **DamagePopup 프리팹** — 데미지 숫자 표시용
   - 빈 오브젝트 생성 → 자식에 `3D Object → Text - TextMeshPro` 추가
   - TMP 설정: Alignment Center, 폰트/크기 원하는 대로
   - `Assets/Prefabs/DamageText`로 저장 → Hierarchy에서 삭제
8. **LevelUpEffect 프리팹** — 레벨업 텍스트 연출용
   - 빈 오브젝트 생성 → 자식에 `3D Object → Text - TextMeshPro` 추가
   - 텍스트: "LEVEL UP!", 색상: 골드, 폰트/크기 원하는 대로
   - `Assets/Prefabs/LevelUpEffect`로 저장 → Hierarchy에서 삭제

### 🔧 코드 작업 (다음 세션)
1. **DamagePopup / LevelUpEffect** — 프리팹 기반 풀링으로 코드 변경
2. **HP 회복 아이템** — 적 처치 시 확률 드롭, 플레이어가 밟으면 회복

### 🔧 이후 작업

#### 콘텐츠
3. **로비/시작 화면** — 타이틀 UI, 게임 시작 버튼 + PauseUI 로비 버튼 연결
4. **소품 배치** — 기둥, 횃불 등 던전 오브젝트로 맵 채우기
5. **BGM/SFX 추가** — AudioManager에 실제 오디오 연결 (현재 껍데기만 있음)

#### UI / 기타
6. **전체 UI 디자인 패스** — 일시정지 팝업, HUD, 게임오버 등 비주얼 통일

### 📋 장기 작업
- 장비 시스템 (무기 3~5종)
- 장비 강화/합성
- 기획 의도서 / 운영 매뉴얼 문서

### 📌 참고 (현재 임시 처리된 것들)
- **로비 버튼**: 누르면 현재 씬 재로드됨 → 로비 씬 만들면 `PauseUI.cs`의 `OnLobbyClicked()` TODO 수정
- **오디오**: `AudioManager.cs`에 BGM/SFX ON/OFF 상태만 저장됨 → AudioSource 추가 후 TODO 주석 부분 구현
- **카메라 경계**: 새 스테이지 추가 시 BoxCollider 오브젝트만 배치하면 됨
- **보스 테스트**: WaveManager → `Debug Boss First Wave` 체크하면 1웨이브에 보스 즉시 등장
- **스킬 디버그 키** (에디터 전용, 빌드 시 제외):
  - `1` 다중 표창 / `2` 독 묻히기 / `3` 분신술 / `4` 질풍 / `5` 연막탄 / `6` 수리검 회전 / `7` 자석
  - 누를 때마다 해당 스킬 1레벨씩 상승 (최대 Lv5)

---

## 컨벤션

### Git 커밋 메시지
| 접두어 | 용도 | 예시 |
|--------|------|------|
| `init` | 초기 설정 | `init: 프로젝트 초기 설정` |
| `feat` | 새 기능 추가 | `feat: 플레이어 이동 구현` |
| `fix` | 버그 수정 | `fix: 표창 충돌 판정 수정` |
| `docs` | 문서 수정 | `docs: README 기획 내용 추가` |
| `refactor` | 리팩토링 | `refactor: 스폰 시스템 구조 개선` |
| `chore` | 설정/기타 | `chore: .gitignore 업데이트` |

### C# 네이밍
| 구분 | 규칙 | 예시 |
|------|------|------|
| 클래스/구조체 | PascalCase | `PlayerController` |
| public 메서드 | PascalCase | `TakeDamage()` |
| private 메서드 | PascalCase | `FindClosestEnemy()` |
| 전역 변수 (필드) | _camelCase | `_moveSpeed`, `_robotController` |
| 지역 변수 | camelCase | `moveDir`, `closestEnemy` |
| 매개변수 | camelCase | `damage`, `targetPos` |
| 상수 | UPPER_SNAKE | `MAX_HP`, `SPAWN_INTERVAL` |
| SerializeField | _camelCase | `[SerializeField] float _attackRange` |

### C# 코드 구조
- **#region 순서**: Serialized Fields → Private Fields → Unity Lifecycle → 기능별 로직 → Public API
- **주석 규칙**:
  - 클래스/public 멤버 위 → `/// <summary>` (XML 문서 주석)
  - region 내부 로직 → `//` 한줄 주석
- **메서드 네이밍**: 기능별로 `Handle~` 접두어 사용 (예: `HandleMovement()`, `HandleRotation()`)

---

## 기술 스택
- **엔진**: Unity 6
- **언어**: C#
- **타겟 플랫폼**: Mobile (Android / iOS)

## 프로젝트 구조
```
Assets/
├── Scripts/
│   ├── Core/           # WaveManager, Billboard 등 공통
│   ├── Player/         # PlayerController, PlayerHealth, PlayerAttack
│   ├── Enemy/          # Enemy AI, 스폰
│   ├── Skill/          # 스킬 데이터, 선택 UI (예정)
│   ├── Item/           # 장비, 강화, 인벤토리 (예정)
│   ├── UI/             # DynamicJoystick, HUD (예정)
│   └── Util/           # 공통 유틸 (예정)
├── Prefabs/
├── ScriptableObjects/
├── Scenes/
└── Art/
```
