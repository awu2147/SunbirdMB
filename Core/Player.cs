using Microsoft.Xna.Framework;
using SunbirdMB.Framework;
using SunbirdMB.Gui;
using SunbirdMB.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunbirdMB.Core
{
    public class Player : Sprite
    {
        private Player() { }

        private Timer walkTimer = new Timer();

        public Player(IMainGame mainGame, SpriteSheet spriteSheet, AnimArgs switchAnimArgs) : base(mainGame, spriteSheet, switchAnimArgs)
        {
            PositionOffset = new Vector2(0, -54);
        }

        public override void LoadContent(IMainGame mainGame)
        {
            base.LoadContent(mainGame);
            walkTimer.OnCompleted = () =>
            {
                if (pathList.Count() != 0)
                {
                    Position = World.IsoFlatCoordToWorldPosition(pathList[pathList.Count() - 1]);
                    pathList.RemoveAt(pathList.Count() - 1);
                    Coords = World.WorldPositionToIsoCoord(Position + new Vector2(36, -18), Altitude);
                }
            };
        }

        public override void Update(GameTime gameTime)
        {
            MoveUpdate(gameTime);
            base.Update(gameTime);
        }

        private Dictionary<Coord, int> CoordDistances = new Dictionary<Coord, int>();
        List<Coord> pathList = new List<Coord>();

        private void MoveUpdate(GameTime gameTime)
        {
            Coord mouseIsoFlatCoord = World.MousePositionToIsoFlatCoord((SunbirdMBGame)MainGame);
            if (Peripherals.LeftButtonTapped() && MainGame.IsActive)
            {
                if (MainToolbarViewModel.Authorization == Authorization.None)
                {
                    Coord target = mouseIsoFlatCoord;
                    Coord current = Coords;
                    //$"Target coords = {target}, Current coords = {current}".Log();
                    MapBuilder mapBuilder = (MainGame as SunbirdMBGame).MapBuilder;
                    CoordDistances.Clear();
                    CoordDistances.Add(current, 0);
                    List<Coord> coordsToVisit = new List<Coord>() { current };
                    List<Coord> coordsVisited = new List<Coord>();

                    int visitedCoordsCount = 0;
                    bool reachedTarget = false;
                    while (true)
                    {
                        if (coordsToVisit.Count() == 0 || visitedCoordsCount > 500)
                        {
                            //"Could not reach target".Log();
                            break;
                        }
                        var nextCoord = coordsToVisit[0];
                        if (nextCoord == target)
                        {
                            //"Reached target".Log();
                            reachedTarget = true;
                            break;
                        }
                        foreach (var coord in mapBuilder.WalkableTileTable[Altitude][nextCoord])
                        {
                            if (!coordsVisited.Contains(coord) && !coordsToVisit.Contains(coord))
                            {
                                CoordDistances.Add(coord, CoordDistances[nextCoord] + 1);
                                coordsToVisit.Add(coord);
                            }                            
                        }
                        coordsVisited.Add(nextCoord);
                        visitedCoordsCount++;
                        coordsToVisit.RemoveAt(0);
                    }

                    if (reachedTarget)
                    {
                        var _nextCoord = target;
                        pathList.Clear();
                        pathList.Add(_nextCoord);
                        while (_nextCoord != current)
                        {
                            List<Coord> adjacentCoords = new List<Coord>() 
                            { 
                                _nextCoord + new Coord(0, 1), 
                                _nextCoord + new Coord(0, -1), 
                                _nextCoord + new Coord(1, 0), 
                                _nextCoord + new Coord(-1, 0),
                                _nextCoord + new Coord(1, 1),
                                _nextCoord + new Coord(1, -1),
                                _nextCoord + new Coord(-1, -1),
                                _nextCoord + new Coord(-1, 1),
                            };
                            foreach (var coord in adjacentCoords)
                            {
                                if (CoordDistances.ContainsKey(coord) && CoordDistances[coord] < CoordDistances[_nextCoord])
                                {
                                    pathList.Add(coord);
                                    _nextCoord = coord;
                                    //$"{_nextCoord}".Log();
                                    break;
                                }
                            }
                        }
                        //"pathlist complete".Log();
                    }

                    //Position = World.IsoFlatCoordToWorldPosition(mouseIsoFlatCoord);
                }
            }
            //Coords = World.WorldPositionToIsoCoord(Position + new Vector2(36, -18), Altitude);
            if (Peripherals.LeftButtonTapped() && MainGame.IsActive)
            {
                //$"{Coords}".Log();
            }
            walkTimer.WaitForSeconds(gameTime, 0.1f);
        }
    }
}
