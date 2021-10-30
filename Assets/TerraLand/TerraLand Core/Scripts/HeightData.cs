using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using BitMiracle.LibTiff.Classic;

public class HeightData : MonoBehaviour
{
    private const float everestPeak = 8848.0f;
    public static float[,] heightData;
    private static float[,] heightDataTemp;
    private static int width = 0;
    private static int length = 0;
    private static List<float> topCorner, bottomCorner, leftCorner, rightCorner;

    public static void LoadHeightDataFromESRITif (string fileName)
    {
        topCorner = new List<float>();
        bottomCorner = new List<float>();
        leftCorner = new List<float>();
        rightCorner = new List<float>();

        try
        {
            using (Tiff inputImage = Tiff.Open(fileName, "r"))
            {
                width = inputImage.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();
                length = inputImage.GetField(TiffTag.IMAGELENGTH)[0].ToInt();
                heightData = new float[length, width];
                heightDataTemp = new float[length, width];

                int tileHeight = inputImage.GetField(TiffTag.TILELENGTH)[0].ToInt();
                int tileWidth = inputImage.GetField(TiffTag.TILEWIDTH)[0].ToInt();

                byte[] buffer = new byte[tileHeight * tileWidth * 4];
                float[,] fBuffer = new float[tileHeight, tileWidth];

                for (int y = 0; y < length; y += tileHeight)
                {
                    for (int x = 0; x < width; x += tileWidth)
                    {
                        inputImage.ReadTile(buffer, 0, x, y, 0, 0);
                        Buffer.BlockCopy(buffer, 0, fBuffer, 0, buffer.Length);

                        for (int i = 0; i < tileHeight; i++)
                            for (int j = 0; j < tileWidth; j++)
                                if ((y + i) < length && (x + j) < width)
                                    heightDataTemp[y + i, x + j] = fBuffer[i, j];

                        //float progressDATA = Mathf.InverseLerp(0f, (float)length, (float)y);
                    }
                }
            }
        }
        catch { }

        float lowestPoint = heightDataTemp.Cast<float>().Min();

        // Rotate terrain heigts and normalize values
        for (int y = 0; y < width; y++)
        {
            for (int x = 0; x < length; x++)
            {
                float currentHeight = heightDataTemp[(width - 1) - y, x];

                try
                {
                    if (lowestPoint >= 0)
                        heightData[y, x] = (currentHeight - lowestPoint) / everestPeak;
                    else
                        heightData[y, x] = (currentHeight + Mathf.Abs(lowestPoint)) / everestPeak;
                }
                catch (ArgumentOutOfRangeException)
                {
                    heightData[y, x] = 0f;
                }

                // Check Terrain Corners
                // Top Row
                if (y == 0)
                    topCorner.Add(currentHeight);

                // Bottom Row
                else if (y == width - 1)
                    bottomCorner.Add(currentHeight);

                // Left Column
                if (x == 0)
                    leftCorner.Add(currentHeight);

                // Right Column
                else if (x == length - 1)
                    rightCorner.Add(currentHeight);
            }
        }

        CheckCornersTIFF(width, length, topCorner, leftCorner, bottomCorner, rightCorner);
    }

    private static void CheckCornersTIFF(int width, int length, List<float> topCorner, List<float> leftCorner, List<float> bottomCorner, List<float> rightCorner)
    {
        // Check Top
        if (topCorner.All(o => o == topCorner.First()))
        {
            for (int y = 0; y < width; y++)
                for (int x = 0; x < length; x++)
                    if (y == 0)
                        heightData[y, x] = heightData[y + 1, x];
        }

        // Check Bottom
        if (bottomCorner.All(o => o == bottomCorner.First()))
        {
            for (int y = 0; y < width; y++)
                for (int x = 0; x < length; x++)
                    if (y == width - 1)
                        heightData[y, x] = heightData[y - 1, x];
        }

        // Check Left
        if (leftCorner.All(o => o == leftCorner.First()))
        {
            for (int y = 0; y < width; y++)
                for (int x = 0; x < length; x++)
                    if (x == 0)
                        heightData[y, x] = heightData[y, x + 1];
        }

        // Check Right
        if (rightCorner.All(o => o == rightCorner.First()))
        {
            for (int y = 0; y < width; y++)
                for (int x = 0; x < length; x++)
                    if (x == length - 1)
                        heightData[y, x] = heightData[y, x - 1];
        }
    }
}

