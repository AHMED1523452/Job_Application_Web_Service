using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace JobApplication.Application.Services
{
    public static class NormalizeContentTextService
    {
        public static string normalizeText(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentNullException("Text is mediatory here");
            return Regex.Replace(text.Trim(), @"\s+"," "); //. 
        }
    }
}
