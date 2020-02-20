using System;
using System.Collections.Generic;
using System.IO;
using TerrariaUI.Widgets.Data;

namespace TerrariaUI.Widgets.Media
{
    public class ImageData
    {
        #region Data

        public static Dictionary<string, Action<string, ImageData>> Readers =
            new Dictionary<string, Action<string, ImageData>>();

        public int Width, Height;
        public dynamic Tiles;
        public List<SignData> Signs = new List<SignData>();

        #endregion

        #region Constructor

        public ImageData(string path)
        {
            if (Readers.TryGetValue(Path.GetExtension(path), out Action<string, ImageData> reader))
                reader.Invoke(path, this);
        }

        #endregion
        #region Copy

        public ImageData(ImageData imageData)
        {
            throw new NotImplementedException("Cloning images not supported yet.");
        }

        #endregion
        #region Load

        public static ImageData[] Load(string path)
        {
            List<ImageData> images = new List<ImageData>();
            if (Path.HasExtension(path) && File.Exists(path))
            {
                ImageData image = new ImageData(path);
                if (image.Tiles != null)
                    images.Add(image);
            }
            else if (Directory.Exists(path))
                foreach (string f in Directory.EnumerateFiles(path))
                {
                    ImageData image = new ImageData(f);
                    if (image.Tiles != null)
                        images.Add(image);
                }
            else
                throw new FileNotFoundException("Invalid TUI Image file or folder: " + path);
            return images.ToArray();
        }

        #endregion
    }
}
