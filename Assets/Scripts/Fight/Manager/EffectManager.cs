using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    private Dictionary<string, GameObject> sightEffect = new Dictionary<string, GameObject>();

    private void FixedUpdate()
    {
        List<string> toRemoveKey = new List<string>();
        foreach (KeyValuePair<string, GameObject> se in sightEffect)
        {
            if (se.Value == null) toRemoveKey.Add(se.Key);
        }
        for (int i = 0; i < toRemoveKey.Count; i++) sightEffect.Remove(toRemoveKey[i]);
    }

    public void CreateSightEffect(string prefab, Vector3 pos, float degree, string key = "", bool loop = false)
    {
        if (sightEffect.ContainsKey(key)) return;

        GameObject effectPrefab = GameRuntime.LoadPrefab(prefab);
        if (!effectPrefab) return;

        GameObject effectGO = Instantiate(effectPrefab, pos, Quaternion.identity, transform);
        effectGO.transform.RotateAround(effectGO.transform.position, Vector3.up, degree);
        if (!effectGO) return;

        SightEffect se = effectGO.GetComponent<SightEffect>();
        if (!se)
        {
            Destroy(effectGO);
            return;
        }

        if (!loop)
        {
            effectGO.AddComponent<UnitRemover>().duration = se.duration;
        }

        if (key != "") sightEffect.Add(key, effectGO);
    }

    public void RemoveSightEffect(string key)
    {
        if (!sightEffect.ContainsKey(key)) return;
        Destroy(sightEffect[key]);
        sightEffect.Remove(key);
    }
}
