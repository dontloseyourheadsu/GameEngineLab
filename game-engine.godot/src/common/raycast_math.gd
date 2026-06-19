class_name RaycastMath
extends RefCounted

## RaycastMath
## Ported from GameEngineLab.Core.Features.Physics.Raycast
## Contains mathematical raycast operations independent of Godot's physics engine.

## Intersects a ray (origin, direction) with a line segment (p1, p2).
## Returns a Dictionary with:
##   - "hit": bool
##   - "distance": float
##   - "normal": Vector2
static func intersect_ray_line(origin: Vector2, direction: Vector2, p1: Vector2, p2: Vector2) -> Dictionary:
	var result := {
		"hit": false,
		"distance": 0.0,
		"normal": Vector2.ZERO
	}

	var v1 := origin - p1
	var v2 := p2 - p1
	var v3 := Vector2(-direction.y, direction.x)

	var dot_val := v2.dot(v3)
	if absf(dot_val) < 0.000001:
		return result

	var t1 := (v2.x * v1.y - v2.y * v1.x) / dot_val
	var t2 := v1.dot(v3) / dot_val

	if t1 >= 0.0 and t2 >= 0.0 and t2 <= 1.0:
		result.hit = true
		result.distance = t1
		var edge := p2 - p1
		var normal := Vector2(-edge.y, edge.x).normalized()
		# Ensure normal points away from ray
		if normal.dot(direction) > 0.0:
			normal = -normal
		result.normal = normal

	return result


## Intersects a ray (origin, direction) with a polygon defined by its vertices (Array of Vector2).
## Returns a Dictionary with:
##   - "hit": bool
##   - "distance": float
##   - "normal": Vector2
static func intersect_ray_polygon(origin: Vector2, direction: Vector2, vertices: Array[Vector2]) -> Dictionary:
	var result := {
		"hit": false,
		"distance": INF,
		"normal": Vector2.ZERO
	}

	var size := vertices.size()
	if size < 3:
		return result

	for i in range(size):
		var p1 := vertices[i]
		var p2 := vertices[(i + 1) % size]

		var hit_res := intersect_ray_line(origin, direction, p1, p2)
		if hit_res.hit:
			if hit_res.distance < result.distance:
				result.hit = true
				result.distance = hit_res.distance
				result.normal = hit_res.normal

	if result.hit:
		return result
	
	result.distance = 0.0
	return result


## Intersects a ray (origin, direction) with a circle (center, radius).
## Returns a Dictionary with:
##   - "hit": bool
##   - "distance": float
##   - "normal": Vector2
static func intersect_ray_circle(origin: Vector2, direction: Vector2, center: Vector2, radius: float) -> Dictionary:
	var result := {
		"hit": false,
		"distance": 0.0,
		"normal": Vector2.ZERO
	}

	var L := center - origin
	var tca := L.dot(direction)
	if tca < 0.0:
		return result

	var d2 := L.dot(L) - tca * tca
	if d2 > radius * radius:
		return result

	var thc := sqrt(radius * radius - d2)
	var dist := tca - thc

	var hit_point := origin + direction * dist
	var normal := (hit_point - center).normalized()

	result.hit = true
	result.distance = dist
	result.normal = normal

	return result
