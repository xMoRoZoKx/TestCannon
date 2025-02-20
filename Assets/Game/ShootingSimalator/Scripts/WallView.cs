using UnityEngine;

public class WallView : MonoBehaviour
{
    private const int TargetResolution = 4096;
    private const int ImpactTextureSize = 256;

    [SerializeField] private Renderer wallRenderer;
    private Texture2D wallTexture;

    private void Start()
    {
        wallTexture = new Texture2D(TargetResolution, TargetResolution, TextureFormat.RGBA32, false);

        wallRenderer.material = new Material(wallRenderer.material);
        wallRenderer.material.mainTexture = wallTexture;
    }
    public void ApplyImpactTexture(Texture2D impactTexture, RaycastHit hit)
    {
        if (wallTexture == null || impactTexture == null) return;

       impactTexture = ResizeTexture(impactTexture, ImpactTextureSize, ImpactTextureSize);

        Vector2 uv = hit.textureCoord;
        int x = Mathf.FloorToInt(uv.x * wallTexture.width);
        int y = Mathf.FloorToInt(uv.y * wallTexture.height);

        int centredX = Mathf.Clamp(x - impactTexture.width / 2, 0, wallTexture.width - 1);
        int centredY = Mathf.Clamp(y - impactTexture.height / 2, 0, wallTexture.height - 1);

        int width = Mathf.Min(impactTexture.width, wallTexture.width - centredX);
        int height = Mathf.Min(impactTexture.height, wallTexture.height - centredY);

        if (width <= 0 || height <= 0)
        {
            Debug.LogError("Impact texture coordinates are out of bounds!");
            return;
        }

        Color[] basePixels = wallTexture.GetPixels(centredX, centredY, width, height);
        Color[] impactPixels = impactTexture.GetPixels(0, 0, width, height);

        for (int i = 0; i < basePixels.Length; i++)
        {
            Color baseColor = basePixels[i];
            Color impactColor = impactPixels[i];

            float alpha = impactColor.a;
            basePixels[i] = Color.Lerp(baseColor, impactColor, alpha);
        }

        wallTexture.SetPixels(centredX, centredY, width, height, basePixels);
        wallTexture.Apply();
    }
    private Texture2D ResizeTexture(Texture2D source, int width, int height)
    {
        RenderTexture rt = new RenderTexture(width, height, 0);
        RenderTexture.active = rt;

        if (source != null) Graphics.Blit(source, rt);

        Texture2D result = new Texture2D(width, height, TextureFormat.RGBA32, false);
        result.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        result.Apply();

        RenderTexture.active = null;
        rt.Release();

        return result;
    }
}
