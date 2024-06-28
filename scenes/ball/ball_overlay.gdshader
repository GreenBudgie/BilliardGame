shader_type canvas_item;

uniform vec4 rotation = vec4(0.0, 0.0, 0.0, 1.0);

uniform vec2 texture_size = vec2(31.0, 15.0);

#include "res://resources/shaders/quaternions.gdshaderinc"

vec2 get_pixel_center_uv(vec2 uv) {
	float pixel_x = floor(uv.x * texture_size.x) + 0.5;
	float pixel_y = floor(uv.y * texture_size.y) + 0.5;
	return vec2(pixel_x, pixel_y);
}

vec2 get_pixel_center_uv_real(vec2 uv) {
	float pixel_x = floor(uv.x * texture_size.x) + 0.5;
	float pixel_y = floor(uv.y * texture_size.y) + 0.5;
	return vec2(pixel_x / texture_size.x, pixel_y / texture_size.y);
}

bool should_discard(vec2 uv) {
	vec2 center = vec2(texture_size.x * 0.5, texture_size.y * 0.5);
	vec2 center_vector = center - get_pixel_center_uv(uv);
	return length(center_vector) > texture_size.y * 0.5;
}

void fragment() {
	vec2 pixelated_uv = get_pixel_center_uv_real(UV);
	vec2 uv = pixelated_uv * 2.0 - 1.0;
	
	float distance_from_center = 4.0 * uv.x * uv.x + uv.y * uv.y;
	if (should_discard(pixelated_uv)) {
		discard;
	}
	float x = sqrt(1.0 - (4.0 * uv.x * uv.x) - (uv.y * uv.y));
	float y = 2.0 * uv.x;
	float z = uv.y;
	vec3 vector = vec3(x, y, z);
	
	vec3 r_vector = rotate(vector, rotation);
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
}