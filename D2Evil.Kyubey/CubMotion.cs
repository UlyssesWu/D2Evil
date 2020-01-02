using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using D2Evil.Kyubey.Models;

namespace D2Evil.Kyubey
{
    public class CubMotion
    {
        #region MotionBase

        public int FadeInMs { get; set; }
        public int FadeOutMs { get; set; }

        #endregion

        public List<Motion> Motions = new List<Motion>();
        public float FPS { get; set; }
        public bool Loop { get; set; }
        public bool LoopFadeIn { get; set; }
        public int DurationMs { get; set; }
        public int LoopDurationMs { get; set; }
        private int _maxLength = 0;
        public static CubMotion Load(string[] lines)
        {
            CubMotion motion = new CubMotion();
            for (var i = 0; i < lines.Length; i++)
            {
                lines[i] = lines[i].Trim();
            }

            Dictionary<string, int> fadeInDic = new Dictionary<string, int>();
            Dictionary<string, int> fadeOutDic = new Dictionary<string, int>();
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                if (line.StartsWith("#")) //comment
                {
                    continue;
                }

                var kv = line.Split('=');
                if (kv.Length < 2)
                {
                    continue; //error
                }

                var k = kv[0];
                var v = kv[1];

                if (k.StartsWith("$"))
                {
                    if (k.StartsWith("$fadein:"))
                    {
                        fadeInDic[k.Substring(8)] = int.Parse(v);
                    }
                    else if (k.StartsWith("$fadeout:"))
                    {
                        fadeOutDic[k.Substring(9)] = int.Parse(v);
                    }
                    else
                    {
                        switch (k)
                        {
                            case "$fps":
                                motion.FPS = float.Parse(v);
                                break;
                            case "$fadein":
                                motion.FadeInMs = int.Parse(v);
                                break;
                            case "$fadeout":
                                motion.FadeOutMs = int.Parse(v);
                                break;
                            default:
                                Debug.WriteLine($"Unknown control type: {k}");
                                break;
                        }
                    }
                    continue;
                }

                Motion m = new Motion();

                //Default types
                if (k.StartsWith("VISIBLE:"))
                {
                    m.MotionType = MotionType.VISIBLE;
                    m.ID = k.Substring(8); //length of VISIBLE:
                }
                else if (k.StartsWith("LAYOUT:"))
                {
                    if (!Enum.TryParse(k.Substring(7), out MotionType t))
                    {
                        continue; //error
                    }

                    switch (t)
                    {
                        case MotionType.ANCHOR_X:
                            m.MotionType = MotionType.ANCHOR_X;
                            break;
                        case MotionType.ANCHOR_Y:
                            m.MotionType = MotionType.ANCHOR_Y;
                            break;
                        case MotionType.SCALE_X:
                            m.MotionType = MotionType.SCALE_X;
                            break;
                        case MotionType.SCALE_Y:
                            m.MotionType = MotionType.SCALE_Y;
                            break;
                        case MotionType.X:
                            m.MotionType = MotionType.X;
                            break;
                        case MotionType.Y:
                            m.MotionType = MotionType.Y;
                            break;
                    }
                }
                else //PARAM
                {
                    m.MotionType = MotionType.Param;
                    m.ID = k;
                }

                //Read values
                var vals = v.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                m.Values = vals.Select(float.Parse).ToArray();
                if (motion._maxLength < m.Values.Length)
                {
                    motion._maxLength = m.Values.Length;
                }

                motion.Motions.Add(m);
            }
            //Set fade in/out
            foreach (var mtn in motion.Motions.Where(mtn => fadeInDic.ContainsKey(mtn.ID)))
            {
                mtn.FadeInMs = fadeInDic[mtn.ID];
            }
            foreach (var mtn in motion.Motions.Where(mtn => fadeOutDic.ContainsKey(mtn.ID)))
            {
                mtn.FadeOutMs = fadeOutDic[mtn.ID];
            }

            return motion;
        }
    }
}
