cmake_minimum_required(VERSION 3.10)

project(rtx)

# Do not update dependencies on every config
option(FETCHCONTENT_UPDATES_DISCONNECTED ON)
set(CMAKE_MODULE_PATH ${CMAKE_MODULE_PATH} "${CMAKE_CURRENT_SOURCE_DIR}/cmake")
set(CMAKE_INSTALL_PREFIX ${CMAKE_SOURCE_DIR}/artifacts)

add_subdirectory(vku)
install(TARGETS vku
        DESTINATION ".")
