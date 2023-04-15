using System.Collections;
using System.Diagnostics;
using TextCompression.Huffman;
using TextCompression.LZW;

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
        FileResult selectedFile = await FilePicker.Default.PickAsync(PickOptions.Default);
        if (selectedFile == null) return;

        activityIndicator.IsRunning = true;

        await HuffmanExecute(selectedFile);
        await LZWExecute(selectedFile);

        activityIndicator.IsRunning = false;
    }

    private async Task HuffmanExecute(FileResult selectedFile)
    {
        TimeSpan encodeTime;
        TimeSpan decodeTime;
        StreamReader streamReader = new StreamReader(selectedFile.FullPath);
        long originalFileSize = new FileInfo(selectedFile.FullPath).Length;
        string textInput = await streamReader.ReadToEndAsync()!;

        string input = textInput;

        HuffmanTree huffmanTree = new HuffmanTree();
        huffmanTree.Build(input);

        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        infoLabel.Text = "Encoding Via Huffman...";
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
        infoLabel.Text = "Decoding Via Huffman...";
        stopwatch.Restart();
        string decoded = await huffmanTree.Decode(neededDecodedBits);
        stopwatch.Stop();
        decodeTime = stopwatch.Elapsed;

        infoLabel.Text = "Writing decoded Text...";
        await File.WriteAllTextAsync(decodedFile, decoded);
        streamReader.Close();
        long encodedFileSize = new FileInfo(encodedFile).Length;

        encodedTimeForHuffman.Text = $"EncodedTime: {encodeTime}";
        decodedTimeForHuffman.Text = $"DeocodedTime: {decodeTime}";
        originalFileSizeForHuffman.Text = $"Original File Size: {(float)originalFileSize / 1024} Kb";
        encodedFileSizeForHuffman.Text = $"Encoded File Size: {(float)encodedFileSize / 1024} Kb";
    }
    
    private async Task LZWExecute(FileResult selectedFile)
    {
        int padLeftValue = 16;
        TimeSpan encodeTime;
        TimeSpan decodeTime;
        StreamReader streamReader = new StreamReader(selectedFile.FullPath);
        long originalFileSize = new FileInfo(selectedFile.FullPath).Length;
        string textInput = await streamReader.ReadToEndAsync()!;


        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        infoLabel.Text = "Encoding Via LZW...";
        LZW lzw = new LZW();
        List<int> compressedInputs = await lzw.Compress(textInput);
        stopwatch.Stop();
        encodeTime = stopwatch.Elapsed;

        //Encode
        List<bool> encodedBoolean = new List<bool>();
        foreach (int compressedValue in compressedInputs)
        {
            string binaryString = Convert.ToString(compressedValue, 2).PadLeft(padLeftValue, '0');
            foreach (char binaryChar in binaryString)
            {
                if (binaryChar == '0')
                    encodedBoolean.Add(false);
                else
                    encodedBoolean.Add(true);
            }
        }
        BitArray encoded = new BitArray(encodedBoolean.ToArray());
        byte[] encodedBytes = new byte[encoded.Count / 8 + (encoded.Count % 8 == 0 ? 0 : 1)];
        encoded.CopyTo(encodedBytes, 0);
        File.WriteAllBytes(encodedFile, encodedBytes);

        // Decode
        byte[] readByte = await File.ReadAllBytesAsync(encodedFile);
        BitArray decodedBits = new BitArray(readByte);
        string decodedString = "";
        List<int> deCompressedList = new List<int>();

        infoLabel.Text = "Extra Work...";
        await Task.Run(() =>
        {
            foreach (bool bit in decodedBits)
            {
                decodedString += bit ? 1 : 0;
            }

            for (int i = 0; i < decodedString.Length; i += padLeftValue)
            {
                string binaryString = decodedString.Substring(i, padLeftValue);
                int deCompressedInt = Convert.ToInt32(binaryString, 2);
                deCompressedList.Add(deCompressedInt);
            }
        });

        infoLabel.Text = "Decoding Via Lzw...";
        stopwatch.Restart();
        string decompressed = await lzw.Decompress(deCompressedList);
        stopwatch.Stop();
        decodeTime = stopwatch.Elapsed;
        await File.WriteAllTextAsync(decodedFile, decompressed);
        streamReader.Close();

        long encodedFileSize = new FileInfo(encodedFile).Length;
        encodedTimeForLZW.Text = $"EncodedTime: {encodeTime}";
        decodedTimeForLZW.Text = $"DecodedTime: {decodeTime}";
        originalFileSizeForLZW.Text = $"Original File Size: {(float)originalFileSize / 1024} Kb";
        encodedFileSizeForLZW.Text = $"Encoded File Size: {(float)encodedFileSize / 1024} Kb";
    }
}

