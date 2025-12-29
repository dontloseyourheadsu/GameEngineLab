pub fn map_txt_to_json_string_serializer(input_path: &str, output_path: &str) {
    use std::fs;

    let content = fs::read_to_string(input_path).expect("Failed to read map file");
    let lines: Vec<String> = content
        .lines()
        .map(|line| line.to_string())
        .collect();

    let width = lines.first().map_or(0, |line| line.len());
    let height = lines.len();

    let map_json = serde_json::json!({
        "width": width,
        "height": height,
        "data": lines,
    });

    fs::write(output_path, map_json.to_string()).expect("Failed to write JSON map file");
}