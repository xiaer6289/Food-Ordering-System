using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WSM.Helpers
{
    public class CaptchaVerification
    {
        public bool success { get; set; }
        public DateTime challenge_ts { get; set; }
        public string hostname { get; set; }
        public List<string> error_codes { get; set; }
    }

    public class ReCaptchaSettings
    {
        public string SecretKey { get; set; } = string.Empty;
    }
}