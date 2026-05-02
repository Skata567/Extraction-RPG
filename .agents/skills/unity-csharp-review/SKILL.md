---
name: unity-csharp-review
description: Unity C# 변경 사항을 리뷰할 때 MonoBehaviour 책임, Inspector 참조, ScriptableObject 오용, Physics2D 설정, Null 가능성, v0.1 범위 준수를 점검하는 스킬.
origin: project
---

# Unity C# 리뷰 스킬

이 스킬은 Unity C# 코드를 작성하거나 수정한 뒤 사용한다.
목표는 스타일 지적이 아니라 실제 버그, 유지보수 위험, Unity 설정 누락 가능성을 찾는 것이다.

## 언제 사용할 것

- `.cs` 파일을 새로 만들거나 수정한 뒤
- 플레이어, 적, 전투, 인벤토리, 던전, 경제 로직을 건드린 뒤
- ScriptableObject, Prefab, Scene 참조가 생긴 뒤
- PR을 만들기 전
- 버그 수정 후 재발 가능성을 확인할 때

## 리뷰 우선순위

### 치명적 문제

- `NullReferenceException` 가능성이 높은 Inspector 참조
- ScriptableObject 원본 데이터를 런타임 저장소처럼 직접 수정
- 플레이어/적 체력, 인벤토리, 골드 같은 핵심 데이터 손실 가능성
- 씬 전환 시 Manager 중복 생성 가능성
- 프리팹 또는 씬에 반드시 필요한 컴포넌트가 코드에서 보장되지 않음
- 파일 삭제, 저장 데이터 변경, 외부 패키지 추가 같은 위험 작업을 경고 없이 수행

### 높은 우선순위

- 하나의 `MonoBehaviour`가 너무 많은 책임을 가짐
- 입력, 이동, 전투, UI, 데이터 저장이 한 클래스에 섞임
- `Update()`에서 무거운 탐색이나 할당을 반복함
- `FindObjectOfType`, `GameObject.Find`, 태그 검색을 매 프레임 호출함
- `public` 필드를 Inspector 노출 용도로 남발함
- `catch`에서 오류를 조용히 무시함
- 태그, 레이어, Trigger, Rigidbody2D 설정에 의존하지만 문서화하지 않음

### 중간 우선순위

- 매직 넘버가 Inspector나 상수로 분리되지 않음
- 메서드가 너무 길고 테스트하기 어려움
- 같은 로직이 여러 스크립트에 중복됨
- 이벤트 구독 해제가 누락될 가능성
- Gizmo가 있으면 좋은 감지 범위/공격 범위를 시각화하지 않음
- 주석이 너무 뻔하거나, 중요한 설계 이유가 빠짐

## Unity C# 작성 기준

- Inspector 노출 값은 기본적으로 `private` + `[SerializeField]`를 사용한다.
- Inspector 설정에는 한글 `[Header]`, `[Tooltip]`을 붙인다.
- 필수 컴포넌트가 있으면 `[RequireComponent]` 사용을 고려한다.
- `Awake`에서는 자기 자신과 필수 참조 초기화를 우선한다.
- `Start`에서는 다른 오브젝트와의 연결처럼 씬 초기화 이후 필요한 처리를 둔다.
- 물리 이동은 가능하면 `FixedUpdate`와 `Rigidbody2D`를 기준으로 한다.
- 입력 수집과 물리 적용은 역할을 분리한다.
- `OnEnable`에서 이벤트를 구독했다면 `OnDisable`에서 해제한다.
- ScriptableObject는 기본 데이터이며, 원정 아이템/골드/체력 같은 런타임 상태를 저장하지 않는다.

## 리뷰 출력 형식

리뷰 결과는 아래 순서로 정리한다.

```md
# 리뷰 결과

## 발견한 문제
- [심각도] 파일:라인 - 문제와 이유

## 수정 제안
- 어떤 구조로 고치면 좋은지

## 확인이 필요한 Unity 설정
- Inspector
- Prefab
- Tag/Layer
- Scene

## 테스트 제안
- EditMode 테스트
- PlayMode 테스트
- 수동 플레이 테스트
```

문제가 없으면 억지로 지적하지 말고, 남은 테스트 위험만 짧게 말한다.
