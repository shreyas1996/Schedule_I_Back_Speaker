using UnityEngine;
using BackSpeakerMod.Core.System;
using BackSpeakerMod.Core.Common.Managers;
using System;

namespace BackSpeakerMod.Core.Common.Helpers
{
    /// <summary>
    /// Unified player head detector for headphones, spheres, and other attachments
    /// </summary>
    public class PlayerHeadDetector
    {
        #region Instance Methods (for spheres and other features that need instance-based usage)
        
        /// <summary>
        /// Find the current player's head transform for attachment
        /// </summary>
        public Transform FindPlayerHead()
        {
            try
            {
                var player = PlayerManager.CurrentPlayer;
                if (player == null)
                {
                    LoggingSystem.Error("No current player found for head detection", "PlayerHeadDetector");
                    return null;
                }

                return FindPlayerHead(player);
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Exception in FindPlayerHead: {ex.Message}", "PlayerHeadDetector");
                return null;
            }
        }

        /// <summary>
        /// Find the head transform for a specific player (instance method)
        /// </summary>
        public Transform FindPlayerHead(Il2CppScheduleOne.PlayerScripts.Player player)
        {
            return FindAttachmentPoint(player);
        }

        /// <summary>
        /// Validate that attachment point is suitable
        /// </summary>
        public bool ValidateAttachmentPoint(Transform attachmentPoint)
        {
            return IsValidAttachmentPoint(attachmentPoint);
        }

        #endregion

        #region Static Methods (for headphones and backward compatibility)

        /// <summary>
        /// Find the best attachment point on player (static method for backward compatibility)
        /// </summary>
        public static Transform FindAttachmentPoint(Il2CppScheduleOne.PlayerScripts.Player player)
        {
            if (player == null)
            {
                LoggingSystem.Error("Player is null - cannot find attachment point", "PlayerHeadDetector");
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
                        LoggingSystem.Debug($"Found avatar head bone: {headBone.name}", "PlayerHeadDetector");
                        return headBone;
                    }
                    
                    LoggingSystem.Warning("Avatar found but no head bone", "PlayerHeadDetector");
                }
                else
                {
                    LoggingSystem.Warning("Player avatar not found", "PlayerHeadDetector");
                }

                // Method 2: Try to find head bone by name in hierarchy
                var headTransform = FindHeadByName(player.transform);
                if (headTransform != null)
                {
                    LoggingSystem.Debug($"Found head by name search: {headTransform.name}", "PlayerHeadDetector");
                    return headTransform;
                }

                // Method 3: Fallback to player transform
                LoggingSystem.Warning("No head bone found, using player transform as fallback", "PlayerHeadDetector");
                return player.transform;
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error finding attachment point: {ex.Message}", "PlayerHeadDetector");
                return player.transform; // Final fallback
            }
        }

        /// <summary>
        /// Find the current player's head (static convenience method)
        /// </summary>
        public static Transform FindCurrentPlayerHead()
        {
            var player = PlayerManager.CurrentPlayer;
            return FindAttachmentPoint(player);
        }

        /// <summary>
        /// Validate that attachment point is suitable (static method)
        /// </summary>
        public static bool IsValidAttachmentPoint(Transform attachmentPoint)
        {
            if (attachmentPoint == null)
                return false;

            // Check if transform is active and valid
            if (attachmentPoint.gameObject == null || !attachmentPoint.gameObject.activeInHierarchy)
            {
                LoggingSystem.Warning("Attachment point is not active in hierarchy", "PlayerHeadDetector");
                return false;
            }

            return true;
        }

        #endregion

        #region Private Helper Methods

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
                    LoggingSystem.Debug($"Found head bone by name pattern: {headName}", "PlayerHeadDetector");
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

        #endregion
    }
} 