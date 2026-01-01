mod serializers;

use crate::serializers::map_txt_to_json_string_serializer::map_txt_to_json_string_serializer;

fn main() {
    map_txt_to_json_string_serializer("data/map.txt", "data/map.json");
}
