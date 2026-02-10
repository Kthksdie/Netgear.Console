using System;
using System.Collections.Generic;
using System.Text;

namespace Netgear.App {
    public static class StringExtensions {
        private static Dictionary<char, char> _superscript = new Dictionary<char, char>() {
            { '-', '⁻' },
            { '+', '⁺' },
            { '0', '⁰' },
            { '1', '¹' },
            { '2', '²' },
            { '3', '³' },
            { '4', '⁴' },
            { '5', '⁵' },
            { '6', '⁶' },
            { '7', '⁷' },
            { '8', '⁸' },
            { '9', '⁹' },
        };

        public static string ToSuperscript(this string value) {
            //⁰¹²³⁴⁵⁶⁷⁸⁹⁺⁻

            if (value == "0") {
                return string.Empty;
            }

            var result = string.Empty;
            foreach (char c in value) {
                if (_superscript.ContainsKey(c)) {
                    result += _superscript[c];
                }
            }

            return result;
        }

        public static string ToSize(this double value) {
            if (value > 1000000) {
                return $"{Math.Round(value / 1000000, 2)}tb";
            }
            else if (value > 1000) {
                return $"{Math.Round(value / 1000, 2)}gb";
            }
            else {
                return $"{Math.Round(value, 2)}mb";
            }
        }
    }
}
