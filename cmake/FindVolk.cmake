include(FetchContent)
FetchContent_Declare(fetch_volk
    GIT_REPOSITORY https://github.com/zeux/volk
)
FetchContent_GetProperties(fetch_volk)
if(NOT fetch_volk_POPULATED)
  FetchContent_Populate(fetch_volk)
  add_subdirectory(${fetch_volk_SOURCE_DIR} ${fetch_volk_BINARY_DIR})
endif()

if(WIN32)
    target_compile_definitions(volk INTERFACE VK_USE_PLATFORM_WIN32_KHR)
else()
    target_compile_definitions(volk INTERFACE VK_USE_PLATFORM_XCB_KHR)
endif()
