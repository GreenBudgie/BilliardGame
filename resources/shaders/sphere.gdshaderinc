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

vec4 inverse_rotation() {
	return vec4(-rotation.x, -rotation.y, -rotation.z, rotation.w);
}

// Get a point on the surface of a sphere with its center at (0, 0, 0) and a radius of 1 after applying an inverse rotation
// XYZ axis looks like: 
/* 
	 ^ Z  
	 |  
	 | 
	 |
	 +-----------> X
	/
   /
  /	
 V Y
*/
vec3 get_spherical_coords_after_rotation(vec2 uv, vec2 texture_size) {
	float sphere_radius = 1.0;
	// map v from [0; 1] to [-1; 1]
	float z_cylinder = (uv.y * 2.0 - 1.0);
	float ball_radius_in_pixels = texture_size.y * 0.5;
	// ball radius in u coords (proportion)
	float ball_radius_u =  ball_radius_in_pixels / texture_size.x;

	// Map current u to interval [-1; 1]
	float x_cylinder = (uv.x - 0.5)/(ball_radius_u);
	// Find y on cylinder with radius = sphere_radius
	float y_cylinder = sqrt(sphere_radius*sphere_radius - x_cylinder*x_cylinder);

	float y_sphere = sqrt(sphere_radius*sphere_radius - z_cylinder*z_cylinder - x_cylinder*x_cylinder);

	vec3 vector = vec3(x_cylinder, y_sphere, z_cylinder);

	// Find inversed rotation to get coordinate which will be in current place after rotation
	vec4 inversed_rotation = inverse_rotation();
	
	return rotate(vector, inversed_rotation);
}


vec2 get_spherical_uv(vec2 uv, vec2 texture_size) {
	vec3 r_vector = get_spherical_coords_after_rotation(uv, texture_size);
	float x = r_vector.x;
	float y = r_vector.y;
	float z = r_vector.z;
	
	// Get u from 0.25 to 0.75 depending on x ([-1; +1])
	float new_u = (x + 1.0)/4.0 + 0.25;
	
	// If we can't see current point
	if (y < 0.0) {
		float distance_to_center = abs(new_u - 0.5);
		// map new_u from [0.5, 0.75] to [1, 0.75] (right end of texture)
		if (new_u > 0.5)
			new_u = 1.0 - distance_to_center;
		else // map new_u from [0.25, 0.5] to [0.25, 0] (left end of texture)
			new_u = distance_to_center;
	}

	// Map v from [-1; 1] to [0, 1]
	float new_v = (z + 1.0)/2.0;
	return vec2(new_u, new_v);
}

float smooth_edges(vec2 uv, float alpha) {
	float distance_from_center = 2.0 * uv.x * uv.x + uv.y * uv.y;
	float distance_from_edge = 1.0 - distance_from_center;
	float threshold_width = 0.1;
	return clamp((distance_from_edge / threshold_width), 0, alpha);
}