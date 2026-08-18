using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Collision
{
    public class CBody
    {
        public int id = 0;
        public int mask = 0;
        public int group = 0;
        public CShape shape = null;
        public CObject obj = null;
        public int isDirty = 1 | 2 | 4;
        public float lower = 0;
        public float upper = 0;
        public float[] aabb = new float[6] { 0, 0, 0, 0, 0, 0 };

        public bool isRemove = false;
        public bool isRetrieve = true;
        public bool isIdentity = true;
        public bool inCollider = false;

        public float radius = 0;
        public Vector3 center = Vector3.zero;
        public Matrix4x4 rotMat4 = Matrix4x4.identity;
        public Vector3 halfSize = Vector3.zero;
        public CBody(CObject obj)
        {
            this.obj = obj;
        }
        //新物体的边界。根据物体的旋转、缩放和位置信息，计算物体的世界边界框
        public bool UpdateBound(Dirty force = Dirty.NON)
        {
            CObject co = this.obj;
            var isDirty = force | co.isDirty;

            if (isDirty > 0)
            {
                var shape = this.shape;
                if ((isDirty & Dirty.R) != 0)
                {
                    // 旋转更新aabb
                    this.isIdentity = false;
                    var rot = this.GetRotation();
                    // this.rotMat3.fromQuat(rot); 
                    this.rotMat4 = Matrix4x4.Rotate(rot);// Matrix4x4.TRS(Vector3.zero, rot, Vector3.one);// 计算缓存Mat3
                    if (rot == Quaternion.identity)
                    {
                        this.isIdentity = true;
                    }

                    shape.UpdateAABB(this.rotMat4, this.isIdentity);
                }
                co.isDirty = Dirty.NON;

                var AABB = this.aabb; // world aabb
                var aabb = shape.aabb; // local aabb
                var s = this.GetScale(); // world scale
                var p = this.GetPosition(); // world position

                // 兼容负放大倍数Math.abs(scale)
                var sx = s.x >= 0 ? s.x : -s.x;
                var sy = s.y >= 0 ? s.y : -s.y;
                var sz = s.z >= 0 ? s.z : -s.z;

                AABB[0] = aabb[0] * sx + p.x;
                AABB[1] = aabb[1] * sy + p.y;
                AABB[2] = aabb[2] * sz + p.z;

                AABB[3] = aabb[3] * sx + p.x;
                AABB[4] = aabb[4] * sy + p.y;
                AABB[5] = aabb[5] * sz + p.z;

                this.isDirty = 1 | 2 | 4; // 设置脏区标记

                return true;
            }

            return false;
        }
        public void Clear()
        {
            shape = null;
            obj.body = null;
            obj = null;
            inCollider = false;
            isRemove = false;
        }

        public Vector3 GetScale()
        {
            return obj.transform.lossyScale;
        }

        public Vector3 GetPosition()
        {
            return obj.transform.position;
        }

        public Quaternion GetRotation()
        {
            return obj.transform.rotation;
        }

        public Matrix4x4 GetRotMat4()
        {
            return rotMat4;
        }

        public Vector3 GetCenter()
        {
            if ((isDirty & 1) != 0)
            {
                isDirty &= (~1);
                var aabb = this.aabb;
                var center = this.center;
                center.x = (aabb[0] + aabb[3]) * 0.5f;
                center.y = (aabb[1] + aabb[4]) * 0.5f;
                center.z = (aabb[2] + aabb[5]) * 0.5f;
            }

            return this.center;
        }
        public float GetRadius()
        {
            if ((isDirty & 2) > 0)
            {
                isDirty &= (~2);

                Vector3 s = GetScale();
                float sx = s.x >= 0 ? s.x : -s.x;
                float sy = s.y >= 0 ? s.y : -s.y;
                float sz = s.z >= 0 ? s.z : -s.z;

                float radius = shape.radius;
                float scale = Mathf.Max(sx, sy, sz);
                this.radius = scale * radius;
            }

            return radius;
        }
        public Vector3 GetHalfSize()
        {
            if ((isDirty & 4) > 0)
            {
                isDirty &= (~4);

                Vector3 s = GetScale();
                float sx = s.x >= 0 ? s.x : -s.x;
                float sy = s.y >= 0 ? s.y : -s.y;
                float sz = s.z >= 0 ? s.z : -s.z;

                Vector3 size = shape.size;
                halfSize.x = sx * size.x * 0.5f;
                halfSize.y = sy * size.y * 0.5f;
                halfSize.z = sz * size.z * 0.5f;
            }

            return halfSize;
        }

        
    }
}
