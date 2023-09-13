using System.ComponentModel.DataAnnotations;

namespace SoftWell.RtClearing.Moex.Configuration;

public class MoexClearingOptions
{
    /// <summary>
    /// Номер счета, который будет отправляться в ордерах на Московскую Биржу
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string AccountId { get; set; } = null!;
}