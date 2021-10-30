This scene represents output results from the components of "TerraLand Terrain" & "Maps Maker" in TerraLand

"TerraLand Terrain" can be loaded from Unity's top menu => Tools => TerraUnity => TerraLand => Terrain
"Maps Maker" can be loaded from Unity's top menu => Tools => TerraUnity => TerraLand => Maps Maker

"TerraLand Terrain" consists of multiple processors which mostly perform on terrain heights and surfaces plus texturing features.

"Maps Maker" consists of 3 processors operating on satellite images as terrain textures.
The 3 processors are "Shadow Remover", "Colormap Generator" & "Landcover(Splatmap) Genrator".

Ref. http://terraunity.com/terraland-terrain-component/
https://forum.unity.com/threads/terraland-2-high-quality-photo-realistic-terrains-from-real-world-gis-data.377858/page-7#post-3164935


Videos:
https://www.youtube.com/watch?v=9nVUu8lGK_Y
https://www.youtube.com/watch?v=V5zfio-42ZM
https://www.youtube.com/watch?v=67QVGLkwmyY

Discussion Thread: https://forum.unity.com/threads/released-terraland-3-streaming-huge-real-world-custom-terrains-for-open-world-environments.532304/


Here is the list of applied processes on terrain surfaces in the scene from left to right North-sided


. Decreased Heightmap Resolution to Half - 256 Resolution
As extra pixels were available
Used HEIGHTMAP RESIZER processor

. Split surface/heightmap to a grid of 4x4 - 16 Tiles
As more controls over different regions was needed
Used TERRAIN SPLITTER processor
http://terraunity.com/terraland-multi-tiles-terrain-generation/
https://forum.unity.com/threads/terraland-2-high-quality-photo-realistic-terrains-from-real-world-gis-data.377858/#post-2591030

. Smoothened surface/heightmap by 2 iterations
As there were artifacts such as bandings, terraces and jaggies 
Used SMOOTHEN TERRAIN HEIGHTS processor
http://terraunity.com/smoothen-operation-on-surfaces-in-terraland-terrain/
https://forum.unity.com/threads/terraland-2-high-quality-photo-realistic-terrains-from-real-world-gis-data.377858/page-5#post-2878638

. Textured terrain tiles out of downloaded Satellite Images from TerraLand
Splatmaps has been automatically generated for textures
Used IMAGE TILER processor
http://terraunity.com/terralands-image-tiler-definition/
https://forum.unity.com/threads/terraland-2-high-quality-photo-realistic-terrains-from-real-world-gis-data.377858/#post-2485204

. Removed Shadows from Satellite Images
As for proper lighting/shadowing, de-lightened Textures are needed
Used SHADOW REMOVER processor
http://terraunity.com/terraland-maps-maker-shadow-removal-colormap-landcover-splatmap-from-images/
https://forum.unity.com/threads/terraland-2-high-quality-photo-realistic-terrains-from-real-world-gis-data.377858/page-5#post-2956710

. Generated RGBA Splatmaps out of input Satellite Images
Automatically generated splats based on the 4 most dominant colors in images
Used LANDCOVER GENERATOR processor
http://terraunity.com/terraland-maps-maker-shadow-removal-colormap-landcover-splatmap-from-images/
https://forum.unity.com/threads/terraland-2-high-quality-photo-realistic-terrains-from-real-world-gis-data.377858/page-5#post-2956710

. Generated Mesh from Terrains
Useful when meshes are needed instead of height-mapped terrain objects
Used TERRAIN TO MESH processor
http://terraunity.com/terrain-to-mesh-conversion-in-terraland/
https://forum.unity.com/threads/terraland-2-high-quality-photo-realistic-terrains-from-real-world-gis-data.377858/page-5#post-2971528

. Generated Terrains from Mesh
Useful when input surface is a 3D model/mesh but terrain features are needed
Used MESH TO TERRAIN processor


Note: There are other processors available in "TerraLand Terrain" & "Maps Maker" components which are useful in different
case scenarios other than the ones shown in this demo. Check them out and play with the settings to discover possibilities.

