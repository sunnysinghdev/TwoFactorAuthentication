using Microsoft.AspNetCore.Mvc;
using TwoFactorAuthentication.Data;
using System.ComponentModel.DataAnnotations;
namespace TwoFactorAuthentication.Controllers;

[ApiController]
[Route("api/2fa")]
public class TwoFactorAuthController : ControllerBase
{
    private readonly ILogger<TwoFactorAuthController> _logger;
    private readonly int _codeLifetime = 0; //seconds
    private readonly int _concurrentCodesPerPhone = 0;
    private IPhoneCodeRespository _phoneCodeRespository;

    public TwoFactorAuthController(
        ILogger<TwoFactorAuthController> logger,
        IConfiguration configuration,
        IPhoneCodeRespository phoneCodeRespository)
    {
        _logger = logger;
        _codeLifetime = Convert.ToInt32(configuration["CodeLifetime"]);
        _concurrentCodesPerPhone = Convert.ToInt32(configuration["ConcurrentCodesPerPhone"]);
        _phoneCodeRespository = phoneCodeRespository;
    }

    
    [HttpGet("{phone}")]
    public IActionResult Get(string phone)
    {
        return Ok(_phoneCodeRespository.Get(phone));
    }

    [HttpPost("code/send")]
    public IActionResult CodeSend([FromBody] CodeSendRequest request)
    {
        //validate
        // expired
        RemoveExpiredCode(request.Phone);
        // concurent
        var codes = _phoneCodeRespository.Get(request.Phone);
        if (codes.Count < _concurrentCodesPerPhone)
        {
            var r = new Random();
            int code = r.Next(100000, 999999);
            _phoneCodeRespository.Add(request.Phone, code);
            return Ok(new { sent = true });
        }
        else
        {
            return BadRequest(new { sent = false, message = "Error sending code. Please try in sometime." });
        }
        return Ok(request);
    }
    [HttpPost("code/confirm")]
    public IActionResult CodeSend([FromBody] CodeConfirmRequest request)
    {
        if (_phoneCodeRespository.Exists(request.Phone))
        {
            RemoveExpiredCode(request.Phone);
            var codes = _phoneCodeRespository.Get(request.Phone);
            var code = codes.Find(x => x.Code == request.Code);
            if (code != null)
            {
                return Ok(new { valid = true, message = "Code is valid" });
            }
        }
        return BadRequest(new { valid = false, message = "Invalid Code" });
    }
    private void RemoveExpiredCode(string phone)
    {
        if (_phoneCodeRespository.Exists(phone))
            _phoneCodeRespository.Get(phone).RemoveAll(x => x.ExpiryDateTime < DateTime.UtcNow.AddSeconds(-_codeLifetime));
    }
}
public class CodeSendRequest
{
    [Required]
    [RegularExpression(@"^[0-9]{10}$")]
    public string Phone { get; set; }
}

public class CodeConfirmRequest
{
    [Required]
    [RegularExpression(@"^[0-9]{10}$")]
    public string Phone { get; set; }
    [Required]
    [Range(100000, 999999)]
    public int Code { get; set; }
}

