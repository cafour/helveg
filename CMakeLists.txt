cmake_minimum_required(VERSION 3.10)

project(helveg)

# Do not update dependencies on every config
option(FETCHCONTENT_UPDATES_DISCONNECTED ON)
set(CMAKE_MODULE_PATH ${CMAKE_MODULE_PATH} "${CMAKE_CURRENT_SOURCE_DIR}/cmake")

add_subdirectory(vku)
install(TARGETS vku
        DESTINATION ".")
