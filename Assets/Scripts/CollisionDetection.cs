using System.Diagnostics;
using UnityEngine;
using System.Linq;

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
    public GameObject objectToSpawn;  // The object to spawn when a collision occurs
    private bool spawnNextFrame = false;  // Flag to spawn the object in the next frame
    private Vector3 spawnPosition;  // Position where the object will be spawned


    private void OnCollisionEnter(Collision collision)
    {
        UnityEngine.Debug.Log($"{collision.gameObject.name} enter collision.");
        

    }

    private void OnCollisionStay(Collision collision)
    {
        UnityEngine.Debug.Log($"{collision.gameObject.name} is collisioning.");
        // Get the position of the current object (the one with this script attached)
        Vector3 positionA = transform.position;

        // Get the position of the other object involved in the collision
        Vector3 positionB = collision.gameObject.transform.position;

        // Calculate the average position
        Vector3 averagePosition = (positionA + positionB) / 2;

        objectToSpawn = GameObject.FindGameObjectsWithTag("MergeObject1").First();
        //objectToSpawn.transform.position = averagePosition;
        // Calculate the average position between the two objects
        spawnPosition = (positionA + positionB) / 2;


        Vector3 viewPosition = Camera.main.WorldToViewportPoint(spawnPosition);
        Vector3 adjustedSpawnPosition = Camera.main.ViewportToWorldPoint(new Vector3(viewPosition.x, viewPosition.y, Camera.main.nearClipPlane + 2.0f));
        objectToSpawn.transform.position = spawnPosition;

        UnityEngine.Debug.Log("AR Camera Position: " + Camera.main.transform.position);
        UnityEngine.Debug.Log("Object Position: " + averagePosition);

    }

    private void OnCollisionExit(Collision collision)
    {
        UnityEngine.Debug.Log($"{collision.gameObject.name} exit collision.");
    }

    private void OnTriggerEnter(Collider other)
    {
        UnityEngine.Debug.Log($"Another object has entered the collider: {other}");
    }

    
}
