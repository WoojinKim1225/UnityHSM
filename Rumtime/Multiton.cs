using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace HSM
{
    [InitializeOnLoad]
    public abstract class Multiton<T> : MonoBehaviour, IMultiton where T : MonoBehaviour
    {
        // 멀티스레드 환경에서 안전하게 인스턴스 딕셔너리에 접근하기 위한 잠금(lock) 객체.
        private static readonly object lockObject = new object();
        
        // 멀티턴 인스턴스들을 키(string)와 값(T) 쌍으로 저장하는 정적 딕셔너리.
        private static Dictionary<string, T> _instances;

        /// <summary>
        /// 유니티가 씬을 로드하기 전에 이 메서드를 자동으로 호출하도록 하는 속성.
        /// 딕셔너리를 초기화하는 역할을 합니다.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeMultiton()
        {
            // 한 번에 하나의 스레드만 이 코드 블록을 실행할 수 있도록 잠급니다.
            lock (lockObject)
            {
                // 인스턴스 딕셔너리를 생성합니다.
                _instances = new Dictionary<string, T>();
            }
        }

        /// <summary>
        /// 이 멀티턴 인스턴스의 고유한 키를 반환하는 추상 메서드.
        /// 이 클래스를 상속받는 하위 클래스에서 반드시 구현해야 합니다.
        /// </summary>
        public abstract string GetMultitonKey();

        /// <summary>
        /// 모든 멀티턴 인스턴스에 접근하는 정적 프로퍼티.
        /// 딕셔너리가 초기화되지 않았으면 초기화한 후 반환합니다.
        /// </summary>
        public static Dictionary<string, T> Instances
        {
            get
            {
                if (_instances == null)
                {
                    InitializeMultiton();
                }
                return _instances;
            }
        }

        /// <summary>
        /// 유니티 컴포넌트의 Awake 메서드를 오버라이드하여 인스턴스를 딕셔너리에 등록합니다.
        /// </summary>
        protected virtual void Awake()
        {
            // 이 인스턴스의 고유 키를 가져옵니다.
            string key = this.GetMultitonKey();

            // 키가 유효한지 확인합니다.
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError($"Multiton '{typeof(T).Name}'의 키가 비어 있습니다. 객체가 딕셔너리에 등록되지 않습니다.");
                return;
            }

            // 딕셔너리에 해당 키가 이미 존재하는지 확인합니다.
            if (Instances.ContainsKey(key))
            {
                Debug.LogError($"'{key}' 키를 가진 Multiton 인스턴스가 이미 존재합니다. '{gameObject.name}' 객체를 파괴합니다.");
                Destroy(gameObject); // 중복 인스턴스를 파괴하여 한 키에 하나의 인스턴스만 보장합니다.
            }
            else
            {
                // 중복이 없다면, 인스턴스를 딕셔너리에 등록합니다.
                Instances[key] = this as T;
                Debug.Log($"Multiton '{typeof(T).Name}' 인스턴스 '{key}'가 등록되었습니다.");

                foreach (var kvp in Instances)
                {
                    Debug.Log(kvp.Key + ": " + kvp.Value);
                }
            }
        }

        /// <summary>
        /// 인스턴스가 파괴될 때 딕셔너리에서 제거하여 메모리 누수를 방지합니다.
        /// </summary>
        protected virtual void OnDestroy()
        {
            string key = this.GetMultitonKey();
            // 키가 유효하고, 딕셔너리에 해당 키가 존재할 경우에만 제거합니다.
            if (!string.IsNullOrEmpty(key) && Instances.ContainsKey(key))
            {
                Instances.Remove(key);
                Debug.Log($"Multiton '{typeof(T).Name}' 인스턴스 '{key}'가 딕셔너리에서 제거되었습니다.");
            }
        }
    }

}
