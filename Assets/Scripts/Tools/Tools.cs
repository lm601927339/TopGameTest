using System.Collections.Generic;
using UnityEngine;

public class Tools
{
    /**
    * !#zh 拷贝object。
    */
    /**
     * 深度拷贝
     * @param {any} sObj 拷贝的对象
     * @returns 
     */
    public static T[] Clone<T>(T[] source)
    {
        T[] clone = new T[source.Length];
        for (int i = 0; i < source.Length; i++)
        {
            clone[i] = source[i];
        }
        return clone;
    }

    public static T[] ObjectToArray<T>(Dictionary<string, T> srcObj)
    {
        T[] resultArr = new T[srcObj.Count];
        srcObj.Values.CopyTo(resultArr, 0);
        return resultArr;
    }

    public static Dictionary<string, T> ArrayToObject<T>(T[] srcObj, string objectKey)
    {
        Dictionary<string, T> resultObj = new Dictionary<string, T>();
        foreach (var item in srcObj)
        {
            resultObj[item.GetType().GetProperty(objectKey).GetValue(item, null).ToString()] = item;
        }
        return resultObj;
    }
    /**
     * 根据权重,计算随机内容
     * @param {arrany} weightArr 
     * @param {number} totalWeight 权重
     * @returns 
     */
    public static int GetWeightRandIndex(int[] weightArr, int totalWeight)
    {
        int randWeight = Random.Range(0, totalWeight);
        int sum = 0;
        for (int weightIndex = 0; weightIndex < weightArr.Length; weightIndex++)
        {
            sum += weightArr[weightIndex];
            if (randWeight < sum)
            {
                return weightIndex;
            }
        }
        return 0;
    }
    /**
    * 从n个数中获取m个随机数
    * @param {Number} n   总数
    * @param {Number} m    获取数
    * @returns {Array} array   获取数列
    */
    public static int[] GetRandomNFromM(int n, int m)
    {
        List<int> array = new List<int>();
        int intRd = 0;
        int count = 0;
        while (count < m)
        {
            if (count >= n + 1)
            {
                break;
            }
            intRd = Random.Range(0, n);
            if (!array.Contains(intRd))
            {
                array.Add(intRd);
                count++;
            }
        }
        return array.ToArray();
    }

    public static int GetRandomInt(int min, int max)
    {
        return Random.Range(min, max + 1);
    }

    public static float GetRandom(float min, float max)
    {
        return Random.Range(min, max);
    }

    public static int GetStringLength(string render)
    {
        int len = 0;
        foreach (char c in render)
        {
            int val = (int)c;
            if (val <= 255)
            {
                len += 1;
            }
            else
            {
                len += 2;
            }
        }
        return Mathf.CeilToInt(len / 2);
    }

    public static bool IsEmptyObject(object obj)
    {
        if (obj is Dictionary<string, object>)
        {
            return ((Dictionary<string, object>)obj).Count == 0;
        }
        return false;
    }

   
}