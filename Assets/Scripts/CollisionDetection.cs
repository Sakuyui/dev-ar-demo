using System.Diagnostics;
using UnityEngine;
using System.Linq;
using AR.ActivationControl;
using System.Collections.Generic;

public class ImageTargetMapping
{
    private static ImageTargetMapping _Instance;
    public static ImageTargetMapping Instance
    {
        get { 
            if(_Instance == null)
            {
                _Instance = new ImageTargetMapping();
            }
            return _Instance;
        }
        set { }
    }

    public Dictionary<string, int> Mapping { get; private set; } = new Dictionary<string, int>();
    public void AddMapping(string ImageTargetName, int index)
    {
        Mapping.Add(ImageTargetName, index);
    }
    static ImageTargetMapping()
    {
        Instance.AddMapping("ImageTarget-idback", 0);
        Instance.AddMapping("ImageTarget-namecard", 1);
    }
    public int this[string ImageTargetName]
    {
        get { return Instance.Mapping[ImageTargetName]; }
        set { Instance.Mapping[ImageTargetName] = value; }
    }
}

public class CollisionDetection : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UnityEngine.Debug.Log("Start collision detection script.");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        UnityEngine.Debug.Log($"{collision.gameObject.name} enter collision.");
        ObjectActivationControl.AddCollision(ImageTargetMapping.Instance[gameObject.name], ImageTargetMapping.Instance[collision.gameObject.name]);
    }

    private void OnCollisionStay(Collision collision)
    {

    }

    private void OnCollisionExit(Collision collision)
    {
        UnityEngine.Debug.Log($"{collision.gameObject.name} exit collision.");
        ObjectActivationControl.RemoveCollision(ImageTargetMapping.Instance[gameObject.name], ImageTargetMapping.Instance[collision.gameObject.name]);
    }

    private void OnTriggerEnter(Collider other)
    {
        UnityEngine.Debug.Log($"Another object has entered the collider: {other}");
    }

    
}
