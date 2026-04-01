using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace D2Evil.Kyubey
{
    public sealed class CaffArchive
    {
        public const string ManifestFileName = ".archive.xml";
        public const string PreviewTag = "preview";
        public const string MainXmlTag = "main_xml";

        private const string ArchiveIdentifier = "CAFF";
        private const long StartPositionPlaceholder = 0x1234567812345678L;
        private const byte GuardByte98 = 98;
        private const byte GuardByte99 = 99;

        private readonly List<CaffFileEntry> _files = new List<CaffFileEntry>();
        private readonly Dictionary<string, CaffFileEntry> _filesByPath =
            new Dictionary<string, CaffFileEntry>(StringComparer.Ordinal);

        public CaffArchive()
        {
            Header = new CaffHeader();
            PreviewImage = new CaffPreviewImageInfo();
        }

        public CaffHeader Header { get; }

        public CaffPreviewImageInfo PreviewImage { get; }

        public IReadOnlyList<CaffFileEntry> Files => _files;

        public static CaffArchive Load(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(path));
            }

            return Load(File.ReadAllBytes(path));
        }

        public static CaffArchive Load(byte[] archiveBytes)
        {
            if (archiveBytes == null)
            {
                throw new ArgumentNullException(nameof(archiveBytes));
            }

            var archive = new CaffArchive();
            archive.ReadArchive(archiveBytes);
            return archive;
        }

        public static CaffArchive LoadFromExtractedDirectory(string extractedDirectory)
        {
            if (string.IsNullOrWhiteSpace(extractedDirectory))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(extractedDirectory));
            }

            string manifestPath = Path.Combine(extractedDirectory, ManifestFileName);
            if (!File.Exists(manifestPath))
            {
                throw new FileNotFoundException("CAFF manifest not found.", manifestPath);
            }

            XDocument document = XDocument.Load(manifestPath);
            XElement root = document.Root ?? throw new InvalidDataException("CAFF manifest is missing the root element.");

            var archive = new CaffArchive();
            archive.Header.ArchiveIdentifier = GetRequiredAttribute(root, "archive_id");
            ParseVersion(GetRequiredAttribute(root, "archive_version"), archive.Header.ArchiveVersion);
            archive.Header.FormatIdentifier = GetRequiredAttribute(root, "format_id");
            ParseVersion(GetRequiredAttribute(root, "format_version"), archive.Header.FormatVersion);
            archive.Header.ObfuscateKey = ParseInt32(GetRequiredAttribute(root, "obfuscateKey"), "obfuscateKey");

            XElement filesElement = root.Element("files") ?? throw new InvalidDataException("CAFF manifest is missing the files element.");
            foreach (XElement fileElement in filesElement.Elements("file"))
            {
                string filePath = GetRequiredAttribute(fileElement, "filePath");
                string tag = (string)fileElement.Attribute("tag") ?? string.Empty;
                bool isObfuscated = ParseBoolean(GetRequiredAttribute(fileElement, "isObfuscated"), "isObfuscated");
                CaffCompressOption compressOption = ParseCompressOption(GetRequiredAttribute(fileElement, "compressOption"));

                if (StringComparer.Ordinal.Equals(tag, PreviewTag))
                {
                    isObfuscated = false;
                    compressOption = CaffCompressOption.RAW;
                }

                string fileOnDisk = ResolveArchivePath(extractedDirectory, filePath);
                if (!File.Exists(fileOnDisk))
                {
                    throw new FileNotFoundException("CAFF file entry is missing on disk.", fileOnDisk);
                }

                archive.AddFile(new CaffFileEntry(filePath, tag, File.ReadAllBytes(fileOnDisk), isObfuscated, compressOption));
            }

            archive.RefreshPreviewMetadata(archive.GetPreviewEntry());
            return archive;
        }

        public static void Extract(string inputPath, string outputDirectory)
        {
            Load(inputPath).ExtractToDirectory(outputDirectory);
        }

        public static void Extract(byte[] archiveBytes, string outputDirectory)
        {
            Load(archiveBytes).ExtractToDirectory(outputDirectory);
        }

        public static void PackDirectory(string extractedDirectory, string outputPath)
        {
            LoadFromExtractedDirectory(extractedDirectory).Save(outputPath);
        }

        public static byte[] PackDirectoryToBytes(string extractedDirectory)
        {
            return LoadFromExtractedDirectory(extractedDirectory).ToBytes();
        }

        public void AddFile(CaffFileEntry entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException(nameof(entry));
            }

            string normalizedPath = NormalizeArchivePath(entry.FilePath);
            if (_filesByPath.ContainsKey(normalizedPath))
            {
                throw new InvalidOperationException("Duplicate CAFF file path: " + normalizedPath);
            }

            entry.FilePath = normalizedPath;
            entry.Tag = entry.Tag ?? string.Empty;
            entry.Content = entry.Content ?? Array.Empty<byte>();
            _files.Add(entry);
            _filesByPath.Add(normalizedPath, entry);
        }

        public CaffFileEntry GetFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(filePath));
            }

            string normalizedPath = NormalizeArchivePath(filePath);
            if (!_filesByPath.TryGetValue(normalizedPath, out CaffFileEntry entry))
            {
                throw new KeyNotFoundException("CAFF file entry not found: " + normalizedPath);
            }

            return entry;
        }

        public void ExtractToDirectory(string outputDirectory)
        {
            if (string.IsNullOrWhiteSpace(outputDirectory))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(outputDirectory));
            }

            Directory.CreateDirectory(outputDirectory);
            foreach (CaffFileEntry entry in _files)
            {
                string outputPath = ResolveArchivePath(outputDirectory, entry.FilePath);
                string parentDirectory = Path.GetDirectoryName(outputPath);
                if (!string.IsNullOrEmpty(parentDirectory))
                {
                    Directory.CreateDirectory(parentDirectory);
                }

                File.WriteAllBytes(outputPath, entry.Content ?? Array.Empty<byte>());
            }

            WriteManifest(outputDirectory);
        }

        public void Save(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(path));
            }

            string fullPath = Path.GetFullPath(path);
            string directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllBytes(fullPath, ToBytes());
        }

        public byte[] ToBytes()
        {
            ValidateHeader();

            CaffFileEntry previewEntry = GetPreviewEntry();
            RefreshPreviewMetadata(previewEntry);
            List<PreparedEntry> preparedEntries = PrepareEntries(previewEntry);

            using (var memoryStream = new MemoryStream())
            {
                var writer = new CaffBinaryWriter(memoryStream);
                WriteArchive(writer, preparedEntries, previewEntry);
                return memoryStream.ToArray();
            }
        }

        private void ReadArchive(byte[] archiveBytes)
        {
            _files.Clear();
            _filesByPath.Clear();

            var reader = new CaffBinaryReader(archiveBytes);
            Header.ArchiveIdentifier = new string(new[]
            {
                (char)reader.ReadByte(0),
                (char)reader.ReadByte(0),
                (char)reader.ReadByte(0),
                (char)reader.ReadByte(0)
            });

            if (!StringComparer.Ordinal.Equals(Header.ArchiveIdentifier, ArchiveIdentifier))
            {
                throw new InvalidDataException("Not a valid CAFF archive.");
            }

            Header.ArchiveVersion[0] = reader.ReadByte(0) & 0xFF;
            Header.ArchiveVersion[1] = reader.ReadByte(0) & 0xFF;
            Header.ArchiveVersion[2] = reader.ReadByte(0) & 0xFF;

            Header.FormatIdentifier = new string(new[]
            {
                (char)reader.ReadByte(0),
                (char)reader.ReadByte(0),
                (char)reader.ReadByte(0),
                (char)reader.ReadByte(0)
            });

            Header.FormatVersion[0] = reader.ReadByte(0) & 0xFF;
            Header.FormatVersion[1] = reader.ReadByte(0) & 0xFF;
            Header.FormatVersion[2] = reader.ReadByte(0) & 0xFF;
            Header.ObfuscateKey = reader.ReadInt32(0);

            int obfuscateKey = Header.ObfuscateKey;
            reader.Skip(8);

            PreviewImage.ImageFormat = ParseImageType(reader.ReadByte(0));
            PreviewImage.ColorType = ParseColorType(reader.ReadByte(0));
            reader.Skip(2);
            PreviewImage.Width = unchecked((ushort)reader.ReadInt16(0));
            PreviewImage.Height = unchecked((ushort)reader.ReadInt16(0));
            PreviewImage.StartPosition = reader.ReadInt64(0);
            PreviewImage.FileSize = reader.ReadInt32(0);
            reader.Skip(8);

            int fileCount = reader.ReadInt32(obfuscateKey);
            if (fileCount < 0)
            {
                throw new InvalidDataException("Invalid CAFF file count.");
            }

            for (int index = 0; index < fileCount; index++)
            {
                string filePath = reader.ReadString(obfuscateKey);
                string tag = reader.ReadString(obfuscateKey);
                long startPosition = reader.ReadInt64(obfuscateKey);
                int storedSize = reader.ReadInt32(obfuscateKey);
                bool isObfuscated = reader.ReadBoolean(obfuscateKey);
                CaffCompressOption compressOption = ParseCompressOption(reader.ReadByte(obfuscateKey));
                reader.Skip(8);

                var entry = new CaffFileEntry(filePath, tag, Array.Empty<byte>(), isObfuscated, compressOption)
                {
                    StartPosition = startPosition,
                    StoredSize = storedSize
                };

                AddFile(entry);
            }

            foreach (CaffFileEntry entry in _files)
            {
                reader.Position = entry.StartPosition;
                byte[] storedBytes = reader.ReadBytes(entry.StoredSize, entry.IsObfuscated ? obfuscateKey : 0);
                try
                {
                    entry.Content = entry.CompressOption == CaffCompressOption.RAW ? storedBytes : Inflate(storedBytes);
                }
                catch (Exception ex)
                {
                    throw new InvalidDataException(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "Failed to decode CAFF entry '{0}' (compress={1}, start=0x{2:X8}, size={3}, head={4}).",
                            entry.FilePath,
                            entry.CompressOption,
                            entry.StartPosition,
                            entry.StoredSize,
                            BitConverter.ToString(storedBytes.Take(Math.Min(8, storedBytes.Length)).ToArray())),
                        ex);
                }
            }

            if (archiveBytes.Length < 2 || archiveBytes[archiveBytes.Length - 2] != GuardByte98 || archiveBytes[archiveBytes.Length - 1] != GuardByte99)
            {
                throw new InvalidDataException("Invalid CAFF guard bytes.");
            }
        }

        private void WriteArchive(CaffBinaryWriter writer, List<PreparedEntry> preparedEntries, CaffFileEntry previewEntry)
        {
            int obfuscateKey = Header.ObfuscateKey;

            WriteIdentifier(writer, Header.ArchiveIdentifier);
            WriteVersion(writer, Header.ArchiveVersion);
            WriteIdentifier(writer, Header.FormatIdentifier);
            WriteVersion(writer, Header.FormatVersion);
            writer.WriteInt32(Header.ObfuscateKey, 0);
            writer.Skip(8);

            writer.WriteByte((byte)PreviewImage.ImageFormat, 0);
            writer.WriteByte((byte)PreviewImage.ColorType, 0);
            writer.Skip(2);
            writer.WriteInt16(unchecked((short)PreviewImage.Width), 0);
            writer.WriteInt16(unchecked((short)PreviewImage.Height), 0);
            long previewStartPositionAddress = writer.Position;
            writer.WriteInt64(0L, 0);
            writer.WriteInt32(PreviewImage.FileSize, 0);
            writer.Skip(8);

            writer.WriteInt32(preparedEntries.Count, obfuscateKey);
            foreach (PreparedEntry preparedEntry in preparedEntries)
            {
                writer.WriteString(preparedEntry.Entry.FilePath, obfuscateKey);
                writer.WriteString(preparedEntry.Entry.Tag ?? string.Empty, obfuscateKey);
                preparedEntry.StartPositionAddress = writer.Position;
                writer.WriteInt64(StartPositionPlaceholder, obfuscateKey);
                writer.WriteInt32(preparedEntry.Entry.StoredSize, obfuscateKey);
                writer.WriteBoolean(preparedEntry.Entry.IsObfuscated, obfuscateKey);
                writer.WriteByte((byte)preparedEntry.Entry.CompressOption, obfuscateKey);
                writer.Skip(8);
            }

            foreach (PreparedEntry preparedEntry in preparedEntries)
            {
                preparedEntry.Entry.StartPosition = writer.Position;
                writer.WriteBytes(preparedEntry.StoredData, 0, preparedEntry.StoredData.Length,
                    preparedEntry.Entry.IsObfuscated ? obfuscateKey : 0);
            }

            writer.WriteByte(GuardByte98, 0);
            writer.WriteByte(GuardByte99, 0);

            foreach (PreparedEntry preparedEntry in preparedEntries)
            {
                writer.Position = preparedEntry.StartPositionAddress;
                writer.WriteInt64(preparedEntry.Entry.StartPosition, obfuscateKey);
            }

            if (previewEntry != null)
            {
                writer.Position = previewStartPositionAddress;
                writer.WriteInt64(previewEntry.StartPosition, 0);
                PreviewImage.StartPosition = previewEntry.StartPosition;
                PreviewImage.FileSize = previewEntry.StoredSize;
            }
        }

        private List<PreparedEntry> PrepareEntries(CaffFileEntry previewEntry)
        {
            var preparedEntries = new List<PreparedEntry>(_files.Count);
            foreach (CaffFileEntry entry in _files)
            {
                if (ReferenceEquals(entry, previewEntry))
                {
                    entry.IsObfuscated = false;
                    entry.CompressOption = CaffCompressOption.RAW;
                }

                byte[] content = entry.Content ?? Array.Empty<byte>();
                byte[] storedData = entry.CompressOption == CaffCompressOption.RAW ?
                    CloneBytes(content) : Deflate(content, entry.CompressOption);
                entry.StoredSize = storedData.Length;
                preparedEntries.Add(new PreparedEntry(entry, storedData));
            }

            if (previewEntry != null)
            {
                PreviewImage.FileSize = previewEntry.StoredSize;
            }

            return preparedEntries;
        }

        private void RefreshPreviewMetadata(CaffFileEntry previewEntry)
        {
            if (previewEntry == null)
            {
                PreviewImage.ImageFormat = CaffImageType.NO_PREVIEW;
                PreviewImage.ColorType = CaffColorType.NO_PREVIEW;
                PreviewImage.Width = 0;
                PreviewImage.Height = 0;
                PreviewImage.StartPosition = 0;
                PreviewImage.FileSize = 0;
                return;
            }

            PngMetadata pngMetadata = ReadPngMetadata(previewEntry.Content ?? Array.Empty<byte>());
            PreviewImage.ImageFormat = CaffImageType.PNG;
            PreviewImage.ColorType = pngMetadata.HasAlpha ? CaffColorType.ARGB : CaffColorType.RGB;
            PreviewImage.Width = pngMetadata.Width;
            PreviewImage.Height = pngMetadata.Height;
            PreviewImage.StartPosition = previewEntry.StartPosition;
            PreviewImage.FileSize = previewEntry.StoredSize;
        }

        private void WriteManifest(string outputDirectory)
        {
            var root = new XElement("root",
                new XAttribute("archive_id", Header.ArchiveIdentifier),
                new XAttribute("archive_version", FormatVersion(Header.ArchiveVersion)),
                new XAttribute("format_id", Header.FormatIdentifier),
                new XAttribute("format_version", FormatVersion(Header.FormatVersion)),
                new XAttribute("obfuscateKey", Header.ObfuscateKey.ToString(CultureInfo.InvariantCulture)));

            root.Add(new XElement("preview",
                new XAttribute("imageFormat", PreviewImage.ImageFormat.ToString()),
                new XAttribute("colorType", PreviewImage.ColorType.ToString()),
                new XAttribute("width", PreviewImage.Width.ToString(CultureInfo.InvariantCulture)),
                new XAttribute("height", PreviewImage.Height.ToString(CultureInfo.InvariantCulture)),
                new XAttribute("startPos", FormatOffset(PreviewImage.StartPosition)),
                new XAttribute("fileSize", PreviewImage.FileSize.ToString(CultureInfo.InvariantCulture))));

            var files = new XElement("files");
            foreach (CaffFileEntry entry in _files)
            {
                files.Add(new XElement("file",
                    new XAttribute("filePath", entry.FilePath),
                    new XAttribute("tag", entry.Tag ?? string.Empty),
                    new XAttribute("startPos", FormatOffset(entry.StartPosition)),
                    new XAttribute("fileSize", entry.StoredSize.ToString(CultureInfo.InvariantCulture)),
                    new XAttribute("isObfuscated", entry.IsObfuscated ? "true" : "false"),
                    new XAttribute("compressOption", entry.CompressOption.ToString())));
            }

            root.Add(files);
            var document = new XDocument(root);
            document.Save(Path.Combine(outputDirectory, ManifestFileName));
        }

        private CaffFileEntry GetPreviewEntry()
        {
            CaffFileEntry previewEntry = null;
            foreach (CaffFileEntry entry in _files)
            {
                if (!StringComparer.Ordinal.Equals(entry.Tag, PreviewTag))
                {
                    continue;
                }

                if (previewEntry != null)
                {
                    throw new InvalidOperationException("CAFF archives support only one preview entry.");
                }

                previewEntry = entry;
            }

            return previewEntry;
        }

        private void ValidateHeader()
        {
            ValidateIdentifier(Header.ArchiveIdentifier, nameof(Header.ArchiveIdentifier));
            ValidateVersion(Header.ArchiveVersion, nameof(Header.ArchiveVersion));
            ValidateIdentifier(Header.FormatIdentifier, nameof(Header.FormatIdentifier));
            ValidateVersion(Header.FormatVersion, nameof(Header.FormatVersion));
        }

        private static void WriteIdentifier(CaffBinaryWriter writer, string identifier)
        {
            ValidateIdentifier(identifier, nameof(identifier));
            for (int index = 0; index < identifier.Length; index++)
            {
                writer.WriteByte(checked((byte)identifier[index]), 0);
            }
        }

        private static void WriteVersion(CaffBinaryWriter writer, int[] version)
        {
            ValidateVersion(version, nameof(version));
            writer.WriteByte(checked((byte)version[0]), 0);
            writer.WriteByte(checked((byte)version[1]), 0);
            writer.WriteByte(checked((byte)version[2]), 0);
        }

        private static void ValidateIdentifier(string identifier, string parameterName)
        {
            if (string.IsNullOrEmpty(identifier) || identifier.Length != 4)
            {
                throw new InvalidOperationException(parameterName + " must be exactly 4 characters.");
            }

            foreach (char value in identifier)
            {
                if (value > byte.MaxValue)
                {
                    throw new InvalidOperationException(parameterName + " must contain only single-byte characters.");
                }
            }
        }

        private static void ValidateVersion(int[] version, string parameterName)
        {
            if (version == null || version.Length != 3)
            {
                throw new InvalidOperationException(parameterName + " must contain exactly 3 values.");
            }

            foreach (int value in version)
            {
                if (value < 0 || value > byte.MaxValue)
                {
                    throw new InvalidOperationException(parameterName + " values must be between 0 and 255.");
                }
            }
        }

        private static string GetRequiredAttribute(XElement element, string attributeName)
        {
            XAttribute attribute = element.Attribute(attributeName);
            if (attribute == null)
            {
                throw new InvalidDataException("Missing required CAFF manifest attribute: " + attributeName);
            }

            return attribute.Value;
        }

        private static void ParseVersion(string value, int[] target)
        {
            string[] parts = value.Split('.');
            if (parts.Length != 3)
            {
                throw new InvalidDataException("Invalid CAFF version string: " + value);
            }

            for (int index = 0; index < 3; index++)
            {
                target[index] = ParseInt32(parts[index], value);
                if (target[index] < 0 || target[index] > byte.MaxValue)
                {
                    throw new InvalidDataException("CAFF version components must be between 0 and 255.");
                }
            }
        }

        private static int ParseInt32(string value, string fieldName)
        {
            if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result))
            {
                throw new InvalidDataException("Invalid integer value for " + fieldName + ": " + value);
            }

            return result;
        }

        private static bool ParseBoolean(string value, string fieldName)
        {
            if (!bool.TryParse(value, out bool result))
            {
                throw new InvalidDataException("Invalid boolean value for " + fieldName + ": " + value);
            }

            return result;
        }

        private static CaffCompressOption ParseCompressOption(string value)
        {
            if (!Enum.TryParse(value, false, out CaffCompressOption compressOption))
            {
                throw new InvalidDataException("Unknown CAFF compress option: " + value);
            }

            return ParseCompressOption((byte)compressOption);
        }

        private static CaffCompressOption ParseCompressOption(byte value)
        {
            switch (value)
            {
                case (byte)CaffCompressOption.RAW:
                    return CaffCompressOption.RAW;
                case (byte)CaffCompressOption.FAST:
                    return CaffCompressOption.FAST;
                case (byte)CaffCompressOption.SMALL:
                    return CaffCompressOption.SMALL;
                default:
                    throw new InvalidDataException("Unknown CAFF compress option: " + value);
            }
        }

        private static CaffColorType ParseColorType(byte value)
        {
            switch (value)
            {
                case (byte)CaffColorType.NOT_INIT:
                    return CaffColorType.NOT_INIT;
                case (byte)CaffColorType.ARGB:
                    return CaffColorType.ARGB;
                case (byte)CaffColorType.RGB:
                    return CaffColorType.RGB;
                case (byte)CaffColorType.NO_PREVIEW:
                    return CaffColorType.NO_PREVIEW;
                default:
                    throw new InvalidDataException("Unknown CAFF preview color type: " + value);
            }
        }

        private static CaffImageType ParseImageType(byte value)
        {
            switch (value)
            {
                case (byte)CaffImageType.NOT_INIT:
                    return CaffImageType.NOT_INIT;
                case (byte)CaffImageType.PNG:
                    return CaffImageType.PNG;
                case (byte)CaffImageType.NO_PREVIEW:
                    return CaffImageType.NO_PREVIEW;
                default:
                    throw new InvalidDataException("Unknown CAFF preview image type: " + value);
            }
        }

        private static byte[] Inflate(byte[] storedBytes)
        {
            try
            {
                using (var input = new MemoryStream(storedBytes))
                using (var zip = new ZipArchive(input, ZipArchiveMode.Read, false))
                {
                    ZipArchiveEntry entry = zip.Entries.FirstOrDefault();
                    if (entry == null)
                    {
                        return Array.Empty<byte>();
                    }

                    using (Stream entryStream = entry.Open())
                    using (var output = new MemoryStream())
                    {
                        entryStream.CopyTo(output);
                        return output.ToArray();
                    }
                }
            }
            catch (InvalidDataException)
            {
                return InflateStreamingZip(storedBytes);
            }
        }

        private static byte[] Deflate(byte[] content, CaffCompressOption compressOption)
        {
            using (var output = new MemoryStream())
            {
                using (var zip = new ZipArchive(output, ZipArchiveMode.Create, true))
                {
                    ZipArchiveEntry entry = zip.CreateEntry("contents", GetCompressionLevel(compressOption));
                    using (Stream entryStream = entry.Open())
                    {
                        entryStream.Write(content, 0, content.Length);
                    }
                }

                return output.ToArray();
            }
        }

        private static byte[] InflateStreamingZip(byte[] storedBytes)
        {
            if (storedBytes == null || storedBytes.Length < 30)
            {
                throw new InvalidDataException("Compressed CAFF entry is too small to contain a ZIP local header.");
            }

            const uint localHeaderSignature = 0x04034B50;
            const uint dataDescriptorSignature = 0x08074B50;

            if (ReadLittleEndianUInt32(storedBytes, 0) != localHeaderSignature)
            {
                throw new InvalidDataException("Compressed CAFF entry does not start with a ZIP local header.");
            }

            ushort flags = ReadLittleEndianUInt16(storedBytes, 6);
            ushort compressionMethod = ReadLittleEndianUInt16(storedBytes, 8);
            ushort fileNameLength = ReadLittleEndianUInt16(storedBytes, 26);
            ushort extraLength = ReadLittleEndianUInt16(storedBytes, 28);
            int dataOffset = 30 + fileNameLength + extraLength;
            if (dataOffset > storedBytes.Length)
            {
                throw new InvalidDataException("Compressed CAFF entry has an invalid ZIP local header.");
            }

            int compressedSize;
            if ((flags & 0x0008) == 0)
            {
                compressedSize = unchecked((int)ReadLittleEndianUInt32(storedBytes, 18));
            }
            else
            {
                int descriptorLength = 12;
                if (storedBytes.Length >= dataOffset + 16 &&
                    ReadLittleEndianUInt32(storedBytes, storedBytes.Length - 16) == dataDescriptorSignature)
                {
                    descriptorLength = 16;
                }

                compressedSize = storedBytes.Length - dataOffset - descriptorLength;
            }

            if (compressedSize < 0 || dataOffset + compressedSize > storedBytes.Length)
            {
                throw new InvalidDataException("Compressed CAFF entry has an invalid compressed payload length.");
            }

            if (compressionMethod == 0)
            {
                var rawData = new byte[compressedSize];
                Buffer.BlockCopy(storedBytes, dataOffset, rawData, 0, compressedSize);
                return rawData;
            }

            if (compressionMethod != 8)
            {
                throw new InvalidDataException("Unsupported ZIP compression method in CAFF entry: " + compressionMethod);
            }

            using (var compressedStream = new MemoryStream(storedBytes, dataOffset, compressedSize, false))
            using (var deflateStream = new DeflateStream(compressedStream, CompressionMode.Decompress, false))
            using (var output = new MemoryStream())
            {
                deflateStream.CopyTo(output);
                return output.ToArray();
            }
        }

        private static CompressionLevel GetCompressionLevel(CaffCompressOption compressOption)
        {
            switch (compressOption)
            {
                case CaffCompressOption.FAST:
                    return CompressionLevel.Fastest;
                case CaffCompressOption.SMALL:
#if NET8_0_OR_GREATER
                    return CompressionLevel.SmallestSize;
#else
                    return CompressionLevel.Optimal;
#endif
                case CaffCompressOption.RAW:
                default:
                    return CompressionLevel.NoCompression;
            }
        }

        private static PngMetadata ReadPngMetadata(byte[] pngBytes)
        {
            byte[] signature = { 137, 80, 78, 71, 13, 10, 26, 10 };
            if (pngBytes == null || pngBytes.Length < 33 || !signature.SequenceEqual(pngBytes.Take(signature.Length)))
            {
                throw new InvalidDataException("Preview entries must contain a valid PNG image.");
            }

            bool hasHeader = false;
            bool hasTransparency = false;
            int width = 0;
            int height = 0;
            byte colorType = 0;
            int offset = signature.Length;

            while (offset + 12 <= pngBytes.Length)
            {
                int chunkLength = ReadBigEndianInt32(pngBytes, offset);
                offset += 4;
                if (chunkLength < 0 || offset + 4 + chunkLength + 4 > pngBytes.Length)
                {
                    throw new InvalidDataException("Preview PNG is malformed.");
                }

                string chunkType = Encoding.ASCII.GetString(pngBytes, offset, 4);
                offset += 4;
                if (chunkType == "IHDR")
                {
                    if (chunkLength < 13)
                    {
                        throw new InvalidDataException("Preview PNG header is malformed.");
                    }

                    width = ReadBigEndianInt32(pngBytes, offset);
                    height = ReadBigEndianInt32(pngBytes, offset + 4);
                    colorType = pngBytes[offset + 9];
                    hasHeader = true;
                }
                else if (chunkType == "tRNS")
                {
                    hasTransparency = true;
                }
                else if (chunkType == "IDAT")
                {
                    break;
                }

                offset += chunkLength + 4;
            }

            if (!hasHeader)
            {
                throw new InvalidDataException("Preview PNG is missing the IHDR chunk.");
            }

            if (width < 0 || width > ushort.MaxValue || height < 0 || height > ushort.MaxValue)
            {
                throw new InvalidDataException("Preview PNG dimensions exceed CAFF limits.");
            }

            bool hasAlpha = colorType == 4 || colorType == 6 || hasTransparency;
            return new PngMetadata(width, height, hasAlpha);
        }

        private static int ReadBigEndianInt32(byte[] buffer, int offset)
        {
            return (buffer[offset] << 24) |
                   (buffer[offset + 1] << 16) |
                   (buffer[offset + 2] << 8) |
                   buffer[offset + 3];
        }

        private static ushort ReadLittleEndianUInt16(byte[] buffer, int offset)
        {
            return unchecked((ushort)(buffer[offset] | (buffer[offset + 1] << 8)));
        }

        private static uint ReadLittleEndianUInt32(byte[] buffer, int offset)
        {
            return unchecked((uint)(buffer[offset] |
                                    (buffer[offset + 1] << 8) |
                                    (buffer[offset + 2] << 16) |
                                    (buffer[offset + 3] << 24)));
        }

        private static string NormalizeArchivePath(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(filePath));
            }

            string normalized = filePath.Replace('\\', '/');
            if (Path.IsPathRooted(normalized) || normalized.IndexOf(':') >= 0)
            {
                throw new InvalidDataException("CAFF file paths must be relative: " + filePath);
            }

            string[] segments = normalized.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length == 0)
            {
                throw new InvalidDataException("Invalid CAFF file path: " + filePath);
            }

            foreach (string segment in segments)
            {
                if (segment == "." || segment == "..")
                {
                    throw new InvalidDataException("CAFF file paths cannot traverse directories: " + filePath);
                }
            }

            return string.Join("/", segments);
        }

        private static string ResolveArchivePath(string baseDirectory, string filePath)
        {
            string normalizedPath = NormalizeArchivePath(filePath);
            string fullBasePath = Path.GetFullPath(baseDirectory);
            string[] pathSegments = normalizedPath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            var combineParts = new string[pathSegments.Length + 1];
            combineParts[0] = fullBasePath;
            Array.Copy(pathSegments, 0, combineParts, 1, pathSegments.Length);
            string combinedPath = Path.GetFullPath(Path.Combine(combineParts));

            string prefix = fullBasePath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
            if (!combinedPath.StartsWith(prefix, GetPathComparison()) && !StringComparerFromComparison(GetPathComparison()).Equals(combinedPath, fullBasePath))
            {
                throw new InvalidDataException("Resolved CAFF path escapes the target directory: " + filePath);
            }

            return combinedPath;
        }

        private static StringComparison GetPathComparison()
        {
            return Path.DirectorySeparatorChar == '\\' ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        }

        private static StringComparer StringComparerFromComparison(StringComparison comparison)
        {
            return comparison == StringComparison.OrdinalIgnoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
        }

        private static string FormatVersion(int[] version)
        {
            return string.Join(".", version.Select(value => value.ToString(CultureInfo.InvariantCulture)));
        }

        private static string FormatOffset(long offset)
        {
            return string.Format(CultureInfo.InvariantCulture, "0x{0:X8}", offset);
        }

        private static byte[] CloneBytes(byte[] buffer)
        {
            if (buffer == null || buffer.Length == 0)
            {
                return Array.Empty<byte>();
            }

            var clone = new byte[buffer.Length];
            Buffer.BlockCopy(buffer, 0, clone, 0, buffer.Length);
            return clone;
        }

        private sealed class PreparedEntry
        {
            public PreparedEntry(CaffFileEntry entry, byte[] storedData)
            {
                Entry = entry;
                StoredData = storedData;
            }

            public CaffFileEntry Entry { get; }

            public byte[] StoredData { get; }

            public long StartPositionAddress { get; set; }
        }

        private struct PngMetadata
        {
            public PngMetadata(int width, int height, bool hasAlpha)
            {
                Width = width;
                Height = height;
                HasAlpha = hasAlpha;
            }

            public int Width { get; }

            public int Height { get; }

            public bool HasAlpha { get; }
        }
    }

    public sealed class CaffHeader
    {
        public CaffHeader()
        {
            ArchiveVersion = new int[3];
            FormatVersion = new int[3];
            ArchiveIdentifier = "CAFF";
            FormatIdentifier = "----";
        }

        public string ArchiveIdentifier { get; set; }

        public int[] ArchiveVersion { get; }

        public string FormatIdentifier { get; set; }

        public int[] FormatVersion { get; }

        public int ObfuscateKey { get; set; }
    }

    public sealed class CaffPreviewImageInfo
    {
        public CaffPreviewImageInfo()
        {
            ImageFormat = CaffImageType.NO_PREVIEW;
            ColorType = CaffColorType.NO_PREVIEW;
        }

        public CaffImageType ImageFormat { get; set; }

        public CaffColorType ColorType { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public long StartPosition { get; internal set; }

        public int FileSize { get; internal set; }
    }

    public sealed class CaffFileEntry
    {
        private byte[] _content;
        private string _tag;

        public CaffFileEntry()
            : this(string.Empty, string.Empty, Array.Empty<byte>(), false, CaffCompressOption.FAST)
        {
        }

        public CaffFileEntry(string filePath, byte[] content)
            : this(filePath, string.Empty, content, false, CaffCompressOption.FAST)
        {
        }

        public CaffFileEntry(string filePath, string tag, byte[] content, bool isObfuscated, CaffCompressOption compressOption)
        {
            FilePath = filePath;
            _tag = tag ?? string.Empty;
            _content = content ?? Array.Empty<byte>();
            IsObfuscated = isObfuscated;
            CompressOption = compressOption;
        }

        public string FilePath { get; internal set; }

        public string Tag
        {
            get => _tag;
            set => _tag = value ?? string.Empty;
        }

        public bool IsObfuscated { get; set; }

        public CaffCompressOption CompressOption { get; set; }

        public byte[] Content
        {
            get => _content;
            set => _content = value ?? Array.Empty<byte>();
        }

        public long StartPosition { get; internal set; }

        public int StoredSize { get; internal set; }
    }

    public enum CaffCompressOption : byte
    {
        RAW = 16,
        FAST = 33,
        SMALL = 37
    }

    public enum CaffColorType : byte
    {
        NOT_INIT = 0,
        ARGB = 1,
        RGB = 2,
        NO_PREVIEW = 127
    }

    public enum CaffImageType : byte
    {
        NOT_INIT = 0,
        PNG = 1,
        NO_PREVIEW = 127
    }

    internal sealed class CaffBinaryReader
    {
        private readonly byte[] _buffer;
        private int _position;

        public CaffBinaryReader(byte[] buffer)
        {
            _buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));
        }

        public long Position
        {
            get => _position;
            set
            {
                if (value < 0 || value > _buffer.Length)
                {
                    throw new EndOfStreamException("Attempted to seek outside the CAFF buffer.");
                }

                _position = checked((int)value);
            }
        }

        public void Skip(int count)
        {
            EnsureAvailable(count);
            _position += count;
        }

        public bool ReadBoolean(int obfuscateKey)
        {
            return ReadByte(obfuscateKey) != 0;
        }

        public byte ReadByte(int obfuscateKey)
        {
            EnsureAvailable(1);
            return unchecked((byte)(_buffer[_position++] ^ obfuscateKey));
        }

        public short ReadInt16(int obfuscateKey)
        {
            EnsureAvailable(2);
            int value = (_buffer[_position] << 8) | _buffer[_position + 1];
            _position += 2;
            return unchecked((short)((short)value ^ obfuscateKey));
        }

        public int ReadInt32(int obfuscateKey)
        {
            EnsureAvailable(4);
            int value = (_buffer[_position] << 24) |
                        (_buffer[_position + 1] << 16) |
                        (_buffer[_position + 2] << 8) |
                        _buffer[_position + 3];
            _position += 4;
            return value ^ obfuscateKey;
        }

        public long ReadInt64(int obfuscateKey)
        {
            EnsureAvailable(8);
            ulong value = ((ulong)_buffer[_position] << 56) |
                          ((ulong)_buffer[_position + 1] << 48) |
                          ((ulong)_buffer[_position + 2] << 40) |
                          ((ulong)_buffer[_position + 3] << 32) |
                          ((ulong)_buffer[_position + 4] << 24) |
                          ((ulong)_buffer[_position + 5] << 16) |
                          ((ulong)_buffer[_position + 6] << 8) |
                          _buffer[_position + 7];
            _position += 8;
            long signedValue = unchecked((long)value);
            long obfuscateMask = CaffBinaryPrimitives.CreateInt64Mask(obfuscateKey);
            return signedValue ^ obfuscateMask;
        }

        public byte[] ReadBytes(int length, int obfuscateKey)
        {
            if (length < 0)
            {
                throw new InvalidDataException("Negative CAFF byte count.");
            }

            EnsureAvailable(length);
            if (length == 0)
            {
                return Array.Empty<byte>();
            }

            var buffer = new byte[length];
            Buffer.BlockCopy(_buffer, _position, buffer, 0, length);
            _position += length;
            if (obfuscateKey != 0)
            {
                for (int index = 0; index < buffer.Length; index++)
                {
                    buffer[index] = unchecked((byte)(buffer[index] ^ obfuscateKey));
                }
            }

            return buffer;
        }

        public int ReadNumber(int obfuscateKey)
        {
            byte first = ReadByte(obfuscateKey);
            if ((first & 128) == 0)
            {
                return first & 0xFF;
            }

            byte second = ReadByte(obfuscateKey);
            if ((second & 128) == 0)
            {
                return ((first & 127) << 7) | (second & 127);
            }

            byte third = ReadByte(obfuscateKey);
            if ((third & 128) == 0)
            {
                return ((first & 127) << 14) | ((second & 127) << 7) | (third & 0xFF);
            }

            byte fourth = ReadByte(obfuscateKey);
            if ((fourth & 128) == 0)
            {
                return ((first & 127) << 21) | ((second & 127) << 14) | ((third & 127) << 7) | (fourth & 0xFF);
            }

            throw new InvalidDataException("Unsupported CAFF variable-length integer.");
        }

        public string ReadString(int obfuscateKey)
        {
            int length = ReadNumber(obfuscateKey);
            byte[] buffer = ReadBytes(length, obfuscateKey);
            return Encoding.UTF8.GetString(buffer, 0, buffer.Length);
        }

        private void EnsureAvailable(int count)
        {
            if (_position > _buffer.Length - count)
            {
                throw new EndOfStreamException("Unexpected end of CAFF buffer.");
            }
        }
    }

    internal sealed class CaffBinaryWriter
    {
        private readonly Stream _stream;

        public CaffBinaryWriter(Stream stream)
        {
            _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        }

        public long Position
        {
            get => _stream.Position;
            set => _stream.Position = value;
        }

        public void Skip(int count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            if (count == 0)
            {
                return;
            }

            WriteBytes(new byte[count], 0, count, 0);
        }

        public void WriteBoolean(bool value, int obfuscateKey)
        {
            WriteByte(value ? (byte)1 : (byte)0, obfuscateKey);
        }

        public void WriteByte(byte value, int obfuscateKey)
        {
            _stream.WriteByte(unchecked((byte)(value ^ obfuscateKey)));
        }

        public void WriteInt16(short value, int obfuscateKey)
        {
            short encoded = unchecked((short)(value ^ obfuscateKey));
            _stream.WriteByte(unchecked((byte)((encoded >> 8) & 0xFF)));
            _stream.WriteByte(unchecked((byte)(encoded & 0xFF)));
        }

        public void WriteInt32(int value, int obfuscateKey)
        {
            int encoded = value ^ obfuscateKey;
            _stream.WriteByte(unchecked((byte)((encoded >> 24) & 0xFF)));
            _stream.WriteByte(unchecked((byte)((encoded >> 16) & 0xFF)));
            _stream.WriteByte(unchecked((byte)((encoded >> 8) & 0xFF)));
            _stream.WriteByte(unchecked((byte)(encoded & 0xFF)));
        }

        public void WriteInt64(long value, int obfuscateKey)
        {
            long obfuscateMask = CaffBinaryPrimitives.CreateInt64Mask(obfuscateKey);
            ulong encoded = unchecked((ulong)(value ^ obfuscateMask));
            _stream.WriteByte((byte)((encoded >> 56) & 0xFF));
            _stream.WriteByte((byte)((encoded >> 48) & 0xFF));
            _stream.WriteByte((byte)((encoded >> 40) & 0xFF));
            _stream.WriteByte((byte)((encoded >> 32) & 0xFF));
            _stream.WriteByte((byte)((encoded >> 24) & 0xFF));
            _stream.WriteByte((byte)((encoded >> 16) & 0xFF));
            _stream.WriteByte((byte)((encoded >> 8) & 0xFF));
            _stream.WriteByte((byte)(encoded & 0xFF));
        }

        public void WriteBytes(byte[] buffer, int offset, int count, int obfuscateKey)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            if (offset < 0 || count < 0 || offset > buffer.Length - count)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            if (count == 0)
            {
                return;
            }

            if (obfuscateKey == 0)
            {
                _stream.Write(buffer, offset, count);
                return;
            }

            var encoded = new byte[count];
            Buffer.BlockCopy(buffer, offset, encoded, 0, count);
            for (int index = 0; index < encoded.Length; index++)
            {
                encoded[index] = unchecked((byte)(encoded[index] ^ obfuscateKey));
            }

            _stream.Write(encoded, 0, encoded.Length);
        }

        public void WriteNumber(int value, int obfuscateKey)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            if (value < 128)
            {
                WriteByte((byte)value, obfuscateKey);
                return;
            }

            if (value < 16384)
            {
                WriteByte((byte)(((value >> 7) & 127) | 128), obfuscateKey);
                WriteByte((byte)(value & 127), obfuscateKey);
                return;
            }

            if (value < 2097152)
            {
                WriteByte((byte)(((value >> 14) & 127) | 128), obfuscateKey);
                WriteByte((byte)(((value >> 7) & 127) | 128), obfuscateKey);
                WriteByte((byte)(value & 127), obfuscateKey);
                return;
            }

            if (value >= 268435456)
            {
                throw new InvalidDataException("Unsupported CAFF variable-length integer: " + value);
            }

            WriteByte((byte)(((value >> 21) & 127) | 128), obfuscateKey);
            WriteByte((byte)(((value >> 14) & 127) | 128), obfuscateKey);
            WriteByte((byte)(((value >> 7) & 127) | 128), obfuscateKey);
            WriteByte((byte)(value & 127), obfuscateKey);
        }

        public void WriteString(string value, int obfuscateKey)
        {
            string actualValue = value ?? string.Empty;
            byte[] bytes = Encoding.UTF8.GetBytes(actualValue);
            WriteNumber(bytes.Length, obfuscateKey);
            WriteBytes(bytes, 0, bytes.Length, obfuscateKey);
        }
    }

    internal static class CaffBinaryPrimitives
    {
        public static long CreateInt64Mask(int obfuscateKey)
        {
            uint lowerBits = unchecked((uint)obfuscateKey);
            long upperBits = obfuscateKey < 0 ? -1L : obfuscateKey;
            return (upperBits << 32) | lowerBits;
        }
    }
}