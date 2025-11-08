using System.ComponentModel.DataAnnotations.Schema;

namespace CustomerOpinionsETL.Domain.Entities;

/// <summary>
/// Dimensión de Canal - Describe el origen de la opinión
/// </summary>
public class DimChannel
{
    [Column("channel_key")]
    public int ChannelKey { get; set; }

    [Column("channel_name")]
    public string ChannelName { get; set; } = string.Empty;

    [Column("channel_type")]
    public string ChannelType { get; set; } = string.Empty;
}
