using UnityEngine;
using BackSpeakerMod.Core.System;
using System;

namespace BackSpeakerMod.Core.Features.Headphones.Attachment
{
    /// <summary>
    /// Detects and validates player head bone for attachment
    /// </summary>
    public static class PlayerHeadDetector
    {
        /// <summary>
        /// Find the best attachment point on player for headphones
        /// </summary>
        public static Transform FindAttachmentPoint(Il2CppScheduleOne.PlayerScripts.Player player)
        {
            if (player == null)
            {
                LoggingSystem.Error("Player is null - cannot find attachment point", "Headphones");
                return null;
            }

            try
            {
                // Method 1: Try to get avatar head bone
                var avatar = player.Avatar;
                if (avatar != null)
                {
                    var headBone = avatar.HeadBone;
                    if (headBone != null)
                    {
                        LoggingSystem.Debug($"Found head bone: {headBone.name}", "Headphones");
                        return headBone;
                    }
                    
                    LoggingSystem.Warning("Avatar found but no head bone", "Headphones");
                }
                else
                {
                    LoggingSystem.Warning("Player avatar not found", "Headphones");
                }

                // Method 2: Try to find head bone by name in hierarchy
                var headTransform = FindHeadByName(player.transform);
                if (headTransform != null)
                {
                    LoggingSystem.Debug($"Found head by name search: {headTransform.name}", "Headphones");
                    return headTransform;
                }

                // Method 3: Fallback to player transform
                LoggingSystem.Warning("No head bone found, using player transform as fallback", "Headphones");
                return player.transform;
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error finding attachment point: {ex.Message}", "Headphones");
                return player.transform; // Final fallback
            }
        }

        /// <summary>
        /// Search for head bone by common naming patterns
        /// </summary>
        private static Transform FindHeadByName(Transform root)
        {
            string[] headNames = { "Head", "head", "HEAD", "Bip01_Head", "mixamorig:Head" };

            foreach (var headName in headNames)
            {
                var found = FindChildByName(root, headName);
                if (found != null)
                {
                    LoggingSystem.Debug($"Found head bone by name pattern: {headName}", "Headphones");
                    return found;
                }
            }

            return null;
        }

        /// <summary>
        /// Recursively search for child by name
        /// </summary>
        private static Transform FindChildByName(Transform parent, string name)
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
        /// Validate that attachment point is suitable
        /// </summary>
        public static bool ValidateAttachmentPoint(Transform attachmentPoint)
        {
            if (attachmentPoint == null)
                return false;

            // Check if transform is active and valid
            if (attachmentPoint.gameObject == null || !attachmentPoint.gameObject.activeInHierarchy)
            {
                LoggingSystem.Warning("Attachment point is not active in hierarchy", "Headphones");
                return false;
            }

            return true;
        }
    }
} 