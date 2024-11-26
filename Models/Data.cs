using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TestCore9.Models;

/// <summary>
/// 
/// </summary>
public class Data
{
    [property: Required]
    [property: Description("姓名")]    
    public string Name { get; set; } = default!;

    [property: Required]
    [property: Description("年齡")]    
    public int Age { get; set; }
}