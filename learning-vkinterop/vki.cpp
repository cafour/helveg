#include "vki.hpp"
#include "sample.hpp"

#include <cstdlib>
#include <volk.h>

int helloTriangle() {
    if (volkInitialize() != VK_SUCCESS) {
        return EXIT_FAILURE;
    }
    try {
        Sample app(1280, 720);
        app.run();
    } catch (const std::exception& e) {
        std::cerr << e.what() << std::endl;
        return EXIT_FAILURE;
    }

    return EXIT_SUCCESS;
}
