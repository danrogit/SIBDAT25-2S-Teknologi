using System;
using System.Collections.ObjectModel;
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

        public MainWindow()
        {
            InitializeComponent();
            InitializeUiData();
        }

        private void InitializeUiData()
        {
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

            _regionTruckMap.Add(midtjylland, new Truck());
            _regionTruckMap.Add(hovedstaden, new Truck());
            _regionTruckMap.Add(nordjylland, new Truck());
            _regionTruckMap.Add(syddanmark, new Truck());
            _regionTruckMap.Add(sjaelland, new Truck());

            LogListBox.ItemsSource = _logs;
            TruckOverviewListBox.ItemsSource = _truckOverview;

            _sortingManager.OnLog += message => Dispatcher.Invoke(() => _logs.Insert(0, message));

            UpdateTruckOverview();
        }

        private void SortPackageButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput(out var senderPostalCode, out var receiverPostalCode, out var senderRegion, out var receiverRegion, out var packageSize))
                return;

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
            senderRegion = null;
            receiverRegion = null;
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
    }
}
