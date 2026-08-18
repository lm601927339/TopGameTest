using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Diagnostics;
using System;

public class StorageManager : MonoBehaviour
{
	private static StorageManager _instance;

	public static StorageManager Instance
	{
		get
		{
			if (_instance != null)
			{
				return _instance;
			}

			_instance = new GameObject("StorageManager").AddComponent<StorageManager>();
			_instance.Start();
			return _instance;
		}
	}

    private void Start()
    {
        
    }
}
