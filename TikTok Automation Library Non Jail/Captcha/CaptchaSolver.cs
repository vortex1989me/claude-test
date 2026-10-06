using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using AForge;
using AForge.Imaging;
using AForge.Imaging.Filters;
using AForge.Math.Geometry;

namespace TikTok_Automation_Library_Non_Jail.Captcha
{
	// Token: 0x020000AE RID: 174
	internal class CaptchaSolver
	{
		// Token: 0x06000248 RID: 584 RVA: 0x00064B84 File Offset: 0x00062D84
		public static int ConvertCoordinate(int x2, int x2Min, int x2Max, int x1Min, int x1Max)
		{
			if (x2 < x2Min)
			{
				x2 = x2Min;
			}
			if (x2 > x2Max)
			{
				x2 = x2Max;
			}
			double num = (double)(x2 - x2Min) / (double)(x2Max - x2Min);
			return (int)((double)x1Min + num * (double)(x1Max - x1Min));
		}

		// Token: 0x06000249 RID: 585 RVA: 0x00064BB4 File Offset: 0x00062DB4
		public static int ConvertCoordinate_iPhone7(int x2, int x2Min = 0, int x2Max = 576)
		{
			if (x2 < x2Min)
			{
				x2 = x2Min;
			}
			if (x2 > x2Max)
			{
				x2 = x2Max;
			}
			double num = (double)(x2 - x2Min) / (double)(x2Max - x2Min);
			return (int)(43.0 + num * 230.0);
		}

		// Token: 0x0600024A RID: 586 RVA: 0x00064BF0 File Offset: 0x00062DF0
		public static float GetCoordsX(string ImagePath)
		{
			string sobelFilter = CaptchaSolver.GetSobelFilter(ImagePath);
			Bitmap bitmap = new Bitmap(sobelFilter);
			float num = CaptchaSolver.Process(bitmap, sobelFilter);
			try
			{
				bitmap.Dispose();
			}
			catch
			{
			}
			string text = ImagePath;
			if (File.Exists(text))
			{
				try
				{
					File.Delete(text);
				}
				catch
				{
				}
			}
			text = text.Replace(".png", "123.png");
			if (File.Exists(text))
			{
				try
				{
					File.Delete(text);
				}
				catch
				{
				}
			}
			text = text.Replace(".png", "_filtered_clone.png");
			if (File.Exists(text))
			{
				try
				{
					File.Delete(text);
				}
				catch
				{
				}
			}
			text = text.Replace(".png", "_detect_square.png");
			if (File.Exists(text))
			{
				try
				{
					File.Delete(text);
				}
				catch
				{
				}
			}
			return num;
		}

		// Token: 0x0600024B RID: 587 RVA: 0x00064CE0 File Offset: 0x00062EE0
		private static Image Crop(Image img, Rectangle cropArea)
		{
			Bitmap bitmap = new Bitmap(img);
			return bitmap.Clone(cropArea, bitmap.PixelFormat);
		}

		// Token: 0x0600024C RID: 588 RVA: 0x00064D04 File Offset: 0x00062F04
		public static string GetSobelFilter(string image_path)
		{
			Bitmap bitmap = new Bitmap(image_path);
			int num = 87;
			int num2 = 446;
			int num3 = 358;
			int num4 = 576;
			Image image = CaptchaSolver.Crop(bitmap, new Rectangle(new Point
			{
				X = num,
				Y = num2
			}, new Size
			{
				Height = num3,
				Width = num4
			}));
			bitmap.Dispose();
			File.Delete(image_path);
			image_path = image_path.Replace(".png", "123.png");
			image.Save(image_path);
			Bitmap bitmap2 = (Bitmap)Image.FromFile(image_path);
			bitmap2 = Grayscale.CommonAlgorithms.RMY.Apply(bitmap2);
			return CaptchaSolver.ApplyFilter(new SobelEdgeDetector(), bitmap2, image_path);
		}

		// Token: 0x0600024D RID: 589 RVA: 0x00064DBC File Offset: 0x00062FBC
		public static string ApplyFilter(IFilter filter, Bitmap originalImage, string image_path)
		{
			Bitmap bitmap = filter.Apply(originalImage);
			string text = image_path.Replace(".png", "") + "_filtered_clone.png";
			Bitmap bitmap2 = Image.Clone(bitmap, PixelFormat.Format32bppArgb);
			bitmap.Dispose();
			bitmap2.Save(text, ImageFormat.Png);
			bitmap2.Dispose();
			return text;
		}

