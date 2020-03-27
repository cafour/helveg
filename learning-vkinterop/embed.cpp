#include <cstdint>
#include <fstream>
#include <iomanip>
#include <iostream>
#include <vector>

const std::size_t U32_PER_LINE = 8;

int main(int argc, const char *argv[])
{
    if (argc != 4) {
        std::cerr << "Usage: " << argv[0] << " SRC DST NAME\n";
        return 1;
    }
    const char *srcName = argv[1];
    const char *dstName = argv[2];
    const char *name = argv[3];
    std::ofstream dst(dstName);
    dst << "#include \"shaders.hpp\"\n"
        << "#include <cstdint>\n"
        << "\n"
        << "static const uint32_t _" << name << "[] = {";

    std::ifstream src(srcName, std::ios::ate | std::ios::binary);
    size_t size = src.tellg();
    src.seekg(0);
    std::vector<char> buffer(size);
    src.read(buffer.data(), size);
    const uint32_t *content = reinterpret_cast<const uint32_t *>(buffer.data());

    for (size_t i = 0; i < size / sizeof(uint32_t); ++i) {
        if (i % U32_PER_LINE == 0) {
            dst << "\n    ";
        }
        dst << "0x" << std::setfill('0') << std::setw(8) << std::hex << +content[i] << ",";
        if (i % U32_PER_LINE != 0) {
            dst << " ";
        }
    }
    dst << "\n"
        << "};\n"
        << "\n"
        << "const size_t " << name << "_LENGTH = " << std::dec << size << ";\n"
        << "const uint32_t *" << name << " = _" << name << ";\n";
    return 0;
}
