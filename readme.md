# DrivingVibe Unity Project

Unity Version: 2019.4.10f1

### How to import

1. Clone the project and open it with Unity.


### Requirement

1. Install the **ViGem Bus Driver** in the computer (Just the driver itself) https://github.com/ViGEm/ViGEm.NET
2. Install NuGetForUnity package into Unity. https://github.com/GlitchEnzo/NuGetForUnity
3. Get **Nefarius.Vigem.Client** from Nuget.


### How To Use

1. Get the device ready. (Connect to the power source.)
2. Make your computer connect to the wifi hotspot of the device.
3. Open "Refactor" scene in the Unity project.
4. Open Assetto Corsa game and get into play mode (where you can see the car)

#### 3D inertia-based

5. Select "Inertia_based" in the inspector of Scene->Canvas->Script->PatternBinder
6. Plug in the gamepad controller.
7. Enter Play Mode.

##### Mirroring

5. Select "Mirroring" in the inspector of Scene->Canvas->Script->PatternBinder
6. **Enter Play Mode first** and then plug in the gamepad controller.
7. Click the botton "GetPhysicalController" in the inspector of Scene->Canvas->Script->Mirroring

