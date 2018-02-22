using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace ProWorldEditor
{
    public static class MyGUI
    {
        public static float LogSlider(float value, float min, float max, params GUILayoutOption[] option)
        {
            var power = Mathf.Log10(value);
            power = GUILayout.HorizontalSlider(power, min, max, option);
            return Mathf.Pow(10, power);
        }

        public static int RoundSlider(int index, int min, int max, params GUILayoutOption[] options)
        {
            var mode = GUILayout.HorizontalSlider(index, min, max, options);
            return Mathf.RoundToInt(mode);

        }

        public static int EnumSlider(int index, int max, params GUILayoutOption[] options)
        {
            var mode = GUILayout.HorizontalSlider(index, 0, max, options);
            return Mathf.RoundToInt(mode);

        }

        public static int EnumButton(int e, string[] names, int width)
        {
            var output = e;
            var bwidth = width/names.Length;

            GUILayout.BeginHorizontal();
            for (var i = 0; i < names.Length; i++)
            {
                GUI.color = i == e ? Color.red : Color.white;

                if (GUILayout.Button("", GUILayout.Width(bwidth)))
                {
                    output = i;
                }
            }
            GUILayout.EndHorizontal();

            GUI.color = Color.white;

            return output;
        }
        public static Enum EnumButton(Enum e, params GUILayoutOption[] options)
        {
            var output = e;

            var val = Enum.GetValues(e.GetType()).OfType<Enum>().ToList();
            var names = Enum.GetNames(e.GetType()).ToList();

            //var bwidth = width / val.Count;

            GUILayout.BeginHorizontal(options);
            for (int index = 0; index < val.Count; index++)
            {
                var en = val[index];
                var name = names[index];

                GUI.color = en.Equals(e) ? Color.red : Color.white;

                var style = UnityEditor.EditorStyles.miniButtonMid;

                if (GUILayout.Button(name, style))
                {
                    output = en;
                }
            }

            /*for (var i = 0; i < names.Length; i++)
            {
                GUI.color = i == e ? Color.red : Color.white;

                if (GUILayout.Button("", GUILayout.Width(bwidth)))
                {
                    output = values[i];
                }
            }*/
            GUILayout.EndHorizontal();

            GUI.color = Color.white;

            return output;
        }

        public static bool HorizontalSliderC(float value, float leftValue, float rightValue, out float result,
                                             params GUILayoutOption[] options)
        {
            result = GUILayout.HorizontalSlider(value, leftValue, rightValue, options);

            return Math.Abs(result - value) > float.Epsilon;
        }

        public static bool HorizontalSliderC(int value, int leftValue, int rightValue, out int result,
                                             params GUILayoutOption[] options)
        {
            result = (int) GUILayout.HorizontalSlider(value, leftValue, rightValue, options);

            return result != value;
        }

        public static float StaticSlider(float value, float min, float max, int width, int decimals = 2)
        {
            if (width < 0)
                throw new UnityException("Width is less than 0");

            var dif = width/Mathf.Pow(10, decimals)/2;

            bool clicked = (Event.current.button == 1) && (Event.current.type == EventType.MouseUp);

            var rect = GUILayoutUtility.GetRect(new GUIContent("Slider"), GUI.skin.horizontalSlider,
                                                GUILayout.Width(width));
            var output = GUI.HorizontalSlider(rect, value, value - dif, value + dif);

            // Right click reset
            if (clicked && rect.Contains(Event.current.mousePosition))
                output = 0;

            output = (float) Math.Round(output, decimals);

            return Mathf.Clamp(output, min, max);
        }

        public static float RoundedLogSlider(float value, int min, int max)
        {
            var before = value;
            value = LogSlider(value, min, max);
            if (Math.Abs(value - before) > float.Epsilon)
                value = RoundToLog(value);

            var sizeT = GUILayout.TextField(value.ToString(CultureInfo.InvariantCulture), 5, GUILayout.Width(50));
            sizeT = Regex.Replace(sizeT, "^[^0-9]", "");
            float.TryParse(sizeT, out value);
            
            return value;
        }

        private static float RoundToLog(float val)
        {
            var pow = (int)Mathf.Log10(val) - 1;
            var closest = Mathf.Pow(10, pow);

            return ((int)Mathf.Round(val / closest)) * (int)closest;
        }
    }
}