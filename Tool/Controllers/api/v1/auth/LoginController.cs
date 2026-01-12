using Application.Result;
using Microsoft.AspNetCore.Mvc;

namespace FlouPoint.Api.Controllers.api.v1.auth
{
    //[Route("api/v1/auth/")]
    //[ApiController]
    //public class LoginController(IOtpManager otpManager) : ControllerBaseHelpers
    //{
    //    [HttpPost("generateOtp")]
    //    public async Task<IActionResult> GenerateOtp(CredentialBase credential)
    //    {
    //        Operation<bool> result = await otpManager.GenerateOtp(credential.Email);
    //        return HandleError(result);
    //    }

    //    [HttpPost("verifyOtp")]
    //    public async Task<IActionResult> VerifyOtp(Credential credential)
    //    {
    //        Operation<ResponseLogin> result = await otpManager.ValidateOtp(credential.Email, credential.OTP);
    //        return HandleError(result);
    //    }
    //}
}
