using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RapidGUI
{
    public enum MouseCursor
    {
        Default,
        ResizeHorizontal,
        ResizeVertical,
        ResizeUpLeft,
    }

    public static partial class RGUIUtility
    {
        //static readonly Dictionary<MouseCursor, CursorData.Data> cursorTable;
        static readonly Dictionary<MouseCursor, CursorType> cursorTable;


        static RGUIUtility()
        {
            //var data = Resources.Load<CursorData>("cursorData");

            //cursorTable = new Dictionary<MouseCursor, CursorData.Data>()
            //{
            //    { MouseCursor.Default, null},
            //    { MouseCursor.ResizeHorizontal, data.resizeHorizontal},
            //    { MouseCursor.ResizeVertical, data.resizeVertical},
            //    { MouseCursor.ResizeUpLeft, data.resizeUpLeft},
            //};

            cursorTable = new Dictionary<MouseCursor, CursorType>() {
                { MouseCursor.Default, CursorType.DEFAULT },
                { MouseCursor.ResizeHorizontal, CursorType.RESIZE_HORIZONTAL },
                { MouseCursor.ResizeVertical, CursorType.PICKUP },
                { MouseCursor.ResizeUpLeft, CursorType.RESIZE },
            };

            RapidGUIBehaviour.Instance.StartCoroutine(UpdateCursor());
        }


        static float cursorLimitTime;
        static float GetCursorTime() => Time.realtimeSinceStartup;

        public static void SetCursor(MouseCursor cursor, float life = 0.1f)
        {
            //ScriptableSingleton<CursorManager>.Instance.setCursorType(cursorTable[cursor]);
            if (cursor == MouseCursor.Default) {
                //SetCursorDefault();
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                cursorLimitTime = float.MaxValue;
            } else {
                ScriptableSingleton<CursorManager>.Instance.setCursorType(cursorTable[cursor]);
                cursorLimitTime = GetCursorTime() + life;
            }
            //else
            //{
            //    var data = cursorTable[cursor];

            //    Cursor.SetCursor(data.tex, data.hotspot, CursorMode.Auto);
            //    cursorLimitTime = GetCursorTime() + life;
            //}
        }

        //public static void SetCursorDefault()
        //{
        //    Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        //    cursorLimitTime = float.MaxValue;
        //}


        static IEnumerator UpdateCursor()
        {
            while (true)
            {
                yield return new WaitUntil(() => GetCursorTime() > cursorLimitTime);
                //SetCursorDefault();
                SetCursor(MouseCursor.Default);
            }
        }
    }
}