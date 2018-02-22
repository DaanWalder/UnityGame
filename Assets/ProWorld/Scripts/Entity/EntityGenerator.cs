using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine;

namespace ProWorldSDK
{
    [Serializable]
    public class EntityGenerator : IEntity, ISerializable
    {
        public enum TreePlacementQuality
        {
            Low = 1,
            Med = 3,
            High = 5,
            Precise = 20
        }

        [Flags]
        private enum BoxPosition
        {
            None = 0,
            Left = 1,
            Right = 2,
            Up = 4,
            UpLeft = Up | Left,
            UpRight = Up | Right,
            Down = 8,
            DownLeft = Down | Left,
            DownRight = Down | Right,
        }

        private struct OverflowData
        {
            public int NumOfBoxes;
            public float BoxSize;
            public float MaxOverflow;
        }

        public List<EntityLayer> EntityLayers = new List<EntityLayer>();
        public TreePlacementQuality PlacementQuality { get; set; }
        public MapManager DensityMap { get; set; }
        [NonSerialized] public float[,] Densisty;

        private readonly World _world;

        public EntityGenerator(World world)
        {
            _world = world;

            PlacementQuality = TreePlacementQuality.Precise;
            DensityMap = new MapManager();
        }

        public Entity[] GetEntities(WorldData data)
        {
            var resolution = data.Heights.GetLength(0);

            CreateDensityMap(resolution, data.Position.x, data.Position.y);
            GenerateAllEntities(data);
            return MergeLayers();
        }

        public void CreateDensityMap(int resolution, float offsetX = 0, float offsetY = 0)
        {
            Densisty = DensityMap.GetArea(resolution, offsetX, offsetY);
        }

        public void GenerateAllEntities(WorldData data)
        {
            for (var index = 0; index < EntityLayers.Count; index++)
            {
                GenerateEntities(data, index);
            }
        }

        public void GenerateEntities(WorldData data, int layer)
        {
            foreach(var group in EntityLayers[layer].Groups)
            {
                GenerateEntityGroup(data, group, layer);
            }
            MergeGroups(data, layer);
        }
        public void GenerateEntityGroup(WorldData data, EntityGroup eg, int layer)
        {
            PlaceEntities(eg);
            CleanupEntities(data, eg, layer);
        }
        public void MergeGroups(WorldData data, int layer)
        {
            // TODO ADD
            /*var l = EntityLayers[layer];

            var list = new List<Entity>();

            foreach (var group in l.Groups)
            {
                foreach (var box in group.Boxes)
                {
                    foreach (var e in box.Objects)
                    {
                        list.Add(e);
                    }
                }
            }

            l.Entities = list.ToArray();*/
        }

        public Entity[] MergeLayers()
        {
            // TODO CHANGE
            return (from layer in EntityLayers 
                    from groups in layer.Groups 
                    from box in groups.Boxes 
                    from e in box.Objects 
                    select e).ToArray();
        }

        private void PlaceEntities(EntityGroup eg)
        {
            var size = _world.TerrainWidth;

            // Sort trees by canopy size (Place big trees first)
            var trees = eg.Entities.OrderByDescending(x => x.Key.Radius[1]).ThenByDescending(x => x.Key.Radius[0]).ToList();
            if (trees.Count == 0)
            {
                eg.Boxes = new Box[0];
                return;
            }

            // For optimal performance: box width is 2x tree with highest radius. We limit this to a minimum of 5 though;
            var boxWidth = Mathf.Max(Mathf.CeilToInt(trees[0].Key.Radius[1])*2, 5);

            var num = Mathf.Max(size/boxWidth, 1);
            var ofd = new OverflowData
                          {
                              NumOfBoxes = num,
                              MaxOverflow = trees[0].Key.Radius[1],
                              // Max radius //trees.Select(t => t.Key.Radius[1]).Concat(new[] { 0f }).Max();
                              BoxSize = size/(float) num
                          };

            eg.Boxes = new Box[ofd.NumOfBoxes*ofd.NumOfBoxes];

            for (var h = 0; h < ofd.NumOfBoxes; h++)
            {
                for (var w = 0; w < ofd.NumOfBoxes; w++)
                {
                    // No Overflow yet
                    eg.Boxes[w + h*ofd.NumOfBoxes] = new Box();
                }
            }

            var totalDensity = trees.Sum(t => t.Value);
            var density = eg.Density/totalDensity;

            // We generate sequence of seeds so that each tree is linked individually
            var sign = eg.Seed < 0 ? -1 : 1; // sign used to do exact pattern for negative seeds
            var seed = Mathf.Abs(eg.Seed);
            var seedPrevious = 0;

            foreach (var treeData in trees)
            {
                var tmp = seed;
                seed += seedPrevious + 1;
                seedPrevious = tmp;
                var r = new System.Random(seed*sign);

                // We check if tree has a collider.
                // If it does, we always apply it precisely so experience is the same, otherwise use quality setting
                var quality = treeData.Key.IsHasCollider ? TreePlacementQuality.Precise : PlacementQuality;

                for (var i = 0; i < density*treeData.Value; i++)
                {
                    for (var j = 0; j < (int) quality; j++)
                    {
                        var pos = new Vector3((float) r.NextDouble()*size, 0,
                                              (float) r.NextDouble()*size);
                        var boxIndex = GetRelevantBoxIndex(pos.x, pos.z, ofd);

                        if (CheckCollision(boxIndex, pos.x, pos.z, treeData.Key, eg, ofd)) continue;

                        var tree = new Entity(treeData.Key)
                                       {
                                           Position = pos
                                       };
                        eg.Boxes[boxIndex].AddObject(tree);
                        break;
                    }
                }
            }
        }

