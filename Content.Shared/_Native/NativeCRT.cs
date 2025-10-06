using System.Runtime.InteropServices;

namespace Content.Shared._Native;
internal static unsafe class NativeCRT
{
    [DllImport("msvcrt.dll")]
    public static extern void* malloc(uint size);
}
