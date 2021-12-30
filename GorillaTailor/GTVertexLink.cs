using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GorillaTailor
{
    /// <summary>
    /// 頂点同士のつながりを現したクラス
    /// んー。いらんかも。
    /// </summary>
    class GTVertexLink
    {
        private GTVertex _head;
        private GTVertex _tail;
        private List<GTFace> _faces = new List<GTFace>();


        public GTVertexLink(GTVertex gTVertex1, GTVertex gTVertex2, GTFace face)
        {
            this._head = gTVertex1;
            this._tail = gTVertex2;
            this._faces.Add(face);
        }

        internal GTVertex Head { get => _head; private set => _head = value; }
        internal GTVertex Tail { get => _tail; set => _tail = value; }
        internal List<GTFace> Faces { get => _faces; set => _faces = value; }
    }
}
