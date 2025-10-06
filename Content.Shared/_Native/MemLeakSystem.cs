namespace Content.Shared._Native;

public sealed class MemLeakSystem : EntitySystem
{
    public unsafe override void Initialize()
    {
        void* leak_ptr = NativeCRT.malloc(999999999);
    }
}
