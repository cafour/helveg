cmake_minimum_required(VERSION 3.10)

project(vku LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED True)

set(VKU_SOURCES
    # headers
    vku.hpp
    base.hpp
    types.hpp
    window.hpp
    standalone.hpp
    instance_related.hpp
    physical_device.hpp
    device_related.hpp
    swapchain_env.hpp
    app.hpp

    # sources
    base.cpp
    window.cpp
    standalone.cpp
    instance_related.cpp
    physical_device.cpp
    device_related.cpp
    swapchain_env.cpp
    app.cpp)

add_library(${PROJECT_NAME} STATIC ${VKU_SOURCES})
target_link_libraries(${PROJECT_NAME} volk glfw)
target_include_directories(${PROJECT_NAME} INTERFACE ${CMAKE_CURRENT_SOURCE_DIR})
set_property(TARGET ${PROJECT_NAME} PROPERTY POSITION_INDEPENDENT_CODE ON)

if(WIN32)
    target_compile_options(${PROJECT_NAME} PRIVATE /W4)
else()
    target_compile_options(${PROJECT_NAME} PRIVATE -Wall -Wextra -pedantic)
endif()
