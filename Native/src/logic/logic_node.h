//
// Created by jamie on 30/07/19.
//

#ifndef REFACTOR_NATIVE_LOGIC_NODE_H
#define REFACTOR_NATIVE_LOGIC_NODE_H

#include <string>
#include <vector>
#include <memory>
#include <common.h>
#include <gen/Node2D.hpp>

namespace Refactor {

    enum class LogicNodeConnection {
        NONE,
        ACTION,
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
#define ACTION(a) NamedLogicNodeConnection{a, LogicNodeConnection::ACTION}
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

        const LogicNodeType *ROOT = NODE_TYPE0_1( "#ff9900", "Root", ACTION("Action"))
        const LogicNodeType *TOGGLE_IF = NODE_TYPE1_1( "#ff9900", "Toggle if", ACTION("Action"), BOOL("Condition"));
        const LogicNodeType *ON_IF = NODE_TYPE1_1( "#ff3300", "On if", ACTION("Action"), BOOL("Condition"));

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

    class LogicNode;

    struct LogicNodeOutput {
        int parent_output_index;
        std::shared_ptr<LogicNode> parent;
    };

    struct LogicNodeInput {
        int index;
        bool enabled;
        godot::Node2D* ghost;
        std::shared_ptr<LogicNode> node;
    };


    class LogicNode {
    public:
        explicit LogicNode(const LogicNodeType* type) {
          this->logic_node_type = type;
          this->output = std::make_shared<LogicNodeOutput>();
          this->inputs = {
                  std::make_shared<LogicNodeInput>(LogicNodeInput { .index = 0, .enabled = !type->connections_in.empty() }),
                  std::make_shared<LogicNodeInput>(LogicNodeInput { .index = 1, .enabled = type->connections_in.size() > 1 })
          };
        }

        const LogicNodeType* get_type() {
          return this->logic_node_type;
        }

        godot::Node2D *get_graphical_node() const {
          return graphical_node;
        }

        void set_graphical_node(godot::Node2D *graphical_node) {
          LogicNode::graphical_node = graphical_node;
        }

        std::shared_ptr<LogicNodeOutput> get_output() const {
          return output;
        }

        std::vector<std::shared_ptr<LogicNodeInput>> get_inputs() {
          return inputs;
        }

    private:
        const LogicNodeType *logic_node_type;
        godot::Node2D* graphical_node = nullptr;

        std::shared_ptr<LogicNodeOutput> output;
        std::vector<std::shared_ptr<LogicNodeInput>> inputs;
    };

}

#endif //REFACTOR_NATIVE_LOGIC_NODE_H
