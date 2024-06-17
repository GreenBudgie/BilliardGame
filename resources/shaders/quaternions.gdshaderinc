// Provides basic functions for quaternions 

// Computes the conjugate of a quaternion
vec4 quat_conj(vec4 q)
{ 
	return vec4(-q.x, -q.y, -q.z, q.w); 
}

// Multiplies two quaternions q1 and q2
// The result is a quaternion representing the combined rotation of q1 and q2
// Quaternion multiplication is not commutative: quat_mult(q1, q2) != quat_mult(q2, q1)
vec4 quat_mult(vec4 q1, vec4 q2)
{ 
	vec4 qr;
	qr.x = (q1.w * q2.x) + (q1.x * q2.w) + (q1.y * q2.z) - (q1.z * q2.y);
	qr.y = (q1.w * q2.y) - (q1.x * q2.z) + (q1.y * q2.w) + (q1.z * q2.x);
	qr.z = (q1.w * q2.z) + (q1.x * q2.y) - (q1.y * q2.x) + (q1.z * q2.w);
	qr.w = (q1.w * q2.w) - (q1.x * q2.x) - (q1.y * q2.y) - (q1.z * q2.z);
	return qr;
}

// Creates a quaternion from an axis and an angle
// The axis must be a normalized vector, and the angle is in radians
// This quaternion represents a rotation around the given axis by the given angle
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

// Rotates a 3D vertex position around a given axis by a specified angle
// Uses quaternion rotation to perform the rotation
// The axis must be a normalized vector, and the angle is in radians
vec3 rotate_vertex_position(vec3 position, vec3 axis, float angle)
{ 
	vec4 qr = quat_from_axis_angle(axis, angle);
	vec4 qr_conj = quat_conj(qr);
	vec4 q_pos = vec4(position.x, position.y, position.z, 0);

	vec4 q_tmp = quat_mult(qr, q_pos);
	qr = quat_mult(q_tmp, qr_conj);

	return vec3(qr.x, qr.y, qr.z);
}