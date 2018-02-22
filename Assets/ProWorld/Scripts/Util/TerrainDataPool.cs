using System.Collections.Generic;
using UnityEngine;

namespace ProWorldSDK
{
	public static class TerrainDataPool
	{
        private static int _pool;

	    private static readonly Stack<TerrainData> Available = new Stack<TerrainData>();
        private static readonly List<TerrainData> InUse = new List<TerrainData>();

        public static void Setup(int pool)
        {
            ResizePool(pool);
        }

        public static void ResizePool(int neighbours)
	    {
	        var pool = (int) Mathf.Pow(neighbours*2 + 1, 2)*2;

            var current = InUse.Count + Available.Count;
	        var difference = pool - current;
            
            if (difference > 0)
            {
                for (var i = 0; i < difference; i++)
                {
                    var td = CreateNewTerrainData();
                    Available.Push(td);
                }
            }
            else if (difference < 0)
            {
                if (Available.Count < Mathf.Abs(difference))
                {
                    throw new UnityException("Trying to remove terrainData that is in use");
                }

                for (var i = 0; i < Mathf.Abs(difference); i++)
                {
                    Available.Pop();
                }
            }
        }

        public static TerrainData GetNext()
        {
            if (Available.Count == 0)
            {
                var newTd = CreateNewTerrainData();
                Available.Push(newTd);

                Debug.Log("Ran out of terrainData: creating more. This shouldn't happen, you are generating more than can be applied.");
            }

            var td = Available.Pop();
            InUse.Add(td);

            return td;
        }

        private static TerrainData CreateNewTerrainData()
        {
            var td = new TerrainData();

            return td;
        }

        public static void FreeUp(TerrainData td)
        {
            if (!InUse.Contains(td)) return;

            InUse.Remove(td);
            Available.Push(td);

            td.treeInstances = new TreeInstance[0];
            td.SetHeights(0,0,new float[0,0]);

            td.splatPrototypes = new SplatPrototype[0];
        }

        public static int AvailableCount()
        {
            return Available.Count;
        }
	}
}
