using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using AForge;
using AForge.Imaging;
using AForge.Imaging.Filters;

namespace TikTok_Automation_Library_Non_Jail.Captcha
{
	// Token: 0x020000B1 RID: 177
	internal class FiguredCaptcha
	{
		// Token: 0x06000255 RID: 597 RVA: 0x000650F0 File Offset: 0x000632F0
		public FiguredCaptcha(string CaptchaSceenShotPath)
		{
			this.filename_directory = CaptchaSceenShotPath;
			string text = Guid.NewGuid().ToString();
			if (!Directory.Exists("Captcha"))
			{
				Directory.CreateDirectory("Captcha");
			}
			this.PathC += text;
			Directory.CreateDirectory(this.PathC);
		}

		// Token: 0x06000256 RID: 598 RVA: 0x00064CE0 File Offset: 0x00062EE0
		private static Image Crop(Image img, Rectangle cropArea)
		{
			Bitmap bitmap = new Bitmap(img);
			return bitmap.Clone(cropArea, bitmap.PixelFormat);
		}

		// Token: 0x06000257 RID: 599 RVA: 0x0006518C File Offset: 0x0006338C
		public Coords CalculatePositions(bool IsX)
		{
			Image image = new Bitmap(this.filename_directory);
			this.img_left = 49;
			this.img_top = 241;
			if (IsX)
			{
				this.img_left = 49;
				this.img_top = 314;
			}
			int num = 99;
			int num2 = 483;
			int num3 = 343;
			int num4 = 552;
			if (IsX)
			{
				num = 148;
				num2 = 941;
				num3 = num - 976;
				num4 = num2 - 1458;
			}
			Image image2 = FiguredCaptcha.Crop(image, new Rectangle(new Point
			{
				X = num,
				Y = num2
			}, new Size
			{
				Height = num3,
				Width = num4
			}));
			this.filename_directory = this.filename_directory.Replace(".png", "123.png");
			image2.Save(this.filename_directory);
			Image image3 = new Bitmap((Bitmap)Image.FromFile(this.filename_directory), new Size(277, 195));
			this.filename_directory = this.filename_directory.Replace(".png", "1aqq23.png");
			image3.Save(this.filename_directory);
			new FileInfo(this.filename_directory);
			string text = this.FigureFiltered(this.filename_directory);
			this.SaveCoordinates(text);
			this.CropBounds(text);
			this.CalcHash();
			string[] array = this.SravnitHashi().Split(new char[] { ';' });
			return new Coords
			{
				x1 = int.Parse(array[0].Split(new char[] { ',' })[0]),
				y1 = int.Parse(array[1].Split(new char[] { ',' })[0]),
				x2 = int.Parse(array[2].Split(new char[] { ',' })[0]),
				y2 = int.Parse(array[3].Split(new char[] { ',' })[0])
			};
		}

		// Token: 0x06000258 RID: 600 RVA: 0x00065388 File Offset: 0x00063588
		private string FigureFiltered(string image_path)
		{
			this.img = null;
			string text2;
			try
			{
				this.img = (Bitmap)Image.FromFile(image_path);
				new ColorFiltering
				{
					Red = new IntRange(200, 255),
					Green = new IntRange(200, 255),
					Blue = new IntRange(200, 255)
				}.ApplyInPlace(this.img);
				for (int i = 0; i <= this.img.Width - 1; i++)
				{
					for (int j = 0; j <= this.img.Height - 1; j++)
					{
						Color pixel = this.img.GetPixel(i, j);
						Color color;
						if (pixel.R >= 20 || pixel.G >= 20 || pixel.B >= 20)
						{
							color = Color.FromArgb((int)pixel.A, 0, 0, 0);
						}
						else
						{
							color = Color.FromArgb((int)pixel.A, 255, 255, 255);
						}
						this.img.SetPixel(i, j, color);
					}
				}
				string text = image_path.Replace(".png", "") + "_filtered_clone.png";
				this.img.Save(text);
				text2 = text;
			}
			finally
			{
				this.img.Dispose();
				File.Delete(image_path);
			}
			return text2;
		}

		// Token: 0x06000259 RID: 601 RVA: 0x00065500 File Offset: 0x00063700
		private void SaveCoordinates(string image_path)
		{
			this.img = null;
			this.click_points = new List<string>();
			this.bounds = new List<string>();
			try
			{
				this.img = (Bitmap)Image.FromFile(image_path);
				BlobCounter blobCounter = new BlobCounter();
				blobCounter.ProcessImage(this.img);
				foreach (Rectangle rectangle in blobCounter.GetObjectsRectangles())
				{
					if (rectangle.Width > 15 && rectangle.Height > 15)
					{
						double num = 0.0;
						double num2 = 0.0;
						double num3 = 0.5;
						double num4 = 0.5;
						double num5 = (double)rectangle.X + (double)rectangle.Width * (num3 - num);
						double num6 = (double)rectangle.Y + (double)rectangle.Height * (num4 - num2);
						this.click_points.Add(Convert.ToString(num5) + ";" + Convert.ToString(num6));
						this.bounds.Add(string.Concat(new string[]
						{
							Convert.ToString(rectangle.X),
							";",
							Convert.ToString(rectangle.Width),
							";",
							Convert.ToString(rectangle.Y),
							";",
							Convert.ToString(rectangle.Height)
						}));
					}
				}
				this.img.Dispose();
			}
			catch
			{
				this.img.Dispose();
				File.Delete(image_path);
			}
		}

