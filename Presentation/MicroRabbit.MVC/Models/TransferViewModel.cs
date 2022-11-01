namespace MicroRabbit.MVC.Models;

public class TransferViewModel
{
    public string TransferNotes { get; set; } = default!;
    public int FromAccount { get; set; }
    public int ToAccount { get; set; }
    public decimal TransferAmount { get; set; }
}