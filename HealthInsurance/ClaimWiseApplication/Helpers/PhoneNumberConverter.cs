using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClaimWise.Application.Helpers
{
    public static class PhoneNumberConverter
    {
        public static string ToWhatsAppFormat(string rawNumber)
        {
            if (string.IsNullOrWhiteSpace(rawNumber))
                return null;

            // Remove any non-digit characters
            var digitsOnly = new string(rawNumber.Where(char.IsDigit).ToArray());

            // Ensure it's a 10-digit Indian mobile number
            if (digitsOnly.Length == 10)
                return $"+91{digitsOnly}";

            // If already in E.164 format, return as-is
            if (digitsOnly.StartsWith("91") && digitsOnly.Length == 12)
                return $"+{digitsOnly}";

            return null; // Invalid format
        }
    }

}
