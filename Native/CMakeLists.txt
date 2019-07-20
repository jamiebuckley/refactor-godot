cmake_minimum_required(VERSION 2.8.9)
project (refactor_native)

set(PROJECT_SOURCE_DIR src)

set(SRC_FILES
        ${PROJECT_SOURCE_DIR}/game.cpp
        ${PROJECT_SOURCE_DIR}/library.cpp
        ${PROJECT_SOURCE_DIR}/grid/grid.cpp
        ${PROJECT_SOURCE_DIR}/godot_entities/GodotWorker.cpp)

include_directories(godot-cpp/godot_headers)
include_directories(godot-cpp/include)
include_directories(godot-cpp/include/core)
include_directories(godot-cpp/include/gen)
include_directories(${PROJECT_SOURCE_DIR})

link_libraries(${CMAKE_SOURCE_DIR}/godot-cpp/bin/libgodot-cpp.linux.debug.64.a)

add_library(refactor_native SHARED ${SRC_FILES})

add_executable(refactor_tests ${SRC_FILES} test/test_main.cpp test/test1.cpp)