﻿using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Editor.NetCDF
{
    public static class BuildingDataLoader
    {
        public static List<BuildingData> GetBuildingData(string mapName)
        {
            List<BuildingData> buildingDataList = new();
            
            string dataPath = $"{Application.dataPath}/Resources/MapData/{mapName}/BuildingData/buildingData.csv";
            if (!File.Exists(dataPath))
            {
                throw new ArgumentException("The file at " + dataPath + " does not exist!");
            }
            
            using StreamReader streamReader = new(dataPath);

            int currentLine = 0;

            while (streamReader.Peek() >= 0)
            {
                float[] data = AssertDataFormat(streamReader.ReadLine(), currentLine);
                buildingDataList.Add(new BuildingData(data[1], data[0], data[2]));

                currentLine++;
            }

            return buildingDataList;
        }

        
        private static float[] AssertDataFormat(string data, long line)
        {
            string[] stringValues = data.Split(',');

            if (stringValues.Length != 3)
            {
                Debug.Log("Invalid building data format");
                throw new ArgumentException(
                    $"Invalid data format at line: {line}. There should only be three columns of data values, but there are: {stringValues.Length}");
            }

            float[] floatArray = new float[3];

            for (int i = 0; i < stringValues.Length; i++)
            {
                if (!float.TryParse(stringValues[i], out floatArray[i]))
                {
                    Debug.Log("Invalid building data format");

                    throw new ArgumentException(
                        $"Invalid data format at line: {line}, and column: {i + 1}. Make sure the input data contains valid float values. Current value: {stringValues[i]}");
                }
            }

            return floatArray;
        }
    }
}