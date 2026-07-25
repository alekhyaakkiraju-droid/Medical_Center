namespace AngularApi.Logging
{
    public static class PiiMasking
    {
        public static string MaskEmail(string? value)
        {
            if (string.IsNullOrWhiteSpace(value) || !value.Contains('@'))
            {
                return value ?? string.Empty;
            }

            var parts = value.Split('@');
            if (parts[0].Length <= 2)
            {
                return $"**@{parts[1]}";
            }

            return $"{parts[0][..2]}***@{parts[1]}";
        }

        public static string MaskName(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value ?? string.Empty;
            }

            return value.Length <= 1 ? "*" : $"{value[0]}***";
        }
    }
}
