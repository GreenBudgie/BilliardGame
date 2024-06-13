shader_type canvas_item;

uniform vec4 rotation = vec4(0.0, 0.0, 1.0, 0.0);

vec4 quat_conj(vec4 q)
{ 
  return vec4(-q.x, -q.y, -q.z, q.w); 
}
  
vec4 quat_mult(vec4 q1, vec4 q2)
{ 
  vec4 qr;
  qr.x = (q1.w * q2.x) + (q1.x * q2.w) + (q1.y * q2.z) - (q1.z * q2.y);
  qr.y = (q1.w * q2.y) - (q1.x * q2.z) + (q1.y * q2.w) + (q1.z * q2.x);
  qr.z = (q1.w * q2.z) + (q1.x * q2.y) - (q1.y * q2.x) + (q1.z * q2.w);
  qr.w = (q1.w * q2.w) - (q1.x * q2.x) - (q1.y * q2.y) - (q1.z * q2.z);
  return qr;
}

vec4 quat_from_axis_angle(vec3 axis, float angle)
{ 
  vec4 qr;
  float half_angle = angle * 0.5;
  qr.x = axis.x * sin(half_angle);
  qr.y = axis.y * sin(half_angle);
  qr.z = axis.z * sin(half_angle);
  qr.w = cos(half_angle);
  return qr;
}

vec3 rotate_vertex_position(vec3 position, vec3 axis, float angle)
{ 
  vec4 qr = quat_from_axis_angle(axis, angle);
  vec4 qr_conj = quat_conj(qr);
  vec4 q_pos = vec4(position.x, position.y, position.z, 0);
  
  vec4 q_tmp = quat_mult(qr, q_pos);
  qr = quat_mult(q_tmp, qr_conj);
  
  return vec3(qr.x, qr.y, qr.z);
}

void fragment() {
	vec2 uv = UV * 2.0 - 1.0;
	
	// For now, uv.x = [-2;2], uv.y = [-1;1]
	
	if (4.0 * uv.x * uv.x + uv.y * uv.y >= 1.0) {
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
	}
	if (x < 0.0 && z >= 0.0) {
		new_u = 1.0 - y / 4.0;
	}
	float new_v = 0.5 * z + 0.5;
	
	COLOR = texture(TEXTURE, vec2(new_u, new_v));
}