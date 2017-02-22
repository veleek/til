# Importing Heightmap Data from Global Data Explorer into Unity

1. Export data from GDEX (https://gdex.cr.usgs.gov/gdex/)
2. Apply levels to data to make it a bit 'shorter'
3. Save the image as a bitmap.
4. Convert the bitmap to a RAW image

    ```csharp
    /// <summary>
    /// Convert a bitmap to a byte array
    /// </summary>
    /// <param name="bitmap">image to convert</param>
    /// <returns>image as bytes</returns>
    private static byte[] ConvertBitmapToRaw(Bitmap bitmap)
    {
      //Code excerpted from Microsoft Robotics Studio v1.5
      BitmapData raw = null;  //used to get attributes of the image
      byte[] rawImage = null; //the image as a byte[]

      try
      {
        //Freeze the image in memory
        raw = bitmap.LockBits(
          new Rectangle(0, 0, (int)bitmap.Width, (int)bitmap.Height),
          ImageLockMode.ReadOnly,
          PixelFormat.Format8bppIndexed
        );

        int size = raw.Height * raw.Width;
        rawImage = new byte[size];

        int rem = raw.Width % 4;
        int padding = rem == 0 ? 0 : 4 - rem;

        int basePtr = raw.Scan0.ToInt32();
        //Copy the image into the byte[]
        for (int r = 0; r < raw.Height; r++)
        {
          int srcOffset = r * (raw.Width + padding);
          int dstOffset = r * raw.Width;
          Marshal.Copy(new IntPtr(basePtr + srcOffset), rawImage, dstOffset, raw.Width);
        }
      }
      finally
      {
        if (raw != null)
        {
          //Unfreeze the memory for the image
          bitmap.UnlockBits(raw);
        }
      }
      return rawImage;
    }
    ```
    
5. Import the RAW image into Unity.
    * It should detect the settings.
6. Download GoogleMaps data for the given area.
