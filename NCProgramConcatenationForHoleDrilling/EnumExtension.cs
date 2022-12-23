using System;

namespace NCProgramConcatenationForHoleDrilling
{
    public static class EnumExtension
    {
        public static string GetEnumDisplayName<T>(this T enumValue)
        {
            var field = typeof(T).GetField(enumValue.ToString());
            var attrType = typeof(EnumDisplayNameAttribute);
            var attribute = Attribute.GetCustomAttribute(field, attrType);
            return (attribute as EnumDisplayNameAttribute)?.Name;
        }
    }
}
