# UI Prefab Checklist

이 문서는 인벤토리, 전리품, 상점, 가방, 창고 UI를 프리팹으로 만들 때 필요한 오브젝트와 역할을 정리한다.

현재 코드는 `PrototypeRTBootstrapper`가 런타임에 UI를 직접 생성한다. 빠른 테스트에는 좋지만, 위치, 색상, 이미지, 폰트를 Unity Editor에서 수정하기 어렵다. 그래서 장기적으로는 아래 UI를 프리팹으로 만들어 연결하는 방향이 좋다.

## 핵심 프리팹

### InventoryWindow_RT.prefab

추천 경로:

```text
Assets/NYH/03.Prefab/UI/InventoryWindow_RT.prefab
```

역할:

- 전리품 루팅
- 플레이어 가방 확인
- 나중의 상점
- 나중의 창고
- 나중의 장비 관리

공통으로 사용할 큰 인벤토리 창이다.

추천 구조:

```text
InventoryWindow_RT
├─ Background
├─ Header
│  └─ TitleText
├─ PlayerSection
│  ├─ SectionTitle
│  ├─ EquipmentArea
│  │  ├─ WeaponSlot
│  │  ├─ HeadSlot
│  │  ├─ BodySlot
│  │  ├─ PocketSlot_L
│  │  └─ PocketSlot_R
│  ├─ BagTitle
│  └─ PlayerBagMount
├─ TargetSection
│  ├─ SectionTitle
│  └─ TargetGrid
├─ TooltipPanel
│  └─ TooltipText
└─ HintText
```

## 오브젝트별 제작 방식

### InventoryWindow_RT

구성:

- `RectTransform`
- `Image`
- `LootPanelUI`

역할:

- 창 전체를 켜고 끄는 최상위 루트다.
- 배경은 어두운 반투명 색을 추천한다.
- 나중에 상점, 창고, 시체 루팅, 보물상자 UI가 같은 창 구조를 공유할 수 있다.

이미지 필요 여부:

- 통짜 배경 이미지는 필수 아님.
- 단색 `Image`로 시작하고, 나중에 테두리 장식이 필요하면 9-slice Sprite로 교체한다.

### Background

구성:

- `RectTransform`
- `Image`

역할:

- 전체 창의 뒤판이다.
- 게임 화면과 UI를 분리해서 루팅 중이라는 느낌을 준다.

이미지 필요 여부:

- 처음에는 Unity `Image` 단색으로 충분하다.
- 나중에 금속 프레임, 낡은 가죽, 어두운 철판 같은 테마가 정해지면 Sprite로 교체한다.

### Header / TitleText

Header 구성:

- `RectTransform`
- 필요하면 `Image`

TitleText 구성:

- `Text`

역할:

- 현재 열린 창의 이름을 표시한다.
- 예: `전리품`, `상자`, `상점`, `창고`

이미지 필요 여부:

- `TitleText`는 이미지로 만들지 않는다.
- 텍스트는 런타임에 바뀔 수 있으므로 `Text` 또는 나중에 `TextMeshPro`를 사용한다.

### PlayerSection

구성:

- `RectTransform`
- `Image`

역할:

- 플레이어의 장비, 주머니, 가방을 보여주는 왼쪽 영역이다.

이미지 필요 여부:

- 단색 `Image`로 시작한다.
- 나중에 섹션별 테두리 이미지를 붙일 수 있다.

### EquipmentArea

구성:

- `RectTransform`

역할:

- 무기, 머리, 몸, 주머니 슬롯을 묶는 빈 부모 오브젝트다.
- 지금은 실제 장착 로직을 넣지 않고 자리만 잡는다.

이미지 필요 여부:

- 필요 없음.

### WeaponSlot / HeadSlot / BodySlot / PocketSlot_L / PocketSlot_R

구성:

- `RectTransform`
- `Image`

역할:

- 장비 슬롯 자리 표시용이다.
- 지금 단계에서는 실제 장착 로직을 담당하지 않는다.
- 나중에 `EquipmentSlotUI` 같은 스크립트를 붙일 수 있다.

이미지 필요 여부:

- 슬롯 프레임 Sprite를 쓰면 좋다.
- 하지만 초기에는 단색 `Image`와 라벨만으로 충분하다.

추천 이름:

```text
UI_Inventory_Slot_Weapon
UI_Inventory_Slot_Head
UI_Inventory_Slot_Body
UI_Inventory_Slot_Pocket
```

### PlayerBagMount

구성:

- `RectTransform`

역할:

- 기존 플레이어 가방 `ItemGrid`가 루팅창 안으로 임시 이동될 위치다.
- 이 오브젝트 자체는 아이템을 담지 않는다.
- 말 그대로 "가방 격자를 붙여둘 자리"다.

이미지 필요 여부:

- 필요 없음.

