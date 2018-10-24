using Harmony;
using System.Reflection;
using UnityEngine;

namespace TiltEm
{
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class TiltEm : MonoBehaviour
    {
        public static HarmonyInstance HarmonyInstance = HarmonyInstance.Create("TiltEm");

        public void Awake()
        {
            DontDestroyOnLoad(this);
            HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
