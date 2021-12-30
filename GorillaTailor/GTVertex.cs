using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GorillaTailor
{
    /// <summary>
    /// 頂点属性をまとめたクラス
    /// </summary>
    class GTVertex
    {
        public GTVertex(Vector3 pos, Vector3 normal)
        {
            this.Position = pos;
            this.Normal = normal;
            this.Faces = new HashSet<GTFace>();
            this.Links = new List<GTVertexLink>();
            this.LinkVertices = new List<GTVertex>();
        }

        public Vector3 Position { get; private set; }
        public Vector3 Normal { get; private set; }
        public HashSet<GTFace> Faces { get; private set; }
        public List<GTVertexLink> Links { get; private set; }
        public List<GTVertex> LinkVertices { get; private set; }
        public GTVirtualVertex VirtualVertex { get; internal set; }

        internal void linkFace(GTFace face)
        {
            this.Faces.Add(face);


            if (face.Vertices[0] == this)
            {
                if (!this.LinkVertices.Contains(face.Vertices[1]))
                {
                    this.Links.Add(new GTVertexLink(this, face.Vertices[1], face));
                    face.Vertices[1].Links.Add(new GTVertexLink(face.Vertices[1], this, face));
                    this.LinkVertices.Add(face.Vertices[1]);
                }

                if (!this.LinkVertices.Contains(face.Vertices[2]))
                {
                    this.Links.Add(new GTVertexLink(this, face.Vertices[2], face));
                    face.Vertices[2].Links.Add(new GTVertexLink(face.Vertices[2], this, face));
                    this.LinkVertices.Add(face.Vertices[2]);
                }
            }

            if (face.Vertices[1] == this)
            {
                if (!this.LinkVertices.Contains(face.Vertices[1]))
                {
                    this.Links.Add(new GTVertexLink(this, face.Vertices[0], face));
                    face.Vertices[1].Links.Add(new GTVertexLink(face.Vertices[0], this, face));
                    this.LinkVertices.Add(face.Vertices[0]);
                }

                if (!this.LinkVertices.Contains(face.Vertices[1]))
                {
                    this.Links.Add(new GTVertexLink(this, face.Vertices[2], face));
                    face.Vertices[2].Links.Add(new GTVertexLink(face.Vertices[2], this, face));
                    this.LinkVertices.Add(face.Vertices[2]);
                }
            }

            if (face.Vertices[2] == this)
            {
                if (!this.LinkVertices.Contains(face.Vertices[0]))
                {
                    this.Links.Add(new GTVertexLink(this, face.Vertices[0], face));
                    face.Vertices[1].Links.Add(new GTVertexLink(face.Vertices[0], this, face));
                    this.LinkVertices.Add(face.Vertices[0]);
                }

                if (!this.LinkVertices.Contains(face.Vertices[1]))
                {
                    this.Links.Add(new GTVertexLink(this, face.Vertices[1], face));
                    face.Vertices[2].Links.Add(new GTVertexLink(face.Vertices[1], this, face));
                    this.LinkVertices.Add(face.Vertices[1]);
                }
            }
        }
    }
}
