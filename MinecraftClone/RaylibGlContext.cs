#define WINDOWS

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Silk.NET.Core.Contexts;

namespace RayLib3dTest;

public class RaylibGlContext : IGLContext
{

    public void Dispose()
    {
    }

    public unsafe IntPtr GetProcAddress(string proc, int? slot = null)
    {
#if WINDOWS
        // Try wglGetProcAddress first (extensions)
        nint addr = wglGetProcAddress(proc);
        if (addr == 0 || addr == 1 || addr == 2 || addr == 3 || addr == -1)
        {
            // Fall back to the OpenGL32 export for core functions
            nint module = GetModuleHandle("opengl32.dll");
            addr = GetProcAddress(module, proc);
        }
        return addr;
#elif LINUX
        return glXGetProcAddress(procName);
#elif OSX
        // macOS is different; you’d normally use dlsym to resolve symbols from OpenGL framework.
        return dlsym(RTLD_DEFAULT, procName);
#else
        throw new PlatformNotSupportedException();
#endif
    }

    public bool TryGetProcAddress(string proc, [UnscopedRef] out IntPtr addr, int? slot = null)
    {
        addr = GetProcAddress(proc);
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

#if WINDOWS
    [DllImport("opengl32.dll", EntryPoint = "wglGetProcAddress")]
    private static extern nint wglGetProcAddress(string name);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern nint GetModuleHandle(string lpModuleName);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern nint GetProcAddress(nint hModule, string procName);
#elif LINUX
    [DllImport("libGL.so.1")]
    private static extern nint glXGetProcAddress(string name);
#elif OSX
    private const int RTLD_DEFAULT = -2;
    [DllImport("libSystem.dylib")]
    private static extern nint dlsym(int handle, string symbol);
#endif
}