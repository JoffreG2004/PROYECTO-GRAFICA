using System.Drawing;

namespace proyectoPaint.GraphicsCore
{
    /// <summary>Paleta central para los temas claro y oscuro de Lumina Paint.</summary>
    public static class ThemeColors
    {
        public static bool IsDark { get; private set; }

        public static Color Background    { get { return IsDark ? ColorTranslator.FromHtml("#16181D") : ColorTranslator.FromHtml("#F6F2EE"); } }
        public static Color Panel         { get { return IsDark ? ColorTranslator.FromHtml("#20232B") : ColorTranslator.FromHtml("#FAF8F5"); } }
        public static Color Canvas        { get { return IsDark ? ColorTranslator.FromHtml("#2A2D35") : Color.White; } }
        public static Color Border        { get { return IsDark ? ColorTranslator.FromHtml("#3A3F4B") : ColorTranslator.FromHtml("#D8D0C8"); } }
        public static Color Divider       { get { return IsDark ? ColorTranslator.FromHtml("#343944") : ColorTranslator.FromHtml("#E2DAD2"); } }
        public static Color TextPrimary   { get { return IsDark ? ColorTranslator.FromHtml("#F2EEE9") : ColorTranslator.FromHtml("#332B27"); } }
        public static Color TextSecondary { get { return IsDark ? ColorTranslator.FromHtml("#C3BBB4") : ColorTranslator.FromHtml("#766A63"); } }
        public static Color Accent        { get { return IsDark ? ColorTranslator.FromHtml("#D6B89A") : ColorTranslator.FromHtml("#C89C74"); } }
        public static Color AccentDark    { get { return IsDark ? ColorTranslator.FromHtml("#B98455") : ColorTranslator.FromHtml("#B98455"); } }
        public static Color AccentSoft    { get { return IsDark ? ColorTranslator.FromHtml("#44372F") : ColorTranslator.FromHtml("#EADCCF"); } }
        public static Color Hover         { get { return IsDark ? ColorTranslator.FromHtml("#322B27") : ColorTranslator.FromHtml("#EFE8E1"); } }
        public static Color Selected      { get { return IsDark ? ColorTranslator.FromHtml("#4B3D33") : ColorTranslator.FromHtml("#EADCCF"); } }
        public static Color Icon          { get { return IsDark ? ColorTranslator.FromHtml("#E3D7CD") : ColorTranslator.FromHtml("#4A3F38"); } }
        public static Color Save          { get { return AccentDark; } }
        public static Color Export        { get { return Accent; } }

        public static readonly Color SwatchWhite       = Color.White;
        public static readonly Color SwatchBeigeLight  = ColorTranslator.FromHtml("#E8E3DF");
        public static readonly Color SwatchBeigeMedium = ColorTranslator.FromHtml("#D8D0C8");
        public static readonly Color SwatchSand        = ColorTranslator.FromHtml("#D6B89A");
        public static readonly Color SwatchCaramel     = ColorTranslator.FromHtml("#C89C74");
        public static readonly Color SwatchBrownSoft   = ColorTranslator.FromHtml("#A67C52");
        public static readonly Color SwatchTaupe       = ColorTranslator.FromHtml("#8B7A6B");
        public static readonly Color SwatchDarkBrown   = ColorTranslator.FromHtml("#4A3F38");
        public static readonly Color SwatchGrayLight   = ColorTranslator.FromHtml("#EDEAE7");
        public static readonly Color SwatchGrayMedium  = ColorTranslator.FromHtml("#B8B0AA");
        public static readonly Color SwatchBlackSoft   = ColorTranslator.FromHtml("#2F2925");

        public static Color[] Swatches
        {
            get
            {
                return new[]
                {
                    SwatchDarkBrown, SwatchBrownSoft, SwatchCaramel, SwatchSand, SwatchBeigeMedium, SwatchBeigeLight,
                    SwatchBlackSoft, SwatchTaupe, SwatchGrayMedium, SwatchGrayLight, ColorTranslator.FromHtml("#C7BBAF"), ColorTranslator.FromHtml("#9C8B7A"),
                    SwatchWhite, ColorTranslator.FromHtml("#EFE8E1"), ColorTranslator.FromHtml("#BFA98C"), ColorTranslator.FromHtml("#8A6D4F"), ColorTranslator.FromHtml("#6E5C4C"), Accent
                };
            }
        }

        public static void Toggle() { IsDark = !IsDark; }
    }
}
