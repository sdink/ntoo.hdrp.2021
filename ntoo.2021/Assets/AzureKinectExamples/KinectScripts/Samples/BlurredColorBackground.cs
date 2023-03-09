using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using com.rfilkov.kinect;

namespace com.rfilkov.components
{
    /// <summary>
    /// Blurs the color camera image and applies it to the background. Thanks to Edgaras Artemciukas.
    /// </summary>
    public class BlurredColorBackground : MonoBehaviour
    {
        [Tooltip("Depth sensor index - 0 is the 1st one, 1 - the 2nd one, etc.")]
        public int sensorIndex = 0;

        [Tooltip("RawImage to display the blurred background")]
        public RawImage backgroundImage;

        [Tooltip("Blur pixel offset")]
        [Range(0, 20)]
        public int pixelOffset = 8;

        [Tooltip("Blur pixel step")]
        [Range(1, 5)]
        public int pixelStep = 2;


        // reference to the KinectManager
        private KinectManager kinectManager;
        private KinectInterop.SensorData sensorData = null;

        // blur background material & texture
        private Material blurBackMat;
        private RenderTexture blurredTexture = null;

        // last color frame time
        private ulong lastColorFrameTime = 0;


        void Start()
        {
            kinectManager = KinectManager.Instance;
            sensorData = kinectManager != null ? kinectManager.GetSensorData(sensorIndex) : null;

            // blur material
            Shader blurShader = Shader.Find("Kinect/BlurShader");
            blurBackMat = new Material(blurShader);
        }

        void OnDestroy()
        {
            // release the texture
            blurredTexture.Release();
            Destroy(blurredTexture);

            blurredTexture = null;
            blurBackMat = null;
        }

        void Update()
        {
            if(kinectManager && kinectManager.IsInitialized() && sensorData != null && lastColorFrameTime != sensorData.lastColorFrameTime)
            {
                lastColorFrameTime = sensorData.lastColorFrameTime;

                // create the blurred texture, if needed
                if(blurredTexture == null || blurredTexture.width != sensorData.colorImageWidth || blurredTexture.height != sensorData.colorImageHeight)
                {
                    if(blurredTexture != null)
                    {
                        blurredTexture.Release();
                        Destroy(blurredTexture);
                    }

                    blurredTexture = new RenderTexture(sensorData.colorImageWidth, sensorData.colorImageHeight, 0, RenderTextureFormat.ARGB32);

                    if(backgroundImage)
                    {
                        backgroundImage.texture = blurredTexture;
                        backgroundImage.rectTransform.localScale = sensorData.colorImageScale;
                        backgroundImage.color = Color.white;
                    }
                }

                // render the blurred texture
                blurBackMat.SetFloat("_PixOffset", pixelOffset);
                blurBackMat.SetFloat("_PixStep", pixelStep);
                Graphics.Blit(kinectManager.GetColorImageTex(sensorIndex), blurredTexture, blurBackMat);
            }
        }

    }
}
