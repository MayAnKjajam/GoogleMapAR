TERRALAND OFFLINE STREAMING


Videos:
https://www.youtube.com/watch?v=GmwMbIuFq5U
https://www.youtube.com/watch?v=jOkJOjjmCxE
https://www.youtube.com/watch?v=CXFjf9MQkvM

Discussion Thread: https://forum.unity.com/threads/released-terraland-3-streaming-huge-real-world-custom-terrains-for-open-world-environments.532304/


The steps to create a geo-server in TerraLand Downloader for later offline streaming is so simple.
The created server is a directory on your hard drive containing of 2 main folders one for the "heightmaps" and another one for
the "satellite images", "normal textures" or "splatmaps" to be used for streaming by the Runtime API in TerraLand.

You define an area in TerraLand Downloader but this time the area can cover a very large region such as a city or a country
without being worried about huge resolution values for the total heightmap and imagery.
All needed tiles of obtaining elevation & imagery will be downloaded and saved on your hard drive with desired resolution. 

A section has been added to the Downloader component at top of the UI entitled "DYNAMIC WORLD" which is specialized for Local
Server generating which defines number of total tiles for specified region and each elevation/image tile's resolution as cache.

You can always switch between "DYNAMIC WORLD" & "STATIC WORLD" modes based on your scene setup.

The "STATIC WORLD" mode will download data for selected region and generate terrain(s) in scene to be the game world as a
static environment while in "DYNAMIC WORLD" mode TerraLand will obtain needed data for user specified region, save it somewhere
on user's computer local hard drive as a cache-server and then uses "RuntimeOffline" script to generate terrains in runtime
dynamically.

The generated geo-server has a very simple structure which contains an "Elevation" folder including heightmap tiles and a
folder titled "Imagery" including image tiles.

The Offline Runtime API connects to this local server and loads and applies needed data on terrains around the player in the
scene dynamically as he travels. So users experience a seamless streaming of data and terrain generation just as our previous
demo of WorldExplorer but this time without needing an internet connection and in a more managed system.

Note: In all streaming demos in the project, there's a script named "ExtendedFlyCam.cs" on camera which handles scene navigation
in play mode. The movement speeds are set to be practical based on world scale ("Area Size" & "Size Exaggeration" parameters in
"RuntimeOffline" script) as Runtime Streaming System needs to load & update tiles on each North, South, East or and/or West
direction movements. So giving higher speed to the camera will break the system and update tile positions before finishing
needed operations for tile generation. This behavior will be improved in future updates in order to reduce or entirely remove
this limitation as the highest demanded feature.



SETUP

When servers are created, simply put the generated geo-server containing elevation & imagery tiles in the root folder of the
build or any other places you wish.

If you put the generated server in the root folder of the project (next to the Assets folder), you have to enable the
checkbox "Project Root Path" in "RuntimeOffline" script and type the server's directory name in "Data Base Path" field.

If you do not want to put the server in the root folder of the project/build, you have to disable the checkbox "Project Root Path"
in "RuntimeOffline" script and type down the full path to the main directory of the server in "Data Base Path" field.

It is recommended to put created servers in the root of the project instead of putting them somewhere else as you can also put
servers in root folder of the build after building the project and TerraLand detects server automatically which also makes
projects/builds more organized.

Provided demos' settings in this project assumes that you put cache servers in the root of the project/build.

You can have as many folders as servers in the project/build root directory but TerraLand picks up the one with the specified name
in "Data Base Path" field of the "RuntimeOffline" script in scene.


SERVER STRUCTURE

The structure of the local server is so simplistic. It contains 3 main folders of "Elevation", "Imagery" & "Info". These sub-folders
do not have to be renamed or relocated and any attempts to rename/relocate these folders will break the runtime world generation.

The Elevation folder contains downloaded and generated heightmap tiles and the Imagery folder contains downloaded satellite images
as terrain textures.


As all the information for setting up the server is exposed here and it's configurable from outside TerraLand, so you can simply
generate your own datasets and geo-servers from your own custom elevation/imagery data files created in any external programs and put
them in project/build so that TerraLand will generate worlds from them.

Currently the heightmap format is limited to .raw files and image format is limited to .jpg.



How It Works

TerraLand's Offline Streaming feature is a real-time dynamic system which streams elevation/imagery data on demand. So as the player
travels around the world, the system updates surrounding terrains and feed needed tiles from the assigned server and converts them to
Unity terrain assets.

So imagine the Active Area around the player which terrains are generated as a small square (active terrains around player) which is
in another bigger square (the whole world from dataset) as the smaller square updates its position in world's bounds while the player
travels.


https://forum.unity.com/threads/terraland-2-high-quality-photo-realistic-terrains-from-real-world-gis-data.377858/page-8#post-3339237
https://forum.unity.com/threads/terraland-2-high-quality-photo-realistic-terrains-from-real-world-gis-data.377858/page-8#post-3388627

