set(VOLK_PULL_IN_VULKAN OFF CACHE BOOL "" FORCE)
if(WIN32)
  set(VOLK_STATIC_DEFINES VK_USE_PLATFORM_WIN32_KHR)
else()
  set(VOLK_STATIC_DEFINES VK_USE_PLATFORM_XCB_KHR)
endif()
set(VOLK_STATIC_DEFINES ${VOLK_STATIC_DEFINES} VK_ENABLE_BETA_EXTENSIONS)

include(FetchContent)
FetchContent_Declare(fetch_volk
    GIT_REPOSITORY https://github.com/zeux/volk
)
FetchContent_GetProperties(fetch_volk)
if(NOT fetch_volk_POPULATED)
  FetchContent_Populate(fetch_volk)
  add_subdirectory(${fetch_volk_SOURCE_DIR} ${fetch_volk_BINARY_DIR})
endif()
