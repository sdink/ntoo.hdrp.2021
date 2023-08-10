using com.rfilkov.kinect;
using UnityEngine;

public class BodyImageViewer : MonoBehaviour
{
  // the KinectManager instance
  private KinectManager kinectManager;

  [Tooltip("Single image width, as percent of the screen width. The height is estimated according to the image's aspect ratio.")]
  [Range(0.1f, 0.5f)]
  public float displayImageWidthPercent = 0.2f;

  void Start()
  {
    kinectManager = KinectManager.Instance;
  }

  void OnGUI()
  {
    if (!kinectManager || !kinectManager.IsInitialized())
      return;

    var imageTex = kinectManager.GetUsersImageTex();
    var imageScale = kinectManager.GetDepthImageScale(0);

    // display the image on screen
    if (imageTex != null)
    {
        KinectInterop.DisplayGuiTexture(0, displayImageWidthPercent, imageScale, imageTex);
    }
  }
}
