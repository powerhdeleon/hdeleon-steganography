using System.Drawing;
using System.Text;

var pathFile = "C:\\Users\\strat\\source\\repos\\hdeleon 2025\\Steganography\\image.png";
var pathFileWithMessage = "C:\\Users\\strat\\source\\repos\\hdeleon 2025\\Steganography\\imageWithMessage.png";
// WriteMessageInPng(pathFile, pathFileWithMessage, "¡A la Mierda JavaScript!");

var message = ExtractMessageFromPng(pathFileWithMessage);
Console.WriteLine(message);

void WriteMessageInPng(string inputImagePath, string outputImagePath, string message)
{
    Bitmap bmp = new Bitmap(inputImagePath);

    byte[] messageBytes = Encoding.UTF8.GetBytes(message);

    byte[] lengthBytes = BitConverter.GetBytes(messageBytes.Length);

    if (!BitConverter.IsLittleEndian)
        Array.Reverse(lengthBytes);

    int totalBitsNeeded = (lengthBytes.Length + messageBytes.Length) * 8;

    if (totalBitsNeeded > bmp.Width * bmp.Height * 3)
        throw new ArgumentException("El mensaje es demasiado grande para esta imagen.");

    int bitIndex = 0;

    foreach (var b in lengthBytes)
        bitIndex = EmbedByte(b, bmp, bitIndex);

    foreach (var b in messageBytes)
        bitIndex = EmbedByte(b, bmp, bitIndex);

    bmp.Save(outputImagePath, System.Drawing.Imaging.ImageFormat.Png);

}

string ExtractMessageFromPng(string inputImagePath)
{
    Bitmap bmp = new Bitmap(inputImagePath);

    int bitIndex = 0;

    byte[] lengthBytes = new byte[4];
    for (int i = 0; i < 4; i++)
        lengthBytes[i] = ExtractByte(bmp, ref bitIndex);

    if (!BitConverter.IsLittleEndian)
        Array.Reverse(lengthBytes);

    int messageLength = BitConverter.ToInt32(lengthBytes, 0);

    byte[] messageBytes = new byte[messageLength];
    for (int i = 0; i < messageLength; i++)
        messageBytes[i] = ExtractByte(bmp, ref bitIndex);

    return Encoding.UTF8.GetString(messageBytes);
}



int EmbedByte(byte b, Bitmap bmp, int bitIndex)
{
    for (int i = 0; i < 8; i++)
    {
        int x = (bitIndex / 3) % bmp.Width;
        int y = (bitIndex / 3) / bmp.Width;

        Color pixel = bmp.GetPixel(x, y);

        int bit = (b >> (7 - i)) & 1;

        switch (bitIndex % 3)
        {
            case 0:
                pixel = Color.FromArgb(pixel.A, (pixel.R & 0xFE) | bit, pixel.G, pixel.B);
                break;
            case 1:
                pixel = Color.FromArgb(pixel.A, pixel.R, (pixel.G & 0xFE) | bit, pixel.B);
                break;
            case 2:
                pixel = Color.FromArgb(pixel.A, pixel.R, pixel.G, (pixel.B & 0xFE) | bit);
                break;
        }

        bmp.SetPixel(x, y, pixel);

        bitIndex++;

    }

    return bitIndex;

}

byte ExtractByte(Bitmap bmp, ref int bitIndex)
{
    byte b = 0;
    for (int i = 0; i < 8; i++)
    {
        int x = (bitIndex / 3) % bmp.Width;
        int y = (bitIndex / 3) / bmp.Width;

        Color pixel = bmp.GetPixel(x, y);

        int bit = 0;
        switch (bitIndex % 3)
        {
            case 0: bit = pixel.R & 1; break;
            case 1: bit = pixel.G & 1; break;
            case 2: bit = pixel.B & 1; break;
        }

        b = (byte)((b << 1) | bit);

        bitIndex++;
    }
    return b;
}
