using System.Text;

namespace Helveg
{
    /**
     * The CRC32 implementation is based on http://c.snippets.org/snip_lister.php?fname=crc_32.c licensed as such:
     *
     * Copyright (C) 1986 Gary S. Brown.  You may use this program, or
     * code or tables extracted from it, as desired without restriction.
     */

    public static class Checksum
    {
        public static readonly uint[] Crc32Table = CreateCrc32Table(0xedb88320);

        public static uint[] CreateCrc32Table(uint polynomial)
        {
            var table = new uint[256];

            for (uint b = 0; b < 256; ++b)
            {
                uint value = b;
                for (int i = 7; i >= 0; --i)
                {
                    value = (value & 1) == 1
                        ? (value >> 1) ^ polynomial
                        : value >> 1;
                }
                table[b] = value;
            }
            return table;
        }

        public static uint UpdateCrc32(uint crc, byte ch)
        {
            return Crc32Table[(crc ^ ch) & 0xff] ^ (crc >> 8);
        }

        public static int GetCrc32(byte[] buffer)
        {
            uint crc = 0xFFFFFFFF;
            for (int i = 0; i < buffer.Length; ++i)
            {
                crc = UpdateCrc32(crc, buffer[i]);
            }

            return (int)~crc;
        }

        public static int GetCrc32(string? value)
        {
            value ??= string.Empty;
            return GetCrc32(Encoding.ASCII.GetBytes(value));
        }
    }
}
