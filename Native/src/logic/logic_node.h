//
// Created by jamie on 30/07/19.
//

#ifndef REFACTOR_NATIVE_LOGIC_NODE_H
#define REFACTOR_NATIVE_LOGIC_NODE_H

#include <string>
#include <vector>
#include <memory>
#include <common.h>

namespace Refactor {

    class LogicNode;

    class LogicRootNode {

    private:
        EntityType type;
        std::shared_ptr<LogicNode> logic_tree;

    public:
        LogicRootNode(EntityType type) {
          this->type = type;
        }

        std::shared_ptr<LogicNode> get_tree() {
          return logic_tree;
        }

        void put_root(std::shared_ptr<LogicNode> node) {
          this->logic_tree = node;
        }

        EntityType get_type() const {
          return type;
        }
    };


    enum class LogicNodeConnection {
        NONE,
        BOOLEAN,
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
        std::string color;
        int id;
        std::string name;
        NamedLogicNodeConnection connection_out;
        std::vector<NamedLogicNodeConnection> connections_in;
    };


// Define shorthands to reduce verbosity here
static int logic_node_type_id = 0;
#define NODE_TYPE0_1(c, n, in) new LogicNodeType { .color = c,  .id = logic_node_type_id++, .name = n, .connection_out = NamedLogicNodeConnection{"none", LogicNodeConnection::NONE}, .connections_in = { in } };
#define NODE_TYPE1_0(c, n, out) new LogicNodeType { .color = c, .id = logic_node_type_id++, .name = n, .connection_out = out, .connections_in = {} };
#define NODE_TYPE1_1(c, n, out, in) new LogicNodeType { .color = c, .id = logic_node_type_id++, .name = n, .connection_out = out, .connections_in = { in } };
#define NODE_TYPE1_2(c, n, out, in1, in2) new LogicNodeType { .color = c, .id = logic_node_type_id++, .name = n, .connection_out = out, .connections_in = { in1, in2 } };

#define BOOL(a) NamedLogicNodeConnection{a, LogicNodeConnection::BOOLEAN}
#define WORKER_TYPE(a) NamedLogicNodeConnection{a, LogicNodeConnection::WORKER_TYPE}
#define INVENTORY_ITEM(a) NamedLogicNodeConnection{a, LogicNodeConnection::INVENTORY_ITEM}
#define NUMBER(a) NamedLogicNodeConnection{a, LogicNodeConnection::NUMBER}
#define NUMERICAL_COMPARISON(a) NamedLogicNodeConnection{a, LogicNodeConnection::NUMERICAL_COMPARISON}

    class LogicNodeTypes {
    private:
        LogicNodeTypes() = default;
        ~LogicNodeTypes() = default;

    public:
        const LogicNodeType *NONE = new LogicNodeType{ "#333300", logic_node_type_id++, "None" };
        const LogicNodeType *TOGGLE_IF = NODE_TYPE0_1( "#ff9900", "Toggle if", BOOL("Condition"));
        const LogicNodeType *ON_IF = NODE_TYPE0_1( "#ff3300", "On if", BOOL("Condition"));

        const LogicNodeType *NOT = NODE_TYPE1_1( "#800000", "Not", BOOL("Out"), BOOL("In 1"));
        const LogicNodeType *AND = NODE_TYPE1_2 ("#0066ff", "And", BOOL("Out"), BOOL("In 1"), BOOL("In 2"));
        const LogicNodeType *OR = NODE_TYPE1_2("#33cc33", "Or", BOOL("Out"), BOOL("In 1"), BOOL("In 2"));
        const LogicNodeType *XOR = NODE_TYPE1_2( "#009933", "XOr", BOOL("Out"), BOOL("In 1"), BOOL("In 2"));
        const LogicNodeType *WORKER_IS = NODE_TYPE1_1( "#9900cc", "Worker is", BOOL("Out"), WORKER_TYPE("In 1"));

        const LogicNodeType *WORKER_TYPE = NODE_TYPE1_0( "#6600cc", "Worker type", WORKER_TYPE("Out"));
        const LogicNodeType *WORKER_HAS = NODE_TYPE1_2( "#0099cc", "Worker has", BOOL("Out"), NUMERICAL_COMPARISON("In 1"), INVENTORY_ITEM("In 2"));
        const LogicNodeType *INVENTORY_ITEM = NODE_TYPE1_0( "#336699", "Inventory item", INVENTORY_ITEM("Out"));

        const LogicNodeType *COUNTER_IS = NODE_TYPE1_1( "#009999", "Counter is", BOOL("Out"), NUMERICAL_COMPARISON("In 1"));
        const LogicNodeType *NUMERICAL_EQUALS = NODE_TYPE1_1( "#339966", "Equals", NUMERICAL_COMPARISON("Out"), NUMBER("In 1"));
        const LogicNodeType *NUMERICAL_GREATER_THAN = NODE_TYPE1_1( "#339933", "Greater than", NUMERICAL_COMPARISON("Out"), NUMBER("In 1"));
        const LogicNodeType *NUMERICAL_LESS_THAN = NODE_TYPE1_1( "#003366", "Less than", NUMERICAL_COMPARISON("Out"), NUMBER("In 1"));
        const LogicNodeType *NUMBER = NODE_TYPE1_0( "#ff5050", "Number", NUMBER("Out"));

        const std::vector<const LogicNodeType*> toVector() {
          return std::vector<const LogicNodeType*>{
            NONE,
            TOGGLE_IF,
            ON_IF,
            NOT,
            AND,
            OR,
            XOR,
            WORKER_IS,
            WORKER_TYPE,
            WORKER_HAS,
            INVENTORY_ITEM,
            COUNTER_IS,
            NUMERICAL_EQUALS,
            NUMERICAL_GREATER_THAN,
            NUMERICAL_LESS_THAN,
            NUMBER,
          };
        }

        static LogicNodeTypes *getInstance() {
            static auto _internal = new LogicNodeTypes();
            return _internal;
        }
    };

#undef NLNC
#undef LNC

    class LogicNode {
    public:
        explicit LogicNode(const LogicNodeType* logic_node_type) {
          this->logic_node_type = logic_node_type;
        }

        const LogicNodeType* get_type() {
          return this->logic_node_type;
        }

        void set_output(std::shared_ptr<LogicNode> _output) {
          this->output = _output;
        }

        std::shared_ptr<LogicNode> get_output() {
          return this->output;
        }

        void set_root_output(std::shared_ptr<LogicRootNode> _output) {
          this->root_output = _output;
        }

        std::shared_ptr<LogicRootNode> get_root_output() {
          return this->root_output;
        }

        void set_input_1(std::shared_ptr<LogicNode> _input) {
          this->input_1 = _input;
        }

        std::shared_ptr<LogicNode> get_input_1() {
          return this->input_1;
        }

        void set_input_2(std::shared_ptr<LogicNode> _input) {
          this->input_2 = _input;
        }

        std::shared_ptr<LogicNode> get_input_2() {
          return this->input_2;
        }

    private:
        const LogicNodeType *logic_node_type;

        std::shared_ptr<LogicNode> output;
        std::shared_ptr<LogicRootNode> root_output;

        std::shared_ptr<LogicNode> input_1 = nullptr;
        std::shared_ptr<LogicNode> input_2 = nullptr;
    };

}

#endif //REFACTOR_NATIVE_LOGIC_NODE_H
