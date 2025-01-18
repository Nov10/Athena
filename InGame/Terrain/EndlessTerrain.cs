using System;
using System.Collections.Generic;
using System.Linq;
using Athena.Maths;
using Athena.Engine.Core;
using Athena.Engine.Core.Image;
using Athena.Engine.Core.Rendering;
using Athena.Engine.Core.Rendering.Shaders;

namespace Athena.InGame.Terrain
{
    public class EndlessTerrain : Component
    {
        const float Scale = 3f;

        const float ViewerMoveThresholdForChunckUpdate = 10;
        const float sqrViewerMoveThresholdForChunckUpdate = ViewerMoveThresholdForChunckUpdate * ViewerMoveThresholdForChunckUpdate;

        Vector2 preViewerPosition;

        public LODInfo[] DetailLevels;
        public static float MaxViewDistance;
        public GameObject Viewer;

        static MapGenerator MapGeneratorReference;

        public static Vector2 ViewerPosition;
        int ChunkSize;
        int ChunkVisibleInViewDistance;

        Dictionary<Vector2, TerrainChunck> TerrainChuncks = new Dictionary<Vector2, TerrainChunck>();
        static List<TerrainChunck> TerrainChuncksVisibleLastUpdate = new List<TerrainChunck>();

        public void Initialize(MapGenerator generator)
        {
            MapGeneratorReference = generator;
        }

        public override void Start()
        {
            MaxViewDistance = DetailLevels[DetailLevels.Length - 1].VisibleDistanceThreshold;
            ChunkSize = MapGenerator.MapCunckSize - 1;
            ChunkVisibleInViewDistance = (int)System.MathF.Round(MaxViewDistance) / ChunkSize;

            UpdateVisibleChuncks();
        }

        public override void Update()
        {
            ViewerPosition = new Vector2(Viewer.WorldPosition.x, Viewer.WorldPosition.z) / Scale;
            if ((preViewerPosition - ViewerPosition).sqrMagnitude > sqrViewerMoveThresholdForChunckUpdate)
            {
                preViewerPosition = ViewerPosition;
                UpdateVisibleChuncks();
            }
        }
        public Camera cam;
        void UpdateVisibleChuncks()
        {

            for (int i = 0; i < TerrainChuncksVisibleLastUpdate.Count; i++)
            {
                TerrainChuncksVisibleLastUpdate[i].SetVisible(false);
            }
            TerrainChuncksVisibleLastUpdate.Clear();
            int currentChunkCorrdX = XMath.RoundToInt(ViewerPosition.x / ChunkSize);
            int currentChunkCorrdY = XMath.RoundToInt(ViewerPosition.y / ChunkSize);

            for (int yOffset = -ChunkVisibleInViewDistance; yOffset <= ChunkVisibleInViewDistance; yOffset++)
            {
                for (int xOffset = -ChunkVisibleInViewDistance; xOffset <= ChunkVisibleInViewDistance; xOffset++)
                {
                    Vector2 viewedChunckCorrd = new Vector2(currentChunkCorrdX + xOffset, currentChunkCorrdY + yOffset);

                    if (TerrainChuncks.ContainsKey(viewedChunckCorrd))
                    {
                        TerrainChuncks[viewedChunckCorrd].UpdateTerrainChunck();
                        //if (TerrainChuncks[viewedChunckCorrd].IsVisible())
                        //{
                        //    TerrainChuncksVisibleLastUpdate.Add(TerrainChuncks[viewedChunckCorrd]);
                        //}
                    }
                    else
                    {
                        TerrainChuncks.Add(viewedChunckCorrd, new TerrainChunck(viewedChunckCorrd, ChunkSize, DetailLevels, Controller));
                    }
                }
            }
        }

