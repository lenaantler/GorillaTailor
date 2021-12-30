using System.Collections.Generic;

namespace GorillaTailor
{
    /// <summary>
    /// んー。いらんかも
    /// </summary>
    internal class GTFace
    {
        private List<GTVertex> _vertices;

        public GTFace(List<GTVertex> vertices)
        {
            this._vertices = vertices;
        }

        internal List<GTVertex> Vertices { get => _vertices; private set => _vertices = value; }
    }
}