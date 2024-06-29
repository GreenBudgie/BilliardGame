uniform vec4 rotation = vec4(0.0, 0.0, 0.0, 1.0);

#include "res://resources/shaders/quaternions.gdshaderinc"

vec2 get_texture_size(vec2 pixel_size) {
	return 1.0 / pixel_size;
}

vec2 get_pixel_center(vec2 uv, vec2 texture_size) {
	return floor(uv * texture_size) + 0.5;
}

vec2 get_pixelated_uv(vec2 uv, vec2 texture_size) {
	return get_pixel_center(uv, texture_size) / texture_size;
}

bool should_discard(vec2 pixelated_uv, vec2 texture_size) {
	vec2 center = vec2(texture_size.x * 0.5, texture_size.y * 0.5);
	vec2 center_vector = center - get_pixel_center(pixelated_uv, texture_size);
	return length(center_vector) > texture_size.y * 0.5;
}

// uv should be converted to 
vec2 get_spherical_uv(vec2 uv) {
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
	return vec2(new_u, new_v);
}

float smooth_edges(vec2 uv, float alpha) {
	float distance_from_center = 2.0 * uv.x * uv.x + uv.y * uv.y;
	float distance_from_edge = 1.0 - distance_from_center;
	float threshold_width = 0.1;
	return clamp((distance_from_edge / threshold_width), 0, alpha);
}