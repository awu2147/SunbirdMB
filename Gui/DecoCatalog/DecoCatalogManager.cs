using SunbirdMB.Core;
using SunbirdMB.Framework;
using SunbirdMB.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SunbirdMB.Gui
{ 
    public class DecoCatalogManager : PropertyChangedBase, IImporter
    {
        internal readonly DecoCatalogArgs Args;
        internal readonly DecoCatalogViewModel ViewModel;
        private ObservableCollection<DecoCatalogItem> decoCollection = new ObservableCollection<DecoCatalogItem>();
        private ObservableCollection<DecoCatalogItem> cachedDecoCollection;
        private bool isLocalSubLevel;

        public ObservableCollection<DecoCatalogItem> DecoCollection
        {
            get { return decoCollection; }
            set { SetProperty(ref decoCollection, value); }
        }

        public bool IsLocalSubLevel
        {
            get { return isLocalSubLevel; }
            set { SetProperty(ref isLocalSubLevel, value, ViewModel, nameof(ViewModel.IsSubLevel), nameof(ViewModel.IsAnimationComboBoxEnabled)); }
        }

        internal DecoCatalogManager(DecoCatalogViewModel viewModel, DecoCatalogArgs args)
        {
            ViewModel = viewModel;
            ViewModel.SelectedTabChanged += ViewModel_SelectedTabChanged;
            Args = args;
        }

        private void ViewModel_SelectedTabChanged(object sender, StringChangedEventArgs e)
        {
            if (e.NewString == Args.CatalogName)
            {
                ViewModel.CurrentMetadata = DecoCollection.Where(d => (d.Selection == SelectionMode.Selected) || (d.Selection == SelectionMode.Active)).First().DecoMetadata;
                ViewModel.C_Import = new RelayCommand((o) => Import());
                MapBuilder.GhostMarker.MorphCurrentDeco();
            }
        }

        internal void EnterSubLevel(DecoCatalogItem dci)
        {
            cachedDecoCollection = DecoCollection;
            CreateSubLevelCollection(dci);
            IsLocalSubLevel = true;
        }

        private void CreateSubLevelCollection(DecoCatalogItem dci)
        {
            DecoCollection = new ObservableCollection<DecoCatalogItem>();
            int count = 0;
            for (int y = 0; y < dci.DecoMetadata.SheetRows; y++)
            {
                for (int x = 0; x < dci.DecoMetadata.SheetColumns; x++)
                {
                    count++;
                    SelectionMode selection = dci.DecoMetadata.ActiveFrames.Contains(count) ? SelectionMode.Active : SelectionMode.None;
                    var newDci = new DecoCatalogItem(this, dci.ImagePath, dci.DecoMetadata, Args.ItemWidth, Args.ItemHeight);
                    newDci.SourceRect = new Int32Rect(newDci.ItemWidth * x, newDci.ItemHeight * y, newDci.ItemWidth, newDci.ItemHeight);
                    newDci.Selection = selection;
                    DecoCollection.Add(newDci);
                    if (count == dci.DecoMetadata.FrameCount)
                    {
                        break;
                    }
                }
                if (count == dci.DecoMetadata.FrameCount)
                {
                    break;
                }
            }
        }

        internal void ExitSubLevel()
        {
            DecoCollection = cachedDecoCollection;
            IsLocalSubLevel = false;
        }

        internal void ImportAll()
        {
            var decos = Directory.GetFiles(Args.ImportDirectory, "*.png", SearchOption.AllDirectories);
            foreach (var deco in decos)
            {
                Import(deco);
            }
        }

        internal void Import()
        {            
            Importer.CopyBuildImport(Args.ImportDirectory, this);
            MetadataItemBase.Sort(DecoCollection);
        }

        public void Import(string path)
        {
            try
            {
                var contentPath = path.MakeContentRelative();
                if (contentPath == string.Empty) { return; }

                var metadataPath = Path.ChangeExtension(path, ".metadata");
                DecoMetadata dmd;
                if (File.Exists(metadataPath))
                {
                    dmd = Serializer.ReadXML<DecoMetadata>(DecoMetadata.DecoMetadataSerializer, metadataPath);
                    dmd.LoadContent(ViewModel.MainGame);
                }
                else
                {
                    dmd = new DecoMetadata() { ContentPath = contentPath };
                    dmd.LoadContent(ViewModel.MainGame);
                    dmd.Serialize(metadataPath);
                    $"Creating {Path.GetFileName(metadataPath)}...".Log();
                }

                dmd.Dimensions = Args.DecoDimension;
                dmd.PositionOffset = Args.DecoPositionOffset;
                DecoCollection.Add(new DecoCatalogItem(this, path, dmd, Args.ItemWidth, Args.ItemHeight));

            }
            catch (Exception e)
            {
                e.Message.Log();
            }
        }

    }
}
