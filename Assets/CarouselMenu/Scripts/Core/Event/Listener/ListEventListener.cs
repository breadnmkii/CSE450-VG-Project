using System.Collections.Generic;

namespace MS.Core
{
    public class ListEventListener : GenericEventListener<ListEvent, List<object>>
    {
        public override void OnEventRaised(List<object> payload)
        {
            base.OnEventRaised(payload);
        }
    }
}