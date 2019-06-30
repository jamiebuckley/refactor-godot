//
// Created by jamie on 30/06/19.
//

#ifndef REFACTOR_NATIVE_GODOT_INTERFACE_H
#define REFACTOR_NATIVE_GODOT_INTERFACE_H

namespace Refactor {
    class GodotInterface {
    public:
      virtual void print(const char* message) {
        godot::Godot::print(message);
      }

      virtual void print_error(const char* message, const char* function, const char* file, int line) {
        godot::Godot::print_error(message, function, file, line);
      }

      virtual void create_worker(int grid_x, int grid_z, godot::Vector3) {

      };

      template <class... Args> void call(godot::Spatial * spatial, const char* function_name, Args... args) {
        if(call_godot()) {
          spatial->call(function_name, godot::Array::make(args...));
        }
      }

      virtual bool call_godot() {
        return true;
      }
    };
}

#endif //REFACTOR_NATIVE_GODOT_INTERFACE_H
