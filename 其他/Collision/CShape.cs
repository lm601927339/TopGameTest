using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Collision
{
    //形状的类型，包括盒子（Box）、球体（Sphere）和胶囊体（Capsule）
    public enum ShapeType
    {
        Box = 1,
        Sphere = 2,
        Capsule = 4
    }

    public abstract class CShape
    {
        public float radius = 0;
        public float height = 0;
        public ShapeType type = ShapeType.Box;
        public Vector3 size = Vector3.zero;
        public Vector3 center = Vector3.zero;
        public Vector3 halfSize = Vector3.zero;
        public float[] aabb = new float[6] { 0, 0, 0, 0, 0, 0 };

        public CShape(Vector3 center, ShapeType type)
        {
            this.type = type;
            this.center = center;
        }
        //旋转形状的轴对齐包围盒（AABB），接受一个旋转矩阵 world 作为参数
        public float[] RotateAABB(Matrix4x4 world)
        {
            Vector3 center = this.center;
            float cx = center.x, cy = center.y, cz = center.z;

            Vector3 size = this.halfSize;
            float x = size.x, y = size.y, z = size.z;

            float uX = world.m00, uY = world.m01, uZ = world.m02;
            float vX = world.m10, vY = world.m11, vZ = world.m12;
            float wX = world.m20, wY = world.m21, wZ = world.m22;

            float cX = uX * cx + vX * cy + wX * cz;
            float cY = uY * cx + vY * cy + wY * cz;
            float cZ = uZ * cx + vZ * cy + wZ * cz;

            float absU = Mathf.Abs(uX) * x + Mathf.Abs(vX) * y + Mathf.Abs(wX) * z;
            float absV = Mathf.Abs(uY) * x + Mathf.Abs(vY) * y + Mathf.Abs(wY) * z;
            float absW = Mathf.Abs(uZ) * x + Mathf.Abs(vZ) * y + Mathf.Abs(wZ) * z;

            float[] aabb = this.aabb;
            aabb[0] = cX - absU; aabb[1] = cY - absV; aabb[2] = cZ - absW;
            aabb[3] = cX + absU; aabb[4] = cY + absV; aabb[5] = cZ + absW;

            return aabb;
        }

        public void NormalAABB()
        {
            float[] aabb = this.aabb;
            Vector3 size = this.halfSize;
            Vector3 center = this.center;
            float cX = center.x, cY = center.y, cZ = center.z;

            aabb[0] = cX - size.x;
            aabb[1] = cY - size.y;
            aabb[2] = cZ - size.z;

            aabb[3] = cX + size.x;
            aabb[4] = cY + size.y;
            aabb[5] = cZ + size.z;
        }

        public void UpdateAABB(Matrix4x4 world, bool isIdentity = true)
        {
            if (isIdentity || this.type == ShapeType.Sphere)
            {
                this.NormalAABB();
            }
            else
            {
                this.RotateAABB(world);
            }
        }
    }

    public class Box : CShape
    {
        public Box(Vector3 center, Vector3 size) : base(center, ShapeType.Box)
        {
            this.size = size;
            this.halfSize = size * 0.5f;
        }
    }

    public class Sphere : CShape
    {
        public Sphere(Vector3 center, float radius) : base(center, ShapeType.Sphere)
        {
            this.radius = radius;
            this.size = Vector3.one * radius * 2;
            this.halfSize = Vector3.one * radius;
        }
    }

    public class Capsule : CShape
    {
        public Capsule(Vector3 center, float radius, float height) : base(center, ShapeType.Capsule)
        {
            this.radius = radius;
            this.height = height;
            this.size = new Vector3(radius * 2, height * 2, radius * 2);
            this.halfSize = new Vector3(radius, height, radius);
        }
    }

    public static class CollisionDetection
    {
        static Vector3 center = new Vector3();
        public static Func<CBody, CBody, bool>[] SupportFunctions = new Func<CBody, CBody, bool>[8];

        public static bool BoxBox(CBody a, CBody b)
        {
            // a,b have no rotation and have been processed with AABB, directly return true
            if (a.isIdentity && b.isIdentity) return true;

            return AILib.ObbIntersect(a.GetCenter(), a.GetHalfSize(), a.GetRotMat4(), b.GetCenter(), b.GetHalfSize(), b.GetRotMat4());
        }
        public static bool BoxSphere(CBody a, CBody b)
        {
            // a has no rotation, treated as AABB
            if (a.isIdentity)
            {
                // Convert to AABB coordinate system

                center = b.GetCenter()- a.GetCenter();
                return AILib.SphereAABBDistance(a.GetCenter(), b.GetRadius(), a.GetHalfSize());
            }

            return AILib.SphereOBBDistance(b.GetCenter(), b.GetRadius(), a.GetCenter(), a.GetRotation(), a.GetHalfSize());
        }

        public static bool SphereSphere(CBody a, CBody b)
        {
            Vector3 ca = a.GetCenter();
            Vector3 cb = b.GetCenter();
            center = ca - cb;
            float lengthSqr = center.sqrMagnitude;
            float radii = a.GetRadius() + b.GetRadius();

            return lengthSqr <= radii * radii;
        }

        public static bool BoxCapsule(CBody a, CBody b)
        {
            // AI TODO
            return true;
        }

        public static bool SphereCapsule(CBody a, CBody b)
        {
            // AI TODO
            return true;
        }

        public static bool CapsuleCapsule(CBody a, CBody b)
        {
            // AI TODO
            return true;
        }


    }
}
