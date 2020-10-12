using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
        List<Coord> pathList = null;
        List<Coord> cachedPathList = null;
        Coord EffectiveCoord;
        bool isWalking = false;
        int walkTicks = 0;
        Vector2 increment;
        Direction direction;

        private void MoveUpdate(GameTime gameTime)
        {
            Coord mouseIsoFlatCoord = World.MousePositionToIsoFlatCoord((SunbirdMBGame)MainGame);
            if (Peripherals.LeftButtonTapped() && MainGame.IsActive)
            {
                if (MainToolbarViewModel.Authorization == Authorization.None)
                {
                    Coord target = mouseIsoFlatCoord;
                    Coord current;
                    if (isWalking)
                    {
                        current = EffectiveCoord;
                    }
                    else
                    {
                        current = Coords;
                    }
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
                        if (cachedPathList == null)
                        {
                            cachedPathList = new List<Coord>();
                        }
                        cachedPathList.Clear();
                        cachedPathList.Add(_nextCoord);
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
                                    cachedPathList.Add(coord);
                                    _nextCoord = coord;
                                    //$"{_nextCoord}".Log();
                                    break;
                                }
                            }
                        }
                        cachedPathList.RemoveAt(cachedPathList.Count() - 1);
                        "pathlist complete".Log();
                        foreach (var p in cachedPathList)
                        {
                            p.ToString().Log();
                        }
                    }

                    //Position = World.IsoFlatCoordToWorldPosition(mouseIsoFlatCoord);
                }
            }
            //Coords = World.WorldPositionToIsoCoord(Position + new Vector2(36, -18), Altitude);
            if (cachedPathList != null && cachedPathList.Count() != 0 && isWalking == false)
            {
                pathList = new List<Coord>(cachedPathList);
                cachedPathList = null;
                var nextCoord = pathList[pathList.Count() - 1];
                var diff = !isWalking ? nextCoord - Coords : nextCoord - EffectiveCoord;
                increment = diff.X * new Vector2(1 / 0.5f, 0.5f / 0.5f) + diff.Y * new Vector2(1 / 0.5f, -0.5f / 0.5f);
                EffectiveCoord = World.WorldPositionToIsoCoord(Position + (increment * 36 * 0.5f) + new Vector2(36, -18), Altitude);
                walkTicks = 17;
                if ((int)Math.Abs(Math.Round(increment.X, 0)) == 4)
                {
                    walkTicks = 35;
                    increment = new Vector2(increment.X / 2f, increment.Y);
                }
                isWalking = true;

                direction = Direction.None;
                if (increment.X > 0)
                {
                    direction |= Direction.East;
                }
                else if (increment.X < 0)
                {
                    direction |= Direction.West;
                }
                if (increment.Y > 0)
                {
                    direction |= Direction.South;
                }
                else if (increment.Y < 0)
                {
                    direction |= Direction.North;
                }

                if (direction == Direction.North)
                {
                    Animator.Reconfigure(new AnimArgs(1, 4, 0.133f, AnimationState.Loop));
                }
                else if (direction == Direction.East || direction == Direction.NorthEast || direction == Direction.SouthEast)
                {
                    Animator.Reconfigure(new AnimArgs(5, 4, 0.133f, AnimationState.Loop));
                }
                else if (direction == Direction.South)
                {
                    Animator.Reconfigure(new AnimArgs(9, 4, 0.133f, AnimationState.Loop));
                }
                else if (direction == Direction.West || direction == Direction.NorthWest || direction == Direction.SouthWest)
                {
                    Animator.Reconfigure(new AnimArgs(13, 4, 0.133f, AnimationState.Loop));
                }

                //Position = World.IsoFlatCoordToWorldPosition(pathList[pathList.Count() - 1]);
                //pathList.RemoveAt(pathList.Count() - 1);
                //Coords = World.WorldPositionToIsoCoord(Position + new Vector2(36, -18), Altitude);
            }
            else if (pathList != null && pathList.Count() != 0 && isWalking == false)
            {
                var nextCoord = pathList[pathList.Count() - 1];
                var diff = !isWalking ? nextCoord - Coords : nextCoord - EffectiveCoord;
                increment = diff.X * new Vector2(1 / 0.5f, 0.5f / 0.5f) + diff.Y * new Vector2(1 / 0.5f, -0.5f / 0.5f);
                EffectiveCoord = World.WorldPositionToIsoCoord(Position + (increment * 36 * 0.5f) + new Vector2(36, -18), Altitude);
                walkTicks = 17;
                if ((int)Math.Abs(Math.Round(increment.X, 0)) == 4)
                {
                    walkTicks = 35;
                    increment = new Vector2(increment.X / 2f, increment.Y);
                }
                isWalking = true;

            }
            if (isWalking)
            {
                //SunbirdMBGame.SamplerState = SamplerState.AnisotropicClamp;
                if (walkTicks > 0)
                {
                    Position += increment;
                    Coords = World.WorldPositionToIsoCoord(Position + new Vector2(36, -18), Altitude);
                    walkTicks--;
                }
                else if (walkTicks == 0)
                {
                    Position = World.IsoFlatCoordToWorldPosition(pathList[pathList.Count() - 1]);
                    pathList.RemoveAt(pathList.Count() - 1);
                    isWalking = false;
                    //reconfigure direction
                    if (pathList.Count() != 0)
                    {
                        var nextCoord = pathList[pathList.Count() - 1];
                        var diff = nextCoord - Coords;
                        var inc = diff.X * new Vector2(1 / 0.5f, 0.5f / 0.5f) + diff.Y * new Vector2(1 / 0.5f, -0.5f / 0.5f);
                        if (increment != inc)
                        {
                            increment = inc;
                            "reconfig".Log();
                            direction = Direction.None;
                            if (increment.X > 0)
                            {
                                direction |= Direction.East;
                            }
                            else if (increment.X < 0)
                            {
                                direction |= Direction.West;
                            }
                            if (increment.Y > 0)
                            {
                                direction |= Direction.South;
                            }
                            else if (increment.Y < 0)
                            {
                                direction |= Direction.North;
                            }

                            if (direction == Direction.North)
                            {
                                Animator.Reconfigure(new AnimArgs(1, 4, 0.133f, AnimationState.Loop));
                            }
                            else if (direction == Direction.East || direction == Direction.NorthEast || direction == Direction.SouthEast)
                            {
                                Animator.Reconfigure(new AnimArgs(5, 4, 0.133f, AnimationState.Loop));
                            }
                            else if (direction == Direction.South)
                            {
                                Animator.Reconfigure(new AnimArgs(9, 4, 0.133f, AnimationState.Loop));
                            }
                            else if (direction == Direction.West || direction == Direction.NorthWest || direction == Direction.SouthWest)
                            {
                                Animator.Reconfigure(new AnimArgs(13, 4, 0.133f, AnimationState.Loop));
                            }
                            direction.ToString().Log();
                        }
                    }
                    if (pathList.Count() == 0)
                    {
                        if (direction == Direction.North)
                        {
                            Animator.Reconfigure(new AnimArgs(1, 4, 0.133f, AnimationState.None));
                        }
                        else if (direction == Direction.East || direction == Direction.NorthEast || direction == Direction.SouthEast)
                        {
                            Animator.Reconfigure(new AnimArgs(5, 4, 0.133f, AnimationState.None));
                        }
                        else if (direction == Direction.South)
                        {
                            Animator.Reconfigure(new AnimArgs(9, 4, 0.133f, AnimationState.None));
                        }
                        else if (direction == Direction.West || direction == Direction.NorthWest || direction == Direction.SouthWest)
                        {
                            Animator.Reconfigure(new AnimArgs(13, 4, 0.133f, AnimationState.None));
                        }
                    }
                    //walkTicks = 0;
                    SunbirdMBGame.SamplerState = SamplerState.PointClamp;
                    Coords = World.WorldPositionToIsoCoord(Position + new Vector2(36, -18), Altitude);
                }
            }
            //walkTimer.WaitForSeconds(gameTime, 0.1f);
        }
    }
}
