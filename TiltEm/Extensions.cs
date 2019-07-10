using UnityEngine;

namespace TiltEm
{
    public static class Extensions
    {
        /// <summary>
        /// This method is almost the same as vessel.GoOnRails() but instead it doesn't neutralize the controls
        /// </summary>
        /// <param name="vessel"></param>
        public static void CustomGoOnRails(this Vessel vessel)
        {
            if (vessel.packed)
            {
                return;
            }
            vessel.precalc.GoOnRails();

            // ReSharper disable once ForCanBeConvertedToForeach
            for(var i = 0; i < vessel.vesselModules.Count; i++)
            {
                vessel.vesselModules[i].OnGoOnRails();
            }

            GameEvents.onVesselGoOnRails.Fire(vessel);

            //vessel.ctrlState.Neutralize();

            MonoBehaviour.print(string.Concat("Packing ", vessel.vesselName, " for orbit"));
            if (vessel.loaded)
            {
                // ReSharper disable once ForCanBeConvertedToForeach
                for (var i = 0; i < vessel.parts.Count; i++)
                {
                    vessel.parts[i].Pack();
                }
            }

            vessel.packed = true;
            vessel.orbitDriver.SetOrbitMode(OrbitDriver.UpdateMode.UPDATE);
            vessel.SendMessage("OnVesselPack", SendMessageOptions.DontRequireReceiver);
        }
    }
}
