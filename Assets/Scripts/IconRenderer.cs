using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Asynchronously renders icons
/// </summary>
public class IconRenderer : MonoBehaviour
{
  /// <summary> The Camera used for rendering </summary>
  public new Camera camera;
  /// <summary> The Light used for illumination </summary>
  public new Light light;
  /// <summary>
  /// The class used to encapsulate a icon render job
  /// </summary>
  private class Job
  {
  /// <summary> The specification for the model, camera and lighting </summary>
    public PieceIconSet.PieceIconEntry entry;
  /// <summary> The color for the model </summary>
    public Color color;
  }
  /// <summary> The Queue for the icon render jobs </summary>
  private Queue<Job> jobs = new Queue<Job>();

  /// <summary>
  /// Called by Unity, starts the coroutine that handles icon render job
  /// processing
  /// </summary>
  private void Start()
  {
    StartCoroutine(ProcessJobs());
  }

  /// <summary>
  /// Enqueues a new job comprised of the passed in entries and color
  /// </summary>
  /// <param name="pieceIconEntries">A list of icons being requested</param>
  /// <param name="color">The common color that each icon in <paramref name="pieceIconEntries"/> will share</param>
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

  /// <summary>
  /// A coroutine that is run asynchronously to the main update events (though
  /// on the same thread). Manages how many icons are rendered per update.
  /// </summary>
  /// <returns>
  /// A way to signal the calling environment how long to wait to resume the
  /// coroutine or if the coroutine is finished.
  /// </returns>
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

  /// <summary>
  /// Renders one icon using the specifications in <paramref name="entry"/> and
  /// <paramref name="color"/>. Declared as public static so that the functionality
  /// is exposed to the rest of the code without going through the <see cref="jobs"/>
  /// </summary>
  /// <param name="entry">The specifications for the icon</param>
  /// <param name="color">The color to be used by the model</param>
  /// <param name="camera">The camera to be used for rendering</param>
  /// <param name="light">The light to be used for lighting</param>
  /// <returns>true, if the model was loaded and the rendering was successful. False, otherwise.</returns>
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
