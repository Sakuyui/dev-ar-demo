using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace AR.ActivationControl {
    public class ActivationConfiguration
    {
        public record ActivationState
        {
            public bool activated = false;
        }
        public Dictionary<string, ActivationState> configuration { get; private set; }
        public ActivationConfiguration(string[] controlObjectNames)
        {
            this.configuration = controlObjectNames.Select(x => x).ToDictionary(x => x, x => new ActivationState());
        }
        public void setActivationState(string objectName, bool isActivated)
        {
            if (configuration.ContainsKey(objectName))
            {
                UnityEngine.Debug.Log($"Set {objectName} to " + (isActivated ? "active": "inactive") + " in configuration.");
                configuration[objectName].activated = isActivated;
            }
        }
        public bool getActivationState(string objectName)
        {
            return configuration[objectName].activated;
        }
    }

    public class ObjectActivationControl : MonoBehaviour
    {
        public static ActivationConfiguration activationConfiguration { get; private set; } = 
            new(new[] {"DuckBigger", "Box", "DuckNormal"} );

        public static Dictionary<string, GameObject> gameObjectCaching = new Dictionary<string, GameObject>();

        public static void UpdateConfiguration(string objectName, bool isActivated)
        {
            activationConfiguration.setActivationState(objectName, isActivated);
        }

        private GameObject FindFirstObjectWithTagName(string tagName)
        {
            if (gameObjectCaching.ContainsKey(tagName))
            {
                return gameObjectCaching[tagName];
            }
            foreach(var obj in (GameObject.FindObjectsOfType(typeof(GameObject)) as GameObject[])){
                if (obj.tag == tagName)
                {
                    gameObjectCaching[tagName] = obj;
                    return obj;
                }
            }
            return null;
        }

        void UpdateActivationStates()
        {
            foreach (var name in activationConfiguration.configuration.Keys)
            {                
                var gameObject = FindFirstObjectWithTagName (name);
                if (gameObject == null)
                {
                    UnityEngine.Debug.LogWarning($"Cannot find gameObject with tag {name}.");
                    continue;
                }
                gameObject.SetActive(activationConfiguration.getActivationState(name));
            }
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is 
        void Start()
        {
            UpdateActivationStates();
        }

        // Update is called once per frame
        void Update()
        {
            UpdateActivationStates();
        }
    }
}
