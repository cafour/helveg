cmake_minimum_required(VERSION 3.10)

project(vku LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED True)

set(VKU_SOURCES
    # headers
    vku.hpp
    base.hpp
    shader.hpp

    # sources
    base.cpp
    shader.cpp)

set(PROJECT_FILES
    ${VKU_SOURCES})

add_library(${PROJECT_NAME} STATIC ${PROJECT_FILES})
target_link_libraries(${PROJECT_NAME} volk)
