set(DOTNET_CLI dotnet CACHE FILEPATH "Path to the dotnet CLI")
add_executable(dotnet IMPORTED)
set_target_properties(dotnet
    PROPERTIES IMPORTED_LOCATION ${DOTNET_CLI})
message(STATUS "Using dotnet: ${DOTNET_CLI}")

if(WIN32)
    set(DOTNET_RID win-x64)
else()
    set(DOTNET_RID linux-x64)
endif()

function(add_dotnet_build TARGET)
    add_executable(${TARGET} IMPORTED)
    set_target_properties(dotnet
        PROPERTIES IMPORTED_LOCATION ${CMAKE_CURRENT_BINARY_DIR}/${TARGET})
    add_custom_command(
        OUTPUT ${CMAKE_CURRENT_BINARY_DIR}/${TARGET}
        COMMAND dotnet build ${CMAKE_CURRENT_SOURCE_DIR} -o ${CMAKE_CURRENT_BINARY_DIR} -r ${DOTNET_RID}
        DEPENDS dotnet)
endfunction()
