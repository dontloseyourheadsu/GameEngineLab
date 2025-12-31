use std::collections::HashMap;

#[derive(Clone)]
pub struct Sprite2D {
    pub animation_state: String,
    pub animation_frame: usize,
    pub animation_mapper: HashMap<String, Vec<usize>>,
}
