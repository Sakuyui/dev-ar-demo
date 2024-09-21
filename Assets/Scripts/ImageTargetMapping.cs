using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;

namespace AR
{
    public class ImageTargetMapping
    {
        private static ImageTargetMapping _Instance;
        public static ImageTargetMapping Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = new ImageTargetMapping();
                }
                return _Instance;
            }
            set { }
        }

        public Dictionary<string, int> Mapping { get; private set; }
        public static string ImageTargetPath = "./Assets/Configurations/image_target_mapping.config";
        public void AddMapping(string ImageTargetName, int index)
        {
            Mapping.Add(ImageTargetName, index);
        }
        static ImageTargetMapping()
        {
            string[] lines = File.ReadAllLines(ImageTargetPath);
            Instance.Mapping = lines.Select(line => line.Trim().Split(" ")).ToDictionary(kv => kv[0], kv => int.Parse(kv[1]));
            //Instance.AddMapping("ImageTarget-idback", 0);
            //Instance.AddMapping("ImageTarget-namecard", 1);
        }
        public int this[string ImageTargetName]
        {
            get { return Instance.Mapping[ImageTargetName]; }
            set { Instance.Mapping[ImageTargetName] = value; }
        }
    }
}