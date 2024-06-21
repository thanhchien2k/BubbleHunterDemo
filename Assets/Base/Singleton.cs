using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    [SerializeField] protected bool isDondestoyOnLoad = true;
    private static T instance;
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<T>();

                if (instance == null)
                {
                    SetUpInstance();
                }
                else
                {
                    string typeName = typeof(T).Name;
                    Debug.Log("Singleton of " + typeName + " instance already created: " + instance.gameObject.name);
                }
            }

            return instance;
        }
    }
    protected virtual void Awake()
    {
        if (instance == null)
        {
            instance = this as T;
            if (isDondestoyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }
        }
        else if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
    }

    private static void SetUpInstance()
    {
        instance = FindObjectOfType<T>();

        if (instance == null)
        {
            GameObject newObj = new();
            newObj.name = typeof(T).Name;
            instance = newObj.AddComponent<T>();
        }
    }

}
