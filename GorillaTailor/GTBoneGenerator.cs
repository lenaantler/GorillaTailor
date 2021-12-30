using PEPlugin;
using PEPlugin.Pmx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GorillaTailor
{
    /// <summary>
    /// メッシュを解析して適切なボーンを生成するクラス、だといいね！
    /// </summary>
    class GTBoneGenerator
    {
        /// <summary>
        /// メッシュを解析してボーンを生成する
        /// </summary>
        /// <param name="host"></param>
        /// <param name="model"></param>
        internal void generateBones(IPEPluginHost host, IPXPmx model)
        {
            // 面倒なので全材質対象
            foreach (var material in model.Material)
            {
                GTMeshParser meshParser = new GTMeshParser();
                meshParser.parseMesh(material);


                GTBoneRouteFinder boneRouteFinder = new GTBoneRouteFinder();
                List<GTBoneRoute> boneRouteList = boneRouteFinder.searchAllRoute(meshParser);

                //this._testAllWires(host, model, meshParser);
                //this._testEndPointWires(host, model, meshParser);
                //this._testStartRingWires(host, model, meshParser);
                this._testBoneRouteWires(host, model, boneRouteList);
            }
        }

        /// <summary>
        /// ボーンルートにボーンを張って確認したいだけ
        /// </summary>
        /// <param name="host"></param>
        /// <param name="model"></param>
        /// <param name="boneRouteList"></param>
        private void _testBoneRouteWires(IPEPluginHost host, IPXPmx model, List<GTBoneRoute> boneRouteList)
        {
            foreach(var boneRoute in boneRouteList)
            {
                foreach(var link in boneRoute.Links)
                {
                    {
                        var head = link.Head;
                        var tail = link.Tail;
                        var headPos = head.Center;
                        var tailPos = tail.Center;


                        // ボーンを生成する
                        var bone = host.Builder.Pmx.Bone();
                        bone.Name = "virtualBone";
                        bone.Parent = model.Bone[0];  // 適当に0番を親にしてみる
                        bone.Position = Utility.toSdx(headPos);
                        bone.ToOffset = Utility.toSdx(tailPos - headPos);  // 相対位置指定
                        model.Bone.Add(bone);
                    }

                    //// ちょっとY-Upみせて
                    //{
                    //    var head = link.Head;
                    //    var headPos = head.Center;
                    //    var tailPos = head.Center + link.Vertical;


                    //    // ボーンを生成する
                    //    var bone = host.Builder.Pmx.Bone();
                    //    bone.Name = "virtualBone";
                    //    bone.Parent = model.Bone[0];  // 適当に0番を親にしてみる
                    //    bone.Position = Utility.toSdx(headPos);
                    //    bone.ToOffset = Utility.toSdx(tailPos - headPos);  // 相対位置指定
                    //    model.Bone.Add(bone);
                    //}

                    //// ちょっと変換行列確認
                    //{
                    //    var head = link.Head;
                    //    var headPos = head.Center;
                    //    var tailPos = head.Center + (Matrix4x4.CreateTranslation(1, 0, 0) * link.Xform).Translation;


                    //    // ボーンを生成する
                    //    var bone = host.Builder.Pmx.Bone();
                    //    bone.Name = "virtualBone";
                    //    bone.Parent = model.Bone[0];  // 適当に0番を親にしてみる
                    //    bone.Position = Utility.toSdx(headPos);
                    //    bone.ToOffset = Utility.toSdx(tailPos - headPos);  // 相対位置指定
                    //    model.Bone.Add(bone);
                    //}
                }
            }
        }

        /// <summary>
        /// 起点リングにボーンを張って確認したいだけ
        /// </summary>
        /// <param name="host"></param>
        /// <param name="model"></param>
        /// <param name="meshParser"></param>
        private void _testStartRingWires(IPEPluginHost host, IPXPmx model, GTMeshParser meshParser)
        {
            var vertices = meshParser.StartRingVirtualVertices;
            for(int n = 0; n < (vertices.Count - 1); ++n)
            {
                var head = vertices[n];
                var tail = vertices[n + 1];
                var headPos = head.Center;
                var tailPos = tail.Center;


                // ボーンを生成する
                var bone = host.Builder.Pmx.Bone();
                bone.Name = "virtualBone";
                bone.Parent = model.Bone[0];  // 適当に0番を親にしてみる
                bone.Position = Utility.toSdx(headPos);
                bone.ToOffset = Utility.toSdx(tailPos - headPos);  // 相対位置指定
                model.Bone.Add(bone);
            }

            {
                var head = vertices[vertices.Count - 1];
                var tail = vertices[0];
                var headPos = head.Center;
                var tailPos = tail.Center;


                // ボーンを生成する
                var bone = host.Builder.Pmx.Bone();
                bone.Name = "virtualBone";
                bone.Parent = model.Bone[0];  // 適当に0番を親にしてみる
                bone.Position = Utility.toSdx(headPos);
                bone.ToOffset = Utility.toSdx(tailPos - headPos);  // 相対位置指定
                model.Bone.Add(bone);

            }
        }

        /// <summary>
        /// ポリ端のエッジにボーンを張って状態を確認したいだけ
        /// </summary>
        /// <param name="host"></param>
        /// <param name="model"></param>
        /// <param name="meshParser"></param>
        private void _testEndPointWires(IPEPluginHost host, IPXPmx model, GTMeshParser meshParser)
        {
            foreach (var keyValuePair in meshParser.VirtualVertexDictionaryFromPoint)
            {
                var virtualVertex = keyValuePair.Value;

                if (!virtualVertex.isEndPoint()) continue;

                foreach (var link in virtualVertex.Links)
                {
                    if (!link.isEndPoint()) continue;

                    var headPos = virtualVertex.Center;
                    var tailPos = link.Tail.Center;

                    // ボーンを生成する
                    var bone = host.Builder.Pmx.Bone();
                    bone.Name = "virtualBone";
                    bone.Parent = model.Bone[0];  // 適当に0番を親にしてみる
                    bone.Position = Utility.toSdx(headPos);
                    bone.ToOffset = Utility.toSdx(tailPos - headPos);  // 相対位置指定
                    model.Bone.Add(bone);
                }
            }
        }

        /// <summary>
        /// メッシュを解析した仮想頂点リンクにボーンを張って状態を確認したいだけ
        /// </summary>
        /// <param name="host"></param>
        /// <param name="model"></param>
        /// <param name="meshParser"></param>
        private void _testAllWires(IPEPluginHost host, IPXPmx model, GTMeshParser meshParser)
        {
            foreach (var keyValuePair in meshParser.VirtualVertexDictionaryFromPoint)
            {
                var virtualVertex = keyValuePair.Value;

                foreach (var link in virtualVertex.Links)
                {
                    var headPos = virtualVertex.Center;
                    var tailPos = link.Tail.Center;

                    // ボーンを生成する
                    var bone = host.Builder.Pmx.Bone();
                    bone.Name = "virtualBone";
                    bone.Parent = model.Bone[0];  // 適当に0番を親にしてみる
                    bone.Position = Utility.toSdx(headPos);
                    bone.ToOffset = Utility.toSdx(tailPos - headPos);  // 相対位置指定
                    model.Bone.Add(bone);
                }
            }
        }
    }
}
