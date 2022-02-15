using System.Runtime.InteropServices;

namespace AirDriVR
{
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct ACHandshaker{
        [MarshalAs(UnmanagedType.U4)]
        public uint identifier;
        [MarshalAs(UnmanagedType.U4)]
        public uint version;
        [MarshalAs(UnmanagedType.U4)]
        public uint operationId;

        public ACHandshaker(uint identifier, uint version, uint operationId)
        {
            this.identifier = identifier;
            this.version = version;
            this.operationId = operationId;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
    public struct ACHandshackerResponse
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
        public string carName;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
        public string driverName;

        [MarshalAs(UnmanagedType.U4)]
        public uint identifier; // Status code from the server, currently just '4242' to see that it works.

        [MarshalAs(UnmanagedType.U4)]
        public uint version; // Server version, not yet supported.

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
        public string trackName;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
        public string trackConfig;

        public override string ToString()
        { 
            return $"CarName: {carName}, DriverName: {driverName}, Identifier: {identifier}, Version: {version}, TrackName: {trackName}, TrackConfig: {trackConfig}";
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct ACCarInfo
    {          
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 2)]
        public string identifier;
        [MarshalAs(UnmanagedType.U4)]
        public int size;

        [MarshalAs(UnmanagedType.R4)]
        public float speed_Kmh;
        [MarshalAs(UnmanagedType.R4)]
        public float speed_Mph;
        [MarshalAs(UnmanagedType.R4)]
        public float speed_Ms;
    
        [MarshalAs(UnmanagedType.U1)]
        public bool isAbsEnabled;
        [MarshalAs(UnmanagedType.U1)]
        public bool isAbsInAction;
        [MarshalAs(UnmanagedType.U1)]
        public bool isTcInAction;
        [MarshalAs(UnmanagedType.U1)]
        public bool isTcEnabled;
        [MarshalAs(UnmanagedType.U1)]
        public bool isInPit;
        [MarshalAs(UnmanagedType.U1)]
        public bool isEngineLimiterOn;
    
        [MarshalAs(UnmanagedType.R4)]
        public float accG_horizontal;
        [MarshalAs(UnmanagedType.R4)]
        public float accG_vertical;
        [MarshalAs(UnmanagedType.R4)]
        public float accG_frontal;

        [MarshalAs(UnmanagedType.U4)]
        public int lapTime;
        [MarshalAs(UnmanagedType.U4)]
        public int lastLap;
        [MarshalAs(UnmanagedType.U4)]
        public int bestLap;
        [MarshalAs(UnmanagedType.U4)]
        public int lapCount;

        [MarshalAs(UnmanagedType.R4)]
        public float gas;
        [MarshalAs(UnmanagedType.R4)]
        public float brake;
        [MarshalAs(UnmanagedType.R4)]
        public float clutch;
        [MarshalAs(UnmanagedType.R4)]
        public float engineRPM;
        [MarshalAs(UnmanagedType.R4)]
        public float steer;
        [MarshalAs(UnmanagedType.U4)]
        public int gear;
        [MarshalAs(UnmanagedType.R4)]
        public float cgHeight;

        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.R4, SizeConst = 4)]
        public float[] wheelAngularSpeed;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.R4, SizeConst = 4)]
        public float[] slipAngle;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.R4, SizeConst = 4)]
        public float[] slipAngle_ContactPatch;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.R4, SizeConst = 4)]
        public float[] slipRatio;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.R4, SizeConst = 4)]
        public float[] tyreSlip;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.R4, SizeConst = 4)]
        public float[] ndSlip;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.R4, SizeConst = 4)]
        public float[] load;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.R4, SizeConst = 4)]
        public float[] Dy;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.R4, SizeConst = 4)]
        public float[] Mz;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.R4, SizeConst = 4)]
        public float[] tyreDirtyLevel;
    
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.R4, SizeConst = 4)]
        public float[] camberRAD;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.R4, SizeConst = 4)]
        public float[] tyreRadius;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.R4, SizeConst = 4)]
        public float[] tyreLoadedRadius;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.R4, SizeConst = 4)]
        public float[] suspensionHeight;

        [MarshalAs(UnmanagedType.R4)]
        public float carPositionNormalized;
        [MarshalAs(UnmanagedType.R4)]
        public float carSlope;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.R4, SizeConst = 3)]
        public float[] carCoordinates;

        public override string ToString()
        {
            return $"SpeedKmh: {speed_Kmh},\n SpeedMs: {speed_Ms},\n AccGVertical: {accG_vertical},\n AccGHorizontal: {accG_horizontal},\n AccGFrontal: {accG_frontal},\n LapTime: {lapTime},\n LastLap: {lastLap},\n BestLap: {bestLap},\n LapCount: {lapCount},\n Gas: {gas},\n Brake: {brake},\n Clutch: {clutch},\n EngineRpm: {engineRPM},\n Steer: {steer},\n Gear: {gear}";
        }
    }
}