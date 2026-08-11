namespace EgyMediChain.Api.Common;

// Backend Action Report §5.4 - shared email delivery abstraction. Every call site (Forgot
// Password OTP, password-changed notifications, official-email-changed notifications, admin
// reset-password) goes through this single interface, so swapping providers later only touches
// one implementation.
public interface IEmailService
{
    // Fire-and-forget by design: callers should not let a delivery failure block the request that
    // triggered it (see Program.cs registration - failures are caught and logged, never thrown
    // back into the request pipeline for something like a password reset).
    Task SendAsync(string toEmail, string subject, string htmlBody);
}
