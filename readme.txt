Watermelona.unity
====

메세지 기반 프로그래밍

Goal
----
* 네트워크 레이어의 완전 추상화
* 쓸데없는 `Manager` 클래스들 안만들어도됨
* 오브젝트와 오브젝트간의 디커플링
* http://pjc0247.tistory.com/81

Message Modeling
----
```c#
public class AvatarChanged {
    public Player sender {get;set;}
    public Avatar avatar {get;set;}
}
```
가장 먼저 사용할 메세지를 모델링합니다.<br>
메세지는 내부적으로만 사용되는 이벤트일수도 있고, 네트워크 패킷일수도, 둘 다 일수도 있습니다.

Basic Subscription
----
```c#
class MyProfileUI {

  [Subscriber(typeof(AvatarChanged))]
  public void OnAvatarChanged(AvatarChanged e) {
    if (e.player != CurrentPlayer)
      return;

    texture = e.avatar.texture;
    name = e.avatar.name;
  }
}
```
`Subscriber` 속성을 지정하면 자동으로 구독이 등록됩니다.<br>
로컬 이벤트와, 네트워크 패킷간에 전혀 다른 코드를 작성할 이유가 없습니다.

Basic Broadcasting
----
```c#
// 아바타 바꾸기 버튼 클릭 시 ....
PubSub.Publish(new AvatarChanged() {
    sender = CurrentPlayer,
    avatar = new Avatar(...)
});
```
`Publish` API를 이용하여 전송할 메세지를 생성하고, 발송합니다.<br>
대상 목적지는 로컬일수도, 다른 피어 클라이언트일수도, 서버일수도 있습니다.

Message Channel
----
```c#
[Channel("network")]
public class ChatMessage { };
```
```c#
public class NetworkPublisher
{
    // 특정 타입 대신, 채널 이름으로 구독합니다.
    [Subscriber("network")]
    public void OnNetworkMessage(object msg)
    {
        /* 서버 또는 피어로 전송 */
    }
}
```

기본적으로 모든 메세지는 로컬에 방송됩니다.<br>
하지만 채널 기능을 사용하여 네트워크 전용 채널 구독기를 만든 후, 해당 구독기가 네트워크에 다시 Publish 하는 방법으로 네트워크상에 메세지를 전송할 수 있습니다.

```c#
public class ChattingUI
{
    [Subscriber(typeof(ChatMessage)]
    public void OnChatMessage(ChatMessage packet) 
    {
        /* 채팅 UI 처리 */
    }
}
```
채널이 등록된 메세지도, 타입으로 구독될 수 있습니다.
<br>
결과적으로 어떤 오브젝트가 `ChatMessage` 메세지를 방송하게 되면 로컬에 있는 `ChattingUI` 와 `NetworkPublisher`에게 전달되고,<br>
`NetworkPublisher`는 네트워크 송신 역할을, `ChattingUI`는 채팅 메세지의 UI 처리를 수행하게 됩니다.

Recording
----
```c#
[Recordable]
public class MoveCharacter { /* ... */ };
```
`Recordable` 속성을 지정하면 녹화 가능한 메세지가 됩니다.<br>
이러한 종류의 메세지들은 `Recorder` API에 의해서 녹화될 수 있습니다.

```c#
var recorder = Recorder.Create("RECORDER_NAME");

[Subscriber(typeof(GameStart))]
void OnGameStart() {
    recorder.Start();
}
[Subscriber(typeof(GameEnd))]
void OnGameEnd() {
    recorder.Stop();
}
```
위 코드는 새 레코더를 생성하고, 게임 시작부터 끝날때까지의 메세지를 녹화합니다.<br>
녹화된 메세지들은 파일에 저장되거나, 다시 재생될 수 있습니다.<br>
다시 재생될 경우, 레코더가 기록한 delta time에 따라 메세지를 순차적으로 다시 publish 하게 되므로
별도의 분기 처리 없이도 리플레이를 재생할 수 있게 됩니다.

Advanced Recording
----
```c#
[Channel("game.move")]
[Recordable]
public class MoveCharacter { };

// Recordable(string)은 Channel + Recordable의 쇼트컷입니다.
[Recordable("game.chat")]
public class ChatMessage { };

[Recordable("game.chat.emoji")]
public class ChatEmoji { };
```

```c#
// 채팅 레코더를 생성합니다.
//   메세지와, 이모지 전부 녹화합니다.
Recorder.Create("CHATTING_LOG", "game.chat.*");

// 채팅 레코더를 생성합니다.
//   메세지만 녹화합니다.
Recorder.Create("CHATTING_LOG_MESSAGE", "game.chat");

// 모든 Recordable 메세지를 녹화합니다.
Recorder.Create("ALL", "*");
```


MockupEditor
----
![mock](imgs/events.png)
<br>
이벤트를 런타임에 임의로 만들어낼 수 있습니다.<br>
public 프로퍼티들에 대해서 자동으로 에디트할 수 있는 UI가 제공됩니다.

StaticMockup
----
![static](imgs/static.png)
<br>

```c#
[PacketScheme]
public S2CPackets
{
    public static PlayEnd PlayEnd_WinPlayer() {
        return new PlayEnd() {
            winner = CurrentPlayer.playerId
	};
    }
    public static PlayEnd PlayEnd_Opponent() {
        return new PlayEnd() {
            winner = Opponent.playerId
	};
    }
}
```
정적 목업은 미리 작성된 코드를 통해 메세지에 값을 채워넣을 수 있습니다.<br>
런타임에 값이 바인딩되기 때문에 실제 게임 상황에 맞는 값을 대입하여 목업할 수 있습니다.<br>
개발자는 제공되는 UI 버튼을 통해 메세지 전송 시점만을 컨트롤합니다.

Fade
----
![fade](img/fade.png)
<br>
특정 메세지가 수신되면 페이드 액션을 수행하는 컴포넌트입니다.

Activate
----
![act](img/active.png)
<br>
특정 메세지가 수신되면 활성화 상태를 변경하는 컴포넌트입니다.

SetText
----
![text](img/set_text.png)
<br>
특정 메세지가 수신되면 메세지 안의 값을 UI 텍스트에 바로 바인딩하는 컴포넌트입니다.<br>
이 컴포넌트를 활용해 상대방 플레이어 정보를 수신한 후, 코드 없이 바로 라벨에 닉네임 등을 출력할 수 있습니다.