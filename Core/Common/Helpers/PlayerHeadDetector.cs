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

        /// <summary>
        /// Find the best attachment point specifically for headphones
        /// </summary>
        public static Transform FindHeadphoneAttachmentPoint(Il2CppScheduleOne.PlayerScripts.Player player)
        {
            if (player == null)
            {
                LoggingSystem.Error("Player is null - cannot find headphone attachment point", "PlayerHeadDetector");
                return null;
            }

            try
            {
                LoggingSystem.Debug("Searching for headphone-specific attachment point", "PlayerHeadDetector");

                // Method 1: Try to get avatar head bone (most reliable)
                var avatar = player.Avatar;
                if (avatar != null)
                {
                    var headBone = avatar.HeadBone;
                    if (headBone != null)
                    {
                        LoggingSystem.Info($"Found Avatar.HeadBone: {headBone.name}", "PlayerHeadDetector");
                        
                        // Check if the head bone has children that might be better for headphones
                        var earChild = FindEarChildBone(headBone);
                        if (earChild != null)
                        {
                            LoggingSystem.Info($"Found better ear attachment point: {earChild.name}", "PlayerHeadDetector");
                            return earChild;
                        }

                        // HeadBone is good enough - it's the actual head bone from the avatar system
                        LoggingSystem.Info($"Using Avatar.HeadBone for headphones: {headBone.name}", "PlayerHeadDetector");
                        return headBone;
                    }
                    else
                    {
                        LoggingSystem.Warning("Avatar found but HeadBone is null", "PlayerHeadDetector");
                    }
                }
                else
                {
                    LoggingSystem.Warning("Player avatar not found", "PlayerHeadDetector");
                }

                // Method 2: Search for head-like bones in the player hierarchy (fallback)
                var headTransform = FindHeadByName(player.transform);
                if (headTransform != null)
                {
                    LoggingSystem.Info($"Found head by name search: {headTransform.name}", "PlayerHeadDetector");
                    
                    // Check if this head has ear children
                    var earChild = FindEarChildBone(headTransform);
                    if (earChild != null)
                    {
                        LoggingSystem.Info($"Found ear child from head search: {earChild.name}", "PlayerHeadDetector");
                        return earChild;
                    }

                    return headTransform;
                }

                // Method 3: Try to find any head/ear specific bones by name
                var earAttachment = FindEarAttachmentPoint(player.transform);
                if (earAttachment != null)
                {
                    LoggingSystem.Info($"Found ear attachment point by name: {earAttachment.name}", "PlayerHeadDetector");
                    return earAttachment;
                }

                // Method 4: Final fallback to player transform
                LoggingSystem.Warning("No suitable headphone attachment point found, using player transform", "PlayerHeadDetector");
                return player.transform;
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error finding headphone attachment point: {ex.Message}", "PlayerHeadDetector");
                return player.transform; // Final fallback
            }
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Search for head bone by common naming patterns
        /// </summary>
        private static Transform FindHeadByName(Transform root)
        {
            // Common Unity/Mixamo head bone naming patterns
            string[] headNames = { 
                // Mixamo standard naming
                "mixamorig:Head", 
                // Standard Unity naming
                "Head", "head", 
                // Common variations
                "Head_01", "head_01", "Head1", "head1",
                // Humanoid rig standard
                "Bip01_Head", "bip01_head"
            };

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

        /// <summary>
        /// Find ear-specific attachment points
        /// </summary>
        private static Transform FindEarAttachmentPoint(Transform root)
        {
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

            foreach (var boneName in headEarNames)
            {
                var found = FindChildByName(root, boneName);
                if (found != null)
                {
                    LoggingSystem.Debug($"Found head/ear bone by name pattern: {boneName}", "PlayerHeadDetector");
                    return found;
                }
            }

            return null;
        }

        /// <summary>
        /// Find ear child bones from a head bone
        /// </summary>
        private static Transform FindEarChildBone(Transform headBone)
        {
            if (headBone == null) return null;

            LoggingSystem.Debug($"Searching for ear bones in HeadBone children. HeadBone: {headBone.name}, Child count: {headBone.childCount}", "PlayerHeadDetector");

            // Look for children with ear-like names
            for (int i = 0; i < headBone.childCount; i++)
            {
                var child = headBone.GetChild(i);
                var childName = child.name.ToLower();
                
                LoggingSystem.Debug($"  Child {i}: {child.name}", "PlayerHeadDetector");
                
                // Look for common ear bone patterns
                if (childName.Contains("ear") || 
                    childName.Contains("end") || 
                    childName.Contains("top"))
                {
                    LoggingSystem.Info($"Found ear child bone: {child.name}", "PlayerHeadDetector");
                    return child;
                }
            }

            // If no specific ear bones, look for the highest positioned child
            // (ears are typically higher on the head than other features)
            Transform highestChild = null;
            float highestY = float.MinValue;

            for (int i = 0; i < headBone.childCount; i++)
            {
                var child = headBone.GetChild(i);
                var worldPos = child.position;
                
                if (worldPos.y > highestY)
                {
                    highestY = worldPos.y;
                    highestChild = child;
                }
            }

            if (highestChild != null)
            {
                LoggingSystem.Debug($"Using highest positioned child as ear candidate: {highestChild.name} (Y: {highestY})", "PlayerHeadDetector");
                return highestChild;
            }

            return null;
        }

        /// <summary>
        /// Find child with partial name match
        /// </summary>
        private static Transform FindChildWithPartialName(Transform parent, string partialName)
        {
            if (parent.name.ToLower().Contains(partialName.ToLower()))
                return parent;

            for (int i = 0; i < parent.childCount; i++)
            {
                var result = FindChildWithPartialName(parent.GetChild(i), partialName);
                if (result != null)
                    return result;
            }

            return null;
        }

        #endregion
    }
} 