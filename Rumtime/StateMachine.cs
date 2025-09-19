using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HSM
{
    /// <summary>
    /// 계층적 상태 머신을 관리하는 제네릭 클래스입니다.
    /// T는 MonoBehaviour와 ICharacter 인터페이스를 모두 구현해야 합니다.
    /// Managed.Multiton 패턴을 사용하여 클래스 당 하나의 인스턴스를 보장합니다.
    /// </summary>
    /// <typeparam name="T">상태 머신이 적용될 캐릭터 타입</typeparam>
    public class StateMachine<T> : Multiton<StateMachine<T>> where T : MonoBehaviour, ICharacter
    {
        protected System.Type rootType;
        protected Dictionary<System.Type, State<T>> _STATEDICT;
        protected int _HEIGHT;

        /// <summary>
        /// 캐릭터가 처음으로 상태 머신에 진입할 때 호출됩니다.
        /// 루트 상태부터 시작하여 초기 상태 경로를 설정하고 각 상태의 OnEnter를 호출합니다.
        /// </summary>
        /// <param name="c">상태 머신이 제어하는 캐릭터 인스턴스</param>
        public void OnInitialEnter(T c)
        {
            c.states = new System.Type[_HEIGHT];
            for ((System.Type s, int i) = (rootType, 0); s != null; (s, i) = (_STATEDICT[s].GetInitialState(), i + 1))
            {
                c.states[i] = s;
                _STATEDICT[s].OnEnter(c);
            }
        }

        /// <summary>
        /// 매 프레임 업데이트 시 호출되며, 상태 전이 조건 확인 및 현재 상태의 OnUpdate를 실행합니다.
        /// </summary>
        /// <param name="c">상태 머신이 제어하는 캐릭터 인스턴스</param>
        /// <param name="dt">Time.deltaTime</param>
        public void OnUpdate(T c, float dt)
        {
            CheckState(c);
            foreach (var s in c.states)
            {
                _STATEDICT[s].OnUpdate(c, dt);
            }
        }

        /// <summary>
        /// 물리 업데이트(FixedUpdate) 시 호출되며, 현재 상태의 OnFixedUpdate를 실행합니다.
        /// </summary>
        /// <param name="c">상태 머신이 제어하는 캐릭터 인스턴스</param>
        /// <param name="dt">Time.fixedDeltaTime</param>
        public void OnFixedUpdate(T c, float dt)
        {
            foreach (var s in c.states)
            {
                _STATEDICT[s].OnFixedUpdate(c, dt);
            }
        }

        /// <summary>
        /// 상태 전이를 확인하고 새로운 상태로 전환하는 로직을 수행합니다.
        /// </summary>
        /// <param name="c">상태 머신이 제어하는 캐릭터 인스턴스</param>
        private void CheckState(T c)
        {
            System.Type nextStateType = null;

            // 현재 상태 계층을 순회하며 전이(Transition) 조건을 확인합니다.
            for (int i = 0; i < _HEIGHT; i++)
            {
                if (c.states[i] == null) break;
                var transition = _STATEDICT[c.states[i]].GetTransition(c);
                if (transition != null)
                {
                    nextStateType = transition;
                    break;
                }
            }

            // 전이할 상태가 없으면 로직을 종료합니다.
            if (nextStateType == null) return;

            // 새로운 상태 경로를 계산합니다.
            List<System.Type> newStates = GetStatePath(nextStateType, c);

            // 이전 상태에서 나가고, 새로운 상태로 진입합니다.
            // 상태 배열의 각 요소에 대해 이전 상태와 새로운 상태를 비교합니다.
            for (int i = 0; i < _HEIGHT; i++)
            {
                if (c.states[i] == newStates[i]) continue;

                // 이전 상태가 있으면 OnExit 호출
                _STATEDICT[c.states[i]]?.OnExit(c);
                // 새로운 상태가 있으면 OnEnter 호출
                _STATEDICT[newStates[i]]?.OnEnter(c);
                // 현재 상태를 새로운 상태로 업데이트
                c.states[i] = newStates[i];
            }
        }

        /// <summary>
        /// 주어진 상태 타입부터 시작하여 전체 상태 경로를 계산합니다.
        /// </summary>
        /// <param name="startStateType">전이가 시작되는 상태 타입</param>
        /// <param name="c">캐릭터 인스턴스</param>
        /// <returns>완성된 새로운 상태 경로 리스트</returns>
        private List<System.Type> GetStatePath(System.Type startStateType, T c)
        {
            List<System.Type> path = new List<System.Type>();
            System.Type currentType = startStateType;

            // 1. 부모 상태를 찾아 리스트의 앞쪽에 삽입하여 경로를 완성합니다.
            while (currentType != null)
            {
                path.Insert(0, currentType);
                if (_STATEDICT[currentType].parent == null) break;
                currentType = _STATEDICT[currentType].parent.GetType();
            }

            // 2. 가장 하위 상태부터 초기 자식 상태를 찾아 경로에 추가합니다.
            currentType = startStateType;
            while (currentType != null)
            {
                var state = _STATEDICT[currentType];
                var newChildStateType = state.GetTransition(c);
                if (newChildStateType == null) newChildStateType = state.GetInitialState();
                if (newChildStateType == null) break;
                path.Add(newChildStateType);
                currentType = newChildStateType;
            }

            while (path.Count < _HEIGHT)
            {
                path.Add(null);
            }

            return path;
        }

        /// <summary>
        /// Managed.Multiton 패턴에 필요한 키를 반환합니다.
        /// </summary>
        /// <returns>캐릭터 타입의 이름을 키로 사용합니다.</returns>
        public override string GetMultitonKey()
        {
            return typeof(T).Name;
        }
    }
}