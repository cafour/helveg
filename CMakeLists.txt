cmake_minimum_required(VERSION 3.10)

project(rtx)

set(CMAKE_MODULE_PATH ${CMAKE_MODULE_PATH} "${CMAKE_CURRENT_SOURCE_DIR}/cmake")

add_subdirectory(vku)
install(TARGETS vku)
