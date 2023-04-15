using System.Text;

namespace TextCompression.LZW
{
    public class LZW
    {
        public Task<List<int>> Compress(string uncompressed)
        {
            return Task.Run(() =>
            {
                Dictionary<string, int> dictionary = new Dictionary<string, int>();
                for (int i = 0; i < 256; i++)
                    dictionary.Add(((char)i).ToString(), i);

                string w = string.Empty;
                List<int> compressed = new List<int>();

                foreach (char c in uncompressed)
                {
                    string wc = w + c;
                    if (dictionary.ContainsKey(wc))
                    {
                        w = wc;
                    }
                    else
                    {
                        compressed.Add(dictionary[w]);
                        dictionary.Add(wc, dictionary.Count);
                        w = c.ToString();
                    }
                }

                if (!string.IsNullOrEmpty(w))
                    compressed.Add(dictionary[w]);

                return compressed;
            });
        }

        public Task<string> Decompress(List<int> compressed)
        {
            return Task.Run(() =>
            {
                Dictionary<int, string> dictionary = new Dictionary<int, string>();
                for (int i = 0; i < 256; i++)
                    dictionary.Add(i, ((char)i).ToString());

                string w = dictionary[compressed[0]];
                compressed.RemoveAt(0);

                StringBuilder decompressed = new StringBuilder(w);

                foreach (int k in compressed)
                {
                    string entry = String.Empty;
                    if (dictionary.ContainsKey(k))
                        entry = dictionary[k];
                    else if (k == dictionary.Count)
                        entry = w + w[0];

                    decompressed.Append(entry);
                    dictionary.Add(dictionary.Count, w + entry[0]);

                    w = entry;
                }

                return decompressed.ToString();
            });
        }
    }
}