		// Token: 0x0600024E RID: 590 RVA: 0x00064E10 File Offset: 0x00063010
		public static float Process(Bitmap image, string filepath)
		{
			bool flag = false;
			int num = 145;
			Blob blob;
			for (;;)
			{
				new List<List<IntPoint>>();
				BitmapData bitmapData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, image.PixelFormat);
				UnmanagedImage unmanagedImage = new UnmanagedImage(bitmapData);
				UnmanagedImage unmanagedImage2;
				if (image.PixelFormat == PixelFormat.Format8bppIndexed)
				{
					unmanagedImage2 = unmanagedImage;
				}
				else
				{
					unmanagedImage2 = UnmanagedImage.Create(unmanagedImage.Width, unmanagedImage.Height, PixelFormat.Format8bppIndexed);
					Grayscale.CommonAlgorithms.BT709.Apply(unmanagedImage, unmanagedImage2);
				}
				UnmanagedImage unmanagedImage3 = new DifferenceEdgeDetector().Apply(unmanagedImage2);
				UnmanagedImage unmanagedImage4 = new Threshold(num).Apply(unmanagedImage3);
				UnmanagedImage unmanagedImage5 = new ConnectedComponentsLabeling().Apply(unmanagedImage4);
				BlobCounter blobCounter = new BlobCounter();
				blobCounter.MinHeight = 88;
				blobCounter.MinWidth = 87;
				blobCounter.MaxHeight = 118;
				blobCounter.MaxWidth = 117;
				blobCounter.FilterBlobs = true;
				blobCounter.ObjectsOrder = 2;
				blobCounter.ProcessImage(unmanagedImage5);
				Blob[] array = blobCounter.GetObjectsInformation();
				array = array.Where<Blob>(new Func<Blob, bool>(CaptchaSolver.Class21.Field0.Method0)).ToArray<Blob>();
				List<IntPoint> list = null;
				SimpleShapeChecker simpleShapeChecker = new SimpleShapeChecker();
				int i = 0;
				int num2 = array.Length;
				while (i < num2)
				{
					try
					{
						List<IntPoint> blobsEdgePoints = blobCounter.GetBlobsEdgePoints(array[i]);
						list = null;
						Drawing.Polygon(unmanagedImage5, blobsEdgePoints, Color.White);
						if (simpleShapeChecker.IsQuadrilateral(blobsEdgePoints, ref list))
						{
							List<IntPoint> list2;
							List<IntPoint> list3;
							blobCounter.GetBlobsLeftAndRightEdges(array[i], ref list2, ref list3);
							blob = array[i];
						}
					}
					catch
					{
					}
					i++;
				}
				Console.WriteLine("Найдено blobsov: " + array.Length.ToString());
				unmanagedImage5.ToManagedImage().Save(filepath.Replace(".png", "") + "_detect_square.png", ImageFormat.Png);
				blob = array.Where<Blob>(new Func<Blob, bool>(CaptchaSolver.Class21.Field0.Method1)).FirstOrDefault<Blob>();
				if (blob != null)
				{
					break;
				}
				image.UnlockBits(bitmapData);
				if (flag)
				{
					num++;
				}
				else
				{
					num--;
				}
				if (num <= 0)
				{
					flag = true;
					num = 146;
				}
				else if (num >= 300)
				{
					goto Block_9;
				}
			}
			return (float)(blob.Rectangle.X + blob.Rectangle.Width / 2);
			Block_9:
			return 0f;
		}

		// Token: 0x020000AF RID: 175
		[CompilerGenerated]
		[Serializable]
		private sealed class Class21
		{
			// Token: 0x06000252 RID: 594 RVA: 0x00065084 File Offset: 0x00063284
			internal bool Method0(Blob x)
			{
				return x.Rectangle.X != 1 && x.Rectangle.Y != 1;
			}

			// Token: 0x06000253 RID: 595 RVA: 0x000650B8 File Offset: 0x000632B8
			internal bool Method1(Blob b)
			{
				return b.Rectangle.X >= 88 && b.Rectangle.Width >= 87;
			}

			// Token: 0x040005B6 RID: 1462
			public static readonly CaptchaSolver.Class21 Field0 = new CaptchaSolver.Class21();

			// Token: 0x040005B7 RID: 1463
			public static Func<Blob, bool> Field1;

			// Token: 0x040005B8 RID: 1464
			public static Func<Blob, bool> Field2;
		}
	}
}
