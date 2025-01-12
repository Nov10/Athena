using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Athena.Maths;
using System.Threading;
using Athena.Engine.Core.Image;
using Athena.Engine.Core;

namespace Athena.Terrain
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
        struct MapThreadInfo<T>
        {
            public readonly Action<T> Callback;
            public readonly T Parameter;

            public MapThreadInfo(Action<T> callback, T parameter)
            {
                Callback = callback;
                Parameter = parameter;
            }
        }

        public enum eDrawaMode
        {
            NoiseMap = 0,
            ColorMap,
            DrawMesh
        }
        public const int MapCunckSize = 32 + 1;
        //[Range(0, 6)]
        public int EditorPreviewLevlOfDetail;
        public eDrawaMode DrawMode;
        public Noise.eNormalizeMode NormalizeMode;

        public float NoiseScale;
        public int Octaves;
        //[Range(0, 1)]
        public float Persistance;
        public float Lacunarity;

        public float MeshHeightMultiplier;
        //public AnimationCurve MeshHeightCurve;

        public int Seed;
        public Vector2 offset;

        public bool AutoUpdate;

        public TerrainType[] Regions;

        Queue<MapThreadInfo<MapData>> MapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
        Queue<MapThreadInfo<MeshData>> MeshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();


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
                MapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, data));
            }
        }

        public void RequestMeshData(MapData data, int lod, Action<MeshData> callback)
        {
            ThreadStart threadStart = delegate
            {
                MeshDataThread(data, lod, callback);
            };

            new Thread(threadStart).Start();
        }
        void MeshDataThread(MapData mapData, int lod, Action<MeshData> callback)
        {
            MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.HeightMap, MeshHeightMultiplier, lod);
            lock (MeshDataThreadInfoQueue)
            {
                MeshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
            }
        }
        public override void Update()
        {
            if (MapDataThreadInfoQueue.Count > 0)
            {
                for (int i = 0; i < MapDataThreadInfoQueue.Count; i++)
                {
                    MapThreadInfo<MapData> thredInfo = MapDataThreadInfoQueue.Dequeue();
                    thredInfo.Callback(thredInfo.Parameter);
                }
            }
            if (MeshDataThreadInfoQueue.Count > 0)
            {
                for (int i = 0; i < MeshDataThreadInfoQueue.Count; i++)
                {
                    MapThreadInfo<MeshData> thredInfo = MeshDataThreadInfoQueue.Dequeue();
                    thredInfo.Callback(thredInfo.Parameter);
                }
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

        public void DrawMapInEditor()
        {
            MapData data = GenerateMapData(Vector2.zero);

            if(Controller.TryGetComponent(out MapDisplay display))
            {
                switch (DrawMode)
                {
                    case eDrawaMode.NoiseMap:
                        display.DrawTexture(TextureGenerator.TextureFromHeightMap(data.HeightMap));
                        break;
                    case eDrawaMode.ColorMap:
                        display.DrawTexture(TextureGenerator.TextureFromColorMap(data.ColorMap, MapCunckSize, MapCunckSize));
                        break;
                    case eDrawaMode.DrawMesh:
                        display.DrawMesh(MeshGenerator.GenerateTerrainMesh(data.HeightMap, MeshHeightMultiplier, EditorPreviewLevlOfDetail),
                                        TextureGenerator.TextureFromColorMap(data.ColorMap, MapCunckSize, MapCunckSize));
                        break;
                }
            }
        }

        private void OnValidate()
        {
            if (Lacunarity < 1)
                Lacunarity = 1;
            if (Octaves < 0)
                Octaves = 0;
        }

        public override void Awake()
        {
        }

        public override void Start()
        {
        }
    }
}
