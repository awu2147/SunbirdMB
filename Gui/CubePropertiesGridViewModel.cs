using SunbirdMB.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SunbirdMB.Gui
{
    public class CubePropertiesGridViewModel : PropertyChangedBase
    {

        public CubePropertiesGridViewModel(CubeMetaData cmd)
        {

        }
    }

    public class PropertyValuePair : PropertyChangedBase
    {
        private string property;

        public string Property
        {
            get { return property; }
            set { SetProperty(ref property, value); }
        }

        private string value;

        public string Value
        {
            get { return value; }
            set { SetProperty(ref value, value); }
        }
    }
}
