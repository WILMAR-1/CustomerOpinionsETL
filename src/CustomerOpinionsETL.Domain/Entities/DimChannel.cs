namespace CustomerOpinionsETL.Domain.Entities;

/// <summary>
/// Dimensión de Canal - Describe el origen de la opinión
/// </summary>
public class DimChannel
{
    public int ChannelKey { get; set; }
    public string ChannelName { get; set; } = string.Empty;
    public string ChannelType { get; set; } = string.Empty;
}
