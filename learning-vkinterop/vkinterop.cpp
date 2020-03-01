#include "vkinterop.hpp"

extern "C" {
    int calculateValue() {
        return 3 * 7 << 1;
    }
}
