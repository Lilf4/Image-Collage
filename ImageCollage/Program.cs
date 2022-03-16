using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using Newtonsoft.Json;

namespace ImageCollage
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Input path to link collection");
            string LinkTextFile = Console.ReadLine();
            LinkTextFile = LinkTextFile.Replace("\"", "");
            List<string> IMGURLS = new List<string>();

            using (StreamReader SR = new StreamReader(LinkTextFile))
            {
                while (!SR.EndOfStream)
                {
                    string link = SR.ReadLine();
                    if (link.EndsWith(".jpg") || link.EndsWith(".jpeg") || link.EndsWith(".png"))
                    {
                        IMGURLS.Add(link);
                    }
                }
            }

            Console.WriteLine("Total Images: " + IMGURLS.Count);

            List<ImageInfo> imgInfo = new List<ImageInfo>();

            foreach (string link in IMGURLS)
            {
                byte[] imgData = new WebClient().DownloadData(link);
                MemoryStream imgStream = new MemoryStream(imgData);
                Image img = Image.FromStream(imgStream);
                imgStream.Dispose();

                imgInfo.Add(new ImageInfo()
                {
                    ID = imgInfo.Count,
                    URL = link,
                    Height = img.Height,
                    Width = img.Width
                });
                img.Dispose();

                Console.WriteLine("Images done: " + imgInfo.Count + "/" + IMGURLS.Count);
                Console.SetCursorPosition(0, Console.CursorTop - 1);
            }

            Console.SetCursorPosition(0, Console.CursorTop + 2);
            Console.WriteLine("Done Converting Images");

            Console.Write("Input SaveName: ");
            string SaveFileName = Console.ReadLine();
            File.WriteAllText(SaveFileName + ".json", JsonConvert.SerializeObject(imgInfo));

        }
    }
}
