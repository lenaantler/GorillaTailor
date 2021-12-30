using System;
using System.Collections.Generic;
using System.Numerics;

namespace GorillaTailor
{
    internal class GTMeshParser
    {
        private Dictionary<Vector3, GTVirtualVertex> _virtualVertexDictionaryFromPoint = new Dictionary<Vector3, GTVirtualVertex>();


        public GTMeshParser()
        {
            this.EndVirtices = new List<GTVirtualVertex>();
            this.StartRingVirtualVertices = new List<GTVirtualVertex>();
            this.GoalVirtualVirticesRing = new List<GTVirtualVertex>();
        }

        internal Dictionary<Vector3, GTVirtualVertex> VirtualVertexDictionaryFromPoint { get => _virtualVertexDictionaryFromPoint; set => _virtualVertexDictionaryFromPoint = value; }
        public List<GTVirtualVertex> EndVirtices { get; private set; }
        public List<GTVirtualVertex> StartRingVirtualVertices { get; private set; }
        public List<GTVirtualVertex> GoalVirtualVirticesRing { get; private set; }

        internal void parseMesh(PEPlugin.Pmx.IPXMaterial material)
        {
            foreach (var f in material.Faces)
            {
                List<PEPlugin.Pmx.IPXVertex> pmxVertices = new List<PEPlugin.Pmx.IPXVertex>(3);
                pmxVertices.Add(f.Vertex1);
                pmxVertices.Add(f.Vertex2);
                pmxVertices.Add(f.Vertex3);
                List<GTVertex> vertices = _convertVertices(pmxVertices);

                this._buildVirtualVirtices(vertices);
            }

            System.Diagnostics.Debug.WriteLine("仮想頂点数:" + this._virtualVertexDictionaryFromPoint.Count.ToString());

            this._makeVirtualVertexLinks();

            this._makeVirtualFaces();

            this._collectEndPointVirtices();

            this._collectStartAndGoalRingVirtices();
        }

        private void _collectStartAndGoalRingVirtices()
        {
            //
            // 起点仮想頂点群／終点仮想頂点群を収集する
            //
            Vector3 globalVector = new Vector3(0, -1, 0);  // スカートなので上から下へ
            var pair = this._findStartAndGoal(globalVector);
            var startVertexSample = pair.Item1;  // 起点スキャン開始頂点
            var goalVertexSample = pair.Item2;  // 終点スキャン開始頂点

            // スカート形状が前提条件なので起点リングと終点リングがあるはず。それらを構築する
            this.StartRingVirtualVertices = this._buildEndPointRing(startVertexSample);
            foreach (var v in this.StartRingVirtualVertices) v.IsStartPoint = true;
            System.Diagnostics.Debug.WriteLine("起点リング頂点数:" + this.StartRingVirtualVertices.Count.ToString());

            this.GoalVirtualVirticesRing = this._buildEndPointRing(goalVertexSample);
            foreach (var v in this.GoalVirtualVirticesRing) v.IsGoalPoint = true;
            System.Diagnostics.Debug.WriteLine("終点リング頂点数:" + this.GoalVirtualVirticesRing.Count.ToString());
        }

        /// <summary>
        /// ポリ端からポリ端の頂点をたどっていくロジック。なのでポリ端が交差するような形状があるとバグる
        /// </summary>
        /// <param name="baseVirtex"></param>
        /// <returns></returns>
        private List<GTVirtualVertex> _buildEndPointRing(GTVirtualVertex baseVirtex)
        {
            List<GTVirtualVertex> ringVirtices = new List<GTVirtualVertex>();


            ringVirtices.Add(baseVirtex);  // スキャン開始頂点を追加

            GTVirtualVertex prev = null;
            GTVirtualVertex next = baseVirtex;


            while (next != null)
            {
                var linkedVertices = new List<GTVirtualVertex>(next.LinkedVirtualVirtices);  // リストのコピーを作成
                linkedVertices.Remove(prev);  // 前の頂点は除外する
                linkedVertices.RemoveAll(delegate (GTVirtualVertex v)
                {
                    // 既にスキャン済み、あるいはポリ端ではないなら除外する
                    if (!v.isEndPoint()) return true;
                    if (ringVirtices.Contains(v)) return true;
                    return false;
                });


                // 進むべきエッジが見つからなくなったら終端と判断して終了
                if (linkedVertices.Count == 0)
                {
                    break;
                }


                ringVirtices.Add(linkedVertices[0]);  // 頂点を追加


                // 次の頂点を探索する
                prev = next;
                next = linkedVertices[0];
            }

            return ringVirtices;
        }

