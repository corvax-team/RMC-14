// fpv_drone_screen.fsh

uniform sampler2D SCREEN_TEXTURE;
uniform float time;

in vec2 uv;
out vec4 fragColor;

void main()
{
    vec4 color = texture(SCREEN_TEXTURE, uv);

    // Зелёный оттенок
    float green = (color.r + color.g + color.b) / 3.0;
    color = vec4(0.2 * green, 1.0 * green, 0.2 * green, 1.0);

    // Наложение простого шума
    float noise = fract(sin(dot(uv * time, vec2(12.9898, 78.233))) * 43758.5453);
    color.rgb += (noise - 0.5) * 0.04;

    fragColor = color;
}