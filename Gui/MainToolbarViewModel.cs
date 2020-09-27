using SunbirdMB.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SunbirdMB.Gui
{
    public class MainToolbarViewModel : PropertyChangedBase
    {
        private bool isBuilding;
        public bool IsBuilding
        {
            get { return isBuilding; }
            set { SetProperty(ref isBuilding, value); }
        }

        public ICommand C_BuildButtonClick { get; set; }
        public ICommand C_WorldButtonClick { get; set; } 

        private SunbirdMBGame SunbirdMBGame { get; set; }

        public MainToolbarViewModel(SunbirdMBGame sunbirdMBGame)        
        {
            SunbirdMBGame = sunbirdMBGame;
            C_BuildButtonClick = new RelayCommand((o) => SetBuildMode(true));
            C_WorldButtonClick = new RelayCommand((o) => SetBuildMode(false));
        }

        private void SetBuildMode(bool mode)
        {
            IsBuilding = mode;
            (SunbirdMBGame.CurrentState as MapBuilder).Authorization = mode ? Authorization.Builder : Authorization.None;
        }
    }
}
