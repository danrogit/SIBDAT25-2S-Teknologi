using System;
using System.Collections.Generic;
using System.Text;

namespace TeknologiProject
{
    public class PostalHub
    {
        public event Loghandler? OnLog;
        public event Action<Package>? OnPackageDelivered;
        public event Action<int>? OnActivePostmenChanged;

        //Beregner arbejdstid for postmanden baseret på pakkens størrelse
        Func<PackageSize, int> CalculateWorktime = (package) =>
        {
            if (package == PackageSize.Small) //Hvis pakken er lille, tager det 10 sekunder at levere
            {
                return 10000;
            }
            else if (package == PackageSize.Medium) //Hvis pakken er medium, tager det 20 sekunder at levere
            {
                return 20000;
            }
            return 30000; //Hvis pakken er stor, tager det 30 sekunder at levere
            
        };
        bool _isOperating = true; //Styring af systemets tilstand
        public int ActivePostmen = 0; //Antal aktive postmænd
        object counterLock = new object(); //Lock til sikker opdatering af counter
        Mutex mutex = new Mutex(); // Mutex til beskyttelse af data
        public int MaximumPostmen = 3; // Max antal postmænd
        Semaphore semaphore; //Semaphore bruger vi til at begrænse antal tråde
        SortingManager _sortingManager; // Instantiere en person (Sortingmanager)
        Dictionary<Region, Truck> _trucks; // Bruges til at koble truck og region sammen
        public PostalHub(SortingManager sortingManager,Dictionary<Region,Truck> trucks)
        {
            _sortingManager = sortingManager;
            _trucks = trucks;
            semaphore = new Semaphore(0, MaximumPostmen);
        }

        //Opretter og starter en tråd for hver postmand, som skal udføre arbejdet i Work-metoden
        public void SpawnPostman(int id)
        {
            Thread thread = new Thread(() => { Work(id); });   
            thread.IsBackground = true;
            thread.Start();

            OnLog?.Invoke($"Postman {id} has started working.");
        }

        //Stopper systemet
        public void SetShutDown()
        {
            _isOperating = false;
        }

        public void AvoidShutDown()
        {
            _isOperating = true;
        }

        //Metode som hver postmandstråd kører, hvor de henter pakker fra sorteringsmanageren og leverer dem til lastbilerne
        public void Work(int id) 
        {
            while(true)
            {
                Package package = _sortingManager.TryTakePackageFromQueue(); //Henter pakke fra kø
                if (package == null)
                {
                    if(!_isOperating) // Hvis systemet ikke længere er i drift og der ikke er flere pakker, kan tråden afslutte
                    {
                        break;
                    }
                    Thread.Sleep(1000);
                    continue;
                }
                semaphore.WaitOne(); //Begræns antallet af aktive postmænd
                lock (counterLock) //Sikker opdatering af counter
                {
                    ActivePostmen++;
                    OnActivePostmenChanged?.Invoke(ActivePostmen);
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
                int workTime = 0; //Beregner arbejdstid baseret på pakkens størrelse, hvis størrelsen ikke er angivet, antages det at være medium
                if ( package.Size != null )
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
                OnPackageDelivered?.Invoke(package);
                OnLog?.Invoke($"Postman {id} delivered a package to {package.Receiver.Name} in region {package.Receiver.Region.Name}");
                lock (counterLock)
                {
                    ActivePostmen--;
                    OnActivePostmenChanged?.Invoke(ActivePostmen);
                }
                semaphore.Release();

            }
            OnLog?.Invoke($"Postman {id} is shutting down.");
        }
    }
}