		// Token: 0x0600025A RID: 602 RVA: 0x000656B0 File Offset: 0x000638B0
		private void CropBounds(string image_path)
		{
			this.path_image_crops = new List<string>();
			try
			{
				Image image = Image.FromFile(image_path);
				foreach (string text in this.bounds)
				{
					string[] array = text.Split(new char[] { ';' });
					int num = int.Parse(array[0]);
					int num2 = int.Parse(array[1]);
					int num3 = int.Parse(array[2]);
					int num4 = int.Parse(array[3]);
					Image image2 = new Bitmap(num2, num4);
					using (Graphics graphics = Graphics.FromImage(image2))
					{
						graphics.Clear(Color.Transparent);
						float num5 = (float)num2 / (float)image.Width;
						float num6 = (float)num4 / (float)image.Height;
						graphics.DrawImage(image, 0, 0, new Rectangle(num, num3, num2, num4), GraphicsUnit.Pixel);
					}
					string text2 = Path.Combine(Directory.GetCurrentDirectory(), this.PathC, string.Format("crop{0}.png", this.bounds.IndexOf(text)));
					this.path_image_crops.Add(text2);
					image2.Save(text2, ImageFormat.Png);
				}
			}
			catch
			{
			}
		}

		// Token: 0x0600025B RID: 603 RVA: 0x00065834 File Offset: 0x00063A34
		private void CalcHash()
		{
			this.List_hash = new List<string>();
			foreach (string text in this.path_image_crops)
			{
				string text2 = "";
				try
				{
					Bitmap bitmap = (Bitmap)Image.FromFile(text);
					Bitmap bitmap2 = new Bitmap(bitmap, new Size(64, 64));
					bitmap.Dispose();
					for (int i = 0; i < bitmap2.Width; i++)
					{
						for (int j = 0; j < bitmap2.Height; j++)
						{
							if (bitmap2.GetPixel(j, i).R < 20 && bitmap2.GetPixel(j, i).G < 20 && bitmap2.GetPixel(j, i).B < 20)
							{
								text2 += "1";
							}
							else
							{
								text2 += "0";
							}
						}
					}
					bitmap2.Dispose();
					this.List_hash.Add(text2);
				}
				catch
				{
					File.Delete(text);
				}
			}
		}

		// Token: 0x0600025C RID: 604 RVA: 0x0006596C File Offset: 0x00063B6C
		private string SravnitHashi()
		{
			int num = Convert.ToInt32(this.path_image_crops.Count);
			int num2 = 5000;
			string[] array = new string[0];
			string[] array2 = new string[0];
			string text7;
			try
			{
				for (int i = 0; i < num; i++)
				{
					for (int j = 0; j < num; j++)
					{
						string text = this.List_hash[i];
						string text2 = this.List_hash[j];
						int num3 = 0;
						if (!text.Equals(text2))
						{
							for (int k = 0; k < 4096; k++)
							{
								if ((byte)text[k] != (byte)text2[k])
								{
									num3++;
								}
							}
							if (num3 < num2)
							{
								num2 = num3;
								array = this.click_points[i].Split(new char[] { ';' });
								array2 = this.click_points[j].Split(new char[] { ';' });
							}
						}
					}
				}
				string text3 = Convert.ToString(Convert.ToInt32(this.img_left) + Convert.ToInt32(array[0].Split(new char[] { ',' })[0]));
				string text4 = Convert.ToString(Convert.ToInt32(this.img_top) + Convert.ToInt32(array[1].Split(new char[] { ',' })[0]));
				string text5 = Convert.ToString(Convert.ToInt32(this.img_left) + Convert.ToInt32(array2[0].Split(new char[] { ',' })[0]));
				string text6 = Convert.ToString(Convert.ToInt32(this.img_top) + Convert.ToInt32(array2[1].Split(new char[] { ',' })[0]));
				text7 = string.Concat(new string[] { text3, ";", text4, ";", text5, ";", text6 });
			}
			catch
			{
				text7 = "error";
			}
			return text7;
		}

		// Token: 0x040005BD RID: 1469
		private Bitmap img;

		// Token: 0x040005BE RID: 1470
		private string filename_directory;

		// Token: 0x040005BF RID: 1471
		private string PathC = "Captcha/";

		// Token: 0x040005C0 RID: 1472
		private List<string> click_points = new List<string>();

		// Token: 0x040005C1 RID: 1473
		private List<string> bounds = new List<string>();

		// Token: 0x040005C2 RID: 1474
		private List<string> path_image_crops = new List<string>();

		// Token: 0x040005C3 RID: 1475
		private List<string> List_hash = new List<string>();

		// Token: 0x040005C4 RID: 1476
		private int img_left;

		// Token: 0x040005C5 RID: 1477
		private int img_top;
	}
}
