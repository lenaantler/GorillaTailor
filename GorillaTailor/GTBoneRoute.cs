using System.Collections.Generic;

namespace GorillaTailor
{
    internal class GTBoneRoute
    {
        public GTBoneRoute()
        {
            this.Good = false;
            this.Bad = false;
            this.Links = new List<GTVirtualVertexLink>();
        }

        public List<GTVirtualVertexLink> Links { get; internal set; }
        public bool Good { get; private set; }
        public bool Bad { get; internal set; }
    }
}