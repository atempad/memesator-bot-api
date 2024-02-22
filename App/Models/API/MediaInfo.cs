namespace App.Models.API;

public struct MediaInfo
{
    public MediaData Data;
    public MediaData ThumbnailData;
    public int? Width;
    public int? Height;
    public double Duration;
}