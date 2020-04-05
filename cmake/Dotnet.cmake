if(WIN32)
    set(DOTNET_RID win-x64)
else()
    set(DOTNET_RID linux-x64)
endif()

set(DOTNET_CLI dotnet CACHE FILEPATH "Path to the dotnet CLI")
add_executable(dotnet IMPORTED)
set_target_properties(dotnet
    PROPERTIES IMPORTED_LOCATION ${DOTNET_CLI})
message(STATUS "Using dotnet: ${DOTNET_CLI}")
