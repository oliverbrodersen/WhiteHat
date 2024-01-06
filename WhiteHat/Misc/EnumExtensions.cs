namespace WhiteHat.Misc
{
    public static class EnumExtensions
    {
        public static T Next<T>(this T enumValue) where T : Enum
        {
            var values = Enum.GetValues(enumValue.GetType()).Cast<T>();
            var nextIndex = (Array.IndexOf(values.ToArray(), enumValue) + 1) % values.Count();
            return values.ElementAt(nextIndex);
        }
    }
}
