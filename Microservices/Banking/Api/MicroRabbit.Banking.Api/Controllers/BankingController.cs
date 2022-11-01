using MicroRabbit.Banking.Application.Interfaces;
using MicroRabbit.Banking.Application.Models;
using MicroRabbit.Banking.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace MicroRabbit.Banking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BankingController : ControllerBase
{
    private readonly IAccountService _accountService;

    public BankingController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    // GET api/banking
    [HttpGet]
    public ActionResult<IEnumerable<Account>> Get()
    {
        return Ok(_accountService.GetAccounts());
    }

    [HttpPost]
    public ActionResult Post([FromBody] AccountTransfer accountTransfer)
    {
        _accountService.Transfer(accountTransfer);
        return Ok(accountTransfer);
    }
}