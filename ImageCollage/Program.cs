using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;

namespace ImageCollage
{
    class Program
    {
        static void Main(string[] args)
        {
            string LinkTextFile = "";
            string SaveFileName = "";
            float Percent = 0;


            Console.WriteLine("Input path to link collection");
            LinkTextFile = Console.ReadLine();

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


                imgInfo.Add(new ImageInfo()
                {
                    ID = imgInfo.Count,
                    Height = img.Height,
                    Width = img.Width,
                    Image = Image.FromStream(imgStream)
                });
                imgStream.Dispose();
                img.Dispose();

                Console.WriteLine("Images done: " + imgInfo.Count + "/" + IMGURLS.Count);
                Console.SetCursorPosition(0, Console.CursorTop - 1);
            }
            IMGURLS.Clear();
            Console.WriteLine("Done Getting Images");
            Console.Write("\nChoose percent of largest image width (1-100): ");
            Percent = float.Parse(Console.ReadLine()) / 100;

            //Naive/Simple Decreasing height packing
            //determine max width of final image
            int MaxWidth = (int)(GetRowWidth(imgInfo) * Percent);

            List<ImageInfo> HeightSortedImages = new List<ImageInfo>();
            while (imgInfo.Count > 0)
            {
                int ImageIndex = 0;
                for (int i = 0; i < imgInfo.Count; i++)
                {
                    if (imgInfo[i].Height > imgInfo[ImageIndex].Height) { imgInfo[ImageIndex] = imgInfo[i]; ImageIndex = i; }
                }
                HeightSortedImages.Add(imgInfo[ImageIndex]);
                imgInfo.RemoveAt(ImageIndex);
            }

            List<List<ImageInfo>> PackedImages = new List<List<ImageInfo>>();
            //Add first level
            PackedImages.Add(new List<ImageInfo>());
            while (HeightSortedImages.Count > 0)
            {
                bool ImagePacked = false;
                for (int i = 0; i < PackedImages.Count; i++)
                {
                    if (GetRowWidth(PackedImages[i]) + HeightSortedImages[0].Width <= MaxWidth)
                    {
                        PackedImages[i].Add(HeightSortedImages[0]);
                        HeightSortedImages.RemoveAt(0);
                        ImagePacked = true;
                        break;
                    }
                }
                if (!ImagePacked)
                {
                    PackedImages.Add(new List<ImageInfo>());
                    PackedImages[PackedImages.Count - 1].Add(HeightSortedImages[0]);
                    HeightSortedImages.RemoveAt(0);
                }
            }

            int FinImgHeight = 0;
            int FinImgWidth = 0;

            //Go thru images and set their correspondent positions
            for (int i = 0; i < PackedImages.Count; i++)
            {
                for (int j = 0; j < PackedImages[i].Count; j++)
                {
                    //Set image X Pos
                    if (j - 1 >= 0)
                    {
                        PackedImages[i][j].PosX = PackedImages[i][j - 1].PosX + PackedImages[i][j - 1].Width;
                    }
                    //Set image Y Pos
                    if (i - 1 >= 0)
                    {
                        PackedImages[i][j].PosY = PackedImages[i - 1][0].PosY + PackedImages[i - 1][0].Height;
                    }
                }

                //Get the widest row and update the final image's width
                int RowWidth = GetRowWidth(PackedImages[i]);
                if (RowWidth > FinImgWidth) { FinImgWidth = RowWidth; }

                //Get the final image Height
                FinImgHeight += PackedImages[i][0].Height;
            }

            //Construct the final image
            var finalImg = new Bitmap(FinImgWidth, FinImgHeight);
            using (var g = Graphics.FromImage(finalImg))
            {
                for (int i = 0; i < PackedImages.Count; i++)
                {
                    foreach (var ImgInfo in PackedImages[i])
                    {
                        g.DrawImage(ImgInfo.Image, ImgInfo.PosX, ImgInfo.PosY);
                        ImgInfo.Image.Dispose();
                    }
                }
            }

            Console.Write("\nInput SaveName: ");
            SaveFileName = Console.ReadLine();
            SaveFileName = SaveFileName.Replace("\"", "");

            finalImg.Save(SaveFileName + ".png", System.Drawing.Imaging.ImageFormat.Png);
            finalImg.Dispose();
        }

        static int GetRowWidth(List<ImageInfo> Row)
        {
            int val = 0;
            foreach (ImageInfo image in Row)
            {
                val += image.Width;
            }
            return val;
        }

        //Not needed for simple algorithm
        /*
        static bool doOverlap(ImageInfo image1, ImageInfo image2)
        {
            int[] Image1TopLeft = new int[2] { image1.PosY + image1.Height, image1.PosX };
            int[] Image1BotRight = new int[2] { image1.PosY, image1.PosX + image1.Width };
            int[] Image2TopLeft = new int[2] { image2.PosY + image2.Height, image2.PosX };
            int[] Image2BotRight = new int[2] { image2.PosY, image2.PosX + image2.Width };

            return !(Image1TopLeft[0] <= Image2BotRight[0] || Image2TopLeft[0] <= Image1BotRight[0]) && !(Image1TopLeft[1] <= Image2BotRight[1] || Image2TopLeft[1] <= Image1BotRight[1]);
        }*/

    }
}