        private void CleanupEntities(WorldData data, EntityGroup eg, int layer)
        {
            var size = _world.TerrainWidth;

            var layerMask = data.LayerMasks;
            var mask = MergeMasks(layer, layerMask, eg);
            var mapLength = mask.GetLength(0);

            var nfactor = mapLength/(float) size; // size > maplength in general

            var random = new System.Random(eg.Seed);

            var heights = data.Heights;

            // Cleanup
            // Remove trees in water
            // Remove trees in other sections
            foreach (var box in eg.Boxes)
            {
                for (var index = 0; index < box.Objects.Count; index++)
                {
                    var remove = false;
                    var tree = box.Objects[index];

                    var xx = (int)(tree.Position.x * nfactor);
                    var yy = (int)(tree.Position.z * nfactor);

                    var height = heights[yy, xx];

                    if (!eg.IsInWater && height < _world.Water.WaterLevel)
                    {
                        remove = true;
                    }
                    // If not check if we are in the right region
                    else
                    {
                        var count = 0;
                        var current = false;

                        for (var x = 0; x < xx; x++)
                        {
                            if (current)
                            {
                                if (!mask[yy, xx])
                                {
                                    current = false;
                                    count++;
                                }
                            }
                            else
                            {
                                if (mask[yy, xx])
                                {
                                    current = true;
                                    count++;
                                }
                            }
                        }

                        if (count%2 == 0) // remove because it is not part of layer
                        {
                            remove = true;
                        }
                        else if(eg.IsUseDensityMap) // otherwise apply a probability of removal
                        {
                            // TODO ADD MINIMUM/MAXIMUM CHANCE
                            if (random.NextDouble() * (0.8f - 0.1f) > Densisty[yy, xx] - 0.1f) //  r > (v - min) / (max - min) || r (max - min) > v - min
                            {
                                remove = true;
                            }
                        }
                    }

                    if (remove)
                    {
                        box.Objects.Remove(tree);
                        index--;
                    }
                }
            }
        }

        private bool[,] MergeMasks(int layer, int[,] layerMask, EntityGroup eg)
        {
            //var lm = Util.ResizeArray(layerMask, resolution);
            var size = layerMask.GetLength(0);
            var mask = new bool[size, size];

            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
// ReSharper disable LoopCanBeConvertedToQuery
                    foreach (var area in eg.Area)
// ReSharper restore LoopCanBeConvertedToQuery
                    {
                        if (area.MaskArea != null)
                        {
                            var nfactor = area.MaskArea.GetLength(0)/(float) size;

                            if (area.MaskArea[(int) (y*nfactor), (int) (x*nfactor)] && layerMask[y, x] == layer)
                            {
                                mask[y, x] = true;
                                break;
                            }
                        }
                    }
                }
            }

