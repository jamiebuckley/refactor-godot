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
        COUNTER,
        NUMERICAL_COMPARISON,
        NUMBER,
        WORKER_TYPE,
        INVENTORY_ITEM
    };

    struct NamedLogicNodeConnection {
        std::string name;
        LogicNodeConnection connection_type;
    };

    struct LogicNodeType {
        std::vector<NamedLogicNodeConnection> connections_out;
        std::vector<NamedLogicNodeConnection> connections_in;
    };


// Define shorthands to reduce verbosity here
#define NLNC NamedLogicNodeConnection
#define LNC LogicNodeConnection

    class LogicNodeTypes {
    private:
        LogicNodeTypes() = default;

        ~LogicNodeTypes() = default;

    public:
        const LogicNodeType *NOT = new LogicNodeType{
                .connections_out =  {NLNC{"Out", LNC::BOOLEAN}},
                .connections_in = {NLNC{"Value 1 In", LNC::BOOLEAN}}
        };

        const LogicNodeType *AND = new LogicNodeType{
                .connections_out =  {NLNC{"Out", LNC::BOOLEAN}},
                .connections_in = {
                        NLNC{"Value 1 In", LNC::BOOLEAN},
                        NLNC{"Value 2 In", LNC::BOOLEAN}
                }
        };

        const LogicNodeType *OR = new LogicNodeType{
                .connections_out =  {NLNC{"Out", LNC::BOOLEAN}},
                .connections_in = {
                        NLNC{"Value 1 In", LNC::BOOLEAN},
                        NLNC{"Value 2 In", LNC::BOOLEAN}
                }
        };

        const LogicNodeType *XOR = new LogicNodeType{
                .connections_out =  {NLNC{"Out", LNC::BOOLEAN}},
                .connections_in = {
                        NLNC{"Value 1 In", LNC::BOOLEAN},
                        NLNC{"Value 2 In", LNC::BOOLEAN}
                }
        };

        const LogicNodeType *WORKER_IS = new LogicNodeType{
                .connections_out =  {NLNC{"Out", LNC::BOOLEAN}},
                .connections_in = {NLNC{"Value is", LNC::WORKER_TYPE}}
        };

        const LogicNodeType *WORKER_HAS = new LogicNodeType{
                .connections_out =  {NLNC{"Out", LNC::BOOLEAN}},
                .connections_in = {NLNC{"Value is", LNC::INVENTORY_ITEM}, NLNC{"Value is", LNC::NUMERICAL_COMPARISON}}
        };

        const LogicNodeType *COUNTER_IS = new LogicNodeType{
                .connections_out =  {NLNC{"Out", LNC::BOOLEAN}},
                .connections_in = {NLNC{"Value is", LNC::NUMERICAL_COMPARISON}}
        };

        const LogicNodeType *NUMERICAL_EQUALS = new LogicNodeType{
                .connections_out =  {NLNC{"Out", LNC::NUMERICAL_COMPARISON}},
                .connections_in = {NLNC{"Value is", LNC::NUMBER}}
        };

        const LogicNodeType *NUMERICAL_GREATER_THAN = new LogicNodeType{
                .connections_out =  {NLNC{"Out", LNC::NUMERICAL_COMPARISON}},
                .connections_in = {NLNC{"Value is", LNC::NUMBER}}
        };

        const LogicNodeType *NUMERICAL_LESS_THAN = new LogicNodeType{
                .connections_out =  {NLNC{"Out", LNC::NUMERICAL_COMPARISON}},
                .connections_in = {NLNC{"Value is", LNC::NUMBER}}
        };

        static LogicNodeTypes *getInstance() {
            static auto _internal = new LogicNodeTypes();
            return _internal;
        }
    };

#undef NLNC
#undef LNC

    class LogicNode {
    public:
        bool stuff() {
            auto thingy = LogicNodeTypes::getInstance()->AND;
            auto thingy2 = LogicNodeTypes::getInstance()->AND;
            return thingy == thingy2;
        }
    };

}

#endif //REFACTOR_NATIVE_LOGIC_NODE_H
