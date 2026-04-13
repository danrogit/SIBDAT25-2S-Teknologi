using System;
using System.Collections.Generic;
using System.IO.Packaging;
using System.Text;

namespace TeknologiProject
{
    public class SortingManager
    {
        public event Loghandler? OnLog;
        public event Action<int>? OnQueueChanged;
        public Queue<Package> PackageQueue = new Queue<Package>();
        object _lock = new object();
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
        public void AddPackageToQueue(Package package)
        {
            lock (_lock)
            {
                PackageQueue.Enqueue(package);
            }
            OnQueueChanged?.Invoke(PackageQueue.Count);
            OnLog?.Invoke($"Package added to queue. Queue size: {PackageQueue.Count}");
        }
        public Package TryTakePackageFromQueue()
        {
            lock (_lock)
            {
                if (PackageQueue.Count == 0)
                {
                    return null;
                }
                Package package = PackageQueue.Dequeue();
                OnQueueChanged?.Invoke(PackageQueue.Count);
                OnLog?.Invoke($"Package taken from queue. Queue size: {PackageQueue.Count}");
                return package;
            }
        }
    }
}