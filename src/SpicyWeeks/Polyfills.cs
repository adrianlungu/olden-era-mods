// The interop UnityEngine.CoreModule hides the BCL's nullable attributes from the compiler; provide local copies.
namespace System.Runtime.CompilerServices;

[AttributeUsage(AttributeTargets.All, Inherited = false)]
internal sealed class NullableAttribute : Attribute
{
    public NullableAttribute(byte flag) { }
    public NullableAttribute(byte[] flags) { }
}

[AttributeUsage(AttributeTargets.All, Inherited = false)]
internal sealed class NullableContextAttribute : Attribute
{
    public NullableContextAttribute(byte flag) { }
}
