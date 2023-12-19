using System.ComponentModel.DataAnnotations;

namespace SoftWell.RtClearing.Moex.Configuration;

public class MoexClearingOptions
{
    /// <summary>
    /// Номер счета, который будет отправляться в ордерах на Московскую Биржу
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string AccountId { get; set; } = null!;

    /// <summary>
    /// Передавать параметр matchref в МБ. None - не передавать. Comment - передавать в поле комментарий. 
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public MatchRefDirection UseMatchRefSource { get; set; } = MatchRefDirection.None;
}