# AR-Foundation-Framework
For my university thesis, creating an AR App with AR Foundation framework in Unity with ARCore XR Plug-in installed and activated; it also works for iPhone, just install and activate the plugin

# How I Did It
## Basic Initial Set-up to start
1. Create a 3D empty project in Unity Hub
2. Window > Package Management > Package Manager and then go in Unity Registry and install AR Foundation and Google ARCore.
3. Edit > Project Settings > XR Plug In Management and enable XR Simulator in Windows and in Android ARCore
4. Edit > Project Settings > Player in Android delete Vulkan because maybe cause bug in the mobile phone, like black screen.
5. In the project directory at the bottom, Assets > Settings and add in Mobile Renderer and PC Renderer the comonent "AR Background Renderer Feature"
6. Delete the current main camera and create with right click > XR > AR Session and XR Origin
7. Window > XR > AR Foundation > XR Enviroment to create and visualize a simulation of application
8. Then add what you want in XR Origin (ARManager,...) and if you want add script to manage the behaviour of managers
9. Create the various build that you want, File > Build Profiles for the various project builds

## Set Up Smartphone and connection to PC

1. You have to need installed and updated the "Google Play Services for AR", then you active the "developer account" by clicking ten or more times on the number of build in your system settings in your smartphone, then you have to enable de Debug USB.
2. then you connect your smartphone to your pc and enable the "file transmission" connection, then go to File > Build Profiles and in device click refresh until you see your device, then you click build o build and run, then import the apk file generated in an directory on your smartphone 
