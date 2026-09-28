# PartyJoin 사용법

PartyJoin은 FFXIV ACT Plugin에서 파티 참가 패킷을 감지하고 Discord 웹훅으로 알림을 보낼 수 있는 ACT 플러그인입니다.

## 설치

1. 릴리스 ZIP에서 `PartyJoin.dll`을 압축 해제합니다.
2. 다음 두 방법 중 하나로 플러그인을 설치합니다.
	- **ACT에서 수동 추가:** ACT의 `Plugins` 탭에서 `Browse...`를 눌러 `PartyJoin.dll`을 선택하고 추가합니다.
	- **Plugins 폴더에 직접 복사:** ACT를 종료한 뒤 ACT 설치 폴더의 `Plugins` 폴더에 `PartyJoin.dll`을 복사하고 ACT를 다시 실행합니다. 플러그인이 목록에 나타나지 않으면 ACT의 `Plugins` 탭에서 해당 DLL을 수동으로 추가합니다.
3. `FFXIV_ACT_Plugin`이 실행 중인지 확인합니다.
4. ACT에서 `PartyJoin` 탭을 엽니다.

PartyJoin은 FFXIV ACT Plugin이 실행 중일 때만 네트워크 패킷을 수신합니다.

## Discord 알림 설정

1. [Discord 공식 웹훅 만들기 안내](https://support.discord.com/hc/ko/articles/228383668-%EC%9B%B9%ED%9B%84%ED%81%AC-%EC%86%8C%EA%B0%9C#:~:text=%C2%A0%20Facebook-,%EC%9B%B9%ED%9B%84%ED%81%AC%20%EB%A7%8C%EB%93%A4%EA%B8%B0,-%EC%9D%B4%EB%A5%BC%20%EC%97%BC%EB%91%90%EC%97%90%20%EB%91%90%EA%B3%A0)를 참고하여 Discord 채널의 웹훅을 만들고 URL을 복사합니다.
2. `Party notification` 탭에서 `Notify when someone joins`를 선택합니다.
3. `Discord webhook URL`에 웹훅 URL을 입력합니다.
4. [Discord 공식 개발자 모드 활성화 안내](https://support.discord.com/hc/ko/articles/206346498-%EC%82%AC%EC%9A%A9%EC%9E%90-%EC%84%9C%EB%B2%84-%EB%A9%94%EC%8B%9C%EC%A7%80-ID%EB%8A%94-%EC%96%B4%EB%94%94%EC%84%9C-%ED%99%95%EC%9D%B8%ED%95%98%EB%82%98%EC%9A%94#h_01HRSTXPS5H5D7JBY2QKKPVKNA:~:text=%EB%AA%A8%EB%B0%94%EC%9D%BC-,%EA%B0%9C%EB%B0%9C%EC%9E%90%20%EB%AA%A8%EB%93%9C%EB%A5%BC%20%ED%99%9C%EC%84%B1%ED%99%94%ED%95%98%EB%8A%94%20%EB%B0%A9%EB%B2%95,-ID%20%EB%B2%88%ED%98%B8%EB%A5%BC%20%EB%B3%B5%EC%82%AC%ED%95%98%EA%B8%B0)와 [사용자 ID 찾기 안내](https://support.discord.com/hc/ko/articles/206346498-%EC%82%AC%EC%9A%A9%EC%9E%90-%EC%84%9C%EB%B2%84-%EB%A9%94%EC%8B%9C%EC%A7%80-ID%EB%8A%94-%EC%96%B4%EB%94%94%EC%84%9C-%ED%99%95%EC%9D%B8%ED%95%98%EB%82%98%EC%9A%94#h_01HRSTXPS5H5D7JBY2QKKPVKNA:~:text=%EC%B0%BE%EB%8A%94%20%EB%B0%A9%EB%B2%95%EC%9D%84%20%EC%82%B4%ED%8E%B4%EB%B3%B4%EA%B2%A0%EC%8A%B5%EB%8B%88%EB%8B%A4.-,%EC%82%AC%EC%9A%A9%EC%9E%90%20ID%20%EB%B2%88%ED%98%B8%EB%A5%BC%20%EC%B0%BE%EB%8A%94%20%EB%B0%A9%EB%B2%95,-%EC%84%9C%EB%B2%84%2C%20%EA%B7%B8%EB%A3%B9%20%EC%B1%84%ED%8C%85)를 순서대로 참고하여 개발자 모드를 활성화하고 사용자 ID를 확인합니다.
5. 확인한 Discord 사용자 ID를 `Discord user IDs`에 입력합니다. 여러 ID는 쉼표로 구분합니다.
6. `Save settings`를 누른 다음 `Send test`로 알림을 확인합니다.

웹훅 URL은 `https://discord.com/api/webhooks/`로 시작해야 합니다. 사용자 ID를 입력하지 않으면 멘션 없이 알림을 보냅니다.

## 패킷 로그

진단이 필요할 때 `Packet log` 탭에서 `Log incoming packets`를 선택합니다. `Clear`로 표시된 로그를 지울 수 있습니다. 패킷 로그는 디버깅 목적으로만 사용하는 것을 권장합니다.

## 설정 파일

설정은 ACT 애플리케이션 데이터 폴더 아래 `Config/PartyJoin.config`에 저장됩니다.

```ini
NotifyOnPartyJoin=true
DiscordWebhookUrl=https://discord.com/api/webhooks/your-webhook-url
DiscordUserIds=123456789012345678,987654321098765432
LogPackets=false
```

웹훅 URL이 노출되면 Discord에서 해당 웹훅을 삭제하고 새로 만드세요.

## 문제 해결

- `FFXIV ACT Plugin is not running.`: FFXIV ACT Plugin을 실행합니다.
- `Discord webhook URL is empty.`: 웹훅 URL을 입력하고 저장합니다.
- `Discord webhook URL is invalid.`: 전체 Discord 웹훅 URL을 복사했는지 확인합니다.
- 알림이 오지 않음: 설정과 웹훅 권한을 확인한 뒤 `Send test`를 실행합니다.

English documentation: [plugin-usage-en.md](plugin-usage-en.md)