namespace DevLifeBackend.Extensions
{
    public static class EnumExtensions
    {
        public static string[] ToFlagNames<TEnum>(this TEnum enumValue) where TEnum : Enum
        {
            if (!typeof(TEnum).IsDefined(typeof(FlagsAttribute), false))
            {
                return new[] { enumValue.ToString() };
            }

            var names = new List<string>();
            foreach (TEnum flag in Enum.GetValues(typeof(TEnum)))
            {
                if (flag.Equals(default(TEnum))) 
                {
                    continue;
                }
                if (enumValue.HasFlag(flag))
                {
                    names.Add(flag.ToString());
                }
            }
            return names.ToArray();
        }
    }
}
