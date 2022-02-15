using UnityEngine;

namespace AirDriVR
{
    public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
 
        private static object _lock = new object();
 
        public static T Instance
        {
            get
            {
                lock(_lock)
                {
                    if (_instance == null)
                    {
                        if ( FindObjectsOfType(typeof(T)).Length > 1 )
                        {
                            Debug.LogError("[Singleton] Something went really wrong " +
                                           " - there should never be more than 1 singleton!" +
                                           " Reopening the scene might fix it.");
                            return _instance;
                        }
                        
                        _instance = (T) FindObjectOfType(typeof(T));
 
                        if (_instance == null)
                        {
                            var singleton = new GameObject();
                            _instance = singleton.AddComponent<T>();
                            singleton.name = typeof(T).ToString() + " [Auto-Generated]";
 
                            DontDestroyOnLoad(singleton);
 
                            Debug.Log("[Singleton] An instance of " + typeof(T) + 
                                      " is needed in the scene, so '" + singleton +
                                      "' was created with DontDestroyOnLoad.");
                        } else {
                            Debug.Log("[Singleton] Using instance already created: " +
                                      _instance.gameObject.name);
                        }
                    }
 
                    return _instance;
                }
            }
        }
    }
}