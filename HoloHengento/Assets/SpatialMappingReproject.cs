using UnityEngine;
using HoloToolkit.Unity.SpatialMapping;
using HoloToolkit.Unity;

public class SpatialMappingReproject : MonoBehaviour
{
    public GameObject SpatialMapping;
    public Camera HoloLensCamera;
    public Material ReprojectionMaterial;
    WebCamTexture webCamTexture;

    private void Awake()
    {
        var spatialMappingSources = SpatialMapping.GetComponents<SpatialMappingSource>();
        foreach (var source in spatialMappingSources)
        {
            source.SurfaceAdded += SpatialMappingSource_SurfaceAdded;
            source.SurfaceUpdated += SpatialMappingSource_SurfaceUpdated;
        }
    }

    void Start()
    {
        var devices = WebCamTexture.devices;
        if (devices.Length > 0)
        {
            webCamTexture = new WebCamTexture(devices[0].name);
            ReprojectionMaterial.mainTexture = webCamTexture;
            webCamTexture.Play();
            SpatialMappingManager.Instance.SetSurfaceMaterial(ReprojectionMaterial);
        }
        else
        {
            Debug.Log("Webカメラが検出できませんでした");
            return;
        }
    }

    private void SpatialMappingSource_SurfaceAdded(object sender, DataEventArgs<SpatialMappingSource.SurfaceObject> e)
    {
        InitSurface(e.Data.Object);
    }

    private void SpatialMappingSource_SurfaceUpdated(object sender, DataEventArgs<SpatialMappingSource.SurfaceUpdate> e)
    {
        InitSurface(e.Data.New.Object);
    }

    void InitSurface(GameObject surfaceObject)
    {
        var meshReproject = surfaceObject.GetComponent<MeshReproject>();
        if (meshReproject == null)
        {
            meshReproject = surfaceObject.AddComponent<MeshReproject>();
            meshReproject.ProjectionPerspective = HoloLensCamera;
        }
    }
}