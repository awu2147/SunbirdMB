using SunbirdMB.Core;
using SunbirdMB.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SunbirdMB.Gui
{
    public class MainToolbarViewModel : PropertyChangedBase
    {
        public static event PropertyChangedEventHandler StaticPropertyChanged;

        private static readonly PropertyChangedEventArgs AuthorizationPropertyEventArgs = new PropertyChangedEventArgs(nameof(Authorization));
        private static readonly PropertyChangedEventArgs BuildModePropertyEventArgs = new PropertyChangedEventArgs(nameof(BuildMode));

        private SunbirdMBWindowViewModel SunbirdMBWindowViewModel;

        private bool isBuilding;
        public bool IsBuilding
        {
            get { return isBuilding; }
            set { SetProperty(ref isBuilding, value); }
        }

        public ICommand C_BuildButtonClick { get; set; }
        public ICommand C_WorldButtonClick { get; set; }
        public ICommand C_ShowCubeDesigner { get; set; }
        public ICommand C_ShowDecoCatalog { get; set; }

        private static Authorization authorization;
        public static Authorization Authorization
        {
            get { return authorization; }
            set
            {
                if (authorization == value)
                    return;
                authorization = value;
                StaticPropertyChanged?.Invoke(null, AuthorizationPropertyEventArgs);
            }
        }

        private static BuildMode buildMode;
        public static BuildMode BuildMode
        {
            get { return buildMode; }
            set
            {
                if (buildMode == value)
                    return;
                buildMode = value;
                StaticPropertyChanged?.Invoke(null, BuildModePropertyEventArgs);
            }
        }

        internal MainToolbarViewModel(SunbirdMBWindowViewModel sunbirdMBWindowViewModel)        
        {
            SunbirdMBWindowViewModel = sunbirdMBWindowViewModel;
            C_BuildButtonClick = new RelayCommand((o) => SetBuildMode(Authorization.Builder));
            C_WorldButtonClick = new RelayCommand((o) => SetBuildMode(Authorization.None));
            C_ShowCubeDesigner = new RelayCommand((o) => ShowCubeDesigner());
            C_ShowDecoCatalog = new RelayCommand((o) => ShowDecoCatalog());
        }

        private void SetBuildMode(Authorization auth)
        {
            Authorization = auth;
        }

        private void ShowCubeDesigner()
        {
            BuildMode = BuildMode.Cube;
            SunbirdMBWindowViewModel.CubeDecoSelectedIndex = 0;
            MapBuilder.GhostMarker.MorphImage(CubeFactory.CreateCurrentCube(Coord.Zero, Coord.Zero, 0));
        }
        private void ShowDecoCatalog()
        {
            BuildMode = BuildMode.Deco;
            SunbirdMBWindowViewModel.CubeDecoSelectedIndex = 1;
            MapBuilder.GhostMarker.MorphImage(DecoFactory.CreateCurrentDeco(Coord.Zero, Coord.Zero, 0));
        }

    }
}
