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

        public Player(IMainGame mainGame, SpriteSheet spriteSheet, AnimArgs switchAnimArgs) : base(mainGame, spriteSheet, switchAnimArgs)
        {
            PositionOffset = new Vector2(0, -54);
        }

        public override void LoadContent(IMainGame mainGame)
        {
            base.LoadContent(mainGame);
        }

        public override void Update(GameTime gameTime)
        {
            MoveUpdate(gameTime);
            base.Update(gameTime);
        }

        private Dictionary<Coord2D, int> CoordDistances = new Dictionary<Coord2D, int>();
        List<Coord2D> pathList = null;
        List<Coord2D> cachedPathList = null;
        Coord2D EffectiveCoord;
        bool isWalking = false;
        int walkTicks = 0;
        Vector2 increment;
        Direction direction;
        int velocity = 3;

        private void MoveUpdate(GameTime gameTime)
        {
            Coord2D mouseIsoFlatCoord = World.MousePositionToIsoFlatCoord((SunbirdMBGame)MainGame);
            if (Peripherals.LeftButtonTapped() && MainGame.IsActive)
            {
                if (MainToolbarViewModel.Authorization == Authorization.None)
                {
                    Coord2D target = mouseIsoFlatCoord;
                    Coord2D current;
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
                    List<Coord2D> coordsToVisit = new List<Coord2D>() { current };
                    List<Coord2D> coordsVisited = new List<Coord2D>();

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
                        if (mapBuilder.WalkableTileTable.ContainsKey(Altitude))
                        {
                            foreach (var coord in mapBuilder.WalkableTileTable[Altitude][nextCoord])
                            {
                                if (!coordsVisited.Contains(coord) && !coordsToVisit.Contains(coord))
                                {
                                    // identify if coord is a diagonal of nextCoord.
                                    var diff = coord - nextCoord;
                                    if (diff.X == 0 || diff.Y == 0)
                                    {
                                        // coord is not a diagonal, so it's fine to add it without additional checks.
                                        CoordDistances.Add(coord, CoordDistances[nextCoord] + 1);
                                        coordsToVisit.Add(coord);

                                    }
                                    else
                                    {
                                        var xDiff = new Coord2D(nextCoord.X + diff.X, nextCoord.Y);
                                        var yDiff = new Coord2D(nextCoord.X, nextCoord.Y + diff.Y);
                                        if (mapBuilder.WalkableTileTable[Altitude][nextCoord].Contains(xDiff) &&
                                            mapBuilder.WalkableTileTable[Altitude][nextCoord].Contains(yDiff))
                                        {
                                            CoordDistances.Add(coord, CoordDistances[nextCoord] + 1);
                                            coordsToVisit.Add(coord);
                                        }
                                    }
                                }
                            }
                            coordsVisited.Add(nextCoord);
                            visitedCoordsCount++;
                            coordsToVisit.RemoveAt(0);
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (reachedTarget)
                    {
                        var _nextCoord = target;
                        if (cachedPathList == null)
                        {
                            cachedPathList = new List<Coord2D>();
                        }
                        cachedPathList.Clear();
                        cachedPathList.Add(_nextCoord);
                        while (_nextCoord != current)
                        {
                            List<Coord2D> adjacentCoords = new List<Coord2D>() 
                            {

                                _nextCoord + new Coord2D(0, 1), 
                                _nextCoord + new Coord2D(0, -1), 
                                _nextCoord + new Coord2D(1, 0), 
                                _nextCoord + new Coord2D(-1, 0),
                                _nextCoord + new Coord2D(1, 1),
                                _nextCoord + new Coord2D(1, -1),
                                _nextCoord + new Coord2D(-1, -1),
                                _nextCoord + new Coord2D(-1, 1),
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
                        //"pathlist complete".Log();
                        foreach (var p in cachedPathList)
                        {
                            //p.ToString().Log();
                        }
                    }

                    //Position = World.IsoFlatCoordToWorldPosition(mouseIsoFlatCoord);
                }
            }
            //Coords = World.WorldPositionToIsoCoord(Position + new Vector2(36, -18), Altitude);
            if (cachedPathList != null && cachedPathList.Count() != 0 && isWalking == false)
            {
                pathList = new List<Coord2D>(cachedPathList);
                cachedPathList = null;
                var nextCoord = pathList[pathList.Count() - 1];
                var diff = !isWalking ? nextCoord - Coords : nextCoord - EffectiveCoord;
                increment = diff.X * new Vector2(1f * velocity, 0.5f * velocity) + diff.Y * new Vector2(1f * velocity, -0.5f * velocity);
                EffectiveCoord = World.WorldPositionToIsoCoord(Position + (increment * (36f / velocity)) + new Vector2(36, -18), Altitude);
                walkTicks = (36 / velocity) - 1;
                if ((int)Math.Abs(Math.Round(increment.X, 0)) == (2 * velocity))
                {
                    walkTicks = (72 / velocity) - 1;
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
                    Animator.Reconfigure(new AnimArgs(1, 4, 0.1f, AnimationState.Loop));
                }
                else if (direction == Direction.East || direction == Direction.NorthEast || direction == Direction.SouthEast)
                {
                    Animator.Reconfigure(new AnimArgs(5, 4, 0.1f, AnimationState.Loop));
                }
                else if (direction == Direction.South)
                {
                    Animator.Reconfigure(new AnimArgs(9, 4, 0.1f, AnimationState.Loop));
                }
                else if (direction == Direction.West || direction == Direction.NorthWest || direction == Direction.SouthWest)
                {
                    Animator.Reconfigure(new AnimArgs(13, 4, 0.1f, AnimationState.Loop));
                }

                //Position = World.IsoFlatCoordToWorldPosition(pathList[pathList.Count() - 1]);
                //pathList.RemoveAt(pathList.Count() - 1);
                //Coords = World.WorldPositionToIsoCoord(Position + new Vector2(36, -18), Altitude);
            }
            else if (pathList != null && pathList.Count() != 0 && isWalking == false)
            {
                var nextCoord = pathList[pathList.Count() - 1];
                var diff = !isWalking ? nextCoord - Coords : nextCoord - EffectiveCoord;
                increment = diff.X * new Vector2(1f * velocity, 0.5f * velocity) + diff.Y * new Vector2(1f * velocity, -0.5f * velocity);
                EffectiveCoord = World.WorldPositionToIsoCoord(Position + (increment * (36f / velocity)) + new Vector2(36, -18), Altitude);
                walkTicks = (36 / velocity) - 1;
                if ((int)Math.Abs(Math.Round(increment.X, 0)) == (2 * velocity))
                {
                    walkTicks = (72 / velocity) - 1;
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
                        var inc = diff.X * new Vector2(1f * velocity, 0.5f * velocity) + diff.Y * new Vector2(1f * velocity, -0.5f * velocity);
                        if (increment != inc && (increment * new Vector2(2,1)) != inc)
                        {
                            increment = inc;
                            //"reconfig".Log();
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
                                Animator.Reconfigure(new AnimArgs(1, 4, 0.1f, AnimationState.Loop));
                            }
                            else if (direction == Direction.East || direction == Direction.NorthEast || direction == Direction.SouthEast)
                            {
                                Animator.Reconfigure(new AnimArgs(5, 4, 0.1f, AnimationState.Loop));
                            }
                            else if (direction == Direction.South)
                            {
                                Animator.Reconfigure(new AnimArgs(9, 4, 0.1f, AnimationState.Loop));
                            }
                            else if (direction == Direction.West || direction == Direction.NorthWest || direction == Direction.SouthWest)
                            {
                                Animator.Reconfigure(new AnimArgs(13, 4, 0.1f, AnimationState.Loop));
                            }
                            //direction.ToString().Log();
                        }
                    }
                    if (pathList.Count() == 0)
                    {
                        if (direction == Direction.North)
                        {
                            Animator.Reconfigure(new AnimArgs(1, 4, 0.1f, AnimationState.None));
                        }
                        else if (direction == Direction.East || direction == Direction.NorthEast || direction == Direction.SouthEast)
                        {
                            Animator.Reconfigure(new AnimArgs(5, 4, 0.1f, AnimationState.None));
                        }
                        else if (direction == Direction.South)
                        {
                            Animator.Reconfigure(new AnimArgs(9, 4, 0.1f, AnimationState.None));
                        }
                        else if (direction == Direction.West || direction == Direction.NorthWest || direction == Direction.SouthWest)
                        {
                            Animator.Reconfigure(new AnimArgs(13, 4, 0.1f, AnimationState.None));
                        }
                    }
                    Coords = World.WorldPositionToIsoCoord(Position + new Vector2(36, -18), Altitude);
                }
            }
        }
    }
}
