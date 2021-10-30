All TerraLand components can be loaded from Unity's top menu under Tools => TerraUnity => TerraLand


*************************************************************************************

IMPORTANT: After package importing is completed, go to Player tab in project settings in Unity and change the Scripting Runtime Version to .NET 4.x and
API Compatibility Level to .NET 4 to bypass all errors. When the errors are gone, you can now see the new Tools menu in Unity.
Ref: https://docs.unity3d.com/Manual/ScriptingRuntimeUpgrade.html

Note: FYI, in TerraLand menu under Tools, you can see "Streaming Map" which you can not access it directly and pressing on it will not show any windows.
This menu is there to be accessed from the codes in "RuntimeOffline" script to bring up a window to show global map of the sever and select the starting
point at level start.
More info: https://forum.unity.com/threads/released-terraland-3-streaming-huge-real-world-custom-terrains-for-open-world-environments.532304/page-7#post-4433686


Quick Preview: Simply bring up Downloader component from Tools => TerraUnity => TerraLand => Downloader and press "GENERATE TERRAIN" button at the bottom
of the UI and see the terrains creating in your scene. The UI settings are coming from a sample preset which you can change to your liking.


Have questions or want to contact with us? Join our Discord channel here: https://discord.gg/uZKmAy9


List of useful links for Help & Discussion


TerraUnity Website

TerraUnity website: http://terraunity.com/
TerraLand 3 product page: http://terraunity.com/product/terraland-3/
TerraLand Tournament page: http://terraunity.com/terraland-tournament/
TerraUnity Online Help: http://terraunity.com/onlinehelp/
TerraUnity Downloads: http://terraunity.com/downloads/
TerraLand Feature Request: http://terraunity.com/terraland/
TerraUnity Login/Register: http://terraunity.com/my-account/
TerraUnity Contact & Support: http://terraunity.com/contact/


Unity Forums

TerraLand 3 Official Discussion Thread: https://forum.unity.com/threads/released-terraland-3-streaming-huge-real-world-custom-terrains-for-open-world-environments.532304/
TerraLand 2 Official Discussion Thread: http://forum.unity3d.com/threads/terraland-2-high-quality-photo-realistic-terrains-from-real-world-gis-data.377858/
TerraLand 1 Official Discussion Thread: http://forum.unity3d.com/threads/terraland-photorealistic-terrains-from-heightmaps-satellite-images-released.179358/
TerraLand Tournament Official Discussion Thread: http://forum.unity3d.com/threads/terraland-tournament-car-racing-ultra-realistic-graphics-and-gameplay-free-to-play.392020/


Unity Asset Store

TerraLand 3 Asset Store page: https://www.assetstore.unity3d.com/#!/content/119097


Social Networks

Youtube: https://www.youtube.com/user/TerraUnity
Twitter: https://twitter.com/terraunity
FaceBook: https://www.facebook.com/TerraUnity


*************************************************************************************



SPECIAL NOTES




After package imporing, there will be a script called "TerrainNeighbors" under TerraLand => Scripts folder in your project.

Every time there are terrains generated in the scene, just drag & drop it on single terrain object or parent game object including multiple terrain chunks if not already existing.
So terrain neighbors and their basemap distance will be set automatically during runtime & also in Editor.



*************************************************************************************

There are already sample preset files for "TerraLand Downloader" component which you can access from Downloader's GUI top tab "Prseset Management".
Pressing "LOAD PRESET" will open a new window containing previously saved preset files. All user adjustments in Downloader interface can be saved using the above step and finally pressing "SAVE PRESET".



*************************************************************************************

"Lightmap Static" feature on terrains


In Unity 5 and later, whenever a terrain is generated, it will have a static tag for its game object. This causes Unity to calculate lighting information for generated terrain(s) so the computer slows down.
This option is automatically bypassed in generated terrains by TerraLand.

If you want to manually set the operation run/stop for terrains do the following:

You can optionally bypass the operation by selecting terrains and uncheck/deselect "Lightmap Static" from the "Static" dropdown list at Top-Right corner of Inspector GUI. And you can always revert it back.

Another option is to go to Unity's "Lighting" window, select "Lightmaps" tab and uncheck/deselect "Auto" option. So you can manually press button "Build" whenever you have done placing objects in scene.

"Lightmap Static": This indicates to Unity that the object's location is fixed and so it should participate in the GI. If an object is not marked as Lightmap Static then it can still be lit using Light Probes.
More Info: http://docs.unity3d.com/Manual/GlobalIllumination.html




*************************************************************************************


For any technical questions and support please go to the Contact section of our site here: http://terraunity.com/contact/ or email us at this address: info@terraunity.com
Also the best place for asking general questions are unity forums listed as above of this file.


TerraUnity Team

www.terraunity.com
info@terraunity.com

