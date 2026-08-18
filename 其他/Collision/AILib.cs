using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Collision
{
    public static class AILib
    {
        private static Vector3 localP = Vector3.zero;
        private static Vector3 maxDist = Vector3.zero;
        private static Vector3 localCenter = Vector3.zero;
        private static Quaternion obbToLocal = Quaternion.identity;

        // 计算球体到 AABB 的 SDF 距离
        // 它接受球体的中心点坐标center，半径radius和AABB包围盒的尺寸size作为参数，
        // 返回一个布尔值表示球体是否与AABB包围盒相交
        public static bool SphereAABBDistance(Vector3 center, float radius, Vector3 size)
        {
            // 计算离包围盒最近的点
            maxDist.x = Mathf.Max(-size.x, Mathf.Min(center.x, size.x));
            maxDist.y = Mathf.Max(-size.y, Mathf.Min(center.y, size.y));
            maxDist.z = Mathf.Max(-size.z, Mathf.Min(center.z, size.z));

            maxDist -= center;

            float distSqr = Vector3.Dot(maxDist, maxDist);

            return distSqr <= radius * radius;
        }

        /// <summary>
        /// 计算球体到 OBB 的 SDF 距离
        /// </summary>
        /// <param name="sphereCenter">// 球体中心点坐标</param>
        /// <param name="radius">球体半径</param>
        /// <param name="obbCenter">OBB 中心点坐标</param>
        /// <param name="obbQuaternion">BB 旋转四元数</param>
        /// <param name="obbHalfExtents">OBB 半扩展尺寸</param>
        /// <returns></returns>
        public static bool SphereOBBDistance(Vector3 sphereCenter, float radius, Vector3 obbCenter, Quaternion obbQuaternion, Vector3 obbHalfExtents)
        {
            localCenter = sphereCenter - obbCenter;

            obbToLocal = Quaternion.Inverse(obbQuaternion);

            Vector3 localP = Quaternion.Inverse(obbQuaternion) * localCenter;

            return AILib.SphereAABBDistance(localP, radius, obbHalfExtents);
        }

        public static bool ObbIntersect(Vector3 centerA, Vector3 halfA, Matrix4x4 rotA, Vector3 centerB, Vector3 halfB, Matrix4x4 rotB)
        {
            float ae0 = halfA.x, ae1 = halfA.y, ae2 = halfA.z;
            float au00 = rotA.m00, au01 = rotA.m01, au02 = rotA.m02;
            float au10 = rotA.m10, au11 = rotA.m11, au12 = rotA.m12;
            float au20 = rotA.m20, au21 = rotA.m21, au22 = rotA.m22;

            float be0 = halfB.x, be1 = halfB.y, be2 = halfB.z;
            float bu00 = rotB.m00, bu01 = rotB.m01, bu02 = rotB.m02;
            float bu10 = rotB.m10, bu11 = rotB.m11, bu12 = rotB.m12;
            float bu20 = rotB.m20, bu21 = rotB.m21, bu22 = rotB.m22;

            float R00 = au00 * bu00 + au01 * bu01 + au02 * bu02;
            float R01 = au00 * bu10 + au01 * bu11 + au02 * bu12;
            float R02 = au00 * bu20 + au01 * bu21 + au02 * bu22;
            float R10 = au10 * bu00 + au11 * bu01 + au12 * bu02;
            float R11 = au10 * bu10 + au11 * bu11 + au12 * bu12;
            float R12 = au10 * bu20 + au11 * bu21 + au12 * bu22;
            float R20 = au20 * bu00 + au21 * bu01 + au22 * bu02;
            float R21 = au20 * bu10 + au21 * bu11 + au22 * bu12;
            float R22 = au20 * bu20 + au21 * bu21 + au22 * bu22;

            float v0 = centerB.x - centerA.x;
            float v1 = centerB.y - centerA.y;
            float v2 = centerB.z - centerA.z;

            float t0 = v0 * au00 + v1 * au01 + v2 * au02;
            float t1 = v0 * au10 + v1 * au11 + v2 * au12;
            float t2 = v0 * au20 + v1 * au21 + v2 * au22;

            float ra, rb, abs;
            float epsilon = float.Epsilon;

            float A00 = (R00 >= 0 ? R00 : -R00) + epsilon, A01 = (R01 >= 0 ? R01 : -R01) + epsilon, A02 = (R02 >= 0 ? R02 : -R02) + epsilon;
            float A10 = (R10 >= 0 ? R10 : -R10) + epsilon, A11 = (R11 >= 0 ? R11 : -R11) + epsilon, A12 = (R12 >= 0 ? R12 : -R12) + epsilon;
            float A20 = (R20 >= 0 ? R20 : -R20) + epsilon, A21 = (R21 >= 0 ? R21 : -R21) + epsilon, A22 = (R22 >= 0 ? R22 : -R22) + epsilon;

            ra = ae0;
            rb = be0 * A00 + be1 * A01 + be2 * A02;
            if ((t0 >= 0 ? t0 : -t0) > ra + rb) return false;

            ra = ae1;
            rb = be0 * A10 + be1 * A11 + be2 * A12;
            if ((t1 >= 0 ? t1 : -t1) > ra + rb) return false;

            ra = ae2;
            rb = be0 * A20 + be1 * A21 + be2 * A22;
            if ((t2 >= 0 ? t2 : -t2) > ra + rb) return false;

            rb = be0;
            ra = ae0 * A00 + ae1 * A10 + ae2 * A20;
            abs = t0 * R00 + t1 * R10 + t2 * R20;
            if ((abs >= 0 ? abs : -abs) > ra + rb) return false;

            rb = be1;
            ra = ae0 * A01 + ae1 * A11 + ae2 * A21;
            abs = t0 * R01 + t1 * R11 + t2 * R21;
            if ((abs >= 0 ? abs : -abs) > ra + rb) return false;

            rb = be2;
            ra = ae0 * A02 + ae1 * A12 + ae2 * A22;
            abs = t0 * R02 + t1 * R12 + t2 * R22;
            if ((abs >= 0 ? abs : -abs) > ra + rb) return false;

            // test axis L = A0 x B0
            ra = ae1 * A20 + ae2 * A10;
            rb = be1 * A02 + be2 * A01;
            abs = t2 * R10 - t1 * R20;
            if ((abs >= 0 ? abs : -abs) > ra + rb) return false;

            // test axis L = A0 x B1
            ra = ae1 * A21 + ae2 * A11;
            rb = be0 * A02 + be2 * A00;
            abs = t2 * R11 - t1 * R21;
            if ((abs >= 0 ? abs : -abs) > ra + rb) return false;

            // test axis L = A0 x B2
            ra = ae1 * A22 + ae2 * A12;
            rb = be0 * A01 + be1 * A00;
            abs = t2 * R12 - t1 * R22;
            if ((abs >= 0 ? abs : -abs) > ra + rb) return false;

            // test axis L = A1 x B0
            ra = ae0 * A20 + ae2 * A00;
            rb = be1 * A12 + be2 * A11;
            abs = t0 * R20 - t2 * R00;
            if ((abs >= 0 ? abs : -abs) > ra + rb) return false;

            // test axis L = A1 x B1
            ra = ae0 * A21 + ae2 * A01;
            rb = be0 * A12 + be2 * A10;
            abs = t0 * R21 - t2 * R01;
            if ((abs >= 0 ? abs : -abs) > ra + rb) return false;

            // test axis L = A1 x B2
            ra = ae0 * A22 + ae2 * A02;
            rb = be0 * A11 + be1 * A10;
            abs = t0 * R22 - t2 * R02;
            if ((abs >= 0 ? abs : -abs) > ra + rb) return false;

            // test axis L = A2 x B0
            ra = ae0 * A10 + ae1 * A00;
            rb = be1 * A22 + be2 * A21;
            abs = t1 * R00 - t0 * R10;
            if ((abs >= 0 ? abs : -abs) > ra + rb) return false;

            // test axis L = A2 x B1
            ra = ae0 * A11 + ae1 * A01;
            rb = be0 * A22 + be2 * A20;
            abs = t1 * R01 - t0 * R11;
            if ((abs >= 0 ? abs : -abs) > ra + rb) return false;

            // test axis L = A2 x B2
            ra = ae0 * A12 + ae1 * A02;
            rb = be0 * A21 + be1 * A20;
            abs = t1 * R02 - t0 * R12;
            if ((abs >= 0 ? abs : -abs) > ra + rb) return false;

            return true;
        }
    }
}
