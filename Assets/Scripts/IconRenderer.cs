using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IconRenderer : MonoBehaviour
{
  public new Camera camera;
  public new Light light;
  private class Job
  {
    public PieceIconSet.PieceIconEntry entry;
    public Color color;
  }
  private Queue<Job> jobs = new Queue<Job>();
  private void Start()
  {
    StartCoroutine(ProcessJobs());
  }

  public void RenderIcons(PieceIconSet.PieceIconEntry[] pieceIconEntries, Color color)
  {
    lock (jobs)
    {
      for (int i = 0; i < pieceIconEntries.Length; i++)
      {
        var renderTexture = camera.targetTexture;
        var iconEntry = pieceIconEntries[i];
        iconEntry.texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);
        Job job = new Job { entry = iconEntry, color = color };
        jobs.Enqueue(job);
      }
    }
  }

  private IEnumerator ProcessJobs()
  {
    while (true)
    {
      if (jobs.Count > 0)
      {
        Job job = null;
        lock (jobs)
        {
          job = jobs.Dequeue();
        }

        if (job != null)
        {
          RenderIcon(job.entry, job.color, camera, light);
        }
      }
      yield return null;
    }
  }

  private void Update()
  {

  }

  public static bool RenderIcon(PieceIconSet.PieceIconEntry entry, Color color, Camera camera, Light light)
  {
    int culledLayer = LayerMask.NameToLayer("Icon Rendering");
    int cullingMask = 1 << culledLayer;
    camera.cullingMask = cullingMask;

    GameObject gameObjectPrefab = Resources.Load<GameObject>(entry.modelName);
    if (gameObjectPrefab == null)
    {
      Debug.LogWarning("Missing game object, using white texture");
      entry.texture = Texture2D.whiteTexture;
      return false;
    }

    GameObject gameObject = Instantiate<GameObject>(gameObjectPrefab);
    foreach (Transform t in gameObject.GetComponentsInChildren<Transform>())
    {
      t.gameObject.layer = culledLayer;
    }
    gameObject.transform.position = Vector3.zero;
    gameObject.transform.rotation = Quaternion.identity;

    MeshRenderer renderer = gameObject.GetComponentInChildren<MeshRenderer>();
    renderer.material.color = color;

    camera.transform.position = entry.cameraPosition;
    camera.transform.LookAt(renderer.bounds.center);

    light.transform.rotation = Quaternion.AngleAxis(0.0f, entry.lightDirection.normalized);
    light.color = entry.lightColor;

    var currentRenderTexture = RenderTexture.active;
    var renderTexture = camera.targetTexture;
    RenderTexture.active = renderTexture;

    camera.Render();
    
    entry.texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
    entry.texture.Apply();

    RenderTexture.active = currentRenderTexture;

    DestroyImmediate(gameObject);
    return true;
  }
}
