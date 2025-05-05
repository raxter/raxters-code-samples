

void random_offset_xz_to_xz_float(float3 world_pos, out float3 offset)
{
    // Generate a random value based on the world position
    float2 random_value = frac(sin(dot(world_pos.xz,float2(35.61278, 125.6898)))*258282.1234462);

    offset.y = 0;
    // Scale the random value to a range of -1 to 1
    offset.xz = (random_value - 0.5) * 2;
}

void random_offset_xz_to_2d_float(float3 world_pos, out float2 offset)
{
    // Generate a random value based on the world position
    float2 random_value = frac(sin(dot(world_pos.xz,float2(35.61278, 125.6898)))*258282.1234462);

    // Scale the random value to a range of -1 to 1
    offset = (random_value - 0.5) * 2;

}


void random_offset_2d_to_2d_float(float2 world_pos, out float2 offset)
{
    // Generate a random value based on the world position
    float2 random_value = frac(sin(dot(world_pos,float2(35.61278, 125.6898)))*258282.1234462);

    // Scale the random value to a range of -1 to 1
    offset = (random_value - 0.5) * 2;
}

