# ArrayReinterpreter
Fast converter of array of structures to array of bytes (and vice versa) without copying memory.
This is useful for fast serializing/deserializing arrays of structures.

Supports C# 3.0, .NET Framework 2.0 and above. Works for 32-bits and 64-bits.

- **ArrayReinterpreter.cs** - Generic class to reinterpret arrays
- **FastStructStream.cs** - Serializer/deserializer

## Examples

### Float array to byte array

```c#
float[] floats = new float[] { 0, 1.5f, 3, -1 };
ArrayReinterpreter<float> reinterpreter = new ArrayReinterpreter<float>();

reinterpreter.AsByteArray(floats, bytes => 
{
    foreach (byte item in bytes)
    {
        Console.Write(item + " ");
    }
    Console.WriteLine("({0} items)", bytes.Length);
});
```
For more examples, see *Program.cs*

### Reading and writing to a stream

Writing:
```c#
const int count = 5;
ushort[] shorts = new ushort[] { 0x0011, 0x2233, 0x4455 };
double[] doubles = new double[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
{
    FastStructStream fastStream = new FastStructStream(fs);

    fastStream.Write(shorts.Length);
    fastStream.Write(shorts);

    fastStream.Write(count);
    fastStream.Write(doubles, 3, count);
}
```
Reading:
```c#
ushort[] shorts;
double[] doubles;

using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
{
    FastStructStream fastStream = new FastStructStream(fs);

    int shortsLength = fastStream.Read<int>();
    shorts = new ushort[shortsLength];
    fastStream.Read(shorts);

    int doublesLength = fastStream.Read<int>();
    doubles = new double[doublesLength];
    fastStream.Read(doubles);
}
```

## Original idea
The original idea belongs to Omer Mor (https://gist.github.com/OmerMor/1050703)
