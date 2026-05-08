using NGeoHash;

// Test basic encode
var basic = GeoHash.Encode(32.768799, -97.309341);
Console.WriteLine($"Basic (9 chars): {basic}");

// Test with precision
try {
    var withPrecision = GeoHash.Encode(32.768799, -97.309341, 5);
    Console.WriteLine($"With precision 5: {withPrecision}");
} catch (Exception ex) {
    Console.WriteLine($"Precision overload error: {ex.Message}");
}

// Check method signatures via reflection
var methods = typeof(GeoHash).GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
foreach (var m in methods.Where(m => m.Name == "Encode")) {
    var parms = string.Join(", ", m.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
    Console.WriteLine($"Encode({parms})");
}
