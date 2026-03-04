using StardewValley;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley.Extensions;
using xTile.Layers;
using xTile.Tiles;

namespace Pets_Enhanced_Mod.Utilities.Custom_Classes
{
    public static class PetsEnhancedPathfindHelper
    {
        public struct PathNodeStruct
        {
            public readonly int X;

            public readonly int Y;

            public readonly int ID;

            public readonly int G;

            public readonly int parentX;

            public readonly int parentY;

            public PathNodeStruct(int x, int y, int g, int _parentX, int _parentY)
            {
                this.X = x;
                this.Y = y;
                this.G = g;
                this.parentX = _parentX;
                this.parentY = _parentY;
                this.ID = ComputeHash(x, y);
            }
            public static int ComputeHash(int x, int y)
            {
                return HashCode.Combine(x, y);
            }
        }

        public static Point ConvertTileToItsCenterPixel(Point tile)
        {
            return new Point((tile.X * Game1.tileSize) + 32, (tile.Y * Game1.tileSize) + 32);
        }

        private static readonly sbyte[,] Directions = new sbyte[4, 2] { { -1, 0 }, { 1, 0 }, { 0, 1 }, { 0, -1 } };
    
        private static readonly PriorityQueue<PathNodeStruct,int> _openList = new();

        public static readonly Dictionary<int, PathNodeStruct> _closedList = new();

        public static void findPath(Point startPoint, Point endPoint, GameLocation location,Stack<Point> listForUse, int maxNodes, int maxPathSize, bool getJumpPath, Rectangle? boundingBox = null)
        {
            try
            {
                if (location is null || listForUse is null || maxNodes <= 0 || maxPathSize <= 0) { return; }

                _openList.Clear();
                _closedList.Clear();
                _openList.Enqueue(new PathNodeStruct(startPoint.X, startPoint.Y, 0, startPoint.X,startPoint.Y), Math.Abs(endPoint.X - startPoint.X) + Math.Abs(endPoint.Y - startPoint.Y));

                int layerWidth = location.map.Layers[0].LayerWidth;
                int layerHeight = location.map.Layers[0].LayerHeight;

                int currentProcessedNodes = 0;
                while (_openList.Count > 0 && currentProcessedNodes < maxNodes)
                {
                    currentProcessedNodes += 1;

                    PathNodeStruct pathNode = _openList.Dequeue();
                    if (pathNode.X == endPoint.X && pathNode.Y == endPoint.Y)
                    {
                        listForUse.Clear();
                        if (getJumpPath && boundingBox is not null) { reconstructJumpPath(pathNode, listForUse, maxPathSize,location, boundingBox.Value); }
                        else
                        {
                            reconstructPath(pathNode, listForUse, maxPathSize);
                        }
                        return;
                    }

                    _closedList.TryAdd(pathNode.ID,pathNode);

                    for (int i = 0; i < 4; i++)
                    {
                        int nX = pathNode.X + Directions[i, 0];
                        int nY = pathNode.Y + Directions[i, 1];
                        int nID = PathNodeStruct.ComputeHash(nX, nY);
                        if (_closedList.ContainsKey(nID))
                        {
                            continue;
                        }

                        if ((nX != endPoint.X || nY != endPoint.Y) && (nX < 0 || nY < 0 || nX >= layerWidth || nY >= layerHeight))
                        {
                            continue;
                        }

                        if (IsCollidingTilePosition(nX, nY, location,false, false))
                        {
                            continue;
                        }

                        PathNodeStruct neighborNode = new(nX, nY, pathNode.G + 1,pathNode.X,pathNode.Y);
                        int priority = neighborNode.G + (Math.Abs(endPoint.X - nX) + Math.Abs(endPoint.Y - nY));

                        _closedList.TryAdd(nID, neighborNode);
                        _openList.Enqueue(neighborNode, priority);
                    }
                }

                return;
            }
            finally
            {
            }
        }


