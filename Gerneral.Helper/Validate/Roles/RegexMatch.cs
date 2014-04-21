using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Gerneral.Helper
{
    public static partial class ValidationExtensions
    {
        public static Validation RegexMatch(this Validation validation, string input, string pattern)
        {
            return Check(
                validation,
                () => Regex.IsMatch(input, pattern),
                string.Format("Parameter should match format {0}", pattern)
            );
        }

    }
}
