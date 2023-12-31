﻿using System;
using Editor.NetCDF;
using Editor.NetCDF.Types;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Editor.Spawner.RadiationSpawner
{
    /// <summary>
    /// The BaseRadiationSpawner class is an abstract base class for creating and managing
    /// radiation visualizations in a Unity scene using geospatial data.
    /// </summary>
    public abstract class BaseRadiationSpawner
    {
        /// <summary>
        /// Name of the holder GameObject. 
        /// </summary>
        protected const string HolderName = "Radiation Holder";
        
        /// <summary>
        /// Name of the prefab that will be used to display radiation.
        /// </summary>
        private const string PrefabName = "Radiation";
        
        /// <summary>
        /// Holder GameObject.
        /// </summary>
        protected GameObject RadiationHolder;

        /// <summary>
        /// The scope of the dataset containing the radiation data.
        /// </summary>
        protected readonly DatasetScope SelectedDatasetScope;
        
        /// <summary>
        /// The GameObject that will contain the building holder.
        /// </summary>
        protected readonly GameObject Map;
        
        /// <summary>
        /// How many degrees to rotate the data.
        /// It rotates based on the origin position defined in <see cref="SelectedDatasetScope"/>
        /// </summary>
        protected readonly float RotationAngle;
        
        
        private readonly Texture2D _radiationImage;
        private readonly Texture2D _heightMap;

        private static readonly int RadiationMap = Shader.PropertyToID("_RadiationMap");
        private static readonly int Heightmap = Shader.PropertyToID("_Heightmap");
        

        /// <summary>
        /// Calculates the distortion value to account for the Mercator projection's distortion
        /// away from the equator.
        /// </summary>
        protected virtual float LatDistortionValue =>
            (float) (1 / Math.Cos(Math.PI * SelectedDatasetScope.position.lat / 180.0));

        
        /// <summary>
        /// Initializes a new instance of the BaseRadiationSpawner class.
        /// </summary>
        /// <param name="mapName">The name of the map to spawn the radiation visualization in.</param>
        /// <param name="cdfFilePath">The file path of the Cloud Data File (CDF).</param>
        /// <param name="map">The GameObject representing the map in the Unity scene.</param>
        /// <param name="rotationAngle">The rotation angle for the radiation visualization.</param>
        protected BaseRadiationSpawner(string mapName, string cdfFilePath, GameObject map, float rotationAngle)
        {
            SelectedDatasetScope = ScopeDataGetter.GetDatasetScope(cdfFilePath);

            Map = map;
            RotationAngle = rotationAngle;

            _radiationImage = LoadFirstRadiationImage(mapName);
            _heightMap = ImageLoader.GetHeightMapImg(mapName);
        }

        
        /// <summary>
        /// Spawns and sets up the radiation visualization in the scene.
        /// </summary>
        public void SpawnAndSetupRadiation()
        {
            DeletePreviousHolder();
            CreateRadiationHolder();

            SpawnRadiation();

            Debug.Log("Finished creating radiation");
        }

        
        /// <summary>
        /// Creates the radiation holder GameObject. To be implemented in derived classes.
        /// </summary>
        protected abstract void CreateRadiationHolder();

        
        /// <summary>
        /// Instantiates the radiation prefab, sets its images, LOD group size, and scale,
        /// and then adds it to the RadiationHolder GameObject.
        /// </summary>
        private void SpawnRadiation()
        {
            GameObject radiationPrefab = Resources.Load<GameObject>($"Prefabs/{PrefabName}");

            if (radiationPrefab == null)
            {
                Debug.LogError($"Cloud prefab not found at 'Prefabs/{PrefabName}'");
                return;
            }

            GameObject rad = Object.Instantiate(radiationPrefab, RadiationHolder.transform, false);
            rad.name = "Radiation";

            SetRadiationImages(rad, _radiationImage, _heightMap);

            LODGroup lodGroup = rad.GetComponent<LODGroup>();
            lodGroup.size = SelectedDatasetScope.size.x;

            //Prefab base size is 1km
            float scale = SelectedDatasetScope.size.x / 1000.0f * LatDistortionValue;
            rad.transform.localScale = new Vector3(scale, SelectedDatasetScope.size.x / 1000.0f, scale);
        }

        
        /// <summary>
        /// Searches for and deletes any existing radiation holder GameObjects in the map.
        /// </summary>
        private void DeletePreviousHolder()
        {
            for (int i = Map.transform.childCount - 1; i >= 0; i--)
            {
                Transform child = Map.transform.GetChild(i);
                if (child.name == HolderName)
                {
                    Object.DestroyImmediate(child.gameObject);
                }
            }
        }
        
        
        /// <summary>
        /// Applies the radiation and height map images to every renderer contained in the radiation GameObject.
        /// </summary>
        /// <param name="radiation"></param>
        /// <param name="radiationImage"></param>
        /// <param name="heightMapImage"></param>
        private static void SetRadiationImages(GameObject radiation, Texture2D radiationImage, Texture2D heightMapImage)
        {
            LOD[] lods = radiation.GetComponent<LODGroup>().GetLODs();
            foreach (LOD lod in lods)
            {
                foreach (Renderer ren in lod.renderers)
                {
                    Material material = ren.sharedMaterial;
                    material.SetTexture(RadiationMap, radiationImage);
                    material.SetTexture(Heightmap, heightMapImage);
                }
            }
        }

        
        /// <summary>
        /// Loads the first radiation image for the given map name.
        /// </summary>
        /// <param name="mapName">The name of the map.</param>
        /// <returns>The first radiation image as a Texture2D object.</returns>
        /// 
        private static Texture2D LoadFirstRadiationImage(string mapName) => ImageLoader.GetRadiationImages(mapName)[0];
    }
}
