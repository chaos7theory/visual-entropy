﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Reflection;
using VisualEntropy.BinaryToBitmap;

namespace C7Theory.VisualEntropy
{
	class Program
	{
		// Parameters that are one character long can use only '-' as a leading character.
		// Otherwise, 'word-length' paramters follow the convention of '--' beforehand.
		// The last parameter must always be the output file name/path.
		public static int Main(string[] args)
		{
			int exitCode = 0;
			if (args.Contains("--help"))
			{
				DisplayHelp();
				exitCode = 1;
			}
			if (exitCode == 0 && (args.Contains("-v") || args.Contains("--version")))
			{
				DisplayVersion();
				exitCode = 1;
			}

			if (exitCode != 0)
			{
				return exitCode;
			}

			try
			{
				ValidateParameters(args);
				int width = 256;
				if (args.Contains("-w") || args.Contains("--width"))
				{
					width = Int32.Parse(args[Array.IndexOf(args, "-w") + 1]);
				}
				int height = 256;
				if (args.Contains("-h") || args.Contains("--height"))
				{
					height = Int32.Parse(args[Array.IndexOf(args, "-h") + 1]);
				}
				bool isAlpha = false;
				if (args.Contains("-a")) { isAlpha = true; }
#if DEBUG
				string path = "temp.png";
				if (args.Length > 0)
				{
					path = args[args.Length - 1];
				}
#else
				if (args.Length < 1)
				{
					DisplayHelp();
					return 1;
				}
				string path = args[args.Length - 1];
#endif
				// Converts the path to a universal, absolute path that can be used on both Windows and Unix-based paltforms.
				path = new Uri(Path.GetFullPath(path)).LocalPath;

				Console.WriteLine("Visual Entropy");
				Console.WriteLine("Copyright (c) 2013 Christopher Robert Philabaum");
				Console.WriteLine();

				Random rand = new Random();
				Console.Write("Setting the values... ");
				int[,] pixels = new int[width, height];
				if (args.Contains("-g16"))
				{
					pixels = Get16GrayscalePixels(rand, width, height, isAlpha);
				}
				else if (args.Contains("-g"))
				{
					pixels = Get256GrayscalePixels(rand, width, height, isAlpha);
				}
				else
				{
					pixels = GetBinaryPixels(rand, width, height, isAlpha);
				}
				Console.WriteLine("done!");

				Console.Write("Saving the picture to {0}... ", path);
				BinaryToBitmap.ToPNG(width, height, pixels, "temp.png");
				Console.WriteLine("done!");
#if DEBUG
				// Opens the picture if it is in DEBUG mode.
				System.Diagnostics.Process.Start(path);
#endif
			}
			catch (Exception error)
			{
				Console.WriteLine();
				Console.WriteLine(error.Message);
				exitCode = 2;
#if DEBUG
				Console.WriteLine(error.StackTrace);
			}
			finally
			{
				Console.WriteLine();
				Console.Write("Press ENTER to continue... ");
				Console.ReadKey();
#endif
			}

			return exitCode;
		}

		#region Command Prompt
		public static void DisplayHelp()
		{
			Console.Write("Visual Entropy: ");
			Console.WriteLine(GetVersionString());
			Console.WriteLine("Copyright (c) 2013 Christopehr Robert Philabaum");
			Console.WriteLine();
			Console.WriteLine("A simple program that generates a bitmap through pseudo-random generation. This can have several uses, such as demonstrating the effectiveness of pseudo-random number generation, pattern recognition in cognitive science, or other personal uses (e.g. for the heck of it).");
			Console.WriteLine();
			Console.WriteLine("Usage: Visual Entropy [options] path/filename.png");
			Console.WriteLine("Options:");
			Console.WriteLine("(Defaults to 2 states: black or white pixels)");
			Console.WriteLine("  --help\t\t\tThe help display.");
			Console.WriteLine("  -w pixels | --width pixels\tPicture width. Defaults to 256.");
			Console.WriteLine("  -h pixels | --height pixels\tPicture height. Defaults to 256.");
			Console.WriteLine("  -a\t\t\tInclude the alpha (transparency) channel.");
			Console.WriteLine("  -g\t\t\t8-bit (256 states) grayscale.");
			Console.WriteLine("  -g16\t\t\t4-bit (16 states) grayscale.");

		}

		public static void DisplayVersion()
		{
			Console.WriteLine(GetVersionString());
		}

		public static Version GetVersion()
		{
			return Assembly.GetExecutingAssembly().GetName().Version;
		}

		public static string GetVersionString()
		{
			Version version = GetVersion();
			string versionText = "v";
			versionText += version.Major;
			versionText += "." + version.Minor;
			if (version.Build != 0 || version.Revision != 0)
			{
				versionText += "." + version.Build;
			}
			if (version.Revision != 0)
			{
				versionText += "." + version.Revision;
			}

			return versionText;
		}

		public static void ValidateParameters(string[] args)
		{
			for (int index = 0; index < args.Length; index++)
			{
				string param = args[index];
				// If the first character of the parameter is a dash.
				if (param[0] == '-')
				{
					// Switch between the different cases of the second character.
					switch (param)
					{
						case ("-w"):
						case ("--width"):
						case ("-h"):
						case ("--height"):
							// If it isn't the last argument and if the next argument doesn't begin with a dash.
							if (index != args.Length - 1 && args[index + 1][0] != '-')
							{
								index++;
							}
							else
							{
								throw new FormatException(String.Format("Missing value for argument '{0}'.",
									param[1]));
							}
							break;
						case ("--help"):
						case ("-v"):
						case ("--version"):
						case ("-a"):
						case ("-g"):
						case ("-g16"):
							break;
						default:
							if (index != args.Length - 1)
								throw new FormatException("Invalid argument.");
							break;
					}
				}

				if (!args[args.Length - 1].ToLower().EndsWith(".png"))
				{
					throw new FormatException("The file doesn't end with '.png' or '.PNG'.");
				}
			}
		}
		#endregion



