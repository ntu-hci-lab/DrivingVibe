using System.Runtime.InteropServices;
using UnityEngine;

namespace AirDriVR
{
    public class GameListener : MonoBehaviour
    {

        public float horizontalMultipler = 1f;
        public float longitudinalMultiplier = 1f;
        
        public static T FromBytes<T>(byte[] arr) where T : new()
        {
            var size = Marshal.SizeOf<T>();
            var ptr = Marshal.AllocHGlobal(size);

            Marshal.Copy(arr, 0, ptr, Mathf.Min(size, arr.Length));

            var str = Marshal.PtrToStructure<T>(ptr);
            Marshal.FreeHGlobal(ptr);

            return str;
        }
    }
}
