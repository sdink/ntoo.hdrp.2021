using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

public class LiveHelpers
{
    static private readonly string liveUnityVersion = "2.5.5.141";
    static public readonly char dictionarySeparator = '\t';
    static public readonly char objAttrSeparator = ':';

    static public string CurrentVersion()
    {
        return liveUnityVersion;
    }

    static public string JoinNameAttr(string name, string attr)
    {
        return name + objAttrSeparator + attr;
    }

    /****************************************************************************************************/
    static public void SplitNameAttr(string control, out string name, out string attr)
    {
        int pos = control.LastIndexOf(objAttrSeparator);
        name = control.Substring(0, pos);
        attr = control.Substring(pos + 1);
    }

    static public string GetAttrString(string control)
    {
        int pos = control.LastIndexOf(objAttrSeparator);
        return control.Substring(pos + 1);
    }

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
    [DllImport("user32.dll", EntryPoint = "FindWindow")]
    private static extern IntPtr FindWindow(string sClass, string sWindow);

    [DllImport("user32.dll")]
    public static extern int SendMessage(IntPtr hWnd, uint uMsg, IntPtr wParam, ref COPYDATASTRUCT lParam);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder strText, int maxCount);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowTextLength(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

    // A struct to build a COPYDATA OBJECT
    public struct COPYDATASTRUCT
    {
        public IntPtr dwData;
        public int cbData;
        [MarshalAs(UnmanagedType.LPStr)]
        public string lpData;
    }

    // Delegate to filter which windows to include 
    public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    public static string GetWindowText(IntPtr hWnd)
    {
        int size = GetWindowTextLength(hWnd);
        if (size > 0)
        {
            var builder = new StringBuilder(size + 1);
            GetWindowText(hWnd, builder, builder.Capacity);
            return builder.ToString();
        }

        return String.Empty;
    }

    public static IEnumerable<IntPtr> FindWindows(EnumWindowsProc filter)
    {
        List<IntPtr> windows = new List<IntPtr>();

        EnumWindows(delegate (IntPtr wnd, IntPtr param)
        {
            if (filter(wnd, param))
            {
                // only add the windows that pass the filter
                windows.Add(wnd);
            }
            // but return true here so that we iterate all windows
            return true;
        }, IntPtr.Zero);

        return windows;
    }

    public static IEnumerable<IntPtr> FindWindowsWithText(string titleText)
    {
        return FindWindows(delegate (IntPtr wnd, IntPtr param)
        {
            return GetWindowText(wnd).Contains(titleText);
        });
    }
#endif
}

// Extension Method to allow is to search all children transforms to find a specific GameObject
public static class TransformDeepChildExtension
{
    //Breadth-first search
    public static Transform FindDeepChild(this Transform aParent, string aName)
    {
        if (aParent.name == aName)
            return aParent;
        var result = aParent.Find(aName);
        if (result != null)
            return result;
        foreach (Transform child in aParent)
        {
            result = child.FindDeepChild(aName);
            if (result != null)
                return result;
        }
        return null;
    }

    public static List<GameObject> ReturnAllChildren(this Transform aParent, List<GameObject> objects)
    {
        objects.Add(aParent.gameObject);
        // Call set layer on any children
        for (int i = 0; i < aParent.childCount; i++)
        {
            ReturnAllChildren(aParent.GetChild(i), objects);
        }

        return objects;
    }
}