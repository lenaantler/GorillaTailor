using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GorillaTailor
{
    class Utility
    {
        /// <summary>
        /// ベクトル間の内積を取って正規化した誤差を返す
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        internal static float calcError(Vector3 v1, Vector3 v2)
        {
            return (float)(Math.Acos(Vector3.Dot(v1, v2)) / Math.PI);
        }

        /// <summary>
        /// ベクトルの姿勢を計算する
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        internal static Matrix4x4 calcRotation(Vector3 vector)
        {
            Vector3 vecX = new Vector3(1, 0, 0);
            Matrix4x4 matRotation;
            Matrix4x4 matYaw;
            //Matrix4x4 matPitch;
            //Matrix4x4 matRoll;
            Vector3 axis;
            float angle;


            // yaw
            {
                Vector3 v1 = vector;
                v1.Y = 0;  // to xz-plane
                v1 = Vector3.Normalize(v1);
                if (vecX == v1 || v1 == Vector3.Zero
                    || float.IsNaN(v1.X) || float.IsNaN(v1.Y) || float.IsNaN(v1.Z))
                {
                    matYaw = Matrix4x4.Identity;
                }
                else
                {
                    angle = (float)Math.Acos(Vector3.Dot(vecX, v1));
                    if (angle < 0.001f)
                    {
                        matYaw = Matrix4x4.Identity;
                    }
                    else if ((float)Math.PI <= angle)
                    {
                        matYaw = Matrix4x4.CreateFromAxisAngle(new Vector3(0, 1, 0), angle);
                    }
                    else
                    {
                        axis = Vector3.Normalize(Vector3.Cross(vecX, v1));
                        System.Diagnostics.Debug.Assert(!float.IsNaN(axis.X));
                        System.Diagnostics.Debug.Assert(!float.IsNaN(axis.Y));
                        System.Diagnostics.Debug.Assert(!float.IsNaN(axis.Z));
                        System.Diagnostics.Debug.Assert(0 < axis.Length());
                        System.Diagnostics.Debug.Assert(!float.IsNaN(angle));
                        matYaw = Matrix4x4.CreateFromAxisAngle(axis, angle);
                    }
                }
            }

            // pitch
            {
                Matrix4x4 matInv;
                Matrix4x4.Invert(matYaw, out matInv);
                Vector3 v1 = (Matrix4x4.CreateTranslation(vector) * matInv).Translation;
                //v1.X = 0;  // to yz-plane
                v1 = Vector3.Normalize(v1);
                if (vecX == v1)
                {
                    matRotation = matYaw;
                }
                else
                {
                    axis = Vector3.Normalize(Vector3.Cross(vecX, v1));
                    angle = (float)Math.Acos(Vector3.Dot(vecX, v1));
                    System.Diagnostics.Debug.Assert(!float.IsNaN(axis.X));
                    System.Diagnostics.Debug.Assert(!float.IsNaN(axis.Y));
                    System.Diagnostics.Debug.Assert(!float.IsNaN(axis.Z));
                    System.Diagnostics.Debug.Assert(0 < axis.Length());
                    System.Diagnostics.Debug.Assert(!float.IsNaN(angle));
                    matRotation = Matrix4x4.CreateFromAxisAngle(axis, angle) * matYaw;
                }
            }

            // 必要であればrollを計算して2軸目を求める

            // 2軸が決まれば姿勢は決まっている
            return matRotation;
        }


        #region SDXのベクトル型との相互変換関数
        internal static Vector3 fromSdx(PEPlugin.SDX.V3 v3)
        {
            return new Vector3(v3.X, v3.Y, v3.Z);
        }
        internal static PEPlugin.SDX.V3 toSdx(Vector3 v3)
        {
            return new PEPlugin.SDX.V3(v3.X, v3.Y, v3.Z);
        }

        /// <summary>
        /// ベクトルの内積値を[0.0 - 1.0]にして返す
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        internal static float calcAngleError(Vector3 v1, Vector3 v2)
        {
            var theta = Vector3.Dot(Vector3.Normalize(v1), Vector3.Normalize(v2));
            theta = (float)Math.Max(-1.0, Math.Min(1.0, theta));
            var angle = Math.Acos(theta);
            return (float)(angle / Math.PI);
        }
        #endregion
    }
}