        public string Get()
        {
            Dictionary<Vector2, bool> skipResults = new Dictionary<Vector2, bool>();

            foreach (KeyValuePair<Vector2, TerrainChunck> entry in TerrainChuncks)
            {
                var corners = entry.Value.RenderComponenet.RenderDatas[0].ThisAABB.GetCorners();
                Matrix4x4 cameraTransform = cam.CalculateVPMatrix();
                Matrix4x4 objectTransform = entry.Value.RenderComponenet.CreateObjectTransformMatrix();
                Matrix4x4 transform = cameraTransform * objectTransform;

                bool skipNowData = true;

                foreach (var corner in corners)
                {
                    Vector3 p = corner;
                    var clipPoint = TransformMatrixCaculator.TransformH(p, transform);
                    float w = clipPoint.w;

                    // 프러스텀 범위 내에 있는지 검사
                    if ((-w <= clipPoint.x && clipPoint.x <= w) ||
                        (-w <= clipPoint.y && clipPoint.y <= w) ||
                        (-w <= clipPoint.z && clipPoint.z <= w))
                    {
                        skipNowData = false;
                        break;
                    }
                }

                // Dictionary에 기록 ("이 위치의 Chunk는 건너뛰어야 하나?"의 여부)
                skipResults[entry.Key] = skipNowData;
            }

            // 2. skipResults에 저장된 위치들의 x, y 최소/최대값 구하기
            var allPositions = skipResults.Keys.ToList();

            float minX = allPositions.Min(pos => pos.x);
            float maxX = allPositions.Max(pos => pos.x);
            float minY = allPositions.Min(pos => pos.y);
            float maxY = allPositions.Max(pos => pos.y);

            // 혹시 Vector2의 x,y가 정수가 아니라면, (int) 변환 과정에서
            // 원하는 형태로 범위를 조정해야 합니다. (예: Floor, Ceil 등)
            int minXi = XMath.FloorToInt(minX);
            int maxXi = XMath.CeilToInt(maxX);
            int minYi = XMath.FloorToInt(minY);
            int maxYi = XMath.CeilToInt(maxY);

            // 3. 2중 for문을 돌면서 행렬 형태로 "O" 또는 "X" 출력
            // 일반적으로 y는 위에서 아래로 내려가므로 maxY -> minY 순으로 갈 수도 있고
            // 필요에 따라 반대로 하시면 됩니다.
            string result = string.Empty;
            int currentChunkCorrdX = XMath.RoundToInt(ViewerPosition.x / ChunkSize);
            int currentChunkCorrdY = XMath.RoundToInt(ViewerPosition.y / ChunkSize);
            Vector2 viewerP = new Vector2(currentChunkCorrdX, currentChunkCorrdY);
                // 한 줄(문자열)을 구성할 StringBuilder (또는 그냥 +연산)를 사용
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int y = maxYi; y >= minYi; y--)  // 위에서 아래 순
            {

                for (int x = minXi; x <= maxXi; x++)
                {
                    Vector2 pos = new Vector2(x, y);

                    if((pos - viewerP).sqrMagnitude < 1)
                    {
                        sb.Append("P ");
                    }

                    // skipResults에 해당 키가 있으면 O/X, 없으면 다른 표시(예: ".")
                    else if (skipResults.ContainsKey(pos))
                    {
                        sb.Append(skipResults[pos] ? "O " : "x ");
                    }
                    else
                    {
                        sb.Append(". ");
                    }
                }

                // 완성된 문자열을 한 줄로 출력(콘솔/로그 등)
                sb.Append("\n");
            }
            return sb.ToString();
        }

        public override void Awake()
        {
        }


        public class TerrainChunck
        {
            GameObject MeshObject;
            Vector2 position;
            //Bounds Bounds;
            Bounds Bound;
            public Athena.Engine.Core.MeshRenderer RenderComponenet;
            //MeshRenderer MeshRenderer;
            //MeshFilter MeshFilter;

            LODInfo[] Detailevels;
            LODMesh[] LODMeshes;

            MapData MapData;
            bool MapDataReceived;
            int previousLODIndex = -1;
            NBitmap tex;
            void OnMapDataReceived(MapData data)
            {
                MapDataReceived = true;
                MapData = data;
                
                tex = TextureGenerator.TextureFromColorMap(data.ColorMap, MapGenerator.MapCunckSize, MapGenerator.MapCunckSize);
                Texture2DHelper.ConvertFromBitmap((RenderComponenet.RenderDatas[0].Shader as NormalShader).MainTexture, tex);
                //RenderComponenet.RenderDatas[0] = MeshGenerator.GenerateTerrainMesh(data.HeightMap, MeshHeightMultiplier, EditorPreviewLevlOfDetail).CreateMesh();
                //(RenderComponenet.RenderDatas[0].Shader as Shader1).MainTexture = texture;
                UpdateTerrainChunck();
                //MapGeneratorReference.RequestMeshData(data, OnMeshDataReceived);
            }

