﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ImageWriter.Helper
{
    public class WriterHelper
    {
        public enum ImageFormat
        {
            known,
            unknown
        }

        public static ImageFormat GetImageFormat(MemoryStream ms)
        {
            byte[] buffer = new byte[8];
            byte[] bufferEnd = new byte[2];

            var bmp = new byte[] { 0x42, 0x4D };               // BMP "BM"
            var gif87a = new byte[] { 0x47, 0x49, 0x46, 0x38, 0x37, 0x61 };     // "GIF87a"
            var gif89a = new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 };     // "GIF89a"
            var png = new byte[] { 0x89, 0x50, 0x4e, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };   // PNG "\x89PNG\x0D\0xA\0x1A\0x0A"
            var tiffI = new byte[] { 0x49, 0x49, 0x2A, 0x00 }; // TIFF II "II\x2A\x00"
            var tiffM = new byte[] { 0x4D, 0x4D, 0x00, 0x2A }; // TIFF MM "MM\x00\x2A"
            var jpeg = new byte[] { 0xFF, 0xD8, 0xFF };        // JPEG JFIF (SOI "\xFF\xD8" and half next marker xFF)
            var jpegEnd = new byte[] { 0xFF, 0xD9 };           // JPEG EOI "\xFF\xD9"

            try
            {
                if (ms.Length > buffer.Length)
                {
                    ms.Position = 0;
                    ms.Seek(0, SeekOrigin.Begin);
                    ms.Read(buffer, 0, buffer.Length);
                    ms.Position = (int)ms.Length - bufferEnd.Length;
                    ms.Read(bufferEnd, 0, bufferEnd.Length);
                }
                ms.Close();
                ms.Dispose();

                if (ByteArrayStartsWith(buffer, bmp) ||
                    ByteArrayStartsWith(buffer, gif87a) ||
                     ByteArrayStartsWith(buffer, gif89a) ||
                     ByteArrayStartsWith(buffer, png) ||
                     ByteArrayStartsWith(buffer, tiffI) ||
                     ByteArrayStartsWith(buffer, tiffM))
                {
                    return ImageFormat.known;
                }

                if ( ByteArrayStartsWith(buffer, jpeg))
                {
                    // Offset 0 (Two Bytes): JPEG SOI marker (FFD8 hex)
                    // Offest 1 (Two Bytes): Application segment (FF?? normally ??=E0)
                    // Trailer (Last Two Bytes): EOI marker FFD9 hex
                    if ( ByteArrayStartsWith(bufferEnd, jpegEnd))
                    {
                        return ImageFormat.known;
                    }
                }
            }
            catch (Exception)
            {
                return ImageFormat.unknown;
            }

            return ImageFormat.unknown;
        }
        /// <summary>
        /// Returns a value indicating whether a specified subarray occurs within array
        /// </summary>
        /// <param name="a">Main array</param>
        /// <param name="b">Subarray to seek within main array</param>
        /// <returns>true if a array starts with b subarray or if b is empty; otherwise false</returns>
        private static bool ByteArrayStartsWith(byte[] a, byte[] b)
        {
            if (a.Length < b.Length)
            {
                return false;
            }

            for (int i = 0; i < b.Length; i++)
            {
                if (a[i] != b[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
