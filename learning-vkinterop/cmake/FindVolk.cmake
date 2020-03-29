include(FetchContent)
FetchContent_Declare(volk_FETCH
    GIT_REPOSITORY https://github.com/zeux/volk
)
FetchContent_MakeAvailable(volk_FETCH)
if(WIN32)
    target_compile_definitions(volk INTERFACE VK_USE_PLATFORM_WIN32_KHR)
else()
    target_compile_definitions(volk INTERFACE VK_USE_PLATFORM_XCB_KHR)
endif()
