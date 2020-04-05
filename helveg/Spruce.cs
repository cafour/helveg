using System;
using System.Text;

namespace Helveg
{
    public static class Spruce
    {
        public static string Rewrite(
            string axiom,
            int branchCount,
            int maxBranching,
            int minBranching,
            int initialBranching,
            int branchingDiff)
        {
            var buffers = new StringBuilder[2]
            {
                new StringBuilder(axiom, 1 << 10),
                new StringBuilder(1 << 10)
            };
            var trunkHeight = branchCount / 2;
            var currentBranching = initialBranching;
            var src = buffers[0];
            var dst = buffers[1];
            var index = 0;
            while (branchCount > 0)
            {
                dst.Clear();
                for (int i = 0; i < src.Length; ++i)
                {
                    switch (src[i])
                    {
                        case 'T':
                            if (trunkHeight > 0)
                            {
                                dst.Append("FT");
                                trunkHeight--;
                            }
                            else
                            {
                                dst.Append('T');
                            }
                            break;
                        case 'C':
                            for (int j = 0; j < currentBranching && j < branchCount; ++j)
                            {
                                dst.Append("[FB]");
                            }
                            branchCount = Math.Max(0, branchCount - currentBranching);
                            currentBranching = ((currentBranching + branchingDiff - minBranching)
                                % (maxBranching - minBranching)) + minBranching;
                            dst.Append("FC");
                            break;
                        case 'B':
                            dst.Append("[FB][FB]FB");
                            break;
                        default:
                            dst.Append(src[i]);
                            break;
                    }
                }

                dst = buffers[index];
                index = (index + 1) % 2;
                src = buffers[index];
            }
            return src.ToString();
        }
    }
}
