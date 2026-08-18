using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Collision
{
    public enum Trigger
    {
        Default = 0,
        Enter = 1,
        Stay = 2,
        Exit = 3
    }

    public enum Dirty
    {
        R = 1,
        T = 2,
        S = 4,
        RTS = 7,
        RS = R | S,
        NON = 0
    }

    public class CObjData {
        public long key;
        public long frameID = 0;
        public CBody a, b;
    }

    public class CObject : MonoBehaviour
    {
        [SerializeField]
        public bool trigger = false; //碰撞开关
        public int group = 0; //碰撞分组
        public ShapeType type = ShapeType.Box; //相交形状类型
        public Vector3 center = Vector3.zero; //偏移位置，是shape相对node节点的中心偏移
        public Vector3 size = Vector3.zero; //方块的长宽高
        public float radius = 0; //半径，sphere 或者 capsule
        public float height = 0; //半高 ,capsule 专用

        public Dirty isDirty = Dirty.RTS;
        public CShape shape = null;
        public CBody body = null;


        //常用变量
        public Vector3 velocity = Vector3.zero; //当前速度
        public float speed = 0; //最大速度
        public float angle = 0; //旋转角度

        private void Awake()
        {
            //创建碰撞形状
            switch (type)
            {
                case ShapeType.Box:
                    shape = new Box(center, size);
                    break;
                case ShapeType.Sphere:
                    shape = new Sphere(center, radius);
                    break;
                case ShapeType.Capsule:
                    shape = new Capsule(center, radius, height);
                    break;
            }

            //创建碰撞body容器
            body = CCollider.inst.Create(this);

            body.shape = shape; //绑定碰撞形状
            body.group = group; //碰撞分组掩码
            body.mask = Physics2D.GetLayerCollisionMask(group);

            //把body加入碰撞管理
            CCollider.inst.Insert(body);

            isDirty = Dirty.RTS; //首次更新标记
        }

        //同步位置到body
        public void SetPosition(Vector3 position)
        {
            transform.position = position;
            isDirty |= Dirty.T;
        }

        //同步旋转到body
        public void SetRotation(Quaternion rotation)
        {
            transform.rotation = rotation;
            isDirty |= Dirty.R;
        }

        //同步缩放到body
        public void SetScale(Vector3 scale)
        {
            transform.localScale = scale;
            isDirty |= Dirty.S;
        }

        public Quaternion GetRotation() { return transform.rotation; }
        public Vector3 GetPosition() { return transform.position; }
        public Vector3 GetScale() { return transform.localScale; }

        //删除当前节点
        public GameObject Remove(bool retrieve = true)
        {
            //移除body, retrieve: 是否回收body ？
            CCollider.inst.Remove(body, retrieve);

            //从父节点移除
            transform.parent = null;

            //最后node用户自己控制回收和释放
            //this.Remove().Destroy() // 回收body，释放node
            //pool.push(this.Remove(false)); //不回收body , 回收node

            return gameObject;
        }

        //重新添加到父节点
        public void Insert(Transform parent)
        {
            //插入body, 强制更新body数据
            CCollider.inst.Insert(body, true);

            //添加到父节点
            if (transform.parent != parent)
                transform.parent = parent;
        }

        public void SetAnimation(string name) { }
        public void SetColor(Color color) { }
        public void Init() { }

        //trigger 回调 enter,stay exit
        public virtual void OnTrigger(CBody b, Trigger trigger)
        {
            switch (trigger)
            {
                case Trigger.Enter:
                    //onTriggerEnter();
                    break;
                case Trigger.Stay:
                    //onTriggerStay();
                    break;
                case Trigger.Exit:
                    //onTriggerExit();
                    break;
            }
        }

        private void OnDestroy()
        {
            CancelInvoke();
            shape = null;
            body = null;
        }
    }
}
