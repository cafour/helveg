#version 300 es

precision mediump float;

// START OF NOISE

//
// Description : Array and textureless GLSL 2D simplex noise function.
//      Author : Ian McEwan, Ashima Arts.
//  Maintainer : stegu
//     Lastmod : 20110822 (ijm)
//     License : Copyright (C) 2011 Ashima Arts. All rights reserved.
//               Distributed under the MIT License. See LICENSE file.
//               https://github.com/ashima/webgl-noise
//               https://github.com/stegu/webgl-noise
// 

vec3 mod289(vec3 x) {
  return x - floor(x * (1.0 / 289.0)) * 289.0;
}

vec2 mod289(vec2 x) {
  return x - floor(x * (1.0 / 289.0)) * 289.0;
}

vec3 permute(vec3 x) {
  return mod289(((x*34.0)+10.0)*x);
}

float snoise(vec2 v)
  {
  const vec4 C = vec4(0.211324865405187,  // (3.0-sqrt(3.0))/6.0
                      0.366025403784439,  // 0.5*(sqrt(3.0)-1.0)
                     -0.577350269189626,  // -1.0 + 2.0 * C.x
                      0.024390243902439); // 1.0 / 41.0
// First corner
  vec2 i  = floor(v + dot(v, C.yy) );
  vec2 x0 = v -   i + dot(i, C.xx);

// Other corners
  vec2 i1;
  //i1.x = step( x0.y, x0.x ); // x0.x > x0.y ? 1.0 : 0.0
  //i1.y = 1.0 - i1.x;
  i1 = (x0.x > x0.y) ? vec2(1.0, 0.0) : vec2(0.0, 1.0);
  // x0 = x0 - 0.0 + 0.0 * C.xx ;
  // x1 = x0 - i1 + 1.0 * C.xx ;
  // x2 = x0 - 1.0 + 2.0 * C.xx ;
  vec4 x12 = x0.xyxy + C.xxzz;
  x12.xy -= i1;

// Permutations
  i = mod289(i); // Avoid truncation effects in permutation
  vec3 p = permute( permute( i.y + vec3(0.0, i1.y, 1.0 ))
		+ i.x + vec3(0.0, i1.x, 1.0 ));

  vec3 m = max(0.5 - vec3(dot(x0,x0), dot(x12.xy,x12.xy), dot(x12.zw,x12.zw)), 0.0);
  m = m*m ;
  m = m*m ;

// Gradients: 41 points uniformly over a line, mapped onto a diamond.
// The ring size 17*17 = 289 is close to a multiple of 41 (41*7 = 287)

  vec3 x = 2.0 * fract(p * C.www) - 1.0;
  vec3 h = abs(x) - 0.5;
  vec3 ox = floor(x + 0.5);
  vec3 a0 = x - ox;

// Normalise gradients implicitly by scaling m
// Approximation of: m *= inversesqrt( a0*a0 + h*h );
  m *= 1.79284291400159 - 0.85373472095314 * ( a0*a0 + h*h );

// Compute final noise value at P
  vec3 g;
  g.x  = a0.x  * x0.x  + h.x  * x0.y;
  g.yz = a0.yz * x12.xz + h.yz * x12.yw;
  return 130.0 * dot(m, g);
}

// END OF NOISE

in float v_coreRadius;
in float v_sauceRadius;
in float v_crustRadius;
in float v_totalSize;
flat in int v_index;

uniform float u_sizeRatio;
uniform float u_crustWidth;
uniform float u_sauceWidth;

out vec4 f_color;

// const vec3 DOUGH_COLOR = vec3(200.0, 151.0, 108.0) / 255.0; // #c8976c
// const vec3 DOUGH_COLOR = vec3(242.0, 179.0, 96.0) / 255.0; // #F2B360
const vec3 DOUGH_COLOR = vec3(213.0, 169.0, 123.0) / 255.0; // #d5a97b

void main(void) { 
    vec2 relative = gl_PointCoord - vec2(0.5, 0.5);
    vec2 absolute = relative * v_totalSize;
    float dist = length(absolute);

    float noise = snoise(relative * 10.0 + float(v_index));
    float outerRaggedness = noise * u_crustWidth * 0.05;

    if (dist > v_sauceRadius && dist < v_crustRadius + outerRaggedness) {
        float crust = (snoise(relative * 50.0 + float(v_index)) + 1.0) * 0.5;
        if (crust < 0.5) {
            f_color.rgb = DOUGH_COLOR * 0.9;
        } else if (crust > 0.9) {
            f_color.rgb = DOUGH_COLOR * 1.1;
        } else {
            f_color.rgb = DOUGH_COLOR;
        }

        f_color.a = 1.0;
    } else {
        discard;
    }
}
