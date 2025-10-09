// fragment shader (fs.glsl)
#version 330

in vec2 fragTexCoord;
in vec3 fragNormal;
in vec4 fragColor;
out vec4 finalColor;
uniform sampler2D texture0;
uniform vec4 colDiffuse;


uniform vec3 sunDirection;

void main()
{
    vec4 texelColor = texture(texture0, fragTexCoord);

    if (texelColor.a < 0.1)
        discard;

    float lightBrightness = 0;
    if (sunDirection != vec3(0))
    {
        lightBrightness = clamp(dot(fragNormal, sunDirection), 0, 1);
    }

    float ambientLighting = 0.65;
    lightBrightness = ambientLighting + lightBrightness * ambientLighting;

    //finalColor = vec4((fragNormal.xyz + 1) / 2, 1);
    finalColor = texelColor*colDiffuse*fragColor;
    finalColor = vec4(finalColor.rgb * lightBrightness, finalColor.a);
}