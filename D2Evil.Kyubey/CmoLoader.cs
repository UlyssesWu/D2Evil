using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace D2Evil.Kyubey
{
    //TODO: They re-invented a (ugly) wheel for binary XML... WTF
    //MyXmlIO / 0x62732430 (1651713072) / 0x12345678
    class CmoLoader
    {
        static readonly byte[] Signature = { 148, 132, 242, 151 };

        public static void Decompress(string path)
        {
            var bts = File.ReadAllBytes(path);
            var sig = bts.Take(4);

            if (!sig.SequenceEqual(Signature))
            {
                throw new BadImageFormatException("Not a valid CMO file.");
            }
            LJRandom r = new LJRandom((ulong)(bts.Length - 4)); //aji... I mean, "clever" stream cipher
            for (int i = 4; i < bts.Length; i += 3)
            {
                int m = r.NextInt();
                bts[i] = ((byte)(bts[i] ^ m));
            }

            using (var ms = new MemoryStream(bts, 4, bts.Length - 4))
            {
                ZipArchive zip = new ZipArchive(ms);
                zip.ExtractToDirectory(Path.ChangeExtension(path, null));
            }
        }
    }
}
