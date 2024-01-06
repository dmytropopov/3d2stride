# 3d2stride
CLI tool for converting 3D files in different formats to raw OpenGL strides/indices

# Features
- Read single/multiple input files
- Write single output file (combine input files, e.g. combine UV coords from two inputs)
- Indices optimization (duplicate strides removal)
- Stride size alignment
- Small processing of input values (normalize, negate, one minus)

## Input
Currently only OBJ input format supported.

## Output
Writes two output files `*-indices.bin` and `*-strides.bin` which are meant to be loaded into video memory without conversion.