using System;
using System.Collections.Generic;
using System.Numerics;

namespace GorillaTailor
{
    internal class GTVirtualVertexLink
    {
        private GTVirtualVertex _head;
        private GTVirtualVertex _tail;
        private Dictionary<Vector3, GTVirtualFace> _faces = new Dictionary<Vector3, GTVirtualFace>();


        public GTVirtualVertexLink(GTVirtualVertex head, GTVirtualVertex tail)
        {
            this._head = head;
            this._tail = tail;
            this.Xform = Matrix4x4.Identity;
        }

        internal GTVirtualVertex Head { get => _head; private set => _head = value; }
        internal GTVirtualVertex Tail { get => _tail; private set => _tail = value; }
        internal Dictionary<Vector3, GTVirtualFace> Faces { get => _faces; private set => _faces = value; }
        public Matrix4x4 Xform { get; internal set; }
        public Vector3 Direction { get; private set; }
        public Vector3 Vertical { get; private set; }
        public Vector3 Horizontal { get; private set; }

        internal void addFace(GTVirtualFace face)
        {
            GTVirtualFace value;
            if(!this._faces.TryGetValue(face.Center, out value))
            {
                this._faces.Add(face.Center, face);
            }
        }

        internal bool isEndPoint()
        {
            return this._faces.Count == 1;
        }

        internal void calcBoneXform()
        {
            if (this.Xform != Matrix4x4.Identity)
            {
                return;
            }
            
            Vector3 vecDirection;  // ボーン進行方向(x)
            Vector3 vecVert;  // ボーン進行方向に対しての縦軸(y)
            Vector3 vecHorz;  // ボーン進行方向に対しての横軸(z)

            vecDirection = Vector3.Normalize(this.Tail.Center - this.Head.Center);  // ボーン進行方向(x)
            {
                //
                // 周辺の面法線を参考にエッジ法線を算出する感じで
                //
                Vector3 virtualNormal1 = this.Head.calcVirtualNormal();  // 関連頂点が構成する周辺の面法線から仮想頂点の法線を算出する
                Vector3 virtualNormal2 = this.Tail.calcVirtualNormal();  // 関連頂点が構成する周辺の面法線から仮想頂点の法線を算出する
                vecVert = Vector3.Normalize((virtualNormal1 + virtualNormal2) * 0.5f);  // 仮の縦軸(y)
            }
            vecHorz = Vector3.Cross(vecDirection, vecVert);  // 横軸(z)
            vecVert = Vector3.Cross(vecHorz, vecDirection);  // 縦軸(y)


            System.Diagnostics.Debug.Assert(!float.IsNaN(vecDirection.X));
            System.Diagnostics.Debug.Assert(0 < vecDirection.Length());

            Matrix4x4 matBoneXform;

            matBoneXform = Matrix4x4.Identity;
            matBoneXform.M11 = vecDirection.X;
            matBoneXform.M12 = vecDirection.Y;
            matBoneXform.M13 = vecDirection.Z;

            matBoneXform.M21 = vecVert.X;
            matBoneXform.M22 = vecVert.Y;
            matBoneXform.M23 = vecVert.Z;

            matBoneXform.M31 = vecHorz.X;
            matBoneXform.M32 = vecHorz.Y;
            matBoneXform.M33 = vecHorz.Z;

            //matBoneXform.M41 = this.Head.Position.X;
            //matBoneXform.M42 = this.Head.Position.Y;
            //matBoneXform.M43 = this.Head.Position.Z;

            System.Diagnostics.Debug.Assert(!float.IsNaN(matBoneXform.M11));

            this.Direction = vecDirection;
            this.Vertical = vecVert;
            this.Horizontal = vecHorz;
            this.Xform = matBoneXform;
            System.Diagnostics.Debug.Assert(this.Xform != new Matrix4x4());
        }
    }
}