using UnityEngine;
using Steamworks.Data;

[RequireComponent(typeof(UnityEngine.UI.RawImage))]
public class SteamImage : MonoBehaviour
{
	public void LoadTextureFromImage(Image img)
	{
		var texture = new Texture2D((int)img.Width, (int)img.Height);

		for (int x = 0; x < img.Width; x++)
			for (int y = 0; y < img.Height; y++)
			{
				var p = img.GetPixel(x, y);

				texture.SetPixel(x, (int)img.Height - y, new Color32(p.r, p.g, p.b, p.a));
			}

		texture.Apply();

		ApplyTexture(texture);
	}

	public virtual void ApplyTexture(Texture2D texture)
	{
		var rawImage = GetComponent<UnityEngine.UI.RawImage>();
		if (rawImage != null)
		{
			rawImage.texture = texture;
		}
	}
}
