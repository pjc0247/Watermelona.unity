using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using UnityEngine;

using Network;

namespace Watermelona
{
    public class ActivateOnEvent : SubscriberBase
    {
        public enum ActivationType
        {
            Activate,
            Deactivate
        }

        public ActivationType activationType;

        protected override bool keepSubscriptionOnDisable
        {
            get
            {
                return true;
            }
        }

        protected override void Fire(object b)
        {
            if(activationType == ActivationType.Activate)
                gameObject.SetActive(true);
            else
                gameObject.SetActive(false);
        }
    }
}
