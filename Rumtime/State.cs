using System;

namespace HSM
{
    /// <summary>
    /// 계층적 상태 머신(Hierarchical State Machine)의 기본 상태를 정의하는 추상 클래스입니다.
    /// 모든 구체적인 상태 클래스는 이 클래스를 상속받아야 합니다.
    /// </summary>
    /// <typeparam name="T">상태를 소유하는 캐릭터 타입 (ICharacter 인터페이스를 구현해야 함)</typeparam>
    public abstract class State<T> where T : ICharacter
    {
        /// <summary>
        /// 이 상태의 부모 상태를 나타냅니다.
        /// 최상위 상태(Root State)는 부모가 null입니다.
        /// readonly 키워드를 사용하여 생성 시 한 번만 할당되도록 합니다.
        /// </summary>
        public readonly State<T> parent;

        /// <summary>
        /// 상태 클래스의 생성자입니다.
        /// </summary>
        /// <param name="parent">이 상태의 부모 상태 인스턴스</param>
        public State(State<T> parent)
        {
            this.parent = parent;
        }

        /// <summary>
        /// 이 상태의 초기 진입 상태(Initial State)를 반환합니다.
        /// 하위 상태를 가지는 상태(예: IdleState -> Idle_Walk, Idle_Stand)는 이 메서드를 오버라이드하여
        /// 초기 진입할 자식 상태를 지정해야 합니다.
        /// </summary>
        /// <returns>초기 진입할 자식 상태의 타입</returns>
        public virtual Type GetInitialState() => null;

        /// <summary>
        /// 이 상태에서 다음 상태로 전이(Transition)할 조건을 확인하고, 전이할 상태의 타입을 반환합니다.
        /// 전이 조건이 충족되지 않으면 null을 반환합니다.
        /// </summary>
        /// <param name="c">상태를 제어하는 캐릭터 인스턴스</param>
        /// <returns>다음 상태의 타입 또는 전이가 없을 경우 null</returns>
        public virtual Type GetTransition(T c) => null;

        /// <summary>
        /// 이 상태로 진입할 때 한 번 호출됩니다.
        /// 주로 초기화 로직(애니메이션 재생, 변수 설정 등)을 구현합니다.
        /// </summary>
        /// <param name="c">상태를 제어하는 캐릭터 인스턴스</param>
        public virtual void OnEnter(T c) { }

        /// <summary>
        /// 이 상태에서 나갈 때 한 번 호출됩니다.
        /// 주로 정리 로직(애니메이션 중지, 효과음 중단 등)을 구현합니다.
        /// </summary>
        /// <param name="c">상태를 제어하는 캐릭터 인스턴스</param>
        public virtual void OnExit(T c) { }

        /// <summary>
        /// 매 프레임 업데이트 시 호출됩니다.
        /// 주로 캐릭터의 실시간 동작(입력 처리, 이동 등)을 구현합니다.
        /// </summary>
        /// <param name="c">상태를 제어하는 캐릭터 인스턴스</param>
        /// <param name="dt">Time.deltaTime</param>
        public virtual void OnUpdate(T c, float dt) { }

        /// <summary>
        /// 물리 업데이트(FixedUpdate) 시 호출됩니다.
        /// 주로 물리 기반 동작(Rigidbody 조작, 충돌 감지 등)을 구현합니다.
        /// </summary>
        /// <param name="c">상태를 제어하는 캐릭터 인스턴스</param>
        /// <param name="dt">Time.fixedDeltaTime</param>
        public virtual void OnFixedUpdate(T c, float dt) { }
    }
}