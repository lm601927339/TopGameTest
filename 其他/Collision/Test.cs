using Collision;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Test : MonoBehaviour
{
    public Slider slider1,slider2;
    public Text Num1, Num2;
    public Toggle toggle;

    public GameObject circleGo;
    public GameObject squareGo;

    public Transform parentNode;
    private long boxNum = 0, sphereNum = 0;
    private long circleTotalCount = 1000, squareTotalCount = 1000;

    private List<CObject> objects;

    private Vector2 world = new Vector2(100, 60);
    public float speed = 50f;

    bool isRotate = false;
    void Start()
    {
        Num1.text = boxNum.ToString();
        Num2.text = sphereNum.ToString();
        slider1.value = boxNum / circleTotalCount;
        slider1.onValueChanged.AddListener((float value) =>
        {
            boxNum = Mathf.FloorToInt(value);
            Num1.text = boxNum.ToString();
        });

        slider2.value = sphereNum / squareTotalCount;
        slider2.onValueChanged.AddListener((float value) =>
        {
            sphereNum = Mathf.FloorToInt(value);
            Num2.text = sphereNum.ToString();
        });

        objects = new List<CObject>();

        toggle.onValueChanged.AddListener((bool value) => {
            isRotate = value;
            onRotation();
        });
    }

    void onRotation() {
        
        if (!isRotate)
            ResetRotation();
        else
            RandomRotation();
            
       
    }

    private void ResetRotation()
    {
        for (int i = 0; i < objects.Count; i++)
        {
            CObject obj = objects[i];
            obj.SetRotation(Quaternion.identity);
        }
    }

    private void RandomRotation()
    {
        for (int i = 0; i < objects.Count; i++)
        {
            CObject obj = objects[i];
            Quaternion tempRot = Quaternion.Euler(0, 0, Random.Range(0, 360));
            obj.SetRotation(tempRot);
        }
    }

    private void OnDestroy()
    {
        boxNum = 0;
        sphereNum = 0;
        //objects = new CObject[capacity];
        CCollider.inst.Reset();
    }

    private void AddObjects(int num, GameObject prefab)
    {
        for (int i = 0; i < num; i++)
        {
            GameObject node = Instantiate(prefab);
            node.transform.parent = parentNode;

            Vector3 tempPos = new Vector3(Random.Range(-0.5f, 0.5f) * world.x, Random.Range(-0.5f, 0.5f) * world.y, 0);
            node.transform.position = tempPos;

            CObject obj = node.GetComponent<CObject>();
            float angle = Random.Range(0, Mathf.PI * 2);
            obj.angle = Random.value;

            float speed = this.speed * (Random.Range(0.1f, 1f));
            obj.velocity.x = Mathf.Cos(angle) * speed;
            obj.velocity.y = Mathf.Sin(angle) * speed;
            obj.velocity.z = 0;

            if (isRotate)
            {
                Quaternion tempRot = Quaternion.Euler(0, 0, Random.Range(0, 360));
                obj.SetRotation(tempRot);
            }

            objects.Add( obj);
        }
    }

    private void Update()
    {
        float dt = Time.deltaTime * Config.maxSpeed;

        int boxNum = Mathf.RoundToInt(Config.box * Config.maxNum);
        if (boxNum > this.boxNum)
        {
            int max = (int)Mathf.Clamp(boxNum - this.boxNum, 0, 20);
            AddObjects(max, circleGo);
            this.boxNum += max;
        }

        int sphereNum = Mathf.RoundToInt(Config.sphere * Config.maxNum);
        if (sphereNum > this.sphereNum)
        {
            int max = (int)Mathf.Clamp(sphereNum - this.sphereNum, 0, 20);
            AddObjects(max, squareGo);
            this.sphereNum += max;
        }

        for (int i = 0; i < objects.Count; i++)
        {
            CObject obj = objects[i];
            Vector3 tempPos = obj.GetPosition();

            if (Mathf.Abs(tempPos.x + obj.velocity.x * dt) > world.x / 2)
            {
                obj.velocity.x = -obj.velocity.x;
            }

            if (Mathf.Abs(tempPos.y + obj.velocity.y * dt) > world.y / 2)
            {
                obj.velocity.y = -obj.velocity.y;
            }

            tempPos.x += obj.velocity.x * dt;
            tempPos.y += obj.velocity.y * dt;
            tempPos.z += obj.velocity.z * dt;

            obj.SetPosition(tempPos);
            
            if (boxNum < this.boxNum)
            {
                if (obj.type == ShapeType.Box)
                {
                    obj.Remove();
                    Destroy(obj.gameObject);
                    objects[i] = null;
                    this.boxNum--;
                }
            }

            if (sphereNum < this.sphereNum)
            {
                if (obj.type == ShapeType.Sphere)
                {
                    obj.Remove();
                    Destroy(obj.gameObject);
                    objects[i] = null;
                    this.sphereNum--;
                }
            }
        }

        CCollider.inst.Update(dt);
    }
}

public class Config {
    public const int demoIdx = 0;
    public const int totalNum = 0;
    //public const bool isRotate = true;//开启旋转(精确碰撞)
    public const float maxSpeed = 1.0f;   //模拟世界速度
    public const int maxNum = 10;  //最大总个数
    public const float sphere = 0.5f; //shpere 比例
    public const float box = 0.5f; //box 比例
};


