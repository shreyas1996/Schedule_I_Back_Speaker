using UnityEngine;
using BackSpeakerMod.Core.System;
using BackSpeakerMod.Core.Features.Player;
using BackSpeakerMod.Configuration;
using System;
using BackSpeakerMod.S1Wrapper.Interfaces;

namespace BackSpeakerMod.Core.Features.Player
{
    /// <summary>
    /// Unified player head detector for headphones, spheres, and other attachments
    /// </summary>
    public class PlayerHeadDetector
    {
        #region Instance Methods (for spheres and other features that need instance-based usage)

        /// <summary>
        /// Find the best attachment point specifically for headphones
        /// </summary>
        public static Transform? FindHeadphoneAttachmentPoint(IPlayer player)
        {
            if (player == null)
            {
                LoggingSystem.Error("Player is null - cannot find headphone attachment point", "PlayerHeadDetector");
                return null;
            }

            try
            {
                LoggingSystem.Debug("Searching for headphone-specific attachment point", "PlayerHeadDetector");

                // RUNTIME BONE DISCOVERY - Log all available bones first
                if (FeatureFlags.Headphones.EnableBoneDiscovery)
                {
                    DiscoverAndLogAllBones(player);
                }

                // Method 1: Try to get avatar head bone (most reliable)
                var avatar = player.Avatar;
                if (avatar != null)
                {
                    var headBone = avatar.HeadBone;
                    if (headBone != null)
                    {
                        LoggingSystem.Debug($"Found Avatar.HeadBone: {headBone.name}", "PlayerHeadDetector");
                        
                        // Check if the head bone has children that might be better for headphones
                        var earChild = FindEarChildBone(headBone);
                        if (earChild != null)
                        {
                            LoggingSystem.Debug($"Found better ear attachment point: {earChild.name}", "PlayerHeadDetector");
                            return earChild;
                        }

                        // HeadBone is good enough - it's the actual head bone from the avatar system
                        LoggingSystem.Debug($"Using Avatar.HeadBone for headphones: {headBone.name}", "PlayerHeadDetector");
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
                var headTransform = FindHeadByName(player.Transform);
                if (headTransform != null)
                {
                    LoggingSystem.Debug($"Found head by name search: {headTransform.name}", "PlayerHeadDetector");
                    
                    // Check if this head has ear children
                    var earChild = FindEarChildBone(headTransform);
                    if (earChild != null)
                    {
                        LoggingSystem.Debug($"Found ear child from head search: {earChild.name}", "PlayerHeadDetector");
                        return earChild;
                    }

                    return headTransform;
                }

                // Method 3: Try to find any head/ear specific bones by name
                var earAttachment = FindEarAttachmentPoint(player.Transform);
                if (earAttachment != null)
                {
                    LoggingSystem.Debug($"Found ear attachment point by name: {earAttachment.name}", "PlayerHeadDetector");
                    return earAttachment;
                }

                // Method 4: Final fallback to player transform
                LoggingSystem.Warning("No suitable headphone attachment point found, using player transform", "PlayerHeadDetector");
                return player.Transform;
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error finding headphone attachment point: {ex.Message}", "PlayerHeadDetector");
                return player.Transform; // Final fallback
            }
        }

        /// <summary>
        /// Discover and log all bones in the player hierarchy for debugging
        /// </summary>
        private static void DiscoverAndLogAllBones(IPlayer player)
        {
            try
            {
                LoggingSystem.Debug("=== RUNTIME BONE DISCOVERY ===", "PlayerHeadDetector");
                LoggingSystem.Debug($"Player: {player.Name}", "PlayerHeadDetector");

                // Check Avatar bones first
                var avatar = player.Avatar;
                if (avatar != null)
                {
                    LoggingSystem.Debug("Avatar bone structure:", "PlayerHeadDetector");
                    LoggingSystem.Debug($"  HeadBone: {(avatar.HeadBone != null ? avatar.HeadBone.name : "NULL")}", "PlayerHeadDetector");
                    
                    // Log other avatar bones if they exist
                    try
                    {
                        // These might exist in the Avatar class
                        var avatarType = avatar.GetType();
                        var fields = avatarType.GetFields();
                        
                        foreach (var field in fields)
                        {
                            if (field.FieldType == typeof(Transform) && field.Name.Contains("Bone"))
                            {
                                var boneTransform = field.GetValue(avatar) as Transform;
                                LoggingSystem.Debug($"  {field.Name}: {(boneTransform != null ? boneTransform.name : "NULL")}", "PlayerHeadDetector");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LoggingSystem.Debug($"Could not inspect avatar fields: {ex.Message}", "PlayerHeadDetector");
                    }
                }

                // Discover all transforms in hierarchy
                LoggingSystem.Debug("Full transform hierarchy:", "PlayerHeadDetector");
                LogAllTransforms(player.Transform, 0);

                LoggingSystem.Debug("=== END BONE DISCOVERY ===", "PlayerHeadDetector");
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Failed to discover bones: {ex.Message}", "PlayerHeadDetector");
            }
        }

        /// <summary>
        /// Recursively log all transforms in hierarchy with indentation
        /// </summary>
        private static void LogAllTransforms(Transform parent, int depth)
        {
            if (depth > 10) return; // Prevent infinite recursion
            
            string indent = new string(' ', depth * 2);
            // LoggingSystem.Debug($"{indent}- {parent.name} (Position: {parent.localPosition}, Children: {parent.childCount})", "PlayerHeadDetector");
            
            for (int i = 0; i < parent.childCount; i++)
            {
                LogAllTransforms(parent.GetChild(i), depth + 1);
            }
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Search for head bone by common naming patterns
        /// </summary>
        private static Transform? FindHeadByName(Transform root)
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
        private static Transform? FindEarAttachmentPoint(Transform? root)
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
                var found = FindChildByName(root!, boneName);
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
        private static Transform? FindEarChildBone(Transform? headBone)
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
                    LoggingSystem.Debug($"Found ear child bone: {child.name}", "PlayerHeadDetector");
                    return child;
                }
            }

            // If no specific ear bones, look for the highest positioned child
            // (ears are typically higher on the head than other features)
            Transform? highestChild = null;
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
        private static Transform? FindChildWithPartialName(Transform parent, string partialName)
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