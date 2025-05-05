namespace TycoonTerrain.Utils;

public static class ByteSizeUtils
{
	public static double ConvertBytesToKilobytesRounded( long bytes )
	{
		return System.Math.Round( ConvertBytesToKilobytes( bytes) );
	}
	
	public static double ConvertBytesToKilobytes( long bytes )
	{
		return bytes / 1024f;
	}
}
