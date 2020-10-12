using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunbirdMB.Framework
{
    public enum Direction
    {
        None = 0,
        North = 1,
        East = 2,
        South = 4,
        West = 8,
        NorthEast = 3,
        SouthEast = 6,
        NorthWest = 9,
        SouthWest = 12,
    }

    public enum BuildMode
    {
        Cube,
        Deco
    }

    public enum Orientation
    {
        Vertical,
        Horizontal
    }

    public enum DeferAction
    {
        Add,
        Remove,
    }

    public enum Authorization
    {
        None,
        Builder
    }

    public enum CubePart
    {
        Top,
        Base,
        All
    }

    public enum AnimationState
    {
        None,
        Once,
        Loop
    }

    public enum SelectionMode
    { 
        None,
        Selected,
        Active    
    }

}
