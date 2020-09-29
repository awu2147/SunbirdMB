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
        private bool isBuilding;
        public bool IsBuilding
        {
            get { return isBuilding; }
            set { SetProperty(ref isBuilding, value); }
        }

        public ICommand C_BuildButtonClick { get; set; }
        public ICommand C_WorldButtonClick { get; set; }


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

        private static readonly PropertyChangedEventArgs AuthorizationPropertyEventArgs = new PropertyChangedEventArgs(nameof(Authorization));
        public static event PropertyChangedEventHandler StaticPropertyChanged;

        public MainToolbarViewModel()        
        {
            C_BuildButtonClick = new RelayCommand((o) => SetBuildMode(Authorization.Builder));
            C_WorldButtonClick = new RelayCommand((o) => SetBuildMode(Authorization.None));
        }

        private void SetBuildMode(bool mode)
        {
            IsBuilding = mode;
            //MapBuilder.Authorization = mode ? Authorization.Builder : Authorization.None;
        }

        private void SetBuildMode(Authorization auth)
        {
            Authorization = auth;
            //MapBuilder.Authorization = auth; // ? Authorization.Builder : Authorization.None;
        }

    }
}
