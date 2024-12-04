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
        private  IAddressService _addressService;
    
        public AddressController(IAddressService addressService)
        {
            _addressService = addressService;
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
        
    }
}