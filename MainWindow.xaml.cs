using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Diagnostics;

namespace UsersSupport
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void LoadList()
        {
            DataAPI api = null;
            CollectionViewSource _itemSourceList = null;
            ICollectionView Itemlist = null;

            BackgroundWorker worker = new BackgroundWorker();

            //this is where the long running process should go
            worker.DoWork += (o, ea) =>
            {
                //no direct interaction with the UI is allowed from this method
                api = new DataAPI();

                // Collection which will take your ObservableCollection
                _itemSourceList = new CollectionViewSource() { Source = api.GetComputers() };

                // ICollectionView the View/UI part 
                Itemlist = _itemSourceList.View;

                Itemlist.SortDescriptions.Add(new SortDescription("lastLogon", ListSortDirection.Descending));

                /*/ your Filter
                var filter = new Predicate<object>(item => ((Model)item).Name.Contains("Max"));

                //now we add our Filter
                Itemlist.Filter = yourCostumFilter;*/
            };
            worker.RunWorkerCompleted += (o, ea) =>
            {
                //work has completed. you can now interact with the UI
                ComputersGrid.ItemsSource = Itemlist;
                _busyIndicator.IsBusy = false;
            };

            //set the IsBusy before you start the thread
            _busyIndicator.BusyContent = "Mise à jour de la liste des ordinateurs...";
            _busyIndicator.IsBusy = true;
            worker.RunWorkerAsync();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            this.LoadList();
        }

        private void ComputersGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Computer computer = (Computer)ComputersGrid.SelectedItem;

            computer.OpenTightVNC();
        }

        private void MenuItem_CopyIP(object sender, RoutedEventArgs e)
        {
            Computer computer = (Computer)ComputersGrid.SelectedItem;

            Clipboard.SetData(DataFormats.Text, computer.ip);
        }

        private void MenuItem_CopyName(object sender, RoutedEventArgs e)
        {
            Computer computer = (Computer)ComputersGrid.SelectedItem;

            Clipboard.SetData(DataFormats.Text, computer.name);
        }

        private void MenuItem_Connect(object sender, RoutedEventArgs e)
        {
            Computer computer = (Computer)ComputersGrid.SelectedItem;

            computer.OpenTightVNC();
        }

        private void MenuItem_ConnectedUser(object sender, RoutedEventArgs e)
        {
            Computer computer = (Computer)ComputersGrid.SelectedItem;

            String connectedUser = "";

            BackgroundWorker worker = new BackgroundWorker();

            //this is where the long running process should go
            worker.DoWork += (o, ea) =>
            {
                connectedUser = computer.GetConnectedUser();
            };

            worker.RunWorkerCompleted += (o, ea) =>
            {
                //work has completed. you can now interact with the UI
                if (connectedUser == null)
                {
                    MessageBox.Show("L'ordinateur ne semble pas connecté.");
                }
                else if(connectedUser == "")
                {
                    MessageBox.Show("Utilisateur inconnu.");
                }
                else
                {
                    MessageBox.Show("Utilisateur connecté : " + connectedUser);
                }
                
                _busyIndicator.IsBusy = false;
            };

            //set the IsBusy before you start the thread
            _busyIndicator.BusyContent = "Recherche de l'utilisateur connecté...";
            _busyIndicator.IsBusy = true;
            worker.RunWorkerAsync();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            this.LoadList();
        }
    }
}
