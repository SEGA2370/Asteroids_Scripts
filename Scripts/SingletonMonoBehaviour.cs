using UnityEngine;

public class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    private static readonly object _lock = new object();
    private static bool _isApplicationQuitting = false;

    public static T Instance
    {
        get
        {
            if (_isApplicationQuitting)
            {
                return null;
            }

            lock (_lock)
            {
                if (_instance == null)
                {
                    // Look for an existing instance
                    _instance = FindObjectOfType<T>();

                    // If there is no existing instance, create a new GameObject with the singleton component
                    if (_instance == null)
                    {
                        GameObject singletonObject = new GameObject(typeof(T).Name);
                        _instance = singletonObject.AddComponent<T>();
                        DontDestroyOnLoad(singletonObject);
                    }
                    //else
                    //{
                    //    Debug.Log($"[SingletonMonoBehaviour] Using already created instance of '{typeof(T)}': {_instance.gameObject.name}.");
                    //}
                }

                return _instance;
            }
        }
    }

    // Ensure that the singleton instance is reset on application quit
    protected virtual void OnApplicationQuit()
    {
        _isApplicationQuitting = true;
    }

    // Reset the quitting flag when a new instance is created
    protected virtual void OnDestroy()
    {
        if (_instance == this)
        {
            _isApplicationQuitting = true;
        }
    }

    // Optional: Override Awake in subclasses to add initialization logic
    protected virtual void Awake()
    {
        // Prevent multiple instances from existing
        if (_instance == null)
        {
            _instance = this as T;
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }
}

