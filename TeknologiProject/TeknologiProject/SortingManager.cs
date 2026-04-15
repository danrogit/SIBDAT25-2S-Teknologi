using System;
using System.Collections.Generic;
using System.IO.Packaging;
using System.Text;

namespace TeknologiProject
{
    public class SortingManager
    {
        public event Loghandler? OnLog; //Event til logning af beskeder
        public event Action? OnQueueChanged; //Event til opdatering af kø-størrelse, sender den nye størrelse som parameter
        public Queue<Package> PackageQueue = new Queue<Package>(); //Kø til pakker, som skal sorteres og leveres
        object _lock = new object(); //Lock objekt til at sikre trådsikker adgang til køen, så flere tråde ikke kan ændre køen samtidigt
        public string Name; //Navn på sorteringsmanageren, som kan bruges til at identificere ham i logs og UI
        public bool IsWorking = false; //Indikator for om sorteringsmanageren er i gang med at arbejde
        public int ID; //Unikt ID for sorteringsmanageren, som kan bruges til at identificere ham i logs og UI

        // Legacy funktion - bruges ikke længere da sorteringen nu foregår i WORK funktionen i postalhub med threads
        //Metode til sortering af pakker i lastbiler baseret på regioner
        //public void Sort(Package package, Dictionary<Region, Truck> regionTruckMap)
        //{
        //    if (package == null) return; //Tjek om pakken er null, hvis ja returner uden at gøre noget
        //    if (package.Receiver == null) return; //Tjek om pakken har en modtager, hvis ikke returner uden at gøre noget
        //    Region region = package.Receiver.Region; 
        //    if (regionTruckMap.ContainsKey(region)) //Tjek om der allerede er en lastbil til den pågældende region i regionTruckMap
        //    {
        //        regionTruckMap[region].Packages.Add(package); //Hvis ja, tilføj pakken til den eksisterende lastbil for den region
        //    }
        //}
        ////Tilføjelse af pakker til sorteringskøen

        public void AddPackageToQueue(Package package)
        {
            lock (_lock) //Sikkerhed for at flere tråde ikke tilføjer pakker til køen samtidigt
            {
                PackageQueue.Enqueue(package);
            }
            OnQueueChanged?.Invoke(); //Opdatering af kø-størrelse til event handlers
            OnLog?.Invoke($"[Kø] Pakke tilføjet til køen. Køens størrelse er nu: {PackageQueue.Count}"); //Logning af tilføjelse af pakke til køen
        }

        public Package TryTakePackageFromQueue() //Forsøg på at tage en pakke fra køen, returnerer null hvis køen er tom
        {
            lock (_lock) //Sikre at flere tråde ikke forsøger at tage pakker fra køen samtidigt
            {
                if (PackageQueue.Count == 0) //Tjek om køen er tom, hvis ja returner null
                {
                    return null;
                }
                Package package = PackageQueue.Dequeue(); //Tag en pakke fra køen
                OnQueueChanged?.Invoke(); //Opdatering af kø-størrelse til event handlers
                OnLog?.Invoke($"[Kø] Pakke taget fra køen. Køens størrelse er nu: {PackageQueue.Count}"); //Logning af fjernelse af pakke fra køen
                return package; //Returner den tagne pakke
            }
        }
    }
}