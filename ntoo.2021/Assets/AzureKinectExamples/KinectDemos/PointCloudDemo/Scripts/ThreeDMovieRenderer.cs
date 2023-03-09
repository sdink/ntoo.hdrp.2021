using UnityEngine;
using System.Collections;
using com.rfilkov.kinect;


namespace com.rfilkov.components
{
    /// <summary>
    /// ThreeDMovieRenderer renders the environment in 3D, as detected by the given sensor. 
    /// </summary>
    public class ThreeDMovieRenderer : MonoBehaviour
    {
        [Tooltip("Depth sensor index - 0 is the 1st one, 1 - the 2nd one, etc.")]
        public int sensorIndex = 0;

        [Range(0f, 10f)]
        [Tooltip("Minimum distance, in meters.")]
        public float minDepth = 1f;

        [Range(0f, 10f)]
        [Tooltip("Maximum distance, in meters.")]
        public float maxDepth = 4f;

        [Tooltip("Time interval between scene mesh updates, in seconds. 0 means no wait.")]
        private float updateMeshInterval = 0f;


        // reference to object's mesh
        private Mesh mesh = null;

        // references to KM and data
        private KinectManager kinectManager = null;
        private KinectInterop.SensorData sensorData = null;
        private DepthSensorBase sensorInt = null;

        // transformed depth image
        //private ushort[] tDepthImage = null;
        private ComputeBuffer depthImageBuffer = null;
        private Vector4 imageSize = Vector4.zero;

        // times
        private ulong lastDepthFrameTime = 0;
        private float lastMeshUpdateTime = 0f;

        // image parameters
        private int imageWidth = 0;
        private int imageHeight = 0;

        // depth scale factor
        private float depthScale = 1f;

        // mesh parameters
        private Vector3[] meshVertices = null;
        private int[] meshIndices = null;
        private Vector2[] meshUvs = null;

        private bool bMeshInited = false;

        // mesh-renderer material
        private Material meshRenderMat = null;


        void Start()
        {
            // get sensor data
            kinectManager = KinectManager.Instance;
            sensorData = (kinectManager != null && kinectManager.IsInitialized()) ? kinectManager.GetSensorData(sensorIndex) : null;
        }


        void OnDestroy()
        {
            if (bMeshInited)
            {
                // release the mesh-related resources
                DestroyMesh();
            }
        }


        void LateUpdate()
        {
            if(kinectManager.IsInitialized() && sensorData != null && sensorData.sensorInterface != null)
            {
                if(!bMeshInited || imageWidth != sensorData.colorImageWidth || imageHeight != sensorData.colorImageHeight)
                {
                    // init mesh and its related data
                    InitMesh();
                }
            }

            if (bMeshInited)
            {
                // update the mesh
                UpdateMesh();
            }
        }


