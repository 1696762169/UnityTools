using System;

namespace UnityEngine
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false)]
    public sealed class NonSerializedFieldAttribute : Attribute
    {

    }
}
