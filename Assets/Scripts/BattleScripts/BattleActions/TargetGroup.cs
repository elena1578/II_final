using UnityEngine;


// https://learn.microsoft.com/en-us/dotnet/fundamentals/runtime-libraries/system-flagsattribute
// allows for multiple values to be combined using bitwise operations (e.g., Self | Allies) to 
// rep actions that can target multiple groups
[System.Flags]
public enum TargetGroup
{
    None  = 0,
    Self  = 1 << 0,
    Allies = 1 << 1,
    Enemies = 1 << 2
}