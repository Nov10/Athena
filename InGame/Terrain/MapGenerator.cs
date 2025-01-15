using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Athena.Maths;
using System.Threading;
using Athena.Engine.Core.Image;
using Athena.Engine.Core;

namespace Athena.InGame.Terrain
{
    [System.Serializable]
    public struct TerrainType
    {
        public string Name;
        public float Height;
        public Color Color;

        public TerrainType(string name, float height, Color color)
        {
            Name = name;
            Height = height;
            Color = color;
        }
    }

    public struct MapData
    {
        public readonly float[,] HeightMap;
        public readonly Color[] ColorMap;

        public MapData(float[,] heightMap, Color[] colorMap)
        {
            HeightMap = heightMap;
            ColorMap = colorMap;
        }
    }

    public class MapGenerator : Component
    {
        struct CallbackThreadInfo<T>
        {
            public readonly Action<T> Callback;
            public readonly T Parameter;

            public CallbackThreadInfo(Action<T> callback, T parameter)
            {
                Callback = callback;
                Parameter = parameter;
            }
        }

        //MapCunckSize가 크면 클수록 드로우콜이 줄어들여 유리합니다.
        public const int MapCunckSize = 256 + 1;
        public Noise.eNormalizeMode NormalizeMode;

        float NoiseScale;
        int Octaves;
        float Persistance;
        float Lacunarity;
        float MeshHeightMultiplier;

        public int Seed;
        public Vector2 offset;

        TerrainType[] Regions;

        public MapGenerator(float noiseScale, int octave, float persistance, float lacunarity, float heightMultiplier, TerrainType[] regions)
        {
            NoiseScale = noiseScale;
            Octaves = octave;
            Persistance = persistance;
            Lacunarity = lacunarity;
            MeshHeightMultiplier = heightMultiplier;
            Regions = regions;

            if (Lacunarity < 1)
                Lacunarity = 1;
            if (Octaves < 0)
                Octaves = 0;
        }

        Queue<CallbackThreadInfo<MapData>> MapDataThreadInfoQueue = new Queue<CallbackThreadInfo<MapData>>();
        Queue<CallbackThreadInfo<TerrainMeshData>> MeshDataThreadInfoQueue = new Queue<CallbackThreadInfo<TerrainMeshData>>();

        /// <summary>
        /// Map Data의 생성을 요청합니다. 생성 이후 callback을 실행합니다.
        /// </summary>
        public void RequestMapData(Vector2 center, Action<MapData> callback)
        {
            ThreadStart threadStart = delegate
            {
                MapDataThread(center, callback);
            };

            new Thread(threadStart).Start();
        }
        void MapDataThread(Vector2 center, Action<MapData> callback)
        {
            MapData data = GenerateMapData(center);
            lock (MapDataThreadInfoQueue)
            {
                MapDataThreadInfoQueue.Enqueue(new CallbackThreadInfo<MapData>(callback, data));
            }
        }
        /// <summary>
        /// Terrain Mesh Data의 생성을 요청합니다. 생성 이후 callback을 실행합니다.
        /// </summary>
        public void RequestMeshData(MapData data, int lod, Action<TerrainMeshData> callback)
        {
            ThreadStart threadStart = delegate
            {
                MeshDataThread(data, lod, callback);
            };

            new Thread(threadStart).Start();
        }
        void MeshDataThread(MapData mapData, int lod, Action<TerrainMeshData> callback)
        {
            TerrainMeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.HeightMap, MeshHeightMultiplier, lod);
            lock (MeshDataThreadInfoQueue)
            {
                MeshDataThreadInfoQueue.Enqueue(new CallbackThreadInfo<TerrainMeshData>(callback, meshData));
            }
        }
        public MapData GenerateMapData(Vector2 center)
        {
            float[,] noiseMap = Noise.GenerateNoiseMap(MapCunckSize, MapCunckSize, NoiseScale, Seed, Octaves, Persistance, Lacunarity, offset + center, NormalizeMode);


            Color[] colorMap = new Color[MapCunckSize * MapCunckSize];
            for (int y = 0; y < MapCunckSize; y++)
            {
                for (int x = 0; x < MapCunckSize; x++)
                {
                    float currentHeight = noiseMap[x, MapCunckSize - y - 1];
                    for (int i = 0; i < Regions.Length; i++)
                    {
                        if (currentHeight >= Regions[i].Height)
                        {
                            colorMap[y * MapCunckSize + x] = Regions[i].Color;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }

            return new MapData(noiseMap, colorMap);
        }

        public override void Update()
        {
            //누적된 callback을 실행합니다.
            if (MapDataThreadInfoQueue.Count > 0)
            {
                for (int i = 0; i < MapDataThreadInfoQueue.Count; i++)
                {
                    CallbackThreadInfo<MapData> thredInfo = MapDataThreadInfoQueue.Dequeue();
                    thredInfo.Callback(thredInfo.Parameter);
                }
            }
            if (MeshDataThreadInfoQueue.Count > 0)
            {
                for (int i = 0; i < MeshDataThreadInfoQueue.Count; i++)
                {
                    CallbackThreadInfo<TerrainMeshData> thredInfo = MeshDataThreadInfoQueue.Dequeue();
                    thredInfo.Callback(thredInfo.Parameter);
                }
            }
        }


        public override void Awake()
        {
        }

        public override void Start()
        {
        }
    }
}
