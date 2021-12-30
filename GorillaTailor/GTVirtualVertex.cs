using System;
using System.Collections.Generic;
using System.Numerics;

namespace GorillaTailor
{
    internal class GTVirtualVertex : IComparable<GTVirtualVertex>
    {
        public GTVirtualVertex(System.Numerics.Vector3 position)
        {
            this.Position = position;
            this.RealVertices = new List<GTVertex>();
            this.Links = new List<GTVirtualVertexLink>();
            //this.Faces = new Dictionary<Vector3, GTVirtualFace>();
            this.LinkedVirtualVirtices = new List<GTVirtualVertex>();
        }

        public Vector3 Position { get; }
        public Vector3 Center { get; private set; }
        public List<GTVertex> RealVertices { get; }
        public List<GTVirtualVertexLink> Links { get; private set; }
        public List<GTVirtualVertex> LinkedVirtualVirtices { get; private set; }  // いちいちリンクのリストのテイルを列挙するのだるいし
        public bool IsStartPoint { get; internal set; }
        public bool IsGoalPoint { get; internal set; }

        //public Dictionary<Vector3, GTVirtualFace> Faces { get; private set; }

        /// <summary>
        /// 決まったルールで並び替えできればそれでよい
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(GTVirtualVertex other)
        {
            if (this.Position.X < other.Position.X) return -1;
            if (other.Position.X < this.Position.X) return 1;
            if (this.Position.Y < other.Position.Y) return -1;
            if (other.Position.Y < this.Position.Y) return 1;
            if (this.Position.Z < other.Position.Z) return -1;
            if (other.Position.Z < this.Position.Z) return 1;

            return 0;
        }

        internal bool isEndPoint()
        {
            foreach(var link in this.Links)
            {
                if (link.isEndPoint()) return true;
            }
            return false;
        }

        internal void makeLinks()
        {
            Vector3 center = Vector3.Zero;

            foreach (var v in this.RealVertices)
            {
                center += v.Position;

                foreach (var link in v.Links)
                {
                    var tail = link.Tail;
                    var virtualTail = tail.VirtualVertex;
                    //var faces = link.Faces;


                    if (virtualTail == this) continue;
                    if (this.LinkedVirtualVirtices.Contains(virtualTail)) continue;


                    GTVirtualVertexLink virtualLink = new GTVirtualVertexLink(this, virtualTail);
                    this.Links.Add(virtualLink);
                    this.LinkedVirtualVirtices.Add(virtualTail);
                }
            }

            center /= this.RealVertices.Count;
            this.Center = center;
        }

        /// <summary>
        /// 重複実面登録をはじいてないはずなのでちょっと雑な法線になるけどとりあえず。
        /// </summary>
        /// <returns></returns>
        internal Vector3 calcVirtualNormal()
        {
            Vector3 virtualNormal = Vector3.Zero;

            foreach( var v in this.RealVertices)
            {
                Vector3 fNormal = Vector3.Zero;

                foreach(var f in v.Faces)
                {
                    Vector3 vNormal = Vector3.Zero;
                    vNormal += f.Vertices[0].Normal;
                    vNormal += f.Vertices[1].Normal;
                    vNormal += f.Vertices[2].Normal;
                    vNormal /= 3;

                    fNormal += vNormal;
                }

                fNormal /= v.Faces.Count;
                virtualNormal += fNormal;
            }
            virtualNormal /= this.RealVertices.Count;

            virtualNormal = Vector3.Normalize(virtualNormal);
            return virtualNormal;
        }

        /// <summary>
        /// 仮想頂点がなす面を構成する
        /// </summary>
        internal void makeVirtualFaces()
        {
            foreach (var link in this.Links)
            {
                List<GTVirtualFace> faces;
                faces = link.Tail.makeVirtualFaces(this);
                foreach (var face in faces)
                {
                    link.addFace(face);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseVertex"></param>
        /// <returns></returns>
        private List<GTVirtualFace> makeVirtualFaces(GTVirtualVertex baseVertex)
        {
            List<GTVirtualFace> faces = new List<GTVirtualFace>();


            foreach (var link in this.Links)
            {
                if (link.Tail == baseVertex) continue;

                foreach (var link2 in link.Tail.Links)
                {
                    if (link2.Tail == baseVertex)
                    {
                        // 面を構成している
                        List<GTVirtualVertex> vertices = new List<GTVirtualVertex>(3);
                        vertices.Add(baseVertex);
                        vertices.Add(link.Tail);
                        vertices.Add(link2.Tail);
                        vertices.Sort();

                        GTVirtualFace face = new GTVirtualFace(vertices);
                        faces.Add(face);
                    }
                }
            }

            return faces;
        }
    }
}