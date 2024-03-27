public static class Constants
{
    public const int TEXTURE_PPU = 16;
    public const int CHUNK_SIZE_PIXELS = 64;
    public const int CHUNK_SIZE_UNITS = CHUNK_SIZE_PIXELS / TEXTURE_PPU;
    public const int CHUNK_SIZE_UNITS_BITMASK = CHUNK_SIZE_UNITS - 1;
    
    public const int STATION_DEPTH_INTERVAL = 200;
}