            return mask;
        }

        /// <summary>
        /// Function to check if tree collides with any other tree
        /// </summary>
        /// <param name="index">Box index</param>
        /// <param name="x">X position of tree</param>
        /// <param name="y">Y position of tree</param>
        /// <param name="ed">Tree data</param>
        /// <param name="eg">Tree section data</param>
        /// <param name="ofd"> </param>
        /// <returns></returns>
        private static bool CheckCollision(int index, float x, float y, EntityData ed, EntityGroup eg, OverflowData ofd)
        {
            // Find the largest radius
            var rad = ed.Radius[0] > ed.Radius[1] ? 0 : 1;

            // Check box
            if ((from obj in eg.Boxes[index].Objects
                 let pos = obj.Position
                 where InCircle(pos.x, pos.z, obj.Data.Radius[rad], x, y, ed.Radius[rad])
                 select obj
                ).Any())
            {
                return true;
            }

            var overflowArea = GetOverflowBoxes(index, x, y, ed.Radius[rad], eg, ofd);

            // Check overflow areas
            return (from box in overflowArea
                    from obj in box.Objects
                    let pos = obj.Position
                    where InCircle(pos.x, pos.z, obj.Data.Radius[rad], x, y, ed.Radius[rad])
                    select obj
                   ).Any();
        }

        private static int GetRelevantBoxIndex(float x, float y, OverflowData ofd)
        {
            var w = Mathf.Clamp((int)(x / ofd.BoxSize), 0, ofd.NumOfBoxes - 1);
            // We clamp on rare case we are exactly at max position
            var h = Mathf.Clamp((int)(y / ofd.BoxSize), 0, ofd.NumOfBoxes - 1);

            return w + h * ofd.NumOfBoxes;
        }

        private static IEnumerable<Box> GetOverflowBoxes(int box, float x, float y, float radius, EntityGroup tsd, OverflowData ofd)
        {
            var overflow = ofd.MaxOverflow + radius;

            // Box index in x/y coords
            var boxX = box % ofd.NumOfBoxes;
            var boxY = box / ofd.NumOfBoxes;

            // Remainder to check if we are overflowing
            var posX = x % ofd.BoxSize;
            var posY = y % ofd.BoxSize;

            var boxes = new List<Box>();
            var newBoxes = BoxPosition.None;

            if (posX < overflow && boxX != 0)
                newBoxes |= BoxPosition.Left;
            if (posX > ofd.BoxSize - overflow && boxX != ofd.NumOfBoxes - 1)
                newBoxes |= BoxPosition.Right;
            if (posY < overflow && boxY != 0)
                newBoxes |= BoxPosition.Down;
            if (posY > ofd.BoxSize - overflow && boxY != ofd.NumOfBoxes - 1)
                newBoxes |= BoxPosition.Up;

            if ((newBoxes & BoxPosition.Left) == BoxPosition.Left)
                boxes.Add(tsd.Boxes[boxX - 1 + boxY * ofd.NumOfBoxes]);

            if ((newBoxes & BoxPosition.Right) == BoxPosition.Right)
                boxes.Add(tsd.Boxes[boxX + 1 + boxY * ofd.NumOfBoxes]);

            if ((newBoxes & BoxPosition.Down) == BoxPosition.Down)
                boxes.Add(tsd.Boxes[boxX + (boxY - 1) * ofd.NumOfBoxes]);

            if ((newBoxes & BoxPosition.Up) == BoxPosition.Up)
                boxes.Add(tsd.Boxes[boxX + (boxY + 1) * ofd.NumOfBoxes]);

            if ((newBoxes & BoxPosition.DownLeft) == BoxPosition.DownLeft)
                boxes.Add(tsd.Boxes[boxX - 1 + (boxY - 1) * ofd.NumOfBoxes]);

            if ((newBoxes & BoxPosition.DownRight) == BoxPosition.DownRight)
                boxes.Add(tsd.Boxes[boxX + 1 + (boxY - 1) * ofd.NumOfBoxes]);

            if ((newBoxes & BoxPosition.UpLeft) == BoxPosition.UpLeft)
                boxes.Add(tsd.Boxes[boxX - 1 + (boxY + 1) * ofd.NumOfBoxes]);

            if ((newBoxes & BoxPosition.UpRight) == BoxPosition.UpRight)
                boxes.Add(tsd.Boxes[boxX + 1 + (boxY + 1) * ofd.NumOfBoxes]);

            return boxes;
        }
        private static bool InCircle(float x1, float y1, float r1, float x2, float y2, float r2)
        {
            var squareDist = Mathf.Pow(x1 - x2, 2) + Mathf.Pow(y1 - y2, 2);
            return squareDist < Mathf.Pow(r1 + r2, 2);
        }

        public EntityGenerator(SerializationInfo info, StreamingContext context)
        {
            EntityLayers = (List<EntityLayer>)info.GetValue("EntityLayers", typeof(List<EntityLayer>));
            PlacementQuality = (TreePlacementQuality) info.GetValue("PlacementQuality", typeof (TreePlacementQuality));
            DensityMap = (MapManager) info.GetValue("DensityMap", typeof (MapManager));
            _world = (World)info.GetValue("World", typeof(World));
        }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("EntityLayers", EntityLayers);
            info.AddValue("PlacementQuality", PlacementQuality);
            info.AddValue("DensityMap", DensityMap);
            info.AddValue("World", _world);
        }
    }
}
