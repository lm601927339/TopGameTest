using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collision
{
    public class CCollider
    {
        private int id = 0;
        private List<CBody> pools = new List<CBody>();
        private static CCollider _inst = null;
        public static CCollider inst
        {
            get
            {
                if (_inst == null)
                {
                    _inst = new CCollider();
                }
                return _inst;
            }
        }
        private int axis = -1;
        private int frameID = 0;
        private List<CBody> bodys = new List<CBody>();
        private bool isDirty = false;
        private Dictionary<int, CObjData> pairs = new Dictionary<int, CObjData>();

        public CBody Create(CObject obj)
        {
            CBody body = pools.Count > 0 ? pools.First() : new CBody(obj);
            body.id = id++;
            return body;
        }

        public void Insert(CBody body, bool force = false)
        {
            if (body == null) return;

            if (!body.inCollider)
            {
                bodys.Add(body);
                body.inCollider = true;
            }

            body.isRemove = false;
            body.isRetrieve = false;

            if (force && body.obj != null)
            {
                body.obj.isDirty = Dirty.RTS;
            }
        }

        public void Remove(CBody body, bool retrieve = false)
        {
            if (body == null) return;
            body.isRemove = true;
            body.isRetrieve = retrieve;
        }

        public void Reset()
        {
            axis = -1;
            frameID = 0;
            isDirty = true;

            foreach (CBody body in bodys)
            {
                pools.Add(body);
                body.Clear();
            }
            bodys.Clear();
        }

        public void Clear()
        {
            id = 0;
            axis = -1;
            frameID = 0;
            isDirty = true;
            pools.Clear();

            foreach (CBody body in bodys)
            {
                body.Clear();
            }
            bodys.Clear();
        }

        public void Update(float dt)
        {
            ReBuild();
            Triggers();
        }
        //用于进行相交碰撞测试，检测物体之间是否发生了碰撞，并触发相应的事件
        //相交碰撞测试
        private void Triggers()
        {
            int axis = this.axis;
            int n = (axis >> 2) & 0x3;
            int m = (axis >> 4) & 0x3;

            Dictionary<int, CObjData> pairs = this.pairs;
            int frameID = ++this.frameID;
            int N = bodys.Count;
            for (int i = 0; i < N; i++)
            {
                CBody bi = bodys[i];
                if (bi.isRemove) continue;

                float[] A = bi.aabb;
                float an = A[n];
                float am = A[m];
                int mask = bi.mask;
                int group = bi.group;
                float upper = bi.upper;
                CObject objA = bi.obj;

                for (int j = i + 1; j < N; j++)
                {
                    CBody bj = bodys[j];
                    if (bj.isRemove) continue;

                    if (upper <= bj.lower)
                    {
                        break;
                    }

                    float[] B = bj.aabb;
                    CObject objB = bj.obj;
                    int a2b = mask & bj.group;
                    int b2a = bj.mask & group;

                    if ((a2b != 0 || b2a != 0) && !(an > B[n + 3] || B[n] > A[n + 3] || am > B[m + 3] || B[m] > A[m + 3]))
                    {
                        int at = (int)objA.shape.type;
                        int bt = (int)objB.shape.type;
                        if (at > bt)
                        {
                            if (!CollisionDetection.BoxSphere(bj, bi))
                                continue;
                        }
                        else
                        {
                            if (!CollisionDetection.BoxBox(bi, bj))
                                continue;
                        }

                        Trigger trigger ;
                        int aid = bi.id;
                        int bid = bj.id;
                        int id = aid < bid ? aid << 16 | bid : bid << 16 | aid;

                        if (pairs.ContainsKey(id))
                        {
                            trigger = Trigger.Stay;
                            pairs[id].frameID = frameID;
                        }
                        else
                        {
                            trigger = Trigger.Enter;
                            pairs.Add(id, new CObjData(){ key = id, a = bi, b = bj, frameID = frameID });
                        }

                        //trigger 回调
                        if (a2b != 0 && objA.trigger && !bi.isRemove)
                        {
                            objA.OnTrigger(bj, trigger);
                        }

                        if (b2a != 0 && objB.trigger && !bj.isRemove)
                        {
                            objB.OnTrigger(bi, trigger);
                        }

                        
                    }
                }
            }

            List<int> deletes = new List<int>();
            int length = pairs.Count;
            var entries = pairs.Values.GetEnumerator();
            for (int i = 0; i < length; i++)
            {
                entries.MoveNext();
                var value = entries.Current;
                if (value.frameID != frameID)
                {
                    var bi = value.a;
                    var bj = value.b;
                    var objA = bi.obj;
                    var objB = bj.obj;

                    if (objA != null && !bi.isRemove)
                        objA.OnTrigger(bj, Trigger.Exit);

                    if (objB != null && !bj.isRemove)
                        objB.OnTrigger(bi, Trigger.Exit);

                    deletes.Add((int)value.key);
                }
            }

            length = deletes.Count;
            for (int i = 0; i < length; i++)
            {
                pairs.Remove(deletes[i]);
            }
            deletes.Clear();
        }
        private void ReBuild()
        {
            bool change = false;
            int axis = preBuildAxis();
            if ((axis & 0x3) != (this.axis & 0x3) || this.axis < 0)
            {
                this.axis = axis;
                change = true;
            }

            if (change || this.isDirty)
            {
                this.isDirty = false;

                List<CBody> bodys = this.bodys;
                axis = this.axis & 0x3;
                for (int i = 0, N = bodys.Count; i < N; i++)
                {
                    CBody bi = bodys[i];
                    float[] aabb = bi.aabb;
                    bi.lower = aabb[axis];
                    bi.upper = aabb[axis + 3];
                }

                if (!change)
                    this.Sort(bodys);
                else
                    bodys.Sort((a, b) => a.lower.CompareTo(b.lower));
            }
        }
        // 用于对物体数组进行排序，根据物体的边界信息进行排序
        private void Sort(List<CBody> a)
        {
            for (int i = 1, l = a.Count; i < l; i++)
            {
                CBody v = a[i];
                float lower = v.lower;
                int j = i - 1;
                while (j >= 0 && a[j].lower > lower)
                {
                    a[j + 1] = a[j];
                    j--;
                }
                a[j + 1] = v;
            }
        }
        private int preBuildAxis()
        {
            float sumX = 0, sumX2 = 0;
            float sumY = 0, sumY2 = 0;
            float sumZ = 0, sumZ2 = 0;
            float x = 0.0f, y = 0.0f, z = 0.0f;

            List<CBody> bodys = this.bodys;
            int N = bodys.Count;

            int length = 0;
            bool isDirty = false;

            for (int i = 0; i < N; i++)
            {
                CBody body = bodys[i];

                if (body.isRemove)
                {
                    if (body.isRetrieve)
                    {
                        pools.Add(body);
                        body.Clear();
                    }
                    body.inCollider = false;
                    continue;
                }

                if (++length <= i)
                {
                    bodys[length - 1] = body;
                }

                if (body.UpdateBound())
                {
                    isDirty = true;
                }

                float[] s = body.aabb;
                float sx = (s[3] - s[0]);
                float sy = (s[4] - s[1]);
                float sz = (s[5] - s[2]);
                x += sx * sx;
                y += sy * sy;
                z += sz * sz;

                float cX = (s[3] + s[0]) * 0.5f;
                sumX += cX;
                sumX2 += cX * cX;

                float cY = (s[4] + s[1]) * 0.5f;
                sumY += cY;
                sumY2 += cY * cY;

                float cZ = (s[5] + s[2]) * 0.5f;
                sumZ += cZ;
                sumZ2 += cZ * cZ;
            }

            this.bodys.RemoveRange(length, N - length);
            this.isDirty = isDirty;

            float invN = 1.0f / length;
            x = x > 0 ? length / x : 0;
            y = y > 0 ? length / y : 0;
            z = z > 0 ? length / z : 0;

            float X = (sumX2 - sumX * sumX * invN) * x;
            float Y = (sumY2 - sumY * sumY * invN) * y;
            float Z = (sumZ2 - sumZ * sumZ * invN) * z;

            if (X == 0) X = x;
            if (Y == 0) Y = y;
            if (Z == 0) Z = z;

            if (X > Y)
            {
                if (X > Z)
                {
                    axis = 0;
                    axis |= (Y > Z ? (1 << 2) | (2 << 4) : (1 << 4) | (2 << 2)); // yz:zy
                }
                else
                {
                    axis = 2;
                    axis |= (X > Y ? (0 << 2) | (1 << 4) : (0 << 4) | (1 << 2)); // xy:yx
                }
            }
            else if (Y > Z)
            {
                axis = 1;
                axis |= (X > Z ? (0 << 2) | (2 << 4) : (0 << 4) | (2 << 2)); // xz:zx
            }
            else
            {
                axis = 2;
                axis |= (X > Y ? (0 << 2) | (1 << 4) : (0 << 4) | (1 << 2)); // xy:yx
            }

            return axis;
        }
    }
}
