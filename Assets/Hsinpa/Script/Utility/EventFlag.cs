using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventFlag
{
    public class LevelComponent {
        public const string SnakeType = "snake";
        public const string SpeedType = "speed";
    }

    public class Path {
        public const string SnakePathSRPFolder = "Assets/Hsinpa/PathSRPData/";
    }

    public class ColorSet {
        public readonly static Color Red = new Color32(255, 102, 88, 255);
        public readonly static Color Blue = new Color32(88, 248, 255, 255);
    }
    public class SnakeShaderVar
    {
        public const string Color = "_Color";
        public const string DisplayContraint = "_UseDisplayContraint";
    }

}
