#shader vertex
#version 330 core

#include "UBOs/Camera.ubo"

layout(location = 0) in vec4 position;
layout(location = 1) in vec2 texCoord;
layout(location = 2) in vec4 normal;
layout(location = 3) in vec2 texCoord2;
layout(location = 4) in vec4 vertcolor;
layout(location = 5) in uint vertLight;
float lightMul = 0.0625f;

uniform mat4 u_World =
mat4(
	1, 0, 0, 0,
	0, 1, 0, 0,
	0, 0, 1, 0,
	0, 0, 0, 1
);

out vec2 v_TexCoord2;
out vec2 v_TexCoord;
out vec4 v_Normal;
out vec4 v_Color;
out float v_SunLight;
out vec3 v_BlockLight;

void main()
{
	v_Normal = normalize(normal * u_World);

	v_Color = vertcolor;

	v_TexCoord = texCoord;
	v_TexCoord2 = texCoord2;

	v_SunLight = float((vertLight >> 12) & 0xFu) * lightMul;
	float r = float((vertLight >> 8) & 0xFu) * lightMul;
	float g = float((vertLight >> 4) & 0xFu) * lightMul;
	float b = float(vertLight & 0xFu) * lightMul;
	v_BlockLight = vec3(r, g, b);

	mat4 wvp = Camera.ProjectionMat * Camera.ViewMat * u_World;
	gl_Position = wvp * position;
}

//////////////////////////////////////////////////////////

#shader fragment
#version 330 core

#queue opaque

#include "Includes/Voxel.glsl"
#include "UBOs/Lighting.ubo"

layout(location = 0) out vec4 color;

uniform sampler2D u_ColorMap;

in vec4 v_Color;
in vec2 v_TexCoord2;
in vec2 v_TexCoord;
in vec4 v_Normal;
in float v_SunLight;
in vec3 v_BlockLight;

void main()
{
	vec4 texCol = texture(u_ColorMap, v_TexCoord);

	vec3 worldNormal = normalize(v_Normal.rgb);

	vec4 sunColor = Lighting.SunStrength * Lighting.SunColor;


	vec4 pxLight = (sunColor * (pow(v_SunLight + 0.1, 2))) + vec4(v_BlockLight, 1);
	if (v_TexCoord2 != vec2(-1, -1))
	{
		vec4 mask = texture(u_ColorMap, v_TexCoord2);
		if (mask.a != 0)
		{
			texCol = mask * v_Color;
		}
	}
	if (texCol.a < 0.1)
		discard;

	color = (texCol * pxLight);
}