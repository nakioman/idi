IDI Speech Framework
====================

This is a C# project with speech support using both your computer microphone or the Kinect sensor

Requirements
------------

To build this project you will need:

* [Kinect SDK] (http://www.microsoft.com/en-us/kinectforwindows/develop/overview.aspx)
* [NuGet package manager] (http://nuget.org/)
* Update the NuGet references on the project
* Visual Studio 2010
* [Microsoft Speech SDK x86] (http://www.microsoft.com/en-us/download/details.aspx?id=27226)

Included plugins
----------------

Currently there are to proof of concept plugins implemented

*Weather, that will give you the weather for a City (implemented for Spanish Speech recognition)
*Music, that will play songs by artist, or albume (also implemented for Spanish Speech recognition)

Additional notes
----------------

* If you need more speech recognition languages or text to speech you can find more in the following link [Microsoft Speech Languages](http://www.microsoft.com/en-us/download/details.aspx?id=27224)

* Plugins can be created using by Extending the PluginBase class

* For information on how to create grammar files, you can take a look at:
	* [tag Element (Microsoft.Speech)] (http://msdn.microsoft.com/en-us/library/hh378486.aspx)
	* [Speech Recognition Grammar Specification Version 1.0] (http://www.w3.org/TR/speech-grammar/)
	* the tag-format should be "semantics/1.0"