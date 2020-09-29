using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SunbirdMB.Interfaces;
using SunbirdMB.Framework;

namespace SunbirdMB.Core
{
    [Serializable]
    public class Deco : Sprite, IWorldObject
    {
        public XDictionary<int, HashSet<Coord>> OccupiedCoords { get; set; } = new XDictionary<int, HashSet<Coord>>();
        public Dimension Dimensions { get; set; }

        public Deco() { }

    }

    [Serializable]
    public class DecoMetaData
    {
        [XmlIgnore]
        public Texture2D Texture;
        public string Path { get; set; }

        [XmlIgnore]
        public Texture2D AntiShadow;
        [XmlIgnore]
        public Texture2D SelfShadow;

        public string TypeName { get; set; }
        public Vector2 PositionOffset { get; set; }
        public int SheetRows { get; set; } = 1;
        public int SheetColumns { get; set; } = 1;
        public int StartFrame { get; set; } = 0;
        public int CurrentFrame { get; set; } = 0;
        public int FrameCount { get; set; } = 1;
        public float FrameSpeed { get; set; } = 0.133f;
        public AnimationState AnimState { get; set; } = AnimationState.None;
        public Dimension Dimensions { get; set; }

        public DecoMetaData()
        {

        }

        /// <summary>
        /// Core method used to re-instantiate non-serializable properties and delegates. This can create garbage if called during runtime.
        /// </summary>
        public void LoadContent(SunbirdMBGame mainGame)
        {
            Texture = mainGame.Content.Load<Texture2D>(Path);

            if (GraphicsHelper.AntiShadowLibrary.ContainsKey(Path))
            {
                AntiShadow = GraphicsHelper.AntiShadowLibrary[Path];
            }
            else
            {
                AntiShadow = GraphicsHelper.GetAntiShadow(mainGame, Texture);
                GraphicsHelper.AntiShadowLibrary.Add(Path, AntiShadow);
            }

            if (GraphicsHelper.SelfShadowLibrary.ContainsKey(Path))
            {
                SelfShadow = GraphicsHelper.SelfShadowLibrary[Path];
            }
            else
            {
                SelfShadow = GraphicsHelper.GetSelfShadow(mainGame, Texture);
                GraphicsHelper.SelfShadowLibrary.Add(Path, SelfShadow);
            }
        }

        public void NextFrame()
        {
            if (AnimState == AnimationState.None)
            {
                CurrentFrame++;
                if (CurrentFrame >= FrameCount)
                {
                    CurrentFrame = 0;
                }
            }
        }

        public void PreviousFrame()
        {
            if (AnimState == AnimationState.None)
            {
                CurrentFrame--;
                if (CurrentFrame < 0)
                {
                    CurrentFrame = FrameCount - 1;
                }
            }
        }
    }

    public static class DecoFactory
    {

        public static DecoMetaData CurrentDecoMetaData1x1 { get; set; }
        public static DecoMetaData CurrentDecoMetaData2x2 { get; set; }
        public static DecoMetaData CurrentDecoMetaData3x3 { get; set; }

        public static bool IsRandom { get; set; }

        public static int CurrentIndex1x1 { get; set; } = 0;
        public static int CurrentIndex2x2 { get; set; } = 1;
        public static int CurrentIndex3x3 { get; set; } = 2;

        public static List<DecoMetaData> DecoMetaDataLibrary1x1 { get; set; }
        public static List<DecoMetaData> DecoMetaDataLibrary2x2 { get; set; }
        public static List<DecoMetaData> DecoMetaDataLibrary3x3 { get; set; }

        public static XDictionary<string, DecoMetaData> DecoMetaDataLibrary { get; set; }

        public static Deco CreateDeco(SunbirdMBGame mainGame, DecoMetaData decoMD, Coord coords, Coord relativeCoords, int altitude)
        {
            var rand = new Random();
            Type type = Type.GetType(decoMD.TypeName);
            var deco = Activator.CreateInstance(type) as Deco;
            deco.Position = World.IsoFlatCoordToWorldPosition(coords);
            deco.PositionOffset = decoMD.PositionOffset;
            deco.Coords = relativeCoords;
            deco.Altitude = altitude;
            deco.AntiShadow = decoMD.AntiShadow;
            deco.SelfShadow = decoMD.SelfShadow;
            // Odd NxN takes priority over even NxN when coords along same horizontal line.
            deco.Dimensions = decoMD.Dimensions;
            if (deco.Dimensions.X % 2 == 0)
            {
                deco.DrawPriority = -1;
            }

            // Create deco animator.
            var spriteSheet = SpriteSheet.CreateNew(decoMD.Texture, decoMD.Path, decoMD.SheetRows, decoMD.SheetColumns);
            deco.Animator = new Animator(deco, spriteSheet, decoMD.StartFrame, decoMD.CurrentFrame, decoMD.FrameCount, decoMD.FrameSpeed, decoMD.AnimState);

            for (int k = 0; k < decoMD.Dimensions.Z; k++)
            {
                deco.OccupiedCoords.Add(altitude + k, new HashSet<Coord>() { });
                for (int i = 0; i < decoMD.Dimensions.X; i++)
                {
                    for (int j = 0; j < decoMD.Dimensions.Y; j++)
                    {
#if DEBUG
                        Debug.Assert(decoMD.Dimensions.X == decoMD.Dimensions.Y);
#endif
                        int offset = decoMD.Dimensions.Y / 2;
                        deco.OccupiedCoords[altitude + k].Add(deco.Coords + new Coord(i - offset, -j + offset));
                    }
                }
            }

            if (IsRandom == true && decoMD.AnimState == AnimationState.None)
            {
                deco.Animator.CurrentFrame = rand.Next(0, deco.Animator.FramesInLoop);
            }
            else
            {
                deco.Animator.CurrentFrame = decoMD.CurrentFrame;
            }

            return deco;
        }

