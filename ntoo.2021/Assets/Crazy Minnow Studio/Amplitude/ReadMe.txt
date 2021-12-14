**************************
Amplitude

Version 1.2.0
**************************

Amplitude is a Unity3D asset that provides access to amplitude and frequency data on the WebGL platform. Unity uses the FMOD audio framework on all of their distribution platforms except WebGL, where they use a wrapper for the browser-based Web Audio API. Unfortunately, this means many of the more sophisticated tools for audio analysis simply are not available on the WebGL platform. Amplitude uses a native JavaScript library to communicate directly with the underlying web browser to access Web Audio API capabilities that have not been exposed by the Unity API wrapper.

Amplitude is easy to use, simply add the component, link your Unity AudioSource to the Amplitude AudioSource field, set the Data Type (Amplitude, Frequency), set the Sample Size as a multiple of 2 (32, 64, 128, 256, 512, 1024, 2048, 4096), an amount of boost if desired, and whether or not you want your output to use absolute values (amplitude only). Play your audio using the normal Unity AudioSource API by accessing the AudioSource directly or through Amplitude's AudioSource reference. While your audio is playing, Amplitude exposes a float array of the size you specified, and a float average amplitude/frequency. The values range from -1 to 1 (amplitude), or 0 to 1 for amplitude absolute values or frequency.

It comes with a clean and simple custom inspector, and of course we created a SALSA lip-sync add-on that allow SALSA to leverage Amplitude for WebGL-based character lip-sync. The SALSA add-on (AmplitudeSALSA) is available for download from our downloads page, free for SALSA customers. You must have SALSA, Amplitude, and the free AmplitudeSALSA add-on to use SALSA on the WebGL platform.

A sample scene is also included that displays the output from 64 samples, and the amplitude/frequency average, while playing a sample clip.

Crazy Minnow Studio, LLC
CrazyMinnowStudio.com

https://crazyminnowstudio.com/unity-3d/amplitude-webgl/


Package Contents
-------------------------
Crazy Minnow Studio/Amplitude
	Examples
		Audio
			100hz-21000hz_0.98-amplitude.ogg
				Sample clip with frequency tones from 100hz to 21,000hz at 0.98 amplitude.
		Prefabs
			panel_AmplitudeSamplesUI
				UI prefab for visualizing amplitude/frequency output in a WebGL build.
		Scenes
			AmplitudeSamples
				Sample scene and uses the panel_AmplitudeSamplesUI prefab.
		Scripts
			AmplitudeSamplesUI.cs
				Script for the panel_AmplitudeSamplesUI prefab.
			AmplitudeTester.cs
				Script for demonstrating variable access.
	Plugins
		Amplitude.cs
			The component that communicates with the native JavaScript library.
		AmplitudeLib.jslib
			Native Javascript library for accessing the Web Audio API directly in the web browser.
		Editor
			AmplitudeEditor.cs
				The component inspector.
	Resources
		Amplitude.png
			Logo
	ReadMe.txt
		This readme file.


Installation Instructions
-------------------------
1. Install Amplitude into your project.
	Select [Window] -> [Asset Store]
	Once the Asset Store window opens, select the download icon, and download and import [Amplitude].


Usage Instructions
-------------------------
1. Add an AudioSource to your scene. 
2. Add the Amplitude component.
3. Link your AudioSource to the Amplitude [Audio Source] field.
4. Set the Data Type (Amplitude or Frequency).
	Amplitude is useful to obtain the average power of your audio.
	Frequency is useful to obtain the power of your audio isolated by frequency.
5. Set the desired sample size (32, 64, 128, 256, 512, 1024, 2048).
	Frequency in the editor requires a minimum of 64 samples, but 32 works fine when building to WebGL.
6. Access the sample float array [Amplitude.sample], or the averaged value [Amplitude.average].
7. When in the editor, the Amplitude component seamlessly uses native Unity API to mimic the functionality Amplitude provides in WebGL.

Code Example
-------------------------
See [AmplitudeTester.cs] in Crazy Minnow Studio/Amplitude/Examples/Scripts