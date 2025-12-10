shader_type spatial;

uniform sampler2D source_texture_mask : source_color;
uniform sampler2D source_texture_black : source_color;
uniform sampler2D source_texture_red : source_color;
uniform sampler2D source_texture_green : source_color;
uniform sampler2D source_texture_green_mixer : source_color;
uniform sampler2D source_texture_blue : source_color;

uniform float uv_size : hint_range(0.01, 100.0, 0.01) = 1.0;
uniform float blend_factor = 0.5;

void fragment() {

vec2 UV_Scaled = UV * uv_size;

// texture_rgbmask UV is not scaled.
vec3 texture_rgbmask= texture(source_texture_mask, UV).rgb;
vec3 texture_black 	= texture(source_texture_black, UV_Scaled).rgb;
vec3 texture_red 	= texture(source_texture_red, UV_Scaled*0.1).rgb;
vec3 texture_green 	= texture(source_texture_green, UV_Scaled).rgb;
vec3 texture_green_mixer 	= texture(source_texture_green_mixer, UV_Scaled*0.05).rgb;
vec3 texture_blue 	= texture(source_texture_blue, UV_Scaled).rgb;

float summed_texture_channels = (
	texture_rgbmask.r +
	texture_rgbmask.g +
	texture_rgbmask.b);

vec3 mixed_terrain = clamp(
		(	texture_rgbmask.r * texture_red +
			texture_rgbmask.g * mix(texture_green,texture_green_mixer,blend_factor) +
			texture_rgbmask.b * texture_blue) /
			summed_texture_channels,
			vec3(0.0), vec3(1.0) // Clamp min, max values.
			);

ALBEDO = mix(mixed_terrain,mix(texture_black,texture_green_mixer,blend_factor),1.0 - summed_texture_channels);

} // Fragment end
//void light() {
	// Called for every pixel for every light affecting the material.
	// Uncomment to replace the default light processing function with this one.
//}
