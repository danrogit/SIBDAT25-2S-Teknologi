using System;
using System.Collections.Generic;
using System.Text;

namespace TeknologiProject
{
    public class SortingManager
    {
        public string Name;
        public bool IsWorking = false;
        public int ID;

        //Metode til sortering af pakker i lastbiler baseret på regioner
        public void Sort(Package package, Dictionary<Region, Truck> regionTruckMap)
        {
            if (package == null) return;
            if (package.Receiver == null) return;

            Region region = package.Receiver.Region;
           if (regionTruckMap.ContainsKey(region))
           {
               regionTruckMap[region].Packages.Add(package);
           }
        }
    }
}
