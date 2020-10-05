using SunbirdMB.Core;
using SunbirdMB.Framework;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using SunbirdMB.Interfaces;
using SelectionMode = SunbirdMB.Framework.SelectionMode;
using Microsoft.Xna.Framework;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Linq;

namespace SunbirdMB.Gui
{
    internal class DecoCatalogViewModel : PropertyChangedBase
    {
        private DecoCatalogManager decoCatalog1x1x1;
        private DecoCatalogManager decoCatalog1x1x2;
        private DecoCatalogManager decoCatalog1x1x3;
        private DecoCatalogManager decoCatalog2x2x2;
        private DecoCatalogManager decoCatalog3x3x3;
        private TabItem selectedTab;

        public static DecoMetadata CurrentDecoMetadata { get; private set; }

        public event StringChangedEventHandler SelectedTabChanged;

        protected virtual void OnSelectedTabChanged(string tabName)
        {
            StringChangedEventHandler handler = SelectedTabChanged;
            handler?.Invoke(this, new StringChangedEventArgs(tabName));
        }
        public DecoCatalogManager DecoCatalog1x1x1
        {
            get { return decoCatalog1x1x1; }
            set { SetProperty(ref decoCatalog1x1x1, value); }
        }

        public DecoCatalogManager DecoCatalog1x1x2
        {
            get { return decoCatalog1x1x2; }
            set { SetProperty(ref decoCatalog1x1x2, value); }
        }

        public DecoCatalogManager DecoCatalog1x1x3
        {
            get { return decoCatalog1x1x3; }
            set { SetProperty(ref decoCatalog1x1x3, value); }
        }

        public DecoCatalogManager DecoCatalog2x2x2
        {
            get { return decoCatalog2x2x2; }
            set { SetProperty(ref decoCatalog2x2x2, value); }
        }

        public DecoCatalogManager DecoCatalog3x3x3
        {
            get { return decoCatalog3x3x3; }
            set { SetProperty(ref decoCatalog3x3x3, value); }
        }

        public TabItem SelectedTab
        {
            get { return selectedTab; }
            set 
            { 
                SetProperty(ref selectedTab, value, nameof(SelectedTab), nameof(CurrentMetadata));
                if (SelectedTab != null)
                {
                    OnSelectedTabChanged(SelectedTab.Header.ToString());
                }
            }
        }

        public DecoMetadata CurrentMetadata
        {
            get { return CurrentDecoMetadata; }
            set
            {
                CurrentDecoMetadata = value;
                NotifyPropertyChanged(nameof(CurrentMetadata));
            }
        }

        public bool IsSubLevel
        {
            get
            {
                return ((DecoCatalog1x1x1.IsLocalSubLevel && SelectedTab.Header.ToString() == DecoCatalog1x1x1.Args.CatalogName) ||
                        (DecoCatalog1x1x2.IsLocalSubLevel && SelectedTab.Header.ToString() == DecoCatalog1x1x2.Args.CatalogName) ||
                        (DecoCatalog1x1x3.IsLocalSubLevel && SelectedTab.Header.ToString() == DecoCatalog1x1x3.Args.CatalogName) ||
                        (DecoCatalog2x2x2.IsLocalSubLevel && SelectedTab.Header.ToString() == DecoCatalog2x2x2.Args.CatalogName) ||
                        (DecoCatalog3x3x3.IsLocalSubLevel && SelectedTab.Header.ToString() == DecoCatalog3x3x3.Args.CatalogName));
            }
        }

        public bool IsAnimationComboBoxEnabled { get { return !IsSubLevel; } }

        internal IMainGame MainGame { get; set; }

        private ICommand c_Import;
        public ICommand C_Import
        {
            get { return c_Import; }
            set { SetProperty(ref c_Import, value); }
        }

        public DecoCatalogViewModel() : this(null) { }

