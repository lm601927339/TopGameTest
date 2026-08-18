using System;
using System.Collections;
using System.Collections.Generic;
using RVO;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = System.Random;
using Vector2 = UnityEngine.Vector2;

public class World : MonoBehaviour
{
    /***
     * addObstacle 添加障碍物，逆时针取点
     * setTimeStep 设置步长 0.25f
     * setAgentDefaults 设置代理默认参数
     * processObstacles 障碍物初始化
     * setAgentPrefVelocity 设置代理预处理速度
     *
     */

    private const int guiWidth = 150;
    private const int guiHeight = 50;
    
    [HideInInspector]
    public List<ItemAgent> itemAgents;

    [HideInInspector]
    public Collider2D mapCollider;

    public GameObject itemAgentGo;

    [HideInInspector]
    public Transform map;

    [HideInInspector]
    public Transform itemRoot;

    /// <summary>
    /// 障碍物
    /// </summary>
    [HideInInspector] public Transform obstacleRoot;
    
    [HideInInspector]
    private int agentCnt = 300;
    
    public Random random;
    
    /// <summary>
    /// 起始点
    /// </summary>
    private Vector3 starPos = new Vector3(-3.5f, 6.5f, 0);

    [NonSerialized]
    public List<ObstacleAgent> obstacleAgents = new List<ObstacleAgent>();

    public void Start()
    {
        
        
        random = new Random();
        
        this.map = transform.Find("Map");
        mapCollider = map.GetComponent<Collider2D>();

        this.obstacleRoot = transform.Find("obstacle");

        for (int i = 0; i < this.obstacleRoot.childCount; i++)
        {
            var child = this.obstacleRoot.GetChild(i);
            SpriteRenderer spriteRen = child.GetComponent<SpriteRenderer>();

            ObstacleAgent obstacleAgent = new ObstacleAgent();
            
            IList<RVO.Vector2> obstacle = new List<RVO.Vector2>();
            obstacle.Add(new RVO.Vector2(spriteRen.bounds.max.x,spriteRen.bounds.max.y));
            obstacle.Add(new RVO.Vector2(spriteRen.bounds.min.x,spriteRen.bounds.max.y));
            obstacle.Add(new RVO.Vector2(spriteRen.bounds.min.x,spriteRen.bounds.min.y));
            obstacle.Add(new RVO.Vector2(spriteRen.bounds.max.x,spriteRen.bounds.min.y));

            obstacleAgent.polygon = Simulator.Instance.addObstacle(obstacle);
            obstacleAgent.tf = child;
            
            obstacleAgents.Add(obstacleAgent);
            //Debug.Log("max:" + spriteRen.bounds.max + ",min:" + spriteRen.bounds.min);
        }

        itemRoot = new GameObject("itemRoot").transform;
        itemRoot.SetParent(transform);

        int col = 20;
        itemAgents = new List<ItemAgent>();// ItemAgent[agentCnt];
        Simulator.Instance.setAgentDefaults(15f, 10, 10.0f, 10.0f, 0.125f, 0.5f, new RVO.Vector2(0.0f, 0.0f));
        for (int i = 0; i < agentCnt; i++)
        {
            itemAgents.Add(CreateImemAgent(new Vector2(starPos.x + 0.2f * (i % col), starPos.y - 0.2f * (i / col))));
        }
        
        //Simulator.Instance.processObstacles();
    }

    private ItemAgent CreateImemAgent(Vector2 pos)
    {
        Agent agent = Simulator.Instance.addAgent(new RVO.Vector2(pos.x,pos.y));
        GameObject go = Instantiate(itemAgentGo,itemRoot);
        go.transform.position = pos;
        ItemAgent itemAgent = go.AddComponent<ItemAgent>();
        itemAgent.agent = agent;
        return itemAgent;
    }

