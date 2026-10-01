namespace JobApplication.Infrastructure.Services.Email
{
    public static class OtpEmailHtmlBuilder
    {
        public static string Build(string title, string body, string? code)
        {
            var codeBlock = string.IsNullOrEmpty(code)
                ? string.Empty
                        : $@"<div style='display:inline-block; padding:15px 25px;
                                        background:#f1f3f5; border-radius:8px;
                                        letter-spacing:6px; font-size:28px;
                                        font-weight:bold; color:#222;'>
                                {code}
                            </div>";

                    return $@"
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset='UTF-8'>
            <meta name='viewport' content='width=device-width, initial-scale=1.0'>
        </head>
        <body style='margin:0; padding:0; background-color:#f4f6f8; font-family:Arial, Helvetica, sans-serif;'>
            <div style='padding:40px 20px;'>
                <div style='max-width:500px; margin:0 auto; background:#ffffff;
                            border-radius:12px; padding:40px 30px;
                            text-align:center; box-shadow:0 4px 15px rgba(0,0,0,0.08);'>
                    <h1 style='margin:0 0 15px; color:#222; font-size:24px;'>{title}</h1>
                    <p style='margin:0 0 25px; color:#666; font-size:15px; line-height:1.6;'>{body}</p>
                    {codeBlock}
                    <p style='margin:25px 0 0; color:#888; font-size:13px; line-height:1.5;'>
                        This is an automated email. Please do not reply.
                    </p>
                </div>
            </div>
        </body>
        </html>";
                }
    }
}
