using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace AR.ActivationControl {

    public class AR3DObjectStorage
    {
        private Dictionary<string, int> objectIndexMap = new Dictionary<string, int>();
        private Dictionary<int, string> reversedObjectIndexMap = new Dictionary<int, string>();
        private AR3DObjectStorage() { }

        private static AR3DObjectStorage _Instance = null;
        public static AR3DObjectStorage Instance {
            get {
                if (_Instance == null)
                {
                    _Instance = new AR3DObjectStorage();
                }
                return _Instance;
            }
            private set { }
        }

        public void RecordObjectStateIndex(string objectName, int index)
        {
            objectIndexMap[objectName] = index;
            reversedObjectIndexMap[index] = objectName;
        }

        public bool HasObject(string objectName)
        {
            return objectIndexMap.ContainsKey(objectName);
        }
        
        public int this[string objectName]
        {
            get {  return objectIndexMap[objectName]; }
            set { objectIndexMap[objectName] = value; }
        }
        public string this[int index] {
            get { return reversedObjectIndexMap[index]; }
            set { reversedObjectIndexMap[index] = value; }
        }
    }

    public class ActivationConfiguration
    {
        bool dirty = true;
        string cachedEncodedConfiguration = "";
        private List<ActivationState> configuration = new List<ActivationState>();
        private static ActivationConfiguration _Instance = null;
        private ActivationConfiguration() { }

        public static ActivationConfiguration Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = new ActivationConfiguration();
                }
                return _Instance;
            }
            set { _Instance = value; }           
        }

        public record ActivationState
        {
            public bool activated = false;
        }

        public void RecordNewObject(string objectName, bool isActivated = false)
        {
            dirty = true;
            if (AR3DObjectStorage.Instance.HasObject(objectName))
            {
                UnityEngine.Debug.LogWarning($"Object {objectName} already exists in the 'AR3DObjectStorage'.");
                return;
            }
            AR3DObjectStorage.Instance.RecordObjectStateIndex(objectName, configuration.Count());
            configuration.Add(new ActivationState { activated = isActivated});
        }

        public void SetActivationState(string objectName, bool isActivated)
        {
            UnityEngine.Debug.Log($"Try setting {objectName} to " + (isActivated ? "active" : "inactive") + " in configuration.");

            dirty = true;
            if (AR3DObjectStorage.Instance.HasObject(objectName))
            {
                UnityEngine.Debug.Log($"Set {objectName} to " + (isActivated ? "active": "inactive") + " in configuration.");
                configuration[AR3DObjectStorage.Instance[objectName]].activated = isActivated;
            }
        }

        public void SetActivationState(int index, bool isActivated)
        {
            dirty = true;
            configuration[index].activated = isActivated;
        }
        
        public bool GetActivationState(string objectName)
        {
            return configuration[AR3DObjectStorage.Instance[objectName]].activated;
        }

        public string EncodeConfiguration()
        {
            if (!dirty)
            {
                return cachedEncodedConfiguration;
            }
            StringBuilder sb = new StringBuilder();
            foreach (ActivationState state in configuration)
            {
                sb.Append(state.activated ? "1" : "0");
            }
            cachedEncodedConfiguration = sb.ToString();
            dirty = false;
            return cachedEncodedConfiguration;
        }

    }

    public class CollisionConfiguration
    {
        private static CollisionConfiguration _Instance;
        public static HashSet<int> collapsing = new HashSet<int>();
        public static CollisionConfiguration Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = new CollisionConfiguration();
                }
                return _Instance;
            }
            private set { }
        }

        public bool ConnectionCheck(IEnumerable<int> indexes)
        {
            List<int> indexList = indexes.ToList();
            if(indexes.Count() < 2)
            {
                return false;
            }
            List<List<int>> connectionGraph = Enumerable.Repeat(0, indexes.Count()).Select(_ => new List<int>()).ToList();
            for(int i = 0; i < indexes.Count(); i++)
            {
                for(int j = i + 1; j < indexes.Count(); j++)
                {
                    if(i == j)
                    {
                        continue;
                    }
                    if(IsCollapsing(indexList[i], indexList[j]))
                    {
                        connectionGraph[i].Add(j);
                        connectionGraph[j].Add(i);
                    }
                }
            }

            var visited = new HashSet<int>();
            var queue = new Queue<int>();
            queue.Enqueue(0);
            while (queue.Any())
            {
                var nodeId = queue.Dequeue();
                List<int> children = connectionGraph[nodeId];
                foreach(int child in children)
                {
                    if (visited.Contains(child))
                    {
                        continue;
                    }
                    else
                    {
                        queue.Enqueue(child);
                    }
                }
            }
            return visited.Count == indexes.Count();
        }

        private CollisionConfiguration() { 

        }

        public void AddCollision(int objectIndex1, int objectIndex2)
        {
            UnityEngine.Debug.Log($"set obj:{objectIndex1} and obj:{objectIndex2} collided.");
            collapsing.Add((objectIndex1 << 16) | objectIndex2);
            collapsing.Add((objectIndex2 << 16) | objectIndex1);
        }

        public void AddCollision(string objectName1, string objectName2)
        {
            int objectIndex1 = AR3DObjectStorage.Instance[objectName1];
            int objectIndex2 = AR3DObjectStorage.Instance[objectName2];
            AddCollision(objectIndex1, objectIndex2);
        }

        public void RemoveCollision(int objectIndex1, int objectIndex2) {
            collapsing.Remove((objectIndex1 << 16) | objectIndex2);
            collapsing.Remove((objectIndex2 << 16) | objectIndex1);
            UnityEngine.Debug.Log($"unset obj:{objectIndex1} and obj:{objectIndex2} collided.");
        }

        public void RemoveCollision(string objectName1, string objectName2)
        {
            int objectIndex1 = AR3DObjectStorage.Instance[objectName1];
            int objectIndex2 = AR3DObjectStorage.Instance[objectName2];
            RemoveCollision(objectIndex1, objectIndex2);
        }

        public bool IsCollapsing(int objectIndex1, int objectIndex2)
        {
            return collapsing.Contains((objectIndex1 << 16) | objectIndex2) || collapsing.Contains((objectIndex2 << 16) | objectIndex1);
        }
    }

    public class ObjectActivationControl : MonoBehaviour
    {
        public bool enableDistanceControl = false;
        public static ActivationConfiguration activationConfiguration { get; private set; } = ActivationConfiguration.Instance;
        public static CollisionConfiguration collisionConfiguration { get; private set; } = CollisionConfiguration.Instance;
        public static Dictionary<string, GameObject> gameObjectCaching = new Dictionary<string, GameObject>();

        private static Dictionary<int, List<List<int>>> connectionRecognizationConfiguration = new Dictionary<int, List<List<int>>>();


        
        static ObjectActivationControl() {
            string objectConfigurationPath = "Assets/Configurations/objects.config";
            string connectionTrackingConfigurationPath = "Assets/Configurations/connectionRecognization.config";
            string[] objectNames = File.ReadAllLines(objectConfigurationPath).Select(line => line.Trim()).ToArray();

            List<List<int>> ParseStateSetupConfigurationLineItems(string items)
            {
                int parsingState = 0;
                List<List<int>> result = new List<List<int>>();
                List<int> currentParsingConfiguration = new List<int>();
                StringBuilder currentParsingString = new StringBuilder();
                for (int i = 0; i < items.Length; i++)
                {
                    char ch = items[i];
                    if (parsingState == 0)
                    {
                        if (ch == '[') { 
                                parsingState = 1;
                                currentParsingConfiguration = new List<int>();
                                break;
                        }
                    }
                    else if (parsingState == 1)
                    {
                        if (ch > '0' && ch < '9')
                        {
                            currentParsingString.Append(ch);
                        }else if (ch == ',')
                        {
                            currentParsingConfiguration!.Add(int.Parse(currentParsingString.ToString()));
                            currentParsingString.Clear();
                        }else if (ch == ']')
                        {
                            currentParsingConfiguration!.Add(int.Parse(currentParsingString.ToString()));
                            currentParsingString.Clear();
                            result.Add(currentParsingConfiguration);
                            currentParsingConfiguration = null;
                            parsingState = 0;
                        }
                    }
                }
                return result;

            };

            connectionRecognizationConfiguration = 
                File.ReadAllLines(connectionTrackingConfigurationPath).Select(line =>
                {
                    int firstSpacePosition = line.ToString().IndexOf(" ");
                    var key = line[..firstSpacePosition];
                    var value = ParseStateSetupConfigurationLineItems(line[(firstSpacePosition + 1)..]);
                    return (key: int.Parse(key), value: value);
                }).ToDictionary(kv => kv.key, kv => kv.value);

            foreach (string objectName in objectNames) {
                activationConfiguration.RecordNewObject(objectName, false);
            }

            // connectionRecognizationConfiguration.Add(2, new List<List<int>>() { new List<int>() { 0, 1 } });
        }

        public static void UpdateConfiguration(string objectName, bool isActivated)
        {
            activationConfiguration.SetActivationState(objectName, isActivated);
        }

        public static void AddCollision(int object1Index, int object2Index)
        {
            collisionConfiguration.AddCollision(object1Index, object2Index);
        }

        public static void RemoveCollision(int object1Index, int object2Index)
        {
            collisionConfiguration.RemoveCollision(object1Index, object2Index);
        }

        private GameObject FindFirstObjectWithTagName(string tagName)
        {
            if (tagName == null) return null;
            if (gameObjectCaching.ContainsKey(tagName))
            {
                return gameObjectCaching[tagName];
            }
            foreach (var obj in (GameObject.FindObjectsOfType(typeof(GameObject)) as GameObject[])) {
                if (obj.tag == tagName)
                {
                    gameObjectCaching[tagName] = obj;
                    return obj;
                }
            }
            return null;
        }

        public void AlignToConfiguration(string configuration)
        {
            UnityEngine.Debug.Log($"Align To {configuration}");

            for (int i = 0; i < configuration.Length; i++)
            {
                char c = configuration[i];
                bool active = c == '1' ? true : false;
                activationConfiguration.SetActivationState(i, active);
                string objectName = AR3DObjectStorage.Instance[i];
                UnityEngine.Debug.Log($" - Object Name = {objectName}");
                GameObject gameObject = FindFirstObjectWithTagName(objectName);
                gameObject.SetActive(active);
            }
        }


        void UpdateActivationStates()
        {
            var configuration = activationConfiguration.EncodeConfiguration();
            UnityEngine.Debug.Log($" - configuration = {configuration}");

            /* create a configuration file. describe what should us to in each configuration. */
            var newConfiguration = new StringBuilder(configuration);
            foreach(int bitId in connectionRecognizationConfiguration.Keys)
            {
                var connectionLists = connectionRecognizationConfiguration[bitId];
                bool connected = false;
                foreach(var connectionList in connectionLists)
                {
                    if (collisionConfiguration.ConnectionCheck(connectionList))
                    {
                        connected = true;
                        break;
                    }
                }
                if (bitId >= 0 && bitId < newConfiguration.Length)  // Added bounds check for safety
                {
                    newConfiguration[bitId] = connected ? '1' : '0';
                }
                else
                {
                    UnityEngine.Debug.LogError($"bitId {bitId} is out of range.");
                }
            }

            AlignToConfiguration(newConfiguration.ToString());
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