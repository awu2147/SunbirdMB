﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SunbirdMB.Gui
{
    public abstract class PropertyChangedBase : INotifyPropertyChanged
    {
        //The interface only includes this evennt
        public event PropertyChangedEventHandler PropertyChanged;

        //Common implementations of SetProperty
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName]string name = null)
        {
            bool propertyChanged = false;

            //If we have a different value, do stuff
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;
                NotifyPropertyChanged(name);
                propertyChanged = true;
            }

            return propertyChanged;
        }

        protected bool SetProperty<T>(ref T field, T value, params string[] names)
        {
            bool propertyChanged = false;

            //If we have a different value, do stuff
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;
                foreach (var name in names)
                {
                    NotifyPropertyChanged(name);
                }
                propertyChanged = true;
            }

            return propertyChanged;
        }

        protected bool SetProperty<T>(ref T field, T value, PropertyChangedBase parent, params string[] names)
        {
            bool propertyChanged = false;

            //If we have a different value, do stuff
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;
                foreach (var name in names)
                {
                    parent.NotifyPropertyChanged(name);
                }
                propertyChanged = true;
            }

            return propertyChanged;
        }

        //The C#6 version of the common implementation
        internal void NotifyPropertyChanged([CallerMemberName]string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

    }
}
