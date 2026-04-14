using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;

namespace TeknologiProject
{
    public partial class MainWindow : Window
    {
        private readonly SortingManager _sortingManager = new SortingManager();
        private readonly List<Region> _regions = new List<Region>();
        private readonly Dictionary<Region, Truck> _regionTruckMap = new Dictionary<Region, Truck>();
        private readonly ObservableCollection<string> _logs = new ObservableCollection<string>();
        private readonly ObservableCollection<string> _truckOverview = new ObservableCollection<string>();
        private PostalHub postalHub = null!;
        private const int MaxPostmen = 3;
        private bool _workersStarted;

        public MainWindow()
        {
            InitializeComponent();
            InitializeUiData();

            _sortingManager.OnQueueChanged += () => Dispatcher.Invoke(() =>
            {
                QueueCounter.Text = _sortingManager.PackageQueue.Count.ToString();
                //if (_sortingManager.PackageQueue.Count != 0 && !_workersStarted)
                //{
                //    for (int i = 0; i < MaxPostmen; i++)
                //    {
                //        postalHub.SpawnPostman(i + 1);
                //    }

                //    _workersStarted = true;
                //}
            });

            postalHub.OnActivePostmenChanged += count => Dispatcher.Invoke(() => PostmanCounter.Text = count.ToString());

            postalHub.OnPackageDelivered += package =>
            {
                Dispatcher.Invoke(() =>
                {
                    _logs.Insert(0, $"✓ Pakke leveret til {package.Receiver.Region.Name} ({package.Size})");
                    UpdateTruckOverview();
                });
            };
            postalHub.OnLog += message => Dispatcher.Invoke(() => _logs.Insert(0, message));
            _sortingManager.OnLog += message => Dispatcher.Invoke(() => _logs.Insert(0, message));

        }

        private void InitializeUiData()
        {
            // Regioner i Danmark, som bruges til at kategorisere afsender og modtager
            var midtjylland = new Region("Midtjylland");
            var hovedstaden = new Region("Hovedstaden");
            var nordjylland = new Region("Nordjylland");
            var syddanmark = new Region("Syddanmark");
            var sjaelland = new Region("Sjælland");

            _regions.Add(midtjylland);
            _regions.Add(hovedstaden);
            _regions.Add(nordjylland);
            _regions.Add(syddanmark);
            _regions.Add(sjaelland);

            SenderRegionComboBox.ItemsSource = _regions;
            ReceiverRegionComboBox.ItemsSource = _regions;
            SenderRegionComboBox.DisplayMemberPath = "Name";
            ReceiverRegionComboBox.DisplayMemberPath = "Name";

            PackageSizeComboBox.ItemsSource = Enum.GetValues(typeof(PackageSize));

            // Liste over lastbiler, som peger på hvilken region lastbilen skal køre til
            _regionTruckMap.Add(midtjylland, new Truck());
            _regionTruckMap.Add(hovedstaden, new Truck());
            _regionTruckMap.Add(nordjylland, new Truck());
            _regionTruckMap.Add(syddanmark, new Truck());
            _regionTruckMap.Add(sjaelland, new Truck());

            LogListBox.ItemsSource = _logs;
            TruckOverviewListBox.ItemsSource = _truckOverview;

            _sortingManager.OnLog += message => Dispatcher.Invoke(() => _logs.Insert(0, message));

            UpdateTruckOverview();

            postalHub = new PostalHub(_sortingManager, _regionTruckMap);
            Thread.Sleep(1000);
        }


        private void CreateRandom()
        {
            for (int i = 0; i < 10; i++)
            {
                var random = new Random();
                var senderRegion = _regions[random.Next(_regions.Count)];
                var receiverRegion = _regions[random.Next(_regions.Count)];
                var packageSize = (PackageSize)random.Next(Enum.GetValues(typeof(PackageSize)).Length);
                var package = new Package
                {
                    Sender = new Sender
                    {
                        Name = $"Sender {i + 1}",
                        City = $"City {i + 1}",
                        Postalcode = random.Next(1000, 9999),
                        Region = senderRegion
                    },
                    Receiver = new Receiver
                    {
                        Name = $"Receiver {i + 1}",
                        Address = $"Address {i + 1}",
                        City = $"City {i + 1}",
                        Postalcode = random.Next(1000, 9999),
                        Region = receiverRegion
                    },
                    Size = packageSize
                };
                _sortingManager.AddPackageToQueue(package);
            }
        }


        private void SortPackageButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput(out var senderPostalCode, out var receiverPostalCode, out var senderRegion, out var receiverRegion, out var packageSize))
                return;

            // Oprettelse af afsender, modtager og pakke ud fra formularen
            var package = new Package
            {
                Sender = new Sender
                {
                    Name = SenderNameTextBox.Text,
                    City = SenderCityTextBox.Text,
                    Postalcode = senderPostalCode,
                    Region = senderRegion
                },
                Receiver = new Receiver
                {
                    Name = ReceiverNameTextBox.Text,
                    Address = ReceiverAddressTextBox.Text,
                    City = ReceiverCityTextBox.Text,
                    Postalcode = receiverPostalCode,
                    Region = receiverRegion
                },
                Size = packageSize
            };

            _sortingManager.Sort(package, _regionTruckMap);
            _logs.Insert(0, $"✓ Pakke sorteret til {receiverRegion.Name} ({packageSize})");
            UpdateTruckOverview();
            ClearForm();
        }

        private bool ValidateInput(out int senderPostalCode, out int receiverPostalCode, out Region senderRegion, out Region receiverRegion, out PackageSize packageSize)
        {
            senderPostalCode = 0;
            receiverPostalCode = 0;
            senderRegion = null!;
            receiverRegion = null!;
            packageSize = PackageSize.Small;

            if (!int.TryParse(SenderPostalCodeTextBox.Text, out senderPostalCode) ||
                !int.TryParse(ReceiverPostalCodeTextBox.Text, out receiverPostalCode))
            {
                MessageBox.Show("Postnummer skal være et tal.", "Ugyldigt input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (SenderRegionComboBox.SelectedItem is not Region sr ||
                ReceiverRegionComboBox.SelectedItem is not Region rr ||
                PackageSizeComboBox.SelectedItem is not PackageSize ps)
            {
                MessageBox.Show("Vælg region og pakkestørrelse.", "Manglende input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            senderRegion = sr;
            receiverRegion = rr;
            packageSize = ps;
            return true;
        }

        private void UpdateTruckOverview()
        {
            _truckOverview.Clear();

            foreach (var entry in _regionTruckMap)
            {
                _truckOverview.Add($"{entry.Key.Name}: {entry.Value.Packages.Count} pakke(r)");
            }
        }

        private void ClearForm()
        {
            SenderNameTextBox.Clear();
            SenderCityTextBox.Clear();
            SenderPostalCodeTextBox.Clear();
            SenderRegionComboBox.SelectedIndex = -1;
            ReceiverNameTextBox.Clear();
            ReceiverAddressTextBox.Clear();
            ReceiverCityTextBox.Clear();
            ReceiverPostalCodeTextBox.Clear();
            ReceiverRegionComboBox.SelectedIndex = -1;
            PackageSizeComboBox.SelectedIndex = -1;
        }

        private void StartUI(object sender, RoutedEventArgs e)
        {
            CreateRandom();

            //if (_workersStarted)
            //{
            //    return;
            //}

            for (int i = 0; i < MaxPostmen; i++)
            {
                postalHub.SpawnPostman(i + 1);
            }

            //_workersStarted = true;
        }           
    }
}
