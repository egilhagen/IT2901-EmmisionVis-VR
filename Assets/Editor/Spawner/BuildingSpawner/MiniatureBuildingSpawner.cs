﻿using Editor.NetCDF;
using Editor.NetCDF.Types;
using Microsoft.Geospatial;
using Microsoft.Maps.Unity;
using UnityEngine;

namespace Editor.Spawner.BuildingSpawner
{
    public class MiniatureBuildingSpawner : BaseBuildingSpawner
    {
        private double _metersPerUnit;
        private Vector3 _worldSpacePin;
        
        
        public MiniatureBuildingSpawner(string mapName, string cdfFilePath, GameObject map, float rotationAngle)
            : base(mapName, cdfFilePath, map, rotationAngle)
        {
        }
        
        
        protected override void CreateAndSetupBuildingHolder()
        {
            Debug.Log("Creating building holder");
            
            MapRenderer mapRenderer = Map.GetComponent<MapRenderer>();
            
            _metersPerUnit = mapRenderer.ComputeUnityToMapScaleRatio(SelectedCdfAttributes.position) / Map.transform.lossyScale.x;
            _worldSpacePin = mapRenderer.TransformLatLonAltToWorldPoint(SelectedCdfAttributes.position);
            
            VisualizationHolder = new GameObject(HolderName);
            VisualizationHolder.transform.SetParent(Map.transform, false);
            VisualizationHolder.transform.localRotation = Quaternion.Euler(0, RotationAngle, 0);
            
            MapPin mapPin = VisualizationHolder.AddComponent<MapPin>();
            mapPin.Location = SelectedCdfAttributes.position;
            mapPin.UseRealWorldScale = true;
            mapPin.AltitudeReference = AltitudeReference.Ellipsoid;
        }


        protected override void SpawnBuilding(BuildingData buildingData)
        {
            float distanceX = (float)(buildingData.X / _metersPerUnit);
            float distanceZ = (float)(buildingData.Y/ _metersPerUnit);
            string objectName = $"Small Building {VisualizationHolder.transform.childCount + 1}";

            Vector3 mapUp = Map.transform.up;
    
            Vector3 rotatedOffset = Quaternion.Euler(0, RotationAngle, 0) * new Vector3(distanceX, 0, distanceZ);

            Vector3 origin =
                _worldSpacePin +
                Map.transform.right * rotatedOffset.x +
                Map.transform.forward * rotatedOffset.z +
                mapUp * (10.0f * Map.transform.lossyScale.y);
            
            Ray ray = new(origin, mapUp * -1);
            
            Map.GetComponent<MapRenderer>().Raycast(ray, out MapRendererRaycastHit hitInfo);
            
            Vector3 pos = VisualizationHolder.transform.InverseTransformVector(hitInfo.Point - _worldSpacePin) * ((float)_metersPerUnit * Map.transform.lossyScale.x);
            GameObject building = Object.Instantiate(BuildingPrefab, VisualizationHolder.transform, false);
            
            building.name = objectName;
            building.transform.localPosition += pos;
        }
    }
}