            void OnMeshDataReceived(TerrainMeshData data)
            {
                //MeshFilter.mesh = data.CreateMesh();
            }

            public TerrainChunck(Vector2 coord, int size, LODInfo[] lods, GameObject parent)
            {
                Detailevels = lods;
                position = coord * size;
                Bound = new Bounds(new Vector3(position.x, 0, position.y), new Vector3(1,0,1) * size);
                Vector3 position3 = new Vector3(position.x, 0, position.y);


                MeshObject = new GameObject("Terrain Chunck");
                RenderComponenet = new Athena.Engine.Core.MeshRenderer();
                MeshObject.AddComponent(RenderComponenet);
                MeshObject.Parent = parent;
                MeshObject.WorldPosition = position3 * Scale;
                MeshObject.LocalScale = new Vector3(1,1,1) * Scale;
                var d = new RenderData();
                d.Shader = new NormalShader();
                RenderComponenet.RenderDatas.Add(d);

                SetVisible(false);

                LODMeshes = new LODMesh[Detailevels.Length];
                for (int i = 0; i < Detailevels.Length; i++)
                {
                    LODMeshes[i] = new LODMesh(Detailevels[i].LOD, UpdateTerrainChunck);
                }

                MapGeneratorReference.RequestMapData(position, OnMapDataReceived);
            }

            public void UpdateTerrainChunck()
            {
                if (MapDataReceived)
                {
                    //float viewerDistanceNearestEdge =((new Vector3(ViewerPosition.x, 0, ViewerPosition.y) - new Vector3(MeshObject.WorldPosition.x, 0, MeshObject.WorldPosition.z)).magnitude);// MathF.Sqrt(Bound.SqrDistance(new Vector3(ViewerPosition.x, 0, ViewerPosition.y)));
                    float viewerDistanceNearestEdge =  MathF.Sqrt(Bound.SqrDistance(new Vector3(ViewerPosition.x, 0, ViewerPosition.y)));
                    bool visible = viewerDistanceNearestEdge <= MaxViewDistance;

                    if (visible)
                    {
                        int lodIndex = 0;
                        for (int i = 0; i < Detailevels.Length - 1; i++)
                        {
                            if (viewerDistanceNearestEdge > Detailevels[i].VisibleDistanceThreshold)
                            {
                                lodIndex = i + 1;
                            }
                            else
                            {
                                break;
                            }
                        }


                        if (previousLODIndex != lodIndex)
                        {
                            LODMesh lodMesh = LODMeshes[lodIndex];
                            if (lodMesh.HasMesh)
                            {
                                previousLODIndex = lodIndex;
                                RenderComponenet.RenderDatas[0] = lodMesh.Data;
                                Texture2DHelper.ConvertFromBitmap
                                    ((RenderComponenet.RenderDatas[0].Shader as NormalShader).MainTexture, tex);
                            }
                            else if (lodMesh.HasRequestedMesh == false)
                            {
                                lodMesh.RequestMesh(MapData);
                            }
                        }

                        TerrainChuncksVisibleLastUpdate.Add(this);
                    }

                    SetVisible(visible);
                }
            }

            public void SetVisible(bool visible)
            {
                MeshObject.Active = (visible);
            }

            public bool IsVisible()
            {
                return MeshObject.Active;
            }
        }

        class LODMesh
        {
            public RenderData Data;
            public bool HasRequestedMesh;
            public bool HasMesh;
            int lod;

            System.Action UpdateCallback;

            public LODMesh(int lod, System.Action updateCallback)
            {
                this.lod = lod;
                UpdateCallback = updateCallback;
            }

            void OnMeshDataReceived(TerrainMeshData data)
            {
                Data = data.CreateMesh();
                HasMesh = true;

                UpdateCallback();
            }

            public void RequestMesh(MapData data)
            {
                HasRequestedMesh = true;
                MapGeneratorReference.RequestMeshData(data, lod, OnMeshDataReceived);
            }
        }

        [System.Serializable]
        public struct LODInfo
        {
            public int LOD;
            public float VisibleDistanceThreshold;

            public LODInfo(int lod, float  visibleDistanceThreshold)
            {
                this.LOD = lod;
                VisibleDistanceThreshold = visibleDistanceThreshold;
            }
        }
    }

}
