using UnityEngine;
using System.Collections.Generic;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get { return Singleton<PoolManager>.Instance; } }

    private Dictionary<string, Queue<GameObject>> _dictPool = new Dictionary<string, Queue<GameObject>>();
    private Dictionary<string, GameObject> _dictPrefab = new Dictionary<string, GameObject>();

    public GameObject GetNode(GameObject prefab, Transform parent)
    {
        string name = prefab.name;
        _dictPrefab[name] = prefab;
        GameObject node = null;
        if (_dictPool.ContainsKey(name))
        {
            if (_dictPool[name].Count > 0)
            {
                node = _dictPool[name].Dequeue();
            }
            else
            {
                node = Instantiate(prefab);
            }
        }
        else
        {
            Queue<GameObject> pool = new Queue<GameObject>();
            _dictPool[name] = pool;

            node = Instantiate(prefab);
        }

        node.transform.parent = parent;
        node.SetActive(true);
        return node;
    }

    public void PutNode(GameObject node)
    {
        if (node == null)
        {
            return;
        }
        string name = node.name;
        Queue<GameObject> pool = null;
        if (_dictPool.ContainsKey(name))
        {
            pool = _dictPool[name];
        }
        else
        {
            pool = new Queue<GameObject>();
            _dictPool[name] = pool;
        }

        pool.Enqueue(node);
    }

    public void ClearPool(string name)
    {
        if (_dictPool.ContainsKey(name))
        {
            _dictPool[name].Clear();
            _dictPool.Remove(name);
        }
    }
}