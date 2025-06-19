using System;
using UnityEngine;

namespace BackSpeakerMod.NewBackend.Utils
{
    public static class FindHeadAttachmentPoint
    {
        /// <summary>
        /// Recursively search for child by name
        /// </summary>
        private static Transform? FindChildByName(Transform parent, string name)
        {
            if (parent.name.Equals(name, StringComparison.OrdinalIgnoreCase))
                return parent;

            for (int i = 0; i < parent.childCount; i++)
            {
                var result = FindChildByName(parent.GetChild(i), name);
                if (result != null)
                    return result;
            }

            return null;
        }

        /// <summary>
        /// Find ear-specific attachment points
        /// </summary>
        public static Transform? FindEarAttachmentPoint(Transform? root)
        {
            if (root == null)
            {
                NewLoggingSystem.Error("Root is null", "PlayerHeadDetector");
                return null;
            }

            // Common Unity/Mixamo bone naming patterns for heads and ears
            string[] headEarNames = { 
                // Head end points (common in Mixamo rigs)
                "mixamorig:HeadTop_End", "HeadTop_End", "Head_End", 
                // Actual ear bones (less common but possible)
                "mixamorig:LeftEar", "mixamorig:RightEar", 
                "LeftEar", "RightEar", "Ear_L", "Ear_R",
                // Head variations that might be closer to ears
                "mixamorig:Head1", "Head1", "head1"
            };
            // Choose just mixamorig:HeadTop_End
            string headTopEnd = "mixamorig:HeadTop_End";

            var found = FindChildByName(root!, headTopEnd);
                if (found != null)
                {
                    NewLoggingSystem.Debug($"Found head/ear bone by name pattern: {headTopEnd}", "PlayerHeadDetector");
                    return found;
                }

            return null;
        }
    }
}