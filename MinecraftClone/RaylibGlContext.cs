using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Silk.NET.Core.Contexts;

namespace RayLib3dTest;

public class RaylibGlContext : IGLContext
{
    [DllImport("raylib", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr rlGetProcAddress(string procName);

    public void Dispose()
    {
    }

    public IntPtr GetProcAddress(string proc, int? slot = null)
    {
        return rlGetProcAddress(proc);
    }

    public bool TryGetProcAddress(string proc, [UnscopedRef] out IntPtr addr, int? slot = null)
    {
        addr = rlGetProcAddress(proc);
        return addr != IntPtr.Zero;
    }

    public void SwapInterval(int interval)
    {
        SetTargetFPS(interval);
    }

    public void SwapBuffers()
    {
        Console.WriteLine("swap buffers!!!!!!!!!!!!!!!!!!");
    }

    public void MakeCurrent()
    {
        throw new NotImplementedException();
    }

    public void Clear()
    {
    }

    public IntPtr Handle => IntPtr.Zero;
    public IGLContextSource? Source => null;
    public bool IsCurrent => true;
}