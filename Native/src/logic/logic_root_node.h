//
// Created by jamie on 30/07/19.
//

#ifndef REFACTOR_NATIVE_LOGIC_ROOT_NODE_H
#define REFACTOR_NATIVE_LOGIC_ROOT_NODE_H

#include "logic_node.h"
#include "common.h"
#include <memory>

namespace Refactor {

    enum class LogicRootNodeAction {
        NONE,
        TOGGLE_IF,
        ON_IF,
        OFF_IF,
        SET_CARDINAL_IF,
        INCREMENT_IF,
        INCREMENT_BY_IF,
        DECREMENT_IF,
        DECREMENT_BY_IF,
    };

    class LogicRootNode {

    private:
      EntityType type;
      LogicRootNodeAction action;
      std::shared_ptr<LogicNode> logic_tree;

    public:
      LogicRootNode(EntityType type, LogicRootNodeAction action) {
        this->type = type;
        this->action = action;
      }

      std::shared_ptr<LogicNode> get_tree() {
        return logic_tree;
      }
    };
}

#endif //REFACTOR_NATIVE_LOGIC_ROOT_NODE_H
