#version 330
in vec3 vertexPosition;
in vec2 vertexTexCoord;
in vec4 vertexColor;
in vec3 vertexNormal;
in vec4 vertexTangent;

out vec2 fragTexCoord;
out vec4 fragColor;
out vec3 fragNormal;
out vec3 foliageColor;

uniform float time;
uniform mat4 mvp;

float mapTo4PI(float x) {
    return x * (3.14159265358979323846 / 4.0);
}

void main()
{
    vec3 position = vertexPosition;

    fragTexCoord = vertexTexCoord;
    fragColor = vertexColor;
    fragNormal = vertexNormal;

    if(vertexTangent.x != 0)
    {
        float negativeOneToOne = (sin(time + mapTo4PI(position.x) + mapTo4PI(position.z)) - 1) * 0.125f;

        position.y += negativeOneToOne;
    }

    gl_Position = mvp*vec4(position, 1.0);
}


