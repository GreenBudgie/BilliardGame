shader_type canvas_item;

#include "res://resources/shaders/smooth_pixel_inc.gdshaderinc"
#include "res://resources/shaders/sphere.gdshaderinc"

void fragment() {
	// Discarding pixels outside the circle radius
	vec2 texture_size = get_texture_size(TEXTURE_PIXEL_SIZE);
	vec2 pixelated_uv = get_pixelated_uv(UV, texture_size);
	if (should_discard(pixelated_uv, texture_size)) {
		discard;
	}
	
	// Mapping to spherical coordinates
	vec2 uv = UV * 2.0 - 1.0;
	vec2 spherical_uv = get_spherical_uv(uv);
	COLOR = texture_point_smooth(TEXTURE, spherical_uv, TEXTURE_PIXEL_SIZE);
	//COLOR = texture(TEXTURE, spherical_uv);
	
	COLOR.a = smooth_edges(uv, COLOR.a);
}