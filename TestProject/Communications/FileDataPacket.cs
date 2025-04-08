using System;

namespace TestProject.Communications
{
    public class FileDataPacket : Packet
    {
        public byte[] BinaryData { get; set; }
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public DateTime CreationTimeUtc { get; set; }
        public DateTime LastWriteTimeUtc { get; set; }

    }
}
