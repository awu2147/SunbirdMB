using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Globalization;
using SunbirdMB.Interfaces;

namespace SunbirdMB.Core
{
    public static class GraphicsHelper
    {

        public static RenderTarget2D NewRenderTarget2D(GraphicsDevice graphicsDevice)
        {
            return new RenderTarget2D(
            graphicsDevice,
            graphicsDevice.PresentationParameters.BackBufferWidth,
            graphicsDevice.PresentationParameters.BackBufferHeight,
            true,
            graphicsDevice.PresentationParameters.BackBufferFormat,
            DepthFormat.None);
        }

        /// <summary>
        /// <para>Takes an original color array and returns a new array which only includes data points contained inside the destination rectangle.</para>
        /// <para>The returned array will be ordered from top-left to bottom-right, like the original. The given the rectangle must lie within the original area.</para>
        /// </summary>
        /// <param name="colorData"> The original color data. </param>
        /// <param name="width"> The original width. </param>
        /// <param name="rectangle"> The rectangular section of data to extract. </param>
        /// <returns></returns>
        private static Color[] GetImageData(Color[] colorData, int width, Rectangle rectangle)
        {
            Color[] color = new Color[rectangle.Width * rectangle.Height];
            for (int x = 0; x < rectangle.Width; x++)
            {
                for (int y = 0; y < rectangle.Height; y++)
                {
                    color[x + y * rectangle.Width] = colorData[x + rectangle.X + (y + rectangle.Y) * width];
                }
            }
            return color;
        }

        public static HashSet<Point> SolidPixels(Animator animator)
        {
            // Get the animator spritesheet pixel data.
            var texture = animator.SpriteSheet.Texture;
            var textureTP = texture.Width * texture.Height;
            Color[] textureColorArray = new Color[textureTP];
            texture.GetData(textureColorArray);
            // Reduce to only the visible frame's pixel data.
            Color[] viewAreaColorArray = GetImageData(textureColorArray, texture.Width, animator.SheetViewArea());

            // Resulting set of pixels on which Contains() will be called on.
            var solidPixels = new HashSet<Point>();

            // Analyse all original pixels.
            for (int i = 0; i < viewAreaColorArray.Length; i++)
            {
                if (viewAreaColorArray[i].A != 0)
                {
                    // Add valid pixels.
                    solidPixels.Add(new Point(i % animator.SpriteSheet.FrameWidth, i / animator.SpriteSheet.FrameWidth));
                }
            }
            return solidPixels;
        }


        /// <summary>
        /// Returns a mask texture from a base texture. This creates garbage.
        /// </summary>
        public static Texture2D GetMask(IMainGame mainGame, Texture2D texture, Color color)
        {
            var totalPixels = texture.Width * texture.Height;
            Color[] maskPixels = new Color[totalPixels];
            texture.GetData(maskPixels);

            for (int i = 0; i < maskPixels.Length; i++)
            {
                if (maskPixels[i].A != 0)
                {
                    maskPixels[i] = color;
                }
            }

            var mask = new Texture2D(mainGame.GraphicsDevice, texture.Width, texture.Height);
            mask.SetData(maskPixels);
            return mask;
        }

        public static Texture2D GetAntiShadow(IMainGame mainGame, Texture2D texture)
        {
            return GetMask(mainGame, texture, Color.Black);
        }

        public static Dictionary<string, Texture2D> AntiShadowLibrary = new Dictionary<string, Texture2D>() { };

        public static Texture2D GetSelfShadow(IMainGame mainGame, Texture2D texture)
        {
            return GetMask(mainGame, texture, new Color(109, 117, 141));
        }

        public static Dictionary<string, Texture2D> SelfShadowLibrary = new Dictionary<string, Texture2D>() { };

        public static Color HexColor(string hexCode)
        {
            int argb = Int32.Parse(hexCode.Replace("#", ""), NumberStyles.HexNumber);
            return new Color((uint)argb);
        }

        #region Obsolete

        [Obsolete("Use the LightingStencil shader effect instead.")]
        public static void ApplyStencil(Texture2D source, Texture2D stencil, Color color)
        {
            var totalPixels = source.Width * source.Height;
            Color[] stencilPixels = new Color[totalPixels];
            stencil.GetData(stencilPixels);
            Color[] sourcePixels = new Color[totalPixels];
            source.GetData(sourcePixels);

#if DEBUG
            Debug.Assert(stencilPixels.Length == sourcePixels.Length);
#endif

            for (int i = 0; i < stencilPixels.Length; i++)
            {
                if (stencilPixels[i] != Color.Black)
                {
                    sourcePixels[i] = color;
                }
            }

            source.SetData(sourcePixels);
        }

        #endregion

    }
}

