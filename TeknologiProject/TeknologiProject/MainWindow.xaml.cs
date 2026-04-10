using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TeknologiProject
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            //Regioner i Danmark, som kan bruges til at kategorisere afsender og modtager
            Region Midtjylland = new Region("Midtjylland");
            Region Hovedstaden = new Region("Hovedstaden");
            Region Nordjylland = new Region("Nordjylland");
            Region Syddanmark = new Region("Syddanmark");
            Region Sjaelland = new Region("Sjælland");

            List<Region> RegionList = new List<Region>();
            RegionList.Add(Midtjylland);
            RegionList.Add(Hovedstaden);
            RegionList.Add(Nordjylland);
            RegionList.Add(Syddanmark);
            RegionList.Add(Sjaelland);

            //Oprettelse af en afsender
            Sender sender = new Sender();
            sender.Name = "Philip Nord Nielsen";
            sender.City = "Aarhus";
            sender.Postalcode = 8000;
            sender.Region = Midtjylland;

            //Oprettelse af en modtager
            Receiver receiver = new Receiver();
            receiver.Name = "Mads Mikkelsen";
            receiver.City = "Copenhagen";
            receiver.Postalcode = 1000;
            receiver.Region = Hovedstaden;

            Package package = new Package();
            package.Sender = sender;
            package.Receiver = receiver;
            package.Size = PackageSize.Small;

            SortingManager sortingManager = new SortingManager();

            Truck truckMidt = new Truck();
            Truck truckHoved = new Truck();
            Truck truckSyd = new Truck();
            Truck truckNord = new Truck();
            Truck truckSjael = new Truck();

            //Liste der indeholder lastbilerne, som peger på hvilken region lastbilen skal køre til
            Dictionary<Region, Truck> regionTruckMap = new Dictionary<Region, Truck>();
            regionTruckMap.Add(Midtjylland, truckMidt);
            regionTruckMap.Add(Hovedstaden, truckHoved);
            regionTruckMap.Add(Syddanmark, truckSyd);
            regionTruckMap.Add(Nordjylland, truckNord);
            regionTruckMap.Add(Sjaelland, truckSjael);

            sortingManager.Sort(package, regionTruckMap);
        }
    }
}