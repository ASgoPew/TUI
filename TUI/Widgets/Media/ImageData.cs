using System;
using System.Collections.Generic;
using System.IO;

namespace TUI.Widgets.Media
{
    public class ImageData
    {
        #region Data

        public List<SignData> Signs = new List<SignData>();

        public dynamic Tiles;
        public int Width, Height;

        public static Dictionary<string, Action<BinaryReader, ImageData>> Readers =
            new Dictionary<string, Action<BinaryReader, ImageData>>();

        #endregion

        #region Load

        public static ImageData[] Load(string path)
        {
            List<ImageData> images = new List<ImageData>();
            if (Path.HasExtension(path))
            {
                ImageData image = LoadImage(path);
                if (image != null)
                    images.Add(image);
            }
            else
                foreach (string f in Directory.EnumerateFiles(path))
                {
                    ImageData image = LoadImage(f);
                    if (image != null)
                        images.Add(image);
                }
            return images.ToArray();
        }

        #endregion
        #region LoadImage

        private static ImageData LoadImage(string path)
        {
            if (!Readers.TryGetValue(Path.GetExtension(path),
                    out Action<BinaryReader, ImageData> reader))
                return null;
            
            ImageData image = new ImageData();
            using (FileStream fs = File.OpenRead(path))
            using (BinaryReader br = new BinaryReader(fs))
                reader.Invoke(br, image);
            return image;
        }

        #endregion
    }
}
