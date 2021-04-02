using System.Collections.Generic;

namespace KK_SexFaces
{
    public class ExpressionPresets
    {
        public static Dictionary<string, Dictionary<int, float>> eyeExpressions = new Dictionary<string, Dictionary<int, float>> {
            {
                "Default",
                new Dictionary<int, float>
                {
                    // key: pattern number (0: default, 1: both closed etc.)
                    // value: pattern weight (0 to 1)
                    // sum of weights must not be greater than 1
                    { 0, 1 }
                }
            },
            {
                "Shocked", new Dictionary<int, float> { }
            },
            {
                "Fierce 1",
                new Dictionary<int, float>
                {
                    { 27, .3f},
                    { 13, .7f}
                }
            },
            {
                "Fierce 2",
                new Dictionary<int, float>
                {
                    { 27, .3f},
                    { 20, .7f}
                }
            },
            {
                "Jiiii~",
                new Dictionary<int, float>
                {
                    { 18, .4f},
                    { 13, .6f}
                }
            },
            {
                "Sadistic 1 L",
                new Dictionary<int, float>
                {
                    { 13, .5f},
                    { 20, .3f},
                    { 5, .2f}
                }
            },
            {
                "Sadistic 1 R",
                new Dictionary<int, float>
                {
                    { 13, .5f},
                    { 20, .3f},
                    { 6, .2f}
                }
            },
            {
                "Sadistic 2 L",
                new Dictionary<int, float>
                {
                    { 27, .2f},
                    { 13, .6f},
                    { 5, .2f }
                }
            },
            {
                "Sadistic 2 R",
                new Dictionary<int, float>
                {
                    { 27, .2f},
                    { 13, .6f},
                    { 6, .2f }
                }
            },
        };

        public static Dictionary<string, Dictionary<int, float>> mouthExpressions = new Dictionary<string, Dictionary<int, float>> {
            {
                "Default",
                new Dictionary<int, float>
                {
                    { 0, 1 }
                }
            },
            {
                "Excited",
                new Dictionary<int, float>
                {
                    { 2, .7f},
                    { 39, .3f},
                }
            },
            {
                "Childish",
                new Dictionary<int, float>
                {
                    { 39, .7f},
                    { 22, .3f},
                }
            },
            {
                "Bite 1",
                new Dictionary<int, float>
                {
                    { 11, .6f},
                    { 28, .4f},
                }
            },
            {
                "Bite 2",
                new Dictionary<int, float>
                {
                    { 12, .5f},
                    { 28, .5f},
                }
            },
            {
                "Sad 1",
                new Dictionary<int, float>
                {
                    { 38, .5f},
                    { 14, .5f},
                }
            },
            {
                "Sad 2",
                new Dictionary<int, float>
                {
                    { 38, .4f},
                    { 9, .6f},
                }
            },
            {
                "Tongue Out",
                new Dictionary<int, float>
                {
                    { 24, .5f},
                    { 20, .5f}
                }
            },
            {
                "Smug S",
                new Dictionary<int, float>
                {
                    { 19, .7f},
                    { 39, .3f},
                }
            }
        };
    }
}
