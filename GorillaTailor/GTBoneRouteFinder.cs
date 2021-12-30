using System;
using System.Collections.Generic;
using System.Numerics;

namespace GorillaTailor
{
    internal class GTBoneRouteFinder
    {
        public GTBoneRouteFinder()
        {
        }

        /// <summary>
        /// 全てのボーンルートを導き出す
        /// </summary>
        /// <param name="meshParser"></param>
        /// <returns></returns>
        internal List<GTBoneRoute> searchAllRoute(GTMeshParser meshParser)
        {
            List<GTBoneRoute> boneRouteList = new List<GTBoneRoute>();
            Vector3 rootCenter = this._calcRootCenter(meshParser.StartRingVirtualVertices);

            foreach (var startVertex in meshParser.StartRingVirtualVertices)
            {
                List<GTBoneRoute> tempBoneRouteList = new List<GTBoneRoute>();
                List<GTVirtualVertex> doneList = new List<GTVirtualVertex>();

                foreach (var link in startVertex.Links)
                {
                    if (link.Tail.IsStartPoint) continue;  // 起点仮想頂点は除外する
                    if (doneList.Contains(link.Tail)) continue;  // 探索済み仮想頂点は除外する

                    GTBoneRoute boneRoute = this._searchBoneRoute(rootCenter, link);
                    if (boneRoute.Bad) continue;  // 袋小路は除外
                    tempBoneRouteList.Add(boneRoute);
                }


                //
                // tempBoneRouteListの中で一番成績が良いものを選択したいんだけど評価値どうするか未定
                //
                foreach (var route in tempBoneRouteList)
                {
                    boneRouteList.Add(route);
                }
            }

            return boneRouteList;
        }

        /// <summary>
        /// 起点リングの重心を算出する
        /// </summary>
        /// <param name="startVertices"></param>
        private Vector3 _calcRootCenter(List<GTVirtualVertex> startVertices)
        {
            Vector3 rootCenter = Vector3.Zero;
            foreach (var startVertex in startVertices)
            {
                rootCenter += startVertex.Position;
            }
            rootCenter /= startVertices.Count;

            return rootCenter;
        }

