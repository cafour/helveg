cmake_minimum_required(VERSION 3.10)

project(helveg)

include(Dotnet)

add_dotnet_build(${PROJECT_NAME})
target_link_libraries(${PROJECT_NAME} vku)
