using System;

namespace NCProgramConcatenationForHoleDrilling
{
    [AttributeUsage(AttributeTargets.Enum | AttributeTargets.Field)]
    public class EnumDisplayNameAttribute : Attribute
    {
        /// <summary>表示名</summary>
        public string Name { get; set; }

        /// <summary>enum表示名属性</summary>
        /// <param name="name">表示名</param>
        public EnumDisplayNameAttribute(string name)
        {
            Name = name;
        }
    }
}
