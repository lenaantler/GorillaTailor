using System.Collections.Generic;
using System.Numerics;

namespace GorillaTailor
{
    internal class GTVirtualFace
    {
        private List<GTVirtualVertex> _vertices;

        public GTVirtualFace(List<GTVirtualVertex> vertices)
        {
            System.Diagnostics.Debug.Assert(vertices.Count == 3);

            this._vertices = vertices;

            Vector3 center = Vector3.Zero;
            foreach(var v in vertices)
            {
                center += v.Position;
            }
            center /= vertices.Count;
            this.Center = center;
        }

        public Vector3 Center { get; private set; }
    }
}