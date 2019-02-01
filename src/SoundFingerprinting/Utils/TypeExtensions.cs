using System;
using System.Text;

namespace SoundFingerprinting.Utils
{
    public static class TypeExtensions
    {
        public static string GetNameWithGenericArgs(this Type t)
        {
            if (!t.IsGenericType)
                return t.Name;

            StringBuilder sb = new StringBuilder();
            sb.Append(t.Name.Substring(0, t.Name.IndexOf('`')));
            sb.Append('<');
            bool appendComma = false;
            foreach (Type arg in t.GetGenericArguments())
            {
                if (appendComma) sb.Append(", ");
                sb.Append(GetNameWithGenericArgs(arg));
                appendComma = true;
            }
            sb.Append('>');
            return sb.ToString();
        }
    }
}