        public static Deco CreateCurrentDeco1x1(SunbirdMBGame mainGame, Coord coords, Coord relativeCoords, int altitude)
        {
            return CreateDeco(mainGame, CurrentDecoMetaData1x1, coords, relativeCoords, altitude);
        }
        public static Deco CreateCurrentDeco2x2(SunbirdMBGame mainGame, Coord coords, Coord relativeCoords, int altitude)
        {
            return CreateDeco(mainGame, CurrentDecoMetaData2x2, coords, relativeCoords, altitude);
        }
        public static Deco CreateCurrentDeco3x3(SunbirdMBGame mainGame, Coord coords, Coord relativeCoords, int altitude)
        {
            return CreateDeco(mainGame, CurrentDecoMetaData3x3, coords, relativeCoords, altitude);
        }

        public static void FindNext(BuildMode buildMode)
        {
            if (buildMode == BuildMode._1x1)
            {
                FindNext1x1();
            }
            else if (buildMode == BuildMode._2x2)
            {
                FindNext2x2();
            }
            else if (buildMode == BuildMode._3x3)
            {
                FindNext3x3();
            }
        }

        public static void FindNext1x1()
        {
            CurrentIndex1x1++;
            if (CurrentIndex1x1 >= DecoMetaDataLibrary1x1.Count())
            {
                CurrentIndex1x1 = 0;
            }
            CurrentDecoMetaData1x1 = DecoMetaDataLibrary1x1[CurrentIndex1x1];
        }
        public static void FindNext2x2()
        {
            CurrentIndex2x2++;
            if (CurrentIndex2x2 >= DecoMetaDataLibrary2x2.Count())
            {
                CurrentIndex2x2 = 0;
            }
            CurrentDecoMetaData2x2 = DecoMetaDataLibrary2x2[CurrentIndex2x2];
        }
        public static void FindNext3x3()
        {
            CurrentIndex3x3++;
            if (CurrentIndex3x3 >= DecoMetaDataLibrary3x3.Count())
            {
                CurrentIndex3x3 = 0;
            }
            CurrentDecoMetaData3x3 = DecoMetaDataLibrary3x3[CurrentIndex3x3];
        }

        private static void FindPrevious(BuildMode buildMode)
        {
            if (buildMode == BuildMode._1x1)
            {
                FindPrevious1x1();
            }
            else if (buildMode == BuildMode._2x2)
            {
                FindPrevious2x2();
            }
            else if (buildMode == BuildMode._3x3)
            {
                FindPrevious3x3();
            }
        }

        public static void FindPrevious1x1()
        {
            CurrentIndex1x1--;
            if (CurrentIndex1x1 < 0)
            {
                CurrentIndex1x1 = DecoMetaDataLibrary1x1.Count() - 1;
            }
            CurrentDecoMetaData1x1 = DecoMetaDataLibrary1x1[CurrentIndex1x1];
        }
        public static void FindPrevious2x2()
        {
            CurrentIndex2x2--;
            if (CurrentIndex2x2 < 0)
            {
                CurrentIndex2x2 = DecoMetaDataLibrary2x2.Count() - 1;
            }
            CurrentDecoMetaData2x2 = DecoMetaDataLibrary2x2[CurrentIndex2x2];
        }
        public static void FindPrevious3x3()
        {
            CurrentIndex3x3--;
            if (CurrentIndex3x3 < 0)
            {
                CurrentIndex3x3 = DecoMetaDataLibrary3x3.Count() - 1;
            }
            CurrentDecoMetaData3x3 = DecoMetaDataLibrary3x3[CurrentIndex3x3];
        }

    }

