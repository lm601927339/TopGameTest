using System.Collections.Generic;
using UnityEngine;

public class UnitBindPoint : MonoBehaviour
{
    public string key;
    public Vector3 offset;

    private Dictionary<string, BindGameObjectInfo> bindGameObject = new Dictionary<string, BindGameObjectInfo>();

    private void FixedUpdate()
    {
        List<string> toRemove = new List<string>();
        foreach (KeyValuePair<string, BindGameObjectInfo> goInfo in bindGameObject)
        {
            if (goInfo.Value.gameObject == null)
            {
                toRemove.Add(goInfo.Key);
                continue;
            }

            if (goInfo.Value.forever == false)
            {
                goInfo.Value.duration -= Time.fixedDeltaTime;
                if (goInfo.Value.duration <= 0)
                {
                    Destroy(goInfo.Value.gameObject);
                    toRemove.Add(goInfo.Key);
                }
            }
        }

        for (int i = 0; i < toRemove.Count; i++)
        {
            bindGameObject.Remove(toRemove[i]);
        }
    }

    public void AddBindGameObject(string goPath, string key, bool loop)
    {
        if (key != "" && bindGameObject.ContainsKey(key)) return;

        GameObject prefab = GameRuntime.LoadPrefab(goPath);
        if (!prefab) return;

        GameObject effectGO = Instantiate(prefab, Vector3.zero, Quaternion.identity, transform);
        effectGO.transform.localPosition = offset;
        effectGO.transform.localRotation = Quaternion.identity;
        if (!effectGO) return;

        SightEffect se = effectGO.GetComponent<SightEffect>();
        if (!se)
        {
            Destroy(effectGO);
            return;
        }

        float duration = se.duration * (loop == false ? 1 : -1);
        BindGameObjectInfo bindGameObjectInfo = new BindGameObjectInfo(effectGO, duration);
        if (key != "")
        {
            this.bindGameObject.Add(key, bindGameObjectInfo);
        }
        else
        {
            this.bindGameObject.Add(
                Time.frameCount * Random.Range(1.00f, 9.99f) + "_" + Random.Range(1, 9999),
                bindGameObjectInfo
            );
        }
    }

    public void RemoveBindGameObject(string key)
    {
        if (bindGameObject.ContainsKey(key) == false) return;
        if (bindGameObject[key].gameObject)
        {
            Destroy(bindGameObject[key].gameObject);
        }
        bindGameObject.Remove(key);
    }
}

public class BindGameObjectInfo
{
    public GameObject gameObject;
    public float duration;
    public bool forever;

    public BindGameObjectInfo(GameObject gameObject, float duration)
    {
        this.gameObject = gameObject;
        this.duration = Mathf.Abs(duration);
        this.forever = duration <= 0;
    }
}
