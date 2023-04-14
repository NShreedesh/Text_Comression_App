using System.Collections;
using System.Diagnostics;
using TextCompression.Huffman;

namespace TextCompressionApp;

public partial class MainPage : ContentPage
{
    string originalFile = "D:\\Projects\\Realtime  Development\\App\\TextCompressionApp\\Huffman Files\\original.txt";
    string encodedFile = "D:\\Projects\\Realtime  Development\\App\\TextCompressionApp\\Huffman Files\\encoded.txt";
    string decodedFile = "D:\\Projects\\Realtime  Development\\App\\TextCompressionApp\\Huffman Files\\decoded.txt";

    public MainPage()
	{
		InitializeComponent();
	}

    private async void SelectFileClicked(object sender, EventArgs e)
    {
        var selectedFile = await FilePicker.Default.PickAsync(PickOptions.Default);
        if (selectedFile == null) return;

        activityIndicator.IsRunning = true;
        TimeSpan encodeTime;

        StreamReader streamReader = new StreamReader(selectedFile.FullPath);
        long originalFileSize = new FileInfo(selectedFile.FullPath).Length / 1024;
        string textInput = await streamReader.ReadToEndAsync()!;

        string input = textInput;

        HuffmanTree huffmanTree = new HuffmanTree();
        huffmanTree.Build(input);

        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        infoLabel.Text = "Encoding...";
        // Encode
        BitArray encoded = await huffmanTree.Encode(input);
        stopwatch.Stop();
        encodeTime = stopwatch.Elapsed;

        //saving bytes to file
        byte[] encodedBytes = new byte[encoded.Length / 8 + (encoded.Length % 8 == 0 ? 0 : 1)];
        encoded.CopyTo(encodedBytes, 0);
        await File.WriteAllBytesAsync(encodedFile, encodedBytes);

        BitArray decodedBits = new BitArray(encodedBytes);
        BitArray neededDecodedBits = new BitArray(encoded.Length);
        for (int i = 0; i < encoded.Length; i++)
        {
            neededDecodedBits[i] = decodedBits[i];
        }

        // Decode
        infoLabel.Text = "Decoding...";
        string decoded = await huffmanTree.Decode(neededDecodedBits);
        await File.WriteAllTextAsync(decodedFile, decoded);
        streamReader.Close();
        long encodedFileSize = new FileInfo(encodedFile).Length / 1024;

        encodedTimeLabel.Text = $"EncodedTime: {encodeTime.ToString("m\\:ss")}";
        originalFileSizeLabel.Text = $"Original File Size: {originalFileSize}";
        encodedFileSizeLabel.Text = $"Encoded File Size: {encodedFileSize}";

        activityIndicator.IsRunning = false;
    }
}

