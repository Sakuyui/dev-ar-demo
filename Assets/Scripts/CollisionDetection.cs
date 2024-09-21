using System.Diagnostics;
using UnityEngine;
using System.Linq;
using AR.ActivationControl;
using System.Collections.Generic;
using System.IO;

namespace AR
{
    

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
        }


    }
}