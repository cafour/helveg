include(FetchContent)
FetchContent_Declare(fetch_glm
    URL https://github.com/g-truc/glm/archive/0.9.9.7.tar.gz
    URL_HASH SHA1=df8e6c1472002be7e431bfb3189007d7a65f1745)
FetchContent_GetProperties(fetch_glm)
if(NOT fetch_glm_POPULATED)
    FetchContent_Populate(fetch_glm)
    add_subdirectory(${fetch_glm_SOURCE_DIR} ${fetch_glm_BINARY_DIR})
endif()
