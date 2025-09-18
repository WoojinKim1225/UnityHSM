using System.Collections.Generic;
using System.Reflection;

namespace HSM
{
    /// <summary>
    /// 제네릭 타입 T에 대한 상태 머신 빌더 클래스입니다.
    /// T는 ICharacter 인터페이스를 구현해야 합니다.
    /// </summary>
    /// <typeparam name="T">상태 머신이 적용될 캐릭터 타입 (ICharacter 구현)</typeparam>
    public static class StateMachineBuilder<T> where T : ICharacter
    {
        /// <summary>
        /// 루트 상태(Root State)부터 시작하여 상태 트리(State Tree)를 순회하고,
        /// 모든 상태를 키(Type)와 값(State)으로 구성된 딕셔너리에 저장합니다.
        /// 트리의 최대 깊이(Height)도 함께 계산합니다.
        /// </summary>
        /// <param name="root">트리 순회를 시작할 루트 상태</param>
        /// <param name="height">계산된 상태 트리의 최대 깊이</param>
        /// <returns>상태 타입과 상태 인스턴스를 매핑한 딕셔너리</returns>
        public static Dictionary<System.Type, State<T>> Build(State<T> root, out int height)
        {
            // Debug.Log("상태 머신 빌드를 시작합니다.");

            // 루트 상태가 null인 경우, 빈 딕셔너리를 반환하고 높이를 0으로 설정합니다.
            if (root == null)
            {
                height = 0;
                return new Dictionary<System.Type, State<T>>();
            }

            // 방문한 상태를 추적하여 무한 루프를 방지하는 HashSet.
            var visited = new HashSet<System.Type>();
            // 상태 타입과 인스턴스를 저장할 딕셔너리.
            var stateMap = new Dictionary<System.Type, State<T>>();
            // 상태 트리의 최대 높이를 추적하는 변수.
            int maxHeight = 0;

            // 깊이 우선 탐색(DFS)을 시작하여 상태 트리를 순회하고 딕셔너리를 채웁니다.
            TraverseStatesDFS(root, stateMap, visited, 1, ref maxHeight);

            // 계산된 최대 높이를 반환합니다.
            height = maxHeight;
            // 완성된 상태 맵을 반환합니다.
            return stateMap;
        }

        /// <summary>
        /// 깊이 우선 탐색(DFS)을 사용하여 상태 트리를 재귀적으로 순회하는 메서드입니다.
        /// </summary>
        /// <param name="currentState">현재 방문 중인 상태</param>
        /// <param name="stateMap">상태 타입과 인스턴스를 저장하는 딕셔너리</param>
        /// <param name="visitedStates">이미 방문한 상태 타입을 저장하는 HashSet</param>
        /// <param name="currentHeight">현재 상태의 깊이</param>
        /// <param name="maxHeight">참조로 전달되어 업데이트될 최대 깊이</param>
        private static void TraverseStatesDFS(State<T> currentState, Dictionary<System.Type, State<T>> stateMap, HashSet<System.Type> visited, int currentHeight, ref int maxHeight)
        {
            var currentType = currentState.GetType();

            // 이미 방문한 상태이면 순회를 중단하고 반환합니다.
            if (visited.Contains(currentType)) return;

            // 현재 상태를 방문한 목록에 추가하고 맵에 저장합니다.
            visited.Add(currentType);
            stateMap[currentType] = currentState;

            // 현재 높이가 최대 높이보다 크면 업데이트합니다.
            if (currentHeight > maxHeight) maxHeight = currentHeight;

            // 리플렉션(Reflection)을 사용하여 현재 상태의 모든 인스턴스 필드를 가져옵니다.
            var fields = currentType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);


            foreach (var field in fields)
            {
                // 필드 타입이 State<T>를 상속받고 "parent" 필드가 아닌 경우에만 처리합니다.
                // parent 필드를 제외하는 이유는 부모-자식 관계의 무한 루프를 방지하기 위함입니다.
                if (typeof(State<T>).IsAssignableFrom(field.FieldType) && field.Name != "parent")
                {
                    // 필드에서 자식 상태 인스턴스를 가져옵니다.
                    var childState = (State<T>)field.GetValue(currentState);

                    // 자식 상태가 null이 아니면 재귀적으로 탐색을 계속합니다.
                    if (childState != null)
                    {
                        TraverseStatesDFS(childState, stateMap, visited, currentHeight + 1, ref maxHeight);
                    }
                }
            }
        }
    }
}