        /// <summary>
        /// 与えられたリンクから先のボーンルートを導き出す
        /// </summary>
        /// <param name="targetLink"></param>
        /// <returns></returns>
        private GTBoneRoute _searchBoneRoute(Vector3 rootCenter, GTVirtualVertexLink targetLink)
        {
            GTBoneRoute boneRoute = new GTBoneRoute();
            List<GTVirtualVertex> doneList = new List<GTVirtualVertex>();


            GTVirtualVertexLink next = targetLink;
            next.calcBoneXform();

            boneRoute.Links.Add(next);
            doneList.Add(next.Head);
            doneList.Add(next.Tail);


            while (next != null)
            {
                List<Tuple<GTVirtualVertexLink, float>> localErrors = new List<Tuple<GTVirtualVertexLink, float>>();

                //System.Diagnostics.Debug.WriteLine("起点:" + next.Tail.Center.ToString());

                foreach (var link in next.Tail.Links)
                {
                    if (link.Tail.IsStartPoint) continue;
                    if (doneList.Contains(link.Tail)) continue;

                    link.calcBoneXform();


                    //
                    // ローカル空間で「直進してる方」を選ぶやり方を考える
                    //
                    // ボーンは+Xに伸びているものとして定義しているので親座標系における親ボーンベクトルは(1, 0, 0)である
                    //
                    Matrix4x4 matRotation = next.Xform;
                    matRotation.M41 = matRotation.M42 = matRotation.M43 = 0;
                    Matrix4x4 matToLocal;
                    Matrix4x4.Invert(matRotation, out matToLocal);
                    Matrix4x4 localXform = link.Xform * matToLocal;
                    //Vector3 localNextDirection = (Matrix4x4.CreateTranslation(next.Direction) * matToLocal).Translation;
                    Vector3 localNextDirection = new Vector3(1, 0, 0);  // 誤差が乗るとそれやすいので固定値
                    Vector3 localLinkDirection = (Matrix4x4.CreateTranslation(link.Direction) * matToLocal).Translation;

                    float error = (float)Math.PI;  // 適当に0から遠い値

                    {
                        //System.Diagnostics.Debug.WriteLine("  リンク先頂点:" + link.Tail.Center.ToString());
                        //System.Diagnostics.Debug.WriteLine("    ベクトルA:" + localNextDirection.ToString());
                        //System.Diagnostics.Debug.WriteLine("    ベクトルB:" + localLinkDirection.ToString());

                        {
                            Vector3 vecYaw1 = Vector3.Normalize(localNextDirection * new Vector3(1, 0, 1));
                            Vector3 vecYaw2 = Vector3.Normalize(localLinkDirection * new Vector3(1, 0, 1));
                            var yaw = Utility.calcAngleError(vecYaw1, vecYaw2);
                            if (float.IsNaN(yaw)) yaw = 0.0f;

                            Vector3 vecPitch1 = Vector3.Normalize(localNextDirection * new Vector3(0, 1, 1));
                            Vector3 vecPitch2 = Vector3.Normalize(localLinkDirection * new Vector3(0, 1, 1));
                            var pitch = Utility.calcAngleError(vecPitch1, vecPitch2);
                            if (float.IsNaN(pitch)) pitch = 0.0f;

                            Vector3 vecRoll1 = Vector3.Normalize(localNextDirection * new Vector3(1, 1, 0));
                            Vector3 vecRoll2 = Vector3.Normalize(localLinkDirection * new Vector3(1, 1, 0));
                            var roll = Utility.calcAngleError(vecRoll1, vecRoll2);
                            if (float.IsNaN(roll)) roll = 0.0f;

                            // roll(z)の比重を軽めに、他を重めかつ同等にする感じの評価値にしてみるか
                            error = (yaw * 10) + (pitch * 10) + roll;

                            //System.Diagnostics.Debug.WriteLine("    yaw:" + yaw.ToString());
                            //System.Diagnostics.Debug.WriteLine("    pitch:" + pitch.ToString());
                            //System.Diagnostics.Debug.WriteLine("    roll:" + roll.ToString());
                            //System.Diagnostics.Debug.WriteLine("    評価値:" + error.ToString());
                        }
                    }


                    //    //System.Diagnostics.Debug.WriteLine("  リンク先頂点:" + link.Tail.Position.ToString());
                    //    //System.Diagnostics.Debug.WriteLine("    ベクトルA:" + localNextDirection.ToString());
                    //    //System.Diagnostics.Debug.WriteLine("    ベクトルB:" + localLinkDirection.ToString());
                    //    //System.Diagnostics.Debug.WriteLine("    回転:" + q.ToString());
                    //    //System.Diagnostics.Debug.WriteLine("    誤差:" + error.ToString());
                    //}


                    //
                    // 誤差とともにリンクを格納
                    //
                    localErrors.Add(new Tuple<GTVirtualVertexLink, float>(link, error));
                }

                localErrors.Sort(delegate (Tuple<GTVirtualVertexLink, float> lhs, Tuple<GTVirtualVertexLink, float> rhs)
                {
                    if (lhs.Item2 < rhs.Item2) return -1;
                    if (rhs.Item2 < lhs.Item2) return 1;
                    return 0;
                });


                //
                // 袋小路到着
                //
                if (localErrors.Count == 0)
                {
                    boneRoute.Bad = true;
                    break;
                }

                var prev = next;
                next = localErrors[0].Item1;

                //{
                //    System.Diagnostics.Debug.WriteLine("  選択頂点:" + next.Tail.Center.ToString());
                //    System.Diagnostics.Debug.WriteLine("    誤差:" + localErrors[0].Item2.ToString());
                //}
                next.calcBoneXform();

                boneRoute.Links.Add(next);
                doneList.Add(next.Tail);

                //
                // 終点到着
                //
                if (next.Tail.IsGoalPoint)
                {
                    break;
                }

            }

            return boneRoute;
        }
    }
}