        /// <summary>
        /// 筒状のスカート前提のロジックで起点リングと終点リングに含まれる頂点を探し出す
        /// </summary>
        /// <param name="globalVector"></param>
        /// <returns></returns>
        private Tuple<GTVirtualVertex, GTVirtualVertex> _findStartAndGoal(Vector3 globalVector)
        {
            GTVirtualVertex startPoint = null;  // 起点が1個わかればよい
            GTVirtualVertex goalPoint = null;

            Vector3 startPos = new Vector3(float.NaN, float.NaN, float.NaN);
            Vector3 goalPos = new Vector3(float.NaN, float.NaN, float.NaN);


            foreach (var vertex in this.EndVirtices)
            {
                var pos = vertex.Position * globalVector;

                if (float.IsNaN(startPos.X))
                {
                    startPoint = vertex;
                    startPos = vertex.Position;
                }
                if (float.IsNaN(goalPos.X))
                {
                    goalPoint = vertex;
                    goalPos = vertex.Position;
                }


                // グローバルベクトルは1軸のみという前提条件でロジックを組む。

                // より小さい値（グローバルベクトルの根元に近いほう）が起点。同値は先勝ち
                if (pos.X < startPos.X)
                {
                    startPos = pos;
                    startPoint = vertex;
                }
                else if (pos.Y < startPos.Y)
                {
                    startPos = pos;
                    startPoint = vertex;
                }
                else if (pos.Z < startPos.Z)
                {
                    startPos = pos;
                    startPoint = vertex;
                }

                // より大きい値（グローバルベクトルの先に近いほう）が終点。同値は先勝ち
                if (goalPos.X < pos.X)
                {
                    goalPos = pos;
                    goalPoint = vertex;
                }
                if (goalPos.Y < pos.Y)
                {
                    goalPos = pos;
                    goalPoint = vertex;
                }
                if (goalPos.Z < pos.Z)
                {
                    goalPos = pos;
                    goalPoint = vertex;
                }
            }

            if (startPoint == null || goalPoint == null) throw new Exception("知らんわ");

            return new Tuple<GTVirtualVertex, GTVirtualVertex>(startPoint, goalPoint);
        }

        private void _collectEndPointVirtices()
        {
            //
            // 終端仮想頂点を収集する
            //
            foreach (var pair in this._virtualVertexDictionaryFromPoint)
            {
                var virtualVertex = pair.Value;
                if (virtualVertex.isEndPoint()) this.EndVirtices.Add(virtualVertex);
            }
        }

        private void _makeVirtualFaces()
        {
            //
            // 仮想頂点リンクを張り終えたので仮想面を作成してリンクに隣接する仮想面を数える
            //
            foreach (var pair in this._virtualVertexDictionaryFromPoint)
            {
                var virtualVertex = pair.Value;
                virtualVertex.makeVirtualFaces();
            }
        }

        private void _makeVirtualVertexLinks()
        {
            //
            // 実頂点のリンクを張り終えたので仮想頂点に反映する
            //
            foreach (var pair in this._virtualVertexDictionaryFromPoint)
            {
                var virtualVertex = pair.Value;
                virtualVertex.makeLinks();
            }
        }

        private void _buildVirtualVirtices(List<GTVertex> vertices)
        {
            //
            // 重複頂点・近傍頂点をまとめるための仮想頂点を作成する
            //
            foreach (var v in vertices)
            {
                Vector3 positionKey = this._roundPosition(v.Position);  // カンマ一桁に丸めた座標をキーにする


                GTVirtualVertex virtualVertex;
                if (!this._virtualVertexDictionaryFromPoint.TryGetValue(positionKey, out virtualVertex))
                {
                    virtualVertex = new GTVirtualVertex(positionKey);
                    this._virtualVertexDictionaryFromPoint.Add(positionKey, virtualVertex);
                }
                v.VirtualVertex = virtualVertex;
                virtualVertex.RealVertices.Add(v);
            }
        }

        private static List<GTVertex> _convertVertices(List<PEPlugin.Pmx.IPXVertex> pmxVertices)
        {
            List<GTVertex> vertices = new List<GTVertex>(3);

            foreach (var v in pmxVertices)
            {
                //
                // メッシュから取得が必要なのは座標と法線と面構成情報のみ
                //
                Vector3 pos = Utility.fromSdx(v.Position);
                Vector3 normal = Utility.fromSdx(v.Normal);

                GTVertex vertex = new GTVertex(pos, normal);
                vertices.Add(vertex);
            }


            // 面構成情報を構築
            GTFace face = new GTFace(vertices);

            // 面構成情報からリンクを生成
            foreach (var v in vertices)
            {
                v.linkFace(face);  // 内部で自己参照をはじいてもらう
            }

            return vertices;
        }

        /// <summary>
        /// カンマ二桁で四捨五入した座標を返す
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        private Vector3 _roundPosition(Vector3 position)
        {
            float x = (float)Math.Ceiling((position.X + 0.05) * 10) * 0.1f;
            float y = (float)Math.Ceiling((position.Y + 0.05) * 10) * 0.1f;
            float z = (float)Math.Ceiling((position.Z + 0.05) * 10) * 0.1f;

            return new Vector3(x, y, z);
        }
    }
}