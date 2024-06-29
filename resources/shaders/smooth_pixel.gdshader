shader_type canvas_item;
render_mode unshaded;

#include "res://resources/shaders/smooth_pixel_inc.gdshaderinc"

void fragment()
{
	COLOR = texture_point_smooth(TEXTURE, UV, TEXTURE_PIXEL_SIZE);
}