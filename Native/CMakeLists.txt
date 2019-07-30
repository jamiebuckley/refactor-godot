cmake_minimum_required(VERSION 2.8.9)
project (refactor_native)

set(PROJECT_SOURCE_DIR src)

if (${CMAKE_SYSTEM_NAME} MATCHES "Windows")
    set(WINDOWS TRUE)
elseif (${CMAKE_SYSTEM_NAME} MATCHES "Linux")
    set(lib_name linux)
elseif (${CMAKE_SYSTEM_NAME} MATCHES "Darwin")
    set(CMAKE_MACOSX_RPATH 1)
    set(lib_name osx)
endif()


set(SRC_FILES
        ${PROJECT_SOURCE_DIR}/game.cpp
        ${PROJECT_SOURCE_DIR}/library.cpp
        ${PROJECT_SOURCE_DIR}/grid/grid.cpp
        ${PROJECT_SOURCE_DIR}/godot_entities/godot_worker.cpp
        ${PROJECT_SOURCE_DIR}/logic/logic_editor.cpp
        ${PROJECT_SOURCE_DIR}/logic/logic_node.cpp)

include_directories(godot-cpp/godot_headers)
include_directories(godot-cpp/include)
include_directories(godot-cpp/include/core)
include_directories(godot-cpp/include/gen)
include_directories(${PROJECT_SOURCE_DIR})

link_libraries(${CMAKE_SOURCE_DIR}/godot-cpp/bin/libgodot-cpp.${lib_name}.debug.64.a)
add_library(refactor_native SHARED ${SRC_FILES})
set_property(TARGET refactor_native PROPERTY CXX_STANDARD 17)

add_executable(refactor_tests ${SRC_FILES} test/test_main.cpp test/test1.cpp test/logic_editor_tests.cpp)
set_property(TARGET refactor_tests PROPERTY CXX_STANDARD 17)