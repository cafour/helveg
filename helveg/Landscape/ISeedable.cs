namespace Helveg.Landscape
{
    public interface ISeedable
    {
        // CRC32 (Helveg.Checksum.GetCrc32) of the string "Netušim"
        const int Arbitrary = 702101172;

        int GetSeed();
    }
}
