# 💠 프로젝트명: 방치형 검사 : 닭부터 잡자

귀엽고 유쾌한 SD 전사 캐릭터들이 등장하는 2D 모바일 방치형 RPG 게임으로,

**전설의 검술가가 되기 위해 수련을 시작한 주인공이 닭부터 잡으며 점점 강한 적들과 맞서 싸우는 여정**을 그립니다.


- 🎮 개발 기간: 2025.06.20 ~ 2025.08.14
- 🛠️ 개발 환경: Unity 6000.1.8f1
- 📱 플랫폼: Android (모바일 전용)  
- 💻 PC 환경: WebGL 빌드 제공 (itch.io 통해 브라우저에서 플레이 가능)
  

## 👥 만든 사람들
- [**임예슬**](https://github.com/Imysss)
- [**손채민**](https://github.com/ChaeminSohn)
- [**김경민**](https://github.com/rudals4469)
- [**신희승**](https://github.com/HS-9006)

🤍[팀 노션](https://www.notion.so/teamsparta/13-2172dc3ef51480aa93c9d8b284b36aeb)

🎞️ [시연 영상](https://youtu.be/XhhWhLDwNo4)

## 📂 목차
1. [게임 소개](-게임-소개)
2. [주요 구현 기능](-주요-구현-기능)
3. [기능 명세서](#-기능-명세서)
4. [사용 에셋](#-사용-에셋)

## 🕹️ 게임 소개

- 장르: 2D 모바일 방치형 RPG
- 설명: 점점 더 강해지는 적들을 처치하며 플레이어를 성장시켜 나가는 게임

<details>
<summary>게임 소개 배너</summary>

<br>

![banner](Images/banner1.png)
![cutscene](Images/cutScene.png) ![tutorial](Images/tutorial.png)
![dungeon](Images/dungeon.png) ![gacha](Images/gacha.png)
![offlineReward](Images/offlineReward.png)

</details>


## 🛠️ 주요 구현 기능

- **게임 시스템 및 콘텐츠 루프 완성**
    - 스탯 업그레이드, 장비/스킬/동료 뽑기 및 장착, 자동 스킬 시스템
    - 골드 던전 / 보스 러시 / 방치 보상 / 컷신 기반 튜토리얼 흐름 구현
        → **방치형 RPG의 기본 구조와 콘텐츠 루프를 실현**  
- **Excel 기반 외부 데이터 설계 + 내부 데이터베이스 시스템 구축**
    - 장비/스킬/동료/가챠/퀘스트 등 **모든 데이터를 Excel 기반 테이블로 설계**
    - 이를 내부 **데이터베이스 구조로 연동 및 관리** → **유저의 성장/보유 상태도 저장 가능**
    - 유저의 스탯, 보유 아이템, 게임 진행 상황 등을 **직렬화하여 자동 저장/로드 처리**
- **UI/UX 설계 및 게임 흐름 구현**
    - 버튼 흐름 기반 튜토리얼 + 에그 대장 내레이션으로 유저 가이드
    - 장착 시 외형 변화, 자동 스킬 토글, 가챠 연출 등 직관적이고 몰입감 있는 인터페이스 구성
- **Google Play 출시 준비 및 테스트 중**
    - APK 빌드 및 업로드 테스트 완료
    - 사전 테스트 사용자 피드백을 기반으로 **완성도 개선 및 버그 수정 진행 중**


## 📃 기능 명세서

<details>
<summary>UML, 기능 정리</summary>
  
<br>

**UI**

![ui](Images/uiDiagram.png)

**플레이어, 적**

![playerEnemy](Images/playerEnemyDiagram.png)

**스킬**

![skill](Images/skillDiagram.png)

**아이템**

![item](Images/itemDiagram.png)

**로그인**

![login](Images/loginDiagram.png)

**방치 보상**

![offlineRewardDiagram](Images/offlineRewardDiagram.png)

**가챠**

![gachaDiagram](Images/gachaDiagram.png)

**스탯 강화**

![statUpgrade](Images/statUpgradeDiagram.png)

**프로필 수정**

![propfile](Images/profileDiagram.png)



**퀘스트**

![quest](Images/questDiagram.png)

**튜토리얼**

![tutorial](Images/tutorialDiagram.png)

</details>

### 로그인

| 스크립트 | 내용 | 기여자 |
|----------|------|--------|
| [GuestManager](Scripts/Firebase/GuestManager.cs) | 게스트 로그인 기능| 신희승 |
| [FirebaseInitializer](Scripts/Firebase/FirebaseInitializer.cs) | Firebase SDK 초기화 | 신희승 |
| [GoogleLoginManager](Scripts/Firebase/GoogleLoginManager.cs) | 구글 로그인 | 신희승 |
| [SaveLoadManager](Scripts/Manager/Core/SaveLoadManager.cs) | 사용자 데이터 불러오기, 서버 API 연동 | 신희승, 손채민 |



## 📦 사용 에셋

- https://assetstore.unity.com/packages/2d/characters/character-editor-fantasy-90592
- https://assetstore.unity.com/packages/2d/characters/fantasy-monsters-animated-megapack-159572
- https://assetstore.unity.com/packages/2d/characters/fantasy-monsters-animated-bosses-300879
- https://assetstore.unity.com/packages/2d/gui/icons/fantasy-inventory-icons-117467
- https://assetstore.unity.com/packages/2d/textures-materials/nature/fantasy-backgrounds-megapack-153154
- https://assetstore.unity.com/packages/2d/gui/idle-game-vertical-ui-kit-315169
- https://assetstore.unity.com/packages/vfx/particles/game-vfx-slash-collection-urp-293636

