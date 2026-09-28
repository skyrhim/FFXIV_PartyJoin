# PartyJoin Usage Guide

PartyJoin is an ACT plugin for FFXIV that detects party join packets and can send notifications through a Discord webhook.

## Installation

1. Extract `PartyJoin.dll` from the release ZIP file.
2. Install the plugin using one of these methods:
	- **Add it in ACT:** Open ACT's `Plugins` tab, click `Browse...`, select `PartyJoin.dll`, and add it.
	- **Copy it directly:** Close ACT, copy `PartyJoin.dll` into the `Plugins` folder inside the ACT installation directory, and restart ACT. If the plugin does not appear in the list, add the DLL manually from ACT's `Plugins` tab.
3. Make sure `FFXIV_ACT_Plugin` is running.
4. Open the `PartyJoin` tab in ACT.

PartyJoin can receive network packets only while the FFXIV ACT Plugin is running.

## Configure Discord Notifications

1. Follow the [official Discord guide to creating a webhook](https://support.discord.com/hc/ko/articles/228383668-%EC%9B%B9%ED%9B%84%ED%81%AC-%EC%86%8C%EA%B0%9C#:~:text=%C2%A0%20Facebook-,%EC%9B%B9%ED%9B%84%ED%81%AC%20%EB%A7%8C%EB%93%A4%EA%B8%B0,-%EC%9D%B4%EB%A5%BC%20%EC%97%BC%EB%91%90%EC%97%90%20%EB%91%90%EA%B3%A0), create a webhook in the Discord channel settings, and copy its URL.
2. Open the `Party notification` tab and enable `Notify when someone joins`.
3. Enter the URL in `Discord webhook URL`.
4. Follow both the [official Discord guide to enabling Developer Mode](https://support.discord.com/hc/ko/articles/206346498-%EC%82%AC%EC%9A%A9%EC%9E%90-%EC%84%9C%EB%B2%84-%EB%A9%94%EC%8B%9C%EC%A7%80-ID%EB%8A%94-%EC%96%B4%EB%94%94%EC%84%9C-%ED%99%95%EC%9D%B8%ED%95%98%EB%82%98%EC%9A%94#h_01HRSTXPS5H5D7JBY2QKKPVKNA:~:text=%EB%AA%A8%EB%B0%94%EC%9D%BC-,%EA%B0%9C%EB%B0%9C%EC%9E%90%20%EB%AA%A8%EB%93%9C%EB%A5%BC%20%ED%99%9C%EC%84%B1%ED%99%94%ED%95%98%EB%8A%94%20%EB%B0%A9%EB%B2%95,-ID%20%EB%B2%88%ED%98%B8%EB%A5%BC%20%EB%B3%B5%EC%82%AC%ED%95%98%EA%B8%B0) and the [user ID lookup guide](https://support.discord.com/hc/ko/articles/206346498-%EC%82%AC%EC%9A%A9%EC%9E%90-%EC%84%9C%EB%B2%84-%EB%A9%94%EC%8B%9C%EC%A7%80-ID%EB%8A%94-%EC%96%B4%EB%94%94%EC%84%9C-%ED%99%95%EC%9D%B8%ED%95%98%EB%82%98%EC%9A%94#h_01HRSTXPS5H5D7JBY2QKKPVKNA:~:text=%EC%B0%BE%EB%8A%94%20%EB%B0%A9%EB%B2%95%EC%9D%84%20%EC%82%B4%ED%8E%B4%EB%B3%B4%EA%B2%A0%EC%8A%B5%EB%8B%88%EB%8B%A4.-,%EC%82%AC%EC%9A%A9%EC%9E%90%20ID%20%EB%B2%88%ED%98%B8%EB%A5%BC%20%EC%B0%BE%EB%8A%94%20%EB%B0%A9%EB%B2%95,-%EC%84%9C%EB%B2%84%2C%20%EA%B7%B8%EB%A3%B9%20%EC%B1%84%ED%8C%85), enable Developer Mode, and find the user ID.
5. Enter the Discord user ID in `Discord user IDs`. Separate multiple IDs with commas.
6. Click `Save settings`, then use `Send test` to verify the notification.

The webhook URL must start with `https://discord.com/api/webhooks/`. If no user IDs are entered, the notification is sent without mentions.

## Packet Log

For diagnostics, open the `Packet log` tab and enable `Log incoming packets`. Click `Clear` to remove the displayed log. Packet logging is intended for debugging.

## Configuration File

Settings are stored in `Config/PartyJoin.config` under ACT's application data folder.

```ini
NotifyOnPartyJoin=true
DiscordWebhookUrl=https://discord.com/api/webhooks/your-webhook-url
DiscordUserIds=123456789012345678,987654321098765432
LogPackets=false
```

Treat the webhook URL like a password. Delete and recreate it in Discord if it is exposed.

## Troubleshooting

- `FFXIV ACT Plugin is not running.`: Start the FFXIV ACT Plugin.
- `Discord webhook URL is empty.`: Enter a webhook URL and save the settings.
- `Discord webhook URL is invalid.`: Copy the complete Discord webhook URL.
- No notification arrives: Check the settings and webhook permissions, then use `Send test`.

한국어 문서: [plugin-usage-ko.md](plugin-usage-ko.md)