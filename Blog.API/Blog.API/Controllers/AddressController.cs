using Blog.API.Infrastucture.Email;
using Blog.API.Models.DTOs;
using Blog.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;



namespace Blog.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AddressController : ControllerBase
    {
        private IAddressService _addressService;
        private IEmailSender _emailSender;
    
        public AddressController(IAddressService addressService, IEmailSender emailSender)
        {
            _addressService = addressService;
            _emailSender = emailSender;

        }
        
        [AllowAnonymous]
        [HttpGet("search")]
        public async Task<ActionResult<List<SearchAddressModel>>> SearchAddress(Int64 parentObjectId, string? query)
        {
            var response = await _addressService.SearchAddress(parentObjectId, query);
            return Ok(response);
        }
        
        [AllowAnonymous]
        [HttpGet("chain")]
        public async Task<ActionResult<List<SearchAddressModel>>> GetAddressChain(Guid objectGuid)
        {
            var response = await _addressService.GetAddressChain(objectGuid);
            return Ok(response);
        }
        
        [AllowAnonymous]
        [HttpGet("SendEmail")]
        public async Task<ActionResult<string>> GetAddressChain(string email, string subject, string body)
        {
            await _emailSender.SendEmailAsync(email, subject, body);
            return Ok("Sent");

        }
        
    }
}