using Ganss.Xss;
using System.Text;


namespace MroczwareFramework.Helpers
{
    public static class HtmlAndSvgSanitizer
    {
        private static readonly IList<string> allowedHtmlTags = new List<string> { "meta", "html", "head", "body" };
        private static readonly IList<string> allowedHtmlAttributes = new List<string> { "http-equiv", "content", "name", "itemscope", "itemprop", "itemtype", "class", "style" };

        // lista elementów i atrybutów - z https://pythonhosted.org/feedparser/html-sanitization.html#svg-sanitization
        // Pełna lista z specyfikacji https://www.w3.org/TR/SVG2/Overview.html, appendix F (Element Index) oraz G (Attribute Index)
        private static readonly IList<string> allowedSvgTags = new List<string> {
            "a", "animate", "animateColor", "animateMotion", "animateTransform",
            "circle", "defs", "desc", "ellipse", "font-face", "font-face-name",
            "font-face-src", "foreignObject", "g", "glyph", "hkern", "line", "linearGradient",
            "marker", "metadata", "missing-glyph", "mpath", "path", "polygon", "polyline",
            "radialGradient", "rect", "set", "stop", "svg", "switch", "text", "title", "tspan", "use" };
        private static readonly IList<string> allowedSvgAttributes = new List<string> {
            "accent-height", "accumulate", "additive", "alphabetic", "arabic-form", "ascent",
            "attributeName", "attributeType", "baseProfile", "bbox", "begin", "by", "calcMode",
            "cap-height", "class", "color", "color-rendering", "content", "cx", "cy", "d", "descent", "display",
            "dur", "dx", "dy", "end", "fill", "fill-opacity", "fill-rule", "font-family", "font-size",
            "font-stretch", "font-style", "font-variant", "font-weight", "from", "fx", "fy", "g1", "g2",
            "glyph-name", "gradientUnits", "hanging", "height", "horiz-adv-x", "horiz-origin-x", "id",
            "ideographic", "k", "keyPoints", "keySplines", "keyTimes", "lang", "marker-end", "marker-mid",
            "marker-start", "markerHeight", "markerUnits", "markerWidth", "mathematical", "max", "min", "name",
            "offset", "opacity", "orient", "origin", "overline-position", "overline-thickness", "panose-1", "path",
            "pathLength", "points", "preserveAspectRatio", "r", "refX", "refY", "repeatCount", "repeatDur",
            "requiredExtensions", "requiredFeatures", "restart", "rotate", "rx", "ry", "slope", "stemh", "stemv",
            "stop-color", "stop-opacity", "strikethrough-position", "strikethrough-thickness",
            "stroke", "stroke-dasharray", "stroke-dashoffset", "stroke-linecap", "stroke-linejoin",
            "stroke-miterlimit", "stroke-opacity", "stroke-width", "systemLanguage", "target", "text-anchor",
            "to", "transform", "type", "u1", "u2", "underline-position", "underline-thickness", "unicode", "unicode-range",
            "units-per-em", "values", "version", "viewBox", "visibility", "width", "widths", "x", "x-height", "x1", "x2",
            "xlink:actuate", "xlink:arcrole", "xlink:href", "xlink:role", "xlink:show", "xlink:title", "xlink:type",
            "xml:base", "xml:lang", "xml:space", "xmlns", "xmlns:xlink",
            "y", "y1", "y2", "zoomAndPan"
        };

        private static readonly HtmlSanitizer sanitizer = new();

        static HtmlAndSvgSanitizer()
        {
            // dodaję custom HTML tags i atrybuty
            foreach (var tag in allowedHtmlTags)
                sanitizer.AllowedTags.Add(tag);

            foreach (var attribute in allowedHtmlAttributes)
                sanitizer.AllowedAttributes.Add(attribute);

            // dodaję custom SVG tags i atrybuty
            foreach (var tag in allowedSvgTags)
                sanitizer.AllowedTags.Add(tag);

            foreach (var attribute in allowedSvgAttributes)
                sanitizer.AllowedAttributes.Add(attribute);

            // pozwalamy na wszystkie data-attributes
            sanitizer.AllowDataAttributes = true;

            //pozwalamy na adresy mailto
            sanitizer.AllowedSchemes.Add("mailto");
        }

        /// <summary>
        /// Czyszczenie fragmentu HTML i dokumentu (w tym SVG) z konstrukcji, które mogą prowadzić do ataków XSS.
        /// </summary>
        /// <param name="text">Fragment dokumenta do czyszczenia</param>
        /// <returns>Wyczyszczony fragment dokumenta</returns>
        public static string SanitizeText(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }
            string result = sanitizer.Sanitize(text);
            //result = Regex.Replace(result, @"^\s+$[\r\n]*", string.Empty, RegexOptions.Multiline).Trim(); // usuwam pusle linije w środku

            bool isChanged = result.CompareTo(text.Trim()) != 0; // dla testów

            return result;
        }

        /// <summary>
        /// Czyszczenie pliku z fragmentów HTML itp. z konstrukcji, które mogą prowadzić do ataków XSS.
        /// </summary>
        /// <param name="file">Plik do czyszczenia</param>
        /// <returns>Wyczyszczony plik</returns>
        public static MemoryStream SanitizeFile(Stream file)
        {
            // zczytuję text z pliku
            using var sr = new StreamReader(file);
            string fileText = sr.ReadToEnd();

            // czyszczę tekst
            string clearedFileText = SanitizeText(fileText);

            // zapisuję tekst do pliku
            MemoryStream result = new(Encoding.UTF8.GetBytes(clearedFileText));

            return result;
        }

        /// <summary>
        /// Czyszczenie pliku z fragmentów HTML itp. z konstrukcji, które mogą prowadzić do ataków XSS.
        /// </summary>
        /// <param name="file">Plik do czyszczenia</param>
        /// <returns>Wyczyszczony plik</returns>
        public static byte[] SanitizeFile(byte[] file)
        {
            // zczytuję text z pliku
            string fileText = Encoding.UTF8.GetString(file);

            // czyszczę tekst
            string clearedFileText = SanitizeText(fileText);

            // zapisuję tekst do pliku
            byte[] result = Encoding.UTF8.GetBytes(clearedFileText);

            return result;
        }

        ///// <summary>
        ///// Wywołuje metodę MoveCssInLine z nugueta Premailer
        ///// </summary>
        ///// <param name="source"></param>
        ///// <returns></returns>
        //public static string MoveCssInline(string source)
        //{
        //    return Premailer.MoveCssInline(source).Html;
        //}

    }
}
