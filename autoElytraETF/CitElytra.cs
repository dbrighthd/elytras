using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace autoElytraETF
{
    internal class CitElytra
    {
        public string displayName;
        public string displayNameRaw;
        public string texturePath;
        //public int id;

        public CitElytra(string displayName, string texturePath, string displayNameRaw)
        {
            this.displayName = displayName;
            //this.id = id;
            this.texturePath = texturePath;
            this.displayNameRaw = displayNameRaw;
        }
    }
}
