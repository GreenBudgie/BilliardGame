shader_type canvas_item;

uniform vec4 rotation = vec4(0.0, 0.0, 1.0, 0.0);
uniform vec3 base_color : source_color;
uniform vec3 paint_color : source_color;

#include "res://resources/shaders/quaternions.gdshaderinc"

void fragment() {
	vec2 uv = UV * 2.0 - 1.0;
	
	float distance_from_center = 4.0 * uv.x * uv.x + uv.y * uv.y;
	if (distance_from_center >= 1.0) {
		discard;
	}
	float x = sqrt(1.0 - (4.0 * uv.x * uv.x) - (uv.y * uv.y));
	float y = 2.0 * uv.x;
	float z = uv.y;
	vec3 vector = vec3(x, y, z);
	
	vec3 r_vector = rotate_vertex_position(
		vector, 
		normalize(rotation.xyz),
		rotation.w
	);
	x = r_vector.x;
	y = r_vector.y;
	z = r_vector.z;
	
	float new_u = y / 4.0 + 0.5;
	if (x < 0.0 && z < 0.0) {
		new_u = -y / 4.0;
	} else if (x < 0.0 && z >= 0.0) {
		new_u = 1.0 - y / 4.0;
	}
	float new_v = 0.5 * z + 0.5;
	
	COLOR = texture(TEXTURE, vec2(new_u, new_v));
	COLOR.rgb = mix(paint_color, base_color, COLOR.r);
	
	float distance_from_edge = 1.0 - distance_from_center;
	float threshold_width = 0.1;
	COLOR.a = clamp((distance_from_edge / threshold_width), 0, COLOR.a);
}