    /// <summary>
    /// Acts as a data store for the static class CubeFactory during serialization.
    /// </summary>
    [Serializable]
    public class DecoFactoryData
    {
        public static readonly XmlSerializer DecoFactoryDataSerializer = Serializer.CreateNew(typeof(DecoFactoryData), new Type[] { typeof(DecoMetaData) });

        public bool IsRandom { get; set; }

        public DecoMetaData CurrentDecoMetaData1x1 { get; set; }
        public DecoMetaData CurrentDecoMetaData2x2 { get; set; }
        public DecoMetaData CurrentDecoMetaData3x3 { get; set; }

        public int CurrentIndex1x1 { get; set; }
        public int CurrentIndex2x2 { get; set; }
        public int CurrentIndex3x3 { get; set; }

        public List<DecoMetaData> DecoMetaDataLibrary1x1 { get; set; }
        public List<DecoMetaData> DecoMetaDataLibrary2x2 { get; set; }
        public List<DecoMetaData> DecoMetaDataLibrary3x3 { get; set; }

        public DecoFactoryData() { }

        public void Serialize()
        {
            Serializer.WriteXML<DecoFactoryData>(DecoFactoryDataSerializer, this, "DecoFactoryData.xml");
        }

        /// <summary>
        /// Create a copy of DecoFactory's static properties;
        /// </summary>
        public void Save()
        {
            IsRandom = DecoFactory.IsRandom;

            CurrentDecoMetaData1x1 = DecoFactory.CurrentDecoMetaData1x1;
            CurrentDecoMetaData2x2 = DecoFactory.CurrentDecoMetaData2x2;
            CurrentDecoMetaData3x3 = DecoFactory.CurrentDecoMetaData3x3;

            CurrentIndex1x1 = DecoFactory.CurrentIndex1x1;
            CurrentIndex2x2 = DecoFactory.CurrentIndex2x2;
            CurrentIndex3x3 = DecoFactory.CurrentIndex3x3;

            DecoMetaDataLibrary1x1 = DecoFactory.DecoMetaDataLibrary1x1;
            DecoMetaDataLibrary2x2 = DecoFactory.DecoMetaDataLibrary2x2;
            DecoMetaDataLibrary3x3 = DecoFactory.DecoMetaDataLibrary3x3;
        }

        /// <summary>
        /// Reassign values to DecoFactory's static properties;
        /// </summary>
        public void Load(SunbirdMBGame mainGame)
        {
            DecoFactory.IsRandom = IsRandom;

            DecoFactory.CurrentDecoMetaData1x1 = CurrentDecoMetaData1x1;
            DecoFactory.CurrentDecoMetaData2x2 = CurrentDecoMetaData2x2;
            DecoFactory.CurrentDecoMetaData3x3 = CurrentDecoMetaData3x3;

            DecoFactory.CurrentIndex1x1 = CurrentIndex1x1;
            DecoFactory.CurrentIndex2x2 = CurrentIndex2x2;
            DecoFactory.CurrentIndex3x3 = CurrentIndex3x3;

            DecoFactory.DecoMetaDataLibrary1x1 = DecoMetaDataLibrary1x1;
            DecoFactory.DecoMetaDataLibrary2x2 = DecoMetaDataLibrary2x2;
            DecoFactory.DecoMetaDataLibrary3x3 = DecoMetaDataLibrary3x3;

            // Generate library Textures, AntiShadows, and SelfShadows from Path and populate master DecoMetaDataLibrary (Dictionary).
            DecoFactory.DecoMetaDataLibrary = new XDictionary<string, DecoMetaData>();
            foreach (var dmd in DecoFactory.DecoMetaDataLibrary1x1)
            {
                dmd.LoadContent(mainGame);
                if (!DecoFactory.DecoMetaDataLibrary.ContainsKey(dmd.Path))
                {
                    DecoFactory.DecoMetaDataLibrary.Add(dmd.Path, dmd);
                }
            }
            foreach (var dmd in DecoFactory.DecoMetaDataLibrary2x2)
            {
                dmd.LoadContent(mainGame);
                if (!DecoFactory.DecoMetaDataLibrary.ContainsKey(dmd.Path))
                {
                    DecoFactory.DecoMetaDataLibrary.Add(dmd.Path, dmd);
                }
            }
            foreach (var dmd in DecoFactory.DecoMetaDataLibrary3x3)
            {
                dmd.LoadContent(mainGame);
                if (!DecoFactory.DecoMetaDataLibrary.ContainsKey(dmd.Path))
                {
                    DecoFactory.DecoMetaDataLibrary.Add(dmd.Path, dmd);
                }
            }

            DecoFactory.CurrentDecoMetaData1x1.LoadContent(mainGame);
            DecoFactory.CurrentDecoMetaData2x2.LoadContent(mainGame);
            DecoFactory.CurrentDecoMetaData3x3.LoadContent(mainGame);
        }

    }

}

