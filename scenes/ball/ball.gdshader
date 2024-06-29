shader_type canvas_item;

uniform vec3 base_color : source_color;
uniform vec3 paint_color : source_color;

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
	vec2 uv = pixelated_uv * 2.0 - 1.0;
	vec2 spherical_uv = get_spherical_uv(uv);
	COLOR = texture(TEXTURE, spherical_uv);

	COLOR.rgb = mix(paint_color, base_color, COLOR.r);
}