using System;
using System.Collections.Generic;

namespace VTubeLink
{
    public static class DataMapper
    {
        public static List<MappedParam> BuildParamsDict(CapturedData data)
        {
            var parameters = new List<MappedParam>();
            var blendshapes = data.Blendshapes;

            float Bs(string key) => blendshapes.TryGetValue(key, out var val) ? val : 0f;

            // Opentrack Primary Parameters
            // TX, TY, TZ in cm
            parameters.Add(new MappedParam { Id = "TX", Value = data.HeadPositionX * 100f });
            parameters.Add(new MappedParam { Id = "TY", Value = data.HeadPositionY * 100f });
            parameters.Add(new MappedParam { Id = "TZ", Value = -data.HeadPositionZ * 100f }); // Typical ARKit uses -z for forward, opentrack might need adjustment but keeping negative standard.

            // Yaw, Pitch, Roll in degrees
            parameters.Add(new MappedParam { Id = "Yaw", Value = data.HeadRotationY });
            parameters.Add(new MappedParam { Id = "Pitch", Value = -data.HeadRotationX });
            parameters.Add(new MappedParam { Id = "Roll", Value = data.HeadRotationZ });

            // Custom Params (ARKit) mapped from blendshapes 1-1 to customParams names
            var nameMappings = new Dictionary<string, string>
            {
                { "EyeBlinkLeft", "eyeBlink_L" }, { "EyeLookDownLeft", "eyeLookDown_L" }, { "EyeLookInLeft", "eyeLookIn_L" },
                { "EyeLookOutLeft", "eyeLookOut_L" }, { "EyeLookUpLeft", "eyeLookUp_L" }, { "EyeSquintLeft", "eyeSquint_L" },
                { "EyeWideLeft", "eyeWide_L" }, { "EyeBlinkRight", "eyeBlink_R" }, { "EyeLookDownRight", "eyeLookDown_R" },
                { "EyeLookInRight", "eyeLookIn_R" }, { "EyeLookOutRight", "eyeLookOut_R" }, { "EyeLookUpRight", "eyeLookUp_R" },
                { "EyeSquintRight", "eyeSquint_R" }, { "EyeWideRight", "eyeWide_R" }, { "JawForward", "jawForward" },
                { "JawLeft", "jawLeft" }, { "JawRight", "jawRight" }, { "JawOpen", "jawOpen" }, { "MouthClose", "mouthClose" },
                { "MouthFunnel", "mouthFunnel" }, { "MouthPucker", "mouthPucker" }, { "MouthLeft", "mouthLeft" }, { "MouthRight", "mouthRight" },
                { "MouthSmileLeft", "mouthSmile_L" }, { "MouthSmileRight", "mouthSmile_R" }, { "MouthFrownLeft", "mouthFrown_L" },
                { "MouthFrownRight", "mouthFrown_R" }, { "MouthDimpleLeft", "mouthDimple_L" }, { "MouthDimpleRight", "mouthDimple_R" },
                { "MouthStretchLeft", "mouthStretch_L" }, { "MouthStretchRight", "mouthStretch_R" }, { "MouthRollLower", "mouthRollLower" },
                { "MouthRollUpper", "mouthRollUpper" }, { "MouthShrugLower", "mouthShrugLower" }, { "MouthShrugUpper", "mouthShrugUpper" },
                { "MouthPressLeft", "mouthPress_L" }, { "MouthPressRight", "mouthPress_R" }, { "MouthLowerDownLeft", "mouthLowerDown_L" },
                { "MouthLowerDownRight", "mouthLowerDown_R" }, { "MouthUpperUpLeft", "mouthUpperUp_L" }, { "MouthUpperUpRight", "mouthUpperUp_R" },
                { "BrowDownLeft", "browDown_L" }, { "BrowDownRight", "browDown_R" }, { "BrowInnerUp", "browInnerUp" },
                { "BrowOuterUpLeft", "browOuterUp_L" }, { "BrowOuterUpRight", "browOuterUp_R" }, { "CheekPuff", "cheekPuff" },
                { "CheekSquintLeft", "cheekSquint_L" }, { "CheekSquintRight", "cheekSquint_R" }, { "NoseSneerLeft", "noseSneer_L" },
                { "NoseSneerRight", "noseSneer_R" }, { "TongueOut", "tongueOut" }
            };

            foreach (var customParam in Constants.CustomParams)
            {
                if (nameMappings.TryGetValue(customParam, out var mappedKey))
                {
                    parameters.Add(new MappedParam { Id = customParam, Value = Bs(mappedKey) });
                }
                else
                {
                    parameters.Add(new MappedParam { Id = customParam, Value = 0.0f });
                }
            }

            return parameters;
        }
    }
}
