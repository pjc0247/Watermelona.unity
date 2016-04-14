using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using UnityEngine;

using Network;

namespace Watermelona
{
    public class SubscriberBase : MonoBehaviour
    {
        [HideInInspector]
        [SerializeField]
        public string eventTypeName;

        /// <summary>
        /// deactive 상태에서도 수신을 계속할것인지를 선택한다.
        /// 기본값은 false 이며, 이 프로퍼티를 오버라이드하여 변경할 수 있다.
        /// </summary>
        protected virtual bool keepSubscriptionOnDisable
        {
            get
            {
                return false;
            }
        }

        private object callback { get; set; }
        private Type eventType
        {
            get
            {
                return GetType().Assembly
                    .GetTypes()
                    .Where(x => x.Name == eventTypeName)
                    .FirstOrDefault();
            }
        }

        void OnEnable()
        {
            if (callback != null)
                return;
            if (eventTypeName == null)
                return;

            callback = Delegate.CreateDelegate(
                typeof(Action<>).MakeGenericType(eventType),
                this,
                GetType().GetMethod("Fire", BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance));

            typeof(PubSub).GetMethod("Subscribe")
                .MakeGenericMethod(eventType)
                .Invoke(null, new object[] { eventTypeName, callback });
        }
        void OnDisable()
        {
            if (keepSubscriptionOnDisable)
                return;
            if (eventTypeName == null)
                return;

            typeof(PubSub).GetMethod("Unsubscribe")
                .Invoke(null, new object[] { eventTypeName, callback });
            callback = null;
        }

        /// <summary>
        /// 이 메소드를 오버라이드하여 이벤트가 수신되었을 때
        /// 취할 액션을 작성한다.
        /// </summary>
        /// <param name="b">사용되지 않음</param>
        protected virtual void Fire(object b)
        {
        }
    }
}
