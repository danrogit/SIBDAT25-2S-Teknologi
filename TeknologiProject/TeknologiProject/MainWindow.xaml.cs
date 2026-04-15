using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
        private PostalHub postalHub;

        public MainWindow()
        {
            InitializeComponent();
            InitializeUiData();


			postalHub = new PostalHub(_sortingManager, _regionTruckMap);

			_sortingManager.OnLog += message => Dispatcher.Invoke(() => _logs.Insert(0, message));
			_sortingManager.OnQueueChanged += () => Dispatcher.Invoke(() =>
            {
                QueueCounter.Text = $"{_sortingManager.PackageQueue.Count.ToString()}";
			});

            postalHub.OnActivePostmenChanged += count =>
            {
                Dispatcher.Invoke(() =>
                {
                    PostmanCounter.Text = $"{count.ToString()} / {postalHub.MaximumWorkingPostmen}";
                });
            };

            postalHub.OnPackageDelivered += (package) =>
            {
                Dispatcher.Invoke(() =>
                {
                    _logs.Insert(0, $"[Lastbil] Pakke til {package.Receiver.Name} i {package.Receiver.Region.Name} med størrelse ({package.Size}) er afleveret i lastbil");
                    UpdateTruckOverview();
                });
            };
            postalHub.OnLog += message =>
            {
                Dispatcher.Invoke(() =>
                {
                    _logs.Insert(0, message);
                });
            };
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

            UpdateTruckOverview();

            Thread.Sleep(1000);
        }


        private void CreateRandomPackages()
        {
            // 20 pakker
            for (int i = 0; i < 20; i++)
            {
                string[] firstnames = new string[]{ "Erik", "Maria", "Lise", "Signe", "Lucas", "Emma", "Ida", "Josefine", "Johannes", "Marc", "Nina", "Lotte", "Mathias", "Christian", "Mathilde", "Anton", "August", "Sofia", "Emma", "Magnus", "Felix", "Olivia", "Clara", "Oscar", "Nora", "Elias", "Freeja", "William"};
                string[] lastnames = { "Hansen", "Jensen", "Nielsen", "Pedersen", "Andersen", "Christensen", "Larsen", "Rasmussen", "Jørgensen", "Sørensen", "Olsen", "Knudsen", "Thomsen", "Poulsen", "Petersen" };
                Random random = new Random();
                string senderFirstname = firstnames[random.Next(0, (firstnames.Length - 1))];
                string senderLastname = lastnames[random.Next(0, (lastnames.Length - 1))];
                string receiverFirstname = firstnames[random.Next(0, (firstnames.Length - 1))];
                string receiverLastname = lastnames[random.Next(0, (lastnames.Length - 1))];
                Region senderRegion = _regions[random.Next(_regions.Count)];
                Region receiverRegion = _regions[random.Next(_regions.Count)];
                PackageSize packageSize = (PackageSize)random.Next(Enum.GetValues(typeof(PackageSize)).Length);
                Package package = new Package
                {
                    Sender = new Sender
                    {
                        Name = $"{senderFirstname} {senderLastname}",
                        City = $"City {i + 1}",
                        Postalcode = random.Next(1000, 9999),
                        Region = senderRegion
                    },
                    Receiver = new Receiver
                    {
						Name = $"{receiverFirstname} {receiverLastname}",
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
            Package package = new Package
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

            _sortingManager.AddPackageToQueue(package);
            _logs.Insert(0, $"[Kø] Ny pakke i køen til {package.Receiver.Name} i {receiverRegion.Name} med størrelsen ({packageSize})");
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
            CreateRandomPackages();

            for (int i = 0; i < 6; i++)
            {
                postalHub.SpawnPostman(i + 1);
            }

            postalHub.SetShutDown();
        }           
    }
}
