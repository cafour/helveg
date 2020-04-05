add_executable(embed cmake/embed.cpp)

file(MAKE_DIRECTORY ${CMAKE_SOURCE_DIR}/generated)
function(add_embedded_shader INPUT_GLSL OUTPUT_CPP SHADER_NAME)
    get_filename_component(_SPV ${INPUT_GLSL} NAME)
    set(_SPV generated/${_SPV}.spv)
    add_custom_command(
        OUTPUT ${_SPV}
        COMMAND glslc ${INPUT_GLSL} -o ${_SPV}
        MAIN_DEPENDENCY ${INPUT_GLSL}
        WORKING_DIRECTORY ${CMAKE_SOURCE_DIR}
    )
    add_custom_command(
        OUTPUT ${OUTPUT_CPP}
        COMMAND embed ${_SPV} ${OUTPUT_CPP} ${SHADER_NAME}
        MAIN_DEPENDENCY ${_SPV}
        WORKING_DIRECTORY ${CMAKE_SOURCE_DIR}
        DEPENDS embed
    )
endfunction()
