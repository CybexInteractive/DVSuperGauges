using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Cybex.DVSuperGauges
{
	public static class Texture2DExtensions
	{
		public static Texture2D DTXnm2RGBA (this Texture2D texture2D)
		{
			// copied from skin manager mod
			var colors = texture2D.GetPixels();
			for (int i = 0; i < colors.Length; i++)
			{
				Color c = colors[i];
				c.r = c.a * 2 - 1;  //red<-alpha (x<-w)
				c.g = c.g * 2 - 1; //green is always the same (y)
				Vector2 xy = new Vector2(c.r, c.g); //this is the xy vector
				c.b = Mathf.Sqrt(1 - Mathf.Clamp01(Vector2.Dot(xy, xy))); //recalculate the blue channel (z)
				colors[i] = new Color(c.r * 0.5f + 0.5f, c.g * 0.5f + 0.5f, c.b * 0.5f + 0.5f); //back to 0-1 range
			}
			texture2D.SetPixels(colors);
			texture2D.Apply();
			return texture2D;
		}
	}
}