		#region Pixel Methods
		public static int[,] GetBinaryPixels(Random rand, int width, int height, bool isAlpha = false)
		{
			// Width, Height, RGBa
			int[,] pixels = new int[width, height];
			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					byte bit = 0;
					if ((uint)rand.Next(int.MinValue, int.MaxValue) >> 31 > 0) { bit = 0xFF; }
					byte alpha = 255;
					if (isAlpha) { alpha = (byte)((rand.Next(int.MinValue, int.MaxValue) >> 24) & 0xFF); }

					pixels[x, y] = 0;
					pixels[x, y] = SetAlpha(pixels[x, y], alpha);
					pixels[x, y] = SetRed(pixels[x, y], bit);
					pixels[x, y] = SetGreen(pixels[x, y], bit);
					pixels[x, y] = SetBlue(pixels[x, y], bit);
				}
			}

			return pixels;
		}

		public static int[,] Get16GrayscalePixels(Random rand, int width, int height, bool isAlpha = false)
		{
			// Width, Height, RGBa
			int[,] pixels = new int[width, height];
			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					// 4-digit binary number for 16 possible states.
					byte binary = (byte)((rand.Next(int.MinValue, int.MaxValue) >> 28) & 0x0F);
					byte alpha = 255;
					if (isAlpha) { alpha = (byte)((rand.Next(int.MinValue, int.MaxValue) >> 24) & 0xFF); }
					byte gray = 0;
					// Converts the 4-digit binary number to an 8-digit byte spread out evenly.
					for (int shift = 3; shift >= 0; shift--)
					{
						if ((int)((binary >> shift) & 0x01) > 0) { gray += 0x03; }
						if (shift > 0)
						{
							gray <<= 2;
						}
					}

					pixels[x, y] = 0;
					pixels[x, y] = SetAlpha(pixels[x, y], alpha);
					pixels[x, y] = SetRed(pixels[x, y], gray);
					pixels[x, y] = SetGreen(pixels[x, y], gray);
					pixels[x, y] = SetBlue(pixels[x, y], gray);
				}
			}

			return pixels;
		}

		public static int[,] Get256GrayscalePixels(Random rand, int width, int height, bool isAlpha = false)
		{
			// Width, Height, RGBa
			int[,] pixels = new int[width, height];
			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					// 8-digit binary number for 256 possible states.
					byte gray = (byte)((rand.Next(int.MinValue, int.MaxValue) >> 24) & 0xFF);
					byte alpha = 255;
					if (isAlpha) { alpha = (byte)((rand.Next(int.MinValue, int.MaxValue) >> 24) & 0xFF); }

					pixels[x, y] = 0;
					pixels[x, y] = SetAlpha(pixels[x, y], alpha);
					pixels[x, y] = SetRed(pixels[x, y], gray);
					pixels[x, y] = SetGreen(pixels[x, y], gray);
					pixels[x, y] = SetBlue(pixels[x, y], gray);
				}
			}

			return pixels;
		}



		private static byte[] GetRGBa(int color)
		{
			// Red, Green, Blue, Alpha
			byte[] cBytes = new byte[4];
			cBytes[0] = Convert.ToByte((color >> 16) & 0x0FF);
			cBytes[1] = Convert.ToByte((color >> 8) & 0x0FF);
			cBytes[2] = Convert.ToByte(color & 0x0FF);
			cBytes[3] = Convert.ToByte((color >> 24) & 0x0FF);

			return cBytes;
		}

		private static byte[] GetRGBa(Color color)
		{
			return GetRGBa(color.ToArgb());
		}

		private static int SetRed(int color, byte red)
		{
			byte[] cBytes = GetRGBa(color);
			color = Convert.ToInt32(cBytes[3]); // Alpha
			color = Convert.ToInt32((color << 8) + red);
			color = Convert.ToInt32((color << 8) + cBytes[1]); // Green
			color = Convert.ToInt32((color << 8) + cBytes[2]); // Blue

			return color;
		}

		private static int SetGreen(int color, byte green)
		{
			byte[] cBytes = GetRGBa(color);
			color = Convert.ToInt32(cBytes[3]); // Alpha
			color = Convert.ToInt32((color << 8) + cBytes[0]);
			color = Convert.ToInt32((color << 8) + green); // Green
			color = Convert.ToInt32((color << 8) + cBytes[2]); // Blue

			return color;
		}

		private static int SetBlue(int color, byte blue)
		{
			byte[] cBytes = GetRGBa(color);
			color = Convert.ToInt32(cBytes[3]); // Alpha
			color = Convert.ToInt32((color << 8) + cBytes[0]); // Red
			color = Convert.ToInt32((color << 8) + cBytes[1]); // Green
			color = Convert.ToInt32((color << 8) + blue); // Blue

			return color;
		}

		private static int SetAlpha(int color, byte alpha)
		{
			byte[] cBytes = GetRGBa(color);
			color = Convert.ToInt32(alpha);
			color = Convert.ToInt32((color << 8) + cBytes[0]); // Red
			color = Convert.ToInt32((color << 8) + cBytes[1]); // Green
			color = Convert.ToInt32((color << 8) + cBytes[2]); // Blue

			return color;
		}
		#endregion
	}
}
