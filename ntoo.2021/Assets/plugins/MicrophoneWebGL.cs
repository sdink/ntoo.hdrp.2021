using System;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

public static class MicrophoneWebGL
{
#if UNITY_WEBGL// && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void MicrophoneWebGL_Init(int bufferSize, int numberOfChannels);

    [DllImport("__Internal")]
    private static extern void MicrophoneWebGL_PollInit(IntPtr resultPtr, int resultMaxLength);

    [DllImport("__Internal")]
    private static extern void MicrophoneWebGL_Start();

    [DllImport("__Internal")]
    private static extern void MicrophoneWebGL_Stop();

    [DllImport("__Internal")]
    private static extern int MicrophoneWebGL_GetNumBuffers();

    [DllImport("__Internal")]
    private static extern bool MicrophoneWebGL_GetBuffer(IntPtr bufferPtr);

    private static int _bufferSize;

    public static void Init(int bufferSize, int numberOfChannels)
    {
        _bufferSize = bufferSize;
        MicrophoneWebGL_Init(bufferSize, numberOfChannels);
    }

    public static string PollInit()
    {
        var buffer = new byte[512];
        var pinnedBuffer = GCHandle.Alloc(buffer, GCHandleType.Pinned);

        MicrophoneWebGL_PollInit(pinnedBuffer.AddrOfPinnedObject(), buffer.Length);

        pinnedBuffer.Free();

        var length = 0;
        while (length + 1 < buffer.Length && (buffer[length] != 0 || buffer[length + 1] != 0))
            length += 2;
        return Encoding.Unicode.GetString(buffer, 0, length);
    }

    public static void Start()
    {
        MicrophoneWebGL_Start();
    }

    public static void Stop()
    {
        MicrophoneWebGL_Stop();
    }

    public static int GetNumBuffers()
    {
        return MicrophoneWebGL_GetNumBuffers();
    }

    public static bool GetBuffer(float[] buffer)
    {
        if (buffer.Length != _bufferSize)
        {
            Debug.LogError(string.Format("Incorrect buffer size {0} - size at initialization was {1}", buffer.Length, _bufferSize));
            return false;
        }

        var pinnedBuffer = GCHandle.Alloc(buffer, GCHandleType.Pinned);
        var result = MicrophoneWebGL_GetBuffer(pinnedBuffer.AddrOfPinnedObject());
        pinnedBuffer.Free();
        return result;
    }
#endif
}
