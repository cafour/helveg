cmake_minimum_required(VERSION 3.10)

project(helveg)

add_custom_target(${PROJECT_NAME}
    ALL dotnet build ${CMAKE_CURRENT_SOURCE_DIR} -o ${CMAKE_CURRENT_BINARY_DIR} -r ${DOTNET_RID}
    DEPENDS vku)
