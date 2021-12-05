using JwtTokenize.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JwtTokenize.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IMembershipService _membershipService;

        public AuthenticationController(IMembershipService membershipService)
        {
            _membershipService = membershipService;
        }

        [HttpPost]
        [Route("Login")]
        [AllowAnonymous]
        public LoginResponse Login(LoginRequest request)
        {
            var member = _membershipService.ValidateUser(request.Email, request.Password);
            if (member == null)
            {
                throw new UnauthorizedAccessException();
            }

            var token = _membershipService.GetToken(member);

            return new LoginResponse { Token = token };
        }
    }
}
