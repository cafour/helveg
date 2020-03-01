#include <cstdint>
#include <fstream>
#include <iomanip>
#include <iostream>

const std::size_t BUFFER_SIZE = 8;

int main(int argc, const char *argv[])
{
    if (argc != 4) {
        std::cerr << "Usage: " << argv[0] << " SRC DST NAME\n";
        return 1;
    }
    const char *srcName = argv[1];
    const char *dstName = argv[2];
    const char *name = argv[3];
    std::ifstream src(srcName, std::ios::in | std::ios::binary);
    std::ofstream dst(dstName);
    dst << "#include \"shaders.hpp\"\n"
        << "#include <cstdint>\n"
        << "\n"
        << "static const uint32_t _" << name << "[] = {\n";

    uint32_t buffer[BUFFER_SIZE];
    size_t total = 0;
    while (src) {
        src.read(reinterpret_cast<char *>(buffer), BUFFER_SIZE * sizeof(uint32_t));
        size_t count = src ? BUFFER_SIZE : src.gcount();
        total += count;
        dst << "    ";
        for (size_t i = 0; i < count; ++i) {
            dst << "0x" << std::setfill('0') << std::setw(8) << std::hex << +buffer[i] << ",";
            if (i != BUFFER_SIZE - 1) {
                dst << " ";
            }
        }
        dst << "\n";
    }
    dst << "};\n"
        << "\n"
        << "const size_t " << name << "_LENGTH = " << std::dec << total * sizeof(uint32_t) << ";\n"
        << "const uint32_t *" << name << " = _" << name << ";\n";
    return 0;
}
