#include "vki.hpp"
#include "sample.hpp"

#include <cstdlib>

int helloTriangle() {
    try {
        Sample app(1280, 720);
        app.run();
    } catch (const std::exception& e) {
        std::cerr << e.what() << std::endl;
        return EXIT_FAILURE;
    }

    return EXIT_SUCCESS;
}
