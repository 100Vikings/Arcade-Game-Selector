using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;
    private static readonly object singletonLock = new();

    public static T Instance
    {
        get
        {
            return GetInstance();
        }
    }

    private static T GetInstance()
    {
        lock (singletonLock)
        {
            if (instance != null)
            {
                return instance;
            }

            instance = (T)FindFirstObjectByType(typeof(T));

            if (instance == null)
            {
                GameObject singleton = new();
                instance = singleton.AddComponent<T>();
                singleton.name = "(Singleton)" + typeof(T).ToString();
            }

            else if (FindObjectsOfType(typeof(T)).Length > 1)
            {
                Debug.LogWarningFormat($"[Singleton] More than one Instance of '{typeof(T)}' exists. Destroying duplicated instances.");
            }

            return instance;
        }
    }

    public virtual void Awake()
    {
        GetInstance();
        if (instance && this != instance)
        {
            Debug.LogWarningFormat($"[Singleton] Destroying duplicated instances of '{typeof(T)}'.");
            Destroy(gameObject);
        }
    }
}