중요:

- 현재 코드에서는 플레이어의 기존 `ItemGrid`를 이 위치로 옮겼다가 창을 닫으면 원래 자리로 되돌린다.
- 그래서 `PlayerBagMount`는 크기와 위치만 정확하면 된다.

### TargetSection

구성:

- `RectTransform`
- `Image`

역할:

- 몬스터 전리품, 상자, 상점 목록, 창고 목록을 보여줄 오른쪽 영역이다.

이미지 필요 여부:

- 단색 `Image`로 시작한다.

### TargetGrid

구성:

- `RectTransform`
- `Image`
- `ItemGrid`

역할:

- 전리품 아이템을 실제 격자 형태로 보여준다.
- 아이템 크기, 배치, 공간 검사 로직은 `ItemGrid`가 담당한다.

이미지 필요 여부:

- 격자 배경 이미지는 선택이다.
- 초기에는 단색 `Image`와 셀 표시용 자식 오브젝트로 충분하다.

추천 설정:

```text
Width: 10
Height: 10
Cell Size: 32 x 32
```

### TooltipPanel / TooltipText

TooltipPanel 구성:

- `RectTransform`
- `Image`

TooltipText 구성:

- `Text`

역할:

- 마우스를 올린 아이템의 정보를 보여준다.
- 현재 표시 후보:
  - 아이템 이름
  - 크기
  - 타입
  - 판매가

이미지 필요 여부:

- 패널 배경은 단색 `Image`로 충분하다.
- 텍스트는 이미지로 만들지 않는다.

### HintText

구성:

- `Text`

역할:

- 조작 안내와 실패 메시지를 보여준다.
- 예:
  - `아이템을 클릭하거나 가방으로 드래그해서 옮깁니다.`
  - `가방 공간이 부족합니다.`
  - `비어 있습니다.`

이미지 필요 여부:

- 이미지로 만들지 않는다.

## 별도 추천 프리팹

### InventoryGrid_RT.prefab

추천 경로:

```text
Assets/NYH/03.Prefab/UI/InventoryGrid_RT.prefab
```

구성:

- `RectTransform`
- `Image`
- `ItemGrid`

역할:

- 가방, 전리품, 창고, 상점 목록에서 재사용할 격자다.

주의:

- 현재 `ItemGrid`는 32px 셀을 기준으로 동작한다.
- 격자 크기는 `width * 32`, `height * 32`로 맞춘다.

### InventorySlot_RT.prefab

추천 경로:

```text
Assets/NYH/03.Prefab/UI/InventorySlot_RT.prefab
```

구성:

- `RectTransform`
- `Image`
- 선택: `Text`

역할:

- 장비 슬롯, 주머니 슬롯, 안전 주머니 슬롯, 상점 슬롯에 재사용한다.

지금은 실제 아이템 장착 기능을 넣지 않는다.

## 이미지로 만들 것과 만들지 않을 것

이미지로 만들면 좋은 것:

- 패널 배경
- 패널 테두리
- 슬롯 프레임
- 격자 셀 배경
- 아이템 아이콘

이미지로 만들지 않는 것이 좋은 것:

- 아이템 이름
- 가격
- 수량
- 내구도
- 창 제목
- 안내 문구
- 툴팁 텍스트

텍스트는 런타임에 자주 바뀌므로 이미지로 만들면 유지보수가 힘들어진다.

## 추천 제작 순서

1. `InventoryWindow_RT`의 큰 레이아웃을 단색 UI Image로 만든다.
2. `PlayerSection`, `TargetSection`, `TooltipPanel`의 위치와 크기를 잡는다.
3. `PlayerBagMount`와 `TargetGrid` 위치를 잡는다.
4. 실제 루팅을 테스트한다.
5. 사용감이 괜찮으면 슬롯 프레임과 패널 배경을 Sprite로 교체한다.
6. 마지막에 상점, 창고, 장비 슬롯 기능을 연결한다.

## 왜 이렇게 만드는가

전체 UI를 하나의 통짜 이미지로 만들면 빠르게 예뻐 보일 수는 있다. 하지만 가방 크기, 슬롯 위치, 텍스트, 상점 탭, 창고 탭이 바뀔 때마다 이미지를 다시 수정해야 한다.

반대로 Unity UI 오브젝트로 나누면 처음에는 조금 번거롭지만, 나중에 다음 확장이 쉽다.

- 전리품 UI를 상점 UI로 재사용
- 가방 격자 크기 변경
- 아이템 툴팁 내용 추가
- 장비 슬롯 기능 추가
- 창고와 플레이어 가방을 같은 화면에 표시
- 패널 스킨만 교체

이 프로젝트는 시스템이 단계적으로 늘어날 Extraction RPG이므로, 통짜 이미지보다 조립형 UI 프리팹이 더 적합하다.