        // inits the mesh and related data
        private void InitMesh()
        {
            if(bMeshInited)
            {
                DestroyMesh();
            }

            // create mesh
            mesh = new Mesh
            {
                name = "MovieMesh-Sensor" + sensorIndex,
                indexFormat = UnityEngine.Rendering.IndexFormat.UInt32
            };

            MeshFilter meshFilter = GetComponent<MeshFilter>();
            if(meshFilter != null)
            {
                meshFilter.sharedMesh = mesh;
            }
            else
            {
                Debug.LogWarning("MeshFilter not found! You may not see the mesh on screen");
            }

            // get the mesh material
            Renderer meshRenderer = GetComponent<Renderer>();
            if (meshRenderer)
            {
                Shader meshShader = Shader.Find("Kinect/ThreeDMovieShader");
                if (meshShader != null)
                {
                    meshRenderMat = new Material(meshShader);
                    meshRenderMat.name = "ThreeDMovieMaterial";
                    meshRenderer.sharedMaterial = meshRenderMat;
                }
            }

            if (sensorData != null && sensorData.sensorInterface != null)
            {
                sensorInt = (DepthSensorBase)sensorData.sensorInterface;

                // enable transformed depth frame
                sensorData.sensorInterface.EnableColorCameraDepthFrame(sensorData, true);

                //tDepthImage = new ushort[sensorData.colorImageWidth * sensorData.colorImageHeight];
                int depthBufferLength = sensorData.colorImageWidth * sensorData.colorImageHeight >> 1;

                if (depthImageBuffer == null || depthImageBuffer.count != depthBufferLength)
                {
                    depthImageBuffer = KinectInterop.CreateComputeBuffer(depthImageBuffer, depthBufferLength, sizeof(uint));
                }

                // image width & height
                imageWidth = sensorData.colorImageWidth;
                imageHeight = sensorData.colorImageHeight;
                int pointCount = imageWidth * imageHeight;

                // mesh arrays
                meshVertices = new Vector3[pointCount];
                meshIndices = new int[(imageWidth - 1) * (imageHeight - 1) * 4];  // Quads
                meshUvs = new Vector2[pointCount];

                Vector3 topLeft = kinectManager.MapColorPointToSpaceCoords(sensorIndex, Vector2.zero, (ushort)((maxDepth - minDepth) * 500f));
                Vector3 botRight = kinectManager.MapColorPointToSpaceCoords(sensorIndex, new Vector2(imageWidth - 1, imageHeight - 1), (ushort)((maxDepth - minDepth) * 500f));
                //Debug.Log("wMeters: " + (botRight.x - topLeft.x) + ", hMeters: " + (botRight.y - topLeft.y));

                // init mesh arrays
                imageSize = new Vector4(imageWidth, imageHeight, 0f, 0f);
                Vector2 meshMult = new Vector2(Mathf.Abs(botRight.x - topLeft.x) / imageWidth, Mathf.Abs(botRight.y - topLeft.y) / imageHeight);
                Vector2 meshCnt = new Vector2(imageWidth * 0.5f, imageHeight * 0.5f);

                depthScale = (meshMult.x + meshMult.y) * 500f;  // 0.5f * 1000f;
                //Debug.Log("MeshMult x: " + meshMult.x + ", y: " + meshMult.y + "; depthScale: " + depthScale);

                for (int y = 0, i = 0; y < imageHeight; y++)
                {
                    for (int x = 0; x < imageWidth; x++, i++)
                    {
                        meshVertices[i] = new Vector3((x - meshCnt.x) * meshMult.x, (y - meshCnt.y) * meshMult.y, maxDepth);
                        meshUvs[i] = new Vector2((float)x / imageWidth, (float)y / imageHeight);

                        if (x < (imageWidth - 1) && y < (imageHeight - 1))
                        {
                            int ii = (y * (imageWidth - 1) + x) << 2;  // Quad - 4 indices
                            meshIndices[ii] = i;
                            meshIndices[ii + 1] = i + 1;
                            meshIndices[ii + 2] = i + 1 + imageWidth;
                            meshIndices[ii + 3] = i + imageWidth;
                        }
                    }
                }

                // set mesh arrays
                mesh.Clear();
                mesh.vertices = meshVertices;
                mesh.SetIndices(meshIndices, MeshTopology.Quads, 0);
                mesh.uv = meshUvs;

                bMeshInited = true;
            }
        }


        // releases mesh-related resources
        private void DestroyMesh()
        {
            if (sensorData.sensorInterface != null)
            {
                sensorData.sensorInterface.EnableColorCameraDepthFrame(sensorData, false);
            }

            if (depthImageBuffer != null)
            {
                depthImageBuffer.Release();
                depthImageBuffer.Dispose();
                depthImageBuffer = null;
            }

            //tDepthImage = null;

            meshVertices = null;
            meshIndices = null;
            meshUvs = null;

            bMeshInited = false;
        }


        // updates the mesh according to current depth frame
        private void UpdateMesh()
        {
            if (meshRenderMat != null && lastDepthFrameTime != sensorData.lastDepthFrameTime && 
                (Time.time - lastMeshUpdateTime) >= updateMeshInterval)
            {
                lastDepthFrameTime = sensorData.lastDepthFrameTime;
                lastMeshUpdateTime = Time.time;

                if (depthImageBuffer != null && sensorData.depthImage != null /**&& depthBufferCreated*/)
                {
                    // get transformed depth frame, if needed
                    ushort[] tDepthImage = null;
                    ulong frameTime = 0;

                    tDepthImage = sensorData.sensorInterface.GetColorCameraDepthFrame(sensorData, ref tDepthImage, ref frameTime);

                    int depthBufferLength = sensorData.colorImageWidth * sensorData.colorImageHeight / 2;
                    KinectInterop.SetComputeBufferData(depthImageBuffer, tDepthImage, depthBufferLength, sizeof(uint));
                }

                meshRenderMat.SetFloat(_MinDepth, minDepth /**sensorInt.minDepthDistance*/);
                meshRenderMat.SetFloat(_MaxDepth, maxDepth /**sensorInt.maxDepthDistance*/);
                meshRenderMat.SetVector(_TexRes, imageSize);
                meshRenderMat.SetFloat(_DepthScale, depthScale);
                meshRenderMat.SetBuffer(_DepthMap, depthImageBuffer);
                meshRenderMat.SetTexture(_ColorTex, sensorData.colorImageTexture);
            }
        }

        // Shader Properties
        private static readonly int _MinDepth = Shader.PropertyToID("_MinDepth");
        private static readonly int _MaxDepth = Shader.PropertyToID("_MaxDepth");
        private static readonly int _TexRes = Shader.PropertyToID("_TexRes");
        private static readonly int _DepthScale = Shader.PropertyToID("_DepthScale");
        private static readonly int _DepthMap = Shader.PropertyToID("_DepthMap");
        private static readonly int _ColorTex = Shader.PropertyToID("_ColorTex");


    }
}
