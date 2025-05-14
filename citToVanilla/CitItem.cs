using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace citToVanilla
{
    internal class CitItem
    {
        public string type { get; set; }
        public string items { get; set; }
        public string model { get; set; }
        public string displayName { get; set; }
        public string displayNameRaw { get; set; }

        public string modelPath { get; set; }
        public string texturePath { get; set; }

        public string absoluteJsonPath { get; set; }
        public CitItem()
        {
        }
        public CitItem(string type, string items, string model, string displayName, string modelPath, string texturePaths, string absoluteJsonPath, string displayNameRaw)
        {
            this.type = type;
            this.items = items;
            this.model = model;
            this.displayName = displayName;
            this.modelPath = modelPath;
            this.texturePath = texturePaths;
            this.absoluteJsonPath = absoluteJsonPath;
            this.displayNameRaw = displayNameRaw;
        }

        public override string ToString()
        {
            return (
                "type: " + type + ", " +
                "items: " + items + ", " +
                "model: " + model + ", " +
                "displayName: " + displayName + ", " +
                "modelPath: " + modelPath + ", " +
                "texturePath: " + texturePath);
        }
    }
}
