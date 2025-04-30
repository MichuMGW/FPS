shader_type spatial;

uniform float reveal_amount : hint_range(0.0, 1.0);

void fragment() {
	if (UV.y > reveal_amount) {
		discard;
	}
}

//void light() {
//	// Called for every pixel for every light affecting the material.
//	// Uncomment to replace the default light processing function with this one.
//}