        public DecoCatalogViewModel(IMainGame mainGame)
        {
            MainGame = mainGame;

            var decoCatalog1x1x1Args = new DecoCatalogArgs()
            {
                CatalogName = "1x1x1",
                ImportDirectory = UriHelper.Deco1x1x1Directory,
                DecoDimension = new Dimension(1, 1, 1),
                DecoPositionOffset = new Vector2(0, 0),
                ItemWidth = 72,
                ItemHeight = 75,
            };
            DecoCatalog1x1x1 = new DecoCatalogManager(this, decoCatalog1x1x1Args);

            var decoCatalog1x1x2Args = new DecoCatalogArgs()
            {
                CatalogName = "1x1x2",
                ImportDirectory = UriHelper.Deco1x1x2Directory,
                DecoDimension = new Dimension(1, 1, 2),
                DecoPositionOffset = new Vector2(0, -36),
                ItemWidth = 72,
                ItemHeight = 111,
            };
            DecoCatalog1x1x2 = new DecoCatalogManager(this, decoCatalog1x1x2Args);

            var decoCatalog1x1x3Args = new DecoCatalogArgs()
            {
                CatalogName = "1x1x3",
                ImportDirectory = UriHelper.Deco1x1x3Directory,
                DecoDimension = new Dimension(1, 1, 3),
                DecoPositionOffset = new Vector2(0, -72),
                ItemWidth = 72,
                ItemHeight = 147,
            };
            DecoCatalog1x1x3 = new DecoCatalogManager(this, decoCatalog1x1x3Args);

            var decoCatalog2x2x2Args = new DecoCatalogArgs()
            {
                CatalogName = "2x2x2",
                ImportDirectory = UriHelper.Deco2x2x2Directory,
                DecoDimension = new Dimension(2, 2, 2),
                DecoPositionOffset = new Vector2(-36, -72),
                ItemWidth = 144,
                ItemHeight = 147,
            };
            DecoCatalog2x2x2 = new DecoCatalogManager(this, decoCatalog2x2x2Args);

            var decoCatalog3x3x3Args = new DecoCatalogArgs()
            {
                CatalogName = "3x3x3",
                ImportDirectory = UriHelper.Deco3x3x3Directory,
                DecoDimension = new Dimension(3, 3, 3),
                DecoPositionOffset = new Vector2(-72, -108),
                ItemWidth = 216,
                ItemHeight = 219,
            };
            DecoCatalog3x3x3 = new DecoCatalogManager(this, decoCatalog3x3x3Args);

        }

        internal void OnAfterContentBuild()
        {
            SunbirdMBWindow.PumpToSplash(() => SunbirdMBSplash.ViewModel.Message = "Importing Deco Content...");
            // Import all decos into their deco catalogs from the Content\Decos folder.
            ImportAll();
            // Initial current selections are set here.
            // TODO: Currently we just pick the first item in our tabs.
            SelectDeco(DecoCatalog1x1x1.DecoCollection[0]);
            SelectDeco(DecoCatalog1x1x2.DecoCollection[0]);
            SelectDeco(DecoCatalog1x1x3.DecoCollection[0]);
            SelectDeco(DecoCatalog2x2x2.DecoCollection[0]);
            SelectDeco(DecoCatalog3x3x3.DecoCollection[0]);
            // Current metadata to display in the deco catalog properties window.
            CurrentMetadata = DecoCatalog1x1x1.DecoCollection[0].DecoMetadata;
        }

        internal void OnGameLoaded() { }

        private void SelectDeco(DecoCatalogItem decoCatalogItem)
        {
            decoCatalogItem.Selection = SelectionMode.Selected;
        }     

        private void ImportAll()
        {
            DecoCatalog1x1x1.ImportAll();
            DecoCatalog1x1x2.ImportAll();
            DecoCatalog1x1x3.ImportAll();
            DecoCatalog2x2x2.ImportAll();
            DecoCatalog3x3x3.ImportAll();
        }

    }
}

