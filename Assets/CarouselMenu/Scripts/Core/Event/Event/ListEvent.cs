using System.Collections.Generic;
using UnityEngine;
//using Sirenix.OdinInspector;

namespace MS.Core
{
    [CreateAssetMenu(menuName = "Carousel/Events/Event<List<object>>")]
    public class ListEvent : GenericEvent<List<object>>
    {
        //[DisableInEditorMode]
        public List<object> Payload;

        //[Button, DisableInEditorMode]
        public void Raise()
        {
            Raise(Payload);
        }
    }
}