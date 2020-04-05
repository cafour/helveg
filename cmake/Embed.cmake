add_executable(embed ${CMAKE_CURRENT_LIST_DIR}/embed.cpp)

set(EMBED_DIR ${CMAKE_BINARY_DIR}/embedded)
file(MAKE_DIRECTORY ${EMBED_DIR})

function(target_embed TARGET FILE EMBEDDED_NAME)
    get_filename_component(_NAME ${FILE} NAME)
    set (_NAME ${EMBED_DIR}/${_NAME}.cpp)
    add_custom_command(
        OUTPUT ${_NAME}
        COMMAND embed ${FILE} ${_NAME} ${EMBEDDED_NAME}
        MAIN_DEPENDENCY ${FILE}
        WORKING_DIRECTORY ${CMAKE_CURRENT_SOURCE_DIR}
        DEPENDS embed)
    target_sources(${TARGET} PRIVATE ${_NAME})
endfunction()

function(target_embed_shader TARGET INPUT_GLSL EMBEDDED_NAME)
    get_filename_component(_SPV ${INPUT_GLSL} NAME)
    set(_SPV ${EMBED_DIR}/${_SPV}.spv)
    add_custom_command(
        OUTPUT ${_SPV}
        COMMAND glslc ${INPUT_GLSL} -o ${_SPV}
        MAIN_DEPENDENCY ${INPUT_GLSL}
        WORKING_DIRECTORY ${CMAKE_CURRENT_SOURCE_DIR}
    )
    target_embed(${TARGET} ${_SPV} ${EMBEDDED_NAME})
endfunction()
