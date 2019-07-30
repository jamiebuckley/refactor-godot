//
// Created by jamie on 30/07/19.
//
#include "catch.hpp"
#include "logic/logic_node.h"

TEST_CASE( "Things happen", "[LogicNode]" ) {
  auto logic_node = Refactor::LogicNode();
  REQUIRE(logic_node.stuff());
}