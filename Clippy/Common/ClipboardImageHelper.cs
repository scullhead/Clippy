﻿/// Clippy - File: "ClipboardImageHelper.cs"
/// Copyright © 2018 by Tobias Zorn
/// Licensed under GNU GENERAL PUBLIC LICENSE

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Clippy.Common
{

    /// <summary>
    /// Helper class to handle WPF clipboard image stuff
    /// </summary>
    /// <remarks>
    /// Code Source: https://www.thomaslevesque.com/2009/02/05/wpf-paste-an-image-from-the-clipboard/
    /// </remarks>
    public static class ClipboardImageHelper
    {
        [StructLayout(LayoutKind.Sequential, Pack = 2)]
        private struct BITMAPFILEHEADER
        {
            public static readonly short BM = 0x4d42; // BM

            public short bfType;
            public int bfSize;
            public short bfReserved1;
            public short bfReserved2;
            public int bfOffBits;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct BITMAPINFOHEADER
        {
            public int biSize;
            public int biWidth;
            public int biHeight;
            public short biPlanes;
            public short biBitCount;
            public int biCompression;
            public int biSizeImage;
            public int biXPelsPerMeter;
            public int biYPelsPerMeter;
            public int biClrUsed;
            public int biClrImportant;
        }

        public static ImageSource ImageFromClipboardDib()
        {
            MemoryStream ms = Clipboard.GetData("DeviceIndependentBitmap") as MemoryStream;
            if (ms != null)
            {
                byte[] dibBuffer = new byte[ms.Length];
                ms.Read(dibBuffer, 0, dibBuffer.Length);

                BITMAPINFOHEADER infoHeader = FromByteArray<BITMAPINFOHEADER>(dibBuffer);

                int fileHeaderSize = Marshal.SizeOf(typeof(BITMAPFILEHEADER));
                int infoHeaderSize = infoHeader.biSize;
                int fileSize = fileHeaderSize + infoHeader.biSize + infoHeader.biSizeImage;

                BITMAPFILEHEADER fileHeader = new BITMAPFILEHEADER();
                fileHeader.bfType = BITMAPFILEHEADER.BM;
                fileHeader.bfSize = fileSize;
                fileHeader.bfReserved1 = 0;
                fileHeader.bfReserved2 = 0;
                fileHeader.bfOffBits = fileHeaderSize + infoHeaderSize + infoHeader.biClrUsed * 4;

                byte[] fileHeaderBytes = ToByteArray<BITMAPFILEHEADER>(fileHeader);

                MemoryStream bitmapStream = new MemoryStream();
                bitmapStream.Write(fileHeaderBytes, 0, fileHeaderSize);
                bitmapStream.Write(dibBuffer, 0, dibBuffer.Length);
                bitmapStream.Seek(0, SeekOrigin.Begin);

                return BitmapFrame.Create(bitmapStream);
            }
            return null;
        }

        public static byte[] BitmapSourceToByteArray(BitmapEncoder encoder, BitmapSource bitmapSource)
        {
            byte[] bytes = null;
            if (bitmapSource != null)
            {
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                using (var stream = new MemoryStream())
                {
                    encoder.Save(stream);
                    bytes = stream.ToArray();
                }
            }

            return bytes;
        }

        public static BitmapSource ByteArrayToBitmapSource(byte[] data)
        {
            var bitmap = new BitmapImage();
            using (var stream = new MemoryStream(data))
            {
                //return BitmapFrame.Create(stream);  
                bitmap.BeginInit();
                bitmap.StreamSource = stream;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();
            }

            return bitmap;
        }

        private static T FromByteArray<T>(byte[] bytes) where T : struct
        {
            IntPtr ptr = IntPtr.Zero;
            try
            {
                int size = Marshal.SizeOf(typeof(T));
                ptr = Marshal.AllocHGlobal(size);
                Marshal.Copy(bytes, 0, ptr, size);
                object obj = Marshal.PtrToStructure(ptr, typeof(T));
                return (T)obj;
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                    Marshal.FreeHGlobal(ptr);
            }
        }

        private static byte[] ToByteArray<T>(T obj) where T : struct
        {
            IntPtr ptr = IntPtr.Zero;
            try
            {
                int size = Marshal.SizeOf(typeof(T));
                ptr = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(obj, ptr, true);
                byte[] bytes = new byte[size];
                Marshal.Copy(ptr, bytes, 0, size);
                return bytes;
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                    Marshal.FreeHGlobal(ptr);
            }
        }

    }
}
