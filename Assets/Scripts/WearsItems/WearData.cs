using System;

namespace WearsItems
{
    [Serializable]
    public class WearData
    {
        public string Name;
        public string Materials;
        public byte[] Photo;

        public WearData(string name, string materials, byte[] photo)
        {
            Name = name;
            Materials = materials;
            Photo = photo;
        }
    }
}