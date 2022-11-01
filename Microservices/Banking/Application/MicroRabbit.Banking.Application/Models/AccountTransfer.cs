namespace MicroRabbit.Banking.Application.Models;

public class AccountTransfer
{
    public int AccountFrom { get; set; }
    public int AccountTo { get; set; }
    public decimal TransferAmount { get; set; }
}