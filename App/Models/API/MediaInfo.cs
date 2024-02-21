namespace App.Models.API;

public struct MediaInfo
{
    public MediaType MediaType;
    public byte[] MediaContentBytes;
    public byte[] ThumbnailContentBytes;
    public int? Width;
    public int? Height;
    public double Duration;
}