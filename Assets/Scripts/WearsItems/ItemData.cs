using System;
using System.Collections.Generic;

namespace WearsItems
{
    [Serializable]
    public class ItemData
    {
        public string SectionName;
        public string CategoryName;
        public string TimeOfDay;
        public List<WearData> WearDatas;

        public ItemData(string sectionName, string categoryName, string timeOfDay, List<WearData> wearDatas)
        {
            SectionName = sectionName;
            CategoryName = categoryName;
            TimeOfDay = timeOfDay;
            WearDatas = wearDatas;
        }
    }
}