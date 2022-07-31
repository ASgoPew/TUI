using System;
using System.Collections.Generic;
using System.IO;
using TerrariaUI.Widgets.Data;

namespace TerrariaUI.Widgets.Media
{
    public class ImageData
    {
        #region Data

        public static Dictionary<string, Func<string, bool, List<ImageData>>> Readers =
            new Dictionary<string, Func<string, bool, List<ImageData>>>();

        public int Width, Height;
        public dynamic Tiles;
        public List<SignData> Signs = new List<SignData>();

        #endregion

        #region Constructor

        public ImageData()
        {
        }

        #endregion
        #region Copy

        public ImageData(ImageData imageData)
        {
            throw new NotImplementedException("Cloning images not supported yet.");
        }

        #endregion
        #region LoadImage

        public static ImageData LoadImage(string name)
        {
            if (Readers.TryGetValue(Path.GetExtension(name), out Func<string, bool, List<ImageData>> reader))
            {
                try
                {
                    var image = reader.Invoke(name, false);
                    if (image?.Count > 0)
                        return image[0];
                } catch (Exception e)
                {
                    TUI.HandleException(e);
                }
            }
            return null;
        }

        #endregion
        #region LoadVideo

        public static List<ImageData> LoadVideo(string name)
        {
            List<ImageData> video = new List<ImageData>();
            if (Directory.Exists(name))
            {
                foreach (string file in Directory.EnumerateFiles(name))
                    if (Readers.TryGetValue(Path.GetExtension(file), out Func<string, bool, List<ImageData>> reader))
                    {
                        try
                        {
                            var image = reader.Invoke(file, false);
                            if (image.Count > 0)
                                video.Add(image[0]);
                        }
                        catch (Exception e)
                        {
                            TUI.HandleException(e);
                        }
                    }
            }
            else if (Readers.TryGetValue(Path.GetExtension(name), out Func<string, bool, List<ImageData>> reader))
                try
                {
                    video = reader.Invoke(name, true);
                }
                catch (Exception e)
                {
                    TUI.HandleException(e);
                }

            if (video.Count > 0)
                return video;
            return null;
        }

        #endregion
    }
}