    private void OnGUI()
    {
        GUI.skin.button.fontSize = 18;
        
        if (GUILayout.Button("随机移除",GUILayout.Width(guiWidth),GUILayout.Height(guiHeight)))
        {
            if (itemAgents.Count > 0)
            {
                int id = random.Next(0, itemAgents.Count);
                if (id != itemAgents[id].agent.id_)
                {
                    Debug.LogError("id 不一致!");
                }
                
                Object.Destroy(itemAgents[id].gameObject);
                itemAgents.RemoveAt(id);
                Simulator.Instance.delAgent(id);
            }
        }

        if (GUILayout.Button("添加",GUILayout.Width(guiWidth),GUILayout.Height(guiHeight)))
        {
            itemAgents.Add(CreateImemAgent(new Vector2(starPos.x, starPos.y)));
        }

        if (GUILayout.Button("设置位置",GUILayout.Width(guiWidth),GUILayout.Height(guiHeight)))
        {
            if (itemAgents.Count > 0)
            {
                Simulator.Instance.setAgentPosition(itemAgents[0].agent.id_, new RVO.Vector2(starPos.x,starPos.y));
            }
        }
        
        if (GUILayout.Button("移除障碍物",GUILayout.Width(guiWidth),GUILayout.Height(guiHeight)))
        {
            int randomIndex = random.Next(0, obstacleAgents.Count);

            if (obstacleAgents[randomIndex].polygon != null)
            {
                Simulator.Instance.delObstacle(obstacleAgents[randomIndex].polygon._id);
                obstacleAgents[randomIndex].polygon = null;
                obstacleAgents[randomIndex].tf.gameObject.SetActive(false);
            }
        }

        if (GUILayout.Button("添加障碍物",GUILayout.Width(guiWidth),GUILayout.Height(guiHeight)))
        {
            int randomIndex = random.Next(0, obstacleAgents.Count);

            if (!obstacleAgents[randomIndex].tf.gameObject.activeSelf)
            {
                SpriteRenderer spriteRen = obstacleAgents[randomIndex].tf.GetComponent<SpriteRenderer>();

                ObstacleAgent obstacleAgent = obstacleAgents[randomIndex];
            
                IList<RVO.Vector2> obstacle = new List<RVO.Vector2>();
                obstacle.Add(new RVO.Vector2(spriteRen.bounds.max.x,spriteRen.bounds.max.y));
                obstacle.Add(new RVO.Vector2(spriteRen.bounds.min.x,spriteRen.bounds.max.y));
                obstacle.Add(new RVO.Vector2(spriteRen.bounds.min.x,spriteRen.bounds.min.y));
                obstacle.Add(new RVO.Vector2(spriteRen.bounds.max.x,spriteRen.bounds.min.y));

                obstacleAgent.polygon = Simulator.Instance.addObstacle(obstacle);
                
                obstacleAgents[randomIndex].tf.gameObject.SetActive(true);
            }
        }
    }

    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Simulator.Instance.setTimeStep(Time.deltaTime);
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RVO.Vector2 target = new RVO.Vector2(worldPos.x, worldPos.y);

            for (int i = 0; i < itemAgents.Count; i++)
            {
                int id = itemAgents[i].agent.id_;
                if (Simulator.Instance.isNeedDelete(id))
                {
                    continue;
                }
                var goalVector = target - Simulator.Instance.getAgentPosition(id);
                if (RVOMath.absSq(goalVector) > 0.01f)
                {
                    goalVector = RVOMath.normalize(goalVector) * 0.5f;
                    Simulator.Instance.setAgentPrefVelocity(id, goalVector);

                    /* Perturb a little to avoid deadlocks due to perfect symmetry. */
                    float angle = (float) random.NextDouble()*2.0f*(float) Math.PI;
                    float dist = (float) random.NextDouble()*0.0001f;

                    Simulator.Instance.setAgentPrefVelocity(id, Simulator.Instance.getAgentPrefVelocity(id) +
                                                                dist*
                                                                new RVO.Vector2((float) Math.Cos(angle), (float) Math.Sin(angle)));
                }
                else
                {
                    Simulator.Instance.setAgentPrefVelocity(id, new RVO.Vector2(0,0));
                }
            }
            
            Simulator.Instance.doStep();

            for (int i = 0; i < itemAgents.Count; i++)
            {
                var pos = Simulator.Instance.getAgentPosition(i);
                itemAgents[i].transform.position = new Vector3(pos.x(), pos.y(), itemAgents[i].transform.position.z);

                if (i != itemAgents[i].agent.id_)
                {
                    Debug.LogError("id 不一致!");
                }
            }
            //float dis = Vector3.Distance(itemAgents[0].transform.position, itemAgents[1].transform.position);
            //Debug.Log("长度:" + dis);
        }
    }
}
