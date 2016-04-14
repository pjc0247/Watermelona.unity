using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Watermelona
{
    public class ChangeStateOnEvent : SubscriberBase
    {
        [SerializeField]
        public string state;

        protected override void Fire(object b)
        {
            GetComponent<Animator>()
                .Play(state);
        }
    }
}
