# UnityHSM
A hierarchical state machine for Unity


## Hierarchical State Machine (HSM) for Unity
Unity에서 복잡한 캐릭터 행동을 효율적이고 구조적으로 관리하기 위해 설계된 계층적 상태 머신(HSM) 패키지입니다. 
이 패키지를 사용하면 상태를 계층적으로 구성하여, 일반적인 상태 머신에서 발생할 수 있는 복잡성과 중복을 크게 줄일 수 있습니다.

### 설치 방법
이 패키지는 유니티 패키지 매니저(UPM)를 통해 Git URL로 쉽게 설치할 수 있습니다.
1. 유니티 에디터에서 Window > Package Manager를 엽니다.
2. 왼쪽 상단의 + 버튼을 클릭하고 Add package from git URL...을 선택합니다.
3. 다음 Git URL을 입력하고 Add를 클릭합니다:[https://github.com/WoojinKim1225/UnityHSM.git](https://github.com/WoojinKim1225/UnityHSM.git)
(만약 특정 버전을 사용하려면 #v1.0.0과 같이 태그를 추가하세요.)

### 핵심 개념
- __계층적 상태(Hierarchical States)__: 상태는 부모-자식 관계를 가질 수 있습니다.  예를 들어, `Grounded` 상태는 `Idle`과 `Move` 상태의 부모가 될 수 있습니다. 이는 상태 전이 로직을 간결하게 만듭니다.
- __상태 전이(State Transition)__: 상태 전이는 자식 상태에서 부모 상태로, 또는 같은 계층의 다른 상태로 발생할 수 있습니다. GetTransition() 메서드를 오버라이드하여 전이 조건을 정의합니다.
- __빌더 패턴(Builder Pattern)__: `StateMachineBuilder`는 상태 클래스들의 계층 구조를 자동으로 탐색하여 상태 머신을 초기화합니다. 개발자는 복잡한 수동 연결 없이 상태 클래스 인스턴스만 생성하면 됩니다.

### 사용 방법
__1. `ICharacter` 인터페이스 구현__
상태 머신이 제어할 캐릭터 클래스(예: PhysicsCharacter.cs)에 `ICharacter` 인터페이스를 구현해야 합니다. 이 인터페이스는 상태 머신이 캐릭터의 상태 배열에 접근할 수 있도록 `states` 속성을 요구합니다.
```
// 이미 구현됨
public interface ICharacter
{
    System.Type[] states { get; set; }
}
```
```
// 예시: Character.cs
public class Character : MonoBehaviour, ICharacter
{
    public System.Type[] states { get; set; }
    public Rigidbody Rigidbody;
    public Vector2 InputDirection;
}
```
__2. 상태(`State`) 클래스 정의__
모든 상태는 `State<T>`를 상속받아야 합니다. `T`는 앞서 정의한 `ICharacter`를 구현하는 캐릭터 클래스입니다. 
- 부모 상태를 정의할 때는 생성자에서 자식 상태들을 인스턴스화합니다. 또한, `GetInitialState()`를 오버라이드하여 기본 진입 상태를 지정해야 합니다.
```
using System;
using UnityEngine;
using HSM;

public class Grounded : State<Character>
{
    public Idle Idle;
    public Move Move;

    public Grounded(State<Character> parent) : base(parent)
    {
        Idle = new(this);
        Move = new(this);
    }

    public override Type GetInitialState()
    {
        return typeof(Idle);
    }
}
```
- 하위 상태는 `GetTransition()`과 `On...` 메서드를 오버라이드하여 동작 및 전이 조건을 구현합니다.
```
public class Idle : State<Character>
{
    public Idle(State<Character> parent) : base(parent) { }

    public override Type GetTransition(Character c)
    {
        // 입력이 있으면 Move 상태로 전이
        if (c.InputDirection != Vector2.zero) return typeof(Move);
        return null;
    }

    public override void OnFixedUpdate(Character c, float dt)
    {
        // 입력이 없으면 캐릭터를 멈추게 함
        c.Rigidbody.AddForce(-c.Rigidbody.linearVelocity, ForceMode.VelocityChange);
    }
}
```

__3. `StateMachine` 클래스 구현__
`StateMachine<T>`를 상속받아 캐릭터 전용 상태 머신을 만듭니다. `Awake()` 메서드에서 루트 상태를 생성하고, `StateMachineBuilder`를 사용하여 전체 상태 트리를 빌드합니다.
```
using HSM;

public class CustomStateMachine : StateMachine<Character>
{
    private Root root;
    protected override void Awake()
    {
        base.Awake();
        // 최상위 상태(Root State) 인스턴스 생성
        root = new Root();
        // 빌더를 사용하여 상태 트리를 구성하고 딕셔너리에 저장
        _STATEDICT = StateMachineBuilder<Character>.Build(root, out _HEIGHT);
        // 루트 상태 타입 설정
        rootType = root.GetType();
    }
}
```

__4. 유니티 컴포넌트 연결__
마지막으로 `Character`와 `CustomStateMachine` 스크립트를 유니티 게임 오브젝트에 컴포넌트로 추가합니다.
 `CustomStateMachine`는 캐릭터의 `OnInitialEnter()`, `OnUpdate()`, `OnFixedUpdate() `메서드를 호출하여 상태를 관리합니다.

#### 기여하기
버그 보고, 기능 제안 또는 개선 사항은 언제든지 GitHub 이슈를 통해 알려주세요. 기여를 환영합니다!
