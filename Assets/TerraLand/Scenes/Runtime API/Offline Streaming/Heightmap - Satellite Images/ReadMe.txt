Download sample databases from here:

128px Heightmaps & 1024px Satellite Images per tile (May be laggy on low-end machines due to 1024px texture resolution):
http://terraunity.com/freedownload/TerraLandStreaming_ZionPark.zip

1- Extract each downloaded zip file in the root directory of the project next to the "Assets" folder
2- Find the gameobject with "RuntimeOffline" script on it in scene hierarchy
3- Enable "Project Root Path" checkbox in "RuntimeOffline" script
3- In the text field of "Data Base Path", type the name of extracted folder in step 1 and run the scene


Videos:
https://www.youtube.com/watch?v=GmwMbIuFq5U
https://www.youtube.com/watch?v=jOkJOjjmCxE
https://www.youtube.com/watch?v=CXFjf9MQkvM

Discussion Thread: https://forum.unity.com/threads/released-terraland-3-streaming-huge-real-world-custom-terrains-for-open-world-environments.532304/


Important Information:

There are 2 parameters of "Area Size" & "Size Exaggeration" in the "RuntimeOffline" script in the scene. The "Area Size"
value defines the total area size of the terrain area in kilometers and the "Size Exaggeration" defines the multiplier
value if you want to extend the area in width & length.

So the formula is:
"AreaSize (e.g. 40)" x "SizeExaggeration (e.g. 10)" = TotalAreaSize 400,000 units in Unity.

As any distances above 100,000 from origin is considered huge for 3D engines with single-precision floating-point
calculations instead of double-precision, there is a script named "FloatingOriginAdvanced" on the main camera to handle
this limitation. The distance from origin which will offset scene elements is defined by the "Distance" parameter. To get
more info on this behavior refer to the following links:
https://forum.unity.com/threads/terraland-2-high-quality-photo-realistic-terrains-from-real-world-gis-data.377858/page-8#post-3391209
https://forum.unity.com/threads/terraland-2-high-quality-photo-realistic-terrains-from-real-world-gis-data.377858/page-7#post-3253566
https://forum.unity.com/threads/terraland-2-high-quality-photo-realistic-terrains-from-real-world-gis-data.377858/page-6#post-3081781


There is also another parameter "Elevation Exaggeration" which defines the vertical factor of the heights on surface. The
value of 1 means original heights from the heightmap is going to be taken and any other numbers multiplies with each
pixel's height.



Links:
https://forum.unity.com/threads/terraland-2-high-quality-photo-realistic-terrains-from-real-world-gis-data.377858/page-8#post-3383823
https://forum.unity.com/threads/terraland-2-high-quality-photo-realistic-terrains-from-real-world-gis-data.377858/page-8#post-3388627
https://forum.unity.com/threads/terraland-2-high-quality-photo-realistic-terrains-from-real-world-gis-data.377858/page-9#post-3414593

