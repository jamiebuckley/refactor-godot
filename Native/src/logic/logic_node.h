//
// Created by jamie on 30/07/19.
//

#ifndef REFACTOR_NATIVE_LOGIC_NODE_H
#define REFACTOR_NATIVE_LOGIC_NODE_H

#include <string>
#include <vector>

namespace Refactor {

    enum class LogicNodeConnection {
        BOOLEAN,
        DATA_WORKER_HAS,
        DATA_WORKER_IS,
        DATA_COUNTER,
        NUMERICAL_COMPARISON,
    };

    struct NamedLogicNodeConnection {
        std::string name;
        LogicNodeConnection connection_type;
    };

    struct LogicNodeType {
      std::vector<NamedLogicNodeConnection> connections_out;
      std::vector<NamedLogicNodeConnection> connections_in;
    };

    class LogicNodeTypes {
    public:
        static LogicNodeType* AND() {
          static LogicNodeType* _AND = new LogicNodeType {
                  .connections_out =  { NamedLogicNodeConnection { "Out", LogicNodeConnection::BOOLEAN } },
                  .connections_in = { NamedLogicNodeConnection { "Value 1 In", LogicNodeConnection::BOOLEAN }, NamedLogicNodeConnection { "Value 2 In", LogicNodeConnection::BOOLEAN } }
          };
          return _AND;
        };
    };

    class LogicNode {
    public:
      bool stuff() {
        auto thingy = LogicNodeTypes::AND();
        auto thingy2 = LogicNodeTypes::AND();
        return thingy == thingy2;
      }
    };
}

#endif //REFACTOR_NATIVE_LOGIC_NODE_H
