using System;
using System.Collections.Generic;
using System.Text;

namespace TeknologiProject
{
    public class PostalHub
    {
        public event Loghandler? OnLog;
        public event Packagedeliveredhandler? OnPackageDelivered;
        public event Action<int>? OnActivePostmenChanged;
        Func<PackageSize, int> CalculateWorktime = (package) =>
        {
            if (package == PackageSize.Small)
            {
                return 10000;
            }
            else if (package == PackageSize.Medium)
            {
                return 20000;
            }
            return 30000;
            
        };
        bool _isOperating = true;
        int _activePostmen = 0;
        object counterLock = new object();
        Mutex mutex = new Mutex();
        int _maximumPostmen = 3;
        Semaphore semaphore;
        SortingManager _sortingManager;
        Dictionary<Region, Truck> _trucks;
        public PostalHub(SortingManager sortingManager,Dictionary<Region,Truck> trucks)
        {
            _sortingManager = sortingManager;
            _trucks = trucks;
            Semaphore semaphore = new Semaphore(_maximumPostmen, _maximumPostmen);
        }
        public void SpawnPostman(int id)
        {
            Thread thread = new Thread(() => { Work(id); });   
            thread.IsBackground = true;
        }
        public void SetShutDown()
        {
            _isOperating = false;
        }
        public void Work(int id) 
        {
            while(true)
            {
                Package package = _sortingManager.TryTakePackageFromQueue();
                if (package == null)
                {
                    if(!_isOperating)
                    {
                        break;
                    }
                    Thread.Sleep(1000);
                    continue;
                }
                semaphore.WaitOne();
                lock (counterLock) 
                {
                    _activePostmen++;
                    OnActivePostmenChanged?.Invoke(_activePostmen);
                }
                if(package.Sender != null)
                {
                    OnLog?.Invoke($"Postman {id} picked up a package from {package.Sender.Name}");
                }
                else
                {
                    OnLog?.Invoke($"Postman {id} picked up a package from an unknown sender");
                }
                if (package.Receiver != null)
                {
                    OnLog?.Invoke($"Postman {id} is delivering a package to {package.Receiver.Name}");
                }
                else
                {
                    OnLog?.Invoke($"Postman {id} is delivering a package to an unknown receiver");
                }
                int workTime = 0;
                if( package.Size != null )
                {
                    workTime = CalculateWorktime(package.Size.Value);
                }
                else
                {
                    workTime = CalculateWorktime(PackageSize.Medium);
                }
                Thread.Sleep(workTime);
                mutex.WaitOne();
                Truck truck = _trucks[package.Receiver.Region];
                truck.Packages.Add(package);
                mutex.ReleaseMutex();
                OnPackageDelivered?.Invoke(package.Receiver.Region.Name, truck.Packages.Count);
                OnLog?.Invoke($"Postman {id} delivered a package to {package.Receiver.Name} in region {package.Receiver.Region.Name}");
                lock (counterLock)
                {
                    _activePostmen--;
                    OnActivePostmenChanged?.Invoke(_activePostmen);
                }
                semaphore.Release();

            }
            OnLog?.Invoke($"Postman {id} is shutting down.");
        }
    }
}