        private static void reconstructPath(PathNodeStruct finalNode, Stack<Point> listForUse, int maxPathSize)
        {
            PathNodeStruct currentNode = finalNode;

            int num = 0;
            while (num < maxPathSize)
            {
                num += 1;
                // Add the current position to the stack
                listForUse.Push(new Point(currentNode.X, currentNode.Y));

                // Check if we have reached the start node. 
                // In your findPath, the start node is initialized with itself as the parent.
                if (currentNode.X == currentNode.parentX && currentNode.Y == currentNode.parentY)
                {
                    break;
                }

                // Calculate the ID of the parent to look it up in our history
                int parentID = PathNodeStruct.ComputeHash(currentNode.parentX, currentNode.parentY);

                if (_closedList.TryGetValue(parentID, out PathNodeStruct parent))
                {
                    currentNode = parent;
                }
                else
                {
                    break;
                }
            }
        }
        private static void reconstructJumpPath(PathNodeStruct finalNode, Stack<Point> listForUse, int maxPathSize, GameLocation location, Rectangle boundingBox)
        {
            PathNodeStruct currentNode = finalNode;

            var rentedHashSet = CacheReciclerHelper.RentHSTIntInt();
            try
            {
                int num = 0;
                while (num < maxPathSize)
                {
                    num += 1;

                    if (num == 1 || (currentNode.X == currentNode.parentX && currentNode.Y == currentNode.parentY))
                    {
                        listForUse.Push(new(currentNode.X, currentNode.Y));

                        if (num != 1) { break; } // Check if we have reached the start node. 
                    }

                    Point objectAtTop64 = ConvertTileToItsCenterPixel(listForUse.Peek());
                    Point currentObjectParent = ConvertTileToItsCenterPixel(new(currentNode.parentX, currentNode.parentY));
                    if (checkCollisionRay(objectAtTop64.X, objectAtTop64.Y, currentObjectParent.X, currentObjectParent.Y, location, boundingBox, rentedHashSet))
                    {
                        listForUse.Push(new(currentNode.X, currentNode.Y));
                    }

                    int parentID = PathNodeStruct.ComputeHash(currentNode.parentX, currentNode.parentY);

                    if (_closedList.TryGetValue(parentID, out PathNodeStruct parent))
                    {
                        currentNode = parent;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            finally
            {
                CacheReciclerHelper.Return(rentedHashSet); //clears automatically when returned
            }
        }

        public static bool checkCollisionRay(int fromX, int fromY, int toX, int toY, GameLocation location, Rectangle boundingBox, HashSet<(int, int)> rentedList)
        {
            if (fromX == toX && fromY == toY) { return true; }

            rentedList.Clear();
            bool collision = false;
            float bboxLenght = CalculateHypotenuse(boundingBox.Width, boundingBox.Height) / 2;
            double rayDirX = toX - fromX;
            double rayDirY = toY - fromY;
            double rayLength = Math.Sqrt((rayDirX * rayDirX) + (rayDirY * rayDirY));
            double rayDirNormalizedX = rayDirX / rayLength;
            double rayDirNormalizedY = rayDirY / rayLength;
            double rayDirNormalized16X = rayDirNormalizedX * 16;
            double rayDirNormalized16Y = rayDirNormalizedY * 16;

            int pointA1X = fromX + (int)(-rayDirNormalizedY * bboxLenght);
            int pointA1Y = fromY + (int)(rayDirNormalizedX * bboxLenght);
            int pointA2X = toX + (int)((-rayDirNormalizedY * bboxLenght) - rayDirNormalized16X);
            int pointA2Y = toY + (int)((rayDirNormalizedX * bboxLenght) - rayDirNormalized16Y);
            int pointB1X = fromX + (int)(-rayDirNormalizedY * -bboxLenght);
            int pointB1Y = fromY + (int)(rayDirNormalizedX * -bboxLenght);
            int pointB2X = toX + (int)((-rayDirNormalizedY * -bboxLenght) - rayDirNormalized16X);
            int pointB2Y = toY + (int)((rayDirNormalizedX * -bboxLenght) - rayDirNormalized16Y);

            HashSet<(int, int)> CollisionList = VoxelTraversal(pointB1X, pointB1Y, pointB2X, pointB2Y, Game1.tileSize, VoxelTraversal(pointA1X, pointA1Y, pointA2X, pointA2Y, Game1.tileSize, rentedList));
            foreach ((int _x, int _y) in CollisionList)
            {
                int recX = _x * 64;
                int recY = _y * 64;
                if (IsCollidingTilePosition(_x, _y, location, false, false))
                {
                    collision = true;
                    break;
                }
            }

            return collision;
        }
        public static float CalculateHypotenuse(int a, int b)
        {
            return MathF.Sqrt(MathF.Pow(a, 2) + MathF.Pow(b, 2));
        }
        private static HashSet<(int x, int y)> VoxelTraversal(double startX, double startY, double endX, double endY, int tileSize, HashSet<(int, int)> visitedP)
        {
            int startVoxelX = (int)Math.Floor(startX / tileSize);
            int startVoxelY = (int)Math.Floor(startY / tileSize);
            int endVoxelX = (int)Math.Floor(endX / tileSize);
            int endVoxelY = (int)Math.Floor(endY / tileSize);

            visitedP.Add((startVoxelX, startVoxelY));

            double rayDirX = endX - startX;
            double rayDirY = endY - startY;
            if (rayDirX == 0 && rayDirY == 0)
            {
                return visitedP;
            }
            int stepX = Math.Sign(rayDirX);
            int stepY = Math.Sign(rayDirY);
            double tMaxX = (rayDirX != 0) ? ((startVoxelX + (stepX > 0 ? 1 : 0)) * tileSize - startX) / rayDirX : double.MaxValue;
            double tMaxY = (rayDirY != 0) ? ((startVoxelY + (stepY > 0 ? 1 : 0)) * tileSize - startY) / rayDirY : double.MaxValue;

            double tDeltaX = (rayDirX != 0) ? tileSize / Math.Abs(rayDirX) : double.MaxValue;
            double tDeltaY = (rayDirY != 0) ? tileSize / Math.Abs(rayDirY) : double.MaxValue;

            int currentVoxelX = startVoxelX;
            int currentVoxelY = startVoxelY;

            int maxSteps = Math.Abs(endVoxelX - startVoxelX) + Math.Abs(endVoxelY - startVoxelY) + 4;
            int steps = 0;
            while ((currentVoxelX != endVoxelX || currentVoxelY != endVoxelY) && steps < maxSteps)
            {
                if (tMaxX < tMaxY)
                {
                    if (tMaxX > 1.0 + 1e-9) break;
                    currentVoxelX += stepX;
                    tMaxX += tDeltaX;
                }
                else
                {
                    if (tMaxY > 1.0 + 1e-9) break;
                    currentVoxelY += stepY;
                    tMaxY += tDeltaY;
                }
                visitedP.Add((currentVoxelX, currentVoxelY));
                steps++;
            }

            return visitedP;
        }
        public static Point? CheckAdjacentTilesForPassable(Point tile, GameLocation location, int radius = 1)
        {
            List<Point> availableTiles = CacheReciclerHelper.RentListPoint();

            Point? result = null;
            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    if (!IsCollidingTilePosition(tile.X + x, tile.Y + y, location, false, false))
                    {
                        availableTiles.Add(new Point(tile.X + x, tile.Y + y));
                    }
                }
            }
            if (availableTiles.Contains(tile))
            {
                result = tile;
            }
            else if (availableTiles.Count > 0)
            {
                result = availableTiles[Game1.random.Next(availableTiles.Count)];
            }

            CacheReciclerHelper.Return(availableTiles);

            return result;
        }

        public static bool IsCollidingTilePosition(int tileX, int tileY, GameLocation location, bool includeCharacters = false, bool includeFarmers = false)
        {
            bool flag = Game1.eventUp;
            if (flag && Game1.CurrentEvent != null && !Game1.CurrentEvent.ignoreObjectCollisions)
            {
                flag = false;
            }
            Rectangle position = new(tileX * 64, tileY * 64, 63, 63);

            location.updateMap();
            if (location.IsOutOfBounds(position))
            {
                return false;
            }

            for (int i = 0; i < location.buildings.Count; i++)
            {
                if (location.buildings[i].intersects(position))
                {
                    return true;
                }
            }

            for (int i = 0; i < location.resourceClumps.Count; i++)
            {
                if (location.resourceClumps[i].getBoundingBox().Intersects(position))
                {
                    return true;
                }
            }

            if (!flag)
            {
                for (int i = 0; i < location.furniture.Count; i++)
                {
                    if (location.furniture[i].furniture_type.Value != 12 && location.furniture[i].IntersectsForCollision(position))
                    {
                        return true;
                    }
                }
            }

            if (location.largeTerrainFeatures is not null)
            {
                for (int i = 0; i < location.largeTerrainFeatures.Count; i++)
                {
                    if (!location.largeTerrainFeatures[i].isPassable(Game1.player) && location.largeTerrainFeatures[i].getBoundingBox().Intersects(position))
                    {
                        return true;
                    }
                }
            }
            if (location.objects.TryGetValue(new Vector2(tileX, tileY), out var value3) && value3 is not null)
            {
                Microsoft.Xna.Framework.Rectangle boundingBox5 = value3.GetBoundingBox();
                if (!value3.isPassable() && boundingBox5.Intersects(position))
                {
                    return true;
                }
            }
            if (location.terrainFeatures.TryGetValue(new Vector2(tileX, tileY), out var value) && value is not null && value.getBoundingBox().Intersects(position) && !value.isPassable(Game1.player))
            {
                return true;
            }

            if (includeFarmers)
            {
                foreach (Farmer farmer2 in location.farmers)
                {
                    if (position.Intersects(farmer2.GetBoundingBox()))
                    {
                        return true;
                    }
                }

            }
            if (includeCharacters)
            {
                for (int num = location.characters.Count - 1; num >= 0; num--)
                {
                    NPC nPC = location.characters[num];
                    if (nPC != null && !nPC.IsMonster && !nPC.IsInvisible)
                    {
                        Microsoft.Xna.Framework.Rectangle boundingBox4 = nPC.GetBoundingBox();
                        if (nPC.layingDown)
                        {
                            boundingBox4.Y -= 64;
                            boundingBox4.Height += 64;
                        }
                        if (boundingBox4.Intersects(position))
                        {
                            return true;
                        }
                    }
                }

                if (location.animals.FieldDict.Count > 0)
                {
                    foreach (FarmAnimal value4 in location.animals.Values)
                    {
                        Microsoft.Xna.Framework.Rectangle boundingBox = value4.GetBoundingBox();
                        if (position.Intersects(boundingBox))
                        {
                            return true;
                        }
                    }
                }
            }
            Layer back_layer = location.map.RequireLayer("Back");
            Layer buildings_layer = location.map.RequireLayer("Buildings");
            Tile tmp = buildings_layer.Tiles[tileX, tileY];
            if (tmp is not null && !tmp.TileIndexProperties.ContainsKey("Shadow") && !tmp.TileIndexProperties.ContainsKey("Passable") && !tmp.Properties.ContainsKey("Passable"))
            {
                return true;
            }

            Tile tile4 = back_layer.Tiles[tileX, tileY];
            if (tile4 is not null && (tile4.TileIndexProperties.ContainsKey("Passable") && tile4.Properties.ContainsKey("Passable")))
            {

                return true;
            }

            return false;
        }
    }
}
