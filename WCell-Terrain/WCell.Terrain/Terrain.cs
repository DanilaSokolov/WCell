﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.Constants.World;
using WCell.Core.Paths;
using WCell.Core.Terrain;
using WCell.Terrain.Collision;
using WCell.Terrain.Recast.NavMesh;
using WCell.Util.Graphics;
using Path = System.IO.Path;

namespace WCell.Terrain
{
    ///<summary>
    /// TODO: Read GO collision data, using GameobjectDisplayInfo.dbc
    /// TODO: Global list of vertices, indices & neighbors
	///</summary>
	public abstract class Terrain : ITerrain
	{
		///<summary>
		/// A profile of the tiles in this map
		/// Contains true if Tile [y, x] exists
		///</summary>
		public readonly bool[,] TileProfile = new bool[TerrainConstants.TilesPerMapSide, TerrainConstants.TilesPerMapSide];

		public readonly TerrainTile[,] Tiles = new TerrainTile[TerrainConstants.TilesPerMapSide, TerrainConstants.TilesPerMapSide];

		protected Terrain(MapId mapId, bool loadOnDemand = true)
		{
			MapId = mapId;
			LoadOnDemand = loadOnDemand;
		}

		public abstract bool IsWMOOnly { get; }

    	public abstract void FillTileProfile();

		public MapId MapId
		{
			get; 
			private set;
		}

    	public bool LoadOnDemand
    	{
    		get;
			set;
    	}

		#region Terrain Queries

    	public bool IsAvailable(int tileX, int tileY)
    	{
			if (PositionUtil.VerifyPoint2D(tileX, tileY))
			{
				return TileProfile[tileX, tileY];
			}
			return false;
    	}

    	public bool HasLOS(Vector3 startPos, Vector3 endPos)
		{
			//float tMax;
			//var ray = CollisionUtil.CreateRay(startPos, endPos, out tMax);
			//var intTMax = (int)(tMax + 0.5f);

			//for (var t = 0; t < intTMax; t++)
			//{
			//    var currentPos = ray.Position + ray.Direction * t;

			//    Point2D tileCoord;
			//    Point2D chunkCoord;
			//    Point2D unitCoord;
			//    var currentHeightMapFraction = PositionUtil.GetHeightMapFraction(currentPos,
			//                                                                    out tileCoord,
			//                                                                    out chunkCoord,
			//                                                                    out unitCoord);

			//    var currentTile = GetTile(tileCoord);
			//    if (currentTile == null)
			//    {
			//        // Can't check non-existant tiles.
			//        return false;
			//    }

			//    var terrainHeight = currentTile.GetInterpolatedHeight(chunkCoord, unitCoord, currentHeightMapFraction);

			//    if (terrainHeight < currentPos.Z) continue;
			//    return false;
			//}

			return true;
		}

		public float GetHeightUnderneath(Vector3 worldPos)
		{
			int tileX, tileY;
			if (PositionUtil.GetTileXYForPos(worldPos, out tileX, out tileY))
			{
				// shoot a ray straight down
				var tile = GetOrLoadTile(tileX, tileY);
				if (tile != null)
				{
					float dist;
					if (tile.FindFirstTriangleUnderneath(worldPos, out dist) != -1)
					{
						return worldPos.Z - dist;
					}
					return TerrainConstants.MinHeight;		// TODO: Minimal z value
				}
			}
			return float.NaN;								// Could not reliably lookup the value
		}

		public void FindPath(PathQuery query)
		{
			var wayPoints = FindPath(query.From, query.To);
			query.Reply(wayPoints);
		}

		/// <summary>
		/// TODO: Connect meshes between different tiles
		/// </summary>
		public Vector3[] FindPath(Vector3 from, Vector3 to)
		{
			int fromX, fromY, toX, toY;
			if (!PositionUtil.GetTileXYForPos(from, out fromX, out fromY)) return null;
			if (!PositionUtil.GetTileXYForPos(to, out toX, out toY)) return null;

			if (toX != fromX || toY != fromY)
			{
				return null;
			}

			TerrainTile tile1, tile2;
			if (LoadOnDemand)
			{
				tile1 = GetOrLoadTile(fromX, fromY);
				//tile2 = GetOrLoadTile(toX, toY);
			}
			else
			{
				tile1 = Tiles[fromX, fromY];
				//tile2 = Tiles[toX, toY];
			}

			// cannot traverse tiles yet
			if (tile1 == null)
			{
				return null;
			}

			return tile1.Pathfinder.FindPathUnflavored(from, to);
		}

		//public FluidType GetFluidTypeAtPoint(Vector3 worldPos)
		//{
		//    Point2D tileCoord;
		//    var chunkCoord = PositionUtil.GetXYForPos(worldPos, out tileCoord);

		//    var tile = GetTile(tileCoord);
		//    if (tile == null) return FluidType.None;

		//    return tile.GetFluidType(chunkCoord);
		//}
		#endregion

		public TerrainTile GetOrLoadTile(int x, int y)
		{
			// get loaded tile
			var tile = Tiles[x, y];
			if (tile != null) return tile;

			// check whether the tile exists
			if (!TileProfile[x, y]) return null;

			// if it exists, try to load it from disk
			tile = LoadTile(x, y);
			TileProfile[x, y] = tile != null;
			
			return tile;
		}

		public bool EnsureGroupLoaded(int x, int y)
		{
			var result = true;

			result = GetOrLoadTile(x, y) != null;

			var origX = x;
			var origY = y;
			
			/* 
			 * The given coordinates represent the center tile T and we want to make sure
			 * that all X's are loaded as well.
			 * 
			 * x x x
			 * x T x
			 * x x x
			 */

			x = origX - 1;
			FixCoordsXMin(ref x);
			result = result && GetOrLoadTile(x, y) != null;

			y = origY - 1;
			FixCoordsYMin(ref y);
			result = result && GetOrLoadTile(x, y) != null;

			y = origY + 1;
			FixCoordsYMax(ref y);
			result = result && GetOrLoadTile(x, y) != null;


			x = origX + 1;
			FixCoordsXMax(ref x);
			result = result && GetOrLoadTile(x, y) != null;

			y = origY - 1;
			FixCoordsYMin(ref y);
			result = result && GetOrLoadTile(x, y) != null;

			y = origY + 1;
			FixCoordsYMax(ref y);
			result = result && GetOrLoadTile(x, y) != null;


			x = origX;
			y = origY - 1;
			FixCoordsYMin(ref y);
			result = result && GetOrLoadTile(x, y) != null;

			y = origY + 1;
			FixCoordsYMax(ref y);
			result = result && GetOrLoadTile(x, y) != null;

			return result;
		}

		static void FixCoordsXMin(ref int x)
		{
			x = Math.Max(x, 0);
		}
		static void FixCoordsXMax(ref int x)
		{
			x = Math.Min(x, TerrainConstants.TilesPerMapSide);
		}
		static void FixCoordsYMin(ref int y)
		{
			y = Math.Max(y, 0);
		}
		static void FixCoordsYMax(ref int y)
		{
			y = Math.Min(y, TerrainConstants.TilesPerMapSide);
		}

    	protected abstract TerrainTile LoadTile(int x, int y);

    	//public void DumpMapTileChunk(Vector3 worldPos)
    	//{
    	//    Point2D tileCoord;
    	//    var chunkCoord = PositionUtil.GetXYForPos(worldPos, out tileCoord);

    	//    var tile = GetTile(tileCoord);
    	//    if (tile == null) return;

    	//    tile.DumpChunk(chunkCoord);
    	//}
	}
}
