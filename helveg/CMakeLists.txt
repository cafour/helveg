cmake_minimum_required(VERSION 3.10)

project(helveg)

include(Dotnet)

add_dotnet_build(${PROJECT_NAME})
add_dependencies(${PROJECT_NAME} vku)
