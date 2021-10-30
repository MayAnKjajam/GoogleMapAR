Here is the demo scene which shows how you can do Geo-location & Geo-referencing operations on created terrains in
TerraLand without leaving Unity.

The scene contains 2 layers of terrains, one covering a 12km2 area and the other one a 500km2 area. This is to show
the precision of Geo_location algorithms used to set position of the transforms.

Note that these 2 layers of terrains have been placed and positioned automatically by TerraLand Downloader and the
expression "Automatically Centered" in their names suggest that they are properly offset to maintain the world center
and sync with each other no matter how large the area is. To read further on how the implementation took place see
here: https://forum.unity.com/threads/released-terraland-3-streaming-huge-real-world-custom-terrains-for-open-world-environments.532304/page-5#post-4133215

Also the precision of Geo-Location algorithm is being shown in 2 different approaches (LatLon2UnityMercator & LatLon2Unity)
to compare the produced errors. LatLon2UnityMercator uses the precise Geo-Location algorithm for objects in Unity based on
Mercator projection. To read further on how the implementation took place see here:
https://forum.unity.com/threads/released-terraland-3-streaming-huge-real-world-custom-terrains-for-open-world-environments.532304/page-5#post-4123141


For more information on the implementation, refer to the following links:

Video: https://www.youtube.com/watch?v=hFiYSkGso70


http://terraunity.com/geo-location-geo-referencing-in-terraland/
https://forum.unity.com/threads/terraland-2-high-quality-photo-realistic-terrains-from-real-world-gis-data.377858/page-6#post-3084315

