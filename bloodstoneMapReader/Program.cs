using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace bloodstoneMapReader
{
    class Program
    {
        static void Main(string[] args)
        {
            var projPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\"));
            var colorFile = new BinaryReader(new FileStream(Path.Combine(projPath, "colortable.bin"), FileMode.Open));
            var numColors = (int)(colorFile.BaseStream.Length / 24);
            var colorMap = new Dictionary<byte, Color>(numColors);

            for (byte i = 0; i < numColors; i++)
            {
                var colorData = colorFile.ReadBytes(24);
                var a = colorData[4];
                var r = colorData[6];
                var g = colorData[8];
                var b = colorData[10];
                var color = Color.FromArgb(a, r, g, b);
                colorMap[i] = color;
            }

            // 401438
            // 401 = y (0) 438 = x (0)
            // 401439
            // 401 = y (0) 439 = x (16)
            // ...

            // --X-- --Y--
            // 00 02 20 06 - 401440 (0200, 0620) (16 ints between them - 0x10 = 16)
            // 10 02 20 06 - 401441 (0210, 0620)
            // 20 02 20 06 - 401442
            // 30 02 20 06 - 401443
            // 40 02 20 06 - 401444
            // 50 02 20 06 - 401445
            // 60 02 20 06 - 401446
            // 70 02 20 06 - 401447
            // 80 02 20 06 - 401448
            // 90 02 20 06 - 401449
            // A0 02 20 06 - 401450
            // B0 02 20 06 - 401451
            // C0 02 20 06 - 401452
            // D0 02 20 06 - 401453
            // E0 02 20 06 - 401454
            // F0 02 20 06 - 401455 (02F0, 0620)
            // 00 03 20 06 - 401456 (0300, 0620)
            // 10 03 20 06 - 401457 (0310, 0620)
            // 20 03 20 06 - 401458 (0320, 0620)
            // 30 03 20 06 - 401459 (0330, 0620)
            // (16 ints y axis between 401 and 405)
            //  - 405530
            //  - 405531
            // C0 01 30 06 - 405532 (01C0, 0630)
            // D0 01 30 06 - 405533 (01D0, 0630)

            // First found file.
            // 10 04 20 04 - 270401 (0410, 0420)
            // Next Y: 274, 208...

            // Files go from (y,x) to (y, x+1) for X axis
            // Files fo from (y,x) to (y+4, x) for Y axis

            // 270401 - 0410, 0420
            // 274497 - 0410, 0430
            // - start at same X but different Y

            var tileTypes = new HashSet<byte>();

            // Guess 2000x2000 is enough.
            for (byte floor = 0x00; floor < 0xF; floor++)
            {
                Console.WriteLine($"Generating PNG for floor { floor }...");

                using (var bmp = new Bitmap(2000, 2000))
                using (Graphics gfx = Graphics.FromImage(bmp))
                using (SolidBrush brush = new SolidBrush(Color.Black))
                {
                    gfx.FillRectangle(brush, 0, 0, bmp.Width, bmp.Height);
                    var files = Directory.GetFiles(Path.Combine(projPath, "minimap"), $"minimap*_{ floor }.map");

                    //int biggestX = 0, biggestY = 0;
                    foreach (var file in files)
                    {
                        using (var reader = new BinaryReader(new FileStream(file, FileMode.Open)))
                        {
                            var x = reader.ReadInt16();
                            var y = reader.ReadInt16();
                            var z = reader.ReadByte();

                            if (z != floor)
                            {
                                continue;
                            }

                            //biggestX = Math.Max(x, biggestX);
                            //biggestY = Math.Max(y, biggestY);

                            for (int i = y; i < y + 16; i++)
                            {
                                for (int j = x; j < x + 16; j++)
                                {
                                    var bytes = reader.ReadBytes(3);
                                    var color = colorMap[bytes[1]];

                                    //var highlight = Color.HotPink;
                                    //var def = Color.DarkGray;
                                    //switch (bytes[2])
                                    //{
                                    //    case 0xFF:
                                    //        break;
                                    //    case 0x64:
                                    //        break;
                                    //    case 0x78:
                                    //        break;
                                    //    case 0x82:
                                    //        break;
                                    //    case 0x7D:
                                    //        break;
                                    //    case 0x6E:
                                    //        break;
                                    //    case 0x87:
                                    //        break;
                                    //    case 0x8C:
                                    //        break;
                                    //    case 0xB4:
                                    //        break;
                                    //    case 0xC8:
                                    //        break;
                                    //    case 0x96:
                                    //        break;
                                    //    case 0xFE:
                                    //        break;
                                    //    case 0xFA:
                                    //        break;
                                    //    case 0x9D:
                                    //        break;
                                    //    case 0xF5:
                                    //        break;
                                    //}

                                    bmp.SetPixel(j, i, color);
                                    tileTypes.Add(bytes[2]);
                                }
                            }
                        }
                    }

                    bmp.Save($"map_{floor}.png", ImageFormat.Png);
                }
            }
        }
    }
}
